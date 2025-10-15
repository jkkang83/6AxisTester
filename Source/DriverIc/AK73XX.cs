using Dln;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace FZ4P
{
    public class AK73XX 
    {
        public Process Process { get { return STATIC.Process; } }
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public DLN Dln { get { return STATIC.Dln; } }
        public string Name { get; set; }
   
        public int AFOriginAddr { get; set; }
        public int XOriginAddr { get; set; }
        public int Y1OriginAddr { get; set; }
        public int Y2OriginAddr { get; set; }
        public int AFSlaveAddr { get; set; }
        public int XSlaveAddr { get; set; }
        public int Y1SlaveAddr { get; set; }
        public int Y2SlaveAddr { get; set; }
        public int FRA_Addr { get; set; }

        public int FRA_AFSlaveAddr { get; set; }
        public int FRA_XSlaveAddr { get; set; }
        public int FRA_Y1SlaveAddr { get; set; }
        public int FRA_Y2SlaveAddr { get; set; }

        public AK73XX()
        {
            Name = "AK73XX";

            AFOriginAddr = 0x0C;
            XOriginAddr = 0x0E;
            Y1OriginAddr = 0x4E;
            Y2OriginAddr = 0x00;

            //1C33
            //AFSlaveAddr = 0x0C;
            //XSlaveAddr = 0x0E;
            //Y1SlaveAddr = 0x4E;
            //Y2SlaveAddr = 0x6C;
            //FRA_Addr = 0x14;
            //FRA_AFSlaveAddr = 0x18;
            //FRA_XSlaveAddr = 0x1C;
            //FRA_Y1SlaveAddr = 0x9C;
            //FRA_Y2SlaveAddr = 0xD8;


            //SU2810
            AFSlaveAddr = 0x28;
            XSlaveAddr = 0x70;
            Y1SlaveAddr = 0x30;
            Y2SlaveAddr = 0x00;
            FRA_Addr = 0x14;           
            FRA_AFSlaveAddr = 0x50;
            FRA_XSlaveAddr = 0xE0;
            FRA_Y1SlaveAddr = 0x60;
            FRA_Y2SlaveAddr = 0x00;

        }
        public void OISOn(int ch, string name, bool isOn)
        {
            byte data = 0x00;
            
            if(name.Contains("AF"))
            {
                if (isOn)
                {
                    Process.AddLog(ch, string.Format("AF On"));
                }
                else
                {
                    data = 0x40;
                    Process.AddLog(ch, string.Format("AF Off"));
                }
                if (!Dln.WriteArray(ch, AFSlaveAddr, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} AFData : 0x{1:X2}", 0x02, data));
            }
            else if (name.Contains("X"))
            {
                if (isOn)
                {
                    Process.AddLog(ch, string.Format("OIS X On"));
                }
                else
                {
                    data = 0x40;
                    Process.AddLog(ch, string.Format("OIS X Off"));
                }

                if (!Dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x02, data));
            }
            else if (name.Contains("Y"))
            {
                if (isOn)
                {
                    Process.AddLog(ch, string.Format("OIS Y On"));
                }
                else
                {
                    data = 0x40;
                    Process.AddLog(ch, string.Format("OIS Y Off"));
                }

                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x02, data));

                if(Y2SlaveAddr != 0x00)
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x02, new byte[] { data })) return;
                    Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x02, data));
                }
            }
         
        }
       
      
     
        public bool Move(int ch, string name, int pos, bool openLoop = false)
        {
            int data = pos << 4;
            byte[] buff = new byte[2] { (byte)(data >> 8), (byte)(data % 256) };

            if (name.Contains("AF"))
            {
                if (!Dln.WriteArray(ch, AFSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("X"))
            {
                if (!Dln.WriteArray(ch, XSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y1"))
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y2"))
            {
                if(Y2SlaveAddr != 0x00)
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            else if (name.Contains("Y"))
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
                if (Y2SlaveAddr != 0x00)
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            return true;
        }
        public bool Move_13bit(int ch, string name, int pos, bool openLoop = false)
        {
            int data = pos << 3;
            byte[] buff = new byte[2] { (byte)(data >> 8), (byte)(data % 256) };

            if (name.Contains("AF"))
            {
                if (!Dln.WriteArray(ch, AFSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("X"))
            {
                if (!Dln.WriteArray(ch, XSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y1"))
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y2"))
            {
                if (Y2SlaveAddr != 0x00)
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            else if (name.Contains("Y"))
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, buff)) return false;
                if (Y2SlaveAddr != 0x00)
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            return true;
        }
        public int ReadHall(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;
            else if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;
            else if (name.Contains("Y1") || name.Contains("Y")) addr = Y1SlaveAddr;
         

            byte[] data = new byte[2];

            if (addr != 0x00) Dln.ReadArray(ch, addr, 0x84, data);
            if (name == "Y2" && Y2SlaveAddr != 0x00) Dln.ReadArray(ch, addr, 0x84, data);
        

            return ((data[0] << 8) + data[1]) >> 4;
        }
        public int ReadHallOpenLoop(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;
            else if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;
            else if (name.Contains("Y1") || name.Contains("Y")) addr = Y1SlaveAddr;


            byte[] data = new byte[2];

            if (addr != 0x00) Dln.ReadArray(ch, addr, 0x80, data);
            if (name == "Y2" && Y2SlaveAddr != 0x00) Dln.ReadArray(ch, addr, 0x84, data);


            return ((data[0] << 8) + data[1]) >> 4;
        }
        public int ReadHall_13bit(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;
            else if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y1")) addr = Y1SlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;

            byte[] data = new byte[2];
            if (Y2SlaveAddr != 0x00)
                Dln.ReadArray(ch, addr, 0x84, data);
            return ((data[0] << 8) + data[1]) >> 3;
        }


        public bool FRA_Single(int ch, string name, int amp, int mode, List<double> freq, ref List<double> gain, ref List<double> phase)
        {
            int addr;
            int sAddr;
            string axis;
            if (name.Contains("X"))
            {
                addr = FRA_XSlaveAddr;
                sAddr = XSlaveAddr;
                axis = "X";
            }
            else if (name.Contains("Y1"))
            {
                addr = FRA_Y1SlaveAddr;
                sAddr = Y1SlaveAddr;
                axis = "Y1";
            }
            else if (name.Contains("Y2"))
            {
                addr = FRA_Y2SlaveAddr;
                sAddr = Y2SlaveAddr;
                axis = "Y2";
            }
            else if (name.Contains("AF"))
            {
                addr = FRA_AFSlaveAddr;
                sAddr = AFSlaveAddr;
                axis = "AF";
            }
            else
                return false;

            if(addr != 0x00) SetSlaveAddr(ch, addr);
            byte[] data = new byte[1];

            if (!Dln.WriteArray(ch, sAddr, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(10);
            // Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            if (!Dln.WriteArray(ch, sAddr, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            Dln.ReadArray(ch, sAddr, 0x4B, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x4C, data[0]));


            if ((data[0] & 8) == 8)
            {
                if (!FRAModeDisable(ch)) return false;
            }

            if (!FRAModeEnable(ch, sAddr)) return false;

            if (!Set_Amp(ch, amp)) return false;

            for (int i = 0; i < freq.Count; i++)
            {
                if (!Set_Freq(ch, (int)freq[i])) return false;

                gain.Add(Get_Gain(ch));

                phase.Add(Get_Phase(ch));

                Process.AddLog(ch, string.Format("{0} FRA Freq : {1} gain : {2:0.00} phase : {3:0.00}", axis, freq[i], gain[i], phase[i]));

                if (i > 0)
                {
                    if (mode == 0)
                    {
                        if (gain[i] * gain[i - 1] <= 0 && gain[i - 1] < 0) { Process.AddLog(ch, "Zero Cross Detected."); break; }

                    }
                    else if (mode == 1)
                    {
                        if (phase[i] * phase[i - 1] <= 0 && phase[i - 1] < 0) { Process.AddLog(ch, "Zero Cross Detected."); break; }
                    }
                }

            }

            if (!FRAModeDisable(ch)) return false;

            return true;
        }



        //public bool FRA_Single(int ch, string name, int amp, int mode ,List<double> freq, ref List<double> gain, ref List<double> phase)
        //{
        //    int addr;
        //    int sAddr;
        //    string axis;
        //    if (name.Contains("X"))
        //    {
        //        addr = 0xE0;
        //        sAddr = XSlaveAddr;
        //        axis = "X";
        //    }
        //    else if(name.Contains("Y1"))
        //    {
        //        addr = 0x60;
        //        sAddr = Y1SlaveAddr;
        //        axis = "Y1";
        //    }
        //    else if (name.Contains("Y2"))
        //    {
        //        addr = 0xD8;
        //        sAddr = Y2SlaveAddr;
        //        axis = "Y2";
        //    }
        //    else if (name.Contains("AF"))
        //    {
        //        addr = 0x50;
        //        sAddr = AFSlaveAddr;
        //        axis = "AF";
        //    }
        //    else
        //        return false;

        //    SetSlaveAddr(ch, addr);

        //    byte[] data = new byte[1];

        //    if (!Dln.WriteArray(ch, sAddr, 1, 0x02, new byte[] { 0x40 })) return false;
        //    Thread.Sleep(10);
        //    // Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

        //    if (!Dln.WriteArray(ch, sAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
        //    Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

        //    Dln.ReadArray(ch, sAddr, 1, 0x4B, data);
        //    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x4C, data[0]));


        //    if ((data[0] & 8) == 8)
        //    {
        //        if (!FRAModeDisable(ch)) return false;
        //    }

        //    if (!FRAModeEnable(ch, sAddr)) return false;

        //    if (!Set_Amp(ch, amp)) return false;

        //    for (int i = 0; i < freq.Count; i++)
        //    {
        //        if (!Set_Freq(ch, (int)freq[i])) return false;

        //        gain.Add(Get_Gain(ch));
        //        phase.Add(Get_Phase(ch));
        //        Process.AddLog(ch, string.Format("{0} FRA Freq : {1} gain : {2:0.00} phase : {3:0.00}", axis, freq[i], gain[i], phase[i]));
        //        if(i > 0)
        //        {
        //            if (mode == 0)
        //            {
        //                if (gain[i] * gain[i - 1] <= 0 && gain[i - 1] < 0) { Process.AddLog(ch, "Zero Cross Detected."); break; }

        //            }
        //            else if(mode == 1)
        //            {
        //                if (phase[i] * phase[i - 1] <= 0 && phase[i - 1] < 0) { Process.AddLog(ch, "Zero Cross Detected."); break; }
        //            }
        //        }
        //    }
        //    if (!FRAModeDisable(ch)) return false;

        //    return true;
        //}
      
       

        private bool SetSlaveAddr(int ch, int addr)
        {
            Process.AddLog(ch, string.Format("Set Slave Addr"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x01));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)addr })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x6F, addr));

            return true;
        }
        private bool FRAModeEnable(int ch, int addr)
        {
            Process.AddLog(ch, string.Format("FRA Mode Enable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x56, new byte[] { 0x80 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x56, 0x80));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xAC, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x01));
            Thread.Sleep(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 0x54, new byte[] { 0x0F })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x54, 0x0F));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x55, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x55, 0x00));
            Thread.Sleep(5);

            byte[] data = new byte[1];

            Dln.ReadArray(ch, addr, 0x4B, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x4B, data[0]));

            if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0xC5 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0xC5));
            Thread.Sleep(1000);

            return true;
        }
        public bool FRAModeDisable(int ch)
        {
            Process.AddLog(ch, string.Format("FRA Mode Disable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xAF, new byte[] { 0xEE })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAF, 0xEE));
            Thread.Sleep(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 0xAC, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x00));
            Thread.Sleep(15);

            return true;
        }
        private bool Set_Amp(int ch, int val)
        {
            int data = val << 6;

            if (!Dln.WriteArray(ch, FRA_Addr, 0x52, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X4}", 0x52, data));

            return true;
        }
        private bool Set_Freq(int ch, int val)
        {
            int data = val << 1;

            if (!Dln.WriteArray(ch, FRA_Addr, 0x50, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;

            Thread.Sleep(20000 / val + 10);

            return true;
        }
        private double Get_Gain(int ch)
        {
            byte[] data = new byte[3];
            Dln.ReadArray(ch, FRA_Addr, 0x94, data);
            double val = (data[0] << 16) + (data[1] << 8) + data[2];
            return Math.Log10(val / 65536) * 20;
        }
        private double Get_Phase(int ch)
        {
            byte[] data = new byte[2];
            Dln.ReadArray(ch, FRA_Addr, 0x98, data);
            double val = (data[0] << 8) + data[1];
            val /= 128;
            if (val > 256)
                val -= 512;
            val = 180 + val;
            if (val > 180) val = 360 - val;
            if (val < -180) val += 360;
            return val;
        }

    }
}
