using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Bootstrapper
{
    public class DefaultAppBootStrapper : IAppBootStrapper
    {
        public void Run()
        {
            STATIC.Enable();
            var mainForm = new F_Main();
            Application.Run(mainForm);
        }
    }
}
