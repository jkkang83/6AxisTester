using FZ4P.Helper;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FZ4P
{
    /// <summary>
    /// 공통 로직 추가 예정
    /// </summary>
    public class AK73XXBase
    {
        protected readonly Process _process;
        protected readonly DLN _dln;

        public AKSlaveFRAParam _FRA { get; set; }

        protected int AFSlaveAddr { get; set; }
        protected int XSlaveAddr { get; set; }
        protected int Y1SlaveAddr { get; set; }
        protected int Y2SlaveAddr { get; set; }

        public AK73XXBase(Process process, DLN dln, int afSlaveAddr ,int xSlaveAddr, int y1SlaveAddr, int y2SlaveAddr, int fraBourdAddress)
        {
            _process = process;
            _dln = dln;
            AFSlaveAddr = afSlaveAddr;
            XSlaveAddr = xSlaveAddr;
            Y1SlaveAddr = y1SlaveAddr;
            Y2SlaveAddr = y2SlaveAddr;

            initFRAAddress(fraBourdAddress);
        }

        private void initFRAAddress(int fraBourdAddress)
        {
            _FRA = new AKSlaveFRAParam()
            {
                Addr = fraBourdAddress,
                SlaveAddress = new AKSlaveParam()
                {
                    AFSlaveAddr = AFSlaveAddr << 1,
                    XSlaveAddr = XSlaveAddr << 1,
                    Y1SlaveAddr = Y1SlaveAddr << 1,
                    Y2SlaveAddr = Y2SlaveAddr << 1
                }
            };
        }

        public virtual int ReadHall(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;
            else if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;
            else if (name.Contains("Y1") || name.Contains("Y")) addr = Y1SlaveAddr;


            byte[] data = new byte[2];

            if (addr != 0x00) _dln.ReadArray(ch, addr, 0x84, data);
            if (name == "Y2" && Y2SlaveAddr != 0x00) _dln.ReadArray(ch, addr, 0x84, data);

            return ((data[0] << 8) + data[1]) >> 4;
        }
        public virtual bool Move(int ch, string name, int pos, bool openLoop = false)
        {
            int data = pos << 4;
            byte[] buff = new byte[2] { (byte)(data >> 8), (byte)(data % 256) };

            if (name.Contains("AF"))
            {
                if (!_dln.WriteArray(ch, AFSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("X"))
            {
                if (!_dln.WriteArray(ch, XSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y1"))
            {
                if (!_dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y2"))
            {
                if (Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            else if (name.Contains("Y"))
            {
                if (!_dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
                if (Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            return true;
        }
        public virtual void OISOn(int ch, string name, bool isOn)
        {
            byte data = 0x00;


            if (name.Contains("X"))
            {
                if (isOn)
                {
                    _process.AddLog(ch, string.Format("OIS X On"));
                }
                else
                {
                    data = 0x40;
                    _process.AddLog(ch, string.Format("OIS X Off"));
                }

                if (!_dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { data })) return;
                _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x02, data));
                ProcessHelper.Wait(10);
            }
            else if (name.Contains("Y"))
            {
                if (isOn)
                {
                    _process.AddLog(ch, string.Format("OIS Y On"));
                }
                else
                {
                    data = 0x40;
                    _process.AddLog(ch, string.Format("OIS Y Off"));
                }

                if (!_dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { data })) return;
                _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x02, data));

                if (Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, Y2SlaveAddr, 0x02, new byte[] { data })) return;
                    _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x02, data));
                }
                ProcessHelper.Wait(10);
            }

        }
        public virtual void OIS_drift_test_mode_init(int ch, bool status)
        {
            Move(ch, "X", 2048);
            Move(ch, "Y", 2048);
            OISOn(ch, "X", true);
            OISOn(ch, "Y", true);
            ProcessHelper.Wait(100);
            if (status) { OISOn(ch, "X", false); OISOn(ch, "Y", false); }
            else { OISOn(ch, "X", true); OISOn(ch, "Y", true); }
            ProcessHelper.Wait(100);
        }
        public virtual void OIS_drift_test_mode_close(int ch, bool status)
        {
            if (status) { OISOn(ch, "X", false); OISOn(ch, "Y", false); }
        }
        
        public virtual bool Move_13bit(int ch, string name, int pos, bool openLoop = false)
        {
            int data = pos << 3;
            byte[] buff = new byte[2] { (byte)(data >> 8), (byte)(data % 256) };

            if (name.Contains("AF"))
            {
                if (!_dln.WriteArray(ch, AFSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("X"))
            {
                if (!_dln.WriteArray(ch, XSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y1"))
            {
                if (!_dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y2"))
            {
                if (Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            else if (name.Contains("Y"))
            {
                if (!_dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
                if (Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            return true;
        }
        public virtual int ReadHallOpenLoop(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;
            else if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;
            else if (name.Contains("Y1") || name.Contains("Y")) addr = Y1SlaveAddr;


            byte[] data = new byte[2];

            if (addr != 0x00) _dln.ReadArray(ch, addr, 0x80, data);
            if (name == "Y2" && Y2SlaveAddr != 0x00) _dln.ReadArray(ch, addr, 0x84, data);


            return ((data[0] << 8) + data[1]) >> 4;
        }
        public virtual int ReadHall_13bit(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;
            else if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y1")) addr = Y1SlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;

            byte[] data = new byte[2];
            if (Y2SlaveAddr != 0x00)
                _dln.ReadArray(ch, addr, 0x84, data);
            return ((data[0] << 8) + data[1]) >> 3;
        }

        #region FRA 옵션 기능
        public bool FRA_Single(int ch, string name, int amp, int mode, List<double> freq, ref List<double> gain, ref List<double> phase)
        {
            int addr;
            int sAddr;
            string axis;
            if (name.Contains("X"))
            {
                addr = _FRA.SlaveAddress.XSlaveAddr;
                sAddr = XSlaveAddr;
                axis = "X";
            }
            else if (name.Contains("Y1"))
            {
                addr = _FRA.SlaveAddress.Y1SlaveAddr;
                sAddr = Y1SlaveAddr;
                axis = "Y1";
            }
            else if (name.Contains("Y2"))
            {
                addr = _FRA.SlaveAddress.Y2SlaveAddr;
                sAddr = Y2SlaveAddr;
                axis = "Y2";
            }
            else if (name.Contains("AF"))
            {
                addr = _FRA.SlaveAddress.AFSlaveAddr;
                sAddr = AFSlaveAddr;
                axis = "AF";
            }
            else
                return false;

            if (addr != 0x00) SetSlaveAddr(ch, addr);
            byte[] data = new byte[1];

            if (!_dln.WriteArray(ch, sAddr, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(10);
            // _process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            if (!_dln.WriteArray(ch, sAddr, 0xAE, new byte[] { 0x3B })) return false;
            _process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            _dln.ReadArray(ch, sAddr, 0x4B, data);
            _process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x4C, data[0]));


            if ((data[0] & 8) == 8)
            {
                if (!FRAModeDisable(ch)) return false;
            }

            if (!FRAModeEnable(ch)) return false;

            if (!Set_Amp(ch, amp)) return false;
            int oldfreq = (int)freq[0];
            for (int i = 0; i < freq.Count; i++)
            {
                if (!Set_Freq(ch, (int)freq[i])) return false;
                Thread.Sleep((int)(1000 / oldfreq + 5000 / freq[i] + 15));
                oldfreq = (int)freq[i];

                gain.Add(Get_Gain(ch));

                phase.Add(Get_Phase(ch, 0));

                _process.AddLog(ch, string.Format("{0} FRA Freq : {1} gain : {2:0.00} phase : {3:0.00}", axis, freq[i], gain[i], phase[i]));

                if (i > 0)
                {
                    if (mode == 0)
                    {
                        if (gain[i] * gain[i - 1] <= 0 && gain[i - 1] < 0) { _process.AddLog(ch, "Zero Cross Detected."); break; }

                    }
                    else if (mode == 1)
                    {
                        if (phase[i] * phase[i - 1] <= 0 && phase[i - 1] < 0) { _process.AddLog(ch, "Zero Cross Detected."); break; }
                    }
                }

            }

            if (!FRAModeDisable(ch)) return false;

            return true;
        }
        public bool SetSlaveAddr(int ch, int addr)
        {
            _process.AddLog(ch, string.Format("Set Slave Addr"));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0x00, new byte[] { 0x01 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x01));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0x00, new byte[] { 0x00 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x00));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0x6F, new byte[] { (byte)addr })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x6F, addr));

            return true;
        }
        public bool FRAModeEnable(int ch)
        {
            _process.AddLog(ch, string.Format("FRA Mode Enable"));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0x56, new byte[] { 0x80 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x56, 0x80));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0xAC, new byte[] { 0x01 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x01));
            ProcessHelper.Wait(5);

            if (!_dln.WriteArray(ch, _FRA.Addr, 0x54, new byte[] { 0x0F })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x54, 0x0F));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0x55, new byte[] { 0x00 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x55, 0x00));
            ProcessHelper.Wait(5);


            if (!_dln.WriteArray(ch, _FRA.Addr, 0xA8, new byte[] { 0xC5 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0xC5));
            ProcessHelper.Wait(1000);

            return true;
        }

        public bool FRAModeDisable(int ch)
        {

            _process.AddLog(ch, string.Format("FRA Mode Disable"));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0xA8, new byte[] { 0x00 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0x00));
            if (!_dln.WriteArray(ch, _FRA.Addr, 0xAF, new byte[] { 0xEE })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAF, 0xEE));
            ProcessHelper.Wait(5);

            if (!_dln.WriteArray(ch, _FRA.Addr, 0xAC, new byte[] { 0x00 })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x00));
            ProcessHelper.Wait(15);


            return true;
        }

        public bool Set_Amp(int ch, int val)
        {
            int data = val << 6;

            if (!_dln.WriteArray(ch, _FRA.Addr, 0x52, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;
            _process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X4}", 0x52, data));

            return true;
        }
        public bool Set_Freq(int ch, int val)
        {
            int data = val << 1;

            if (!_dln.WriteArray(ch, _FRA.Addr, 0x50, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;

            ProcessHelper.Wait(20000 / val + 10);

            return true;
        }

        public double Get_Gain(int ch)
        {
            byte[] data = new byte[3];
            _dln.ReadArray(ch, _FRA.Addr, 0x94, data);
            double val = (data[0] << 16) + (data[1] << 8) + data[2];
            return Math.Log10(val / 65536) * 20;
        }

        public double Get_Phase(int ch, int mode)
        {
            byte[] data = new byte[2];
            _dln.ReadArray(ch, _FRA.Addr, 0x98, data);
            double val = (data[0] << 8) + data[1];
            val /= 128;
            if (val > 256)
                val -= 512;
            val = 180 + val;
            if (mode == 0)
            {
                if (val > 180) val = 360 - val;
                if (val < -180) val += 360;
            }
            else
            {
                if (val > 180) val = val - 360;
                if (val < -180) val += 360;
            }

            return val;
        }
        #endregion
    }
}
