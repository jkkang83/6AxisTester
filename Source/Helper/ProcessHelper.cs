using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Helper
{
    public static class ProcessHelper
    {
        public static void Wait(int ms)
        {
            //       Thread.Sleep(ms);
            ms = ms * 1000;
            Stopwatch startNew = Stopwatch.StartNew();

            long usDelayTick = (ms * Stopwatch.Frequency) / 1000000;

            while (startNew.ElapsedTicks < usDelayTick) ;
        }
    }
}
