using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HerbsController : ControllerBase
{
	private readonly HerbService _herbService;

	public HerbsController(HerbService herbService)
	{
		_herbService = herbService;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Herb>>> GetAll()
	{
		var herbs = await _herbService.GetAllAsync();
		return Ok(herbs);
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<Herb>> GetById([FromRoute] int id)
	{
		if (id <= 0)
		{
			return BadRequest(new { message = "Id invalido" });
		}

		var herb = await _herbService.GetByIdAsync(id);

		if (herb == null)
		{
			return NotFound(new { message = "Erva nao encontrada" });
		}

		return Ok(herb);
	}

	[HttpPost]
	public async Task<ActionResult<Herb>> Create([FromBody] Herb herb)
	{
		var validationError = ValidateHerb(herb);
		if (validationError != null)
		{
			return BadRequest(new { message = validationError });
		}

		try
		{
			var created = await _herbService.CreateAsync(herb);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}
		catch (Exception ex)
		{
			// Log the exception details for debugging
			var errorMessage = ex.InnerException?.Message ?? ex.Message;
			
			// Check for duplicate constraint violation
			if (errorMessage.Contains("duplicate key") || errorMessage.Contains("23505"))
			{
				return Conflict(new { message = "Já existe uma erva com este nome e nome científico", details = errorMessage });
			}
			
			return StatusCode(500, new { message = "Erro ao criar erva", details = errorMessage });
		}
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult<Herb>> Update([FromRoute] int id, [FromBody] Herb updated)
	{
		if (id <= 0)
		{
			return BadRequest(new { message = "Id invalido" });
		}

		var validationError = ValidateHerb(updated);
		if (validationError != null)
		{
			return BadRequest(new { message = validationError });
		}

		var updatedHerb = await _herbService.UpdateAsync(id, updated);
		if (updatedHerb == null)
		{
			return NotFound(new { message = "Erva nao encontrada" });
		}

		return Ok(updatedHerb);
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> Delete([FromRoute] int id)
	{
		if (id <= 0)
		{
			return BadRequest(new { message = "Id invalido" });
		}

		var deleted = await _herbService.DeleteAsync(id);
		if (!deleted)
		{
			return NotFound(new { message = "Erva nao encontrada" });
		}

		return NoContent();
	}


	[HttpPost("import")]
	[ApiExplorerSettings(IgnoreApi = true)]
	[Consumes("multipart/form-data")]
	[ProducesResponseType(typeof(HerbImportResult), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<HerbImportResult>> Import([FromForm] IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest(new { message = "Arquivo e obrigatorio" });
		}

		if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) &&
			!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
		{
			return BadRequest(new { message = "Apenas arquivos CSV e XLSX sao suportados" });
		}

		try
		{
			var result = await _herbService.ImportAsync(file);
			return CreatedAtAction(nameof(GetAll), result);
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	private static string? ValidateHerb(Herb herb)
	{
		if (herb == null)
		{
			return "Dados da erva sao obrigatorios";
		}

		if (string.IsNullOrWhiteSpace(herb.Name) ||
			string.IsNullOrWhiteSpace(herb.ScientificName) ||
			string.IsNullOrWhiteSpace(herb.Category) ||
			string.IsNullOrWhiteSpace(herb.Origin))
		{
			return "Campos obrigatorios estao vazios";
		}

		if (herb.CycleDays <= 0)
		{
			return "CycleDays deve ser maior que zero";
		}

		return null;
	}
}