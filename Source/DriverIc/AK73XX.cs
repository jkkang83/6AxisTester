using Dln;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

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


        #region New Function
        public void AK7314_Mode(int ch, byte mode)
        {
            if (mode == 1) Dln.WriteArray(ch, AFSlaveAddr, 0x02, new byte[] { 0x00 });
            else if (mode == 2) Dln.WriteArray(ch, AFSlaveAddr, 0x02, new byte[] { 0x10 });
            else Dln.WriteArray(ch, AFSlaveAddr, 0x02, new byte[] { 0x40 });
        }

        public void AK7326_IC_Mode(int ch, int axis, byte mode)
        {
            byte option = 0, index;
            if (mode == 0) option = 0x40;
            else if (mode == 1) option = 0x00;
            else if (mode == 2) option = 0x40;
            else if (mode == 3) option = 0x00;
            int slaveaddr = axis == 0 ? XSlaveAddr : Y1SlaveAddr;
            string AxisStr = axis == 0 ? "OIS X" : "OIS Y";
            string modeStr = mode == 0 ? "Standby mode" : "Active mode";
            if(mode == 0 || mode == 1)
            {
                Dln.WriteArray(ch, slaveaddr, 0x02, new byte[] { option });
                Process.AddLog(ch, $"{AxisStr} {modeStr}");
            }
            else
            {
                Dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { option });
                Dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { option });
            }
            if (mode == 2) Process.AddLog(ch, "OIS Standby mode");
            if(mode == 3) Process.AddLog(ch, "OIS Active mode");
        }

        public bool AK7314_memory_update(int ch, byte mode)
        {
            byte value = 0, temp;
            ushort time = 0;
            byte[] check_update = new byte[1];

            switch (mode)
            {
                case 0: value = 0x00; time = 0; break;      // null, AK7314
                case 1: value = 0x01; time = 120; break;        // 1:  90 ms (PIDK,PIDU,PCAL,NCAL,SETTING1~2)
                case 2: value = 0x02; time = 270; break;        // 2:  234 ms (PIDA~PIDX			)
                case 3: value = 0x04; time = 170; break;        // 3:  108 ms (PIDAA~PIAJ			)
                case 4: value = 0x08; time = 110; break;     // 4:  AK7314C
                case 5: value = 0x10; time = 40; break;     // 5:  Mload
                default: break;
            }

            for (temp = 0; temp < 5; temp++)
            {
                Dln.WriteArray(ch, AFSlaveAddr, 0x03, new byte[] { value });
                Process.Wait(time);

                Dln.ReadArray(ch, AFSlaveAddr, 0x4B, check_update);// AK7314_Read_byte(0x4B) & 0x04;
                if (check_update[0] == 0x00)
                    break;
            }
            if(check_update[0] != 0x00)
                return false;
            return true;
        }

        public void AK7314_IC_Data(int ch)
        {
            int Pcal = 0, Ncal = 0, PVT = 0, NVT = 0;

            byte[] rbuf = new byte[1];
            byte[] rbuf2 = new byte[2];
            Dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            AK7314_memory_update(ch, 5);
            Dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.ReadArray(ch, AFSlaveAddr, 0x04, rbuf); Pcal = rbuf[0];
            Dln.ReadArray(ch, AFSlaveAddr, 0x06, rbuf); Ncal = (short)(rbuf[0] | 0xFF00);

            AK7314_check_byte(ch, 0x00, 0x0F);
            AK7314_check_byte(ch, 0x10, 0x1F);
            AK7314_check_byte(ch, 0x20, 0x2F);
            AK7314_check_byte(ch, 0x30, 0x3F);
            AK7314_check_byte(ch, 0x90, 0x99);
            AK7314_check_byte(ch, 0xC0, 0xCF);
            AK7314_check_byte(ch, 0xE0, 0xEF);
            AK7314_check_byte(ch, 0xF0, 0xFF);

            Dln.ReadArray(ch, AFSlaveAddr, 0xFB, rbuf);
            byte PIDVer = (byte)(0x0F & rbuf[0]);
            Dln.ReadArray(ch, AFSlaveAddr, 0x03, rbuf);
            byte ProductID = rbuf[0];
            Process.AddLog(ch, $" ====  AK7314 (Addr:{(AFSlaveAddr << 1).ToString("X2")}, PID Ver:{PIDVer}, Pro ID:{ProductID.ToString("X2")}) ===");
            Process.AddLog(ch, "");
            Process.AddLog(ch, $"PCal : {Pcal}, Ncal : {Ncal}");
            Process.AddLog(ch, $"PVT : {PVT}, NVT : {NVT}");
        }
        public void AK7326_IC_Data(int ch)
        {
            byte PIDVer, ProductID;
            int[] data = new int[2];

            byte[] rbuf = new byte[1];
            byte[] rbuf2 = new byte[2];
            Process.AddLog(ch, "=============== AK7326 IC Data ===============");
            for (int i = 0; i < 2; i++)
            {
                int slaveAddr = i == 0 ? XSlaveAddr : Y1SlaveAddr;
                AK7326_check_byte(ch, i, 0x00, 0x0F);
                AK7326_check_byte(ch, i, 0x10, 0x1F);
                AK7326_check_byte(ch, i, 0x20, 0x2F);
                AK7326_check_byte(ch, i, 0x30, 0x3F);
                AK7326_check_byte(ch, i, 0xE0, 0xEF);
                AK7326_check_byte(ch, i, 0xF0, 0xFF);

                Dln.ReadArray(ch, slaveAddr, 0x04, rbuf2);
                data[0] = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                Dln.ReadArray(ch, slaveAddr, 0x06, rbuf2);
                data[1] = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                Process.AddLog(ch, $"PCal : {data[0]}, Ncal : {data[1]}");
            }
           
        }
        public void AK7326_IC_reset(int ch)
        {
            Move(ch, "X", 2048);
            Move(ch, "Y", 2048);
            OISOn(ch, "X", true);
            OISOn(ch, "Y", true);

        }
        public void AK7314_IC_reset(int ch)
        {
            byte[] rbuf = new byte[1];

            AK7314_Mode(ch, 0);
            Process.Wait(50);
            AK7314_memory_update(ch, 5);
            Move(ch, "AF", 2048);
            AK7314_Mode(ch, 1);
            Dln.ReadArray(ch, AFSlaveAddr, 0x03, rbuf);
            Process.AddLog(ch, $"AF14 was reeet, 0x03 = {rbuf[0].ToString("X2")}");

        }

        void AK7314_check_byte(int ch, byte start, byte end)
        {
            int addr = 0; int index = 0;
            string s = string.Empty;
            byte[] rbuf = new byte[1];
            s += $"0x{start.ToString("X2")}~0x{end.ToString("X2")} : ";

            for (addr = start, index = 0; addr <= end; addr++, index++)
            {
                Dln.ReadArray(ch, AFSlaveAddr, addr, rbuf);
                if ((index & 0x0003) == 0x0000)
                    s += " ";
                s += rbuf[0].ToString("X2");
             
            }
            Process.AddLog(ch, s);

        }
        void AK7326_check_byte(int ch, int axis,  byte start, byte end)
        {
            int addr = 0; int index = 0;
            string s = string.Empty;
            byte[] rbuf = new byte[1];
            int slaveaddr = axis == 0 ? XSlaveAddr : Y1SlaveAddr;
            s += $"0x{start.ToString("X2")}~0x{end.ToString("X2")} : ";

            for (addr = start, index = 0; addr <= end; addr++, index++)
            {
                Dln.ReadArray(ch, slaveaddr, addr, rbuf);
                if ((index & 0x0003) == 0x0000)
                    s += " ";
                s += rbuf[0].ToString("X2");

            }
            Process.AddLog(ch, s);

        }
        public void Ak7314_soft_move(int ch , int pos, int loop)
        {
            int i = 0;
            short soft_step, margin_code, old_code = 0, new_code = 0;
            soft_step = (short)((pos - 2048) / 50);
            margin_code = Math.Abs(soft_step);

            if (margin_code == 0) return;
            for (i = 0, new_code = 2048; i < loop; i++)
            {
                old_code = new_code;
                Move(ch, "AF", 2048); Process.Wait(50);
                Move(ch, "AF", pos - soft_step * 10); Process.Wait(50);
                Move(ch, "AF", pos - soft_step * 2); Process.Wait(20);
                Move(ch, "AF", pos - soft_step * 1); Process.Wait(20);
                Move(ch, "AF", pos - soft_step * 0); Process.Wait(50);
                new_code = (short)(ReadHall(ch, "AF"));
                Process.AddLog(ch, $"af pos(t, c) : {pos}, {new_code}");
                if (Math.Abs((int)(pos - new_code)) <= margin_code)
                    break;

            }

        }
        public bool AK7326_memory_update(int ch, byte dir, int mode)
        {
            int index = 0;
            byte[] MemoryUpdataeAddr = new byte[] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10 };
            int[] MemoryUpdataeTime = new int[] { 0, 160, 270, 160, 100, 60 };
            int slaveaddr = dir == 0 ? XSlaveAddr : Y1SlaveAddr;
            bool res = false;
            byte val = 0;
            byte[] rbuf = new byte[1];
            switch (mode)
            {
                case 0:
                    for (index = 0; index < 5; index++)
                    {
                        Dln.WriteArray(ch, slaveaddr, 0x03, new byte[] { MemoryUpdataeAddr[index + 1] });
                        Process.Wait(MemoryUpdataeTime[index]);
                    }
                    for (index = 0; index < 5; index++)
                    {
                        Dln.ReadArray(ch, slaveaddr, 0x4B, rbuf);
                        val = (byte)(rbuf[0] & 0x04);
                   
                        if (val == 0x00)
                            break;
                    }
                    if ((index > 4))
                    {
                        Process.AddLog(ch, $"-- memory update NG (%c) -- {dir}");
                     
                        return false;
                    }

                    break;
                case 1:
                    Dln.WriteArray(ch, slaveaddr, 0x03, new byte[] { MemoryUpdataeAddr[5] });
                    Process.Wait(MemoryUpdataeTime[5]);
                    break;
                default:
                    break;
            }
            return true;
        }
        public void AK7326_PM_set_slave(int ch, int axis)
        {
            Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x00 });
            if (axis == 0) Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)FRA_XSlaveAddr });
            else if(axis == 1) Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)FRA_Y1SlaveAddr });
            else
            {
                Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)FRA_XSlaveAddr });
                Dln.WriteArray(ch, FRA_Addr, 0x89, new byte[] { (byte)FRA_Y1SlaveAddr });
            }
        }
        public void OIS_drift_test_mode_init(int ch, bool status)
        {
            Move(ch, "X", 2048);
            Move(ch, "Y", 2048);
            OISOn(ch, "X", true);
            OISOn(ch, "Y", true);
            Process.Wait(100);
            if(status) { OISOn(ch, "X", false); OISOn(ch, "Y", false); }
            else { OISOn(ch, "X", true); OISOn(ch, "Y", true); }
            Process.Wait(100);
        }
        public void OIS_drift_test_mode_close(int ch, bool status)
        {
            if (status) { OISOn(ch, "X", false); OISOn(ch, "Y", false); }
        }
        public void AK7326_EEPROM_Writecheck(int ch, byte dir, byte address, byte value)
        {
            byte[] rbuf = new byte[1];
            byte data = 0;
            int slave = dir == 0 ? XSlaveAddr : Y1SlaveAddr;
            while (true)
            {
                Dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, slave, address, new byte[] { value });
                Process.Wait(30);

                data++;
                Dln.ReadArray(ch, slave, 0x4B, rbuf);
                if ((rbuf[0] & 0x04) == 0x00)
                    break;
                if (data > 5)
                    break;
            }
            Dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x00 });
      
        }
        public void AK7314_EEPROM_Writecheck(int ch, byte address, byte value)
        {
            byte data;
            byte[] rbuf = new byte[1];
            Dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, AFSlaveAddr, address, new byte[] { value });
            Process.Wait(30);
            Dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x00 });

         
        }

        #endregion







        public void OISOn(int ch, string name, bool isOn)
        {
            byte data = 0x00;
            
       
            if (name.Contains("X"))
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
                Process.Wait(10);
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
                Process.Wait(10);
            }
            else if(name.Contains("AF"))
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
      
       

        public bool SetSlaveAddr(int ch, int addr)
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
        public bool FRAModeEnable(int ch)
        {
            Process.AddLog(ch, string.Format("FRA Mode Enable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x56, new byte[] { 0x80 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x56, 0x80));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xAC, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x01));
            Process.Wait(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 0x54, new byte[] { 0x0F })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x54, 0x0F));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x55, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x55, 0x00));
            Process.Wait(5);

          
            if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0xC5 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0xC5));
            Process.Wait(1000);

            return true;
        }
        public bool FRAModeDisable(int ch)
        {
            
            Process.AddLog(ch, string.Format("FRA Mode Disable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xAF, new byte[] { 0xEE })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAF, 0xEE));
            Process.Wait(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 0xAC, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x00));
            Process.Wait(15);


            return true;
        }
        public bool Set_Amp(int ch, int val)
        {
            int data = val << 6;

            if (!Dln.WriteArray(ch, FRA_Addr, 0x52, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X4}", 0x52, data));

            return true;
        }
        public bool Set_Freq(int ch, int val)
        {
            int data = val << 1;

            if (!Dln.WriteArray(ch, FRA_Addr, 0x50, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;

            Process.Wait(20000 / val + 10);

            return true;
        }
        public double Get_Gain(int ch)
        {
            byte[] data = new byte[3];
            Dln.ReadArray(ch, FRA_Addr, 0x94, data);
            double val = (data[0] << 16) + (data[1] << 8) + data[2];
            return Math.Log10(val / 65536) * 20;
        }
        public  double Get_Phase(int ch, int mode)
        {
            byte[] data = new byte[2];
            Dln.ReadArray(ch, FRA_Addr, 0x98, data);
            double val = (data[0] << 8) + data[1];
            val /= 128;
            if (val > 256)
                val -= 512;
            val = 180 + val;
            if(mode == 0)
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

       



    }
}
