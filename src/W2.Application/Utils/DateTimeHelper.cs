using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using W2.WorkflowDefinitions;

namespace W2.Utils
{
    public static class DateTimeHelper
    {

        public static bool IsDateType(string type)
        {
            return Enum.TryParse<WorkflowInputDefinitionProperyType>(
                type, true, out var parsed)
                && (parsed == WorkflowInputDefinitionProperyType.DateTime
                    || parsed == WorkflowInputDefinitionProperyType.MultiDatetime);
        }

        public static List<DateTime> ParseDateValues(string rawValue)
        {
            var results = new List<DateTime>();
            var text = rawValue.Trim();

            if (text.StartsWith("["))
            {
                results.AddRange(ParseJsonArray(text));
                return results;
            }

            foreach (var chunk in text.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var value = chunk.Trim().Trim('"');

                if (TryParseDate(value, out var parsed))
                {
                    results.Add(parsed.Date);
                }
            }

            return results;
        }

        public static IEnumerable<DateTime> ParseJsonArray(string json)
        {
            List<string> values;

            try
            {
                values = JsonSerializer.Deserialize<List<string>>(json);
            }
            catch
            {
                yield break;
            }

            if (values == null)
            {
                yield break;
            }

            foreach (var item in values)
            {
                if (TryParseDate(item, out var parsed))
                {
                    yield return parsed.Date;
                }
            }
        }

        public static bool TryParseDate(string value, out DateTime parsedDate)
        {
            if (DateTime.TryParseExact(
                    value,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsedDate))
            {
                return true;
            }

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal,
                out parsedDate);
        }
    }
}
