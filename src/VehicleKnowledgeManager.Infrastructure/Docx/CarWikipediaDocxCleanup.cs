using System.Text.RegularExpressions;

namespace VehicleKnowledgeManager.Infrastructure.Docx;

public static class CarWikipediaDocxCleanup
{
    public static string Clean(string markdown)
    {
        var text = markdown.ReplaceLineEndings("\n");

        text = Regex.Replace(text, @"[ \t]+$", string.Empty, RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*(?:|→|⇒|▶|▷|•|·|●|○|▪|▫)\s*", "- ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*-\s*-\s*", "- ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        return text.Trim() + "\n";
    }
}
