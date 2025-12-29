using FZ4P.Helper;

namespace FZ4P
{
    public class DLNPowerSequenceLogic
    {
        private readonly Process _process;
        private readonly DLN _dln;
        public DLNPowerSequenceLogic(Process process, DLN dLN)
        {
            _process = process;
            _dln = dLN;
        }

        public void PowerSequence(int port,int ch)
        {
            _process.AddLog(ch, $"Power Off");
            _dln.PowerOnOff(0, false);
            ProcessHelper.Wait(200);
            _process.AddLog(ch, $"Power On");
            _dln.PowerOnOff(0, true);
            ProcessHelper.Wait(200);
        }
    }
}
