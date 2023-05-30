using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace gfxfont
{
    public partial class BitmapGenerator : Form
    {
        public BitmapGenerator()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ar = richTextBox1.Text.Split(new char[] { '\n', '\r', ',', ' ', '}', ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            List<byte> bytes = new List<byte>();
            Bitmap bmp = new Bitmap((int)numericUpDown1.Value, (int)numericUpDown2.Value);
            foreach (var hex in ar)
            {
                var b = Convert.ToByte(hex.Replace("0x", ""), 16);
                bytes.Add(b);

            }
            int cntr = 0;
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    var byteIdx = cntr / 8;
                    var bitIdx = 7 - cntr % 8;
                    bmp.SetPixel(i, j, (bytes[byteIdx] & (1 << bitIdx)) > 0 ? Color.White : Color.Black);
                    cntr++;
                }
            }
            pictureBox1.Image = bmp;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap((int)numericUpDown1.Value, (int)numericUpDown2.Value);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            gr.DrawString(richTextBox2.Text, fontDialog1.Font, Brushes.Black, new RectangleF(0, 0, bmp.Width, bmp.Height));
            var threshold = (int)numericUpDown3.Value;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    var px = bmp.GetPixel(i, j);
                    if (px.R > threshold)                    
                        bmp.SetPixel(i, j, Color.White);                    
                    else
                        bmp.SetPixel(i, j, Color.Black);
                }
            }
            pictureBox1.Image = bmp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            var bmp = pictureBox1.Image as Bitmap;
            List<byte> bytes = new List<byte>();
            List<byte> accum = new List<byte>();
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
            {
             
                    var px = bmp.GetPixel(i, j);
                    if (px.R > 128)
                        accum.Add(1);
                    else
                        accum.Add(0);

                    if (accum.Count == 8)
                    {
                        byte b = 0;
                        accum.Reverse();
                        for (int z = 0; z < 8; z++)
                        {
                            if (accum[z] == 1)
                                b |= (byte)(1 << z);
                        }
                        accum.Clear();
                        bytes.Add(b);
                    }
                }
            }
            var sz = bmp.Height * bmp.Width / 8;
            for (int i = bytes.Count; i < sz; i++)
            {
                bytes.Add(0xFF);
            }
            var hex = ByteArrayToString(bytes.ToArray());


            richTextBox1.Text = hex;


        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("0x{0:X2}, ", b);
            return hex.ToString();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");            
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");            
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            pictureBox1.Image = Bitmap.FromFile(ofd.FileName);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Clipboard.GetImage();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}
