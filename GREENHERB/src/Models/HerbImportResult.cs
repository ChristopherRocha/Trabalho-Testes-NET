namespace GREENHERB.src.Models;

public class HerbImportResult
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<HerbImportError> Errors { get; set; } = new();
}

public class HerbImportError
{
    public int Row { get; set; }
    public string Message { get; set; } = string.Empty;
}
