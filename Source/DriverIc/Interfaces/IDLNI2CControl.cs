using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Interfaces
{
    public interface IDLNI2CControl
    {
        double GetCurrent(int ch, int mode);
        void SetLEDpower(int id, int value);
        bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data);
        bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data);
    }
}
