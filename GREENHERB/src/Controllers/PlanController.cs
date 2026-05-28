using GREENHERB.src.Models;
using Microsoft.AspNetCore.Mvc;
using GREENHERB.src.Services;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/plans")]
public class PlanController : ControllerBase
{
	private readonly HerbService _herbService;
	private readonly PlanService _planService;

	public PlanController(HerbService herbService, PlanService planService)
	{
		_herbService = herbService;
		_planService = planService;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<CultivationPlan>>> GetAll([FromQuery] int herbId = 0)
	{
		// Se herbId for 0 ou negativo, retornar todos os planos
		if (herbId > 0)
		{
			if (!await _herbService.ExistsAsync(herbId))
			{
				return NotFound(new { message = "Erva nao encontrada" });
			}
			var plansByHerb = await _planService.GetAllAsync(herbId);
			return Ok(plansByHerb);
		}

		// Fallback: retornar lista vazia ou todos os planos
		return Ok(new List<CultivationPlan>());
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<CultivationPlan>> GetById(
		[FromRoute] int id,
		[FromQuery] int herbId = 0)
	{
		if (id <= 0)
		{
			return BadRequest(new { message = "Id invalido" });
		}

		if (herbId > 0 && !await _herbService.ExistsAsync(herbId))
		{
			return NotFound(new { message = "Erva nao encontrada" });
		}

		var plan = herbId > 0 
			? await _planService.GetByIdAsync(herbId, id)
			: new CultivationPlan { Id = id, HerbId = 1, StartDate = DateTime.UtcNow, DurationDays = 60 }; // Fallback mock
		
		if (plan == null)
		{
			return NotFound(new { message = "Plano nao encontrado" });
		}

		return Ok(plan);
	}

	[HttpPost]
	public async Task<ActionResult<CultivationPlan>> Create(
		[FromBody] CultivationPlan plan,
		[FromQuery] int herbId = 0)
	{
		if (plan == null)
		{
			return BadRequest(new { message = "Dados do plano sao obrigatorios" });
		}

		if (plan.HerbId <= 0 && herbId <= 0)
		{
			return BadRequest(new { message = "HerbId invalido" });
		}

		if (herbId > 0)
		{
			plan.HerbId = herbId;
		}

		if (plan.DurationDays <= 0)
		{
			return BadRequest(new { message = "DurationDays deve ser maior que zero" });
		}

		if (plan.StartDate == default || plan.StartDate < DateTime.UtcNow.AddHours(-24))
		{
			return BadRequest(new { message = "StartDate deve ser valida e nao pode estar no passado" });
		}

		if (plan.WateringFrequencyDays <= 0)
		{
			return BadRequest(new { message = "WateringFrequencyDays deve ser maior que zero" });
		}

		if (plan.TemperatureMin.HasValue && plan.TemperatureMax.HasValue)
		{
			if (plan.TemperatureMin < -50 || plan.TemperatureMin > 60)
			{
				return BadRequest(new { message = "TemperatureMin deve estar entre -50 e 60" });
			}
			if (plan.TemperatureMax < -50 || plan.TemperatureMax > 60)
			{
				return BadRequest(new { message = "TemperatureMax deve estar entre -50 e 60" });
			}
			if (plan.TemperatureMin >= plan.TemperatureMax)
			{
				return BadRequest(new { message = "TemperatureMin deve ser menor que TemperatureMax" });
			}
		}

		if (plan.HumidityMin.HasValue && plan.HumidityMax.HasValue)
		{
			if (plan.HumidityMin < 0 || plan.HumidityMin > 100)
			{
				return BadRequest(new { message = "HumidityMin deve estar entre 0 e 100" });
			}
			if (plan.HumidityMax < 0 || plan.HumidityMax > 100)
			{
				return BadRequest(new { message = "HumidityMax deve estar entre 0 e 100" });
			}
			if (plan.HumidityMin > plan.HumidityMax)
			{
				return BadRequest(new { message = "HumidityMin deve ser menor ou igual a HumidityMax" });
			}
		}

		if (plan.LuminosityMin.HasValue && plan.LuminosityMax.HasValue)
		{
			if (plan.LuminosityMin < 0 || plan.LuminosityMin > 100000)
			{
				return BadRequest(new { message = "LuminosityMin deve estar entre 0 e 100000 (lux)" });
			}
			if (plan.LuminosityMax < 0 || plan.LuminosityMax > 100000)
			{
				return BadRequest(new { message = "LuminosityMax deve estar entre 0 e 100000 (lux)" });
			}
			if (plan.LuminosityMin > plan.LuminosityMax)
			{
				return BadRequest(new { message = "LuminosityMin deve ser menor ou igual a LuminosityMax" });
			}
		}

		plan.CreatedAt = DateTime.UtcNow;
		plan.UpdatedAt = DateTime.UtcNow;
		plan.Id = new Random().Next(1000, 9999); // Mock ID gerado

		return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult<CultivationPlan>> Update(
		[FromRoute] int id,
		[FromBody] CultivationPlan request,
		[FromQuery] int herbId = 0)
	{
		if (id <= 0)
		{
			return BadRequest(new { message = "Id invalido" });
		}

		if (request == null)
		{
			return BadRequest(new { message = "Dados do plano sao obrigatorios" });
		}

		// Retornar o plano atualizado (mock)
		request.Id = id;
		request.UpdatedAt = DateTime.UtcNow;

		return Ok(request);
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> Delete(
		[FromRoute] int id)
	{
		if (id <= 0)
		{
			return BadRequest(new { message = "Id invalido" });
		}

		// Mock: sempre retornar sucesso
		return NoContent();
	}
}
