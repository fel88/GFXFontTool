using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gfxfont
{
    public class GfxFont
    {
        public string Name;
        public ushort StartCode;
        public int YAdvance;
        //public int[,] Glyphs;
        //public byte[] Bitmaps;
        public List<Glyph> Glyphs = new List<Glyph>();
        internal void Export(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("const uint8_t exportFontBitmaps[] PROGMEM = {");
            foreach (var gg in Glyphs)
            {
                foreach (var bb in gg.Bitmap)
                {
                    sb.Append($"0x{bb.ToString("X2")}, ");
                }
                sb.AppendLine();
            }
            sb.AppendLine("};");

            sb.AppendLine("const GFXglyph exportFontGlyphs[] PROGMEM = {");
            int offset = 0;
            foreach (var item in Glyphs)
            {
                sb.AppendLine("{" + $"{offset}, {item.Width},{item.Height},{item.xAdvance},{item.xOffset},{item.yOffset} " + "},");
                offset += item.Bitmap.Length;
            }
            sb.AppendLine("};");
            sb.AppendLine("const GFXfont exportFont[] PROGMEM = {");
            sb.AppendLine("(uint8_t*)exportFontBitmaps,");
            sb.AppendLine("(GFXglyph*)exportFontGlyphs,");
            var endCode = StartCode + Glyphs.Count;
            sb.AppendLine($"{StartCode}, {endCode}, {YAdvance} ");
            sb.AppendLine("};");

            File.WriteAllText(fileName, sb.ToString());
        }
    }
}
