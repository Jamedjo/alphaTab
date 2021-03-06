﻿using System;
using AlphaTab.Model;
using AlphaTab.Platform;
using AlphaTab.Platform.Model;

namespace AlphaTab.Rendering.Glyphs
{
    public class WhammyBarGlyph : Glyph
    {
        private const int WhammyMaxOffset = 60;

        private readonly Beat _beat;
        private readonly BeatContainerGlyph _parent;

        public WhammyBarGlyph(Beat beat, BeatContainerGlyph parent)
            : base(0, 0)
        {
            _beat = beat;
            _parent = parent;
        }

        public override void DoLayout()
        {
            base.DoLayout();

            // 
            // Calculate the min and max offsets
            var minY = 0;
            var maxY = 0;

            var sizeY = (int)(WhammyMaxOffset * Scale);
            if (_beat.WhammyBarPoints.Count >= 2)
            {
                var dy = sizeY / Beat.WhammyBarMaxValue;
                for (int i = 0, j = _beat.WhammyBarPoints.Count; i < j; i++)
                {
                    var pt = _beat.WhammyBarPoints[i];
                    var ptY = 0 - (dy * pt.Value);
                    if (ptY > maxY) maxY = ptY;
                    if (ptY < minY) minY = ptY;
                }
            }

            //
            // calculate the overflow 
            TabBarRenderer tabBarRenderer = (TabBarRenderer)Renderer;
            var track = Renderer.Bar.Track;
            var tabTop = tabBarRenderer.GetTabY(0, -2);
            var tabBottom = tabBarRenderer.GetTabY(track.Tuning.Count, -2);

            var absMinY = Y + minY + tabTop;
            var absMaxY = Y + maxY - tabBottom;

            if (absMinY < 0)
                tabBarRenderer.RegisterOverflowTop(Math.Abs(absMinY));
            if (absMaxY > 0)
                tabBarRenderer.RegisterOverflowBottom(Math.Abs(absMaxY));
        }

        public override void Paint(int cx, int cy, ICanvas canvas)
        {
            TabBarRenderer tabBarRenderer = (TabBarRenderer)Renderer;
            var res = Renderer.Resources;
            var startX = cx + X + _parent.OnNotes.Width / 2;
            var endX = _beat.NextBeat == null || _beat.Voice != _beat.NextBeat.Voice
                    ? cx + X + _parent.OnNotes.Width / 2 + _parent.PostNotes.Width
                    : cx + tabBarRenderer.GetBeatX(_beat.NextBeat);
            var startY = cy + X;
            var textOffset = (int)(3 * Scale);

            var sizeY = (int)(WhammyMaxOffset * Scale);

            var old = canvas.TextAlign;
            canvas.TextAlign = TextAlign.Center;
            if (_beat.WhammyBarPoints.Count >= 2)
            {
                var dx = (endX - startX) / Beat.WhammyBarMaxPosition;
                var dy = sizeY / Beat.WhammyBarMaxValue;

                canvas.BeginPath();
                for (int i = 0, j = _beat.WhammyBarPoints.Count - 1; i < j; i++)
                {
                    var pt1 = _beat.WhammyBarPoints[i];
                    var pt2 = _beat.WhammyBarPoints[i + 1];

                    if (pt1.Value == pt2.Value && i == _beat.WhammyBarPoints.Count - 2) continue;

                    var pt1X = startX + (dx * pt1.Offset);
                    var pt1Y = startY - (dy * pt1.Value);

                    var pt2X = startX + (dx * pt2.Offset);
                    var pt2Y = startY - (dy * pt2.Value);

                    canvas.MoveTo(pt1X, pt1Y);
                    canvas.LineTo(pt2X, pt2Y);

                    if (pt2.Value != 0)
                    {
                        var dv = pt2.Value / 4.0;
                        var up = (pt2.Value - pt1.Value) >= 0;
                        var s = "";
                        if (dv < 0) s += "-";

                        if (dv >= 1 || dv <= -1)
                            s += ((int)(Math.Abs(dv))) + " ";

                        dv -= (int)dv;
                        if (dv == 0.25)
                            s += "1/4";
                        else if (dv == 0.5)
                            s += "1/2";
                        else if (dv == 0.75)
                            s += "3/4";

                        canvas.Font = res.GraceFont;
                        //var size = canvas.MeasureText(s);
                        var sy = up
                                    ? pt2Y - res.GraceFont.Size - textOffset
                                    : pt2Y + textOffset;
                        var sx = pt2X;
                        canvas.FillText(s, sx, sy);

                    }
                }
                canvas.Stroke();
            }
            canvas.TextAlign = old;
        }
    }
}
