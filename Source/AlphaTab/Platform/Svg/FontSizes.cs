﻿using System;
using AlphaTab.IO;

namespace AlphaTab.Platform.Svg
{
    /// <summary>
    /// This public class stores text widths for several fonts and allows width calculation 
    /// </summary>
    public class FontSizes
    {
        // NOTE: use tools/FontMeasureMent.html to generate those arrays
        // TODO: probably use a regression function for this
        public static ByteArray TimesNewRoman = new ByteArray(3, 4, 5, 6, 6, 9, 9, 2, 4, 4, 6, 6, 3, 4, 3, 3, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 3, 3, 6, 6, 6, 5, 10, 8, 7, 7, 8, 7, 6, 7, 8, 4, 4, 8, 7, 10, 8, 8, 7, 8, 7, 5, 8, 8, 7, 11, 8, 8, 7, 4, 3, 4, 5, 6, 4, 5, 5, 5, 5, 5, 4, 5, 6, 3, 3, 6, 3, 9, 6, 6, 6, 5, 4, 4, 4, 5, 6, 7, 6, 6, 5, 5, 2, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 4, 6, 6, 6, 6, 2, 5, 4, 8, 4, 6, 6, 0, 8, 6, 4, 6, 3, 3, 4, 5, 5, 4, 4, 3, 3, 6, 8, 8, 8, 5, 8, 8, 8, 8, 8, 8, 11, 7, 7, 7, 7, 7, 4, 4, 4, 4, 8, 8, 8, 8, 8, 8, 8, 6, 8, 8, 8, 8, 8, 8, 6, 5, 5, 5, 5, 5, 5, 5, 8, 5, 5, 5, 5, 5, 3, 3, 3, 3, 6, 6, 6, 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 6, 6);
        public static ByteArray Arial11Pt = new ByteArray(3, 2, 4, 6, 6, 10, 7, 2, 4, 4, 4, 6, 3, 4, 3, 3, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 3, 3, 6, 6, 6, 6, 11, 8, 7, 7, 7, 6, 6, 8, 7, 2, 5, 7, 6, 8, 7, 8, 6, 8, 7, 7, 6, 7, 8, 10, 7, 8, 7, 3, 3, 3, 5, 6, 4, 6, 6, 6, 6, 6, 4, 6, 6, 2, 2, 5, 2, 8, 6, 6, 6, 6, 4, 6, 3, 6, 6, 10, 6, 6, 6, 4, 2, 4, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 2, 6, 6, 7, 6, 2, 6, 4, 8, 4, 6, 6, 0, 8, 6, 4, 6, 4, 4, 4, 6, 6, 4, 4, 4, 5, 6, 9, 10, 10, 6, 8, 8, 8, 8, 8, 8, 11, 7, 6, 6, 6, 6, 2, 2, 2, 2, 8, 7, 8, 8, 8, 8, 8, 6, 8, 7, 7, 7, 7, 8, 7, 7, 6, 6, 6, 6, 6, 6, 10, 6, 6, 6, 6, 6, 2, 2, 2, 2, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6);

        public const int ControlChars = 0x20;


        public static float MeasureString(string s, SupportedFonts f, float size)
        {
            ByteArray data;
            int dataSize;

            if (f == SupportedFonts.TimesNewRoman)
            {
                data = TimesNewRoman;
                dataSize = 11;
            }
            else if (f == SupportedFonts.Arial)
            {
                data = Arial11Pt;
                dataSize = 11;
            }
            else
            {
                data = new ByteArray((byte)8);
                dataSize = 11;
            }

            var stringSize = 0;

            for (int i = 0; i < s.Length; i++)
            {
                var code = Math.Min(data.Length - 1, s[i] - ControlChars);
                if (code >= 0)
                {
                    stringSize += (int)((data[code] * size) / dataSize);
                }
            }

            return stringSize;
        }
    }
}
