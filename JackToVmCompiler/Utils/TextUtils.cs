using System.Text.RegularExpressions;

namespace JackToVmCompiler.Utils
{
    internal static class TextUtils
    {
        private static string ComplexCommentsRegexPattern = @"\/\*\*.*?\*\/|\/\*.*?\*\/";

        public static List<string> ClearTextFromSpacesAndComments(string rawText) =>
            rawText.RemoveComplexComments().SplitTextByRows().RemoveComments();

        public static string RemoveComplexComments(this string rawText)
        {
            return Regex.Replace(rawText, ComplexCommentsRegexPattern, string.Empty, RegexOptions.Singleline);
        }
        public static List<string> RemoveComments(this List<string> lines)
        {
            List<string> cleanLines = new List<string>();

            foreach (var line in lines)
            {
                // Split the line by '//' and take the part before it (index 0)
                string cleanLine = line.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
                cleanLines.Add(cleanLine);
            }


            return cleanLines;
        }

        public static List<string> SplitTextByRows(this string text)
        {
            return text.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !l.StartsWith("//")).ToList();
        }
    }
}
