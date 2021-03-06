﻿namespace AlphaTab.Rendering.Glyphs
{
    public class TimeSignatureGlyph : GlyphGroup
    {
        private readonly int _numerator;
        private readonly int _denominator;

        public TimeSignatureGlyph(int x, int y, int numerator, int denominator)
            : base(x, y)
        {
            _numerator = numerator;
            _denominator = denominator;
        }

        public override bool CanScale
        {
            get { return false; }
        }

        public override void DoLayout()
        {
            var numerator = new NumberGlyph(0, 0, _numerator);
            var denominator = new NumberGlyph(0, (int)(18 * Scale), _denominator);

            AddGlyph(numerator);
            AddGlyph(denominator);

            base.DoLayout();

            for (int i = 0, j = Glyphs.Count; i < j; i++)
            {
                var g = Glyphs[i];
                g.X = (Width - g.Width) / 2;
            }
        }
    }
}
