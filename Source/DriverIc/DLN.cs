using Dln;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public class DLN
    {
        public Process Process { get { return STATIC.Process; } }
        public uint m_PortCount = 0;
        public List<Device> DLNdevice = new List<Device>();
        public Dln.I2cMaster.Port[] DLNi2c;
        public Dln.Gpio.Module[] DLNgpio;

        public event EventHandler SwitchOn = null;
        public event EventHandler SafetyOn = null;
        private bool isSafeOn = false;
        public bool IsSafeOn
        {
            get { return isSafeOn; }
            set { if (value != isSafeOn) { isSafeOn = value; SafetyOn?.Invoke(null, EventArgs.Empty); } }
        }

        private bool IsSwitch = false;
        public bool m_bOccupied = false;
        public bool[] IsLoad = new bool[2] { false, false };
        public DLN()
        {
            if (!Init()) return;
        }
        public bool Init()
        {
            try
            {
                if (DLNdevice.Count > 0)
                    DLNdevice.Clear();

                Library.Connect("localhost", Connection.DefaultPort);

                m_PortCount = Device.Count();

                if (m_PortCount == 0)
                {
                    MessageBox.Show("--- No DLN-series adapters ---.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch /*(Exception ex)*/
            {
                //MessageBox.Show(ex.Message);
                return false;
            }

            for (int i = 0; i < m_PortCount; i++)
            {
                try
                {
                    DLNdevice.Add(Device.Open(i));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Port " + i + " : " + ex.Message + "\n Re-Connect USB Cable!");    // disappeared
                    return false;
                }
            }

            DLNi2c = new Dln.I2cMaster.Port[m_PortCount];
            DLNgpio = new Dln.Gpio.Module[m_PortCount];

            for (int i = 0; i < m_PortCount; i++)
            {
                try
                {
                    if (DLNdevice[i].I2cMaster.Ports[0].Restrictions.MaxReplyCount != Restriction.NotSupported)
                        DLNdevice[i].I2cMaster.Ports[0].MaxReplyCount = 10;

                    if (DLNdevice[i].I2cMaster.Ports[0].Restrictions.Frequency == Restriction.MustBeDisabled)
                        DLNdevice[i].I2cMaster.Ports[0].Enabled = false;

                    DLNdevice[i].I2cMaster.Ports[0].Frequency = 400 * 1000;
                    DLNdevice[i].I2cMaster.Ports[0].Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Port " + i + " : " + ex.Message);
                    return false;
                }
            }
            for (int i = 0; i < m_PortCount; i++)
            {
                // ID
                DLNdevice[i].Gpio.Pins[6].Enabled = true;
                DLNdevice[i].Gpio.Pins[6].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[6].PulldownEnabled = true;
                DLNdevice[i].Gpio.Pins[7].Enabled = true;
                DLNdevice[i].Gpio.Pins[7].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[7].PulldownEnabled = true;

                // 스위치
                DLNdevice[i].Gpio.Pins[8].Enabled = true;
                DLNdevice[i].Gpio.Pins[8].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[8].PulldownEnabled = true;

                //I2C 0x24관련    OIS_RESET
                DLNdevice[i].Gpio.Pins[14].Enabled = true;
                DLNdevice[i].Gpio.Pins[14].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[14].PulldownEnabled = true;

                DLNdevice[i].Gpio.Pins[15].Enabled = true;
                DLNdevice[i].Gpio.Pins[15].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[15].PulldownEnabled = true;

                // 실린더
                DLNdevice[i].Gpio.Pins[24].Enabled = true;
                DLNdevice[i].Gpio.Pins[24].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

                DLNdevice[i].Gpio.Pins[25].Enabled = true;
                DLNdevice[i].Gpio.Pins[25].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

                // FailLED
                DLNdevice[i].Gpio.Pins[26].Enabled = true;
                DLNdevice[i].Gpio.Pins[26].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                                                            // FailLED
                DLNdevice[i].Gpio.Pins[27].Enabled = true;
                DLNdevice[i].Gpio.Pins[27].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

                Thread.Sleep(100);

                int[] res = new int[2];
                res[0] = DLNdevice[i].Gpio.Pins[6].Value;
                int portID = 0;
                if (res[0] == 1)
                    portID++;

                res[1] = DLNdevice[i].Gpio.Pins[7].Value;
                if (res[1] == 1)
                    portID += 2;

                if (portID == 0 || portID == 2) //안전센서는 0만 연결
                {
                    DLNdevice[i].Gpio.Pins[9].Enabled = true;
                    DLNdevice[i].Gpio.Pins[9].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                    DLNdevice[i].Gpio.Pins[9].PulldownEnabled = true;
                }
                else
                {
                    DLNdevice[i].Gpio.Pins[9].Enabled = true;
                    DLNdevice[i].Gpio.Pins[9].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                    DLNdevice[i].Gpio.Pins[9].OutputValue = 1;
                    DLNdevice[i].Gpio.Pins[9].PulldownEnabled = true;
                    //  Driver IC Power On
                }

                int portCount = DLNdevice[i].I2cMaster.Ports.Count;
                if (portCount == 0)
                {
                    MessageBox.Show("Current DLN-series adapter doesn't support I2C Master interface.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                DLNi2c[portID] = DLNdevice[i].I2cMaster.Ports[0];
                DLNgpio[portID] = DLNdevice[i].Gpio;
            }

            //DLNgpio[0].Pins[8].ConditionMetThreadSafe += SWEventHandler; ;
            //DLNgpio[0].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);

            //DLNgpio[0].Pins[9].ConditionMetThreadSafe += SafeEventHandler;
            //DLNgpio[0].Pins[9].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 50);

            //if (DLNgpio.Length <= 2)
            //{
            //    if (DLNgpio[0].Pins[24].OutputValue == 1) IsLoad[0] = false;
            //    else IsLoad[0] = true;
            //}
            //else
            //{
            //    if (DLNgpio[2].Pins[24].OutputValue == 1) IsLoad[1] = false;
            //    else IsLoad[1] = true;
            //}
            return true;
        }
        private void SWEventHandler(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {
            if (e.Value == 1 && !IsSwitch)
            {
                DLNgpio[0].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 50);
                IsSwitch = true;
                SwitchOn?.Invoke(null, EventArgs.Empty);
            }
            else if (e.Value == 0 && IsSwitch)
            {
                DLNgpio[0].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);
                IsSwitch = false;
            }
        }
        private void SafeEventHandler(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {
            if (e.Value == 1)
            {
                DLNgpio[0].Pins[9].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 50);
                IsSafeOn = false;
            }
            else if (e.Value == 0)
            {
                DLNgpio[0].Pins[9].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);
                IsSafeOn = true;
            }
        }
        public void UnloadSocket(int port)
        {
            if (DLNgpio == null) return;
            int ch = port * 2;
            DLNgpio[ch].Pins[24].OutputValue = 1;
            DLNgpio[ch].Pins[25].OutputValue = 0;
            IsLoad[ch] = false;
        }
        public void LoadSocket(int port)
        {
            if (DLNgpio == null) return;
            int ch = port * 2;
            DLNgpio[ch].Pins[24].OutputValue = 0;
            DLNgpio[ch].Pins[25].OutputValue = 1;
            IsLoad[ch] = true;
        }
        public void SetLEDpower(int id, int value)
        {
            byte bufferH = 0;
            byte[] bufferL = new byte[1];

            int lDACaddr = 0x4F;        // A0,A1상태에 따라 ID 변경, 지금은  A0,A1 pull up

            if (value > 4095)
                value = 4095;
            //  기존 single channel dac code
            //   | XXXX | XXXX |  
            //   | XXXX | XXXX | XXXX | 0000 |
            //   | Address | CtrlByte | Value(12bit) |
            bufferH = (byte)(value / 16);
            bufferL[0] = (byte)(value << 4);

            //  기존 single channel dac code
            //bufferH = (byte)(value / 256);
            //bufferL[0] = (byte)(value % 256);


            byte[] left_side = { 0x10 };      //1
            byte[] left_center = { 0x12 };    //2
            byte[] right_side = { 0x14 };     //3
            byte[] right_center = { 0x16 };   //4


            int ch = 0;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                if (id == 1)
                {
                    byte[] datas = { left_side[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 2)
                {
                    byte[] datas = { left_center[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 3)
                {
                    byte[] datas = { right_side[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 4)
                {
                    byte[] datas = { right_center[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                m_bOccupied = false;
            }
            catch
            {
                Init();
                try
                {
                    if (id == 1)
                    {
                        byte[] datas = { left_side[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 2)
                    {
                        byte[] datas = { left_center[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 3)
                    {
                        byte[] datas = { right_side[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 4)
                    {
                        byte[] datas = { right_center[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                }
                catch
                {
                    MessageBox.Show("Fail to LED Power :: Please Check USB Cable");
                    m_bOccupied = false;
                }
                m_bOccupied = false;
            }
        }
        public double GetCurrent(int ch, int mode)
        {
            double res = 0;
            int RegAddr = 0x01;
            byte[] buffer2 = new byte[2];
            try
            {
                if(mode == 0) { DLNi2c[ch + 1].Read(0x40, 1, RegAddr, buffer2); } // AF
                else DLNi2c[ch + 1].Read(0x41, 1, RegAddr, buffer2);
                res = (buffer2[0] * 256 + buffer2[1]) / 10.0;
            }
            catch
            {
                return 0;
            }
            return res;
        }
        public bool WriteArray(int ch, int slaveAddr, int memCnt, int memAddr, byte[] data)
        {
            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                if(Process.IsVirtual)
                {
                    m_bOccupied = false;
                    return true;
                }
                if (DLNi2c[ch+1] != null) DLNi2c[ch + 1].Write(slaveAddr, memCnt, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch
            {
                m_bOccupied = false;
                return false;
            }
        }
        public bool ReadArray(int ch, int slaveAddr, int memCnt, int memAddr, byte[] data)
        {
            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                if (Process.IsVirtual)
                {
                    m_bOccupied = false;
                    return true;
                }
                if (DLNi2c[ch + 1] != null) DLNi2c[ch + 1].Read(slaveAddr, memCnt, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch
            {
                m_bOccupied = false;
                return false;
            }
        }
    }
}
