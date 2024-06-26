using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace W2.Utils
{
    public class TitleTemplateParser
    {
        public static string ParseTitleTemplateToString(string titleTemplate, Dictionary<string, string> input)
        {
            // Check if titleTemplate is null or empty
            if (string.IsNullOrEmpty(titleTemplate))
            {
                return string.Empty;
            }

            // Define a regex pattern to match the placeholders in the format {{variable}}
            string pattern = @"\{\{(\w+)\}\}";

            // Use a MatchEvaluator to replace the placeholders with values from the dictionary
            string result = Regex.Replace(titleTemplate, pattern, match =>
            {
                string key = match.Groups[1].Value;
                // Return the value from the dictionary if it exists, otherwise return an empty string
                return input.ContainsKey(key) ? input[key] : string.Empty;
            });

            return result;
        }
    }
}
