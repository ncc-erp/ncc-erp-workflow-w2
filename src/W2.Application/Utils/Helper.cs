using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace W2.Utils
{
    public static class Helper
    {

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalizedText = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalizedText)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) // Loại bỏ dấu
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ConvertVietnameseToUnsign(string text)
        {
            string result = RemoveDiacritics(text);
            result = Regex.Replace(result, @"Đ", "D"); // Chuyển Đ thành D
            result = Regex.Replace(result, @"đ", "d"); // Chuyển đ thành d
            return result;
        }
    }
}
