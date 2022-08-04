using System;
using System.Text;
using System.Xml.Linq;

namespace gfxfont
{
    public class Glyph
    {
        public string Symbol;
        public byte[] Bitmap;
        public int Width;
        public int Height;
        public int xAdvance;
        public int xOffset;

        public int yOffset;

        public ushort Code { get; set; }

        public void ParseXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            var g = doc.Element("glyph");
            Bitmap = Convert.FromBase64String(g.Value);
            Width = int.Parse(g.Attribute("width").Value);
            Height = int.Parse(g.Attribute("height").Value);
            xOffset = int.Parse(g.Attribute("xOffset").Value);
            yOffset = int.Parse(g.Attribute("yOffset").Value);
            xAdvance = int.Parse(g.Attribute("xAdvance").Value);
            Code = ushort.Parse(g.Attribute("code").Value);
            Symbol = g.Attribute("symbol").Value;
        }
        internal string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine($"<glyph code=\"{Code}\" symbol=\"{Symbol}\" width=\"{Width}\" height=\"{Height}\" xAdvance=\"{xAdvance}\" xOffset=\"{xOffset}\" yOffset=\"{yOffset}\">");
            var b64 = Convert.ToBase64String(Bitmap);
            sb.AppendLine(b64);
            sb.AppendLine("</glyph>");
            return sb.ToString();
        }
    }
}
