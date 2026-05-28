using Xunit;
using Moq;
using GREENHERB.src.Controllers;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.Tests.tests;

public class PlanTestes
{
    private readonly Mock<HerbService> _mockHerbService;
    private readonly Mock<PlanService> _mockPlanService;
    private readonly PlanController _controller;

    public PlanTestes()
    {
        _mockHerbService = new Mock<HerbService>();
        _mockPlanService = new Mock<PlanService>();
        _controller = new PlanController(_mockHerbService.Object, _mockPlanService.Object);
    }

    /*
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

		// Se herbId foi passado como query param, usar ele
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
    */

    #region DECISÃO 1: if (plan == null)

    /// <summary>
    /// TU-261: Verifica se plan nulo retorna BadRequest
    /// DECISÃO 1 [TRUE]: plan == null
    /// </summary>
    [Fact(DisplayName = "TU-261: Plan nulo → BadRequest")]
    public async Task TU261_CreateAsync_WithNullPlan_ReturnsBadRequest()
    {
        CultivationPlan? nullPlan = null;
        const int herbId = 0;

        var result = await _controller.Create(nullPlan!, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("Dados do plano sao obrigatorios", response.message);
    }

    #endregion

    #region DECISÃO 2: if (plan.HerbId <= 0 && herbId <= 0) - AND OPERATOR

    /// <summary>
    /// TU-262: Condição 1=TRUE AND Condição 2=TRUE
    /// plan.HerbId <= 0 (TRUE: -1) AND herbId <= 0 (TRUE: 0)
    /// Decisão 2 [TRUE] → Entra no if → Retorna erro HerbId
    /// </summary>
    [Fact(DisplayName = "TU-262: Dec2 [C1=T, C2=T] plan.HerbId=-1 AND herbId=0")]
    public async Task TU262_CreateAsync_Dec2_C1TrueC2True_BothHerbIdsInvalid()
    {
        var plan = new CultivationPlan
        {
            HerbId = -1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("HerbId invalido", response.message);
    }

    /// <summary>
    /// TU-263: Condição 1=TRUE AND Condição 2=FALSE
    /// plan.HerbId <= 0 (TRUE: 0) AND herbId <= 0 (FALSE: 5)
    /// Decisão 2 [FALSE] → Não entra no if → Vai para Decisão 3
    /// </summary>
    [Fact(DisplayName = "TU-263: Dec2 [C1=T, C2=F] plan.HerbId=0 AND herbId=5")]
    public async Task TU263_CreateAsync_Dec2_C1TrueC2False_InvalidBodyValidQuery()
    {
        var plan = new CultivationPlan
        {
            HerbId = 0,
            DurationDays = 60,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 5;

        var createdPlan = new CultivationPlan
        {
            Id = 1001,
            HerbId = herbId,
            DurationDays = 60,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-264: Condição 1=FALSE AND Condição 2=TRUE
    /// plan.HerbId <= 0 (FALSE: 3) AND herbId <= 0 (TRUE: 0)
    /// Decisão 2 [FALSE] → Não entra no if → Vai para Decisão 3
    /// </summary>
    [Fact(DisplayName = "TU-264: Dec2 [C1=F, C2=T] plan.HerbId=3 AND herbId=0")]
    public async Task TU264_CreateAsync_Dec2_C1FalseC2True_ValidBodyNoQuery()
    {
        var plan = new CultivationPlan
        {
            HerbId = 3,
            DurationDays = 60,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1002,
            HerbId = 3,
            DurationDays = 60,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-265: Condição 1=FALSE AND Condição 2=FALSE
    /// plan.HerbId <= 0 (FALSE: 2) AND herbId <= 0 (FALSE: 7)
    /// Decisão 2 [FALSE] → Não entra no if → Vai para Decisão 3
    /// </summary>
    [Fact(DisplayName = "TU-265: Dec2 [C1=F, C2=F] plan.HerbId=2 AND herbId=7")]
    public async Task TU265_CreateAsync_Dec2_C1FalseC2False_BothHerbIdsValid()
    {
        var plan = new CultivationPlan
        {
            HerbId = 2,
            DurationDays = 60,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 7;

        var createdPlan = new CultivationPlan
        {
            Id = 1003,
            HerbId = herbId,
            DurationDays = 60,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-271: Boundary - Ambas condições = 0
    /// plan.HerbId = 0 (boundary) AND herbId = 0 (boundary)
    /// Decisão 2 [TRUE] → Retorna erro HerbId
    /// </summary>
    [Fact(DisplayName = "TU-271: Dec2 Boundary plan.HerbId=0 AND herbId=0")]
    public async Task TU271_CreateAsync_Dec2_Boundary_BothZero()
    {
        var plan = new CultivationPlan
        {
            HerbId = 0,
            DurationDays = 30,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("HerbId invalido", response.message);
    }

    #endregion

    #region DECISÃO 3: if (herbId > 0) - Sobrescrita de HerbId

    /// <summary>
    /// TU-266: Decisão 3 [TRUE]
    /// herbId > 0 (TRUE: 5)
    /// Plano vai usar herbId do query param (sobrescreve body)
    /// </summary>
    [Fact(DisplayName = "TU-266: Dec3 [T] herbId=5 sobrescreve")]
    public async Task TU266_CreateAsync_Dec3_True_QueryParamOverridesBody()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 45,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 8;

        var createdPlan = new CultivationPlan
        {
            Id = 1004,
            HerbId = herbId,
            DurationDays = 45,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-267: Decisão 3 [FALSE]
    /// herbId <= 0 (FALSE: 0)
    /// Plano usa HerbId do body (não sobrescreve)
    /// </summary>
    [Fact(DisplayName = "TU-267: Dec3 [F] herbId=0 não sobrescreve")]
    public async Task TU267_CreateAsync_Dec3_False_NoOverride()
    {
        var plan = new CultivationPlan
        {
            HerbId = 4,
            DurationDays = 50,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1005,
            HerbId = plan.HerbId,
            DurationDays = 50,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    #region DECISÃO 4: if (plan.DurationDays <= 0)

    /// <summary>
    /// TU-268: Decisão 4 [TRUE] - Boundary
    /// plan.DurationDays = 0
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-268: Dec4 [T] DurationDays=0 Boundary")]
    public async Task TU268_CreateAsync_Dec4_True_ZeroDays()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 0,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("DurationDays deve ser maior que zero", response.message);
    }

    /// <summary>
    /// TU-269: Decisão 4 [TRUE] - Negativo
    /// plan.DurationDays = -10
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-269: Dec4 [T] DurationDays=-10 Negativo")]
    public async Task TU269_CreateAsync_Dec4_True_NegativeDays()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = -10,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("DurationDays deve ser maior que zero", response.message);
    }

    /// <summary>
    /// TU-270: Decisão 4 [FALSE] - Boundary mínimo válido
    /// plan.DurationDays = 1
    /// Cria o plano com sucesso
    /// </summary>
    [Fact(DisplayName = "TU-270: Dec4 [F] DurationDays=1 Boundary válido")]
    public async Task TU270_CreateAsync_Dec4_False_MinimumValidDays()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 1,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1006,
            HerbId = 1,
            DurationDays = 1,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-272: Decisão 4 [FALSE] - Valor típico
    /// plan.DurationDays = 60
    /// Cria o plano com sucesso
    /// </summary>
    [Fact(DisplayName = "TU-272: Dec4 [F] DurationDays=60 Valor típico")]
    public async Task TU272_CreateAsync_Dec4_False_TypicalValue()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1007,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    #region DECISÃO 5: if (plan.StartDate <= default || passado)

    /// <summary>
    /// TU-273: Decisão 5 [TRUE] - StartDate nula/default
    /// plan.StartDate = default
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-273: Dec5 [T] StartDate=default (null)")]
    public async Task TU273_CreateAsync_Dec5_True_NullStartDate()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = default,
            WateringFrequencyDays = 3
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("StartDate deve ser valida e nao pode estar no passado", response.message);
    }

    /// <summary>
    /// TU-274: Decisão 5 [TRUE] - StartDate no passado (< 24h atrás)
    /// plan.StartDate = DateTime.UtcNow.AddDays(-5)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-274: Dec5 [T] StartDate no passado")]
    public async Task TU274_CreateAsync_Dec5_True_PastStartDate()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow.AddDays(-5),
            WateringFrequencyDays = 3
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("StartDate deve ser valida e nao pode estar no passado", response.message);
    }

    /// <summary>
    /// TU-275: Decisão 5 [FALSE] - StartDate válida (presente)
    /// plan.StartDate = DateTime.UtcNow
    /// Valida e continua
    /// </summary>
    [Fact(DisplayName = "TU-275: Dec5 [F] StartDate válida (agora)")]
    public async Task TU275_CreateAsync_Dec5_False_ValidStartDate()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 3
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1009,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-276: Decisão 5 [FALSE] - StartDate no futuro (válido)
    /// plan.StartDate = DateTime.UtcNow.AddDays(5)
    /// Valida e continua
    /// </summary>
    [Fact(DisplayName = "TU-276: Dec5 [F] StartDate no futuro")]
    public async Task TU276_CreateAsync_Dec5_False_FutureStartDate()
    {
        var futureDate = DateTime.UtcNow.AddDays(5);
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = futureDate,
            WateringFrequencyDays = 3
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1010,
            HerbId = 1,
            DurationDays = 60,
            StartDate = futureDate,
            WateringFrequencyDays = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    #region DECISÃO 6: if (plan.WateringFrequencyDays <= 0)

    /// <summary>
    /// TU-277: Decisão 6 [TRUE] - WateringFrequencyDays = 0
    /// plan.WateringFrequencyDays = 0 (boundary)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-277: Dec6 [T] WateringFrequencyDays=0 Boundary")]
    public async Task TU277_CreateAsync_Dec6_True_ZeroWateringFrequency()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 0
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("WateringFrequencyDays deve ser maior que zero", response.message);
    }

    /// <summary>
    /// TU-278: Decisão 6 [TRUE] - WateringFrequencyDays negativo
    /// plan.WateringFrequencyDays = -5
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-278: Dec6 [T] WateringFrequencyDays=-5 Negativo")]
    public async Task TU278_CreateAsync_Dec6_True_NegativeWateringFrequency()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = -5
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("WateringFrequencyDays deve ser maior que zero", response.message);
    }

    /// <summary>
    /// TU-279: Decisão 6 [FALSE] - WateringFrequencyDays = 1 (boundary mínimo válido)
    /// plan.WateringFrequencyDays = 1
    /// Cria o plano com sucesso
    /// </summary>
    [Fact(DisplayName = "TU-279: Dec6 [F] WateringFrequencyDays=1 Boundary válido")]
    public async Task TU279_CreateAsync_Dec6_False_MinimumValidWateringFrequency()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 1
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1011,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-280: Decisão 6 [FALSE] - WateringFrequencyDays = 7 (valor típico)
    /// plan.WateringFrequencyDays = 7
    /// Cria o plano com sucesso
    /// </summary>
    [Fact(DisplayName = "TU-280: Dec6 [F] WateringFrequencyDays=7 Valor típico")]
    public async Task TU280_CreateAsync_Dec6_False_TypicalWateringFrequency()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1012,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    #region DECISÃO 7: if (plan.TemperatureMin ou TemperatureMax inválidos)

    /// <summary>
    /// TU-287: Decisão 7 [TRUE] - TemperatureMin fora do range (-50 até 60)
    /// plan.TemperatureMin = -100 (inválido)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-287: Dec7 [T] TemperatureMin=-100 (abaixo do mínimo)")]
    public async Task TU287_CreateAsync_Dec7_True_TemperatureMinTooLow()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = -100,
            TemperatureMax = 25
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("TemperatureMin deve estar entre -50 e 60", response.message);
    }

    /// <summary>
    /// TU-288: Decisão 7 [TRUE] - TemperatureMax fora do range
    /// plan.TemperatureMax = 100 (inválido)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-288: Dec7 [T] TemperatureMax=100 (acima do máximo)")]
    public async Task TU288_CreateAsync_Dec7_True_TemperatureMaxTooHigh()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 10,
            TemperatureMax = 100
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("TemperatureMax deve estar entre -50 e 60", response.message);
    }

    /// <summary>
    /// TU-289: Decisão 7 [TRUE] - TemperatureMin >= TemperatureMax
    /// plan.TemperatureMin = 30, TemperatureMax = 20 (Min > Max)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-289: Dec7 [T] TemperatureMin > TemperatureMax")]
    public async Task TU289_CreateAsync_Dec7_True_TemperatureMinGreaterThanMax()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 30,
            TemperatureMax = 20
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("TemperatureMin deve ser menor que TemperatureMax", response.message);
    }

    /// <summary>
    /// TU-290: Decisão 7 [FALSE] - Temperatura válida (boundary -50 a 60)
    /// plan.TemperatureMin = -50, TemperatureMax = 60 (válido)
    /// Continua validação
    /// </summary>
    [Fact(DisplayName = "TU-290: Dec7 [F] Temperatura válida (-50 a 60)")]
    public async Task TU290_CreateAsync_Dec7_False_ValidTemperatureRange()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = -50,
            TemperatureMax = 60
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1014,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            TemperatureMin = -50,
            TemperatureMax = 60,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-291: Decisão 7 [FALSE] - Temperatura típica (15-30°C)
    /// plan.TemperatureMin = 15, TemperatureMax = 30 (valor típico)
    /// Continua validação
    /// </summary>
    [Fact(DisplayName = "TU-291: Dec7 [F] Temperatura típica (15-30)")]
    public async Task TU291_CreateAsync_Dec7_False_TypicalTemperatureRange()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1015,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    #region DECISÃO 8: if (plan.HumidityMin ou HumidityMax inválidos)

    /// <summary>
    /// TU-292: Decisão 8 [TRUE] - HumidityMin fora do range (0-100)
    /// plan.HumidityMin = -10 (inválido, menor que 0)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-292: Dec8 [T] HumidityMin=-10 (negativo)")]
    public async Task TU292_CreateAsync_Dec8_True_HumidityMinNegative()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = -10,
            HumidityMax = 80
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("HumidityMin deve estar entre 0 e 100", response.message);
    }

    /// <summary>
    /// TU-293: Decisão 8 [TRUE] - HumidityMax fora do range (0-100)
    /// plan.HumidityMax = 150 (inválido, maior que 100)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-293: Dec8 [T] HumidityMax=150 (acima de 100)")]
    public async Task TU293_CreateAsync_Dec8_True_HumidityMaxTooHigh()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 150
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("HumidityMax deve estar entre 0 e 100", response.message);
    }

    /// <summary>
    /// TU-294: Decisão 8 [TRUE] - HumidityMin > HumidityMax
    /// plan.HumidityMin = 80, HumidityMax = 40 (Min > Max)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-294: Dec8 [T] HumidityMin > HumidityMax")]
    public async Task TU294_CreateAsync_Dec8_True_HumidityMinGreaterThanMax()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 80,
            HumidityMax = 40
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("HumidityMin deve ser menor ou igual a HumidityMax", response.message);
    }

    /// <summary>
    /// TU-295: Decisão 8 [FALSE] - Umidade válida (boundary 0-100)
    /// plan.HumidityMin = 0, HumidityMax = 100
    /// Continua validação
    /// </summary>
    [Fact(DisplayName = "TU-295: Dec8 [F] Umidade válida (0-100)")]
    public async Task TU295_CreateAsync_Dec8_False_ValidHumidityRange()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 0,
            HumidityMax = 100
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1016,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 0,
            HumidityMax = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-296: Decisão 8 [FALSE] - Umidade típica (40-80%)
    /// plan.HumidityMin = 40, HumidityMax = 80
    /// Continua validação
    /// </summary>
    [Fact(DisplayName = "TU-296: Dec8 [F] Umidade típica (40-80)")]
    public async Task TU296_CreateAsync_Dec8_False_TypicalHumidityRange()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1017,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    #region DECISÃO 9: if (plan.LuminosityMin ou LuminosityMax inválidos)

    /// <summary>
    /// TU-297: Decisão 9 [TRUE] - LuminosityMin fora do range (0-100000 lux)
    /// plan.LuminosityMin = -100 (inválido, negativo)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-297: Dec9 [T] LuminosityMin=-100 (negativo)")]
    public async Task TU297_CreateAsync_Dec9_True_LuminosityMinNegative()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = -100,
            LuminosityMax = 5000
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("LuminosityMin deve estar entre 0 e 100000 (lux)", response.message);
    }

    /// <summary>
    /// TU-298: Decisão 9 [TRUE] - LuminosityMax fora do range (0-100000 lux)
    /// plan.LuminosityMax = 200000 (inválido, acima do máximo)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-298: Dec9 [T] LuminosityMax=200000 (acima do máximo)")]
    public async Task TU298_CreateAsync_Dec9_True_LuminosityMaxTooHigh()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 1000,
            LuminosityMax = 200000
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("LuminosityMax deve estar entre 0 e 100000 (lux)", response.message);
    }

    /// <summary>
    /// TU-299: Decisão 9 [TRUE] - LuminosityMin > LuminosityMax
    /// plan.LuminosityMin = 8000, LuminosityMax = 4000 (Min > Max)
    /// Retorna erro de validação
    /// </summary>
    [Fact(DisplayName = "TU-299: Dec9 [T] LuminosityMin > LuminosityMax")]
    public async Task TU299_CreateAsync_Dec9_True_LuminosityMinGreaterThanMax()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 8000,
            LuminosityMax = 4000
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("LuminosityMin deve ser menor ou igual a LuminosityMax", response.message);
    }

