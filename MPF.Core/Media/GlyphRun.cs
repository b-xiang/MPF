﻿using MPF.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace MPF.Media
{
    public class GlyphRun
    {
        private readonly FontFamily _fontFamily;
        private readonly IReadOnlyList<char> _characters;
        private readonly IReadOnlyList<GlyphDrawingEntry> _drawingEntries;
        private readonly float _renderingEmSize;

        public GlyphRun(FontFamily fontFamily, IReadOnlyList<char> characters, float renderingEmSize)
        {
            _fontFamily = fontFamily;
            _characters = characters;
            _renderingEmSize = renderingEmSize;
            _drawingEntries = BuildDrawingEntries();
        }

        public void Draw(IDrawingContext context, Pen pen)
        {
            foreach (var entry in _drawingEntries)
                context.DrawGeometry(entry.Geometry, pen, ref entry.Transform);
        }

        private IReadOnlyList<GlyphDrawingEntry> BuildDrawingEntries()
        {
            var geometries = new List<GlyphDrawingEntry>(_characters.Count);

            float advance = 0.0f;
            char highSurrogate = '\0';
            foreach (var character in _characters)
            {
                var isBlank = char.IsControl(character) || char.IsWhiteSpace(character);
                if(!isBlank)
                {
                    uint code;
                    if (char.IsHighSurrogate(character))
                    {
                        highSurrogate = character;
                        continue;
                    }
                    else if (char.IsLowSurrogate(character))
                    {
                        Contract.Assert(highSurrogate != '\0');
                        code = (uint)char.ConvertToUtf32(highSurrogate, character);
                    }
                    else
                        code = (uint)character;

                    var glyph = _fontFamily.FindGlyph(code);
                    if(glyph != null)
                    {
                        float scale = 0.1f;
                        var fontMetrics = glyph.FontMetrics;
                        var glyphMetrics = glyph.GlyphMetrics;
                        var height = (int)fontMetrics.Ascent + (int)fontMetrics.Descent;
                        var transform = Matrix3x2.CreateTranslation(glyphMetrics.RightSideBearing + advance, fontMetrics.Descent) *
                            Matrix3x2.CreateScale(scale, -scale) * Matrix3x2.CreateTranslation(0, height * scale);
                        advance += glyphMetrics.AdvanceWidth;
                        geometries.Add(new GlyphDrawingEntry
                        {
                            Geometry = glyph.Geometry,
                            Transform = transform
                        });
                        continue;
                    }
                }
            }
            return geometries;
        }

        private class GlyphDrawingEntry
        {
            public Geometry Geometry;
            public Matrix3x2 Transform;
        }
    }
}