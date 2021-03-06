﻿using AlphaTab.Model;

namespace AlphaTab.Rendering.Glyphs
{
    public class TabBeatContainerGlyph : BeatContainerGlyph
    {
        public TabBeatContainerGlyph(Beat beat)
            : base(beat)
        {
        }

        protected override void CreateTies(Note n)
        {
            if (n.IsHammerPullOrigin)
            {
                var tie = new TabTieGlyph(n, n.HammerPullDestination, this);
                Ties.Add(tie);
            }
            else if (n.SlideType == SlideType.Legato)
            {
                var tie = new TabTieGlyph(n, n.SlideTarget, this);
                Ties.Add(tie);
            }

            if (n.SlideType != SlideType.None)
            {
                var l = new TabSlideLineGlyph(n.SlideType, n, this);
                Ties.Add(l);
            }
        }
    }
}
