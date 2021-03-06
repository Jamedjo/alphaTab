﻿using System;
using System.Runtime.CompilerServices;
using AlphaTab.Collections;
using AlphaTab.Model;
using AlphaTab.Platform;
using AlphaTab.Rendering.Utils;

namespace AlphaTab.Rendering.Glyphs
{
    public class ScoreNoteGlyphInfo
    {
        [IntrinsicProperty]
        public Glyph Glyph { get; set; }
        [IntrinsicProperty]
        public int Line { get; set; }

        public ScoreNoteGlyphInfo(Glyph glyph, int line)
        {
            Glyph = glyph;
            Line = line;
        }
    }

    public class ScoreNoteChordGlyph : Glyph
    {
        private readonly FastList<ScoreNoteGlyphInfo> _infos;
        private readonly FastDictionary<int, Glyph> _noteLookup;
        private Glyph _tremoloPicking;

        [IntrinsicProperty]
        public ScoreNoteGlyphInfo MinNote { get; set; }
        [IntrinsicProperty]
        public ScoreNoteGlyphInfo MaxNote { get; set; }

        [IntrinsicProperty]
        public Action SpacingChanged { get; set; }
        [IntrinsicProperty]
        public int UpLineX { get; set; }
        [IntrinsicProperty]
        public int DownLineX { get; set; }

        [IntrinsicProperty]
        public FastDictionary<string, Glyph> BeatEffects { get; set; }

        [IntrinsicProperty]
        public Beat Beat { get; set; }
        [IntrinsicProperty]
        public BeamingHelper BeamingHelper { get; set; }

        public ScoreNoteChordGlyph()
            : base(0, 0)
        {
            _infos = new FastList<ScoreNoteGlyphInfo>();
            BeatEffects = new FastDictionary<string, Glyph>();
            _noteLookup = new FastDictionary<int, Glyph>();
        }

        public BeamDirection Direction
        {
            get
            {
                return BeamingHelper.Direction;
            }
        }

        public int GetNoteX(Note note, bool onEnd = true)
        {
            if (_noteLookup.ContainsKey(note.String))
            {
                var n = _noteLookup[note.String];
                var pos = X + n.X;
                if (onEnd)
                {
                    pos += n.Width;
                }
                return pos;
            }
            return 0;
        }

        public int GetNoteY(Note note)
        {
            if (_noteLookup.ContainsKey(note.String))
            {
                return Y + _noteLookup[note.String].Y;
            }
            return 0;
        }

        public void AddNoteGlyph(Glyph noteGlyph, Note note, int noteLine)
        {
            var info = new ScoreNoteGlyphInfo(noteGlyph, noteLine);
            _infos.Add(info);
            _noteLookup[note.String] = noteGlyph;
            if (MinNote == null || MinNote.Line > info.Line)
            {
                MinNote = info;
            }
            if (MaxNote == null || MaxNote.Line < info.Line)
            {
                MaxNote = info;
            }
        }

        public override bool CanScale
        {
            get { return false; }
        }

        public void UpdateBeamingHelper(int cx)
        {
            BeamingHelper.RegisterBeatLineX(Beat, cx + X + UpLineX, cx + X + DownLineX);
        }

        public bool HasTopOverflow
        {
            get
            {
                return MinNote != null && MinNote.Line < 0;
            }
        }

        public bool HasBottomOverflow
        {
            get
            {
                return MaxNote != null && MaxNote.Line > 8;
            }
        }

