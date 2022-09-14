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

                if (ranges.Count > 0 && !ranges.Any(z => z.Contains(i)))
                {
                    data = new byte[0];
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
            font.Export(sfd.FileName, textBox2.Text);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            setFontStatus();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        List<FontRange> ranges = new List<FontRange>();
        void updateRangesList()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var range in ranges)
            {
                listView1.Items.Add(new ListViewItem(new string[] {
                range.From.ToString("X2"), range.To.ToString("X2") })
                { Tag = range });
            }
            listView1.EndUpdate();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            FontRange range = new FontRange();
            range.From = (int)numericUpDown2.Value;
            range.To = (int)numericUpDown3.Value;
            ranges.Add(range);
            updateRangesList();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            ranges.Remove(listView1.SelectedItems[0].Tag as FontRange);
            updateRangesList();
        }
    }
    public class FontRange
    {
        public int From;
        public int To;
        public bool Contains(int t)
        {
            return t >= From && t <= To;
        }
    }
}
