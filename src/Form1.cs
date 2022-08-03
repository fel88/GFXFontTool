using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gfxfont
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Fonts.Add(parseFont("CourierCyr12.h"));
            _font = Fonts.Last();
            setFont(_font);
            updateFontsList();
        }

        void updateFontsList()
        {
            listView2.Items.Clear();
            for (int i = 0; i < Fonts.Count; i++)
            {
                listView2.Items.Add(new ListViewItem(new string[] { Fonts[i].Name }) { Tag = Fonts[i] });
            }
        }

        void setFont(GfxFont font)
        {
            _font = font;            
            listView1.Items.Clear();
            for (int i = 0; i < font.Glyphs.GetLength(0); i++)
            {
                ushort c = (ushort)(i + font.StartCode);
                byte b1 = (byte)(c & 0xff);
                byte b2 = (byte)((c >> 8) & 0xff);
                var str = Encoding.Unicode.GetString(new[] { b1, b2 });
                listView1.Items.Add(new ListViewItem(new string[] { str, c.ToString("X") , font.Glyphs[i, 1].ToString() , font.Glyphs[i, 2].ToString()
              ,  font.Glyphs[i,3].ToString() ,  font.Glyphs[i,4].ToString()
                }) { Tag = c });
            }
        }

        public List<GfxFont> Fonts = new List<GfxFont>();

        int cellw = 12;

   
        private bool getBitByMask(byte v, byte bitMask)
        {
            return (v & bitMask) > 0;
        }

     
        GfxFont _font;
        public void updateGlyph(int glyphNo)
        {
            int ww = _font.Glyphs[glyphNo, 1];
            int hh = _font.Glyphs[glyphNo, 2];
            byte[] data;
            if (glyphNo < _font.Glyphs.GetLength(0) - 1)
            {
                var diff = _font.Glyphs[glyphNo + 1, 0] - _font.Glyphs[glyphNo, 0];
                data = _font.Bitmaps.Skip(_font.Glyphs[glyphNo, 0]).Take(diff).ToArray();
            }
            else
                data = _font.Bitmaps.Skip(_font.Glyphs[glyphNo, 0]).ToArray();

            Bitmap bmp = new Bitmap(ww * cellw + 5, hh * cellw + 5);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            int index = 0;
            for (int j = 0; j < hh; j++)
            {
                for (int i = 0; i < ww; i++)
                {
                    int bitIndex = i + ww * j;         // Bit index in array of bits
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

            pictureBox1.Image = bmp;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var ss = (ushort)listView1.SelectedItems[0].Tag;
            ss -= _font.StartCode;

            updateGlyph(ss);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;

            var f = listView2.SelectedItems[0].Tag as GfxFont;

            setFont(f);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            

        }
        GfxFont parseFont(string path)
        {
            var txt = File.ReadAllText(path);
            List<int> allb = new List<int>();
            List<int> openb = new List<int>();
            List<int> closeb = new List<int>();
            List<int> colon = new List<int>();
            List<int> cuts = new List<int>();
            Stack<char> stack = new Stack<char>();


            for (int i = 0; i < txt.Length; i++)
            {
                if (txt[i] == '{')
                {
                    openb.Add(i); allb.Add(i);

                    stack.Push('{');
                }
                if (txt[i] == '}')
                {
                    closeb.Add(i); allb.Add(i);
                    if (stack.Peek() != '{') throw new ArgumentNullException();
                    stack.Pop();
                    if (stack.Count == 0)
                        cuts.Add(i);
                }
                if (txt[i] == ';') colon.Add(i);
            }

            List<string> spl22 = new List<string>();
            spl22.Add(txt.Substring(0, cuts[0]));
            for (int i = 1; i < cuts.Count; i++)
            {
                spl22.Add(txt.Substring(cuts[i - 1], cuts[i] - cuts[i - 1]));
            }

            var spl = txt.Split(new string[] { "PROGMEM", ";", "const" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var fr = spl.First(z => z.Contains("[]"));
            var glyphs = spl22.First(z => z.Contains("Glyphs"));
            var bitmaps = spl22.First(z => z.Contains("Bitmaps"));

            GfxFont font1 = new GfxFont();

            font1.Name = fr.Replace("[]", "").Replace("uint8_t", "").Replace("Bitmaps", "").Trim();
            var spl1 = glyphs.Split(new char[] { ',', '{', '}' }).ToArray();
            List<int[]> glp = new List<int[]>();
            int pos = 0;
            int[] aa = new int[6];
            for (int i = 0; i < spl1.Length; i++)
            {
                var sp = spl1[i].Trim();
                if (sp.Contains("/*")) continue;
                if (sp.Contains("\r")) continue;
                int res;

                if (sp.StartsWith("0x"))
                {
                    res = Convert.ToInt32(sp, 16);
                }
                else
                if (!int.TryParse(sp, out res))
                {
                    continue;
                }

                aa[pos] = res;
                pos++;
                if (pos == 6)
                {
                    pos = 0;
                    glp.Add(aa.ToArray());
                    aa = new int[6];
                }
            }

            font1.Glyphs = new int[glp.Count, 6];
            for (int i = 0; i < glp.Count; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    font1.Glyphs[i, j] = glp[i][j];
                }
            }


            //parse bitmaps
            var spl2 = bitmaps.Split(new char[] { ',', '{', '}', '\r', '\n' }).ToArray();
            List<byte> bb = new List<byte>();
            for (int i = 0; i < spl2.Length; i++)
            {
                var sp = spl2[i].Trim();
                if (sp.Contains("/*")) continue;
                if (!sp.Contains("0x")) continue;

                bb.Add(Convert.ToByte(sp, 16));
            }
            font1.Bitmaps = bb.ToArray();


            var desc = spl22.First(z => z.Contains("GFXfont"));
            var spll = desc.Split(new char[] { ',', '}', ';', '\n', '\r' }).ToArray();
            font1.StartCode = Convert.ToUInt16(spll[spll.Length - 3].Trim(), 16);

            return font1;

        }
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            cellw = (int)numericUpDown4.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var f = parseFont(ofd.FileName);
            Fonts.Add(f);
            updateFontsList();
        }
    }
}