        public override void DoLayout()
        {
            _infos.Sort((a, b) => a.Line.CompareTo(b.Line));

            const int padding = 0; // Std.int(4 * getScale());

            var displacedX = 0;

            var lastDisplaced = false;
            var lastLine = 0;
            var anyDisplaced = false;

            var w = 0;
            for (int i = 0, j = _infos.Count; i < j; i++)
            {
                var g = _infos[i].Glyph;
                g.Renderer = Renderer;
                g.DoLayout();

                g.X = padding;

                if (i == 0)
                {
                    displacedX = g.Width + padding;
                }
                else
                {
                    // check if note needs to be repositioned
                    if (Math.Abs(lastLine - _infos[i].Line) <= 1)
                    {
                        // reposition if needed
                        if (!lastDisplaced)
                        {
                            g.X = (int)(displacedX - (Scale));
                            anyDisplaced = true;
                            lastDisplaced = true; // let next iteration know we are displace now
                        }
                        else
                        {
                            lastDisplaced = false;  // let next iteration know that we weren't displaced now
                        }
                    }
                    else // offset is big enough? no displacing needed
                    {
                        lastDisplaced = false;
                    }
                }

                lastLine = _infos[i].Line;
                w = Math.Max(w, g.X + g.Width);
            }

            if (anyDisplaced)
            {
                UpLineX = displacedX;
                DownLineX = displacedX;
            }
            else
            {
                UpLineX = w;
                DownLineX = padding;
            }

            Std.Foreach(BeatEffects.Values, e =>
            {
                e.Renderer = Renderer;
                e.DoLayout();
            });

            if (Beat.IsTremolo)
            {
                var direction = BeamingHelper.Direction;
                int offset;
                var baseNote = direction == BeamDirection.Up ? MinNote : MaxNote;
                var tremoloX = direction == BeamDirection.Up ? displacedX : 0;
                if (Beat.TremoloSpeed != null)
                {
                    var speed = Beat.TremoloSpeed.Value;
                    switch (speed)
                    {
                        case Duration.ThirtySecond:
                            offset = direction == BeamDirection.Up ? -15 : 10;
                            break;
                        case Duration.Sixteenth:
                            offset = direction == BeamDirection.Up ? -12 : 10;
                            break;
                        case Duration.Eighth:
                            offset = direction == BeamDirection.Up ? -10 : 10;
                            break;
                        default: offset = direction == BeamDirection.Up ? -15 : 15;
                            break;
                    }
                }
                else
                {
                    offset = direction == BeamDirection.Up ? -15 : 15;
                }

                _tremoloPicking = new TremoloPickingGlyph(tremoloX, baseNote.Glyph.Y + (int)(offset * Scale), Beat.TremoloSpeed.Value);
                _tremoloPicking.Renderer = Renderer;
                _tremoloPicking.DoLayout();
            }

            Width = w + padding;
        }

        public override void Paint(int cx, int cy, ICanvas canvas)
        {
            var scoreRenderer = (ScoreBarRenderer)Renderer;

            //
            // Note Effects only painted once
            //
            var effectY = BeamingHelper.Direction == BeamDirection.Up
                            ? scoreRenderer.GetScoreY(MaxNote.Line, (int)(1.5f * NoteHeadGlyph.NoteHeadHeight))
                            : scoreRenderer.GetScoreY(MinNote.Line, (int)(-1.0f * NoteHeadGlyph.NoteHeadHeight));
            // TODO: take care of actual glyph height
            var effectSpacing = (BeamingHelper.Direction == BeamDirection.Up)
                            ? (int)(7 * Scale)
                            : (int)(-7 * Scale);

            Std.Foreach(BeatEffects.Values, g =>
            {
                g.Y = effectY;
                g.X = Width / 2;
                g.Paint(cx + X, cy + Y, canvas);
                effectY += effectSpacing;
            });

            canvas.Color = Renderer.Layout.Renderer.RenderingResources.StaveLineColor;

            // TODO: Take care of beateffects in overflow

            var linePadding = (int)(3 * Scale);
            if (HasTopOverflow)
            {
                var l = -1;
                while (l >= MinNote.Line)
                {
                    // + 1 Because we want to place the line in the center of the note, not at the top
                    var lY = cy + Y + scoreRenderer.GetScoreY(l + 1, -1);
                    canvas.BeginPath();
                    canvas.MoveTo(cx + X - linePadding, lY);
                    canvas.LineTo(cx + X + Width + linePadding, lY);
                    canvas.Stroke();
                    l -= 2;
                }
            }

            if (HasBottomOverflow)
            {
                var l = 11;
                while (l <= MaxNote.Line)
                {
                    var lY = cy + Y + scoreRenderer.GetScoreY(l + 1, -1);
                    canvas.BeginPath();
                    canvas.MoveTo(cx + X - linePadding, lY);
                    canvas.LineTo(cx + X + Width + linePadding, lY);
                    canvas.Stroke();
                    l += 2;
                }
            }

            canvas.Color = Renderer.Layout.Renderer.RenderingResources.MainGlyphColor;

            if (_tremoloPicking != null)
                _tremoloPicking.Paint(cx + X, cy + Y, canvas);
            for (int i = 0, j = _infos.Count; i < j; i++)
            {
                var g = _infos[i];
                g.Glyph.Renderer = Renderer;
                g.Glyph.Paint(cx + X, cy + Y, canvas);
            }
        }
    }
}
