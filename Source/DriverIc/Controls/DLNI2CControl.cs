using FZ4P.DriverIc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dln.I2cMaster;

namespace FZ4P.DriverIc.Controls
{
    /// <summary>
    /// TODO : 순환 의존성 제거를 위해 해당 로직을 별도 클래스로 분리할 예정
    /// 
    /// </summary>
    public class DLNI2CControl : IDLNI2CControl
    {
        private Port[] _port;
        object I2cLock = new object();
        private bool m_bOccupied = false;

        public DLNI2CControl(Port[] port)
        {
            _port = port;
        }
        public double GetCurrent(int ch, int mode)
        {
            double res = 0;
            int RegAddr = 0x01;
            byte[] buffer2 = new byte[2];
            try
            {
                lock (I2cLock)
                {
                    if (mode == 0) { _port[ch + 1].Read(0x40, 1, RegAddr, buffer2); } // AF
                    else _port[ch + 1].Read(0x41, 1, RegAddr, buffer2);
                }
                res = (buffer2[0] * 256 + buffer2[1]) / 10.0 + 10;
            }
            catch(Exception ex)
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    throw new Exception("Get Current NG", ex);
                }
                return 0;
            }
            return res;
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
                    lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 2)
                {
                    byte[] datas = { left_center[0], bufferH, bufferL[0] };
                    lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 3)
                {
                    byte[] datas = { right_side[0], bufferH, bufferL[0] };
                    lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 4)
                {
                    byte[] datas = { right_center[0], bufferH, bufferL[0] };
                    lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                m_bOccupied = false;
            }
            catch
            {
                //TODO : kkj 이거는 왜그런건지 한번 물어봐야겠다...
                //Init();
                try
                {
                    if (id == 1)
                    {
                        byte[] datas = { left_side[0], bufferH, bufferL[0] };
                        lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 2)
                    {
                        byte[] datas = { left_center[0], bufferH, bufferL[0] };
                        lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 3)
                    {
                        byte[] datas = { right_side[0], bufferH, bufferL[0] };
                        lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 4)
                    {
                        byte[] datas = { right_center[0], bufferH, bufferL[0] };
                        lock (I2cLock) _port[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                }
                catch(Exception ex)
                {
                    m_bOccupied = false;
                    throw new Exception("Fail to LED Power :: Please Check USB Cable", ex);
                }
                m_bOccupied = false;
            }
        }
        public bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {

            try
            {
                //if (Process.IsVirtual) return true;       //이것도 순환참조 위험
                lock (I2cLock)
                {
                    if (_port[ch + 1] != null) _port[ch + 1].Write(slaveAddr, 1, memAddr, data);
                }

                return true;
            }
            catch(Exception ex)
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    throw new Exception("Dln WriteFail", ex);
                }
                return false;
            }
        }
        public bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {

            try
            {
                //if (Process.IsVirtual) return true;           //이것도 순환참조 위험
                lock (I2cLock)
                {
                    if (_port[ch + 1] != null) _port[ch + 1].Read(slaveAddr, 1, memAddr, data);
                }

                return true;
            }
            catch(Exception ex)
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    throw new Exception("Dln ReadFail", ex);
                }
                return false;
            }
        }
    }
}
