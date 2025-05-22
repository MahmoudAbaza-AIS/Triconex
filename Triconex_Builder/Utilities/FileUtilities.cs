using System.Text.RegularExpressions;

namespace TriconexMigration.Application.Utilities;

public static class FileUtilities
{
    private static int _currentFC;

    public static string GetInputs(string expression)
    {
        expression = Regex.Replace(expression, @"RS_\w+|SR_\w+|RS|SR|TP|TON|TOF|\bT#\w*", "");

        expression = expression.Replace("AND", "")
                               .Replace("OR", "")
                               .Replace("Exclusive", "")
                               .Replace("(", "")
                               .Replace(")", "")
                               .Replace(",", "/");

        expression = Regex.Replace(expression, @"\s*/\s*", "/").Trim('/').Trim();

        var parts = expression.Contains("=")
                  ? expression.Split(['='], StringSplitOptions.RemoveEmptyEntries)
                  : expression.Split(["NOP"], StringSplitOptions.RemoveEmptyEntries);
        expression = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        return expression;
    }

    public static string SaveFileInDocuments(string folderName, string fileName)
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string folderPath = Path.Combine(documentsPath, folderName);

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        return Path.Combine(folderPath, fileName);
    }

    #region Extract Reference
    public static string FindFCForMemoryAddress(string filePath, string rawMemoryAddress)
    {
        var parsedMemoryAddress = ParseMemoryAddress(rawMemoryAddress)?
            .Select(m => m.Replace("_", "."))
            .ToList();

        if (!IsOddMemoryAddress(parsedMemoryAddress)) return $"FC{_currentFC}";

        var lines = File.ReadAllLines(filePath);
        int currentFC = -1;

        foreach (var line in lines)
        {
            if (line.StartsWith("FUNCTION FC"))
                currentFC = ExtractFunctionFC(line) ?? currentFC;

            if (IsMatchingLine(line, parsedMemoryAddress))
            {
                _currentFC = Math.Max(currentFC, _currentFC);
                return $"FC{_currentFC}";
            }
        }
        return $"FC{_currentFC}";
    }
    public static void InitializeFC() => _currentFC = 0;

    private static bool IsOddMemoryAddress(List<string>? parsedMemoryAddress) =>
               parsedMemoryAddress != null &&
               !string.IsNullOrWhiteSpace(parsedMemoryAddress[0]) &&
               !(parsedMemoryAddress[0] == "M" && string.IsNullOrWhiteSpace(parsedMemoryAddress[1])) &&
               parsedMemoryAddress[0] != ":";

    private static int? ExtractFunctionFC(string line)
    {
        var parts = line.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 2 && parts[1] == "FC" && int.TryParse(parts[2], out var fcNumber)) return fcNumber;
        return null;
    }

    private static bool IsMatchingLine(string line, List<string> parsedMemoryAddress) =>
               (line.Contains("=") ||
                line.Contains("T") ||
                line.Contains("SD") ||
                line.Contains("SP") ||
                line.Contains("S") ||
                line.Contains("R")) &&
               !line.Contains("OUT") &&
               !line.Contains("STAT") &&
               line.Contains(parsedMemoryAddress[0]) &&
               Regex.Match(line, @"\b\d+(\.\d+)?\b").Value == parsedMemoryAddress[1];

    private static List<string>? ParseMemoryAddress(string rawMemoryAddress)
    {
        if (string.IsNullOrWhiteSpace(rawMemoryAddress)) return null;

        if (rawMemoryAddress.Contains("DB"))
        {
            var match = Regex.Match(rawMemoryAddress, @"^(.*?)(\d+(_\d+)?)$");
            if (match.Success) return [match.Groups[1].Value, match.Groups[2].Value];
            return [rawMemoryAddress, string.Empty];
        }

        char type = rawMemoryAddress[0];
        string numberPart = rawMemoryAddress[1..].Replace('_', '.');

        return [type.ToString(), numberPart];
    }
    #endregion
}