    /// <summary>
    /// TU-300: Decisão 9 [FALSE] - Luminosidade válida (boundary 0-100000 lux)
    /// plan.LuminosityMin = 0, LuminosityMax = 100000
    /// Continua validação
    /// </summary>
    [Fact(DisplayName = "TU-300: Dec9 [F] Luminosidade válida (0-100000)")]
    public async Task TU300_CreateAsync_Dec9_False_ValidLuminosityRange()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 0,
            LuminosityMax = 100000
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1018,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 0,
            LuminosityMax = 100000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    /// <summary>
    /// TU-301: Decisão 9 [FALSE] - Luminosidade típica (2000-5000 lux para plantas)
    /// plan.LuminosityMin = 2000, LuminosityMax = 5000
    /// Continua validação
    /// </summary>
    [Fact(DisplayName = "TU-301: Dec9 [F] Luminosidade típica (2000-5000)")]
    public async Task TU301_CreateAsync_Dec9_False_TypicalLuminosityRange()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = DateTime.UtcNow,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 2000,
            LuminosityMax = 5000
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1019,
            HerbId = 1,
            DurationDays = 60,
            StartDate = plan.StartDate,
            WateringFrequencyDays = 7,
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 2000,
            LuminosityMax = 5000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
    }

    #endregion

    /// <summary>
    /// TU-281: Precedência completa - HerbId, DurationDays, StartDate, WateringFrequency
    /// Todos inválidos → Deve validar HerbId primeiro
    /// </summary>
    [Fact(DisplayName = "TU-281: Todas validações inválidas - precedência HerbId")]
    public async Task TU281_CreateAsync_Precedence_AllInvalid_ChecksHerbIdFirst()
    {
        var plan = new CultivationPlan
        {
            HerbId = -1,
            DurationDays = -1,
            StartDate = default,
            WateringFrequencyDays = -1
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("HerbId invalido", response.message);
    }

    /// <summary>
    /// TU-284: DurationDays e StartDate inválidos → Deve validar DurationDays primeiro
    /// </summary>
    [Fact(DisplayName = "TU-284: DurationDays e StartDate inválidos - precedência Duration")]
    public async Task TU284_CreateAsync_DurationAndStartDateInvalid_ChecksDurationFirst()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = -1,
            StartDate = default,
            WateringFrequencyDays = 3
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("DurationDays deve ser maior que zero", response.message);
    }

    /// <summary>
    /// TU-285: StartDate e WateringFrequency inválidos → Deve validar StartDate primeiro
    /// </summary>
    [Fact(DisplayName = "TU-285: StartDate e WateringFrequency inválidos - precedência StartDate")]
    public async Task TU285_CreateAsync_StartDateAndWateringInvalid_ChecksStartDateFirst()
    {
        var plan = new CultivationPlan
        {
            HerbId = 1,
            DurationDays = 60,
            StartDate = default,
            WateringFrequencyDays = -1
        };
        const int herbId = 0;

        var result = await _controller.Create(plan, herbId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = (dynamic)badRequest.Value!;
        Assert.Equal("StartDate deve ser valida e nao pode estar no passado", response.message);
    }

    /// <summary>
    /// TU-286: Happy Path Completo - Todas as validações PASSAM
    /// Todos os dados completamente válidos
    /// </summary>
    [Fact(DisplayName = "TU-286: Happy Path Completo - Todas validações")]
    public async Task TU286_CreateAsync_CompleteHappyPath_AllValid()
    {
        var plan = new CultivationPlan
        {
            HerbId = 5,
            DurationDays = 45,
            StartDate = DateTime.UtcNow.AddDays(3),
            WateringFrequencyDays = 7,
            Notes = "Plano completo e válido",
            TemperatureMin = 15,
            TemperatureMax = 30,
            HumidityMin = 40,
            HumidityMax = 80,
            LuminosityMin = 2000,
            LuminosityMax = 5000
        };
        const int herbId = 0;

        var createdPlan = new CultivationPlan
        {
            Id = 1020,
            HerbId = plan.HerbId,
            DurationDays = plan.DurationDays,
            StartDate = plan.StartDate,
            WateringFrequencyDays = plan.WateringFrequencyDays,
            Notes = plan.Notes,
            TemperatureMin = plan.TemperatureMin,
            TemperatureMax = plan.TemperatureMax,
            HumidityMin = plan.HumidityMin,
            HumidityMax = plan.HumidityMax,
            LuminosityMin = plan.LuminosityMin,
            LuminosityMax = plan.LuminosityMax,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockPlanService
            .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CultivationPlanRequest>()))
            .ReturnsAsync(createdPlan);

        var result = await _controller.Create(plan, herbId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PlanController.GetById), created.ActionName);
        Assert.Equal(201, created.StatusCode);

        var returnedPlan = (CultivationPlan)created.Value!;
        Assert.NotNull(returnedPlan);
        Assert.Equal(plan.HerbId, returnedPlan.HerbId);
        Assert.Equal(plan.DurationDays, returnedPlan.DurationDays);
        Assert.Equal(plan.WateringFrequencyDays, returnedPlan.WateringFrequencyDays);
        Assert.Equal(plan.Notes, returnedPlan.Notes);
    }

}
