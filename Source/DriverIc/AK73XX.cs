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
        public int XOriginAddr { get; set; }
        public int Y1OriginAddr { get; set; }
        public int Y2OriginAddr { get; set; }
        public int AFSlaveAddr { get; set; }
        public int XSlaveAddr { get; set; }
        public int Y1SlaveAddr { get; set; }
        public int Y2SlaveAddr { get; set; }
        public int FRA_Addr { get; set; }
        public AK73XX()
        {
            Name = "AK73XX";
            XOriginAddr = 0x0A;
            Y1OriginAddr = 0x0E;
            Y2OriginAddr = 0x4E;

            //1C33
            AFSlaveAddr = 0x0C;
            XSlaveAddr = 0x0E;
            Y1SlaveAddr = 0x4E;
            Y2SlaveAddr = 0x6C;
            FRA_Addr = 0x14;

            //SU2810
            //AFSlaveAddr = 0x28;
            //XSlaveAddr = 0x70;
            //Y1SlaveAddr = 0x30;
            //Y2SlaveAddr = 0x00;
            //FRA_Addr = 0x14;
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
       
        public bool ChangeSlaveAddr(int ch)
        {
            // Y2 : 4E -> 6C
            // Y1 : 0E -> 4E
            // X  : 0A -> 0E
            byte[] rDdata = new byte[1];
            Dln.ReadArray(ch, AFSlaveAddr, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            Dln.ReadArray(ch, XSlaveAddr, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            Dln.ReadArray(ch, Y1SlaveAddr, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            Dln.ReadArray(ch, Y2SlaveAddr, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            bool bChange = true;

            if (Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAE, new byte[] { 0x3B })) bChange = false; }
         
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAE, new byte[] { 0x3B })) bChange = false;           
            if (!Dln.WriteArray(ch, XSlaveAddr, 0xAE, new byte[] { 0x3B })) bChange = false;
            if(bChange)
            {
                Process.AddLog(ch, string.Format("Already Slave Address Changed..", 0xAE, 0x3B));
                return true;
            }

            if (Y2SlaveAddr != 0x00)
            {
                //Y2 Change ==
                if (!Dln.WriteArray(ch, Y2OriginAddr, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, Y2OriginAddr, 0x0B, new byte[] { 0x04 })) return false; // 02 : Normal, 04 : Reverse
                Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x04));

                if (!Dln.WriteArray(ch, Y2OriginAddr, 0x0A, new byte[] { 0x30 })) return false; // Setting Slave Address
                Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x30));

                Thread.Sleep(200);

                if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory

                Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                Process.AddLog(ch, string.Format("Y2 SlaveAddr Change FinIsh."));
            }
       

            //Y1 Change ==
            if (!Dln.WriteArray(ch, Y1OriginAddr, 0xAE, new byte[] { 0x3B })) return false;

            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, Y1OriginAddr, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0B, 0x03));

            if (!Dln.WriteArray(ch, Y1OriginAddr, 0x0A, new byte[] { 0x80 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0A, 0x80));

            Thread.Sleep(200); 

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("Y1 SlaveAddr Change FinIsh."));

            //X Change ==
            if (!Dln.WriteArray(ch, XOriginAddr, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, XOriginAddr, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0B, 0x02));

            if (!Dln.WriteArray(ch, XOriginAddr, 0x0A, new byte[] { 0xF0 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0A, 0xF0));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, XSlaveAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("X SlaveAddr Change FinIsh."));

            return true;
        }
        public bool RestoreSlaveAddr(int ch)
        {
            // X  : 0E -> 0A
            // Y1 : 4E -> 0E
            // Y2 : 6C -> 4E

            bool bChange = true;
            if (Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, Y2OriginAddr, 0xAE, new byte[] { 0x3B })) bChange = false; } 
            if (!Dln.WriteArray(ch, Y1OriginAddr, 0xAE, new byte[] { 0x3B })) bChange = false;
            if (!Dln.WriteArray(ch, XOriginAddr, 0xAE, new byte[] { 0x3B })) bChange = false;
            if (bChange)
            {
                Process.AddLog(ch, string.Format("Already Slave Address Restored..", 0xAE, 0x3B));
                return true;
            }

            //X Restore ==
            if (!Dln.WriteArray(ch, XSlaveAddr, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, XSlaveAddr, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0B, 0x02));

            if (!Dln.WriteArray(ch, XSlaveAddr, 0x0A, new byte[] { 0x00 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0A, 0x00));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, XOriginAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("X SlaveAddr Restore FinIsh."));

            //Y1 Restore ==
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAE, new byte[] { 0x3B })) return false;

            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0B, 0x03));

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x0A, new byte[] { 0x00 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0A, 0x00));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, Y1OriginAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("Y1 SlaveAddr Restore FinIsh."));


            if(Y2SlaveAddr != 0x00)
            {
                //Y2 Restore ==
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x0B, new byte[] { 0x04 })) return false; // 02 : Normal, 04 : Reverse
                Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x04));

                if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x0A, new byte[] { 0x00 })) return false; // Setting Slave Address
                Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x00));

                Thread.Sleep(200);

                if (!Dln.WriteArray(ch, Y2OriginAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory

                Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                Process.AddLog(ch, string.Format("Y2 SlaveAddr Restore FinIsh."));
            }

          

            return true;
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
            else if (name.Contains("Y1")) addr = Y1SlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;

            byte[] data = new byte[2];
            if(Y2SlaveAddr != 0x00)
                Dln.ReadArray(ch, addr, 0x84, data);
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
                addr = 0x1C;
                sAddr = XSlaveAddr;
                axis = "X";
            }
            else if (name.Contains("Y1"))
            {
                addr = 0x9C;
                sAddr = Y1SlaveAddr;
                axis = "Y1";
            }
            else if (name.Contains("Y2"))
            {
                addr = 0xD8;
                sAddr = Y2SlaveAddr;
                axis = "Y2";
            }
            else if (name.Contains("AF"))
            {
                addr = 0x18;
                sAddr = AFSlaveAddr;
                axis = "AF";
            }
            else
                return false;

            SetSlaveAddr(ch, addr);

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
        public bool Sine_Wave_Test(int ch, string name, int mode, int SIN_THD, int CNT_ERR, int SIN_FREQ, int SIN_AMP, int SIN_CYCL, ref List<int> result)
        {
            byte addr = 0x1C;
            try
            {
                if (name.Contains("X"))
                {
                    addr = 0x1C;
                    if (!Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { addr })) return false;
                    if (mode == 0)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x51 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x53 })) return false;
                    }
                }
                else if (name.Contains("Y1"))
                {
                    addr = 0x9C;
                    if (!Dln.WriteArray(ch, FRA_Addr, 0x89, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x52 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x53 })) return false;
                    }
                }
                else if (name.Contains("Y2"))
                {
                    addr = 0xD8;
                    if (!Dln.WriteArray(ch, FRA_Addr, 0x89, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x52 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x53 })) return false;
                    }
                }
                else
                    return false;

                Thread.Sleep(2);
                if (!Dln.WriteArray(ch, FRA_Addr, 0x60, new byte[] { (byte)SIN_THD })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x61, new byte[] { (byte)CNT_ERR })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x62, new byte[] { (byte)SIN_FREQ })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x63, new byte[] { (byte)SIN_AMP })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x64, new byte[] { (byte)SIN_CYCL })) return false;

                string tmpStr = "Sine thr = " + SIN_THD + "\r\n"
                    + "Sine Cnt Error = " + CNT_ERR + "\r\n"
                    + "Sine Freq = " + SIN_FREQ + "\r\n"
                    + "Sine Amp = " + SIN_AMP + "\r\n"
                    + "Sine Cycle = " + SIN_CYCL;

                Process.AddLog(ch, tmpStr);

                if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0xC5 })) return false;

                double LimitTime = 50 + ((double)(((SIN_CYCL >> 4) & 0x0F) + (SIN_CYCL & 0x0F)) / SIN_FREQ * 1000);
                Process.AddLog(ch, string.Format("SinewWave Test Time = {0} ms", LimitTime.ToString("F3")));
                Thread.Sleep((int)LimitTime);
                if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0x00 })) return false;
                Thread.Sleep(1);

                if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x00 })) return false;
                Thread.Sleep(2);

                byte[] data = new byte[1];
                byte[] data2 = new byte[2];

                if (name.Contains("X"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 0xE4, data2);
                    result.Add(data2[0] + (data2[1] << 8));
                    Process.AddLog(ch, string.Format("X SineWave Max Count = {0}", result[0]));
                    Dln.ReadArray(ch, FRA_Addr, 0x6E, data); 
                    result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Sinewave Result = {0}", data[0]));

                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 0xE6, data2);
                    result.Add(data2[0] + (data2[1] << 8));
                    Process.AddLog(ch, string.Format("Y1 SineWave Max Count = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Sinewave Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 0xE6, data2);
                    result.Add(data2[0] + (data2[1] << 8));
                    Process.AddLog(ch, string.Format("Y2 SineWave Max Count = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Sinewave Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;

                return true;
            }
            catch
            {
                if (name.Contains("X"))
                {
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;

                return false;
            }
        }
        public bool Ringing_Test(int ch, string name, int mode, int RNG_THD, int RNG_STVT, int RNG_METM, int RNG_WSEC, ref List<int> result)
        {
            byte addr = 0x1C;
            try
            {
                if (name.Contains("X"))
                {
                    addr = 0x1C;
                    if (!Dln.WriteArray(ch, FRA_Addr , 0x6F, new byte[] { addr })) return false;
                    if (mode == 0)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x21 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x23 })) return false;
                    }
                }
                else if (name.Contains("Y1"))
                {
                    addr = 0x9C;
                    if (!Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x22 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x23 })) return false;
                    }
                }
                else if (name.Contains("Y2"))
                {
                    addr = 0xD8;
                    if (!Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x22 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x23 })) return false;
                    }
                }
                else
                    return false;

                Thread.Sleep(2);
                if (!Dln.WriteArray(ch, FRA_Addr, 0x65, new byte[] { (byte)RNG_THD })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x66, new byte[] { (byte)RNG_STVT })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x68, new byte[] { (byte)RNG_METM })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 0x69, new byte[] { (byte)RNG_WSEC })) return false;

                string tmpStr = "Rng thr = " + RNG_THD + "\r\n"
                              + "Rng Start Position = " + RNG_STVT + "\r\n"
                              + "Rng METM = " + RNG_METM + "\r\n"
                              + "Rng WSEC = " + RNG_WSEC;

                Process.AddLog(ch, tmpStr);

                if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0xC5 })) return false;

                double LimitTime = 100 + RNG_METM + RNG_WSEC;
                Process.AddLog(ch, string.Format("Ringing Test Time = {0} ms", LimitTime.ToString("F3")));
                Thread.Sleep((int)LimitTime);
                if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0x00 })) return false;
                Thread.Sleep(1);
                if (!Dln.WriteArray(ch, FRA_Addr, 0xAD, new byte[] { 0x00 })) return false;
                Thread.Sleep(2);

                byte[] data = new byte[1];

                if (name.Contains("X"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 0x9C, data);
                    Process.AddLog(ch, string.Format("RNG OK X = {0}", data[0]));
                    result.Add(RNG_METM + RNG_WSEC - data[0]);
                    Process.AddLog(ch, string.Format("Ringing Time X = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Ringing Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 0x9D, data);
                    Process.AddLog(ch, string.Format("RNG OK Y1 = {0}", data[0]));
                    result.Add(RNG_METM + RNG_WSEC - data[0]);
                    Process.AddLog(ch, string.Format("Ringing Time Y1 = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Ringing Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 0x9D, data);
                    Process.AddLog(ch, string.Format("RNG OK Y2 = {0}", data[0]));
                    result.Add(RNG_METM + RNG_WSEC - data[0]);
                    Process.AddLog(ch, string.Format("Ringing Time Y2 = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Ringing Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;
                return true;
            }
            catch
            {
                if (name.Contains("X"))
                {
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;

                return false;
            }
        }
       

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
