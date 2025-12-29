using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public class DLNReadLogic
    {
        private readonly Process _process;
        private readonly DLN _dln;

        public DLNReadLogic(Process process, DLN dln)
        {
            _process = process;
            _dln = dln;
        }

        public bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {
            if (_process.IsVirtual) return true;
            return _dln.ReadArray(ch, slaveAddr, memAddr, data);
        }
    }
}
