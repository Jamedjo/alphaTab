﻿using AlphaTab.Platform;

namespace AlphaTab.Rendering.Glyphs
{
    public class SvgGlyph : EffectGlyph
    {
        private readonly LazySvg _svg;
        private string _lastCmd;

        private float _currentX;
        private float _currentY;
        private float _xScale;
        private float _yScale;

        private readonly float _xGlyphScale;
        private readonly float _yGlyphScale;

        private float _lastControlX;
        private float _lastControlY;

        public SvgGlyph(int x, int y, LazySvg svg, float xScale, float yScale)
            : base(x, y)
        {
            _svg = svg;
            _xGlyphScale = xScale * 0.0099f;
            _yGlyphScale = yScale * 0.0099f;
        }

        public override void Paint(int cx, int cy, ICanvas canvas)
        {
            if (_svg == null) return;
            _xScale = _xGlyphScale * Scale;
            _yScale = _yGlyphScale * Scale;

            var startX = X + cx;
            var startY = Y + cy;
            _currentX = startX;
            _currentY = startY;
            canvas.BeginPath();

            for (int i = 0, j = _svg.Commands.Count; i < j; i++)
            {
                ParseCommand(startX, startY, canvas, _svg.Commands[i]);
            }
            canvas.Fill();
        }

        private void ParseCommand(int cx, int cy, ICanvas canvas, SvgCommand cmd)
        {
            bool canContinue; // reusable flag for shorthand curves
            int i;
            switch (cmd.Cmd)
            {
                //
                // Moving
                // 
                case "M": // absolute moveto
                    _currentX = (cx + cmd.Numbers[0] * _xScale);
                    _currentY = (cy + cmd.Numbers[1] * _yScale);
                    canvas.MoveTo(_currentX, _currentY);
                    break;
                case "m": // relative moveto
                    _currentX += (cmd.Numbers[0] * _xScale);
                    _currentY += (cmd.Numbers[1] * _yScale);
                    canvas.MoveTo(_currentX, _currentY);
                    break;

                //
                // Closing
                // 
                case "Z":
                case "z":
                    canvas.ClosePath();
                    break;

                //
                // Lines
                //                 
                case "L": // absolute lineTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        _currentX = (cx + cmd.Numbers[i++] * _xScale);
                        _currentY = (cy + cmd.Numbers[i++] * _yScale);
                        canvas.LineTo(_currentX, _currentY);
                    }
                    break;
                case "l": // relative lineTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        _currentX += (cmd.Numbers[i++] * _xScale);
                        _currentY += (cmd.Numbers[i++] * _yScale);
                        canvas.LineTo(_currentX, _currentY);
                    }
                    break;

