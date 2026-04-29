using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Lofn.Domain.Core
{
    /// <summary>
    /// Pure in-process slug generator. Removes diacritics (accent-folding via
    /// Unicode NFD), lower-cases, replaces every run of non-alphanumeric
    /// characters with a single hyphen, and trims leading/trailing hyphens.
    /// </summary>
    public class SlugGenerator : ISlugGenerator
    {
        private static readonly Regex NonAlphaNum = new(@"[^a-z0-9]+", RegexOptions.Compiled);

        public string Generate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            var stripped = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var hyphenated = NonAlphaNum.Replace(stripped, "-");
            return hyphenated.Trim('-');
        }
    }
}
