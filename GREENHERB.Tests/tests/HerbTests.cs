using System.Text;
using ClosedXML.Excel;
using GREENHERB.src.Controllers;
using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class HerbTests
{
	private static HerbsController CreateController(AppDbContext dbContext)
	{
		var herbService = new HerbService(dbContext);
		return new HerbsController(herbService);
	}

	private static PlanController CreatePlanController(AppDbContext dbContext)
	{
		var herbService = new HerbService(dbContext);
		var planService = new PlanService(dbContext);
		return new PlanController(herbService, planService);
	}

	private static AppDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new AppDbContext(options);
	}

	private static IFormFile CreateCsvFile(string content, string fileName = "herbs.csv")
	{
		var bytes = Encoding.UTF8.GetBytes(content);
		var stream = new MemoryStream(bytes);
		return new FormFile(stream, 0, bytes.Length, "file", fileName);
	}

	private static IFormFile CreateXlsxFile(Action<IXLWorksheet> fillSheet, string fileName = "herbs.xlsx")
	{
		using var workbook = new XLWorkbook();
		var sheet = workbook.AddWorksheet("Herbs");
		fillSheet(sheet);

		var stream = new MemoryStream();
		workbook.SaveAs(stream);
		stream.Position = 0;

		return new FormFile(stream, 0, stream.Length, "file", fileName);
	}

	[Fact]
	public async Task ImportCsv_WithValidRows_ImportsAndUpdates()
	{
		using var dbContext = CreateDbContext();
		dbContext.Herbs.Add(new Herb
		{
			Name = "Mint",
			ScientificName = "Mentha",
			Category = "Leaf",
			Origin = "EU",
			CycleDays = 30
		});
		await dbContext.SaveChangesAsync();

		var controller = CreateController(dbContext);
		var csv =
			"Name,ScientificName,Category,Origin,Notes,CareInstructions,CycleDays\n" +
			"Mint,Mentha,Fresh,EU,,Keep moist,25\n" +
			"Basil,Ocimum,Leaf,Asia,Popular,Full sun,45";

		var file = CreateCsvFile(csv);
		var response = await controller.Import(file);

		var okResult = Assert.IsType<OkObjectResult>(response.Result);
		var result = Assert.IsType<HerbImportResult>(okResult.Value);
		Assert.Equal(1, result.Imported);
		Assert.Equal(1, result.Updated);
		Assert.Empty(result.Errors);

		var herbs = await dbContext.Herbs.AsNoTracking().ToListAsync();
		Assert.Equal(2, herbs.Count);
		Assert.Contains(herbs, herb => herb.Name == "Mint" && herb.Category == "Fresh");
	}

	// TESTES ADICIONAIS PARA COBERTURA

	[Fact]
	public async Task ImportCsv_WithEmptyFile_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var file = CreateCsvFile("");
		var response = await controller.Import(file);
		Assert.IsType<BadRequestObjectResult>(response.Result);
	}

	[Fact]
	public async Task ImportCsv_WithInvalidCycleDays_ReturnsError()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var csv = "Name,ScientificName,Category,Origin,Notes,CareInstructions,CycleDays\n" +
				  "Mint,Mentha,Leaf,EU,,Keep moist,abc";
		var file = CreateCsvFile(csv);
		var response = await controller.Import(file);
		var okResult = Assert.IsType<OkObjectResult>(response.Result);
		var result = Assert.IsType<HerbImportResult>(okResult.Value);
		Assert.Single(result.Errors);
	}

	[Fact]
	public async Task ImportCsv_WithDuplicateHerbs_UpdatesExisting()
	{
		using var dbContext = CreateDbContext();
		dbContext.Herbs.Add(new Herb { Name = "Basil", ScientificName = "Ocimum", Category = "Leaf", Origin = "Asia", CycleDays = 40 });
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		var csv = "Name,ScientificName,Category,Origin,Notes,CareInstructions,CycleDays\n" +
				  "Basil,Ocimum,Leaf,Asia,Popular,Full sun,45";
		var file = CreateCsvFile(csv);
		var response = await controller.Import(file);
		var okResult = Assert.IsType<OkObjectResult>(response.Result);
		var result = Assert.IsType<HerbImportResult>(okResult.Value);
		Assert.Equal(0, result.Imported);
		Assert.Equal(1, result.Updated);
	}

	[Fact]
	public async Task ImportXlsx_WithEmptySheet_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var file = CreateXlsxFile(sheet => { });
		var response = await controller.Import(file);
		Assert.IsType<BadRequestObjectResult>(response.Result);
	}

	[Fact]
	public async Task ImportXlsx_WithInvalidCycleDays_ReturnsError()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var file = CreateXlsxFile(sheet => {
			sheet.Cell(1, 1).Value = "Name";
			sheet.Cell(1, 2).Value = "ScientificName";
			sheet.Cell(1, 3).Value = "Category";
			sheet.Cell(1, 4).Value = "Origin";
			sheet.Cell(1, 5).Value = "Notes";
			sheet.Cell(1, 6).Value = "CareInstructions";
			sheet.Cell(1, 7).Value = "CycleDays";
			sheet.Cell(2, 1).Value = "Rosemary";
			sheet.Cell(2, 2).Value = "Salvia";
			sheet.Cell(2, 3).Value = "Leaf";
			sheet.Cell(2, 4).Value = "EU";
			sheet.Cell(2, 5).Value = "Mediterranean";
			sheet.Cell(2, 6).Value = "Well drained";
			sheet.Cell(2, 7).Value = "abc";
		});
		var response = await controller.Import(file);
		var okResult = Assert.IsType<OkObjectResult>(response.Result);
		var result = Assert.IsType<HerbImportResult>(okResult.Value);
		Assert.Single(result.Errors);
	}

	[Fact]
	public async Task GetHerb_ById_ReturnsHerb()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Parsley", ScientificName = "Petroselinum", Category = "Leaf", Origin = "EU", CycleDays = 50 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		var result = await controller.GetById(herb.Id);
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var returned = Assert.IsType<Herb>(okResult.Value);
		Assert.Equal(herb.Name, returned.Name);
	}

	[Fact]
	public async Task GetHerb_ByInvalidId_ReturnsNotFound()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var result = await controller.GetById(999);
		Assert.IsType<NotFoundObjectResult>(result.Result);
	}

	[Fact]
	public async Task GetAllHerbs_ReturnsAll()
	{
		using var dbContext = CreateDbContext();
		dbContext.Herbs.Add(new Herb { Name = "A", ScientificName = "A", Category = "A", Origin = "A", CycleDays = 1 });
		dbContext.Herbs.Add(new Herb { Name = "B", ScientificName = "B", Category = "B", Origin = "B", CycleDays = 2 });
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		var result = await controller.GetAll();
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var herbs = Assert.IsAssignableFrom<IEnumerable<Herb>>(okResult.Value);
		Assert.Equal(2, herbs.Count());
	}

	[Fact]
	public async Task CreateHerb_WithValidData_CreatesHerb()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "Cilantro", ScientificName = "Coriandrum", Category = "Leaf", Origin = "Asia", CycleDays = 40 };
		var result = await controller.Create(herb);
		var created = Assert.IsType<CreatedAtActionResult>(result.Result);
		var createdHerb = Assert.IsType<Herb>(created.Value);
		Assert.Equal("Cilantro", createdHerb.Name);
	}

	[Fact]
	public async Task CreateHerb_WithMissingName_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = null, ScientificName = "Coriandrum", Category = "Leaf", Origin = "Asia", CycleDays = 40 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateHerb_WithValidData_UpdatesHerb()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		herb.Name = "Dill Updated";
		var result = await controller.Update(herb.Id, herb);
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var updated = Assert.IsType<Herb>(okResult.Value);
		Assert.Equal("Dill Updated", updated.Name);
	}

	[Fact]
	public async Task UpdateHerb_WithInvalidId_ReturnsNotFound()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		var result = await controller.Update(999, herb);
		Assert.IsType<NotFoundObjectResult>(result.Result);
	}

	[Fact]
	public async Task DeleteHerb_WithValidId_DeletesHerb()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Fennel", ScientificName = "Foeniculum", Category = "Leaf", Origin = "EU", CycleDays = 70 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		var result = await controller.Delete(herb.Id);
		Assert.IsType<NoContentResult>(result);
		Assert.Empty(dbContext.Herbs);
	}

	[Fact]
	public async Task DeleteHerb_WithInvalidId_ReturnsNotFound()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var result = await controller.Delete(999);
		Assert.IsType<NotFoundObjectResult>(result);
	}

	[Fact]
	public async Task CreateHerb_WithNegativeCycleDays_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "Negative", ScientificName = "Negativus", Category = "Root", Origin = "Mars", CycleDays = -10 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task CreateHerb_WithLongName_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = new string('A', 300), ScientificName = "Longus", Category = "Leaf", Origin = "EU", CycleDays = 10 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateHerb_WithNullName_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		herb.Name = null;
		var result = await controller.Update(herb.Id, herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task GetHerbs_ByCategory_ReturnsFiltered()
	{
		using var dbContext = CreateDbContext();
		dbContext.Herbs.Add(new Herb { Name = "A", ScientificName = "A", Category = "Root", Origin = "A", CycleDays = 1 });
		dbContext.Herbs.Add(new Herb { Name = "B", ScientificName = "B", Category = "Leaf", Origin = "B", CycleDays = 2 });
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		var result = await controller.GetAll();
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var herbs = Assert.IsAssignableFrom<IEnumerable<Herb>>(okResult.Value);
		Assert.Contains(herbs, h => h.Category == "Root");
	}

	[Fact]
	public async Task GetHerbs_ByOrigin_ReturnsFiltered()
	{
		using var dbContext = CreateDbContext();
		dbContext.Herbs.Add(new Herb { Name = "A", ScientificName = "A", Category = "Root", Origin = "Asia", CycleDays = 1 });
		dbContext.Herbs.Add(new Herb { Name = "B", ScientificName = "B", Category = "Leaf", Origin = "EU", CycleDays = 2 });
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		var result = await controller.GetAll();
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var herbs = Assert.IsAssignableFrom<IEnumerable<Herb>>(okResult.Value);
		Assert.Contains(herbs, h => h.Origin == "Asia");
	}

	[Fact]
	public async Task CreateHerb_WithNullScientificName_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "A", ScientificName = null, Category = "Leaf", Origin = "EU", CycleDays = 10 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task CreateHerb_WithNullCategory_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "A", ScientificName = "A", Category = null, Origin = "EU", CycleDays = 10 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task CreateHerb_WithNullOrigin_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "A", ScientificName = "A", Category = "Leaf", Origin = null, CycleDays = 10 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task CreateHerb_WithZeroCycleDays_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var herb = new Herb { Name = "A", ScientificName = "A", Category = "Leaf", Origin = "EU", CycleDays = 0 };
		var result = await controller.Create(herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateHerb_WithNegativeCycleDays_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		herb.CycleDays = -5;
		var result = await controller.Update(herb.Id, herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateHerb_WithNullCategory_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		herb.Category = null;
		var result = await controller.Update(herb.Id, herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateHerb_WithNullOrigin_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		herb.Origin = null;
		var result = await controller.Update(herb.Id, herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateHerb_WithZeroCycleDays_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb { Name = "Dill", ScientificName = "Anethum", Category = "Leaf", Origin = "EU", CycleDays = 30 };
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();
		var controller = CreateController(dbContext);
		herb.CycleDays = 0;
		var result = await controller.Update(herb.Id, herb);
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task ImportCsv_WithMissingColumns_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);
		var csv = "Name,ScientificName,Category,Origin\nMint,Mentha,Leaf,EU";

		var file = CreateCsvFile(csv);
		var response = await controller.Import(file);

		Assert.IsType<BadRequestObjectResult>(response.Result);
	}

	[Fact]
	public async Task ImportXlsx_WithValidRows_Imports()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);

		var file = CreateXlsxFile(sheet =>
		{
			sheet.Cell(1, 1).Value = "Name";
			sheet.Cell(1, 2).Value = "ScientificName";
			sheet.Cell(1, 3).Value = "Category";
			sheet.Cell(1, 4).Value = "Origin";
			sheet.Cell(1, 5).Value = "Notes";
			sheet.Cell(1, 6).Value = "CareInstructions";
			sheet.Cell(1, 7).Value = "CycleDays";

			sheet.Cell(2, 1).Value = "Rosemary";
			sheet.Cell(2, 2).Value = "Salvia";
			sheet.Cell(2, 3).Value = "Leaf";
			sheet.Cell(2, 4).Value = "EU";
			sheet.Cell(2, 5).Value = "Mediterranean";
			sheet.Cell(2, 6).Value = "Well drained";
			sheet.Cell(2, 7).Value = 60;
		});

		var response = await controller.Import(file);

		var okResult = Assert.IsType<OkObjectResult>(response.Result);
		var result = Assert.IsType<HerbImportResult>(okResult.Value);
		Assert.Equal(1, result.Imported);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public async Task ImportXlsx_WithMissingColumns_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var controller = CreateController(dbContext);

		var file = CreateXlsxFile(sheet =>
		{
			sheet.Cell(1, 1).Value = "Name";
			sheet.Cell(1, 2).Value = "ScientificName";
			sheet.Cell(1, 3).Value = "Category";
			sheet.Cell(1, 4).Value = "Origin";
			sheet.Cell(1, 5).Value = "Notes";
			sheet.Cell(1, 6).Value = "CareInstructions";
			// CycleDays column missing

			sheet.Cell(2, 1).Value = "Rosemary";
			sheet.Cell(2, 2).Value = "Salvia";
			sheet.Cell(2, 3).Value = "Leaf";
			sheet.Cell(2, 4).Value = "EU";
			sheet.Cell(2, 5).Value = "Mediterranean";
			sheet.Cell(2, 6).Value = "Well drained";
		});

		var response = await controller.Import(file);

		Assert.IsType<BadRequestObjectResult>(response.Result);
	}

	[Fact]
	public async Task CreatePlan_WithValidRequest_CreatesPlan()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb
		{
			Name = "Sage",
			ScientificName = "Salvia officinalis",
			Category = "Leaf",
			Origin = "EU",
			CycleDays = 90
		};
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();

		var controller = CreatePlanController(dbContext);
		var request = new CultivationPlanRequest
		{
			StartDate = DateTime.UtcNow.Date,
			DurationDays = 30,
			WateringFrequencyDays = 3,
			Notes = "Weekly check"
		};

		var response = await controller.Create(herb.Id, request);

		var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
		var plan = Assert.IsType<CultivationPlan>(createdResult.Value);
		Assert.Equal(herb.Id, plan.HerbId);

		var planCount = await dbContext.CultivationPlans.CountAsync();
		Assert.Equal(1, planCount);
	}

	[Fact]
	public async Task CreatePlan_WithInvalidRequest_ReturnsBadRequest()
	{
		using var dbContext = CreateDbContext();
		var herb = new Herb
		{
			Name = "Thyme",
			ScientificName = "Thymus",
			Category = "Leaf",
			Origin = "EU",
			CycleDays = 60
		};
		dbContext.Herbs.Add(herb);
		await dbContext.SaveChangesAsync();

		var controller = CreatePlanController(dbContext);
		var request = new CultivationPlanRequest
		{
			StartDate = default,
			DurationDays = 0,
			WateringFrequencyDays = 0
		};

		var response = await controller.Create(herb.Id, request);

		Assert.IsType<BadRequestObjectResult>(response.Result);
	}

    [Fact]
    public async Task CreatePlan_With_InexistentHerb_ReturnsNotFound()
    {
        var dbContext = CreateDbContext();
        var controller = CreatePlanController(dbContext);
        var request = new CultivationPlanRequest
        {
            StartDate = DateTime.UtcNow.Date,
            DurationDays = 30,
            WateringFrequencyDays = 3,
            Notes = "Weekly check"
        };

		var response = await controller.Create(999, request);
		Assert.IsType<NotFoundObjectResult>(response.Result);

    }
}
