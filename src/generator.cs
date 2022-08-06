using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace gfxfont
{
    public partial class generator : Form
    {
        public generator()
        {
            InitializeComponent();
            pictureBox1.InterpolationMode = InterpolationMode.NearestNeighbor;
            setFontStatus();
        }

        private void setFontStatus()
        {
            toolStripStatusLabel1.Text = $"Font: {fontDialog1.Font.FontFamily.Name} {fontDialog1.Font.Size}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bmp = getGlyph(textBox1.Text[0]);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = bmp;

        }
        Bitmap getGlyph(char c)
        {
            var ms = TextRenderer.MeasureText(c + "", fontDialog1.Font);
            Bitmap bmp = new Bitmap(ms.Width, ms.Height);
            Graphics gr = Graphics.FromImage(bmp);
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            gr.Clear(Color.White);
            gr.DrawString(c + "", fontDialog1.Font, Brushes.Black, 0, 0);
            return bmp;
        }


        private byte setBit(byte v1, int bit)
        {
            return (byte)(v1 | bit);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GfxFont font = new GfxFont();
            font.Name = "generatedFont";
            font.YAdvance = fontDialog1.Font.Height;
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                List<Point> pp = new List<Point>();
                var g = getGlyph((char)(i));
                for (int ii = 0; ii < g.Width; ii++)
                {
                    for (int j = 0; j < g.Height; j++)
                    {
                        if (g.GetPixel(ii, j).R < 128)
                        {
                            pp.Add(new Point(ii, j));
                        }
                    }
                }

                byte[] data = new byte[0];

                var ms = TextRenderer.MeasureText((char)(i) + "", fontDialog1.Font);

                int w = 0;
                int h = 0;
                if (((char)i) == ' ')
                {
                    w = ms.Width;
                  //  h = ms.Height;
                }
                if (pp.Any())
                {
                    var minx = pp.Min(z => z.X);
                    var miny = pp.Min(z => z.Y);
                    var maxx = pp.Max(z => z.X);
                    var maxy = pp.Max(z => z.Y);
                    //miny = 0;
                    //minx = 0;
                    maxy = ms.Height - 1;
                    w = maxx - minx + 1;
                    h = maxy - miny + 1;

                    data = new byte[w * h + (8 - (w * h) % 8) % 8];
                    foreach (var p in pp)
                    {
                        var px = p.X - minx;
                        var py = p.Y - miny;
                        int bitIndex = px + (int)w * py;         // Bit index in array of bits
                        int _byte = bitIndex >> 3;             // Byte offset of bit, assume 8 bit bytes
                        int bitMask = 0x80 >> (bitIndex & 7); // Individual bit in byte array for bit

                        data[_byte] = setBit(data[_byte], bitMask);
                    }
                }


                font.Glyphs.Add(new Glyph()
                {
                    Code = (ushort)i,
                    Bitmap = data,
                    Width = w,
                    Height = h,
                    yOffset = -h,
                    xAdvance = w + 1
                });
            }
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            font.Export(sfd.FileName);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            setFontStatus();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
