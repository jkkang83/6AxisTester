using Dln;
using MathNet.Numerics.Financial;
using MathNet.Numerics.Optimization.TrustRegion;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Schema;
using static alglib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace FZ4P
{
    public partial class Process
    {
        List<byte[]> AFPID = new List<byte[]>
        {
            new byte[2]{ 0x0B, 0xE2 },
            new byte[2]{ 0x0A, 0x73 },
            new byte[2]{ 0x08, 0x85 },
            new byte[2]{ 0x09, 0x8C },
            new byte[2]{ 0x10, 0x29 },
            new byte[2]{ 0x11, 0x3E },
            new byte[2]{ 0x12, 0x96 },
            new byte[2]{ 0x13, 0x24 },
            new byte[2]{ 0x14, 0x18 },
            new byte[2]{ 0x15, 0x26 },
            new byte[2]{ 0x16, 0x20 },
            new byte[2]{ 0x17, 0x4B },
            new byte[2]{ 0x18, 0x14 },
            new byte[2]{ 0x1A, 0x00 },
            new byte[2]{ 0x1B, 0x54 },
            new byte[2]{ 0x1C, 0xDC },
            new byte[2]{ 0x1D, 0xCD },
            new byte[2]{ 0x1E, 0xD7 },
            new byte[2]{ 0x1F, 0x1F },
            new byte[2]{ 0x20, 0x18 },
            new byte[2]{ 0x21, 0x1D },
            new byte[2]{ 0x22, 0x14 },
            new byte[2]{ 0x23, 0x32 },
            new byte[2]{ 0x24, 0x50 },
            new byte[2]{ 0x25, 0x9B },
            new byte[2]{ 0x26, 0xCD },
            new byte[2]{ 0x27, 0xC3 },
            new byte[2]{ 0x28, 0x71 },
            new byte[2]{ 0x29, 0xDF },
            new byte[2]{ 0x2A, 0x34 },
            new byte[2]{ 0x2B, 0xC3 },
            new byte[2]{ 0x2C, 0x8E },
            new byte[2]{ 0x2D, 0x21 },
            new byte[2]{ 0x2E, 0x3D },
            new byte[2]{ 0x2F, 0x7A },
            new byte[2]{ 0xC0, 0x10 },
            new byte[2]{ 0xC1, 0x57 },
            new byte[2]{ 0xC2, 0x70 },
            new byte[2]{ 0xC3, 0x50 },
            new byte[2]{ 0xC4, 0xD0 },
            new byte[2]{ 0xC5, 0x50 },
            new byte[2]{ 0xC6, 0xD7 },
            new byte[2]{ 0xC7, 0x50 },
            new byte[2]{ 0xC8, 0x0A },
            new byte[2]{ 0xCA, 0x46 },
            new byte[2]{ 0xCB, 0xD8 },
            new byte[2]{ 0xCC, 0x40 },
            new byte[2]{ 0xCD, 0x32 },
            new byte[2]{ 0xCE, 0x00 },
            new byte[2]{ 0x3D, 0x06 },

        };
        
        List<byte[]> OISPID = new List<byte[]> 
        {
            new byte[4] { 0x0B, 0x12, 0x14, 0x00 },
            new byte[4] { 0x0A, 0x59, 0x59, 0x01 },
            new byte[4] { 0x0C, 0x62, 0x62, 0x01 },
            new byte[4] { 0x08, 0x09, 0x09, 0x01 },
            new byte[4] { 0x09, 0x00, 0x00, 0x01 },
            new byte[4] { 0x24, 0x6C, 0x6C, 0x01 },
            new byte[4] { 0x25, 0x2F, 0x2F, 0x01 },
            new byte[4] { 0x5D, 0x60, 0x60, 0x00 }, //안함
            new byte[4] { 0x5E, 0x00, 0x00, 0x01 },
            new byte[4] { 0x5F, 0x00, 0x04, 0x01 },
            new byte[4] { 0x60, 0x00, 0x00, 0x01 },
            new byte[4] { 0x61, 0x00, 0x00, 0x01 },
            new byte[4] { 0x6B, 0x00, 0x00, 0x01 },
            new byte[4] { 0x6C, 0x00, 0x00, 0x01 },
            new byte[4] { 0x6D, 0x00, 0x00, 0x01 },
            new byte[4] { 0x6E, 0x00, 0x00, 0x01 },
            new byte[4] { 0x6F, 0x00, 0x00, 0x01 },
            new byte[4] { 0xD8, 0x00, 0x00, 0x01 },
            new byte[4] { 0xD9, 0x00, 0x00, 0x01 },
            new byte[4] { 0xDA, 0x00, 0x00, 0x01 },
            new byte[4] { 0xDB, 0x00, 0x00, 0x01 },
            new byte[4] { 0xDC, 0x00, 0x00, 0x01 },
            new byte[4] { 0xDD, 0x00, 0x00, 0x01 },
            new byte[4] { 0x0D, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x0E, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x0F, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x3E, 0x85, 0x85, 0x01 },
            new byte[4] { 0x10, 0x55, 0x50, 0x01 },
            new byte[4] { 0x11, 0x2D, 0x2D, 0x01 },
            new byte[4] { 0x12, 0xFA, 0xF5, 0x01 },
            new byte[4] { 0x13, 0x18, 0x19, 0x01 },
            new byte[4] { 0x14, 0x19, 0x1B, 0x01 },
            new byte[4] { 0x15, 0x50, 0x50, 0x01 },
            new byte[4] { 0x16, 0x25, 0x25, 0x01 },
            new byte[4] { 0x17, 0x6E, 0x6E, 0x01 },
            new byte[4] { 0x18, 0xB3, 0xB4, 0x01 },
            new byte[4] { 0x1A, 0xC2, 0xC3, 0x01 },
            new byte[4] { 0x1B, 0xA6, 0xC0, 0x01 },
            new byte[4] { 0x1C, 0x7D, 0x7C, 0x01 },
            new byte[4] { 0x1D, 0x57, 0x3A, 0x01 },
            new byte[4] { 0x1E, 0x3C, 0x37, 0x01 },
            new byte[4] { 0x1F, 0x33, 0x6C, 0x01 },
            new byte[4] { 0x20, 0x86, 0x8F, 0x01 },
            new byte[4] { 0x21, 0x2B, 0x53, 0x01 },
            new byte[4] { 0x22, 0x3D, 0x39, 0x01 },
            new byte[4] { 0x23, 0xA5, 0x47, 0x01 },
            new byte[4] { 0x27, 0x92, 0x92, 0x01 },
            new byte[4] { 0x28, 0x92, 0x92, 0x00 }, //안함
            new byte[4] { 0x29, 0x18, 0x18, 0x01 },
            new byte[4] { 0x2A, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x2B, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x2C, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x2D, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x2E, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x2F, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x30, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x31, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x32, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x33, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x34, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x35, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x36, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x37, 0xFC, 0x04, 0x01 },
            new byte[4] { 0x50, 0xEF, 0xEF, 0x00 }, //안함
            new byte[4] { 0x51, 0xFF, 0xFF, 0x01 },
            new byte[4] { 0x52, 0x40, 0x40, 0x01 },
            new byte[4] { 0x53, 0x28, 0x1E, 0x01 },
            new byte[4] { 0x54, 0x01, 0x01, 0x01 },
            new byte[4] { 0x55, 0x78, 0x50, 0x01 },
            new byte[4] { 0x56, 0x7D, 0x8C, 0x01 },
            new byte[4] { 0x57, 0xFA, 0xF5, 0x01 },
            new byte[4] { 0x58, 0xFA, 0xF5, 0x01 },
            new byte[4] { 0x59, 0x2D, 0x2D, 0x01 },
            new byte[4] { 0x5A, 0x50, 0x3C, 0x01 },
            new byte[4] { 0x5B, 0xFF, 0xFF, 0x01 },
            new byte[4] { 0x5C, 0x32, 0x32, 0x01 }
        };

        int SinewaveXMaxDiff = 0;
        int SinewaveYMaxDiff = 0;
        int RingingXStabilizer = 0;
        int RingingYStabilizer = 0;

        void AddSequence()
        {
            ItemList.Add(new ActItems() { Name = "AF OpenLoopAging", Func = Act_AFOpenLoopAging, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Initial", Func = Act_AFInit, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF EPA", Func = Act_AFEPA, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Linearity Comp", Func = Act_AFLinComp, IsMulti = true });
        
            ItemList.Add(new ActItems() { Name = "Find AF Best Position", Func = Act_FindBestAFPosition });
            ItemList.Add(new ActItems() { Name = "OIS Init", Func = Act_OISInit });
            ItemList.Add(new ActItems() { Name = "OIS EPA", Func = Act_OISEPA, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Close Loop Aging", Func = Act_CloseLoopAging });
            ItemList.Add(new ActItems() { Name = "OIS X LinComp", Func = Act_OISLinComp });
            ItemList.Add(new ActItems() { Name = "OIS Y LinComp", Func = Act_OISLinComp });
            ItemList.Add(new ActItems() { Name = "Servo Decenter", Func = ServoDecenter, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Phase Margin", Func = OISPhasemargin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Phase Margin", Func = AFPhaseMargin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Loopgain", Func = OISLoopGain, IsMulti = true });
            //  ItemList.Add(new ActItems() { Name = "Gain@10Hz", Func = Act_GaindB10Hz, IsMulti = true });
            //  ItemList.Add(new ActItems() { Name = "Gain@10Hz", Func = Act_GaindB10Hz, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "Phase Margin", Func = Act_Phase_Margin, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "Phase Margin High", Func = Act_Phase_Margin_High, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Gain Margin", Func = AFGainMargin, IsMulti = true });      
            ItemList.Add(new ActItems() { Name = "AF ScanAging", Func = Act_AFScanAging });
            ItemList.Add(new ActItems() { Name = "AF PreDriving", Func = Act_PreAFDriving });
            ItemList.Add(new ActItems() { Name = "OIS Shift", Func = Act_OISShift2, IsMulti = true });
           // ItemList.Add(new ActItems() { Name = "Restore Slave Addr", Func = RestoreSlaveAddr, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Change Slave Addr", Func = ChangeSlaveAddr, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF/OIS Temperature test", Func = TempTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS OpenLoop Test", Func = OISOpenLoopTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Auto Test", Func = AutoTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Sensitivity Test", Func = OISSensitivityTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "PID Verify", Func = PID_Verify, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "IME Test", Func = IME_Test, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "through Peak 25", Func = throughFRA, IsMulti = true });
        }

        #region AddSeq

        void oisOL(int ch, int axis)
        {
            string axisName = axis == 0 ? "X" : "Y";
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int test_time = Condition.OISOLMoveDelay; int test_size = (Condition.OISOLtp2 - Condition.OISOLtp1) / Condition.OISOLStepNum;
            int open_data = 0; int open_input;
            ushort open_output;
            int t_count;
            int[] start_pos = new int[2] { 0, 0 };
            int[] end_pos = new int[2] { 512, 512 };
            int[] square = new int[500];
            uint sum_square;
            int[] Ya = new int[500]; int[] Yb = new int[500]; int[] height = new int[500];
            int dc_count_rising, dc_count_falling = 0, dc_count;
            short[,] dc_value = new short[2, 200];
            uint square_spec = (uint)Condition.OISOLSpec;
            byte dc_result;


            AddLog(ch, $"{axisName} Open test start.");

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);

            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, addr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x00 });

            byte[] rbuf2 = new byte[2];
            dc_count_rising = 0;
            for (open_input = start_pos[0]; open_input < end_pos[1]; open_input += test_size)
            {
                int pos = open_input << 7;
                Dln.WriteArray(ch, addr, 0x00, new byte[] { (byte)(pos >> 8), (byte)pos });
                Wait(test_time);
                Dln.ReadArray(ch, addr, 0x80, rbuf2);
                open_output = (ushort)((rbuf2[0] << 8) + rbuf2[1]);
                open_data = open_output >> 3;
                if (open_data > 0x1000) { open_data -= 0x2000; }

                if ((open_input >= Condition.OISOLtp1) && (open_input <= Condition.OISOLtp2))
                {
                    dc_value[0, dc_count_rising] = (short)open_data;
                    dc_count_rising++;

                }
            }
            dc_count = dc_count_rising;
            dc_count_rising--;
            dc_count_falling = dc_count_rising;
            for (open_input -= test_size; open_input >= start_pos[0]; open_input -= test_size)
            {
                int pos = open_input << 7;
                Dln.WriteArray(ch, addr, 0x00, new byte[] { (byte)(pos >> 8), (byte)pos });
                Wait(test_time);
                Dln.ReadArray(ch, addr, 0x80, rbuf2);
                open_output = (ushort)((rbuf2[0] << 8) + rbuf2[1]);
                open_data = open_output >> 3;
                if (open_data > 0x1000) { open_data -= 0x2000; }
                if ((open_input >= Condition.OISOLtp1) && (open_input <= Condition.OISOLtp2))
                {
                    dc_value[1, dc_count_falling] = (short)open_data;
                    dc_count_falling--;

                }
            }
            AddLog(ch, $"dc_count : {dc_count}");
            Dln.WriteArray(ch, addr, 0xA6, new byte[] { 0x00 });
            t_count = 0;
            byte[] rbuf = new byte[1];
            while (true)
            {
                Dln.ReadArray(ch, addr, 0x4C, rbuf);
                dc_result = rbuf[0];
                Wait(1);
                if ((dc_result & 0x10) == 0x00) break;
                t_count++;
                if(t_count > 100)
                {
                    SetError(ch, NonSpecItem.OIS_Openloop_Test);
                    return;
                }
            }
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            dc_result = 0;
            sum_square = 0;
            for (int i = 0; i < dc_count - 1; i++)
            {
                Ya[i] = dc_value[1, i] - dc_value[0, i];
                Yb[i] = dc_value[1, i + 1] - dc_value[0, i + 1];
                height[i] = test_size;
                square[i] = ((Ya[i] + Yb[i]) * height[i]) >> 1;
                sum_square += (uint)Math.Abs(square[i]);
            }
            sum_square = (sum_square / 10);

            if (axis == 0)
            {
                PassFails[ch].Results[(int)SpecItem.OLTestXResult].Val = sum_square;
                ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult);
            }

            else
            {
                PassFails[ch].Results[(int)SpecItem.OLTestYResult].Val = sum_square;
                ShowDataResults(ch, (int)SpecItem.OLTestYResult, (int)SpecItem.OLTestYResult);
            }

                AddLog(ch, $"sum square : {sum_square}");
            if (sum_square > square_spec || sum_square <= 0)
            {
                dc_result = 0x01;
                AddLog(ch, $"NG Over DC SR, {square_spec}");
                SetError(ch, NonSpecItem.OIS_Openloop_Test);
            }
            AddLog(ch, $"[Final] {axisName} sum square : {sum_square}, result : {dc_result}");
            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            AddLog(ch, $"{axisName} Open test end.");
        }

        void OISOpenLoopTest(int ch, string testItem)
        {
            if (m__G.m_ChannelOn[ch]) oisOL(ch, 0);
            if (m__G.m_ChannelOn[ch]) oisOL(ch, 1);
        }

        void TempTest(int ch, string testItem)
        {
            byte[] rbuf = new byte[1];
            byte AF1ABackData, AF0BBackData, AFC9BackData;
            byte X0DBackData, Y0DBackData, X26BackData, Y26BackData;

            List<byte> xData = new List<byte>();
            List<byte> yData = new List<byte>();
            List<byte> AFData = new List<byte>();

            List<double> xDData = new List<double>();
            List<double> yDData = new List<double>();
            List<double> AFDData = new List<double>();

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x1A, rbuf);
            AF1ABackData = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            AF0BBackData = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0xC9, rbuf);
            AFC9BackData = rbuf[0];

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(AF0BBackData & 0x7F) });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0D, rbuf);
            X0DBackData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { 0xC0 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });

            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0D, rbuf);
            Y0DBackData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { 0xC0 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x26, rbuf);
            X26BackData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x26, new byte[] { 0x00 });

            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x26, rbuf);
            Y26BackData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x26, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });

            Stopwatch st = new Stopwatch();
            st.Start();
            while(st.ElapsedMilliseconds <= 2000)
            {
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x90, rbuf);
                AFData.Add(rbuf[0]);
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x90, rbuf);
                xData.Add(rbuf[0]);
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x90, rbuf);
                yData.Add(rbuf[0]);
                Wait(50);
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x02, rbuf);
               // AFData.Add(rbuf[0]);
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x02, rbuf);
               // xData.Add(rbuf[0]);
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x02, rbuf);
               // yData.Add(rbuf[0]);

            }

            for (int i = 0; i < AFData.Count; i++)
            {
                if (AFData[i] < 128) AFDData.Add(AFData[i] * 0.625);
                else AFDData.Add((AFData[i] - 256) * 0.625);

                if (xData[i] < 128) xDData.Add(xData[i] * 0.625);
                else xDData.Add((xData[i] - 256) * 0.625);

                if (yData[i] < 128) yDData.Add(yData[i] * 0.625);
                else yDData.Add((yData[i] - 256) * 0.625);
            }

            //
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { AF1ABackData });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { AF0BBackData });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { AFC9BackData });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { X0DBackData });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x26, new byte[] { X26BackData });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { Y0DBackData });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x26, new byte[] { Y26BackData });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });


            double xMinVal, yMinVal, AFMinVal;
            double xMaxVal, yMaxVal, AFMaxVal;
            double xVariation, yVariation, AFVariation; 

            xMinVal = xDData.Min(); yMinVal = yDData.Min(); AFMinVal = AFDData.Min();
            xMaxVal = xDData.Max(); yMaxVal = yDData.Max(); AFMaxVal = AFDData.Max();
            xVariation = xMaxVal - xMinVal;
            yVariation = yMaxVal - yMinVal;
            AFVariation = AFMaxVal - AFMinVal;

            AddLog(ch, $"Temp Min, X:{xMinVal}, Y:{yMinVal}, AF:{AFMinVal}");
            AddLog(ch, $"Temp Max, X:{xMaxVal}, Y:{yMaxVal}, AF:{AFMaxVal}");
            AddLog(ch, $"Temp var., X:{xVariation}, Y:{yVariation}, AF:{AFVariation}");

            if (xMinVal < Condition.TempMinSpec || xMaxVal > Condition.TempMaxSpec || xVariation > Condition.TempValSpec)
            {
                PassFails[ch].Results[(int)SpecItem.OISXTempRes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.OISXTempRes, (int)SpecItem.OISXTempRes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISXTempRes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.OISXTempRes, (int)SpecItem.OISXTempRes);
            }
            if (yMinVal < Condition.TempMinSpec || yMaxVal > Condition.TempMaxSpec || yVariation > Condition.TempValSpec)
            {
                PassFails[ch].Results[(int)SpecItem.OISYTempRes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.OISYTempRes, (int)SpecItem.OISYTempRes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISYTempRes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.OISYTempRes, (int)SpecItem.OISYTempRes);
            }
            if (AFMinVal < Condition.TempMinSpec || AFMaxVal > Condition.TempMaxSpec || AFVariation > Condition.TempValSpec)
            {
                PassFails[ch].Results[(int)SpecItem.AFTempRes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.AFTempRes, (int)SpecItem.AFTempRes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.AFTempRes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AFTempRes, (int)SpecItem.AFTempRes);
            }
         

        }
        

        void ChangeSlaveAddr(int ch, string testItem)
        {
            // Y2 : 4E -> 6C
            // Y1 : 0E -> 4E
            // X  : 0A -> 0E

            bool xChanged = true;
            bool Y1Changed = true;
            bool Y2Changed = true;
            bool AFChanged = true;

            byte[] rDdata = new byte[1];
         
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0xAE, new byte[] { 0x3B })) Y2Changed = false; }

            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B })) Y1Changed = false;
            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B })) xChanged = false;
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B })) AFChanged = false;

            if (DrvIC.Y2SlaveAddr != 0x00)
            {
                if (Y2Changed)
                    AddLog(ch, string.Format("Already Y2 Slave Address Changed.."));
                else
                {
                    if (!Dln.WriteArray(ch, DrvIC.Y2OriginAddr, 0xAE, new byte[] { 0x3B })) return;
                    AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

                    if (!Dln.WriteArray(ch, DrvIC.Y2OriginAddr, 0x0B, new byte[] { 0x04 })) return; // 02 : Normal, 04 : Reverse
                    AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x04));

                    if (!Dln.WriteArray(ch, DrvIC.Y2OriginAddr, 0x0A, new byte[] { 0x30 })) return; // Setting Slave Address
                    AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x30));
                    Wait(200);
                    if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                    AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                    AddLog(ch, string.Format("Y2 SlaveAddr Change FinIsh."));
                }

            }
            if (Y1Changed)
                AddLog(ch, string.Format("Already Y Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0xAE, new byte[] { 0x3B })) return;
                AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0x0B, new byte[] { 0x04 })) return; // 02 : Normal, 04 : Reverse
                AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x02));

                if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0x0A, new byte[] { 0x59 })) return; // Setting Slave Address
                AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x59));
                Wait(200);
                if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                AddLog(ch, string.Format("Y SlaveAddr Change FinIsh."));
            }

            if (xChanged)
                AddLog(ch, string.Format("Already X Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0xAE, new byte[] { 0x3B })) return;
                AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0x0B, new byte[] { 0x02 })) return; // 02 : Normal, 04 : Reverse
                AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x02));

                if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0x0A, new byte[] { 0x59 })) return; // Setting Slave Address
                AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x59));
                Wait(200);
                if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                AddLog(ch, string.Format("X SlaveAddr Change FinIsh."));
            }

            if (AFChanged)
                AddLog(ch, string.Format("Already AF Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0xAE, new byte[] { 0x3B })) return;
                AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0x0B, new byte[] { 0x02 })) return; // 02 : Normal, 04 : Reverse
                AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0B, 0x02));

                if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0x0A, new byte[] { 0x70 })) return; // Setting Slave Address
                AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x70));
                Wait(200);
                if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                AddLog(ch, string.Format("X SlaveAddr Change FinIsh."));
            }

        }

        private void Act_AFOpenLoopAging(int ch, string testItem)
        {
            byte[] rbuf = new byte[1];
            byte DataBackup = 0x00;
            int delay = 1000000 / Condition.AFOpenLoopFreq / 2 / 1000;

            ////OIS On
            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });

            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            //AF OpenLoop Aging Seq
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            DataBackup = rbuf[0];
            rbuf[0] = (byte)(rbuf[0] & 0x7F);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            AddLog(ch, $"OpenLoop Range : {0} - {4095}");
            AddLog(ch, $"OpenLoop Freq : {Condition.AFOpenLoopFreq}");
            AddLog(ch, $"OpenLoop Count : {Condition.AFOpenLoopCount}");
            for (int i = 0; i < Condition.AFOpenLoopCount; i++)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF0 });
                Wait(delay);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
                Wait(delay);
            }

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { DataBackup });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        }
        void Act_AFScanAging(int ch, string testItem)
        {
            AddLog(ch, "<<<  AF Scan aging Start  >>>");
            AddLog(ch, $"Start aging {Condition.AFSCanAgingCount} cycle for AF Driving");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", AFCenter);
            Wait(100);

            int curPos = 2047;
            List<int> code = new List<int>();

            do
            {
                code.Add(curPos);
                curPos -= Condition.AFScanAgingStep;
            } while (curPos > Condition.AFScanAgingMin);
            code.Add(Condition.AFScanAgingMin);
            curPos += Condition.AFScanAgingStep;
            do
            {
                code.Add(curPos);
                curPos += Condition.AFScanAgingStep;
            } while (curPos < Condition.AFScanAgingMax);
            code.Add(Condition.AFScanAgingMax);
            curPos -= Condition.AFScanAgingStep;

            do
            {
                code.Add(curPos);
                curPos -= Condition.AFScanAgingStep;
            } while (curPos > 2047);
            code.Add(2047);

            for (int i = 0; i < Condition.AFSCanAgingCount; i++)
            {
                for (int j = 0; j < code.Count; j++)
                {
                    DrvIC.Move(ch, "AF", code[j]);
                    Wait(Condition.AFScanAgingDelay);
                }

            }
            AddLog(ch, "<<<  AF Scan aging End  >>>");
        }
        void Act_PreAFDriving(int ch, string testItem)
        {
            LEDs_All_On(0, true);
            AddLog(ch, "AF Pre Driving");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            FindResult res = new FindResult();


            int[] code = new int[] { 2048, 1600, 320, 160, 0, 3995, 4075, 4085, 4095 }; //4, 8

            for (int i = 0; i < Condition.AFPReDrvCount; i++)
            {
                double[] MtoM = new double[2];
                for (int j = 0; j < code.Length; j++)
                {
                    DrvIC.Move(ch, "AF", code[j]);
                    Wait(Condition.AFPreDrvDelay);
                    if (j == 4)
                    {
                        res = Measure();
                        MtoM[0] = res.cz[0];
                    }
                    if (j == 8)
                    {
                        res = Measure();
                        MtoM[1] = res.cz[0];
                    }
                }
                AddLog(ch, $"{i + 1} scan stroke : {Math.Abs(MtoM[1] - MtoM[0]).ToString("F3")}");
            }
            LEDs_All_On(0, false);
        }

        private void Act_AFInit(int ch, string testItem)
        {
            byte[] rbuf = new byte[1];
            FindResult res = new FindResult();
            double[] zVal = new double[2];

          
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            //AF OpenLoop Seq 추가
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            rbuf[0] = (byte)(rbuf[0] & 0x7F);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);
            AddLog(ch, $"AF Openloop Stroke Check");

            LEDs_All_On(0, true);
            for (int i = 0; i < 11; i++)
            {
                DrvIC.Move(ch, "AF", 4095);
                Wait(50);
                res = Measure();
                zVal[0] = res.cz[0];
                DrvIC.Move(ch, "AF", 0);
                Wait(50);
                res = Measure();
                zVal[1] = res.cz[0];

                AddLog(ch, $"{i + 1} : {Math.Abs(zVal[1] - zVal[0]).ToString("F3")}");

            }
            LEDs_All_On(0, false);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });

            AF_EPA_Reset(ch);
            AF_LinearityComp_Reset(ch);
            AddLog(ch, "PID parameter setting");
            for (int i = 0; i < AFPID.Count; i++)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, AFPID[i][0], new byte[] { AFPID[i][1] });
            }

            AddLog(ch, "Temp register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x80 });
            Wait(10);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x70, rbuf);
            AddLog(ch, $"Read 0x70 : 0x{rbuf[0].ToString("X")}");


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, rbuf);

            AddLog(ch, "Calibration instruction");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x18 });
            Wait(150);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x19, rbuf);
            AddLog(ch, $"Read 0x19 : 0x{rbuf[0].ToString("X")}");

            byte tmpData = (byte)(rbuf[0] * 0.75);
            AddLog(ch, $"CalcData : 0x{tmpData.ToString("X")}");

            if (tmpData >= 0x00 && tmpData <= 0x30)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x19, new byte[] { tmpData });
            }
            else
            {
                SetError(ch, NonSpecItem.AF_Init);
                return;
                //Error처리
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xF3, new byte[] { 0x1E });
            Wait(10);
            Store(ch, 0);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.PowerSequence(0);
            AK7314_ICReset(0);
            CheckData(ch, 0);
        }

        void Act_CloseLoopAging(int ch, string testitem)
        {
            CloseLoopAging(0, Condition.CLAgingMode);
        }
        private void Act_AFEPA(int ch, string testItem)
        {

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            if(DrvIC.Y2SlaveAddr != 0x00) Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 });

            LEDs_All_On(0, true);
            FindResult res = new FindResult();
            int findcount = 0;

            double Target = Condition.AFEPATarget;
            int InfCut = 10;
            int macCut = 6;
            byte[] rbuf2 = new byte[2];
            byte[] rbuf = new byte[1];
            byte backData = 0;
            double InitPos = 0; double EndPos = 0;

            //move 0 code Position
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x19, 0x00 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x05, 0x00 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x02, 0x80 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
            Wait(100);
            //측정하고 값 초기화         
            AddLog(ch, $"af pos(t, c) : {0},{DrvIC.ReadHall(ch, "AF")}");
            Wait(50);
            res = Measure();

            InitPos = res.cz[0];
            int dir = 1;

            int step = 512;
            int pos = step;
            InfCut = (int)(InitPos + 10);
            while (true)
            {
                
                if(findcount > 50)
                {
                    AddLog(ch, "EPA Find NG");
                    SetError(ch, NonSpecItem.AF_EPA);
                    return;
                }
                DrvIC.Move(ch, "AF", pos);
                int a = DrvIC.ReadHall(ch, "AF");
                Wait(100);
                res = Measure();
              

                AddLog(ch, $"Pos:{(int)(res.cz[0] - InitPos)}, Code:{pos}, Step:{step}");

                if (res.cz[0] > InfCut + 1)
                {
                    if (dir == 1)
                    {
                        dir = 0;
                        step = step / 2;
                        pos = pos - step;
                    }
                    else
                    {
                        dir = 0;
                        pos = pos - step;
                    }

                }
                else if (res.cz[0] < InfCut - 1)
                {
                    if (dir == 1)
                    {
                        dir = 1;
                        pos = pos + step;
                    }
                    else
                    {
                        dir = 1;
                        step = step / 2;
                        pos = pos + step;
                    }

                }
                else { break; }
                findcount++;

            }

            int InfPos = pos;
            AddLog(ch, $"Inf Code : {InfPos}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Wait(50);
            res = Measure();
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xE6, 0xF0 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFA, 0xF0 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFD, 0x70 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF8 });
            Wait(100);
            //측정하고 값 초기화, Measure Stroke 구해서 담음
            double measureStroke = 0;


            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x84, rbuf2); // check AF Current Hall
            AddLog(ch, $"af pos(t, c) : {4095},{DrvIC.ReadHall(ch, "AF")}");
            Wait(50);
            res = Measure();

            EndPos = res.cz[0];
            measureStroke = Math.Abs(EndPos - InitPos);
            AddLog(ch, $"Full Stroke = {measureStroke.ToString("F3")}");
            PassFails[ch].Results[(int)SpecItem.AF_NonEPAStroke].Val = measureStroke;
            ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke);
            if (measureStroke - Target - 10 > 6) macCut = (int)(measureStroke - Target - 10);
            AddLog(ch, $"Find macCut = {macCut}");

            dir = 0;
            step = 512;
            pos = 4095 - step;
            macCut = (int)(EndPos - macCut);
            findcount = 0;
            while (true)
            {
                if (findcount > 50)
                {
                    AddLog(ch, "EPA Find NG");
                    SetError(ch, NonSpecItem.AF_EPA);
                    return;
                }
                DrvIC.Move(ch, "AF", pos);
                Wait(100);
                res = Measure();

                AddLog(ch, $"Pos:{(int)(res.cz[0] - EndPos)}, Code:{pos}, Step:{step}");
                //측정하고 값 기입
                if (res.cz[0] > macCut + 1)
                {
                    if (dir == 1)
                    {
                        dir = 0;
                        step = step / 2;
                        pos = pos - step;
                    }
                    else
                    {
                        dir = 0;
                        pos = pos - step;
                    }

                }
                else if (res.cz[0] < macCut - 1)
                {
                    if (dir == 1)
                    {
                        dir = 1;
                        pos = pos + step;
                    }
                    else
                    {
                        dir = 1;
                        step = step / 2;
                        pos = pos + step;
                    }

                }
                else { break; }
                findcount++;

            }
            int macPos = pos;
            AddLog(ch, $"Mac Code : {macPos}");
            //   Inf, Mac EPA 기입 계산

            byte POSVT = (byte)((4096 - macPos) / 16); byte NEGVT = (byte)(InfPos / 16);

            //   byte POSVT = (byte)((-Condition.AFPOSVT) / 16); byte NEGVT = (byte)(Condition.AFNEGVT / 16);

            //     AddLog(ch, $"POSVT = {Condition.AFPOSVT}, NEGVT = {Condition.AFNEGVT}");
            AddLog(ch, $"0x0E : 0x{POSVT.ToString("X")}, 0x0F : 0x{NEGVT.ToString("X")}");


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { POSVT });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { NEGVT });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0x80) });//0x0B값 읽어서 백업해야하는지 확인

            DrvIC.Move(ch, "AF", AFCenter);

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
            Wait(100);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
            Wait(200);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) != 0x00)
            {
                SetError(ch, NonSpecItem.AF_EPA);
                return;
            }
          
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            CheckData(ch, 0);
        }
        private void Act_OISEPA(int ch, string testItem)
        {
            byte[] rbuf = new byte[1];
            byte backData = 0;

            int Xposvt = -Condition.XPOSVT, Xnegvt = Condition.XNEGVT, Yposvt = -Condition.YPOSVT, Ynegvt = Condition.YNEGVT;
            AddLog(ch, $"X POSVT = {Xposvt}, X NEGVT = {Xnegvt}");
            AddLog(ch, $"Y POSVT = {Yposvt}, Y NEGVT = {Ynegvt}");

            AddLog(ch, $"X = 0x0E : 0x{((Xposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Xnegvt / 4) & 0x03).ToString("X")}");
            AddLog(ch, $"Y = 0x0E : 0x{((Yposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Ynegvt / 4) & 0x03).ToString("X")}");

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(5);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { (byte)((Xposvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { (byte)((Xnegvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { (byte)(((Xposvt / 4) & 0x03 << 2) | ((Xnegvt) & 0x03)) });

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { (byte)((Yposvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { (byte)((Ynegvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { (byte)(((Yposvt / 4) & 0x03 << 2) | ((Ynegvt) & 0x03)) });


            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
            Wait(120);

            Store(ch, 1);
            Store(ch, 2);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
        }

        void Store(int ch, int Axis)
        {

            AddLog(ch, "Store Start");
            byte[] rbuf = new byte[1];
            if (Axis == 0)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
                Wait(100);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x02 });
                Wait(200);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x04 });
                Wait(200);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x08 });
                Wait(100);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
                Wait(200);
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
                if ((byte)(rbuf[0] & 0x04) != 0x00)
                {
                    SetError(ch, NonSpecItem.Store_Fail);
                    AddLog(ch, "Store fail");
                    return;
                }


            }
            else
            {
                int addr = Axis == 1 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;

                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x01 });
                Wait(150);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x02 });
                Wait(230);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x04 });
                Wait(120);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x08 });
                Wait(100);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x10 });
                Wait(50);
                Dln.ReadArray(ch, addr, 0x4B, rbuf);
                if ((byte)(rbuf[0] & 0x04) != 0x00)
                {
                    SetError(ch, NonSpecItem.Store_Fail);
                    AddLog(ch, "Store fail");
                    return;
                }

            }
            AddLog(ch, "Store finish");
        }
        void AF_EPA_Reset(int ch)
        {
            AddLog(ch, "AF EPA Reset");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { 0x00 });
        }
        void AF_LinearityComp_Reset(int ch)
        {
            AddLog(ch, "AF Linearity Comp Reset");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x30, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x31, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x32, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x33, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x34, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x35, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x36, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x37, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x38, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x39, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3C, new byte[] { 0x00 });
        }

        void OIS_EPA_Reset(int ch)
        {
            AddLog(ch, "OIS EPA Reset");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { 0x00 });
        }
        void OIS_LinearityComp_Reset(int ch, int Axis)
        {
            if(Axis == 0)
            {
                AddLog(ch, "X Linearity Comp Reset");
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
               

                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2A, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2B, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2C, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2D, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2E, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2F, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x30, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x31, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x32, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x33, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x34, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x35, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x36, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x03, new byte[] { 0x08 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
            }
            else
            {
                AddLog(ch, "Y Linearity Comp Reset");
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2A, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2B, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2C, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2D, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2E, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2F, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x30, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x31, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x32, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x33, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x34, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x35, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x36, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x03, new byte[] { 0x08 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            }



           
        }
        
        void CloseLoopAging(int ch, int mode)
        {
            int AFMin = Condition.CLAgingAFMin, AFMax = Condition.CLAgingAFMax, OISMin = Condition.CLAgingOISMin, OISMax = Condition.CLAgingOISMax, count = Condition.CLAgingCount;
            int delay = 1000000 / Condition.CLAgingFreq / 2 / 1000;

            AddLog(ch, $"AF Range : {AFMin} - {AFMax}");
            AddLog(ch, $"OIS Range : {OISMin} - {OISMax}");
            AddLog(ch, $"Aging Count : {count}, Freq : {Condition.CLAgingFreq}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Wait(100);
            if (mode == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DrvIC.Move(ch, "AF", AFMin);
                    DrvIC.Move(ch, "X", OISMin);
                    DrvIC.Move(ch, "Y", OISMin);
                    Wait(delay);
                    DrvIC.Move(ch, "AF", AFMax);
                    DrvIC.Move(ch, "X", OISMax);
                    DrvIC.Move(ch, "Y", OISMax);
                    Wait(delay);
                }
            }
            else
            {
                Random rnd = new Random();
                for (int i = 0; i < count; i++)
                {
                    DrvIC.Move(ch, "AF", AFMin);
                    DrvIC.Move(ch, "X", rnd.Next(OISMin, OISMax));
                    DrvIC.Move(ch, "Y", rnd.Next(OISMin, OISMax));
                    Wait(delay);
                    DrvIC.Move(ch, "AF", AFMax);
                    DrvIC.Move(ch, "X", rnd.Next(OISMin, OISMax));
                    DrvIC.Move(ch, "Y", rnd.Next(OISMin, OISMax));
                    Wait(delay);
                }
            }


            DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);

            //   Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

        }
        void CheckData(int ch, int axis)
        {
            byte[] data = new byte[256];
            byte[] rbuf = new byte[1];
            byte[] rbuf2 = new byte[2];
            int addr = 0x00;
            string s = string.Empty;
            int Pcal = 0, Ncal = 0, PVT = 0, NVT = 0;

            switch (axis)
            {
                case 0:
                    addr = DrvIC.AFSlaveAddr;
                    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x04, rbuf);
                    if (rbuf[0] > 128) Pcal = rbuf[0] - 256;
                    else Pcal = rbuf[0];
                    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x06, rbuf);
                    if (rbuf[0] > 128) Ncal = rbuf[0] - 256;
                    else Ncal = rbuf[0];
                    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0E, rbuf);
                    PVT = rbuf[0];
                    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0F, rbuf);
                    NVT = rbuf[0];

                    break;
                case 1:                
                    addr = DrvIC.XSlaveAddr;
                    Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x04, rbuf2);
                    Pcal = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                    Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x06, rbuf2);
                    Ncal = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                    break;
                case 2:
                    addr = DrvIC.Y1SlaveAddr;
                    Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x04, rbuf2);
                    Pcal = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                    Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x06, rbuf2);
                    Ncal = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                    break;
            }
            for (int i = 0; i < 256; i++)
            {
                if(i <= 0x3F || i>= 0x90)
                {
                    Dln.ReadArray(ch, addr, 0x00 + i, rbuf);
                    data[i] = rbuf[0];
                }
            }

            for (int i = 0; i < 16; i++)
            {
                if((i * 16 <= 0x30) || (i*16 >= 0x90))
                {
                    s += $"0x{(16 * i).ToString("X2")}~0x{(16 * i + 15).ToString("X2")} : " +
                         $"{data[16 * i].ToString("X2")}{data[16 * i + 1].ToString("X2")}{data[16 * i + 2].ToString("X2")}{data[16 * i + 3].ToString("X2")}  " +
                         $"{data[16 * i + 4].ToString("X2")}{data[16 * i + 5].ToString("X2")}{data[16 * i + 6].ToString("X2")}{data[16 * i + 7].ToString("X2")}  " +
                         $"{data[16 * i + 8].ToString("X2")}{data[16 * i + 9].ToString("X2")}{data[16 * i + 10].ToString("X2")}{data[16 * i + 11].ToString("X2")}  " +
                         $"{data[16 * i + 12].ToString("X2")}{data[16 * i + 13].ToString("X2")}{data[16 * i + 14].ToString("X2")}{data[16 * i + 15].ToString("X2")}\r\n";

                }
            }
            AddLog(ch, s);
            AddLog(ch, $"PCal : {Pcal}, Ncal : {Ncal}");
            AddLog(ch, $"PVT : {PVT}, NVT : {NVT}");

        }
        void Act_OISLinComp(int ch, string testitem)
        {

            int addr = testitem.Contains("X") ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            string Axis = testitem.Contains("X") ? "X" : "Y";
            int axisint = testitem.Contains("X") ? 1 : 2;
            OIS_LinearityComp_Reset(ch, axisint - 1);

            int start = 0, end = 0, step = 0, delay = 0;
            List<float> target = new List<float>();
            List<float> data = new List<float>();
            List<float> ReadHall = new List<float>();
            float RefData = 0;


            if (Axis == "X") { start = Condition.XLinCompStart; end = Condition.XLinCompEnd; step = Condition.XLinCompStep; delay = Condition.XLinCompMoveDelay; }
            else { start = Condition.YLinCompStart; end = Condition.YLinCompEnd; step = Condition.YLinCompStep; delay = Condition.YLinCompMoveDelay; }



            LEDs_All_On(0, true);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Wait(100);

            FindResult tmpres = new FindResult();

            byte pvt = 0, nvt = 0;
            byte[] rbuf = new byte[1];

            Dln.ReadArray(ch, addr, 0x0E, rbuf);
            pvt = rbuf[0];
            Dln.ReadArray(ch, addr, 0x0F, rbuf);
            nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");


            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, Axis, start);
            int index = 0;
            AddLog(ch, $"Target\tPos\tReadHall");
            while (true)
            {
                int currCode = start + (index * step);
                if (currCode > end)
                    currCode = end;
                STATIC.DrvIC.Move(0, Axis, currCode);

                Wait(delay);
                tmpres = Measure();
                target.Add(currCode);
                ReadHall.Add(DrvIC.ReadHall(ch, Axis));
                if (Axis == "X")
                {
                    if (index != 0)
                        data.Add((float)tmpres.cx[0] - RefData);
                    else { data.Add(0); RefData = (float)tmpres.cx[0]; }
                }
                else
                {
                    if (index != 0)
                        data.Add((float)tmpres.cy[0] - RefData);
                    else { data.Add(0); RefData = (float)tmpres.cy[0]; }
                }

                AddLog(ch, $"{target[index]}\t{data[index].ToString("F2")}\t{ReadHall[index]}");
                if (currCode >= end) break;
                index++;
            }


            DrvIC.Move(ch, Axis, OISCenter);
            OISLinCompCoef coef = new OISLinCompCoef();
            int[] lincoef = new int[OISLinCompCoef.NUM_COEF];
            float resError = 0;
            int res = coef.LinCompMain(target.ToArray(), data.ToArray(), data.Count, pvt, nvt, 0, 0, ref lincoef, ref resError);
            if (res != 0)
            {
                AddLog(ch, $"Linearity Comp Fail");

                if (Axis == "X")
                    SetError(ch, NonSpecItem.X_LinearityComp);
                else
                    SetError(ch, NonSpecItem.Y_LinearityComp);
            }
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, addr, 0x2A, new byte[] { (byte)lincoef[0] });
            Dln.WriteArray(ch, addr, 0x2B, new byte[] { (byte)lincoef[1] });
            Dln.WriteArray(ch, addr, 0x2C, new byte[] { (byte)lincoef[2] });
            Dln.WriteArray(ch, addr, 0x2D, new byte[] { (byte)lincoef[3] });
            Dln.WriteArray(ch, addr, 0x2E, new byte[] { (byte)lincoef[4] });
            Dln.WriteArray(ch, addr, 0x2F, new byte[] { (byte)lincoef[5] });
            Dln.WriteArray(ch, addr, 0x30, new byte[] { (byte)lincoef[6] });
            Dln.WriteArray(ch, addr, 0x31, new byte[] { (byte)lincoef[7] });
            Dln.WriteArray(ch, addr, 0x32, new byte[] { (byte)lincoef[8] });
            Dln.WriteArray(ch, addr, 0x33, new byte[] { (byte)lincoef[9] });
            Dln.WriteArray(ch, addr, 0x34, new byte[] { (byte)lincoef[10] });
            Dln.WriteArray(ch, addr, 0x35, new byte[] { (byte)lincoef[11] });
            Dln.WriteArray(ch, addr, 0x36, new byte[] { (byte)lincoef[12] });

            string s = $"0x2A : 0x{lincoef[0].ToString("X")}, 0x2B : 0x{lincoef[1].ToString("X")}, 0x2C : 0x{lincoef[2].ToString("X")}, 0x2D : 0x{lincoef[3].ToString("X")}, 0x2E : 0x{lincoef[4].ToString("X")}\r\n" +
             $"0x2F : 0x{lincoef[5].ToString("X")}, 0x30 : 0x{lincoef[6].ToString("X")}, 0x31 : 0x{lincoef[7].ToString("X")}, 0x32 : 0x{lincoef[8].ToString("X")}, 0x33 : 0x{lincoef[9].ToString("X")}\r\n" +
             $"0x34 : 0x{lincoef[10].ToString("X")}, 0x35 : 0x{lincoef[11].ToString("X")}, 0x36 : 0x{lincoef[12].ToString("X")}";

            AddLog(ch, s);

            Store(ch, axisint);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            LEDs_All_On(0, false);
        }
        void Act_AFLinComp(int ch, string testitem)
        {
            int start = Condition.AfLinCompStart, end = Condition.AfLinCompEnd, step = Condition.AFLinCompStep, delay = Condition.AFLinCompMoveDelay;
            LEDs_All_On(0, true);
            FindResult tmpres = new FindResult();

            List<float> target = new List<float>();
            List<float> data = new List<float>();
            List<float> ReadHall = new List<float>();
            float RefData = 0;
            byte[] rbuf = new byte[1];
        
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            if(DrvIC.Y2SlaveAddr != 0x00) Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(10);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0E, rbuf);
            byte pvt = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0F, rbuf);
            byte nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", start);
            int index = 0;
            AddLog(ch, $"Target\tPos\tReadHall");
            while (true)
            {
                int currCode = start + (index * step);
                if (currCode > end)
                    currCode = end;
                STATIC.DrvIC.Move(0, "AF", currCode);
                Wait(delay);
                STATIC.fVision.m__G.oCam[0].Grab(0);
                tmpres = STATIC.fVision.MeasureTxTyTz(0);
                target.Add(currCode);
                ReadHall.Add(DrvIC.ReadHall(ch, "AF"));
                if (index != 0)
                    data.Add((float)tmpres.cz[0] - RefData);
                else { data.Add(0); RefData = (float)tmpres.cz[0]; }

                AddLog(ch, $"{target[index]}\t{data[index].ToString("F2")}\t{ReadHall[index]}");
                if (currCode >= end) break;
                index++;
            }

            DrvIC.Move(ch, "AF", AFCenter);
            AFLinCompCoef coef = new AFLinCompCoef();
            int[] lincoef = new int[AFLinCompCoef.NUM_COEF];
            float resError = 0;
            int res = coef.LinCompMain(target.ToArray(), data.ToArray(), data.Count, 0, 0, 0, 0, ref lincoef, ref resError);

            if (res != 0)
            {
                AddLog(ch, $"Linearity Comp Fail");
                SetError(ch, NonSpecItem.AF_LinearityComp);
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x30, new byte[] { (byte)lincoef[0] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x31, new byte[] { (byte)lincoef[1] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x32, new byte[] { (byte)lincoef[2] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x33, new byte[] { (byte)lincoef[3] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x34, new byte[] { (byte)lincoef[4] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x35, new byte[] { (byte)lincoef[5] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x36, new byte[] { (byte)lincoef[6] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x37, new byte[] { (byte)lincoef[7] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x38, new byte[] { (byte)lincoef[8] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x39, new byte[] { (byte)lincoef[9] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3A, new byte[] { (byte)lincoef[10] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3B, new byte[] { (byte)lincoef[11] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3C, new byte[] { (byte)lincoef[12] });

            string s = $"0x30 : 0x{lincoef[0].ToString("X")}, 0x31 : 0x{lincoef[1].ToString("X")}, 0x32 : 0x{lincoef[2].ToString("X")}, 0x33 : 0x{lincoef[3].ToString("X")}, 0x34 : 0x{lincoef[4].ToString("X")}\r\n" +
                       $"0x35 : 0x{lincoef[5].ToString("X")}, 0x36 : 0x{lincoef[6].ToString("X")}, 0x37 : 0x{lincoef[7].ToString("X")}, 0x38 : 0x{lincoef[8].ToString("X")}, 0x39 : 0x{lincoef[9].ToString("X")}\r\n" +
                       $"0x3A : 0x{lincoef[10].ToString("X")}, 0x3B : 0x{lincoef[11].ToString("X")}, 0x3C : 0x{lincoef[12].ToString("X")}";

            AddLog(ch, s);
            Store(ch, 0);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

            LEDs_All_On(0, false);
            CheckData(ch, 0);
        }
        void Act_FindBestAFPosition(int ch, string testitem)
        {

            int[] step = new int[9] { 0, 511, 1023, 1535, 2047, 2559, 3071, 3585, 4095 };
            int[] hallX = new int[9];
            int[] hallY = new int[9];

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", 200);
            Wait(50);
            DrvIC.Move(ch, "AF", 0);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });

            //중간 셋팅값 확인 

            //
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            Wait(100);

            for (int i = 0; i < 9; i++)
            {
                int[] tmphallX = new int[6];
                int[] tmphallY = new int[6];
                DrvIC.Move(ch, "AF", step[i]);
                Wait(100);
                for (int j = 0; j < 6; j++)
                {
                    tmphallX[j] = DrvIC.ReadHall(ch, "X");
                    tmphallY[j] = DrvIC.ReadHall(ch, "Y");
                    hallX[i] += tmphallX[j];
                    hallY[i] += tmphallY[j];
                }
                hallX[i] /= 6;
                hallY[i] /= 6;

                AddLog(ch, $"Pos = {step[i]}, DataX[{i}] = {hallX[i]}, DataY[{i}] = {hallY[i]}");
            }
            int xMin = hallX.Min(); int xMax = hallX.Max();
            int yMin = hallY.Min(); int yMax = hallY.Max();
            int xCenter = (xMin + xMax) / 2;
            int yCenter = (yMin + yMax) / 2;
            int xMinIndex = 0; int yMinIndex = 0;
            int xMaxIndex = 0; int yMaxIndex = 0;
            bool XMinFind = false; bool YMinFind = false;
            bool XMaxFind = false; bool YMaxFind = false;
            int xBestPos = 0; int yBestPos = 0;
            for (int i = 0; i < 9; i++)
            {
                if (xMin == hallX[i] && !XMinFind) { XMinFind = true; xMinIndex = i; }
                if (xMax == hallX[i] && !XMaxFind) { XMaxFind = true; xMaxIndex = i; }
                if (yMin == hallY[i] && !YMinFind) { YMinFind = true; yMinIndex = i; }
                if (yMax == hallY[i] && !YMaxFind) { YMaxFind = true; yMaxIndex = i; }
            }
            int startXIndex = 0; int endXIndex = 0; int startYIndex = 0; int endYIndex = 0;
            if (xMinIndex > xMaxIndex)
            {
                startXIndex = xMaxIndex;
                endXIndex = xMinIndex;
            }
            else
            {
                startXIndex = xMinIndex;
                endXIndex = xMaxIndex;
            }
            if (yMinIndex > yMaxIndex)
            {
                startYIndex = yMaxIndex;
                endYIndex = yMinIndex;
            }
            else
            {
                startYIndex = yMinIndex;
                endYIndex = yMaxIndex;
            }
            string s = $"[MAX/MIN Index] 0, start:{startXIndex}, end:{endXIndex}\r\n" +
                       $"[MAX/MIN Index] 1, start:{startYIndex}, end:{endYIndex}\r\n" +
                       $"X Min : {xMin}, X Max : {xMax} ({xMax - xMin})\r\n" +
                       $"Y Min : {yMin}, Y Max : {yMax} ({yMax - yMin})\r\n" +
                       $"X Center :{xCenter}, Y Center : {yCenter}\r\n";
            AddLog(ch, s);

            for (int i = startXIndex; i <= endXIndex; i++)
            {
                if (i == 0) continue;
                if (hallX[i - 1] <= xCenter && hallX[i] >= xCenter || hallX[i - 1] >= xCenter && hallX[i] <= xCenter)
                {

                    xBestPos = (int)(step[i - 1] + (step[i] - step[i - 1]) * (xCenter - hallX[i - 1]) / (hallX[i] - hallX[i - 1]));


                    break;
                }
            }
            for (int i = startYIndex; i <= endYIndex; i++)
            {
                if (i == 0) continue;
                if (hallY[i - 1] <= yCenter && hallY[i] >= yCenter || hallY[i - 1] >= yCenter && hallY[i] <= yCenter)
                {
                    yBestPos = (int)(step[i - 1] + (step[i] - step[i - 1]) * (yCenter - hallY[i - 1]) / (hallY[i] - hallY[i - 1]));

                    break;
                }
            }
            AddLog(ch, $"X_AF : {xBestPos}, Y_AF : {yBestPos}");
            if (xMax - xMin > yMax - yMin)
                BestAFPos = xBestPos;
            else BestAFPos = yBestPos;
            AddLog(ch, $"Chosen Best AF : {BestAFPos}");
        }

        void Act_OISInit(int ch, string testitem)
        {

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(10);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(10);


            byte[] rbuf = new byte[2];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(ch, "AF", BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            Wait(100);

            AddLog(ch, $"X PID parameter setting");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });

            for (int i = 0; i < OISPID.Count; i++)
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, OISPID[i][0], new byte[] { OISPID[i][1] });

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xFE, new byte[] { (byte)Condition.OISPIDVer });
            Wait(20);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xFF, new byte[] { 0x33 });
            Wait(20);

            AddLog(ch, $"X Calibration instruction");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x09 });
            Wait(150);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x19, new byte[] { 0x88 });         
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x04, rbuf);
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x06, rbuf);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5D, new byte[] { 0x68 });
            Store(ch, 1);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
         //   Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });



            AddLog(ch, $"Y PID parameter setting");
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });

            for (int i = 0; i < OISPID.Count; i++)
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, OISPID[i][0], new byte[] { OISPID[i][2] });

            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xFE, new byte[] { 0x0B });
            //Wait(20);


            AddLog(ch, $"Y Calibration instruction");

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x09 });
            Wait(150);
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x19, new byte[] { 0x88 });
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x04, rbuf);
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x06, rbuf);
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5D, new byte[] { 0x68 });
            Store(ch, 2);
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });

            Dln.PowerSequence(0);
            AK7314_ICReset(ch);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            CheckData(ch, 1);
            CheckData(ch, 2);
        }

        private void Act_GaindB10Hz(int ch, string testItem)
        {
            int amp;

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) return;
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) return; }
            //X
            amp = (int)Condition.iLoppgainXAmp;
            AddLog(ch, string.Format("X FRA =="));

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "X", amp, 2, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA X Gain10Hz = {0:0.000}",
                    PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val = gain[0]));
                ShowDataResults(ch, (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz);
            }
            //Y1
            amp = (int)Condition.iLoppgainYAmp;
            AddLog(ch, string.Format("Y1 FRA =="));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "Y1", amp, 2, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA Y1 Gain10Hz = {0:0.000}",
                PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val = gain[0]));
                ShowDataResults(ch, (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz);
            }
            //  Y2
            amp = (int)Condition.iLoppgainYAmp;
            AddLog(ch, string.Format("Y2 FRA =="));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();
            freq.Add(10);

            if (DrvIC.Y2SlaveAddr != 0x00)
            {
                if (!DrvIC.FRA_Single(ch, "Y2", amp, 2, freq, ref gain, ref phase))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    AddLog(ch, string.Format("FRA Y2 Gain10Hz = {0:0.000}",
                    PassFails[ch].Results[(int)SpecItem.FRAY2_Gain10Hz].Val = gain[0]));

                    ShowDataResults(ch, (int)SpecItem.FRAY2_Gain10Hz, (int)SpecItem.FRAY2_Gain10Hz);
                }

            }
        }


        private void Act_Phase_Margin(int ch, string testItem)
        {

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 })) return;
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x00 })) return; }
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            DrvIC.Move(ch, "AF", BestAFPos);
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
          //  DrvIC.Move(ch, "Y2", 2048);
            Wait(200);

            string axis;
            int startFreq;
            int EndFreq;
            int amp;

            int phaseIndex = 0;

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();

            #region X PM Low
            axis = "X";
            startFreq = Condition.iXChirpFrom;
            EndFreq = Condition.iXChirpTo;
            amp = Condition.iXAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iOISFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "X Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;

                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }

                AddLog(ch, string.Format("FRA X Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = phaseRes));

                ShowDataResults(ch, (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);

            }
            #endregion

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 })) return;
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x00 })) return; }
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            DrvIC.Move(ch, "AF", BestAFPos);
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            //  DrvIC.Move(ch, "Y2", 2048);
            Wait(200);
            #region Y PM Low
            //Y1
            axis = "Y1";
            startFreq = Condition.iYChirpFrom;
            EndFreq = Condition.iYChirpTo;
            amp = Condition.iYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iOISFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "Y1 Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }



                AddLog(ch, string.Format("FRA Y1 Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = phaseRes));
                ShowDataResults(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);

            }
            #endregion
            if (DrvIC.Y2SlaveAddr != 0x00)
            {
                #region Y2 PM Low
                //Y2
                axis = "Y2";
                startFreq = Condition.iYChirpFrom;
                EndFreq = Condition.iYChirpTo;
                amp = Condition.iYAmplitude;

                AddLog(ch, string.Format("{0} FRA ==", axis));

                freq = new List<double>();
                gain = new List<double>();
                phase = new List<double>();

                for (int i = 0; i < Condition.iFRAloop; i++)
                {
                    while (true)
                    {
                        freq.Add(startFreq);
                        startFreq -= (int)(startFreq * (Condition.iOISFRAstep / 100f));
                        if (startFreq < EndFreq) break;
                    }
                }

                if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                phaseIndex = FindPhaseIndex(gain);
                if (phaseIndex < 1)
                {
                    AddLog(ch, "Y2 Find Phase Margin Failed.. Freq Range Check Please.");
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    double phaseRes = 0, freqRes = 0;
                    if (phaseIndex == gain.Count - 1)
                    {
                        phaseRes = phase[phaseIndex];
                        freqRes = freq[phaseIndex];
                    }
                    else
                    {
                        phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                        freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                    }

                    AddLog(ch, string.Format("FRA Y2 Freq = {0} PM = {1}",
                          PassFails[ch].Results[(int)SpecItem.FRAY2_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY2_PhaseMargin].Val = phaseRes));
                    ShowDataResults(ch, (int)SpecItem.FRAY2_PMFreq, (int)SpecItem.FRAY2_PhaseMargin);
                }
                #endregion
            }

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 })) return;
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x00 })) return; }
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            DrvIC.Move(ch, "AF", BestAFPos);
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            //  DrvIC.Move(ch, "Y2", 2048);
            Wait(200);
            #region AF PM
            //AF
            axis = "AF";
            startFreq = Condition.iAFChirpFrom;
            EndFreq = Condition.iAFChirpTo;
            amp = (int)Condition.iAFAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (0.2));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "AF Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }
                AddLog(ch, string.Format("FRA AF Freq = {0} PM = {1}",
                      PassFails[ch].Results[(int)SpecItem.FRAAF_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = phaseRes));
                ShowDataResults(ch, (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);

            }
            #endregion

        }

        private void Act_Phase_Margin_High(int ch, string testItem)
        {

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) return;
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) return; }
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            DrvIC.Move(ch, "AF", BestAFPos);
            //DrvIC.Move(ch, "X", 2048);
            //DrvIC.Move(ch, "Y1", 2048);
            //DrvIC.Move(ch, "Y2", 2048);
            Wait(200);

            string axis;
            int startFreq;
            int EndFreq;
            int amp;

            int phaseIndex = 0;

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();


            #region X PM High
            axis = "X";
            startFreq = Condition.iHighXChirpFrom;
            EndFreq = Condition.iHighXChirpTo;
            amp = (int)Condition.iHighXAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iHighFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "X Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }

                AddLog(ch, string.Format("FRA X Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq_High].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin_High].Val = phaseRes));
                ShowDataResults(ch, (int)SpecItem.FRAX_PMFreq_High, (int)SpecItem.FRAX_PhaseMargin_High);

            }
            #endregion
            #region Y PM High
            //Y1
            axis = "Y1";
            startFreq = Condition.iHighYChirpFrom;
            EndFreq = Condition.iHighYChirpTo;
            amp = (int)Condition.iHighYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iHighFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "Y1 Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }


                AddLog(ch, string.Format("FRA Y1 Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq_High].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin_High].Val = phaseRes));
                ShowDataResults(ch, (int)SpecItem.FRAY1_PMFreq_High, (int)SpecItem.FRAY1_PhaseMargin_High);

            }
            #endregion
            if (DrvIC.Y2SlaveAddr != 0x00)
            {
                #region Y2 PM High
                //Y2
                axis = "Y2";
                startFreq = Condition.iHighYChirpFrom;
                EndFreq = Condition.iHighYChirpTo;
                amp = (int)Condition.iHighYAmplitude;

                AddLog(ch, string.Format("{0} FRA ==", axis));

                freq = new List<double>();
                gain = new List<double>();
                phase = new List<double>();

                for (int i = 0; i < Condition.iFRAloop; i++)
                {
                    while (true)
                    {
                        freq.Add(startFreq);
                        startFreq -= (int)(startFreq * (Condition.iHighFRAstep / 100f));
                        if (startFreq < EndFreq) break;
                    }
                }

                if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                phaseIndex = FindPhaseIndex(gain);
                if (phaseIndex < 1)
                {
                    AddLog(ch, "Y2 Find Phase Margin Failed.. Freq Range Check Please.");
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    double phaseRes = 0, freqRes = 0;
                    if (phaseIndex == gain.Count - 1)
                    {
                        phaseRes = phase[phaseIndex];
                        freqRes = freq[phaseIndex];
                    }
                    else
                    {
                        phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                        freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                    }
                    AddLog(ch, string.Format("FRA Y2 Freq = {0} PM = {1}",
                          PassFails[ch].Results[(int)SpecItem.FRAY2_PMFreq_High].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY2_PhaseMargin_High].Val = phaseRes));
                    ShowDataResults(ch, (int)SpecItem.FRAY2_PMFreq_High, (int)SpecItem.FRAY2_PhaseMargin_High);
                }
                #endregion
            }


        }
        public int FindPhaseIndex(List<double> gain)
        {
            bool isNeg = false;
            for (int i = 0; i < gain.Count; i++)
            {
                if (gain[i] >= 0 && !isNeg)
                {
                    continue;
                }
                isNeg = true;
                if (gain[i] >= 0)
                {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return gain.Count - 1;
        }
        //public int FindGainIndex(List<double> phase)
        //{
        //    for (int i = 0; i < phase.Count; i++)
        //    {
        //        if (phase[i] >= 0)
        //        {
        //            if (i == 0) return 0;
        //            return i - 1;
        //        }
        //    }
        //    return 0;
        //}
        //private void Act_Gain_Margin(int ch, string testItem)
        //{
        //    string axis;
        //    int startFreq;
        //    int EndFreq;
        //    int amp;

        //    DrvIC.OISOn(ch, testItem, false);
        //    //X
        //    axis = "X";
        //    startFreq = Condition.iXGainFrom;
        //    EndFreq = Condition.iXGainTo;
        //    amp = (int)Condition.iXAmplitudeGain;

        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    List<double> freq = new List<double>();
        //    List<double> gain = new List<double>();
        //    List<double> phase = new List<double>();

        //    for (int i = 0; i < Condition.iGainLoop; i++)
        //    {
        //        while (true)
        //        {
        //            freq.Add(startFreq);
        //            startFreq -= Condition.iGainStep;
        //            if (startFreq < EndFreq) break;

        //        }
        //    }
        //    if (!DrvIC.FRA_Single(ch, axis, amp, 1, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    int gainIndex = FindGainIndex(phase);
        //    if (gainIndex < 1)
        //    {
        //        AddLog(ch, "X Find Gain Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        AddLog(ch, string.Format("FRA X GM = {0}", PassFails[ch].Results[(int)SpecItem.FRAX_GainMargin].Val = Math.Abs(gain[gainIndex])));
        //        SetResult(ch, (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
        //        ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
        //    }

        //    //Y1
        //    axis = "Y1";
        //    startFreq = Condition.iYGainFrom;
        //    EndFreq = Condition.iYGainTo;
        //    amp = (int)Condition.iYAmplitudeGain;
        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    gain = new List<double>();
        //    phase = new List<double>();

        //    if (!DrvIC.FRA_Single(ch, axis, amp, 1, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    gainIndex = FindGainIndex(phase);
        //    if (gainIndex < 1)
        //    {
        //        AddLog(ch, "Y1 Find Gain Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        AddLog(ch, string.Format("FRA Y1 GM = {0}", PassFails[ch].Results[(int)SpecItem.FRAY1_GainMargin].Val = Math.Abs(gain[gainIndex])));

        //        SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
        //        ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
        //    }

        //    //Y2
        //    //axis = "Y2";
        //    //AddLog(ch, string.Format("{0} FRA ==", axis));

        //    //gain = new List<double>();
        //    //phase = new List<double>();

        //    //if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
        //    //{
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //gainIndex = FindGainIndex(phase);
        //    //if (gainIndex < 1)
        //    //{
        //    //    AddLog(ch, "Y2 Find Gain Margin Failed.. Freq Range Check Please.");
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //else
        //    //{

        //    //    AddLog(ch, string.Format("FRA Y2 GM = {0}", Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_GainMargin].Val = Math.Abs(gain[gainIndex])));

        //    //    Spec.SetResult(ch, (int)SpecItem.FRAY2_GainMargin, (int)SpecItem.FRAY2_GainMargin);
        //    //    ShowDataResults(ch, "FRA Y2");
        //    //}
        //}

        public void ServoDecenter(int port, string name)
        {
            FindResult[] fX = new FindResult[2] { new FindResult(), new FindResult() };
            FindResult[] fY = new FindResult[2] { new FindResult(), new FindResult() };
            int ch = port * 2;
            LEDs_All_On(port, true);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", Condition.ServoDecenterAFPos);
            Wait(300);
            AddLog(ch, $"AF Position : {DrvIC.ReadHall(ch, "AF")}");

            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Wait(100);
            fX[0] = Measure();

            STATIC.DrvIC.OISOn(0, "X", false);
            Wait(100);

            fX[1] = Measure();
            PassFails[0].Results[(int)SpecItem.x_ServoDecenter].Val = fX[1].cx[0] - fX[0].cx[0];


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", Condition.ServoDecenterAFPos);
            Wait(100);
            AddLog(ch, $"AF Position : {DrvIC.ReadHall(ch, "AF")}");


            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Wait(100);
            fY[0] = Measure();

            STATIC.DrvIC.OISOn(0, "Y", false);

            Wait(100);
            fY[1] = Measure();

            PassFails[0].Results[(int)SpecItem.y_ServoDecenter].Val = fY[0].cy[0] - fY[1].cy[0];
            ShowDataResults(0, (int)SpecItem.x_ServoDecenter, (int)SpecItem.y_ServoDecenter);

            LEDs_All_On(port, false);
        }

        private void Act_OISShift(int port, string testItem)
        {


            //      Dln.ReadArray(0, DrvIC.Y1SlaveAddr, 1, 0xE5, b);

            LEDs_All_On(port, true);
            FindResult res = new FindResult();

            List<FindResult> resList = new List<FindResult>();
            List<FindResult> resList2 = new List<FindResult>();
            List<double> diffx = new List<double>();
            List<double> diffy = new List<double>();
            List<double> shiftX = new List<double>();
            List<double> shiftY = new List<double>();
            List<int> hallcompx = new List<int>();
            List<int> hallcompy = new List<int>();
            double RefX;
            double RefY;

            double slopeX = SlopeX; //F_Manage.xSlope;
            double slopeY = SlopeY;//F_Manage.ySlope;
            //double slopeX = F_Manage.xSlope;
            //double slopeY = F_Manage.ySlope;

            AddLog(0, $"X Slope : {slopeX.ToString("F4")}, Y Slope : {slopeY.ToString("F4")}");

            Dln.WriteArray(0, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(0, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(0, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(0, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(0, "X", 2047);
            DrvIC.Move(0, "Y", 2047);
            Wait(100);


            int[] code = new int[] { 0, 512, 1024, 1536, 2048, 2560, 3072, 3584, 4092 };


            DrvIC.Move(0, "AF", BestAFPos - 100);
            Wait(100);
            DrvIC.Move(0, "AF", BestAFPos - 50);
            Wait(100);
            DrvIC.Move(0, "AF", BestAFPos);
            Wait(100);
            STATIC.fVision.m__G.oCam[port].Grab(0);
            res = STATIC.fVision.MeasureTxTyTz(0);

            RefX = res.cx[0];
            RefY = res.cy[0];



            DrvIC.Move(0, "AF", 100);
            Wait(100);
            DrvIC.Move(0, "AF", 50);
            Wait(100);
            DrvIC.Move(0, "AF", 0);
            Wait(100);
            for (int i = 0; i < code.Length; i++)
            {
                resList.Add(new FindResult());
                DrvIC.Move(0, "AF", code[i]);
                Wait(100);
                STATIC.fVision.m__G.oCam[port].Grab(0);
                resList[i] = STATIC.fVision.MeasureTxTyTz(0);
            }

            for (int i = 0; i < resList.Count; i++)
            {
                diffx.Add(resList[i].cx[0] - RefX);
                diffy.Add(resList[i].cy[0] - RefY);
                AddLog(0, $"{code[i]}\t{diffx[i].ToString("F3")}\t{diffy[i].ToString("F3")}\t{resList[i].cz[0].ToString("F3")}");
            }

            for (int i = 0; i < resList.Count; i++)
            {
                hallcompx.Add((int)(-1 * (diffx[i] / slopeX)));
                hallcompy.Add((int)(-1 * (diffy[i] / slopeY)));
                AddLog(0, $"Hall Comp X : {(int)hallcompx[i]}, Hall Comp Y : {(int)hallcompy[i]}");
            }

            Dln.WriteArray(0, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(0, "X", OISCenter);
            DrvIC.Move(0, "Y", OISCenter);

            DrvIC.Move(0, "AF", 100);
            Wait(100);
            DrvIC.Move(0, "AF", 50);
            Wait(100);
            DrvIC.Move(0, "AF", 0);
            Wait(100);


            for (int i = 0; i < code.Length; i++)
            {
                resList2.Add(new FindResult());

                DrvIC.Move(0, "AF", code[i]);
                DrvIC.Move(0, "X", OISCenter + hallcompx[i]);
                DrvIC.Move(0, "Y", OISCenter + hallcompy[i]);
                Wait(100);

                STATIC.fVision.m__G.oCam[port].Grab(0);
                resList2[i] = STATIC.fVision.MeasureTxTyTz(0);


            }

            for (int i = 0; i < resList2.Count; i++)
            {
                shiftX.Add(resList2[i].cx[0] - RefX);
                shiftY.Add(resList2[i].cy[0] - RefY);
                AddLog(0, $"{code[i]}\t{shiftX[i].ToString("F3")}\t{shiftY[i].ToString("F3")}\t{resList2[i].cz[0].ToString("F3")}");
            }

            double xValMax = double.MinValue;
            double yValMax = double.MinValue;
            double xLimitMax = double.MinValue;
            double yLimitMax = double.MinValue;

            int xValMaxIndex = 0;
            int yValMaxIndex = 0;
            int xLimitMaxIndex = 0;
            int yLimitMaxIndex = 0;



            for (int i = 0; i < resList2.Count; i++)
            {
                if (Math.Abs(shiftX[i]) > xValMax) { xValMax = Math.Abs(shiftX[i]); xValMaxIndex = i; }
                if (Math.Abs(shiftY[i]) > yValMax) { yValMax = Math.Abs(shiftY[i]); yValMaxIndex = i; }
                if (Math.Abs(hallcompx[i]) > xLimitMax) { xLimitMax = Math.Abs(hallcompx[i]); xLimitMaxIndex = i; }
                if (Math.Abs(hallcompy[i]) > yLimitMax) { yLimitMax = Math.Abs(hallcompy[i]); yLimitMaxIndex = i; }
            }

            //PassFails[0].Results[(int)SpecItem.x_Shift].Val = shiftX[xValMaxIndex];
            //PassFails[0].Results[(int)SpecItem.y_Shift].Val = shiftY[yValMaxIndex];
            //PassFails[0].Results[(int)SpecItem.x_Limit].Val = hallcompx[xLimitMaxIndex];
            //PassFails[0].Results[(int)SpecItem.y_Limit].Val = hallcompy[yLimitMaxIndex];
            //ShowDataResults(0, (int)SpecItem.x_Shift, (int)SpecItem.y_Limit);

            LEDs_All_On(port, false);
        }
        void AutoTest(int ch, string testItem)
        {
            int autoTestRes = 0;
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            int SinXMaxCount = int.MaxValue, SinYMaxCount = int.MaxValue, SinXNGCnt = int.MaxValue, SinYNGCnt = int.MaxValue, SinResult = int.MaxValue;
            int RNGOKX = int.MaxValue, RNGOKY = int.MaxValue, RNGTimeX = int.MaxValue, RNGTimeY = int.MaxValue, RNGResult = int.MaxValue;
            byte[] rbuf = new byte[1];
            byte X0BBackData = 0x12, Y0BBackData = 0x14;

        
            try
            {
                Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
                Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
                if (Condition.SIN_AXIS == 0)
                {

                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x6F, new byte[] { 0xE0 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x51 });
                }
                else if (Condition.SIN_AXIS == 1)
                {
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x6F, new byte[] { 0x60 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x52 });
                }
                else
                {
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x6F, new byte[] { 0xE0 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x89, new byte[] { 0x60 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x53 });
                }
                Wait(2);



            
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                //Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0B, rbuf);
                //X0BBackData = rbuf[0];
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { (byte)(X0BBackData | 0x08) });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });

                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                //Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0B, rbuf);
                //Y0BBackData = rbuf[0];
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { (byte)(Y0BBackData | 0x08) });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0xCA });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });



                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x60, new byte[] { (byte)Condition.SIN_THD })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x61, new byte[] { (byte)Condition.SIN_CNT_ERR })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x62, new byte[] { (byte)Condition.SIN_FREQ })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x63, new byte[] { (byte)Condition.SIN_AMP })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x3E, new byte[] { (byte)Condition.SIN_AMP })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x64, new byte[] { (byte)Condition.SIN_CYCL })) return;

                string tmpStr = "Sine thr = " + Condition.SIN_THD + "\r\n"
                    + "Sine Cnt Error = " + Condition.SIN_CNT_ERR + "\r\n"
                    + "Sine Freq = " + Condition.SIN_FREQ + "\r\n"
                    + "Sine Amp = " + Condition.SIN_AMP + "\r\n"
                    + "Sine Cycle = " + Condition.SIN_CYCL;

                AddLog(ch, tmpStr);

                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 })) return;

                double LimitTime = ((double)(((Condition.SIN_CYCL >> 4) & 0x0F) + (Condition.SIN_CYCL & 0x0F)) / Condition.SIN_FREQ * 1000);
                AddLog(ch, string.Format("SinewWave Test Time = {0} ms", LimitTime.ToString("F3")));
                Wait((int)LimitTime);
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 })) return;
                Wait(1);

                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x00 })) return;
                Wait(2);


                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { X0BBackData });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });

                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { Y0BBackData });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

                byte[] data = new byte[1];
                byte[] data2 = new byte[2];

                if (Condition.SIN_AXIS == 0)
                {
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE4, data2);
                    SinewaveXMaxDiff = SinXMaxCount = ((data2[0] << 8) + data2[1]) >> 4;
                    AddLog(ch, string.Format("X SineWave Max Count = {0}", SinXMaxCount));                 
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9A, data);
                    SinXNGCnt = data[0];
                    AddLog(ch, string.Format("X Sinewave NG Count = {0}", SinXNGCnt));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, data);
                    SinResult = data[0];
                    AddLog(ch, string.Format("X Sinewave Result = {0}", SinResult));
                    if (SinXNGCnt > Condition.SIN_Spec) autoTestRes = 999; //SetError(ch, NonSpecItem.AutoTest);

                }
                else if (Condition.SIN_AXIS == 1)
                {
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE6, data2);
                    SinewaveYMaxDiff =  SinYMaxCount = ((data2[0] << 8) + data2[1]) >> 4;
                    AddLog(ch, string.Format("Y SineWave Max Count = {0}", SinYMaxCount));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9B, data);
                    SinYNGCnt = data[0];
                    AddLog(ch, string.Format("Y Sinewave NG Count = {0}", SinYNGCnt));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, data);
                    SinResult = data[0];
                    AddLog(ch, string.Format("Y Sinewave Result = {0}", SinResult));
                    if (SinYNGCnt > Condition.SIN_Spec) autoTestRes = 999;// SetError(ch, NonSpecItem.AutoTest);
                }
                else
                {
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE4, data2);
                    SinewaveXMaxDiff = SinXMaxCount = ((data2[0] << 8) + data2[1]) >> 4;
                    AddLog(ch, string.Format("X SineWave Max Count = {0}", SinXMaxCount));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9A, data);
                    SinXNGCnt = data[0];
                    AddLog(ch, string.Format("X Sinewave NG Count = {0}", SinXNGCnt));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE6, data2);
                    SinewaveYMaxDiff = SinYMaxCount = ((data2[0] << 8) + data2[1]) >> 4;
                    AddLog(ch, string.Format("Y SineWave Max Count = {0}", SinYMaxCount));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9B, data);
                    SinYNGCnt = data[0];
                    AddLog(ch, string.Format("Y Sinewave NG Count = {0}", SinYNGCnt));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, data);
                    SinResult = data[0];
                    AddLog(ch, string.Format(" Sinewave Result = {0}", SinResult));

                    if (SinXNGCnt > Condition.SIN_Spec || SinYNGCnt > Condition.SIN_Spec) autoTestRes = 999;// SetError(ch, NonSpecItem.AutoTest);
                }
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE })) return;

                if (Condition.SIN_AXIS == 0)
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x21 });
                else if (Condition.SIN_AXIS == 1)
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x22 });
                else
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x23 });

                Wait(2);

                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                //Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0B, rbuf);
                //X0BBackData = rbuf[0];
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { (byte)(X0BBackData | 0x08) });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0xE6 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x01, new byte[] { 0x60 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });

                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                //Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0B, rbuf);
                //Y0BBackData = rbuf[0];
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { (byte)(Y0BBackData | 0x08) });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0xE6 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x01, new byte[] { 0x60 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });



                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x65, new byte[] { (byte)Condition.RNG_THD })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x66, new byte[] { (byte)Condition.RNG_STVT })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x68, new byte[] { (byte)Condition.RNG_METM })) return;
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x69, new byte[] { (byte)Condition.RNG_WSEC })) return;

                tmpStr = "Rng thr = " + Condition.RNG_THD + "\r\n"
                              + "Rng Start Position = " + Condition.RNG_STVT + "\r\n"
                              + "Rng METM = " + Condition.RNG_METM + "\r\n"
                              + "Rng WSEC = " + Condition.RNG_WSEC;

                AddLog(ch, tmpStr);

                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 })) return;

                LimitTime = 100 + Condition.RNG_METM + Condition.RNG_WSEC;
                AddLog(ch, string.Format("Ringing Test Time = {0} ms", LimitTime.ToString("F3")));
                Wait((int)LimitTime);
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 })) return;
                Wait(1);
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x00 })) return;
                Wait(2);


                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { X0BBackData });

                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { Y0BBackData });

                data = new byte[1];

                if (Condition.SIN_AXIS == 0)
                {
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9C, data);
                    RNGOKX = data[0];
                    AddLog(ch, string.Format("RNG OK X = {0}", RNGOKX));
                    RingingXStabilizer = RNGTimeX = Condition.RNG_METM + Condition.RNG_WSEC - data[0];                
                    AddLog(ch, string.Format("Ringing Time X = {0}", RNGTimeX));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, data);
                    RNGResult = data[0];
                    AddLog(ch, string.Format("Ringing Result = {0}", RNGResult));

                    if (RNGTimeX > Condition.RNG_StabilizerSpec) autoTestRes = 999;// SetError(ch, NonSpecItem.AutoTest);

                }
                else if (Condition.SIN_AXIS == 1)
                {
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9D, data);
                    RNGOKY = data[0];
                    AddLog(ch, string.Format("RNG OK Y = {0}", RNGOKY));
                    RingingYStabilizer = RNGTimeY = Condition.RNG_METM + Condition.RNG_WSEC - data[0];
                    AddLog(ch, string.Format("Ringing Time Y = {0}", RNGTimeY));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, data);
                    RNGResult = data[0];
                    AddLog(ch, string.Format("Ringing Result = {0}", RNGResult));

                    if (RNGTimeY > Condition.RNG_StabilizerSpec) autoTestRes = 999; // SetError(ch, NonSpecItem.AutoTest);
                }
                else
                {
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9C, data);
                    RNGOKX = data[0];
                    AddLog(ch, string.Format("RNG OK X = {0}", RNGOKX));
                    RingingXStabilizer = RNGTimeX = Condition.RNG_METM + Condition.RNG_WSEC - data[0];
                    AddLog(ch, string.Format("Ringing Time X = {0}", RNGTimeX));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9D, data);
                    RNGOKY = data[0];
                    AddLog(ch, string.Format("RNG OK Y = {0}", RNGOKY));
                    RingingYStabilizer = RNGTimeY = Condition.RNG_METM + Condition.RNG_WSEC - data[0];
                    AddLog(ch, string.Format("Ringing Time Y = {0}", RNGTimeY));
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, data);
                    RNGResult = data[0];
                    AddLog(ch, string.Format("Ringing Result = {0}", RNGResult));

                    if (RNGTimeX > Condition.RNG_StabilizerSpec || RNGTimeY > Condition.RNG_StabilizerSpec) autoTestRes = 999;// SetError(ch, NonSpecItem.AutoTest);
                }
                PassFails[ch].Results[(int)SpecItem.AutoTestRes].Val = autoTestRes;
                ShowDataResults(ch, (int)SpecItem.AutoTestRes, (int)SpecItem.AutoTestRes);
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE })) return;

              

            }
            catch
            {
                PassFails[ch].Results[(int)SpecItem.AutoTestRes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.AutoTestRes, (int)SpecItem.AutoTestRes);
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE })) return;
               
            }
        }
        void OISSensitivityTest(int ch, string testItem)
        {

            int[] xCode = new int[] { 2048, 0, 4095, 0, 4095 };
            int[] yCode = new int[] { 2048, 0, 0, 4095, 4095 };
            byte[] rbuf = new byte[1];

            List<byte> xVal = new List<byte>();
            List<byte> yVal = new List<byte>();
            List<int> xHall = new List<int>();
            List<int> yHall = new List<int>();
            List<int> checkRegX = new List<int>();
            List<int> checkRegY = new List<int>();

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Wait(100);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            for (int i = 0; i < xCode.Length; i++)
            {
                DrvIC.Move(ch, "X", xCode[i]);
                DrvIC.Move(ch, "Y", yCode[i]);
                Wait(200);
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x79, rbuf);
                xVal.Add(rbuf[0]);
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x79, rbuf);
                yVal.Add(rbuf[0]);
                xHall.Add(DrvIC.ReadHall(ch, "X"));              
                yHall.Add(DrvIC.ReadHall(ch, "Y"));
                checkRegX.Add(xVal[i] & 0x07);
                checkRegY.Add(yVal[i] & 0x07);
            }

            for (int i = 0; i < xVal.Count; i++)
            {
                AddLog(ch, $"{i * 2}, 0x{xVal[i].ToString("X2")}, 0x{yVal[i].ToString("X2")} ({xHall[i]}, {yHall[i]})");
                AddLog(ch, $"{i * 2 + 1}, 0x{checkRegX[i].ToString("X2")}, 0x{checkRegY[i].ToString("X2")} ({xHall[i]}, {yHall[i]})");
            }

            PassFails[ch].Results[(int)SpecItem.OISSensitivityTestRes].Val = 1;
            ShowDataResults(ch, (int)SpecItem.OISSensitivityTestRes, (int)SpecItem.OISSensitivityTestRes);

        }
        private void Act_OISShift2(int port, string testItem)
        {

            int ch = port * 2;
            int[] code = new int[] { 0, 512, 1024, 1536, 2048, 2560, 3072, 3584, 4095 };
            int RefX = 0, RefY = 0;
            List<int> tmpX = new List<int>();
            List<int> tmpY = new List<int>();
            List<int> XHall = new List<int>();
            List<int> YHall = new List<int>();
            List<int> XDiff = new List<int>();
            List<int> YDiff = new List<int>();

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            if(DrvIC.Y2SlaveAddr != 0x00) Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            DrvIC.Move(ch, "AF", BestAFPos);
            Wait(10);
            DrvIC.Move(ch, "AF", 1024);
            Wait(10);
            DrvIC.Move(ch, "AF", 512);
            Wait(10);
            DrvIC.Move(ch, "AF", 256);
            Wait(10);
            DrvIC.Move(ch, "AF", 128);
            Wait(10);
            DrvIC.Move(ch, "AF", 64);
            Wait(10);
            DrvIC.Move(ch, "AF", 32);
            Wait(10);
            DrvIC.Move(ch, "AF", 16);
            Wait(10);
            DrvIC.Move(ch, "AF", 8);
            Wait(10);
            DrvIC.Move(ch, "AF", 0);
            Wait(10);

            for (int i = 0; i < code.Length; i++)
            {
                tmpX.Clear();
                tmpY.Clear();
                DrvIC.Move(0, "AF", code[i]);
                Wait(50);
                for (int j = 0; j < 10; j++)
                {
                    tmpX.Add(DrvIC.ReadHall(ch, "X"));
                    tmpY.Add(DrvIC.ReadHall(ch, "Y"));
                }
                XHall.Add(tmpX.Min());
                YHall.Add(tmpY.Min());
                if (code[i] == AFCenter)
                {
                    RefX = tmpX.Min();
                    RefY = tmpY.Min();
                }
            }

            AddLog(ch, $"X dynamic range : 0 ~ 4095 (4095)");
            AddLog(ch, $"Y dynamic range : 0 ~ 4095 (4095)");
            AddLog(ch, $"Best AF code {BestAFPos}");
            AddLog(ch, $"X/Y hall at STD : {RefX}, {RefY}");

            AddLog(ch, $"code\txHall\tyHall\txDiff\tyDiff");
            for (int i = 0; i < code.Length; i++)
            {
                XDiff.Add(XHall[i] - RefX);
                YDiff.Add(YHall[i] - RefY);
                AddLog(ch, $"{code[i]}\t{XHall[i]}\t{YHall[i]}\t{XDiff[i]}\t{YDiff[i]}");
            }
            int xDiffMax = Math.Max(Math.Abs(XDiff.Max()), Math.Abs(XDiff.Min()));
            int yDiffMax = Math.Max(Math.Abs(YDiff.Max()), Math.Abs(YDiff.Min()));

            AddLog(ch, $"X Y Max drift code : {xDiffMax}, {yDiffMax}");
            AddLog(ch, $"Drift Test AF range : 512 ~ 3584");
            AddLog(ch, $"X drift range : {XHall.Min()}~{XHall.Max()} (std:{RefX})");
            AddLog(ch, $"Y drift range : {YHall.Min()}~{YHall.Max()} (std:{RefY})");
            AddLog(ch, $"X Max drift : {xDiffMax} code (std:{(int)(600 * xDiffMax / 4095)})");
            AddLog(ch, $"Y Max drift : {yDiffMax} code (std:{(int)(600 * yDiffMax / 4095)})");

            if (xDiffMax > Condition.DriftTestSpec || yDiffMax > Condition.DriftTestSpec)
                SetError(ch, NonSpecItem.DriftTestNG);


        
        }

        void PID_Verify(int ch, string testItem)
        {
            bool AFRes = true;
            bool OISRes = true;
            byte[] rbuf = new byte[1];
            Dln.PowerSequence(0);
            AK7314_ICReset(ch);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x03, rbuf);
            int afid = rbuf[0];
            if(afid != 0x1E)
            {
                AddLog(ch, $"Error, AF IC is not AK7314, 0x{afid.ToString("X2")}");
                AFRes = false;
              
            }
            for (int i = 0; i < AFPID.Count; i++)
            {
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, AFPID[i][0], rbuf);
                if (AFPID[i][1] != rbuf[0])
                {
                    AddLog(ch, "AF PID Verify NG");
                    AddLog(ch, $"Addr : 0x{AFPID[i][0].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{AFPID[i][1].ToString("X2")}");
                    AFRes = false;
                }
            }

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x03, rbuf);
            int xid = rbuf[0];
            if (xid != 0x85)
            {
                AddLog(ch, $"Error, X IC is not AK7326, 0x{afid.ToString("X2")}");
                OISRes = false;
            }

            for (int i = 0; i < OISPID.Count; i++)
            {
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, OISPID[i][0], rbuf);
                if (OISPID[i][3] != 0x00)
                {
                    if (OISPID[i][1] != rbuf[0])
                    {
                        AddLog(ch, "X PID Verify NG");
                        AddLog(ch, $"Addr : 0x{OISPID[i][0].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{OISPID[i][1].ToString("X2")}");
                        OISRes = false;
                    }
                }
               
            }

            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x03, rbuf);
            int yid = rbuf[0];
            if (yid != 0x85)
            {
                AddLog(ch, $"Error, Y IC is not AK7326, 0x{afid.ToString("X2")}");
                OISRes = false;
            }
            for (int i = 0; i < OISPID.Count; i++)
            {
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, OISPID[i][0], rbuf);
                if (OISPID[i][3] != 0x00)
                {
                    if (OISPID[i][2] != rbuf[0])
                    {                                            
                        AddLog(ch, "Y PID Verify NG");
                        AddLog(ch, $"Addr : 0x{OISPID[i][0].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{OISPID[i][2].ToString("X2")}");
                        OISRes = false;
                    }

                }
            }
            if(AFRes)
            {
                PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes);
            }
            if(OISRes)
            {
                PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes);
            }
        
         
       
        }
        void IME_Test(int ch, string testItem)
        {
          
            int OISStroke = Condition.IMEOISStroke;
            AK7314_ICReset(ch);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(10);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);

            byte[] rbuf = new byte[1];
          
            byte[] X_PNCAL = new byte[2];
            byte[] Y_PNCAL = new byte[2];

            int XPCAL = 0, XNCAL = 0;
            int YPCAL = 0, YNCAL = 0;
            int XIME = 0, YIME = 0;

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x04, rbuf);
            X_PNCAL[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x06, rbuf);
            X_PNCAL[1] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x04, rbuf);
            Y_PNCAL[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x06, rbuf);
            Y_PNCAL[1] = rbuf[0];

            XPCAL = (X_PNCAL[0] < 128) ? (X_PNCAL[0] * 2) : ((X_PNCAL[0] * 2) - 512);
            XNCAL = (X_PNCAL[1] < 128) ? (X_PNCAL[1] * 2) : ((X_PNCAL[1] * 2) - 512);
            YPCAL = (Y_PNCAL[0] < 128) ? (X_PNCAL[0] * 2) : ((X_PNCAL[0] * 2) - 512);
            YNCAL = (Y_PNCAL[1] < 128) ? (Y_PNCAL[1] * 2) : ((Y_PNCAL[1] * 2) - 512); 

            XIME = (((OISStroke * XPCAL) / (XPCAL - XNCAL)) - (OISStroke / 2));
            YIME = (((OISStroke * YPCAL) / (YPCAL - YNCAL)) - (OISStroke / 2));

            AddLog(ch, $"Stroke : {OISStroke}, {XIME}, {YIME}");

            if ((XIME < Condition.IMEMinThd) || (XIME > Condition.IMEMaxThd)) // -220 ~ 220
            {
                AddLog(ch, "X IME Test NG");

                PassFails[ch].Results[(int)SpecItem.OISXIMERes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.OISXIMERes, (int)SpecItem.OISXIMERes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISXIMERes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.OISXIMERes, (int)SpecItem.OISXIMERes);
            }
            if ((YIME < Condition.IMEMinThd) || (YIME > Condition.IMEMaxThd)) // -220 ~ 220
            {
                AddLog(ch, "Y IME Test NG");
                PassFails[ch].Results[(int)SpecItem.OISYIMERes].Val = 999;
                ShowDataResults(ch, (int)SpecItem.OISYIMERes, (int)SpecItem.OISYIMERes);
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISYIMERes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.OISYIMERes, (int)SpecItem.OISYIMERes);
            }

        }

        void AK7314_ICReset(int ch)
        {
            byte[] rbuf = new byte[1];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
            Wait(100);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) != 0x00)
            {
                SetError(ch, NonSpecItem.Store_Fail);
                AddLog(ch, "Store fail");
                return;
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Wait(50);
        }

        void throughFRA_Enable(int ch, int axis)
        {
            byte[] rbuf = new byte[1];
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int check_count = 0;
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x56, new byte[] { 0x80 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x02 });
            Wait(5);
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x54, new byte[] { 0x0F });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x55, new byte[] { 0x00 });
            Wait(5);
            
            while(true)
            {
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x4C, rbuf);
                Wait(1);
                if ((rbuf[0] & 0x10) == 0x10)
                    break;
                check_count++;
                if (check_count > 100) { AddLog(ch, "FRA Mode change timeout"); }
            }
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 });
            Wait(150);
        }
        void throughFRA_disable(int ch, int axis)
        {
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xEE });
            Wait(5);
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0xFF });
            Wait(15);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
        }
        double throughFRA_gain(int ch, int axis)
        {
            string axisName = axis == 0 ? "X" : "Y";
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int fraAddr = axis == 0 ? DrvIC.FRA_XSlaveAddr : DrvIC.FRA_Y1SlaveAddr;
            DrvIC.Move(ch, "AF", 2048);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(100);
           
            AddLog(ch, $"Amp : {Condition.throughPeakAmp}");
            AddLog(ch, $"Test Freq : {Condition.throughPeakFreq}");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", 2048);
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(300);

            DrvIC.SetSlaveAddr(ch, fraAddr);
            AddLog(ch, $"{axisName} Test start");

            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
            Wait(30);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            throughFRA_Enable(ch, axis);
            DrvIC.Set_Amp(ch, Condition.throughPeakAmp);
            AddLog(ch, $"Amp\tFreq\tGain");
            DrvIC.Set_Freq(ch, Condition.throughPeakFreq);
            Wait(100 + 5000 / Condition.throughPeakFreq + 10);
            double gain = DrvIC.Get_Gain(ch);
            AddLog(ch, $"{Condition.throughPeakAmp}\t{Condition.throughPeakFreq}\t{gain.ToString("F2")}");
            throughFRA_disable(ch, axis);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(30);
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            Wait(30);
            return gain;
        }

        void throughFRA(int ch, string testItem)
        {
            double gain = 0;
            gain = throughFRA_gain(ch, 0);
            PassFails[ch].Results[(int)SpecItem.ThroughPeak_X_Gain].Val = gain;
            ShowDataResults(ch, (int)SpecItem.ThroughPeak_X_Gain, (int)SpecItem.ThroughPeak_X_Gain);
            gain = throughFRA_gain(ch, 1);
            PassFails[ch].Results[(int)SpecItem.ThroughPeak_Y_Gain].Val = gain;
            ShowDataResults(ch, (int)SpecItem.ThroughPeak_Y_Gain, (int)SpecItem.ThroughPeak_Y_Gain);
        }

        void OISPhasemargin(int ch, string testItem)
        {
            double freq = 0, pm = 0;
            (freq, pm) = OISPM(ch, 0);
            PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq].Val = freq; PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = pm;
            ShowDataResults(ch, (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);
            (freq, pm) = OISPM(ch, 1);
            PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq].Val = freq; PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = pm;
            ShowDataResults(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);
        }
        void OISLoopGain(int ch, string testItem)
        {
            double gain = 0;
            gain = LoopGain(ch, 0);
            PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val = gain;
            ShowDataResults(ch, (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz);

            gain = LoopGain(ch, 1);
            PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val = gain;
            ShowDataResults(ch, (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz);

        }

        (double, double) OISPM(int ch, int axis)
        {
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int FRAaddr = axis == 0 ? DrvIC.FRA_XSlaveAddr : DrvIC.FRA_Y1SlaveAddr;
            string axisName = axis == 0 ? "X" : "Y";

            int startFreq = axis == 0 ? Condition.iXChirpFrom : Condition.iYChirpFrom;
            int finalFreq = axis == 0 ? Condition.iYChirpTo : Condition.iYChirpTo;
            int minphase = axis == 0 ? Condition.PMXMinPhase : Condition.PMYMinPhase;
            int gainTH = axis == 0 ? Condition.PMXGainTH : Condition.PMYGainTH;
            int amp = axis == 0 ? Condition.iXAmplitude : Condition.iYAmplitude;

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);

            DrvIC.SetSlaveAddr(ch, FRAaddr);

            int freqVal, freqTemp = 0, gainTemp, freqPM = 0, freq_index = 0;
            int oldFreq;
            int[] before_after_zero_freq = new int[2] { startFreq, finalFreq };
            double gainVal = 0, pm_val = 0, phaseTemp, prepm = 0, pmindex = 180;
            double[] before_after_zero_gain = new double[2] { 0, 0 };

            AddLog(ch, $"{axisName} PM Test start");

            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
           
            Wait(30);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            DrvIC.FRAModeEnable(ch);
            DrvIC.Set_Amp(ch, amp);
            AddLog(ch, $"Amp\tFreq\tGain\tP/M");
            for (oldFreq = freqVal = startFreq; freqVal >= finalFreq; freqVal -= freqTemp)
            {
                DrvIC.Set_Freq(ch, freqVal);
                Wait(1000 / oldFreq + 5000 / freqVal + 10);
                oldFreq = freqVal;

                gainVal = DrvIC.Get_Gain(ch);
                pm_val = DrvIC.Get_Phase(ch, 0);

                AddLog(ch, $"{amp}\t{freqVal}\t{gainVal.ToString("F2")}\t{pm_val.ToString("F0")}");
                if(gainVal > 0)
                {
                    if((freqVal != startFreq) && (before_after_zero_gain[0] < 0))
                    {
                        pm_val = ((gainVal * prepm) - (before_after_zero_gain[0] * pm_val)) / (gainVal - before_after_zero_gain[0]);
                        freqPM = (int)(((gainVal * before_after_zero_freq[0]) - (before_after_zero_gain[0] * freqVal)) / (gainVal - before_after_zero_gain[0]));

                        before_after_zero_freq[1] = freqVal;
                        before_after_zero_gain[1] = gainVal;
                        freq_index++;
                        break;
                    }
                    else
                    {
                        before_after_zero_freq[0] = freqVal;
                        before_after_zero_gain[0] = gainVal;
                        if((gainVal < 4) && (pm_val < pmindex))
                        {
                            pmindex = pm_val;
                            freq_index = freqVal;
                        }
                    }
                }
                else
                {
                    before_after_zero_freq[0] = freqVal;
                    before_after_zero_gain[0] = gainVal;
                }
                if((pm_val < minphase) && Math.Abs(gainVal) < 3)
                {
                    AddLog(ch, $"Error type 3 : Min Phase NG over period {minphase}");
                    AddLog(ch, $"Freq : {freqVal}, Phase : {pm_val}");
                    DrvIC.FRAModeDisable(ch);
                    return (freqVal, minphase);
                }
                prepm = pm_val;
                freqTemp = freqVal * Condition.iOISFRAstep / 100;
                if (freqTemp < 1) freqTemp = 1;

            }
            AddLog(ch, $"Zero Freq before = {before_after_zero_freq[0]}Hz,{before_after_zero_gain[0].ToString("F2")}dB");
            AddLog(ch, $"Zero Freq after = {before_after_zero_freq[1]}Hz,{before_after_zero_gain[1].ToString("F2")}dB");

            if(freq_index != 0 && freqVal < finalFreq)
            {
                AddLog(ch, $"Minimum phase under 4 db");
                AddLog(ch, $"freq : {freq_index}, Phase : {pmindex}");
                DrvIC.FRAModeDisable(ch);
                return (freq_index, pmindex);

            }
            if(freq_index == 0)
            {
                AddLog(ch, "Couldn`t find zero cross point");
                DrvIC.FRAModeDisable(ch);
                return (freq_index, 1);
            }

            if(Math.Abs(gainVal - before_after_zero_gain[1]) > gainTH)
            {
                AddLog(ch, $"Error type 2 : gain is changed drastically over {gainTH}");
                DrvIC.FRAModeDisable(ch);
                return (0, 2);
            }
            AddLog(ch, "Use Linear Interpolation");
            AddLog(ch, $"{amp}, {freqPM}Hz, {gainVal.ToString("F2")}dB, {pm_val.ToString("F0")}deg");

            DrvIC.FRAModeDisable(ch);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            Wait(10);
            return (freqPM, pm_val);
        }
        double LoopGain(int ch, int axis)
        {
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int FRAaddr = axis == 0 ? DrvIC.FRA_XSlaveAddr : DrvIC.FRA_Y1SlaveAddr;
            string axisName = axis == 0 ? "X" : "Y";

            int startFreq = axis == 0 ? Condition.iXChirpFrom : Condition.iYChirpFrom;
            int finalFreq = axis == 0 ? Condition.iYChirpTo : Condition.iYChirpTo;
            int minphase = axis == 0 ? Condition.PMXMinPhase : Condition.PMYMinPhase;
            int gainTH = axis == 0 ? Condition.PMXGainTH : Condition.PMYGainTH;
            int amp = axis == 0 ? Condition.iXAmplitude : Condition.iYAmplitude;

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);

            DrvIC.SetSlaveAddr(ch, FRAaddr);
            double gainVal = 0;

            AddLog(ch, $"{axisName} LoopGain Test start");

            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
          
            Wait(30);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            DrvIC.FRAModeEnable(ch);
            DrvIC.Set_Amp(ch, amp);
            AddLog(ch, $"Amp\tFreq\tGain");

            DrvIC.Set_Freq(ch, 10);
            Wait(100 + 5000 / 10 + 10);
            Wait(1000);
            gainVal = DrvIC.Get_Gain(ch);
            AddLog(ch, $"{amp}, {10}Hz, {gainVal.ToString("F2")}dB");
            DrvIC.FRAModeDisable(ch);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(10);

            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            Wait(10);
            return gainVal;

           

        }

        void AFGainMargin(int ch, string testItem)
        {
            double res = 0;
            byte scancnt = 0;
            int freqval, freqtemp = 0, gaintemp, oldfreq;
            int[] before_after_zero_freq = new int[2];
            double[] before_after_zero_phase = new double[2];
            int[] freq_PM, freq_GM = new int[2];
            double[] gainval = new double[2] { 0, 0 };
            double[] pmval = new double[2];
            double gmval, phasetemp, prepm = 0;
            double[] pregm = new double[2] { 0, 0};

            DrvIC.SetSlaveAddr(ch, DrvIC.FRA_AFSlaveAddr);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);

            DrvIC.Move(ch, "AF", 2048); Wait(50);
            AddLog(ch, $"GM AF Code, Target {DrvIC.ReadHall(ch, "AF")}");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });

            AddLog(ch, "GainMargin test start");
            DrvIC.FRAModeEnable(ch);
           
            DrvIC.Set_Amp(ch, Condition.AFGMamp);
            AddLog(ch, $"Amp\tFreq\tGain\tP/M");
            for (oldfreq = freqval = Condition.AFGMEndFreq; freqval <= Condition.AFGMStartFreq; freqval += freqtemp)
            {
                DrvIC.Set_Freq(ch, freqval);
                Wait(1000 / oldfreq + 5000 / freqval + 10);
                oldfreq = freqval;
                gainval[scancnt] = DrvIC.Get_Gain(ch);
                pmval[scancnt] = DrvIC.Get_Phase(ch, 1);
                AddLog(ch, $"{scancnt + 1} \t {Condition.AFGMamp}\t{freqval}\t{gainval[scancnt].ToString("F2")}\t{pmval[scancnt].ToString("F0")}");
                if (pmval[scancnt] < 0)
                {
                    gainval[scancnt] = ((pmval[scancnt] * pregm[scancnt]) - (before_after_zero_phase[scancnt] * gainval[scancnt])) / (pmval[scancnt] - before_after_zero_phase[scancnt]);
                    freq_GM[scancnt] = (int)(((pmval[scancnt] * before_after_zero_freq[scancnt]) - (before_after_zero_phase[scancnt] * freqval)) / (pmval[scancnt] - before_after_zero_phase[scancnt]));

                    scancnt++;
                    if(scancnt == 2)
                    {
                        //before_after_zero_freq[scancnt + 1] = freqval;
                        //before_after_zero_phase[scancnt + 1] = pmval[scancnt];
                        break;
                    }

                }
                else
                {
                    before_after_zero_freq[scancnt] = freqval;
                    before_after_zero_phase[scancnt] = pmval[scancnt];
                }
                pregm[scancnt] = gainval[scancnt];
                freqtemp = freqval * Condition.AFGMStep / 100;
                if (freqtemp < 1) freqtemp = 1;
            }
            if(freqval == Condition.AFGMStartFreq && scancnt == 0)
            {
                AddLog(ch, "Error type 1 : Gain over zero at 1st Scan");
                DrvIC.FRAModeDisable(ch);
                res = 1;
            }
            AddLog(ch, "\r\nUse Linear Interpolation");
            AddLog(ch, $"{1} \t {Condition.AFGMamp}\t{freq_GM[0]}\t{gainval[0].ToString("F2")}\t{pmval[0].ToString("F0")}");
            DrvIC.FRAModeDisable(ch);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            AK7314_ICReset(ch);
       
            //if (freq_GM[1] == 0)
            //{
            //    res = (float)((-1) * gainval[0]);
            //}

            //// Send smaller GM
            //if ((float)((-1) * gainval[0]) < (float)((-1) * gainval[1]))
            //{
            //    res = (float)((-1) * gainval[0]);
            //}
            //else
            //{
            //    res = (float)((-1) * gainval[1]);
            //}
            PassFails[ch].Results[(int)SpecItem.FRAAF_GainMargin].Val = Math.Abs(gainval[0]);
            ShowDataResults(ch, (int)SpecItem.FRAAF_GainMargin, (int)SpecItem.FRAAF_GainMargin);
        }

        void AFPhaseMargin(int ch, string testItem)
        {
            double resFreq = 0, respm = 0;
            int freqval, freqtemp = 0, gaintemp, freqpm = 0, oldfreq;
            int[] before_after_zero_freq = new int[2];
            double gainval = 0, pmval, phaestemp, prepm = 0, PM4dB;
            double[] before_after_zero_gain = new double[2];
            byte backup, flag_2nd = 0;
            byte fra_en;
            

            DrvIC.SetSlaveAddr(ch, DrvIC.FRA_AFSlaveAddr);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);

            DrvIC.Move(ch, "AF", 2048); Wait(50);
            AddLog(ch, $"PM AF Code, Target {DrvIC.ReadHall(ch, "AF")}");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(1);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });

            AddLog(ch, "Phase margin test start");
            DrvIC.FRAModeEnable(ch);
          
            DrvIC.Set_Amp(ch, (int)Condition.iAFAmplitude);
            AddLog(ch, $"Amp\tFreq\tGain\tP/M");
            for (oldfreq = freqval = Condition.iAFChirpFrom; freqval >= Condition.iAFChirpTo; freqval -= freqtemp)
            {
                DrvIC.Set_Freq(ch, freqval);
                Wait(1000 / oldfreq + 5000 / freqval + 15);
                oldfreq = freqval;
                gainval = DrvIC.Get_Gain(ch);
                pmval = DrvIC.Get_Phase(ch, 1);
                AddLog(ch, $"{Condition.AFGMamp}\t{freqval}\t{gainval.ToString("F2")}\t{pmval.ToString("F0")}");


                if (gainval > 0)
                {
                    respm = pmval = ((gainval * prepm) - (before_after_zero_gain[0] * pmval)) / (gainval - before_after_zero_gain[0]);
                    resFreq = freqpm = (int)(((gainval * before_after_zero_freq[0]) - (before_after_zero_gain[0] * freqpm)) / (gainval - before_after_zero_gain[0]));
                    before_after_zero_freq[1] = freqval;
                    before_after_zero_gain[1] = gainval;
                    break;
                }
                else
                {
                    before_after_zero_freq[0] = freqval;
                    before_after_zero_gain[0] = gainval;
                }
                prepm = pmval;
                freqtemp = freqval * Condition.iAFFRAstep / 100;

                if (freqtemp < 1) freqtemp = 1;
            }
            AddLog(ch, $"Zero Freq before = {before_after_zero_freq[0]}Hz,{before_after_zero_gain[0].ToString("F2")}dB");
            AddLog(ch, $"Zero Freq after = {before_after_zero_freq[1]}Hz,{before_after_zero_gain[1].ToString("F2")}dB");

            if (freqval == Condition.iAFChirpFrom)
            {
                
                AddLog(ch, " Error type1 : Gain over zero at 1st cycle");
                DrvIC.FRAModeDisable(ch);
                resFreq = freqval;
                respm = 1;
            }
            if ((freqval <= Condition.iAFChirpTo) && (gainval <= 0))
            {

                if (gainval > -2)
                {
                    freqpm = before_after_zero_freq[0];
                    gainval = before_after_zero_gain[0];
                }
                else
                {
                    AddLog(ch, " Error type4 : No cross over point during period\n");
                    DrvIC.FRAModeDisable(ch);
                    resFreq = freqval;
                    respm = 4;                                                //result=4;
                }

                AddLog(ch, " Error type4 : No cross over point during period\n");
                resFreq = freqval;
                respm = 4;

            }
            if (Math.Abs(gainval - before_after_zero_gain[1]) > Condition.PMAFGainTH)
            {
                AddLog(ch, $"Error type 2: gain is changed drastically over {Condition.PMAFGainTH}");
                //---------------------------------------------------------
                // disable
                DrvIC.FRAModeDisable(ch);
                //---------------------------------------------------------
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
                AK7314_ICReset(ch);
                resFreq = freqval;
                respm = 2;
            }

            AddLog(ch, "Use Linear Interpolation");
            AddLog(ch, $"{Condition.iAFAmplitude}, {resFreq}Hz, {gainval.ToString("F2")}dB, {respm.ToString("F0")}deg");

            DrvIC.FRAModeDisable(ch);
            AK7314_ICReset(ch);
            PassFails[ch].Results[(int)SpecItem.FRAAF_PMFreq].Val = resFreq;
            PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = respm;

            ShowDataResults(ch, (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);
        }
        void WriteUserMem(int ch, int res)
        {
            var now = DateTime.Now;
            var year = now.Year - 2000;
            var month = now.Month;
            var day = now.Day;
            var hour = now.Hour;
            var minute = now.Minute;
            var second = now.Second;
            // X Mem
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            byte[] xWriteData = new byte[32];
            xWriteData[0] = (byte)(PassFails[ch].Results[(int)SpecItem.OISX_Rolling].Val * 10);
            xWriteData[1] = (byte)(PassFails[ch].Results[(int)SpecItem.OISX_Ratedstroke].Val / 10);
            //xWriteData[2] = 0; //OC
            //xWriteData[3] = 0; //OC
            xWriteData[4] = 0x01;
            xWriteData[5] = (byte)(BestAFPos >> 4);
            xWriteData[6] = 0;
            xWriteData[7] = 0;
            xWriteData[8] = 0;
            xWriteData[9] = 0;
            xWriteData[10] = 0;
            xWriteData[11] = 0;
            xWriteData[12] = 0;
            xWriteData[13] = 0;
            xWriteData[14] = 0;
            xWriteData[15] = 0;
            xWriteData[16] = 0;
            xWriteData[17] = 0;
            xWriteData[18] = 0;
            xWriteData[19] = 0;
            xWriteData[20] = 0;
            xWriteData[21] = 0;
            xWriteData[22] = 0;
            xWriteData[23] = 0;
            xWriteData[25] = (byte)((byte)(PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val) ^ 0x54 ^ 0xF9);
            xWriteData[26] = (byte)(PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val);
            xWriteData[27] = (byte)RingingXStabilizer;
            xWriteData[28] = (byte)SinewaveXMaxDiff;
            xWriteData[29] = (byte)((byte)(PassFails[ch].Results[(int)SpecItem.AF_Tilt].Val * 10) ^ 0x54 ^ 0xFD);
            xWriteData[30] = (byte)Condition.OISPIDVer;
            xWriteData[31] = 0x33;

            for (int i = 0; i < xWriteData.Length; i++)
            {
                int addr = 0xE0 + i;
                if (addr == 0xF8 || addr == 0xE2 || addr == 0xE3) continue;
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, addr, new byte[] { xWriteData[i] });
                Wait(20);
            }
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });

            //Y Mem
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            byte[] yWriteData = new byte[32];
            yWriteData[0] = (byte)(PassFails[ch].Results[(int)SpecItem.OISY_Rolling].Val * 10);
            yWriteData[1] = (byte)(PassFails[ch].Results[(int)SpecItem.OISY_Ratedstroke].Val / 10);
            //yWriteData[2] = 0; //OC
            //yWriteData[3] = 0; // OC
            yWriteData[4] = 0x01;
            yWriteData[5] = (byte)(BestAFPos >> 4);
            yWriteData[6] = 0;
            yWriteData[7] = 0;
            yWriteData[8] = 0;
            yWriteData[9] = 0;
            yWriteData[10] = 0;
            yWriteData[11] = 0;
            yWriteData[12] = 0;
            yWriteData[13] = 0;
            yWriteData[14] = 0;
            yWriteData[15] = 0;
            yWriteData[16] = 0;
            yWriteData[17] = 0;
            yWriteData[18] = 0;
            yWriteData[19] = 0;
            yWriteData[20] = 0;
            yWriteData[21] = 0;
            yWriteData[22] = 0;
            yWriteData[23] = 0;
            yWriteData[25] = (byte)((byte)(PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val) ^ 0x54 ^ 0xF9);
            yWriteData[26] = (byte)(PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val);
            yWriteData[27] = (byte)RingingYStabilizer;
            yWriteData[28] = (byte)SinewaveYMaxDiff;
            yWriteData[29] = (byte)((byte)(PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val) ^ 0x54 ^ 0xFD);
            yWriteData[30] = 0; //AFCode
          

            for (int i = 0; i < yWriteData.Length; i++)
            {
                int addr = 0xE0 + i;
                if (addr == 0xF8 || addr == 0xFF || addr == 0xE2 || addr == 0xE3 || addr == 0xFE) continue;
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, addr, new byte[] { yWriteData[i] });
                Wait(20);
            }
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            //AF Mem
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
           

            byte[] AFWriteAddr = new byte[] { 0xF0, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB };
            byte[] AFWriteData = new byte[AFWriteAddr.Length];

            // 기존 데이터 입력
            AFWriteData[0] = (byte)res;
            AFWriteData[1] = (byte)(PassFails[ch].Results[(int)SpecItem.AF_Ratedstroke].Val / 4);
            AFWriteData[2] = 0x1E;
            AFWriteData[3] = 0x0B;
            AFWriteData[4] = (byte)(Convert.ToInt16(Model.TesterNo) >> 8);
            AFWriteData[5] = (byte)Convert.ToInt16(Model.TesterNo);
            // 0xF7: 년(6bit) | 월(상위2bit)
            AFWriteData[6] = (byte)(((year & 0x3F) << 2) | ((month >> 2) & 0x03));
            // 0xF8: 월(하위2bit) | 일(5bit) | 시간(1bit)
            AFWriteData[7] = (byte)(((month & 0x03) << 6) | ((day & 0x1F) << 1) | ((hour >> 4) & 0x01));
            // 0xF9: 시간(하위4bit) | 분(상위4bit)
            AFWriteData[8] = (byte)(((hour & 0x0F) << 4) | ((minute >> 2) & 0x0F));
            // 0xFA: 분(하위2bit) | 초(6bit)
            AFWriteData[9] = (byte)(((minute & 0x03) << 6) | (second & 0x3F));
            AFWriteData[10] = (byte)Condition.AFPIDVer;

            for (int i = 0; i < AFWriteAddr.Length; i++)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, AFWriteAddr[i], new byte[] { AFWriteData[i] });
                Wait(20);
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

            Dln.PowerSequence(ch);
            AK7314_ICReset(ch);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            byte[] xCheckData = new byte[32];
            byte[] yCheckData = new byte[32];
            byte[] afCheckData = new byte[AFWriteAddr.Length];

            byte[] rbuf = new byte[1];

            AddLog(ch, "X Nvm Data Check");
            
            for (int i = 0; i < xCheckData.Length; i++)
            {
                int addr = 0xE0 + i;
                if (addr == 0xF8 || addr == 0xE2 || addr == 0xE3) continue;
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, addr, rbuf);
                AddLog(ch, $"Addr : 0x{addr.ToString("X2")}, WData : 0x{xWriteData[i].ToString("X2")}, RData : 0x{rbuf[0].ToString("X2")}");
                if (xWriteData[i] != rbuf[0])
                {
                    AddLog(ch, "NVM Verify NG");
                    SetError(ch, NonSpecItem.NVM_Verify_NG);
                }
            }

            AddLog(ch, "Y Nvm Data Check");
            for (int i = 0; i < yCheckData.Length; i++)
            {
                int addr = 0xE0 + i;
                if (addr == 0xF8 || addr == 0xFF || addr == 0xE2 || addr == 0xE3 || addr == 0xFE) continue;
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, addr, rbuf);
                AddLog(ch, $"Addr : 0x{addr.ToString("X2")}, WData : 0x{yWriteData[i].ToString("X2")}, RData : 0x{rbuf[0].ToString("X2")}");
                if (yWriteData[i] != rbuf[0])
                {
                    AddLog(ch, "NVM Verify NG");
                    SetError(ch, NonSpecItem.NVM_Verify_NG);
                }
            }

            AddLog(ch, "AF Nvm Data Check");
            for (int i = 0; i < afCheckData.Length; i++)
            {
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, AFWriteAddr[i], rbuf);
                AddLog(ch, $"Addr : 0x{AFWriteAddr[i].ToString("X2")}, WData : 0x{AFWriteData[i].ToString("X2")}, RData : 0x{rbuf[0].ToString("X2")}");
                if (AFWriteData[i] != rbuf[0])
                {
                    AddLog(ch, "NVM Verify NG");
                    SetError(ch, NonSpecItem.NVM_Verify_NG);
                }
            }

        }

        public static void Wait(long ms)
        {
            ms = ms * 1000;
            Stopwatch startNew = Stopwatch.StartNew();

            long usDelayTick = (ms * Stopwatch.Frequency) / 1000000;

            while (startNew.ElapsedTicks < usDelayTick) ;
        }

        #endregion
    }
}
