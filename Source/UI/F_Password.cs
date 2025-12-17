using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public partial class F_Password : Form
    {
        public F_Password()
        {
            InitializeComponent();
        }

        private void chkChangePW_CheckedChanged(object sender, EventArgs e)
        {
            if(chkChangePW.Checked)
            {
                tbCurrPW.Visible = true;
                tbChangePW.Visible = true;
                btnChangePW.Visible = true;
                label1.Visible = true;
                label2.Visible = true;
            }
            else
            {
                tbCurrPW.Visible = false;
                tbChangePW.Visible = false;
                btnChangePW.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
            }
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            if (tbPW.Text == STATIC.Rcp.pw.PW) this.DialogResult = DialogResult.OK;
            else MessageBox.Show(this, "Incorrect PW");
        }

        private void btnChangePW_Click(object sender, EventArgs e)
        {
            if (tbCurrPW.Text != STATIC.Rcp.pw.PW) { MessageBox.Show(this, "Incorrect PW"); return; }

            MessageBox.Show(this, "PW Changed");

            STATIC.Rcp.pw.PW = tbChangePW.Text;
            DataIO.SerializeToXMLFile(STATIC.Rcp.pw, STATIC.PasswordDir);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
