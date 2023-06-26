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
            fontDialog1.Font = new Font("Courier New", (int)numericUpDown1.Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<byte> bb = new List<byte>();
            foreach (var item in richTextBox1.Lines)
            {
                var ar = item.Trim().Split(new char[] { ',', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (ar.Length < bytesPerRow)
                    continue;

                for (int i = 0; i < bytesPerRow; i++)
                {
                    var b1 = Convert.ToByte(ar[i].Trim(), 16);
                    bb.Add(b1);
                }
            }

            int cell = 7;
            Bitmap bmp = new Bitmap(cell * outputFontWidth, cell * outputFontHeigth);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            int index = 0;
            for (int i = 0; i < outputFontHeigth; i++)
            {
                var bytes = bb.Skip(index).Take(bytesPerRow).ToArray();
                index += bytesPerRow;
                List<int> bits = new List<int>();
                foreach (var bt in bytes)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if ((bt & (1 << (8 - k - 1))) > 0)
                            bits.Add(1);
                        else
                            bits.Add(0);
                    }

                }
                for (int j = 0; j < outputFontWidth; j++)
                {
                    if (bits[j] > 0)
                        gr.FillRectangle(Brushes.Black, j * cell, i * cell, cell, cell);
                }
            }

            pictureBox1.Image = bmp;
        }

        private void STM32Fonts_Load(object sender, EventArgs e)
        {

        }

        int bytesPerRow = 2;
        public void GenerateOneGlyph(char c)
        {
            Font f = fontDialog1.Font;
            var gl = getGlyph(c, f);
            pictureBox1.Image = gl;
            label1.Text = gl.Width + "x" + gl.Height;
            List<byte> bytes = new List<byte>();
            List<byte> bits = new List<byte>();
            int startX = (int)numericUpDown2.Value;
            int startY = (int)numericUpDown3.Value;

            for (int j = startY; j < gl.Height; j++)
            {
                for (int i = startX; i < gl.Width; i++)
                {
                    if (i >= (bytesPerRow * 8 + startX))
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
                                b0 |= (byte)(1 << (8 - k - 1));
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

            richTextBox2.AppendText($"// {c}" + Environment.NewLine);
            int lines = 0;
            for (int i = 0; i < bytes.Count; i += bytesPerRow)
            {
                richTextBox2.AppendText("0x" + bytes[i].ToString("X2"));
                for (int j = 1; j < bytesPerRow; j++)
                {
                    richTextBox2.AppendText(", 0x" + bytes[i + j].ToString("X2"));
                }

                richTextBox2.AppendText(", " + Environment.NewLine);
                lines++;
                if (lines >= outputFontHeigth) 
                    break;
            }
            if (lines < outputFontHeigth)
            {
                richTextBox2.AppendText("0x0");
                for (int j = 1; j < bytesPerRow; j++)
                {
                    richTextBox2.AppendText(", 0x0" );
                }
                richTextBox2.AppendText(", " + Environment.NewLine);
            }
            richTextBox2.AppendText(Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();

            GenerateOneGlyph(textBox1.Text[0]);
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

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            char start = textBox1.Text[0];
            char end = textBox2.Text[0];
            for (char i = start; i <= end; i++)
            {
                GenerateOneGlyph(i);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            label3.Text = fontDialog1.Font.FontFamily.ToString() + " " + fontDialog1.Font.Size;
        }
        int outputFontHeigth = 16;
        int outputFontWidth = 11;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1)
            {
                outputFontHeigth = 12;
                outputFontWidth = 7;
                bytesPerRow = 1;

            }
            if (comboBox1.SelectedIndex == 0)
            {
                outputFontHeigth = 16;
                outputFontWidth = 11;
                bytesPerRow = 2;
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
