using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using VehicleKnowledgeManager.Core.Interfaces;
using VehicleKnowledgeManager.Core.Models;

namespace VehicleKnowledgeManager.Infrastructure.Docx;

public sealed class BasicDocxToMarkdownConverter : IDocxToMarkdownConverter
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    public Task<ConversionResult> ConvertAsync(
        string sourcePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(sourcePath))
        {
            return Task.FromResult(Fail(sourcePath, $"DOCX 파일을 찾을 수 없습니다: {sourcePath}"));
        }

        if (!string.Equals(Path.GetExtension(sourcePath), ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Fail(sourcePath, "DOCX 파일만 변환할 수 있습니다."));
        }

        try
        {
            using var archive = ZipFile.OpenRead(sourcePath);
            var documentEntry = archive.GetEntry("word/document.xml");
            if (documentEntry is null)
            {
                return Task.FromResult(Fail(sourcePath, "DOCX 본문 XML을 찾을 수 없습니다."));
            }

            using var stream = documentEntry.Open();
            var document = XDocument.Load(stream);
            var body = document.Root?.Element(W + "body");
            if (body is null)
            {
                return Task.FromResult(Fail(sourcePath, "DOCX 본문을 읽을 수 없습니다."));
            }

            var builder = new StringBuilder();
            foreach (var element in body.Elements())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (element.Name == W + "p")
                {
                    AppendParagraph(builder, element);
                }
                else if (element.Name == W + "tbl")
                {
                    AppendTable(builder, element);
                }
            }

            var markdown = CarWikipediaDocxCleanup.Clean(builder.ToString());
            return Task.FromResult(new ConversionResult
            {
                SourcePath = sourcePath,
                IsSuccess = true,
                Markdown = markdown
            });
        }
        catch (InvalidDataException exception)
        {
            return Task.FromResult(Fail(sourcePath, $"DOCX 압축 구조를 읽을 수 없습니다: {exception.Message}"));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            return Task.FromResult(Fail(sourcePath, $"DOCX 변환 중 오류가 발생했습니다: {exception.Message}"));
        }
    }

    private static void AppendParagraph(StringBuilder builder, XElement paragraph)
    {
        var text = GetParagraphText(paragraph).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            AppendBlankLine(builder);
            return;
        }

        var styleId = GetStyleId(paragraph);
        var headingLevel = GetHeadingLevel(styleId);
        if (headingLevel > 0)
        {
            AppendBlock(builder, $"{new string('#', headingLevel)} {text}");
            return;
        }

        if (IsListParagraph(paragraph) || LooksLikeBullet(text))
        {
            AppendBlock(builder, $"- {NormalizeBulletPrefix(text)}");
            return;
        }

        AppendBlock(builder, text);
    }

    private static void AppendTable(StringBuilder builder, XElement table)
    {
        var rows = table.Elements(W + "tr")
            .Select(row => row.Elements(W + "tc")
                .Select(cell => string.Join(" ", cell.Descendants(W + "t").Select(text => text.Value)).Trim())
                .ToList())
            .Where(row => row.Count > 0)
            .ToList();

        if (rows.Count == 0)
        {
            return;
        }

        var columnCount = rows.Max(row => row.Count);
        foreach (var row in rows)
        {
            while (row.Count < columnCount)
            {
                row.Add(string.Empty);
            }
        }

        AppendBlock(builder, "| " + string.Join(" | ", rows[0].Select(EscapeTableCell)) + " |");
        AppendBlock(builder, "| " + string.Join(" | ", Enumerable.Repeat("---", columnCount)) + " |");

        foreach (var row in rows.Skip(1))
        {
            AppendBlock(builder, "| " + string.Join(" | ", row.Select(EscapeTableCell)) + " |");
        }
    }

    private static string GetParagraphText(XElement paragraph)
    {
        var builder = new StringBuilder();
        foreach (var run in paragraph.Elements(W + "r"))
        {
            foreach (var child in run.Elements())
            {
                if (child.Name == W + "t")
                {
                    builder.Append(child.Value);
                }
                else if (child.Name == W + "tab")
                {
                    builder.Append(' ');
                }
                else if (child.Name == W + "br")
                {
                    builder.AppendLine();
                }
            }
        }

        return builder.ToString();
    }

    private static string? GetStyleId(XElement paragraph)
    {
        return paragraph.Element(W + "pPr")
            ?.Element(W + "pStyle")
            ?.Attribute(W + "val")
            ?.Value;
    }

    private static int GetHeadingLevel(string? styleId)
    {
        if (string.IsNullOrWhiteSpace(styleId))
        {
            return 0;
        }

        var match = Regex.Match(styleId, @"Heading(?<level>[1-6])|^H(?<level>[1-6])$", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups["level"].Value, out var level))
        {
            return level;
        }

        return 0;
    }

    private static bool IsListParagraph(XElement paragraph)
    {
        return paragraph.Element(W + "pPr")?.Element(W + "numPr") is not null;
    }

    private static bool LooksLikeBullet(string text)
    {
        return Regex.IsMatch(text, @"^\s*(?:[-*•·●○▶▷▪▫→⇒])\s+");
    }

    private static string NormalizeBulletPrefix(string text)
    {
        return Regex.Replace(text.Trim(), @"^(?:[-*•·●○▶▷▪▫→⇒])\s*", string.Empty).Trim();
    }

    private static string EscapeTableCell(string value)
    {
        return value.Replace("|", "\\|").ReplaceLineEndings(" ").Trim();
    }

    private static void AppendBlock(StringBuilder builder, string value)
    {
        builder.AppendLine(value.TrimEnd());
        builder.AppendLine();
    }

    private static void AppendBlankLine(StringBuilder builder)
    {
        if (builder.Length > 0 && !builder.ToString().EndsWith($"{Environment.NewLine}{Environment.NewLine}", StringComparison.Ordinal))
        {
            builder.AppendLine();
        }
    }

    private static ConversionResult Fail(string sourcePath, string errorMessage)
    {
        return new ConversionResult
        {
            SourcePath = sourcePath,
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
