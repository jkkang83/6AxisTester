using FZ4P.DriverIc.Control;
using FZ4P.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Interfaces
{
    public interface IDLNIOControl
    {
        void LoadSocket(LoadState state);
        void CoverMove(CoverState state);
        void PowerOnOff(int port, bool IsOn = true);
        void SetSocketSensor(bool isOn);
        void PowerSequence(int port);
        bool GetGpioStatus(int input);
    }
}
