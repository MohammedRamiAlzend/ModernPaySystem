using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using SemanticSearchLib.Abstractions;
using SemanticSearchLib.Exceptions;

namespace SemanticSearchLib.Services;

public class FileParserService : IFileParser
{
    private static readonly HashSet<string> Supported = [".docx", ".xlsx", ".txt", ".md", ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif"];

    public bool SupportsFileType(string fileType)
    {
        var ext = fileType.StartsWith('.') ? fileType : $".{fileType}";
        return Supported.Contains(ext.ToLowerInvariant());
    }

    public async Task<string> ParseAsync(Stream fileStream, string fileType, CancellationToken ct = default)
    {
        var ext = fileType.StartsWith('.') ? fileType : $".{fileType}";
        ext = ext.ToLowerInvariant();

        return ext switch
        {
            ".docx" => await Task.Run(() => ParseDocx(fileStream), ct),
            ".xlsx" => await Task.Run(() => ParseXlsx(fileStream), ct),
            ".txt" => await ParseTextAsync(fileStream, ct),
            ".md" => await Task.Run(() => ParseMarkdown(fileStream), ct),
            ".pdf" or ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif" 
                => throw new SemanticSearchException($"File type '{fileType}' requires OCR processing, use SemanticSearchAppService"),
            _ => throw new SemanticSearchException($"Unsupported file type: {fileType}")
        };
    }

    private static string ParseDocx(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;
        if (body is null) return string.Empty;

        var paragraphs = body.Elements<Paragraph>()
            .Select(p => p.InnerText)
            .Where(t => !string.IsNullOrWhiteSpace(t));

        return string.Join(Environment.NewLine, paragraphs);
    }

    private static string ParseXlsx(Stream stream)
    {
        using var spreadsheet = SpreadsheetDocument.Open(stream, false);
        var workbookPart = spreadsheet.WorkbookPart;
        if (workbookPart is null) return string.Empty;

        var texts = new List<string>();

        foreach (var sheetPart in workbookPart.WorksheetParts)
        {
            var sheetData = sheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
            if (sheetData is null) continue;

            foreach (var row in sheetData.Elements<Row>())
            {
                var cells = row.Elements<Cell>()
                    .Select(c => GetCellValue(c, workbookPart))
                    .Where(v => !string.IsNullOrWhiteSpace(v));

                texts.Add(string.Join(" ", cells));
            }
        }

        return string.Join(Environment.NewLine, texts);
    }

    private static string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        var value = cell.InnerText;
        if (string.IsNullOrEmpty(value)) return string.Empty;

        if (cell.DataType is not null && cell.DataType.Value == CellValues.SharedString)
        {
            if (int.TryParse(value, out var index))
            {
                var stringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
                if (stringTable is not null && index < stringTable.Count())
                {
                    value = stringTable.ElementAt(index).InnerText;
                }
            }
        }

        return value;
    }

    private static async Task<string> ParseTextAsync(Stream stream, CancellationToken ct)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(ct);
    }

    private static string ParseMarkdown(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var markdown = reader.ReadToEnd();
        return Markdown.ToPlainText(markdown);
    }
}
