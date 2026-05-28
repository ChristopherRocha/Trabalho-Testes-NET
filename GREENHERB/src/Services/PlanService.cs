using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class PlanService
{
	// Comentar a linha abaixo para usar BD real; descomente para usar mock
	private readonly AppDbContext? _dbContext;
	private bool _useMock = true;

	public PlanService(AppDbContext? dbContext = null)
	{
		_dbContext = dbContext;
		_useMock = dbContext == null; // Usa mock se dbContext é null
	}

	public Task<List<CultivationPlan>> GetAllAsync(int herbId)
	{
		if (_useMock)
			return Task.FromResult(MockDataProvider.GetAllPlans(herbId));

		return _dbContext!.CultivationPlans
			.AsNoTracking()
			.Where(p => p.HerbId == herbId)
			.ToListAsync();
	}

	public Task<CultivationPlan?> GetByIdAsync(int herbId, int planId)
	{
		if (_useMock)
		{
			var plan = MockDataProvider.GetPlanById(planId);
			return Task.FromResult(plan?.HerbId == herbId ? plan : null);
		}

		return _dbContext!.CultivationPlans
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.HerbId == herbId && p.Id == planId);
	}

	public async Task<CultivationPlan> CreateAsync(int herbId, CultivationPlanRequest request)
	{
		var plan = new CultivationPlan
		{
			HerbId = herbId,
			StartDate = request.StartDate,
			DurationDays = request.DurationDays,
			WateringFrequencyDays = request.WateringFrequencyDays,
			Notes = request.Notes
		};

		if (_useMock)
			return MockDataProvider.AddPlan(plan);

		_dbContext!.CultivationPlans.Add(plan);
		await _dbContext.SaveChangesAsync();
		return plan;
	}

	public async Task<CultivationPlan?> UpdateAsync(int herbId, int planId, CultivationPlanRequest request)
	{
		if (_useMock)
		{
			var plan = MockDataProvider.GetPlanById(planId);
			if (plan == null || plan.HerbId != herbId) return null;
			
			var updated = new CultivationPlan
			{
				StartDate = request.StartDate,
				DurationDays = request.DurationDays,
				WateringFrequencyDays = request.WateringFrequencyDays,
				Notes = request.Notes
			};
			return MockDataProvider.UpdatePlan(planId, updated);
		}

		var planDb = await _dbContext!.CultivationPlans
			.FirstOrDefaultAsync(p => p.HerbId == herbId && p.Id == planId);

		if (planDb == null)
		{
			return null;
		}

		planDb.StartDate = request.StartDate;
		planDb.DurationDays = request.DurationDays;
		planDb.WateringFrequencyDays = request.WateringFrequencyDays;
		planDb.Notes = request.Notes;

		await _dbContext.SaveChangesAsync();
		return planDb;
	}

	public async Task<bool> DeleteAsync(int herbId, int planId)
	{
		if (_useMock)
		{
			var mockPlan = MockDataProvider.GetPlanById(planId);
			if (mockPlan == null || mockPlan.HerbId != herbId) return false;
			return MockDataProvider.DeletePlan(planId);
		}

		var plan = await _dbContext!.CultivationPlans
			.FirstOrDefaultAsync(p => p.HerbId == herbId && p.Id == planId);

		if (plan == null)
		{
			return false;
		}

		_dbContext.CultivationPlans.Remove(plan);
		await _dbContext.SaveChangesAsync();
		return true;
	}
}
