using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan;

public static class TextExtensions
{
    /// <summary>
    /// http://www.codeproject.com/KB/database/Alphanumaric_incriment/Alphanumaric_incriment_src.zip
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Increment(this string str)
    {

        //byte[] asciiValues = ASCIIEncoding.ASCII.GetBytes(str);
        char[] chars = str.ToCharArray();
        int StringLength = chars.Length;
        bool isAllZed = true;
        bool isAllNine = true;
        //Check if all has ZZZ.... then do nothing just return empty string.

        for (int i = 0; i < StringLength - 1; i++)
        {
            if (chars[i] != 90)
            {
                isAllZed = false;
                break;
            }
        }
        if (isAllZed && chars[StringLength - 1] == 57)
        {
            chars[StringLength - 1] = (char)64;
        }

        // Check if all has 999... then make it A0
        for (int i = 0; i < StringLength; i++)
        {
            if (chars[i] != 57)
            {
                isAllNine = false;
                break;
            }
        }
        if (isAllNine)
        {
            chars[StringLength - 1] = (char)47;
            chars[0] = (char)65;
            for (int i = 1; i < StringLength - 1; i++)
            {
                chars[i] = (char)48;
            }
        }


        for (int i = StringLength; i > 0; i--)
        {
            if (i - StringLength == 0)
            {
                chars[i - 1] = (Char)(Convert.ToUInt16(chars[i - 1]) + 1);
            }
            if (chars[i - 1] == (char)58)
            {
                chars[i - 1] = (char)48;
                if (i - 2 == -1)
                {
                    break;
                }
                chars[i - 2] = (Char)(Convert.ToUInt16(chars[i - 2]) + 1);
            }
            else if (chars[i - 1] == 91)
            {
                chars[i - 1] = (char)65;
                if (i - 2 == -1)
                {
                    break;
                }
                chars[i - 2] = (Char)(Convert.ToUInt16(chars[i - 2]) + 1);

            }
            else
            {
                break;
            }

        }
        str = new String(chars); // ASCIIEncoding.ASCII.GetString(asciiValues);
        return str;

    }


    public static char ToInvariant(this char ch)
    {
        if (ch < 128) // karakter ascii içeriğe sahiptir
            return ch;

        switch (ch)
        {
            case (char)8216: ch = '\''; break; //'‘'
            case (char)8217: ch = '\''; break; //'’'
            case (char)8220: ch = '"'; break;  //'“'
            case (char)8221: ch = '"'; break;  //'”'
            case (char)8223: ch = '"'; break;  //'‟'
            case (char)8211: ch = '-'; break;  //'–'
            case 'ı': ch = 'i'; break;

        }

        string b = string.Join("", ch.ToString().Normalize(NormalizationForm.FormD).Where(k => char.GetUnicodeCategory(k) != System.Globalization.UnicodeCategory.NonSpacingMark));
        if (b == string.Empty)
            return '_';

        ch = Convert.ToChar(b);
        return ch;
    }

    public static string ToInvariant(this string text)
    {
        if (text == null)
            text = string.Empty;

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            ch = ch.ToInvariant();
            builder.Append(ch);
        }
        return builder.ToString();

    }

    public static bool IsNullOrEmpty(this string text)
    {
        return string.IsNullOrEmpty(text);
    }

    public static string ToFriendly(this string text, params char[] include )
    {
        if (text.IsNullOrEmpty())
            return text;

        text = text.ToInvariant();

        string ftext = string.Empty;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                ftext += c;
            else
            {
                if (include.Contains(c))
                    ftext += c;
            }
        }
        return ftext;
    }

}