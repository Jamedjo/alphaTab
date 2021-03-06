﻿using AlphaTab.Collections;
using AlphaTab.Platform.Model;

namespace AlphaTab.Platform.Svg
{
    /// <summary>
    ///  A canvas implementation storing SVG data
    /// </summary>
    public class SvgCanvas : ICanvas
    {
        private string _buffer;
        private string _currentPath;
        private bool _currentPathIsEmpty;

        public int Width { get; set; }
        public int Height { get; set; }
        public Color Color { get; set; }
        public float LineWidth { get; set; }
        public Font Font { get; set; }
        public TextAlign TextAlign { get; set; }
        public TextBaseline TextBaseline { get; set; }

        public SvgCanvas()
        {
            _buffer = "";
            _currentPath = "";
            _currentPathIsEmpty = true;
            Color = new Color(255, 255, 255);
            LineWidth = 1;
            Width = 0;
            Height = 0;
            Font = new Font("Arial", 10);
            TextAlign = TextAlign.Left;
            TextBaseline = TextBaseline.Default;
        }

        public string ToSvg(bool includeWrapper, string className = null)
        {
            var buf = new StringBuilder();
            if (includeWrapper)
            {
                buf.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"");
                buf.Append(Width);
                buf.Append("px\" height=\"");
                buf.Append(Height);
                buf.Append("px\"");
                if (className != null)
                {
                    buf.Append(" class=\"");
                    buf.Append(className);
                    buf.Append("\"");
                }
                buf.Append(">\n");
            }
            buf.Append(_buffer);
            if (includeWrapper)
            {
                buf.Append("</svg>");
            }
            return buf.ToString();
        }

        public void Clear()
        {
            _buffer = "";
            _currentPath = "";
            _currentPathIsEmpty = true;
        }

        public void FillRect(float x, float y, float w, float h)
        {
            _buffer += "<rect x=\"";
            _buffer += x;
            _buffer += "\" y=\"";
            _buffer += y;
            _buffer += "\" width=\"";
            _buffer += w;
            _buffer += "\" height=\"";
            _buffer += h;
            _buffer += "\" style=\"fill:";
            _buffer += Color.ToRgbaString();
            _buffer += ";\" />\n";
        }

        public void StrokeRect(float x, float y, float w, float h)
        {
            _buffer += "<rect x=\"";
            _buffer += x;
            _buffer += "\" y=\"";
            _buffer += y;
            _buffer += "\" width=\"";
            _buffer += w;
            _buffer += "\" height=\"";
            _buffer += h;
            _buffer += "\" style=\"stroke:";
            _buffer += Color.ToRgbaString();
            _buffer += "; stroke-width:";
            _buffer += LineWidth;
            _buffer += ";\" />\n";
        }

        public void BeginPath()
        {

        }

        public void ClosePath()
        {
            _currentPath += " z";
        }

        public void MoveTo(float x, float y)
        {
            _currentPath += " M";
            _currentPath += x;
            _currentPath += ",";
            _currentPath += y;
        }

        public void LineTo(float x, float y)
        {
            _currentPathIsEmpty = false;
            _currentPath += " L";
            _currentPath += x;
            _currentPath += ",";
            _currentPath += y;
        }

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
        {
            _currentPathIsEmpty = false;
            _currentPath += " Q";
            _currentPath += cpx;
            _currentPath += ",";
            _currentPath += cpy;
            _currentPath += ",";
            _currentPath += x;
            _currentPath += ",";
            _currentPath += y;
        }

        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        {
            _currentPathIsEmpty = false;
            _currentPath += " C";
            _currentPath += cp1x;
            _currentPath += ",";
            _currentPath += cp1y;
            _currentPath += ",";
            _currentPath += cp2x;
            _currentPath += ",";
            _currentPath += cp2y;
            _currentPath += ",";
            _currentPath += x;
            _currentPath += ",";
            _currentPath += y;
        }

        public void Circle(float x, float y, float radius)
        {
            _currentPathIsEmpty = false;
            // 
            // M0,250 A1,1 0 0,0 500,250 A1,1 0 0,0 0,250 z
            _currentPath += " M";
            _currentPath += x - radius;
            _currentPath += ",";
            _currentPath += y;

            _currentPath += " A1,1 0 0,0 ";
            _currentPath += x + radius;
            _currentPath += ",";
            _currentPath += y;

            _currentPath += " A1,1 0 0,0 ";
            _currentPath += x - radius;
            _currentPath += ",";
            _currentPath += y;

            _currentPath += " z";
        }

        public void Rect(float x, float y, float w, float h)
        {
            _currentPathIsEmpty = false;
            _currentPath += " M";
            _currentPath += x;
            _currentPath += ",";
            _currentPath += y;

            _currentPath += " L";

            _currentPath += x + w;
            _currentPath += ",";
            _currentPath += y;
            _currentPath += " ";

            _currentPath += x + w;
            _currentPath += ",";
            _currentPath += y + h;
            _currentPath += " ";

            _currentPath += x;
            _currentPath += ",";
            _currentPath += y + h;
            _currentPath += " z";
        }

        public void Fill()
        {
            if (!_currentPathIsEmpty)
            {
                _buffer += "<path d=\"";
                _buffer += _currentPath;
                _buffer += "\" style=\"fill:";
                _buffer += Color.ToRgbaString();
                _buffer += "\" stroke=\"none\"/>\n";
            }
            _currentPath = "";
            _currentPathIsEmpty = true;
        }

        public void Stroke()
        {
            if (!_currentPathIsEmpty)
            {
                _buffer += "<path d=\"";
                _buffer += _currentPath;
                _buffer += "\" style=\"stroke:";
                _buffer += Color.ToRgbaString();
                _buffer += "; stroke-width:";
                _buffer += LineWidth;
                _buffer += ";\" fill=\"none\" />\n";
            }
            _currentPath = "";
            _currentPathIsEmpty = true;
        }

        public void FillText(string text, float x, float y)
        {
            _buffer += "<text x=\"";
            _buffer += x;
            _buffer += "\" y=\"";
            _buffer += y + GetSvgBaseLineOffset();
            _buffer += "\" style=\"font:";
            _buffer += Font.ToCssString();
            _buffer += "; fill:";
            _buffer += Color.ToRgbaString();
            _buffer += ";\" ";
            _buffer += " dominant-baseline=\"";
            _buffer += GetSvgBaseLine();
            _buffer += "\" text-anchor=\"";
            _buffer += GetSvgTextAlignment();
            _buffer += "\">\n";
            _buffer += text;
            _buffer += "</text>\n";
        }

        private string GetSvgTextAlignment()
        {
            switch (TextAlign)
            {
                case TextAlign.Left: return "start";
                case TextAlign.Center: return "middle";
                case TextAlign.Right: return "end";
            }
            return "";
        }

        private float GetSvgBaseLineOffset()
        {
            switch (TextBaseline)
            {
                case TextBaseline.Top: return 0;
                case TextBaseline.Middle: return 0;
                case TextBaseline.Bottom: return 0;
                default: return Font.Size;
            }
        }

        private string GetSvgBaseLine()
        {
            switch (TextBaseline)
            {
                case TextBaseline.Top: return "top";
                case TextBaseline.Middle: return "middle";
                case TextBaseline.Bottom: return "bottom";
                default: return "top";
            }
        }

        public float MeasureText(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            var font = SupportedFonts.Arial;
            if (Font.Family.Contains("Times"))
            {
                font = SupportedFonts.TimesNewRoman;
            }
            return FontSizes.MeasureString(text, font, Font.Size);
        }
    }
}