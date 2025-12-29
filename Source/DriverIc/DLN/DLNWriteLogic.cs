using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public class DLNWriteLogic
    {
        private readonly Process _process;
        private readonly DLN _dln;
        public DLNWriteLogic(Process process, DLN dln)
        {
            _process = process;
            _dln = dln;
        }

        public bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {
            if (_process.IsVirtual) return true;
            return _dln.WriteArray(ch, slaveAddr, memAddr, data);
        }
    }
}
