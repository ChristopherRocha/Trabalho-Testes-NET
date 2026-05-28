using System.Text;
using ClosedXML.Excel;
using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class HerbService
{
	private static readonly string[] RequiredColumns =
	{
		"name",
		"scientificname",
		"category",
		"origin",
		"cycledays"
	};

	// Comentar a linha abaixo para usar BD real; descomente para usar mock
	private readonly AppDbContext? _dbContext;
	private bool _useMock = true;

	public HerbService(AppDbContext? dbContext = null)
	{
		_dbContext = dbContext;
		_useMock = dbContext == null; // Usa mock se dbContext é null
	}

	public Task<List<Herb>> GetAllAsync()
	{
		if (_useMock)
			return Task.FromResult(MockDataProvider.GetAllHerbs());

		return _dbContext!.Herbs
			.AsNoTracking()
			.ToListAsync();
	}

	public Task<bool> ExistsAsync(int herbId)
	{
		if (_useMock)
			return Task.FromResult(MockDataProvider.GetHerbById(herbId) != null);

		return _dbContext!.Herbs.AnyAsync(h => h.Id == herbId);
	}

	public Task<Herb?> GetByIdAsync(int herbId)
	{
		if (_useMock)
			return Task.FromResult(MockDataProvider.GetHerbById(herbId));

		return _dbContext!.Herbs
			.AsNoTracking()
			.Include(h => h.CultivationPlans)
			.FirstOrDefaultAsync(h => h.Id == herbId);
	}

	public async Task<Herb> CreateAsync(Herb herb)
	{
		if (_useMock)
			return MockDataProvider.AddHerb(herb);

		_dbContext!.Herbs.Add(herb);
		await _dbContext.SaveChangesAsync();
		return herb;
	}

	public async Task<Herb?> UpdateAsync(int herbId, Herb updated)
	{
		if (_useMock)
			return MockDataProvider.UpdateHerb(herbId, updated);

		var existing = await _dbContext!.Herbs.FindAsync(herbId);
		if (existing == null)
		{
			return null;
		}

		existing.Name = updated.Name;
		existing.ScientificName = updated.ScientificName;
		existing.Category = updated.Category;
		existing.Origin = updated.Origin;
		existing.Notes = updated.Notes;
		existing.CareInstructions = updated.CareInstructions;
		existing.CycleDays = updated.CycleDays;

		await _dbContext.SaveChangesAsync();
		return existing;
	}

	public async Task<bool> DeleteAsync(int herbId)
	{
		if (_useMock)
			return MockDataProvider.DeleteHerb(herbId);

		var existing = await _dbContext!.Herbs.FindAsync(herbId);
		if (existing == null)
		{
			return false;
		}

		_dbContext.Herbs.Remove(existing);
		await _dbContext.SaveChangesAsync();
		return true;
	}

	public async Task<HerbImportResult> ImportAsync(IFormFile file)
	{
		var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
		var result = extension switch
		{
			".csv" => await ImportCsvAsync(file),
			".xlsx" => await ImportXlsxAsync(file),
			_ => throw new InvalidOperationException("Formato de arquivo nao suportado")
		};

		return result;
	}

	private async Task<HerbImportResult> ImportCsvAsync(IFormFile file)
	{
		using var stream = file.OpenReadStream();
		using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: false);

		var headerLine = await reader.ReadLineAsync();
		if (string.IsNullOrWhiteSpace(headerLine))
		{
			throw new InvalidOperationException("Cabecalho CSV ausente");
		}

		var headerIndex = BuildHeaderIndex(ParseCsvLine(headerLine));
		ValidateRequiredColumns(headerIndex);

		var result = new HerbImportResult();
		var rowNumber = 1;
		string? line;
		while ((line = await reader.ReadLineAsync()) != null)
		{
			rowNumber++;
			if (string.IsNullOrWhiteSpace(line))
			{
				result.Skipped++;
				continue;
			}

			var values = ParseCsvLine(line);
			if (!TryParseRow(key => GetValue(values, headerIndex, key), rowNumber, out var row, out var error))
			{
				result.Errors.Add(error);
				continue;
			}

			await UpsertHerbAsync(row, result);
		}

		await _dbContext.SaveChangesAsync();
		return result;
	}

	private async Task<HerbImportResult> ImportXlsxAsync(IFormFile file)
	{
		using var stream = file.OpenReadStream();
		using var workbook = new XLWorkbook(stream);
		var worksheet = workbook.Worksheets.FirstOrDefault();
		if (worksheet == null)
		{
			throw new InvalidOperationException("Planilha XLSX nao encontrada");
		}

		var headerRow = worksheet.Row(1);
		var headers = headerRow.Cells().Select(c => c.GetString()).ToList();
		var headerIndex = BuildHeaderIndex(headers);
		ValidateRequiredColumns(headerIndex);

		var result = new HerbImportResult();
		var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

		for (var rowIndex = 2; rowIndex <= lastRow; rowIndex++)
		{
			var row = worksheet.Row(rowIndex);
			if (row.IsEmpty())
			{
				result.Skipped++;
				continue;
			}

			if (!TryParseRow(key => GetValue(row, headerIndex, key), rowIndex, out var rowData, out var error))
			{
				result.Errors.Add(error);
				continue;
			}

			await UpsertHerbAsync(rowData, result);
		}

		await (_useMock ? Task.CompletedTask : _dbContext!.SaveChangesAsync());
		return result;
	}

	private async Task UpsertHerbAsync(HerbImportRow row, HerbImportResult result)
	{
		var normalizedName = row.Name.ToLowerInvariant();
		var normalizedScientificName = row.ScientificName.ToLowerInvariant();

		Herb? existing;
		if (_useMock)
		{
			existing = MockDataProvider.GetAllHerbs().FirstOrDefault(h =>
				h.Name.ToLower() == normalizedName &&
				h.ScientificName.ToLower() == normalizedScientificName);
		}
		else
		{
			existing = await _dbContext!.Herbs.FirstOrDefaultAsync(h =>
				h.Name.ToLower() == normalizedName &&
				h.ScientificName.ToLower() == normalizedScientificName);
		}

		if (existing == null)
		{
			var herb = new Herb
			{
				Name = row.Name,
				ScientificName = row.ScientificName,
				Category = row.Category,
				Origin = row.Origin,
				Notes = row.Notes,
				CareInstructions = row.CareInstructions,
				CycleDays = row.CycleDays
			};

			if (_useMock)
				MockDataProvider.AddHerb(herb);
			else
				_dbContext!.Herbs.Add(herb);

			result.Imported++;
			return;
		}

		existing.Category = row.Category;
		existing.Origin = row.Origin;
		existing.Notes = row.Notes;
		existing.CareInstructions = row.CareInstructions;
		existing.CycleDays = row.CycleDays;
		result.Updated++;
	}

	private static void ValidateRequiredColumns(Dictionary<string, int> headerIndex)
	{
		var missing = RequiredColumns.Where(column => !headerIndex.ContainsKey(column)).ToList();
		if (missing.Count > 0)
		{
			throw new InvalidOperationException($"Colunas obrigatorias ausentes: {string.Join(", ", missing)}");
		}
	}

	private static Dictionary<string, int> BuildHeaderIndex(IEnumerable<string> headers)
	{
		var headerIndex = new Dictionary<string, int>();
		var index = 0;

		foreach (var header in headers)
		{
			var normalized = NormalizeHeader(header);
			if (!string.IsNullOrWhiteSpace(normalized) && !headerIndex.ContainsKey(normalized))
			{
				headerIndex[normalized] = index;
			}

			index++;
		}

		return headerIndex;
	}

	private static bool TryParseRow(
		Func<string, string?> valueProvider,
		int rowNumber,
		out HerbImportRow row,
		out HerbImportError error)
	{
		row = new HerbImportRow();
		error = new HerbImportError { Row = rowNumber };

		row.Name = GetRequiredValue(valueProvider, "name");
		row.ScientificName = GetRequiredValue(valueProvider, "scientificname");
		row.Category = GetRequiredValue(valueProvider, "category");
		row.Origin = GetRequiredValue(valueProvider, "origin");
		row.Notes = GetOptionalValue(valueProvider, "notes");
		row.CareInstructions = GetOptionalValue(valueProvider, "careinstructions");

		var cycleText = GetRequiredValue(valueProvider, "cycledays");
		if (!int.TryParse(cycleText, out var cycleDays) || cycleDays <= 0)
		{
			error.Message = "CycleDays invalido";
			return false;
		}

		row.CycleDays = cycleDays;

		if (string.IsNullOrWhiteSpace(row.Name) ||
			string.IsNullOrWhiteSpace(row.ScientificName) ||
			string.IsNullOrWhiteSpace(row.Category) ||
			string.IsNullOrWhiteSpace(row.Origin))
		{
			error.Message = "Campos obrigatorios vazios";
			return false;
		}

		return true;
	}

	private static string GetRequiredValue(Func<string, string?> valueProvider, string key)
	{
		return valueProvider(key)?.Trim() ?? string.Empty;
	}

	private static string? GetOptionalValue(Func<string, string?> valueProvider, string key)
	{
		var value = valueProvider(key)?.Trim();
		return string.IsNullOrWhiteSpace(value) ? null : value;
	}

	private static string? GetValue(List<string> values, Dictionary<string, int> headerIndex, string key)
	{
		if (!headerIndex.TryGetValue(key, out var index))
		{
			return null;
		}

		return index >= 0 && index < values.Count ? values[index] : null;
	}

	private static string? GetValue(IXLRow row, Dictionary<string, int> headerIndex, string key)
	{
		if (!headerIndex.TryGetValue(key, out var index))
		{
			return null;
		}

		return row.Cell(index + 1).GetString();
	}

	private static List<string> ParseCsvLine(string line)
	{
		var values = new List<string>();
		var current = new StringBuilder();
		var inQuotes = false;

		for (var i = 0; i < line.Length; i++)
		{
			var character = line[i];
			if (character == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					current.Append('"');
					i++;
				}
				else
				{
					inQuotes = !inQuotes;
				}

				continue;
			}

			if (character == ',' && !inQuotes)
			{
				values.Add(current.ToString());
				current.Clear();
				continue;
			}

			current.Append(character);
		}

		values.Add(current.ToString());
		return values;
	}

	private static string NormalizeHeader(string header)
	{
		var normalized = new string(header
			.Where(character => !char.IsWhiteSpace(character) && character != '_' && character != '-')
			.ToArray());

		return normalized.Trim().ToLowerInvariant();
	}

	private sealed class HerbImportRow
	{
		public string Name { get; set; } = string.Empty;
		public string ScientificName { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public string Origin { get; set; } = string.Empty;
		public string? Notes { get; set; }
		public string? CareInstructions { get; set; }
		public int CycleDays { get; set; }
	}
}
