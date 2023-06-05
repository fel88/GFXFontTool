using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace gfxfont
{
    public partial class STM32Fonts : Form
    {
        public STM32Fonts()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<byte> bb = new List<byte>();
            foreach (var item in richTextBox1.Lines)
            {
                var ar = item.Trim().Split(new char[] { ',', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (ar.Length < 2) continue;
                var b1 = Convert.ToByte(ar[0].Trim(), 16);
                var b2 = Convert.ToByte(ar[1].Trim(), 16);
                bb.Add(b1);
                bb.Add(b2);
            }
            int cell = 7;
            Bitmap bmp = new Bitmap(cell * 11, cell * 16);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 11; j++)
                {
                    var byte0 = bb[i * 2];
                    var byte1 = bb[i * 2 + 1];
                    List<int> bits = new List<int>();
                    foreach (var bt in new[] { byte0, byte1 })
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            if ((bt & (1 << (8 - k - 1))) > 0)
                                bits.Add(1);
                            else
                                bits.Add(0);
                        }

                    }

                    if (bits[j] > 0)
                        gr.FillRectangle(Brushes.Black, j * cell, i * cell, cell, cell);
                }

            pictureBox1.Image = bmp;
        }

        private void STM32Fonts_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Font f = new Font("Courier New", (int)numericUpDown1.Value);
            var gl = getGlyph(textBox1.Text[0], f);
            pictureBox1.Image = gl;
            label1.Text = gl.Width + "x" + gl.Height;
            List<byte> bytes = new List<byte>();
            List<byte> bits = new List<byte>();

            for (int j = 0; j < gl.Height; j++)
            {
                for (int i = 0; i < gl.Width; i++)
                {
                    if (i > 16) 
                        break;
                    var px = gl.GetPixel(i, j);
                    if (px.R < 128)
                    {
                        bits.Add(1);
                    }
                    else
                    {
                        bits.Add(0);
                    }
                    if (bits.Count == 8)
                    {
                        byte b0 = 0;
                        for (int k = 0; k < 8; k++)
                        {
                            if (bits[k] == 1)
                            {
                                b0 |= (byte)(1 << (8-k-1));
                            }
                        }
                        bytes.Add(b0);
                        bits.Clear();
                    }
                }
                if (bits.Count > 0)
                {
                    while (bits.Count < 8) bits.Add(0);
                    if (bits.Count == 8)
                    {
                        byte b0 = 0;
                        for (int k = 0; k < 8; k++)
                        {
                            if (bits[k] == 1)
                            {
                                b0 |= (byte)(1 << k);
                            }
                        }
                        bytes.Add(b0);
                        bits.Clear();
                    }
                }
            }
            while (bytes.Count > 32) bytes.RemoveAt(bytes.Count - 1);

            richTextBox2.Clear();
            for (int i = 0; i < bytes.Count; i += 2)
            {
                richTextBox2.AppendText("0x" + bytes[i].ToString("X2") + ", 0x");
                richTextBox2.AppendText(bytes[i + 1].ToString("X2"));
                richTextBox2.AppendText(Environment.NewLine);
            }
        }
        Bitmap getGlyph(char c, Font font)
        {
            var ms = TextRenderer.MeasureText(c + "", font);
            Bitmap bmp = new Bitmap(ms.Width, ms.Height);
            Graphics gr = Graphics.FromImage(bmp);
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            gr.Clear(Color.White);
            gr.DrawString(c + "", font, Brushes.Black, 0, 0);
            return bmp;
        }
    }
}
