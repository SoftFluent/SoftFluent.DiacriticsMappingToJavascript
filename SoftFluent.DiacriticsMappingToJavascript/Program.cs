using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftFluent.DiacriticsMappingToJavascript
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = GetMap(0, 0x10FFFF)
                .Where(m => !m.IsCanonic())
                .ToList();

            var dico = list
                .ToDictionary(m => m.UnicodeString, m => m.CanonicString);

            using (StreamWriter sw = File.CreateText("NormalizationFormKDMap.js"))
            {
                sw.Write("var NormalizationFormKDMap = ");
                CodeFluent.Runtime.Utilities.JsonUtilities.Serialize(sw, dico);
                sw.WriteLine(";");
            }
        }

        private static IEnumerable<Map> GetMap(int minCodePoint, int maxCodePoint)
        {
            if(minCodePoint < 0 || maxCodePoint > 0x10FFFF)
                throw new ArgumentOutOfRangeException();

            for (int i = minCodePoint; i <= maxCodePoint; i++)
            {
                if(i >= 0x00d800 && i <= 0x00DFFF)
                    continue;

                string unicodeString = char.ConvertFromUtf32(i);

                string result;
                try
                {
                    if(IsInvalidCharacter(unicodeString))
                        continue;

                    result = RemoveDiacritics(unicodeString);
                    if(result.Length == 0)
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                yield return new Map { UnicodeString = unicodeString, CanonicString = result };
            }
        }

        private static bool IsInvalidCharacter(string unicodeString)
        {
            if (unicodeString.Length == 0) return false;
            
            return unicodeString.All(IsInvalidCharacter);
        }

        private static bool IsInvalidCharacter(char ch)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            return unicodeCategory == UnicodeCategory.OtherNotAssigned || unicodeCategory == UnicodeCategory.Surrogate;
        }

        struct Map
        {
            public string UnicodeString { get; set; }
            public string CanonicString { get; set; }
            public bool IsCanonic() => string.Equals(UnicodeString, CanonicString, StringComparison.Ordinal);
        }

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormKD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
