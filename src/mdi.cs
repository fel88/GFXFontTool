using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gfxfont
{
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
        }

        private void mdi_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            generator g = new generator();
            g.MdiParent = this;
            g.Show();
        }
    }
}
