namespace FZ4P
{
    public class DLNPowerOnOffLogic
    {
        private readonly Process _process;
        private readonly DLN _dln;
        public DLNPowerOnOffLogic(Process process, DLN dln)
        {
            _process = process;
            _dln = dln;
        }
        /// <summary>
        /// Port 제품 전원 기준
        /// ch Port1개에 나뉜 채널 기준
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ch"></param>
        /// <param name="IsOn"></param>
        public void PowerOnOff(int port,int ch, bool IsOn = true)
        {
            if(IsOn) _process.AddLog(ch, $"Power On");
            else     _process.AddLog(ch, $"Power Off");

            _dln.PowerOnOff(port, IsOn);
        }
    }
}
