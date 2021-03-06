﻿using AlphaTab.Collections;

namespace AlphaTab.Rendering.Glyphs
{
    public class AccidentalGroupGlyph : GlyphGroup
    {
        private const int NonReserved = -3000;

        public AccidentalGroupGlyph()
            : base(0, 0)
        {

        }

        public override void DoLayout()
        {
            if (Glyphs == null)
            {
                Width = 0;
                return;
            }
            //
            // Determine Columns for accidentals
            //
            Glyphs.Sort((a, b) => a.Y.CompareTo(b.Y));

            // defines the reserved y position of the columns
            var columns = new FastList<int>();
            columns.Add(NonReserved);

            var accidentalSize = (int)(21 * Scale);
            for (int i = 0, j = Glyphs.Count; i < j; i++)
            {
                var g = Glyphs[i];
                g.Renderer = Renderer;
                g.DoLayout();

                // find column where glyph fits into

                // as long the glyph does not fit into the current column
                var gColumn = 0;
                while (columns[gColumn] > g.Y)
                {
                    // move to next column
                    gColumn++;

                    // and create the new column if needed
                    if (gColumn == columns.Count)
                    {
                        columns.Add(NonReserved);
                    }
                }

                // temporary save column as X
                g.X = gColumn;
                columns[gColumn] = g.Y + accidentalSize;
            }

            //
            // Place accidentals in columns
            //
            var columnWidth = (int)(8 * Scale);
            if (Glyphs.Count == 0)
            {
                Width = 0;
            }
            else
            {
                Width = columnWidth * columns.Count;
            }

            for (int i = 0, j = Glyphs.Count; i < j; i++)
            {
                var g = Glyphs[i];
                g.X = Width - ((g.X + 1) * columnWidth);
            }
        }
    }
}
