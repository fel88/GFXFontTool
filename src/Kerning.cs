using System.Drawing;
using System.Windows.Media;

namespace gfxfont
{
    public static class Kerning
    {

        public static  void func1()
        {
            Graphics e = null;
            string infoString = "";  // enough space for one line of output
            int ascent;             // font family ascent in design units
            float ascentPixel;      // ascent converted to pixels
            int descent;            // font family descent in design units
            float descentPixel;     // descent converted to pixels
            int lineSpacing;        // font family line spacing in design units
            float lineSpacingPixel; // line spacing converted to pixels

            System.Drawing.FontFamily fontFamily = new System.Drawing.FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               16, FontStyle.Regular,
               GraphicsUnit.Pixel);
            PointF pointF = new PointF(10, 10);
            SolidBrush solidBrush = new SolidBrush(System.Drawing.Color.Black);

            // Display the font size in pixels.
            infoString = "font.Size returns " + font.Size + ".";
            e.DrawString(infoString, font, solidBrush, pointF);

            // Move down one line.
            pointF.Y += font.Height;

            // Display the font family em height in design units.
            infoString = "fontFamily.GetEmHeight() returns " +
               fontFamily.GetEmHeight(FontStyle.Regular) + ".";
            e.DrawString(infoString, font, solidBrush, pointF);

            // Move down two lines.
            pointF.Y += 2 * font.Height;

            // Display the ascent in design units and pixels.
            ascent = fontFamily.GetCellAscent(FontStyle.Regular);

            // 14.484375 = 16.0 * 1854 / 2048
            ascentPixel =
               font.Size * ascent / fontFamily.GetEmHeight(FontStyle.Regular);
            infoString = "The ascent is " + ascent + " design units, " + ascentPixel +
               " pixels.";
            e.DrawString(infoString, font, solidBrush, pointF);

            // Move down one line.
            pointF.Y += font.Height;

            // Display the descent in design units and pixels.
            descent = fontFamily.GetCellDescent(FontStyle.Regular);

            // 3.390625 = 16.0 * 434 / 2048
            descentPixel =
               font.Size * descent / fontFamily.GetEmHeight(FontStyle.Regular);
            infoString = "The descent is " + descent + " design units, " +
               descentPixel + " pixels.";
            e.DrawString(infoString, font, solidBrush, pointF);

            // Move down one line.
            pointF.Y += font.Height;

            // Display the line spacing in design units and pixels.
            lineSpacing = fontFamily.GetLineSpacing(FontStyle.Regular);

            // 18.398438 = 16.0 * 2355 / 2048
            lineSpacingPixel =
            font.Size * lineSpacing / fontFamily.GetEmHeight(FontStyle.Regular);
            infoString = "The line spacing is " + lineSpacing + " design units, " +
               lineSpacingPixel + " pixels.";
            e.DrawString(infoString, font, solidBrush, pointF);









            // create typeface from font
            var typeface = new Typeface("Arial");

            // create glyphtypeface
            typeface.TryGetGlyphTypeface(out var glyphTypeface);

            // retrieve metrics
            glyphTypeface.AdvanceHeights.TryGetValue('S', out var advancedHeight);
        }
    }
}
