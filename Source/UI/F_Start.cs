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
    public partial class F_Start : Form
    {
        public F_Start()
        {
            InitializeComponent();
        }

        public void Log(string e)
        {
            if (StartLog.InvokeRequired)
            {
                StartLog.BeginInvoke((MethodInvoker)delegate
                {
                    StartLog.AppendText(e + "\r\n");
                });
            }
            else
                StartLog.AppendText(e + "\r\n");
        }
    }
}
