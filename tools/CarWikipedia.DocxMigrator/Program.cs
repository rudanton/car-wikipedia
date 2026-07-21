using System.Text;
using VehicleKnowledgeManager.Infrastructure.Docx;

Console.OutputEncoding = Encoding.UTF8;

var options = MigrationOptions.Parse(args);
if (options is null)
{
    PrintUsage();
    return 1;
}

var sourcePaths = ResolveSourcePaths(options.SourcePath);
if (sourcePaths.Count == 0)
{
    Console.Error.WriteLine($"DOCX 파일을 찾을 수 없습니다: {options.SourcePath}");
    return 1;
}

Directory.CreateDirectory(options.OutputDirectory);

var converter = new BasicDocxToMarkdownConverter();
var successCount = 0;
var failCount = 0;

foreach (var sourcePath in sourcePaths)
{
    var result = await converter.ConvertAsync(sourcePath);
    if (!result.IsSuccess || result.Markdown is null)
    {
        failCount++;
        Console.Error.WriteLine($"FAIL {Path.GetFileName(sourcePath)}: {result.ErrorMessage}");
        continue;
    }

    var targets = BuildTargets(sourcePath, result.Markdown, options);
    foreach (var target in targets)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target.OutputPath)!);
        await File.WriteAllTextAsync(target.OutputPath, target.Markdown, Encoding.UTF8);
        Console.WriteLine($"OK   {Path.GetFileName(sourcePath)} -> {Path.GetRelativePath(Environment.CurrentDirectory, target.OutputPath)}");
    }

    successCount++;
}

Console.WriteLine($"완료: 성공 {successCount}개, 실패 {failCount}개");
return failCount == 0 ? 0 : 2;

static List<string> ResolveSourcePaths(string sourcePath)
{
    if (File.Exists(sourcePath))
    {
        return [Path.GetFullPath(sourcePath)];
    }

    if (Directory.Exists(sourcePath))
    {
        return Directory.EnumerateFiles(sourcePath, "*.docx", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    return [];
}

static List<MigrationTarget> BuildTargets(string sourcePath, string markdown, MigrationOptions options)
{
    var fileName = Path.GetFileNameWithoutExtension(sourcePath);
    if (options.SplitSportageSorento && fileName.Contains("스포티지", StringComparison.CurrentCultureIgnoreCase)
                                     && fileName.Contains("쏘렌토", StringComparison.CurrentCultureIgnoreCase))
    {
        return
        [
            new MigrationTarget(Path.Combine(options.OutputDirectory, "스포티지.md"), PrefixSplitNotice(markdown, "스포티지")),
            new MigrationTarget(Path.Combine(options.OutputDirectory, "쏘렌토.md"), PrefixSplitNotice(markdown, "쏘렌토"))
        ];
    }

    return [new MigrationTarget(Path.Combine(options.OutputDirectory, SanitizeFileName(fileName) + ".md"), markdown)];
}

static string PrefixSplitNotice(string markdown, string vehicleName)
{
    return $"# {vehicleName}\n\n> 원본 DOCX에 여러 차량이 포함되어 있어 분리 검수가 필요합니다.\n\n{markdown}";
}

static string SanitizeFileName(string fileName)
{
    foreach (var invalidChar in Path.GetInvalidFileNameChars())
    {
        fileName = fileName.Replace(invalidChar, '_');
    }

    return fileName.Trim();
}

static void PrintUsage()
{
    Console.WriteLine(
        """
        Usage:
          dotnet run --project tools/CarWikipedia.DocxMigrator -- --source <docx-or-folder> --output <folder> [--split-sportage-sorento]

        Examples:
          dotnet run --project tools/CarWikipedia.DocxMigrator -- --source "C:\Users\User\Desktop\차 데이터\gv80.docx" --output ".\vehicles\_converted"
          dotnet run --project tools/CarWikipedia.DocxMigrator -- --source "C:\Users\User\Desktop\차 데이터" --output ".\vehicles\_converted" --split-sportage-sorento
        """);
}

internal sealed record MigrationTarget(string OutputPath, string Markdown);

internal sealed record MigrationOptions(
    string SourcePath,
    string OutputDirectory,
    bool SplitSportageSorento)
{
    public static MigrationOptions? Parse(string[] args)
    {
        string? sourcePath = null;
        string? outputDirectory = null;
        var splitSportageSorento = false;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--source" when index + 1 < args.Length:
                    sourcePath = args[++index];
                    break;
                case "--output" when index + 1 < args.Length:
                    outputDirectory = args[++index];
                    break;
                case "--split-sportage-sorento":
                    splitSportageSorento = true;
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(outputDirectory))
        {
            return null;
        }

        return new MigrationOptions(
            Path.GetFullPath(sourcePath),
            Path.GetFullPath(outputDirectory),
            splitSportageSorento);
    }
}
