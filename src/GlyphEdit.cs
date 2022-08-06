using System;
using System.Drawing;
using System.Windows.Forms;

namespace gfxfont
{
    public partial class GlyphEdit : Form
    {
        public GlyphEdit()
        {
            InitializeComponent();
            pictureBox1.Paint += PictureBox1_Paint;
        }
        int cellw = 12;
        private bool getBitByMask(byte v, byte bitMask)
        {
            return (v & bitMask) > 0;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            var ww = glyph.Width;
            var hh = glyph.Height;
            byte[] data = glyph.Bitmap;


            var gr = e.Graphics;
            gr.Clear(Color.White);
            int index = 0;
            for (int j = 0; j < hh; j++)
            {
                for (int i = 0; i < ww; i++)
                {
                    int bitIndex = i + (int)ww * j;         // Bit index in array of bits
                    int _byte = bitIndex >> 3;             // Byte offset of bit, assume 8 bit bytes
                    int bitMask = 0x80 >> (bitIndex & 7); // Individual bit in byte array for bit

                    gr.DrawRectangle(Pens.Black, i * cellw, j * cellw, cellw, cellw); ;
                    var bit = getBitByMask(data[_byte], (byte)bitMask);
                    if (bit)
                    {
                        gr.FillRectangle(Brushes.Green, i * cellw, j * cellw, cellw, cellw); ;
                    }
                    index++;
                }
            }
            
        }

        Glyph glyph;

        public void Init(Glyph g)
        {
            glyph = g;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            var c = pictureBox1.PointToClient(Cursor.Position);
            var cx = c.X / cellw;
            var cy = c.Y / cellw;

            int bitIndex = cx + (int)glyph.Width * cy;         // Bit index in array of bits
            int _byte = bitIndex >> 3;             // Byte offset of bit, assume 8 bit bytes
            int bitMask = 0x80 >> (bitIndex & 7); // Individual bit in byte array for bit
            var bit = getBitByMask(glyph.Bitmap[_byte], (byte)bitMask);
            if (bit) {
                glyph.Bitmap[_byte] &= (byte)(~bitMask);

            }
            else
            glyph.Bitmap[_byte] |= (byte)bitMask;            

        }
    }
}
