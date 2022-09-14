using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gfxfont
{
    public partial class Form1 : Form
    {
        static Form1()
        {
            Fonts.Add(ParseFont("CourierCyr12.h"));
        }
        public Form1()
        {
            InitializeComponent();

            _font = Fonts.Last();
            setFont(_font);
            updateFontsList();
        }

        void updateFontsList()
        {
            listView2.Items.Clear();
            for (int i = 0; i < Fonts.Count; i++)
            {
                listView2.Items.Add(new ListViewItem(new string[] { Fonts[i].Name, "0x" + Fonts[i].StartCode.ToString("X2"), Fonts[i].YAdvance.ToString(), }) { Tag = Fonts[i] });
            }
        }

        void setFont(GfxFont font)
        {
            _font = font;
            listView1.Items.Clear();
            for (int i = 0; i < font.Glyphs.Count; i++)
            {
                ushort c = (ushort)(i + font.StartCode);
                font.Glyphs[i].Code = c;
                char cc = (char)c;
                listView1.Items.Add(new ListViewItem(new string[] {
                    $"{cc}",
                    "0x"+c.ToString("X"),
                    font.Glyphs[i].Width.ToString(),
                    font.Glyphs[i].Height.ToString(),
                    font.Glyphs[i].xAdvance.ToString(),
                    font.Glyphs[i].xOffset.ToString(),
                    font.Glyphs[i].yOffset.ToString()
                })
                //{ Tag = c });
                { Tag = font.Glyphs[i] });
            }
        }

        public static List<GfxFont> Fonts = new List<GfxFont>();

        int cellw = 12;


        private bool getBitByMask(byte v, byte bitMask)
        {
            return (v & bitMask) > 0;
        }


        GfxFont _font;
        Glyph selectedGlyph = null;
        public void updateGlyph(int glyphNo)
        {
            selectedGlyph = _font.Glyphs[glyphNo];
            var ww = _font.Glyphs[glyphNo].Width;
            var hh = _font.Glyphs[glyphNo].Height;
            byte[] data = _font.Glyphs[glyphNo].Bitmap;
            if (data.Length == 0)
            {
                pictureBox1.Image = null;
                return;
            }

            Bitmap bmp = new Bitmap((int)(ww * cellw + 5), (int)(hh * cellw + 5));
            var gr = Graphics.FromImage(bmp);
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

            pictureBox1.Image = bmp;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var g = (Glyph)listView1.SelectedItems[0].Tag;
            var ss = g.Code;
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
        public static GfxFont ParseFont(string path)
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
            //font1.Bitmaps = bb.ToArray();

            for (int i = 0; i < glp.Count; i++)
            {
                var offset1 = glp[i][0];
                byte[] data = null;
                if (i < glp.Count - 1)
                {
                    var offset2 = glp[i + 1][0];
                    data = bb.Skip(offset1).Take(offset2 - offset1).ToArray();
                }
                else
                    data = bb.Skip(offset1).ToArray();

                font1.Glyphs.Add(new Glyph()
                {
                    Bitmap = data,
                    xAdvance = glp[i][3],
                    Width = glp[i][1],
                    Height = glp[i][2],
                    xOffset = glp[i][4],
                    yOffset = glp[i][5],
                });
            }

            var desc = spl22.First(z => z.Contains("GFXfont"));
            var spll = desc.Split(new char[] { ',', '}', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            try
            {
                var startCodeToken = spll[spll.Length - 3].Trim();
                var yAdvanceToken = spll[spll.Length - 1].Trim();

                font1.StartCode = startCodeToken.StartsWith("0x") ? Convert.ToUInt16(startCodeToken, 16) : ushort.Parse(startCodeToken);
                font1.YAdvance = yAdvanceToken.StartsWith("0x") ? Convert.ToUInt16(yAdvanceToken, 16) : int.Parse(yAdvanceToken);
            }
            catch (Exception ex)
            {

            }

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

            var f = ParseFont(ofd.FileName);
            Fonts.Add(f);
            updateFontsList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            _font.Export(sfd.FileName);
        }

        private void copyGlyphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var gl = (Glyph)listView1.SelectedItems[0].Tag;
            Clipboard.SetText(gl.ToXml());
        }

        private void pasteGlyphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var gl = (Glyph)listView1.SelectedItems[0].Tag;
            var code = gl.Code; //preserve code
            gl.ParseXml(Clipboard.GetText());
            gl.Code = code;
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GlyphEdit ge = new GlyphEdit();
            ge.Init(selectedGlyph);
            ge.ShowDialog();
        }
    }
}
