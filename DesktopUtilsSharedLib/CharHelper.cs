using System.Globalization;

namespace DesktopUtilsSharedLib;

public static partial class CharHelper
{
    public static string NormalizeMono(this string input, char replacement = '_')
    {
        char[] chars = input.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsControl(chars[i]) ||
                char.IsSurrogate(chars[i]) ||
                char.GetUnicodeCategory(chars[i]) is UnicodeCategory.OtherSymbol or UnicodeCategory.OtherLetter)
            {
                chars[i] = replacement;
            }
        }

        return new string(chars);
    }
}