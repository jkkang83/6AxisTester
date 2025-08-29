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

            AFSlaveAddr = 0x0C;
            XSlaveAddr = 0x0E;
            Y1SlaveAddr = 0x4E;
            Y2SlaveAddr = 0x6C;
            FRA_Addr = 0x14;
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
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x02, new byte[] { data })) return;
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

                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { data })) return;
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

                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x02, data));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x02, data));
            }
            else if(name.Contains("ALL"))
            {
                if (isOn)
                {
                    Process.AddLog(ch, string.Format("All On"));

                }
                else
                {
                    data = 0x40;
                    Process.AddLog(ch, string.Format("All Off"));
                }

                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x02, new byte[] { data })) return;
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { data })) return;
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x02, new byte[] { data })) return;
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x02, new byte[] { data })) return;
            }
        }
        public bool Store(int ch, int Mode = 0)
        {
            string axis;
            byte[] data = new byte[1];
            if (Mode == 0) //AF
            {
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x3B));
                axis = "AF";

                Process.AddLog(ch, string.Format("Store {0} Memory", axis));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x01));
                Thread.Sleep(80);

                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x03, new byte[] { 0x02 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x02));
                Thread.Sleep(180);

                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x03, new byte[] { 0x04 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x04));
                Thread.Sleep(180);

                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x03, new byte[] { 0x08 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x08));
                Thread.Sleep(80);

                Dln.ReadArray(ch, AFSlaveAddr, 1, 0x4B, data);
                Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x4B, axis, data[0]));
                if ((data[0] & 0x04) != 0x00)
                {
                    Process.AddLog(ch, string.Format("Store {0} Failed = ", axis));
                    return false;
                }
            }
            else if (Mode == 1) //X
            {
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));
                axis = "X";

                Process.AddLog(ch, string.Format("Store {0} Memory", axis));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x01));
                Thread.Sleep(55);

                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x03, new byte[] { 0x02 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x02));
                Thread.Sleep(127);

                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x03, new byte[] { 0x04 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x04));
                Thread.Sleep(61);

                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x03, new byte[] { 0x10 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x10));
                Thread.Sleep(20);

                Dln.ReadArray(ch, XSlaveAddr, 1, 0x4B, data);
                Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x4B, axis, data[0]));
                if ((data[0] & 0x04) != 0x00)
                {
                    Process.AddLog(ch, string.Format("Store {0} Failed = ", axis));
                    return false;
                }
            }
            else if (Mode == 2) //Y
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x3B));
                //Y1
                axis = "Y1";

                Process.AddLog(ch, string.Format("Store {0} Memory", axis));

                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x01));

                Thread.Sleep(55);
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x03, new byte[] { 0x02 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x02));

                Thread.Sleep(127);
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x03, new byte[] { 0x04 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x04));

                Thread.Sleep(61);
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x03, new byte[] { 0x10 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x10));

                Thread.Sleep(20);

                Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x4B, data);
                Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x4B, axis, data[0]));
                if ((data[0] & 0x04) != 0x00)
                {
                    Process.AddLog(ch, string.Format("Store {0} Failed = ", axis));
                    return false;
                }

                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x3B));
                //Y2
                axis = "Y2";

                Process.AddLog(ch, string.Format("Store {0} Memory", axis));

                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x01));

                Thread.Sleep(55);
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x03, new byte[] { 0x02 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x02));

                Thread.Sleep(127);
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x03, new byte[] { 0x04 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x04));

                Thread.Sleep(61);
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x03, new byte[] { 0x10 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x03, axis, 0x10));

                Thread.Sleep(20);

                Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x4B, data);
                Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x4B, axis, data[0]));
                if ((data[0] & 0x04) != 0x00)
                {
                    Process.AddLog(ch, string.Format("Store {0} Failed = ", axis));
                    return false;
                }
            }
            return true;
        }
        public bool AgingOpenLoop(int ch, string name)
        {
            int loop = Condition.iOLAgingLoop;
            int delay = Condition.iOLAgingDelay;
            int mid = Condition.iOLMidCode;
            int max = Condition.iOLMaxCode;
            int min = Condition.iOLMinCode;
            Process.AddLog(ch, string.Format("Start Aging Open Loop"));

            OISOn(ch, "X", false);
            OISOn(ch, "Y", false);

            //Transfer Open Mode
            //X
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            Thread.Sleep(100);
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xA6, new byte[] { 0x7B })) return false;
            Thread.Sleep(100);
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { 0x00 })) return false;
            Thread.Sleep(100);
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, 1, new byte[] { (byte)(mid & 0xff), (byte)((mid << 6) & 0xff) })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, 1, new byte[] { (byte)(mid & 0xff), (byte)((mid << 6) & 0xff) })) return false;

            for (int i = 0; i < loop; i++)
            {
                if (!Dln.WriteArray(ch, XSlaveAddr, 0x00, 1, new byte[] { (byte)(max & 0xff), (byte)((max << 6) & 0xff) })) return false;
                Thread.Sleep(delay);

                if (!Dln.WriteArray(ch, XSlaveAddr, 0x00, 1, new byte[] { (byte)(min & 0xff), (byte)((min << 6) & 0xff) })) return false;
                Thread.Sleep(delay);
            }
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(100);

            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { 0x00 })) return false;
            Thread.Sleep(100);

            //Y
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

            Thread.Sleep(100);
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xA6, new byte[] { 0x7B })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xA6, new byte[] { 0x7B })) return false;
            Thread.Sleep(100);
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x02, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x02, new byte[] { 0x00 })) return false;
            Thread.Sleep(100);
            if (!Dln.WriteArray(ch, XSlaveAddr, 0x00, 1, new byte[] { (byte)(mid & 0xff), (byte)((mid << 6) & 0xff) })) return false;

            for (int i = 0; i < loop; i++)
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, 1, new byte[] { (byte)(max & 0xff), (byte)((max << 6) & 0xff) })) return false;
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, 1, new byte[] { (byte)(max & 0xff), (byte)((max << 6) & 0xff) })) return false;

                Thread.Sleep(delay);

                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x00, 1, new byte[] { (byte)(min & 0xff), (byte)((min << 6) & 0xff) })) return false;
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x00, 1, new byte[] { (byte)(min & 0xff), (byte)((min << 6) & 0xff) })) return false;

                Thread.Sleep(delay);
            }
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x02, new byte[] { 0x40 })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(100);

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x02, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x02, new byte[] { 0x00 })) return false;
            Thread.Sleep(100);

            return true;
        }
        public bool ChangeSlaveAddr(int ch)
        {
            // Y2 : 4E -> 6C
            // Y1 : 0E -> 4E
            // X  : 0A -> 0E
            byte[] rDdata = new byte[1];
            Dln.ReadArray(ch, AFSlaveAddr, 1, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x03, rDdata);
            Process.AddLog(ch, string.Format("Read 0x03 :  0x{0:X2}", rDdata[0]));
            bool bChange = true;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x3B }))  bChange = false;         
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) bChange = false;           
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) bChange = false;
            if(bChange)
            {
                Process.AddLog(ch, string.Format("Already Slave Address Changed..", 0xAE, 0x3B));
                return true;
            }

            //Y2 Change ==
            if (!Dln.WriteArray(ch, Y2OriginAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, Y2OriginAddr, 1, 0x0B, new byte[] { 0x04 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x04));

            if (!Dln.WriteArray(ch, Y2OriginAddr, 1, 0x0A, new byte[] { 0x30 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x30));

            Thread.Sleep(200); 

            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("Y2 SlaveAddr Change FinIsh."));

            //Y1 Change ==
            if (!Dln.WriteArray(ch, Y1OriginAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, Y1OriginAddr, 1, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0B, 0x03));

            if (!Dln.WriteArray(ch, Y1OriginAddr, 1, 0x0A, new byte[] { 0x80 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0A, 0x80));

            Thread.Sleep(200); 

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("Y1 SlaveAddr Change FinIsh."));

            //X Change ==
            if (!Dln.WriteArray(ch, XOriginAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, XOriginAddr, 1, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0B, 0x02));

            if (!Dln.WriteArray(ch, XOriginAddr, 1, 0x0A, new byte[] { 0xF0 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0A, 0xF0));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x03, new byte[] { 0x01 })) return false; // Store Memory

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
            if (!Dln.WriteArray(ch, Y2OriginAddr, 1, 0xAE, new byte[] { 0x3B })) bChange = false;
            if (!Dln.WriteArray(ch, Y1OriginAddr, 1, 0xAE, new byte[] { 0x3B })) bChange = false;
            if (!Dln.WriteArray(ch, XOriginAddr, 1, 0xAE, new byte[] { 0x3B })) bChange = false;
            if (bChange)
            {
                Process.AddLog(ch, string.Format("Already Slave Address Restored..", 0xAE, 0x3B));
                return true;
            }

            //X Restore ==
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0B, 0x02));

            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0A, new byte[] { 0x00 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0A, 0x00));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, XOriginAddr, 1, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("X SlaveAddr Restore FinIsh."));

            //Y1 Restore ==
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0B, 0x03));

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0A, new byte[] { 0x00 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x0A, 0x00));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, Y1OriginAddr, 1, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("Y1 SlaveAddr Restore FinIsh."));

            //Y2 Restore ==
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0B, new byte[] { 0x04 })) return false; // 02 : Normal, 04 : Reverse
            Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x04));

            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0A, new byte[] { 0x00 })) return false; // Setting Slave Address
            Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x00));

            Thread.Sleep(200);

            if (!Dln.WriteArray(ch, Y2OriginAddr, 1, 0x03, new byte[] { 0x01 })) return false; // Store Memory

            Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
            Process.AddLog(ch, string.Format("Y2 SlaveAddr Restore FinIsh."));

            return true;
        }
        public bool RegisterSetting(int ch, int Mode)
        {
            return true;
        }
        public bool PIDSetting(int ch, List<object[]> param, int Mode = 0)
        {
            byte[] rDdata;
            if (Mode == 0) //AF
            {
                Process.AddLog(ch, string.Format("AF PID Initial Start == "));

                Process.AddLog(ch, string.Format("Go to Standby mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x40));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return false;
                Thread.Sleep(3);

                Process.AddLog(ch, string.Format("Change to Setting mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x3B));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

                Process.AddLog(ch, string.Format("EPA enable & I2C SET setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0B, 0xC2));

                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x0B, new byte[] { 0xC2 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0A, 0x01));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x0A, new byte[] { 0x01 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x08, 0xE1));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x08, new byte[] { 0xE1 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x09, 0x84));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x09, new byte[] { 0x84 })) return false;

                for (int i = 0; i < param.Count; i++)
                {
                    int addr = Convert.ToUInt16(param[i][0].ToString(), 16);
                    int data = Convert.ToUInt16(param[i][1].ToString(), 16);
                    Thread.Sleep(10);
                    if (!Dln.WriteArray(ch, AFSlaveAddr, 1, addr, new byte[] { (byte)data })) return false;
                    Thread.Sleep(10);
                    Process.AddLog(ch, string.Format("Write Pid , Mem : 0x{0:X2} Data : 0x{1:X2}", addr, data));
                }

                Process.AddLog(ch, string.Format("Function register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xCA, 0x46));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xCA, new byte[] { 0x46 })) return false;

                Process.AddLog(ch, string.Format("Function register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xCB, 0xD8));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xCB, new byte[] { 0xD8 })) return false;

                Process.AddLog(ch, string.Format("Function register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xCC, 0x40));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xCC, new byte[] { 0x40 })) return false;

                Process.AddLog(ch, string.Format("Function register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xCD, 0x32));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xCD, new byte[] { 0x32 })) return false;

                Process.AddLog(ch, string.Format("Function register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xCE, 0x00));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xCE, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Function register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x3D, 0x10));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x3D, new byte[] { 0x10 })) return false;

                Process.AddLog(ch, string.Format("Temp. setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xC9, 0x00));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xC9, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Temp. setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x80));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x02, new byte[] { 0x80 })) return false;
                Thread.Sleep(50);

                rDdata = new byte[1];
                Dln.ReadArray(ch, AFSlaveAddr, 1, 0x70, rDdata);
                Process.AddLog(ch, string.Format("Temp. setting == Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x70, rDdata[0]));

                Process.AddLog(ch, string.Format("Temp. setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xC9, rDdata[0]));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xC9, new byte[] { rDdata[0] })) return false;

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0C, 0x62));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x0C, new byte[] { 0x62 })) return false;

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x18));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x02, new byte[] { 0x18 })) return false;
                Thread.Sleep(150);

                rDdata = new byte[1];
                Dln.ReadArray(ch, AFSlaveAddr, 1, 0x19, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x19, rDdata[0]));

                byte[] calData = new byte[1];
                calData[0] = (byte)(((byte)((byte)(rDdata[0] - 0x80) * 2)) + 0x80);
                if (calData[0] > 0x80 || calData[0] < 0xB0)
                {
                    Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x19, calData[0]));
                    if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x19, new byte[] { calData[0] })) return false;
                }
                else
                {
                    Process.AddLog(ch, string.Format("Calibration instruction is net between 80h and B0h"));
                    return false;
                }

                Process.AddLog(ch, string.Format("Product ID == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xF3, 0x1E));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xF3, new byte[] { 0x1E })) return false;
                Thread.Sleep(22);

                if (!Store(ch, 0)) return false;

                Process.AddLog(ch, string.Format("Release Setting mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x00));
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0xAE, new byte[] { 0x00 })) return false;
            }
            else if (Mode == 1) // X
            {
                Process.AddLog(ch, string.Format("X PID Initial Start == "));

                Process.AddLog(ch, string.Format("Change to Setting mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x3B));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0B, 0x12));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0B, new byte[] { 0x12 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0A, 0x05));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0A, new byte[] { 0x05 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0C, 0x62));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0C, new byte[] { 0x62 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0D, 0xC0));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0D, new byte[] { 0xC0 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x08, 0x01));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x08, new byte[] { 0x01 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x09, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x09, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x34, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x34, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x35, 0xC1));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x35, new byte[] { 0xC1 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0E, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0E, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0F, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x0F, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x24, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x24, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x25, 0xFF));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x25, new byte[] { 0xFF })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2F, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2F, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x30, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x30, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x31, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x31, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x3E, 0x9A));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x3E, new byte[] { 0x9A })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xFE, 0x0A));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xFE, new byte[] { 0x0A })) return false;
                Thread.Sleep(20);

                for (int i = 0; i < param.Count; i++)
                {
                    int addr = Convert.ToUInt16(param[i][0].ToString(), 16);
                    int data = Convert.ToUInt16(param[i][1].ToString(), 16);
                    Thread.Sleep(10);
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, addr, new byte[] { (byte)data })) return false;
                    Thread.Sleep(10);
                    Process.AddLog(ch, string.Format("Write Pid , Mem : 0x{0:X2} Data : 0x{1:X2}", addr, data));
                }

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x09));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { 0x09 })) return false;
                Thread.Sleep(150);

                rDdata = new byte[1];
                Dln.ReadArray(ch, XSlaveAddr, 1, 0x19, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x19, rDdata[0]));

                int wData = (int)(rDdata[0] * 0.8);

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x19, wData));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x02, new byte[] { (byte)(rDdata[0] * 0.8) })) return false;

                Dln.ReadArray(ch, XSlaveAddr, 1, 0x04, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x04, rDdata[0]));

                Dln.ReadArray(ch, XSlaveAddr, 1, 0x06, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x06, rDdata[0]));

                if (!Store(ch, 1)) return false;

                Process.AddLog(ch, string.Format("Release Setting mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x00));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x00 })) return false;

            }
            else // Y
            {
                Process.AddLog(ch, string.Format("Y1 PID Initial Start == "));

                Process.AddLog(ch, string.Format("Change to Setting mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x3B));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0B, 0x14));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0B, new byte[] { 0x14 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0A, 0x06));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0A, new byte[] { 0x06 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0C, 0x62));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0C, new byte[] { 0x62 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0D, 0xC0));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0D, new byte[] { 0xC0 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x08, 0x01));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x08, new byte[] { 0x01 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x09, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x09, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x34, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x34, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x35, 0xC1));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x35, new byte[] { 0xC1 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0E, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0E, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0F, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x0F, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x24, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x24, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x25, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x25, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2F, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2F, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x30, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x30, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x31, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x31, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x3E, 0x9A));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x3E, new byte[] { 0x9A })) return false;

                for (int i = 0; i < param.Count; i++)
                {
                    int addr = Convert.ToUInt16(param[i][0].ToString(), 16);
                    int data = Convert.ToUInt16(param[i][1].ToString(), 16);
                    Thread.Sleep(10);
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, addr, new byte[] { (byte)data })) return false;
                    Thread.Sleep(10);
                    Process.AddLog(ch, string.Format("Write Pid , Mem : 0x{0:X2} Data : 0x{1:X2}", addr, data));
                }

                if (!Store(ch, 2)) return false;

                Process.AddLog(ch, string.Format("Release Setting mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAE, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Y2 PID Initial Start == "));

                Process.AddLog(ch, string.Format("Change to Setting mode == Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

                Process.AddLog(ch, string.Format("Change to Setting mode == Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0B, 0x04));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0B, new byte[] { 0x04 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0A, 0x36));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0A, new byte[] { 0x36 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0C, 0x62));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0C, new byte[] { 0x62 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0D, 0xC0));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0D, new byte[] { 0xC0 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x08, 0x01));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x08, new byte[] { 0x01 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x09, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x09, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x34, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x34, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("SETTING register setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x35, 0xC1));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x35, new byte[] { 0xC1 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0E, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0E, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0F, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x0F, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x24, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x24, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x25, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x25, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2F, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2F, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x30, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x30, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x31, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x31, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Register initial setting == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x3E, 0x9A));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x3E, new byte[] { 0x9A })) return false;

                for (int i = 0; i < param.Count; i++)
                {
                    int addr = Convert.ToUInt16(param[i][0].ToString(), 16);
                    int data = Convert.ToUInt16(param[i][2].ToString(), 16);
                    Thread.Sleep(10);
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, addr, new byte[] { (byte)data })) return false;
                    Thread.Sleep(10);
                    Process.AddLog(ch, string.Format("Write Pid , Mem : 0x{0:X2} Data : 0x{1:X2}", addr, data));
                }

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x01));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x02, new byte[] { 0x01 })) return false;

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x01));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x02, new byte[] { 0x01 })) return false;

                Thread.Sleep(150);

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x19, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x19, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Calibration instruction == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x19, new byte[] { 0x00 })) return false;

                rDdata = new byte[1];

                Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x04, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x04, rDdata[0]));

                Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x06, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x06, rDdata[0]));

                Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x04, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x04, rDdata[0]));

                Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x06, rDdata);
                Process.AddLog(ch, string.Format("Calibration instruction == Read Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x06, rDdata[0]));

                if (!Store(ch, 2)) return false;

                Process.AddLog(ch, string.Format("Release Setting mode == Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x00));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x00 })) return false;

                Process.AddLog(ch, string.Format("Release Setting mode == Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x00));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x00 })) return false;
            }
            return true;
        }
        public bool HallCalAxis(int ch, string name, bool readOnly = false)
        {
            int hal1;
            byte[] data;
            byte[] check4;
            byte[] check6;

            if (Condition.HallCalCntl == 0) readOnly = true;

            int sAddr = XSlaveAddr;
            if (name.Contains("Y1"))
            {
                sAddr = Y1SlaveAddr;
            }
            if (name.Contains("Y2"))
            {
                sAddr = Y2SlaveAddr;
            }

            if (!readOnly)
            {
                if (!Dln.WriteArray(ch, sAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));
                Thread.Sleep(100);

                if (!Dln.WriteArray(ch, sAddr, 1, 0x02, new byte[] { 0x09 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0x02, "Y1", 0x09));
                Thread.Sleep(200);

                data = new byte[1];
                Dln.ReadArray(ch, sAddr, 1, 0x19, data);
                Process.AddLog(ch, string.Format(">>>>0x19 {0}", data[0]));

                hal1 = (int)(data[0] * 0.8);

                if (!Dln.WriteArray(ch, sAddr, 1, 0x19, new byte[] { (byte)hal1 })) return false;
                Process.AddLog(ch, string.Format(">>>> Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x19 , hal1));
            }

            check4 = new byte[1];
            Dln.ReadArray(ch, sAddr, 1, 0x04, check4);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1}Data : {2}", 0x04, "Y1", check4[0]));

            check6 = new byte[1];
            Dln.ReadArray(ch, sAddr, 1, 0x06, check6);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1}Data : {2}", 0x06, "Y1", check6[0]));

            return true;
        }
        public bool HallCalibration(int ch, int mode = 0)
        {
            if(Process.Condition.HallCrossOffsetCntlAf == 1)
            {
                OISOn(ch, "AF", true);
                Move(ch, "AF", Condition.HallCrossOffsetAf);
            }
            if (mode == 0)
            {
                OISOn(ch, "X", false);
                HallCalAxis(ch, "Y1");
                HallCalAxis(ch, "Y2");

                OISOn(ch, "Y", false);
                HallCalAxis(ch, "X");

                OISOn(ch, "X", true);
                Move(ch, "X", Condition.iXCrossOffset);
                HallCalAxis(ch, "Y1");
                HallCalAxis(ch, "Y2");

                OISOn(ch, "Y", true);
                Move(ch, "Y", Condition.iYCrossOffset);
                HallCalAxis(ch, "X");
            }
            else if (mode == 1)
            {
                OISOn(ch, "X", false);
                HallCalAxis(ch, "Y1");
                HallCalAxis(ch, "Y2");

                OISOn(ch, "Y", true);
                Move(ch, "Y", Condition.iYCrossOffset);
                HallCalAxis(ch, "X");
            }
            else if (mode == 2)
            {
                OISOn(ch, "X", false);
                HallCalAxis(ch, "Y1");
                HallCalAxis(ch, "Y2");

                OISOn(ch, "Y", false);
                HallCalAxis(ch, "X");
            }

            if (!Store(ch, 1)) return false;
            if (!Store(ch, 2)) return false;

            return true;
        }
        public bool Data_Check(int ch)
        {
            Process.AddLog(ch, string.Format("Go to Standby mode == Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x02, 0x40));
            if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(3);

            List<byte> addrs = new List<byte>();
            List<byte> datas = new List<byte>();

            byte[] rDdata;
            byte addr;
            rDdata = new byte[1];
            addr = 0x0A; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x0B; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x08; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x09; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            for (int i = 0; i < 9; i++)
            { 
                addr = (byte)(0x10 + i); Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]); 
            }
            for (int i = 0; i < 6; i++)
            {
                addr = (byte)(0x1A + i); Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x20 + i); Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 9; i++)
            {
                addr = (byte)(0xC0 + i); Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 5; i++)
            {
                addr = (byte)(0xCA + i); Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            addr = 0x3D; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0xC9; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0xF3; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x19; Dln.ReadArray(ch, AFSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < addrs.Count; i++)
            {
                Process.AddLog(ch, string.Format("AF Data check == Read Mem : 0x{0:X2} Data : 0x{1:X2}", addrs[i], datas[i]));
            }

            addrs = new List<byte>();
            datas = new List<byte>();
            rDdata = new byte[1];

            for (int i = 0; i < 8; i++)
            {
                addr = (byte)(0x08 + i); Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x10 + i); Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x20 + i); Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }

            addr = 0x30; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x31; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x34; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x35; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x36; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x50 + i); Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }

            addr = 0xFE; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x03; Dln.ReadArray(ch, XSlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < addrs.Count; i++)
            {
                Process.AddLog(ch, string.Format("X Data check == Read Mem : 0x{0:X2} Data : 0x{1:X2}", addrs[i], datas[i]));
            }

            addrs = new List<byte>();
            datas = new List<byte>();
            rDdata = new byte[1];

            for (int i = 0; i < 8; i++)
            {
                addr = (byte)(0x08 + i); Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x10 + i); Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x20 + i); Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }

            addr = 0x30; Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x31; Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x34; Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x35; Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x36; Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x50 + i); Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }

            addr = 0x03; Dln.ReadArray(ch, Y1SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < addrs.Count; i++)
            {
                Process.AddLog(ch, string.Format("Y1 Data check == Read Mem : 0x{0:X2} Data : 0x{1:X2}", addrs[i], datas[i]));
            }

            addrs = new List<byte>();
            datas = new List<byte>();
            rDdata = new byte[1];

            for (int i = 0; i < 8; i++)
            {
                addr = (byte)(0x08 + i); Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x10 + i); Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }
            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x20 + i); Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }

            addr = 0x30; Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x31; Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x34; Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x35; Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            addr = 0x36; Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < 16; i++)
            {
                addr = (byte)(0x50 + i); Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);
            }

            addr = 0x03; Dln.ReadArray(ch, Y2SlaveAddr, 1, addr, rDdata); addrs.Add(addr); datas.Add(rDdata[0]);

            for (int i = 0; i < addrs.Count; i++)
            {
                Process.AddLog(ch, string.Format("Y2 Data check == Read Mem : 0x{0:X2} Data : 0x{1:X2}", addrs[i], datas[i]));
            }

            //Process.AddLog(ch, string.Format("All Data Read == "));
            //log = "0x00~0x0F : ";
            //for (int i = 0; i < 16; i++)
            //{
            //    Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x00 + i, rDdata);
            //    log += string.Format("{0:X2}", rDdata[0]);
            //}
            //log += "\r\n0x10~0x1F : ";
            //for (int i = 0; i < 16; i++)
            //{
            //    Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x10 + i, rDdata);
            //    log += string.Format("{0:X2}", rDdata[0]);
            //}
            //log += "\r\n0x20~0x2F : ";
            //for (int i = 0; i < 16; i++)
            //{
            //    Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x20 + i, rDdata);
            //    log += string.Format("{0:X2}", rDdata[0]);
            //}
            //log += "\r\n0xE0~0xEF : ";
            //for (int i = 0; i < 16; i++)
            //{
            //    Dln.ReadArray(ch, Y2SlaveAddr, 1, 0xE0 + i, rDdata);
            //    log += string.Format("{0:X2}", rDdata[0]);
            //}
            //log += "\r\n0xF0~0xFF : ";
            //for (int i = 0; i < 16; i++)
            //{
            //    Dln.ReadArray(ch, Y2SlaveAddr, 1, 0xF0 + i, rDdata);
            //    log += string.Format("{0:X2}", rDdata[0]);
            //}
            //log += "\r\n0x2C~0x2E : ";
            //for (int i = 0; i < 3; i++)
            //{
            //    Dln.ReadArray(ch, Y2SlaveAddr, 1, 0x2C + i, rDdata);
            //    log += string.Format("{0:X2}", rDdata[0]);
            //}
            //Process.AddLog(ch, log);

            return true;
        }
        public bool EPA_Set(int ch, string name, int top, int bottom)
        {
            int sAddr = XSlaveAddr;
            if (name.Contains("Y1"))
            {
                sAddr = Y1SlaveAddr;
            }
            if (name.Contains("Y2"))
            {
                sAddr = Y2SlaveAddr;
            }
            int posvt = -top / 16;
            int negvt = bottom / 16;

            Process.AddLog(ch, string.Format("posvt : {0} negvt : {1}", posvt, negvt));

            byte bPosvt = (byte)(255 - top / 16);
            byte bNegvt = (byte)( bottom / 16);

            if (!Dln.WriteArray(ch, sAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, sAddr, 1, 0x0E, new byte[] { bPosvt })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} posvt Data : 0x{1:X2}", 0x0E, bPosvt));
            if (!Dln.WriteArray(ch, sAddr, 1, 0x0F, new byte[] { bNegvt })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} negvt Data : 0x{1:X2}", 0x0F, bNegvt));

            byte[] data = new byte[1];
            Dln.ReadArray(ch, sAddr, 1, 0x0B, data);
            int wData = data[0] | 0x80;

            if (!Dln.WriteArray(ch, sAddr, 1, 0x0B, new byte[] { (byte)wData })) return false;

            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0B, wData));

            if (!Dln.WriteArray(ch, sAddr, 1, 0x03, new byte[] { 0x01 })) return false;
            Thread.Sleep(90);
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));

            data = new byte[1];
            Dln.ReadArray(ch, sAddr, 1, 0x4B, data);
            if ((data[0] & 0x04) == 0)
                Process.AddLog(ch, string.Format("EPA Stored."));
            else
            {
                Process.AddLog(ch, string.Format("EPA Fail. {0}", data[0]));
                return false;
            }
            return true;
        }
        public bool Ex_EPA_Set(int ch, string name, int top, int bottom)
        {
            if (Process.IsVirtual)
                if (ch == 1) return true;
            int sAddr = XSlaveAddr;
            if (name.Contains("Y1"))
            {
                sAddr = Y1SlaveAddr;
            }
            if (name.Contains("Y2"))
            {
                sAddr = Y2SlaveAddr;
            }

            int posvt = top / 16;
            int negvt = - bottom / 16;

            Process.AddLog(ch, string.Format("posvt : {0} negvt : {1}", posvt, negvt));

            byte bPosvt = (byte)(top / 16);
            byte bNegvt = (byte)(255 - bottom / 16);

            if (!Dln.WriteArray(ch, sAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

            if (!Dln.WriteArray(ch, sAddr, 1, 0x0E, new byte[] { bPosvt })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} posvt Data : 0x{1:X2}", 0x0E, bPosvt));

            if (!Dln.WriteArray(ch, sAddr, 1, 0x0F, new byte[] { bNegvt })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} negvt Data : 0x{1:X2}", 0x0F, bNegvt));

            byte[] data = new byte[1];
            Dln.ReadArray(ch, sAddr, 1, 0x0B, data);
            int wData = data[0] | 0x80;

            if (!Dln.WriteArray(ch, sAddr, 1, 0x0B, new byte[] { (byte)wData })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x0B, wData));

            if(Process.Rcp.Condition.iLinearityCntl == 1)
            {
                byte bData1, bData2, bData3;
                short old_p1, old_p2, old_p3;
                short mid_p1, mid_p2, mid_p3;

                if (Process.IsVirtual)
                {
                    bData1 = 0x4D;
                    bData2 = 0x04;
                    bData3 = 0xF6;
                    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2C, 0x4D));
                    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2D, 0x04));
                    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2E, 0xF6));
                }
                else
                {
                    Dln.ReadArray(ch, sAddr, 1, 0x2C, data); bData1 = data[0];
                    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2C, data[0]));
                    Dln.ReadArray(ch, sAddr, 1, 0x2D, data); bData2 = data[0];
                    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2D, data[0]));
                    Dln.ReadArray(ch, sAddr, 1, 0x2E, data); bData3 = data[0];
                    Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2E, data[0]));
                }


                old_p1 = (short)((bData1 << 1) | ((bData2 & 0x80) >> 7));
                old_p2 = (short)(bData2 & 0x7F);
                old_p3 = bData3;

                Process.AddLog(ch, string.Format("Lin Comp Old Val oldP1 : 0x{0:X2}, oldP2 : 0x{1:X2}, oldP3 : 0x{2:X2}", old_p1, old_p2, old_p3));

                /* 음수 처리 */
                mid_p1 = (short)(old_p1 < 256 ? old_p1 : 0xfe00 | old_p1);
                mid_p2 = (short)(old_p2 < 64 ? old_p2 : 0xff80 | old_p2);
                mid_p3 = (short)(old_p3 < 128 ? old_p3 : 0xff00 | old_p3);

                Process.AddLog(ch, string.Format("Lin Comp Mid Val midP1 : {0}, midP2 : {1}, midP3 : {2}", mid_p1, mid_p2, mid_p3));

                int newP1 = (int)(mid_p1 * (1 + (double)(posvt - negvt) / 256));
                int newP2 = (int)(mid_p2 + mid_p1 * (double)(posvt + negvt) / 256);
                int newP3 = (int)((mid_p3 - negvt) / (1 + (double)(posvt - negvt) / 256));

                Process.AddLog(ch, string.Format("Lin Comp New Val newP1 : {0}, newP2 : {1}, newP3 : {2}", newP1, newP2, newP3));

                bData1 = (byte)((newP1 >> 1) & 0xFF);
                bData2 = (byte)(((newP1 << 7) | newP2) & 0xFF);
                bData3 = (byte)((newP3) & 0xFF);


                if (!Dln.WriteArray(ch, sAddr, 1, 0x2C, new byte[] { bData1 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2C, bData1));

                if (!Dln.WriteArray(ch, sAddr, 1, 0x2D, new byte[] { bData2 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2D, bData2));

                if (!Dln.WriteArray(ch, sAddr, 1, 0x2E, new byte[] { bData3 })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x2E, bData3));

                if (!Store(ch, 1)) return false;
                if (!Store(ch, 2)) return false;
            }

            if (!Dln.WriteArray(ch, sAddr, 1, 0x03, new byte[] { 0x01 })) return false;
            Thread.Sleep(55);
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));

            if (!Dln.WriteArray(ch, sAddr, 1, 0x03, new byte[] { 0x05 })) return false;
            Thread.Sleep(61);
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x05));

            data = new byte[1];
            Dln.ReadArray(ch, sAddr, 1, 0x4B, data);
            if ((data[0] & 0x04) == 0)
                Process.AddLog(ch, string.Format("EPA Stored."));
            else
            {
                Process.AddLog(ch, string.Format("EPA Fail. {0}", data[0]));
                return false;
            }
            return true;
        }
        public bool Move(int ch, string name, int pos, bool openLoop = false)
        {
            int data = pos << 4;
            byte[] buff = new byte[2] { (byte)(data >> 8), (byte)(data % 256) };

            if (name.Contains("AF"))
            {
                if (!Dln.WriteArray(ch, AFSlaveAddr, 1, 0x00, buff)) return false;
            }
            else if(name.Contains("X"))
            {
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x00, buff)) return false;
            }
            else if (name.Contains("Y1"))
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x00, buff)) return false;
            }
            else if (name.Contains("Y2"))
            {
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x00, buff)) return false;
            }
            else if (name.Contains("Y"))
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x00, buff)) return false;
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x00, buff)) return false;
            }
            return true;
        }
        public int ReadHall(int ch, string name)
        {
            int addr = 0x00;
            if (name.Contains("AF")) addr = AFSlaveAddr;           
            else if (name.Contains("X"))  addr = XSlaveAddr;          
            else if (name.Contains("Y1")) addr = Y1SlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;

            byte[] data = new byte[2];
            Dln.ReadArray(ch, addr, 1, 0x84, data);
            return ((data[0] << 8) + data[1]) >> 4;
        }
        public bool FRA_Single(int ch, string name, int amp, List<double> freq, ref List<double> gain, ref List<double> phase)
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
            else if(name.Contains("Y1"))
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

            if (!Dln.WriteArray(ch, sAddr, 1, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(10);
            // Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            if (!Dln.WriteArray(ch, sAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            Dln.ReadArray(ch, sAddr, 1, 0x4B, data);
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
            }

            if (!FRAModeDisable(ch)) return false;
            
            return true;
        }
        public bool Sine_Wave_Test(int ch, string name, int mode, int SIN_THD, int CNT_ERR, int SIN_FREQ, int SIN_AMP, int SIN_CYCL, ref List<int> result)
        {
            byte addr = 0x1C;
            try
            {
                if (name.Contains("X"))
                {
                    addr = 0x1C;
                    if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x6F, new byte[] { addr })) return false;
                    if (mode == 0)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x51 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x53 })) return false;
                    }
                }
                else if (name.Contains("Y1"))
                {
                    addr = 0x9C;
                    if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x89, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x52 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x53 })) return false;
                    }
                }
                else if (name.Contains("Y2"))
                {
                    addr = 0xD8;
                    if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x89, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x52 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x53 })) return false;
                    }
                }
                else
                    return false;

                Thread.Sleep(2);
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x60, new byte[] { (byte)SIN_THD })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x61, new byte[] { (byte)CNT_ERR })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x62, new byte[] { (byte)SIN_FREQ })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x63, new byte[] { (byte)SIN_AMP })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x64, new byte[] { (byte)SIN_CYCL })) return false;

                string tmpStr = "Sine thr = " + SIN_THD + "\r\n"
                    + "Sine Cnt Error = " + CNT_ERR + "\r\n"
                    + "Sine Freq = " + SIN_FREQ + "\r\n"
                    + "Sine Amp = " + SIN_AMP + "\r\n"
                    + "Sine Cycle = " + SIN_CYCL;

                Process.AddLog(ch, tmpStr);

                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xA8, new byte[] { 0xC5 })) return false;

                double LimitTime = 50 + ((double)(((SIN_CYCL >> 4) & 0x0F) + (SIN_CYCL & 0x0F)) / SIN_FREQ * 1000);
                Process.AddLog(ch, string.Format("SinewWave Test Time = {0} ms", LimitTime.ToString("F3")));
                Thread.Sleep((int)LimitTime);
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xA8, new byte[] { 0x00 })) return false;
                Thread.Sleep(1);

                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x00 })) return false;
                Thread.Sleep(2);

                byte[] data = new byte[1];
                byte[] data2 = new byte[2];

                if (name.Contains("X"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 1, 0xE4, data2);
                    result.Add(data2[0] + (data2[1] << 8));
                    Process.AddLog(ch, string.Format("X SineWave Max Count = {0}", result[0]));
                    Dln.ReadArray(ch, FRA_Addr, 1, 0x6E, data); 
                    result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Sinewave Result = {0}", data[0]));

                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 1, 0xE6, data2);
                    result.Add(data2[0] + (data2[1] << 8));
                    Process.AddLog(ch, string.Format("Y1 SineWave Max Count = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 1, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Sinewave Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 1, 0xE6, data2);
                    result.Add(data2[0] + (data2[1] << 8));
                    Process.AddLog(ch, string.Format("Y2 SineWave Max Count = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 1, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Sinewave Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;

                return true;
            }
            catch
            {
                if (name.Contains("X"))
                {
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
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
                    if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x6F, new byte[] { addr })) return false;
                    if (mode == 0)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x21 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x23 })) return false;
                    }
                }
                else if (name.Contains("Y1"))
                {
                    addr = 0x9C;
                    if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x6F, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x22 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x23 })) return false;
                    }
                }
                else if (name.Contains("Y2"))
                {
                    addr = 0xD8;
                    if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x6F, new byte[] { addr })) return false;
                    if (mode == 1)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x22 })) return false;
                    }
                    else if (mode == 2)
                    {
                        if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x23 })) return false;
                    }
                }
                else
                    return false;

                Thread.Sleep(2);
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x65, new byte[] { (byte)RNG_THD })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x66, new byte[] { (byte)RNG_STVT })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x68, new byte[] { (byte)RNG_METM })) return false;
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x69, new byte[] { (byte)RNG_WSEC })) return false;

                string tmpStr = "Rng thr = " + RNG_THD + "\r\n"
                              + "Rng Start Position = " + RNG_STVT + "\r\n"
                              + "Rng METM = " + RNG_METM + "\r\n"
                              + "Rng WSEC = " + RNG_WSEC;

                Process.AddLog(ch, tmpStr);

                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xA8, new byte[] { 0xC5 })) return false;

                double LimitTime = 100 + RNG_METM + RNG_WSEC;
                Process.AddLog(ch, string.Format("Ringing Test Time = {0} ms", LimitTime.ToString("F3")));
                Thread.Sleep((int)LimitTime);
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xA8, new byte[] { 0x00 })) return false;
                Thread.Sleep(1);
                if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAD, new byte[] { 0x00 })) return false;
                Thread.Sleep(2);

                byte[] data = new byte[1];

                if (name.Contains("X"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 1, 0x9C, data);
                    Process.AddLog(ch, string.Format("RNG OK X = {0}", data[0]));
                    result.Add(RNG_METM + RNG_WSEC - data[0]);
                    Process.AddLog(ch, string.Format("Ringing Time X = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 1, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Ringing Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 1, 0x9D, data);
                    Process.AddLog(ch, string.Format("RNG OK Y1 = {0}", data[0]));
                    result.Add(RNG_METM + RNG_WSEC - data[0]);
                    Process.AddLog(ch, string.Format("Ringing Time Y1 = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 1, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Ringing Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    Dln.ReadArray(ch, FRA_Addr, 1, 0x9D, data);
                    Process.AddLog(ch, string.Format("RNG OK Y2 = {0}", data[0]));
                    result.Add(RNG_METM + RNG_WSEC - data[0]);
                    Process.AddLog(ch, string.Format("Ringing Time Y2 = {0}", result[0]));

                    Dln.ReadArray(ch, FRA_Addr, 1, 0x6E, data); result.Add(data[0]);
                    Process.AddLog(ch, string.Format("Ringing Result = {0}", data[0]));
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;
                return true;
            }
            catch
            {
                if (name.Contains("X"))
                {
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y1"))
                {
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else if (name.Contains("Y2"))
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xEE })) return false;
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAF, new byte[] { 0xCE })) return false;
                }
                else
                    return false;

                return false;
            }
        }
        public bool HallDeviation(int ch, int GainFactorX, int GainFactorY)
        {
            byte[] data = new byte[1];
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x10, data);
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x10, new byte[] { (byte)(data[0] * GainFactorX) })) return false;

            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x10, data);
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x10, new byte[] { (byte)(data[0] * GainFactorY) })) return false;

            return true;
        }
        public bool LinearityComp(int ch, string name, List<int> Code, List<double> Stroke, ref List<byte> result)
        {
            byte[] CoefCh = new byte[3];
            double dbMaxError = 0.0;              
            short shErrorTarget = 0;
            double[] LinCode;
            double[] LensPos;

            LinCode = new double[Code.Count];
            LensPos = new double[Code.Count];
            for (int i = 0; i < Code.Count; i++)
            {
                LinCode[i] = Code[i];
                LensPos[i] = Stroke[i];
            }

            Cal_LinComp_Coef(name, LinCode, LensPos, LinCode.Length, ref CoefCh);

            shErrorTarget = Check_LinComp_Coef(LinCode, LensPos, LinCode.Length, CoefCh, ref dbMaxError);
            
            result.Add(CoefCh[0]);
            result.Add(CoefCh[1]);
            result.Add(CoefCh[2]);
            if (name.Contains("X"))
            {
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));
                //Set Linearity value
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2C, new byte[] { CoefCh[0] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2C, CoefCh[0]));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2D, new byte[] { CoefCh[1] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2D, CoefCh[1]));
                if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2E, new byte[] { CoefCh[2] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2E, CoefCh[2]));
            }
            else
            {
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));
                //Set Linearity value
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2C, new byte[] { CoefCh[0] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2C, CoefCh[0]));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2D, new byte[] { CoefCh[1] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2D, CoefCh[1]));
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2E, new byte[] { CoefCh[2] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2E, CoefCh[2]));

                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0xAE, 0x3B));
                //Set Linearity value
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2C, new byte[] { CoefCh[0] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2C, CoefCh[0]));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2D, new byte[] { CoefCh[1] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2D, CoefCh[1]));
                if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2E, new byte[] { CoefCh[2] })) return false;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} : 0x{1:X2}", 0x2E, CoefCh[2]));
            }

            Process.AddLog(ch, string.Format("dbMaxError : {0} , MaxErrorPosition {1}", dbMaxError, shErrorTarget));

            if (!Store(ch, 1)) return false;
            if (!Store(ch, 2)) return false;

            return true;
        }
        public bool LinearityRead(int ch)
        {
            byte[] data = new byte[1];
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x2C, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2C, "X", data[0]));
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x2D, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2D, "X", data[0]));
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x2E, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2E, "X", data[0]));

            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2C, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2C, "Y1", data[0]));
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2D, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2D, "Y1", data[0]));
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2E, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2E, "Y1", data[0]));

            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2C, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2C, "Y2", data[0]));
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2D, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2D, "Y2", data[0]));
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2E, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2E, "Y2", data[0]));
            return true;
        }
        public bool LinearitySave(int ch)
        {
            string log;
            byte[] data = new byte[1];

            Dln.ReadArray(ch, XSlaveAddr, 1, 0x2C, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2C, "X", data[0]));
            log = string.Format("{0:X2}\r", data[0]);
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x2D, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2D, "X", data[0]));
            log += string.Format("{0:X2}\r", data[0]);
            Dln.ReadArray(ch, XSlaveAddr, 1, 0x2E, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2E, "X", data[0]));
            log += string.Format("{0:X2}\r", data[0]);

            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2C, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2C, "Y1", data[0]));
            log += string.Format("{0:X2}\r", data[0]);
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2D, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2D, "Y1", data[0]));
            log += string.Format("{0:X2}\r", data[0]);
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2E, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2E, "Y1", data[0]));
            log += string.Format("{0:X2}\r", data[0]);

            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2C, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2C, "Y2", data[0]));
            log += string.Format("{0:X2}\r", data[0]);
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2D, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2D, "Y2", data[0]));
            log += string.Format("{0:X2}\r", data[0]);
            Dln.ReadArray(ch, Y1SlaveAddr, 1, 0x2E, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} {1} Data : {2:X2}", 0x2E, "Y2", data[0]));
            log += string.Format("{0:X2}\r", data[0]);

            Process.AddLog(ch, string.Format("== Linearty Data Store Path : LineartyData_{0}.txt", ch));
            StreamWriter sw = new StreamWriter(string.Format("LineartyData_{0}.txt", ch));
            sw.WriteLine(log);
            sw.Close();

            return true;
        }
        public bool LinearityDelete(int ch)
        {
            byte[] data = new byte[1];

            Process.AddLog(ch, string.Format("== Linearty Data Delete =="));
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2C, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2D, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2E, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2C, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2D, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2E, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2C, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2D, new byte[] { 0x00 })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2E, new byte[] { 0x00 })) return false;

            LinearityRead(ch);

            return true;
        }
        public bool LinearityRestore(int ch)
        {
            byte[] data = new byte[1];
            string path = string.Format("LineartyData_{0}.txt", ch);
            if (!File.Exists(path)) return false;
            StreamReader sr = new StreamReader(path);
            string[] ReadArry = sr.ReadToEnd().Split('\r');
            sr.Close();

            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2C, new byte[] { (byte)Convert.ToUInt16(ReadArry[0].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2D, new byte[] { (byte)Convert.ToUInt16(ReadArry[1].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0x2E, new byte[] { (byte)Convert.ToUInt16(ReadArry[2].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2C, new byte[] { (byte)Convert.ToUInt16(ReadArry[3].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2D, new byte[] { (byte)Convert.ToUInt16(ReadArry[4].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0x2E, new byte[] { (byte)Convert.ToUInt16(ReadArry[5].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2C, new byte[] { (byte)Convert.ToUInt16(ReadArry[6].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2D, new byte[] { (byte)Convert.ToUInt16(ReadArry[7].ToString(), 16) })) return false;
            if (!Dln.WriteArray(ch, Y2SlaveAddr, 1, 0x2E, new byte[] { (byte)Convert.ToUInt16(ReadArry[8].ToString(), 16) })) return false;

            LinearityRead(ch);

            return true;
        }
        private int Cal_LinComp_Coef(string name, double[] TargetPos, double[] LensPos, int DataCount, ref byte[] Result)
        {
            double[] CoefLin = new double[2];
            double[] CoefQuad = new double[3];
            double[] CoefErr = new double[3];

            if (LensPos[0] - LensPos[DataCount - 1] != 0)
            {
                CoefLin[1] = (TargetPos[0] - TargetPos[DataCount - 1]) / (LensPos[0] - LensPos[DataCount - 1]);
                CoefLin[0] = TargetPos[0] - (CoefLin[1] * LensPos[0]);
            }
            else
                return 1;

            Polynomial(LensPos, TargetPos, 3, ref CoefQuad, DataCount);

            CoefErr[2] = CoefQuad[2];
            CoefErr[1] = CoefQuad[1] - CoefLin[1];
            CoefErr[0] = CoefQuad[0] - CoefLin[0];

            double[] LensError = new double[DataCount];

            for (int i = 0; i < DataCount; i++)
            {
                LensError[i] = CoefErr[2] * LensPos[i] * LensPos[i]
                                            + CoefErr[1] * LensPos[i] + CoefErr[0];
            }

            double[] dbCoefComp = new double[3];
            Polynomial(TargetPos, LensError, 3, ref dbCoefComp, DataCount);
            short[] shCoefComp = new short[3];
            shCoefComp[0] = (short)AKM_Round(dbCoefComp[2] * 1048576.0f);
            shCoefComp[1] = (short)AKM_Round(dbCoefComp[1] * 256.0f + (double)shCoefComp[0]);
            shCoefComp[2] = (short)AKM_Round(dbCoefComp[0] / 8.0f);

            if(name.Contains("2"))
            {
                // Convert for AK7322C registor.
                Result[0] = (byte)(shCoefComp[0] >> 1);
                Result[1] = (byte)((shCoefComp[0] & 0x0001) << 7);
                Result[2] = 0;
            }
            else
            {
                // Convert for AK7321 registor.
                Result[0] = (byte)(shCoefComp[0] >> 1);
                Result[1] = (byte)(((shCoefComp[0] & 0x0001) << 7) + (shCoefComp[1] & 0x7f));
                Result[2] = (byte)shCoefComp[2];
            }


            return 0;
        }
        private short Check_LinComp_Coef(double[] TargetPos, double[] LensPos, int DataCount, byte[] ucResultCoef, ref double dbMaxError)
        {
            double[] dbCheckCoefLin = new double[2];
            double[] dbCheckCoefQuad = new double[3];

            if (TargetPos[0] - TargetPos[DataCount - 1] != 0)
            {
                dbCheckCoefLin[1] = (LensPos[0] - LensPos[DataCount - 1]) / (TargetPos[0] - TargetPos[DataCount - 1]);
                dbCheckCoefLin[0] = LensPos[0] - (dbCheckCoefLin[1] * TargetPos[0]);
            }
            else
            {
                return -1;  // Error
            }

            Polynomial(TargetPos, LensPos, 3, ref dbCheckCoefQuad, DataCount);

            short[] shCoefComp = new short[3];
            shCoefComp[0] = (short)(((short)((char)ucResultCoef[0]) << 1) + (short)((byte)(ucResultCoef[1] & 0x80) >> 7));
            shCoefComp[1] = (short)((char)((ucResultCoef[1] & 0x7f) << 1) >> 1);
            shCoefComp[2] = (short)((char)ucResultCoef[2]);

            short shErrorTarget = 0;

            for (int i = 0; i < 4096; i++)
            {
                // Expected lens position.
                double dbExpectLens = (dbCheckCoefLin[1] * i + dbCheckCoefLin[0]);

                // Target code after compensation.
                double dbCompTarget = (int)((i + (((double)shCoefComp[0]) / 1048576.0f) * i * i
                                            + ((double)(shCoefComp[1] - shCoefComp[0]) / 256.0f) * i + (double)shCoefComp[2] * 8.0f) + 0.5f);

                if (dbCompTarget < 0.0f)
                {
                    dbCompTarget = 0.0f;
                }
                else if (dbCompTarget > 4095.0f)
                {
                    dbCompTarget = 4095.0f;
                }

                // Lens position after compensation.
                double dbCompLens = (dbCheckCoefQuad[2] * (int)dbCompTarget * (int)dbCompTarget + dbCheckCoefQuad[1] * (int)dbCompTarget + dbCheckCoefQuad[0]);

                // Difference between expected lens positon and lens positon after compensation.
                double dbError = dbExpectLens - dbCompLens;

                if (dbError > dbMaxError)
                {
                    shErrorTarget = (short)i;
                    dbMaxError = dbError;
                }
            }

            return shErrorTarget;
        }
        private bool Setting_Mode(int ch, byte mode)
        {
            Thread.Sleep(100);

            if (!Dln.WriteArray(ch, XSlaveAddr, 1, 0xAE, new byte[] { mode })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, mode));

            if (!Dln.WriteArray(ch, Y1SlaveAddr, 1, 0xAE, new byte[] { mode })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0xAE, mode));

            return true;
        }
        private bool SetSlaveAddr(int ch, int addr)
        {
            Process.AddLog(ch, string.Format("Set Slave Addr"));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x00, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x01));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x00, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x6F, new byte[] { (byte)addr })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x6F, addr));

            return true;
        }
        private bool FRAModeEnable(int ch, int addr)
        {
            Process.AddLog(ch, string.Format("FRA Mode Enable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x56, new byte[] { 0x80 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x56, 0x80));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAC, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x01));
            Thread.Sleep(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x54, new byte[] { 0x0F })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x54, 0x0F));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x55, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x55, 0x00));
            Thread.Sleep(5);

            byte[] data = new byte[1];

            Dln.ReadArray(ch, addr, 1, 0x4B, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x4B, data[0]));

            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xA8, new byte[] { 0xC5 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0xC5));
            Thread.Sleep(1000);

            return true;
        }
        private bool FRAModeDisable(int ch)
        {
            Process.AddLog(ch, string.Format("FRA Mode Disable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xA8, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAF, new byte[] { 0xEE })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAF, 0xEE));
            Thread.Sleep(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0xAC, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x00));
            Thread.Sleep(15);

            return true;
        }
        private bool Set_Amp(int ch, int val)
        {
            int data = val << 6;

            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x52, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X4}", 0x52, data));

            return true;
        }
        private bool Set_Freq(int ch, int val)
        {
            int data = val << 1;

            if (!Dln.WriteArray(ch, FRA_Addr, 1, 0x50, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;

            Thread.Sleep(20000 / val + 10);

            return true;
        }
        private double Get_Gain(int ch)
        {
            byte[] data = new byte[3];
            Dln.ReadArray(ch, FRA_Addr, 1, 0x94, data);
            double val = (data[0] << 16) + (data[1] << 8) + data[2];
            return Math.Log10(val / 65536) * 20;
        }
        private double Get_Phase(int ch)
        {
            byte[] data = new byte[2];
            Dln.ReadArray(ch, FRA_Addr, 1, 0x98, data);
            double val = (data[0] << 8) + data[1];
            val /= 128;
            if (val > 256)
                val -= 512;
            return val;
        }
        private void Polynomial(double[] DataX, double[] DataY, int numCoef, ref double[] Coef, int DataCount)
        {
            double[,] arrayLSM = new double[3, 4];

            for (int i = 0; i < numCoef; i++)
            {
                for (int j = 0; j < numCoef + 1; j++)
                {
                    arrayLSM[i, j] = 0;
                }

            }
            for (int i = 0; i < numCoef; i++)
            {
                for (int j = 0; j < numCoef; j++)
                {
                    for (int k = 0; k < DataCount; k++)
                    {
                        arrayLSM[i, j] += AKM_Pow(DataX[k], i + j);
                    }
                }
            }
            for (int i = 0; i < numCoef; i++)
            {
                for (int k = 0; k < DataCount; k++)
                {
                    arrayLSM[i, numCoef] += (AKM_Pow(DataX[k], i) * DataY[k]);
                }
            }
            Gauss(arrayLSM, numCoef, ref Coef);
        }
        private void Gauss(double[,] arrayLSM, int numCoef, ref double[] dbCoef)
        {
            // Sort
            for (int i = 0; i < numCoef; i++)
            {
                double dbMax = 0;
                int iPivot = i;

                // Search for the largest row.
                for (int l = i; l < numCoef; l++)
                {
                    if (dbMax < AKM_Abs(arrayLSM[l, i]))
                    {
                        dbMax = AKM_Abs(arrayLSM[l, i]);
                        iPivot = l;
                    }
                }

                // Replacing rows
                if (iPivot != i)
                {
                    double dbTemp = 0;
                    for (int j = 0; j < numCoef + 1; j++)
                    {
                        dbTemp = arrayLSM[i, j];
                        arrayLSM[i, j] = arrayLSM[iPivot, j];
                        arrayLSM[iPivot, j] = dbTemp;
                    }
                }
            }

            // Forward elimination
            for (int k = 0; k < numCoef; k++)
            {
                double dbTemp1 = arrayLSM[k, k];
                arrayLSM[k, k] = 1;

                for (int j = k + 1; j < numCoef + 1; j++)
                {
                    arrayLSM[k, j] /= dbTemp1;
                }

                for (int i = k + 1; i < numCoef; i++)
                {
                    double dbTemp2 = arrayLSM[i, k];

                    for (int j = k + 1; j < numCoef + 1; j++)
                    {
                        arrayLSM[i, j] -= dbTemp2 * arrayLSM[k, j];
                    }

                    arrayLSM[i, k] = 0;
                }
            }

            // backward substitution
            for (int i = numCoef - 1; i >= 0; i--)
            {
                dbCoef[i] = arrayLSM[i, numCoef];

                for (int j = numCoef - 1; i < j; j--)
                {
                    dbCoef[i] -= arrayLSM[i, j] * dbCoef[j];
                }
            }
        }
        private double AKM_Abs(double dbData)
        {
            double abs = dbData;

            if (dbData < 0)
            {
                abs = dbData * -1.0f;
            }

            return abs;
        }
        private double AKM_Pow(double dbData, int num)
        {
            double pow = 1.0f;

            for (int i = 0; i < num; i++)
            {
                pow *= dbData;
            }

            return pow;
        }
        private int AKM_Round(double dbData)
        {
            double round = 0;

            if (dbData > 0)
            {
                round = dbData + 0.5f;
            }
            else if (dbData < 0)
            {
                round = dbData - 0.5f;
            }

            return (int)round;
        }
    }
}