                case "V": // absolute verticalTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        _currentY = (cy + cmd.Numbers[i++] * _yScale);
                        canvas.LineTo(_currentX, _currentY);
                    }
                    break;
                case "v": // relative verticalTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        _currentY += (cmd.Numbers[i++] * _yScale);
                        canvas.LineTo(_currentX, _currentY);
                    }
                    break;

                case "H": // absolute horizontalTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        _currentX = (cx + cmd.Numbers[i++] * _xScale);
                        canvas.LineTo(_currentX, _currentY);
                    }
                    break;
                case "h": // relative horizontalTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        _currentX += (cmd.Numbers[i++] * _xScale);
                        canvas.LineTo(_currentX, _currentY);
                    }
                    break;

                //
                // cubic bezier curves
                // 
                case "C": // absolute cubicTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (cx + cmd.Numbers[i++] * _xScale);
                        float y1 = (cy + cmd.Numbers[i++] * _yScale);
                        float x2 = (cx + cmd.Numbers[i++] * _xScale);
                        float y2 = (cy + cmd.Numbers[i++] * _yScale);
                        float x3 = (cx + cmd.Numbers[i++] * _xScale);
                        float y3 = (cy + cmd.Numbers[i++] * _yScale);
                        _lastControlX = x2;
                        _lastControlY = y2;
                        _currentX = x3;
                        _currentY = y3;
                        canvas.BezierCurveTo(x1, y1, x2, y2, x3, y3);
                    }
                    break;
                case "c": // relative cubicTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y1 = (_currentY + cmd.Numbers[i++] * _yScale);
                        float x2 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y2 = (_currentY + cmd.Numbers[i++] * _yScale);
                        float x3 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y3 = (_currentY + cmd.Numbers[i++] * _yScale);
                        _lastControlX = x2;
                        _lastControlY = y2;
                        _currentX = x3;
                        _currentY = y3;
                        canvas.BezierCurveTo(x1, y1, x2, y2, x3, y3);
                    }
                    break;

                case "S": // absolute shorthand cubicTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (cx + cmd.Numbers[i++] * _xScale);
                        float y1 = (cy + cmd.Numbers[i++] * _yScale);
                        canContinue = _lastCmd == "c" || _lastCmd == "C" || _lastCmd == "S" || _lastCmd == "s";
                        float x2 = canContinue
                                            ? _currentX + (_currentX - _lastControlX)
                                            : _currentX;
                        float y2 = canContinue
                                            ? _currentY + (_currentY - _lastControlY)
                                            : _currentY;
                        float x3 = (cx + cmd.Numbers[i++] * _xScale);
                        float y3 = (cy + cmd.Numbers[i++] * _yScale);
                        _lastControlX = x2;
                        _lastControlY = y2;
                        _currentX = x3;
                        _currentY = y3;
                        canvas.BezierCurveTo(x1, y1, x2, y2, x3, y3);
                    }
                    break;
                case "s": // relative shorthand cubicTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y1 = (_currentY + cmd.Numbers[i++] * _yScale);
                        canContinue = _lastCmd == "c" || _lastCmd == "C" || _lastCmd == "S" || _lastCmd == "s";
                        float x2 = canContinue
                                            ? _currentX + (_currentX - _lastControlX)
                                            : _currentX;
                        float y2 = canContinue
                                            ? _currentY + (_currentY - _lastControlY)
                                            : _currentY;
                        float x3 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y3 = (_currentY + cmd.Numbers[i++] * _yScale);
                        _lastControlX = x2;
                        _lastControlY = y2;
                        _currentX = x3;
                        _currentY = y3;
                        canvas.BezierCurveTo(x1, y1, x2, y2, x3, y3);
                    }
                    break;

                //
                // quadratic bezier curves
                //
                case "Q": // absolute quadraticTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (cx + cmd.Numbers[i++] * _xScale);
                        float y1 = (cy + cmd.Numbers[i++] * _yScale);
                        float x2 = (cx + cmd.Numbers[i++] * _xScale);
                        float y2 = (cy + cmd.Numbers[i++] * _yScale);
                        _lastControlX = x1;
                        _lastControlY = y1;
                        _currentX = x2;
                        _currentY = y2;
                        canvas.QuadraticCurveTo(x1, y1, x2, y2);
                    }
                    break;
                case "q": // relative quadraticTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y1 = (_currentY + cmd.Numbers[i++] * _yScale);
                        float x2 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y2 = (_currentY + cmd.Numbers[i++] * _yScale);
                        _lastControlX = x1;
                        _lastControlY = y1;
                        _currentX = x2;
                        _currentY = y2;
                        canvas.QuadraticCurveTo(x1, y1, x2, y2);
                    }
                    break;

                case "T": // absolute shorthand quadraticTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        float x1 = (cx + cmd.Numbers[i++] * _xScale);
                        float y1 = (cy + cmd.Numbers[i++] * _yScale);
                        canContinue = _lastCmd == "q" || _lastCmd == "Q" || _lastCmd == "t" || _lastCmd == "T";
                        float cpx = canContinue
                                            ? _currentX + (_currentX - _lastControlX)
                                            : _currentX;
                        float cpy = canContinue
                                            ? _currentY + (_currentY - _lastControlY)
                                            : _currentY;
                        _currentX = x1;
                        _currentY = y1;
                        _lastControlX = cpx;
                        _lastControlY = cpy;
                        canvas.QuadraticCurveTo(cpx, cpy, x1, y1);
                    }
                    break;
                case "t": // relative shorthand quadraticTo
                    i = 0;
                    while (i < cmd.Numbers.Count)
                    {
                        // TODO: buggy/incomplete
                        float x1 = (_currentX + cmd.Numbers[i++] * _xScale);
                        float y1 = (_currentY + cmd.Numbers[i++] * _yScale);
                        canContinue = _lastCmd == "q" || _lastCmd == "Q" || _lastCmd == "t" || _lastCmd == "T";
                        float cpx = canContinue
                                            ? _currentX + (_currentX - _lastControlX)
                                            : _currentX;
                        float cpy = canContinue
                                            ? _currentY + (_currentY - _lastControlY)
                                            : _currentY;
                        _lastControlX = cpx;
                        _lastControlY = cpy;
                        canvas.QuadraticCurveTo(cpx, cpy, x1, y1);
                    }
                    break;
            }
            _lastCmd = cmd.Cmd;
        }
    }
}