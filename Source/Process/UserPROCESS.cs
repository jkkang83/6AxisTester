using Dln;
using Dln.Exceptions;
using MathNet.Numerics.Financial;
using MathNet.Numerics.Optimization.TrustRegion;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using System.Xml.Schema;
using static alglib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace FZ4P
{
    public partial class Process
    {
        byte[] AF_IC_Setting = new byte[5] { 0x73, 0xE2, 0x62, 0x85, 0x8C };

        byte OISPIDVer = 11;
        byte AFPIDVer = 11;

        List<byte[]> AFPID = new List<byte[]>
        {    
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
            new byte[2]{ 0x1B, 0x5A },
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

        List<byte[]> OIS_Set = new List<byte[]>
        {
            new byte[4] { 0x0A, 0x59, 0x59, 0x01 },
            new byte[4] { 0x0B, 0x12, 0x14, 0x00 },
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
        };

        List<byte[]> OIS_reg = new List<byte[]>
        {
            new byte[4] { 0x0D, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x0E, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x0F, 0x00, 0x00, 0x00 }, //안함
            new byte[4] { 0x3E, 0x85, 0x85, 0x01 },
        };


        List<byte[]> OISPID = new List<byte[]> 
        {

            
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
        int[] g_IME = new int[2];
        double[] AFCurrentMinMax = new double[2];
        double[] OISXCurrentMinMax = new double[2];
        double[] OISYCurrentMinMax = new double[2];

        void AddSequence()
        {
            ItemList.Add(new ActItems() { Name = "AF HallCalibration", Func = Act_AFHallCalibration, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS HallCalibration", Func = Act_OISHallCalubration, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS IC Mount Error", Func = IME_Test, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS XYZ Temperature", Func = TempTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS XYZ Aging", Func = Act_CloseLoopAging });
            ItemList.Add(new ActItems() { Name = "OIS LinearityCompensation", Func = Act_OISLinComp });
            ItemList.Add(new ActItems() { Name = "X/Y Servo Decenter", Func = ServoDecenter, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS X/Y OpenLoop", Func = OISOpenLoopTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Auto Test", Func = AutoTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Sensitivity Test", Func = OISSensitivityTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Aging", Func = Act_AFScanAging });
            ItemList.Add(new ActItems() { Name = "AF Scan Driving", Func = Act_PreAFDriving });
            ItemList.Add(new ActItems() { Name = "X/Y Drift Test", Func = Act_OISShift2, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "through Peak 25", Func = throughFRA, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Phase Margin", Func = OISPhasemargin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Loopgain", Func = OISLoopGain, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Gain Margin", Func = AFGainMargin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Phase Margin", Func = AFPhaseMargin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF PID Verify", Func = AFPID_Verify, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS PID Verify", Func = OIS_PIDVerify, IsMulti = true });

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
            for (open_input = start_pos[axis]; open_input < end_pos[1]; open_input += test_size)
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
            for (open_input -= test_size; open_input >= start_pos[axis]; open_input -= test_size)
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
                    PassFails[ch].Results[(int)SpecItem.OLTestXResult].Val = 0;
                    ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult, InspType.Normal, new double[] { });
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
                ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult, InspType.Normal, new double[] { });
            }

            else
            {
                PassFails[ch].Results[(int)SpecItem.OLTestYResult].Val = sum_square;
                ShowDataResults(ch, (int)SpecItem.OLTestYResult, (int)SpecItem.OLTestYResult, InspType.Normal, new double[] { });
            }

            AddLog(ch, $"sum square : {sum_square}");
            //if (sum_square > square_spec || sum_square <= 0)
            //{
            //    dc_result = 0x01;
            //    AddLog(ch, $"NG Over DC SR, {square_spec}");
            //    SetError(ch, NonSpecItem.OIS_Openloop_Test);
            //}
            AddLog(ch, $"[Final] {axisName} sum square : {sum_square}, result : {dc_result}");
            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
         
        }

        void OISOpenLoopTest(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, $"<<<  X Open loop test Start  >>>");
            if (m_ChannelOn[ch]) oisOL(ch, 0);
            AddLog(ch, $"<<<  X Open loop test End  >>>");
            AddLog(ch, "");
            AddLog(ch, $"<<<  Y Open loop test Start  >>>");
            if (m_ChannelOn[ch]) oisOL(ch, 1);
            AddLog(ch, $"<<<  Y Open loop test End  >>>");
        }

        void TempTest(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, $"<<<  AF/OIS Temp. Start  >>>");
            AddLog(ch, $"AF/OIS temperature test (open-loop)");
            byte Temperature_reg = 0xC9;
            float[] min = new float[3] { 100000, 100000, 100000 };
            float[] max = new float[3] { -100000, -100000, -100000 };
            float[] val = new float[3] { 0, 0, 0 };
            byte[] backupData_af = new byte[5];
            byte[] backupData_ois = new byte[10];
            bool result = true;
            float AFVar = 0, XVar = 0, YVar = 0;
            byte AFReset = 0, XReset = 0, YReset = 0;
            byte[] rbuf = new byte[1];
            byte[] buf2 = new byte[2];


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x1A, rbuf); backupData_af[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf); backupData_af[1] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, Temperature_reg, rbuf); backupData_af[2] = rbuf[0];

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(backupData_af[1] & 0x7F) });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, Temperature_reg, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);

            for (int i = 0; i < 2; i++)
            {
                int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                Dln.ReadArray(ch, slaveAddr, 0x0B, rbuf); backupData_ois[0 + i] = rbuf[0];
                Dln.ReadArray(ch, slaveAddr, 0x0D, rbuf); backupData_ois[2 + i] = rbuf[0];
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x3B });
                if(i == 0)
                {
                    Dln.WriteArray(ch, slaveAddr, 0x0B, new byte[] { 0x02 });
                    Dln.WriteArray(ch, slaveAddr, 0x0D, new byte[] { 0xC0 });
                }
                else
                {
                    Dln.WriteArray(ch, slaveAddr, 0x0B, new byte[] { 0x14 });
                    Dln.WriteArray(ch, slaveAddr, 0x0D, new byte[] { 0xC0 });
                }
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x00 });

            }

            for (int i = 0; i < 2; i++)
            {
                int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                Dln.ReadArray(ch, slaveAddr, 0x26, rbuf); backupData_ois[4 + i] = rbuf[0];
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, slaveAddr, 0xA6, new byte[] { 0x7B });
                Dln.WriteArray(ch, slaveAddr, 0x00, new byte[] { 0x80, 0x00 });
                Dln.WriteArray(ch, slaveAddr, 0x26, new byte[] { 0x00 });
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);
            Stopwatch st = new Stopwatch();
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { (byte)((byte)Condition.TemperPos >> 8), (byte)((byte)Condition.TemperPos & 0xFF) });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { (byte)((byte)Condition.TemperPos >> 8), (byte)((byte)Condition.TemperPos & 0xFF) });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { (byte)((byte)Condition.TemperPos >> 8), (byte)((byte)Condition.TemperPos & 0xFF) });

            st.Start();
            while (st.ElapsedMilliseconds < Condition.TemperTime)
            {
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x90, rbuf); val[2] = rbuf[0];
                val[2] = (float)((val[2] <= 128) ? (val[2] * 0.625) : ((val[2] - 256) * 0.625));
                if (val[2] > max[2]) max[2] = val[2];
                if (val[2] < min[2]) min[2] = val[2];

                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x90, rbuf); val[0] = rbuf[0];
                val[0] = (float)((val[0] <= 128) ? (val[0] * 0.625) : ((val[0] - 256) * 0.625));
                if (val[0] > max[0]) max[0] = val[0];
                if (val[0] < min[0]) min[0] = val[0];

                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x90, rbuf); val[1] = rbuf[0];
                val[1] = (float)((val[1] <= 128) ? (val[1] * 0.625) : ((val[1] - 256) * 0.625));
                if (val[1] > max[1]) max[1] = val[1];
                if (val[1] < min[1]) min[1] = val[1];

                Wait(50);
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x02, rbuf); AFReset = (byte)(rbuf[0] & 0x60);
                if (AFReset != 0x00) { result = false; break; }
                if (max[2] > Condition.AFTempMaxSpec) { result = false; break; }
                if (min[2] <= Condition.AFTempMinSpec) { result = false; break; }
                AFVar = (max[2] - min[2]);
                if (AFVar > Condition.AFTempValSpec) { result = false; break; }

                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x02, rbuf); XReset = (byte)(rbuf[0] & 0x60);
                if (XReset != 0x00) { result = false; break; }
                if (max[0] > Condition.OISTempMaxSpec) { result = false; break; }
                if (min[0] <= Condition.OISTempMinSpec) { result = false; break; }
                XVar = (max[0] - min[0]);
                if (XVar > Condition.OISTempValSpec) { result = false; break; }

                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x02, rbuf); YReset = (byte)(rbuf[0] & 0x60);
                if (YReset != 0x00) { result = false; break; }
                if (max[1] > Condition.OISTempMaxSpec) { result = false; break; }
                if (min[1] <= Condition.OISTempMinSpec) { result = false; break; }
                YVar = (max[1] - min[1]);
                if (YVar > Condition.OISTempValSpec) { result = false; break; }

            }
            st.Stop(); st.Reset();
            AddLog(ch, $"Temp Min, X:{min[0]}, Y:{min[1]}, AF:{min[2]}");
            AddLog(ch, $"Temp Max, X:{max[0]}, Y:{max[1]}, AF:{max[2]}");
            AddLog(ch, $"Temp var, X:{XVar}, Y:{YVar}, AF:{AFVar}");

            AddLog(ch, $"\r\nSpec option");
            AddLog(ch, $"current code : {Condition.TemperPos}");
            AddLog(ch, $"test time : {Condition.TemperTime}ms");
            AddLog(ch, $"min spec(af,ois) : {Condition.AFTempMinSpec}, {Condition.OISTempMinSpec}");
            AddLog(ch, $"threshold spec(af,ois) : {Condition.AFTempMaxSpec}, {Condition.OISTempMaxSpec}");
            AddLog(ch, $"variation spec(af,ois) : {Condition.AFTempValSpec}, {Condition.OISTempValSpec}");

            if (result)
            {
                AddLog(ch, $"Test result Ok");
                PassFails[ch].Results[(int)SpecItem.TempRes].Val = 0;
                ShowDataResults(ch, (int)SpecItem.TempRes, (int)SpecItem.TempRes, InspType.Normal, new double[] { });
            }
            else
            {
                AddLog(ch, $"Test result NG");
                PassFails[ch].Results[(int)SpecItem.TempRes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.TempRes, (int)SpecItem.TempRes, InspType.Normal, new double[] { });
            }

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { backupData_af[0] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { backupData_af[1] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, Temperature_reg, new byte[] { backupData_af[2] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

            for (int i = 0; i < 2; i++)
            {
                int slaveaddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                Dln.WriteArray(ch, slaveaddr, 0x0B, new byte[] { backupData_ois[0 + i] });
                Dln.WriteArray(ch, slaveaddr, 0x0D, new byte[] { backupData_ois[2 + i] });
                Dln.WriteArray(ch, slaveaddr, 0x26, new byte[] { backupData_ois[4 + i] });
                Dln.WriteArray(ch, slaveaddr, 0xA6, new byte[] { 0x00 });
                Dln.WriteArray(ch, slaveaddr, 0xAE, new byte[] { 0x00 });
            }
            DrvIC.Move(ch, "AF", 2048);
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(50);

            AddLog(ch, $"<<<  AF/OIS Temp. End  >>>");

            if(Option.SaveRawData)
            {
                StreamWriter sw = null;
                string dateDir = STATIC.CreateDateDir();
                if(!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                string path = dateDir + $"Act_Temperature.csv";
              
                if (!File.Exists(path))
                {
                    sw = File.AppendText(path);
                    string s = $"SPL No, Date, Time, AFDegC, AFVariation, AFReset, XDegC, XVariation, XReset, YDegC, YVariation, YResult,";
                    sw.WriteLine(s);
                    sw.Close();
                }
                sw = File.AppendText(path);
                string data = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                    $"{val[2]},{AFVar},{AFReset},{val[0]},{XVar},{XReset},{val[1]},{YVar},{YReset}";
                sw.WriteLine(data);
                sw.Close();
            }
         

        }
        

        void ChangeSlaveAddr(int ch, string testItem, int InspCnt, bool IsTwice)
        {
            // Y2 : 4E -> 6C
            // Y1 : 0E -> 4E
            // X  : 0A -> 0E

          
        }

        private void Act_AFOpenLoopAging(int ch, string testItem, int InspCnt, bool IsTwice)
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
        void Act_AFScanAging(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, "<<<  AF Scan aging Start  >>>");
            int target = 0, readhall = 0;
            int stepSize = Condition.AFScanAgingStep;
            int stepDelay = Condition.AFScanAgingDelay;
            stepSize = 256;
            stepDelay = 30;

            AddLog(ch, $"Start aging {Condition.AFSCanAgingCount} cycle for AF Driving");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", AFCenter);
            Wait(100);

          
            for (int i = 0; i < Condition.AFSCanAgingCount; i++)
            {
                for (target = 2047; target >= 0; target -= stepSize)
                {
                    if (target <= 0) target = 0;
                    DrvIC.Move(ch, "AF", target); Wait(stepDelay);
                   
                }
                for (target = 0; target <= 4095; target += stepSize)
                {
                    if (target >= 4095) target = 4095;
                    DrvIC.Move(ch, "AF", target); Wait(stepDelay);
                }
                for (target = 4095; target >= 2047; target -= stepSize)
                {
                    if (target <= 2047) target = 2047;
                    DrvIC.Move(ch, "AF", target); Wait(stepDelay);
                }
            }
            AddLog(ch, "<<<  AF Scan aging End  >>>");
            PassFails[0].Results[(int)SpecItem.AFScanAging].Val = 1;
            ShowDataResults(ch, (int)SpecItem.AFScanAging, (int)SpecItem.AFScanAging, InspType.Normal, new double[] { });
        }
        void Act_PreAFDriving(int ch, string testItem, int InspCnt)
        {
            LEDs_All_On(0, true);
            AddLog(ch, "AF Pre Driving");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            FindResult res = new FindResult();

            for (int i = 0; i < 5; i++)
            {
                double[] MtoM = new double[2];
                DrvIC.Move(ch, "AF", 2048); Wait(50);

                DrvIC.Move(ch, "AF", 100); Wait(20);
                DrvIC.Move(ch, "AF", 20); Wait(20);
                DrvIC.Move(ch, "AF", 10); Wait(20);
                DrvIC.Move(ch, "AF", 0); Wait(50);
                res = Measure();
                MtoM[0] = res.cz[0];
                DrvIC.Move(ch, "AF", 4095 - 100); Wait(20);
                DrvIC.Move(ch, "AF", 4095 - 20); Wait(20);
                DrvIC.Move(ch, "AF", 4095 - 10); Wait(20);
                DrvIC.Move(ch, "AF", 4095); Wait(50);
                res = Measure();
                MtoM[1] = res.cz[0];

                AddLog(ch, $"{i + 1} scan stroke : {Math.Abs(MtoM[1] - MtoM[0]).ToString("F3")}");
            }
            LEDs_All_On(0, false);
        }



        byte AFPOSVT = 0, AFNEGVT = 0;

        void Act_AFHallCalibration(int ch, string testItem, int InspCnt)
        {
            bool xChanged = true;
            bool Y1Changed = true;
            bool Y2Changed = true;
            bool AFChanged = true;

            byte[] rDdata = new byte[1];

            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0xAE, new byte[] { 0x3B })) Y2Changed = false; }

            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B })) Y1Changed = false;
            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B })) xChanged = false;
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B })) AFChanged = false;


            if (AFChanged)
                AddLog(ch, string.Format("Already AF Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0xAE, new byte[] { 0x3B })) return;
                AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} AFData : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0x0B, new byte[] { 0x02 })) return; // 02 : Normal, 04 : Reverse
                AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} AFData : 0x{1:X2}", 0x0B, 0x02));

                if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0x0A, new byte[] { 0x70 })) return; // Setting Slave Address
                AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x0A, 0x70));
                Wait(200);
                if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                Wait(100);
                AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                AddLog(ch, string.Format(" AF SlaveAddr Change FinIsh."));
            }

            if (xChanged)
                AddLog(ch, string.Format("Already X Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0xAE, new byte[] { 0x3B })) return;
                AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0x0B, new byte[] { 0x02 })) return; // 02 : Normal, 04 : Reverse
                AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0B, 0x02));

                if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0x0A, new byte[] { 0x59 })) return; // Setting Slave Address
                AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0A, 0x59));
                Wait(200);
                if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                Wait(150);

                AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                AddLog(ch, string.Format("X SlaveAddr Change FinIsh."));
            }

            if (Y1Changed)
                AddLog(ch, string.Format("Already Y Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0xAE, new byte[] { 0x3B })) return;
                AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0x0B, new byte[] { 0x04 })) return; // 02 : Normal, 04 : Reverse
                AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0x0B, 0x02));

                if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0x0A, new byte[] { 0x59 })) return; // Setting Slave Address
                AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0x0A, 0x59));
                Wait(200);
                if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x03, new byte[] { 0x01 })) return; // Store Memory
                Wait(150);
                AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                AddLog(ch, string.Format("Y SlaveAddr Change FinIsh."));
            }

      
            Dln.PowerSequence(0);
            Wait(100);


            int BTM_POS = 10;
            int TOP_POS = 820;
            int TOP_MARGIN = 10;

            byte[] rbuf = new byte[1];
            int agingCount;
            double OldStroke =0, NewStroke = 0;
            FindResult res = new FindResult();
            double[] zVal = new double[2];

            DrvIC.AK7314_Mode(ch, 0);
            Wait(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] & 0x7F) });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            DrvIC.AK7314_Mode(ch, 1);
            AddLog(ch, $"AF Openloop Stroke Check");

            LEDs_All_On(0, true);
            for (agingCount = 0, NewStroke = 0; (agingCount < 10) || ((agingCount < 20) && (NewStroke > OldStroke)); agingCount++)
            {
                OldStroke = NewStroke;
                DrvIC.Move(ch, "AF", 4095);
                Wait(50);
                res = Measure();
                zVal[0] = res.cz[0];
                DrvIC.Move(ch, "AF", 0);
                Wait(50);
                res = Measure();
                zVal[1] = res.cz[0];
                NewStroke = Math.Abs(zVal[1] - zVal[0]);
                AddLog(ch, $"{agingCount + 1} : {NewStroke.ToString("F3")}");
            }
            
            DrvIC.AK7314_Mode(ch, 0);
            Wait(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0A, new byte[] { AF_IC_Setting[0] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { AF_IC_Setting[1] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x08, new byte[] { AF_IC_Setting[3] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x09, new byte[] { AF_IC_Setting[4] });
            //EPA Reset
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { 0x00 });

            //Linearity Reset
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
            for (int i = 0; i < 5; i++)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0C, new byte[] { AF_IC_Setting[2] });
                for (int j = 0; j < 2; j++)
                {
                    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x18 });
                    Wait(300);
                }
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x19, rbuf);
                byte tmpData = (byte)Math.Floor(rbuf[0] * 0.75);
                if (tmpData >= 0x00 && tmpData <= 0x30)
                {
                    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x19, new byte[] { tmpData });
                    AddLog(ch, "AF Calibration OK!");
                    break;
                }
                else
                {
                   // SetError(ch, NonSpecItem.AF_HallCalibration);
                    AddLog(ch, "AF Calibration (Reg 19) error[over 0x90]");
                   
                }
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xF3, new byte[] { 0x1E });
            Wait(25);
            bool WriteRes = DrvIC.AK7314_memory_update(ch, 1);
            WriteRes &= DrvIC.AK7314_memory_update(ch, 2);
            WriteRes &= DrvIC.AK7314_memory_update(ch, 3);
            WriteRes &= DrvIC.AK7314_memory_update(ch, 4);
            WriteRes &= DrvIC.AK7314_memory_update(ch, 5);

            if(!WriteRes)
            {
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });

                AddLog(ch, "AF Calibration Memory Update Fail");
                return;
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", 2048);
            DrvIC.AK7314_Mode(ch, 1);
            DrvIC.AK7314_IC_Data(ch);

            DrvIC.OISOn(ch, "X", false);
            DrvIC.OISOn(ch, "Y", false);
            Wait(100);

          //  AF EPA
            AddLog(ch, "<<<  AF EPA Start  >>>");
            short btm_position, tmp_position, top_position, ctr_position;
            short step, inf_cut, mac_cut;
            ushort posvt, negvt, target_code;
            short stroke;
            int loop = 0, mac_loop = 0;
            int new_con = 0, old_con = 0, cond = 0;
            int mac_loop_max = 50;
            ushort inf_tag_code, mac_tag_code;	// save code value

            DrvIC.AK7314_IC_Data(ch);
            DrvIC.Move(ch, "AF", 2048);
            Wait(50);
            res = Measure();
            ctr_position = (short)res.cz[0];
            DrvIC.Ak7314_soft_move(ch, 0, 10);
            res = Measure();
            short refPos = btm_position = (short)res.cz[0];
            tmp_position = 0;
            AddLog(ch, "Inf Cut Start");
            for (target_code = 0, step = 0x200; step > 0; step >>= 1)
            {
                AddLog(ch, $"tmp_pos:{tmp_position}, tar_code:{target_code}, step:{step}");
                if (tmp_position < BTM_POS - 1) target_code += (ushort)step;
                else if (tmp_position > BTM_POS + 1) target_code -= (ushort)step;
                else break;
                DrvIC.Move(ch, "AF", target_code);
                Wait(50);
                res = Measure();
                tmp_position = btm_position = (short)(res.cz[0] - refPos);
                loop++;
            }
            inf_tag_code = target_code;
            AddLog(ch, $"Inf_loop:{loop}");
            negvt = target_code; inf_cut = tmp_position;
            AddLog(ch, $"Inf_cut:{inf_cut}");
            if ((inf_cut < (BTM_POS - 1)) || (inf_cut > (BTM_POS + 1)))
            {
                AddLog(ch, $"EPA Error");
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });

                LEDs_All_On(0, false);
                return;
            }
            AddLog(ch, $"");

            DrvIC.Ak7314_soft_move(ch, 4095, 10);
            res = Measure();
            top_position = (short)res.cz[0];
            tmp_position = 0;
            stroke = (short)Math.Abs(refPos - top_position);

            if (stroke > TOP_POS + TOP_MARGIN)
            {
                mac_cut = (short)(stroke - (TOP_POS));
                step = 0x300;
                //step = 0xC0;
            }
            else
            {
                mac_cut = (short)TOP_MARGIN;
                step = 0x200;
                //step = 0x80;
            }
            AddLog(ch, "Mac Cut Start");
            AddLog(ch, $"Mac_Cut:{mac_cut}, Mac_Step:{step}");


            for (target_code = 4095; step > 0; step >>= 1)
            {
                string s = string.Empty;
                s += $"tmp_pos:{tmp_position}, tar_code:{target_code},";

                if (tmp_position < -1 - mac_cut)
                {
                    if (cond == 2)
                    {
                        step = (short)(step << 1);
                    }
                    target_code += (ushort)step;
                    cond = 2;
                    s += $"step:{step}, cond:{cond}";
                    AddLog(ch, s);
                }
                else if (tmp_position > 1 - mac_cut)
                {
                    if (cond == 3)
                    {
                        step = (short)(step << 1);
                    }
                    target_code -= (ushort)step;
                    cond = 3;
                    s += $"step:{step}, cond:{cond}";
                    AddLog(ch, s);
                }
                else break;
                DrvIC.Move(ch, "AF", target_code);
                Wait(50);
                res = Measure();
                tmp_position = (short)(res.cz[0] - top_position);
                mac_loop++;

                if (mac_loop > mac_loop_max) break;
            }
            mac_tag_code = target_code;

            if (mac_loop > mac_loop_max)
            {
                AddLog(ch, $"EPA Error");
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });

                LEDs_All_On(0, false);
                return;
            }
            AddLog(ch, $"tmp_pos:{tmp_position}, tar_code:{target_code}, mac_loop:{mac_loop}");
            posvt = target_code;
            AddLog(ch, "");
            AddLog(ch, "---------------------------------");
            AddLog(ch, $"Target stroke : {810}um");
            AddLog(ch, $"Target btm_top MG : {BTM_POS}_{TOP_MARGIN} um");
            AddLog(ch, $"Measured stroke : {stroke}um");
            AddLog(ch, $"Measured Mac_cut : {mac_cut}um");
            AddLog(ch, $"Inf cut-off size : {inf_cut}um");
            AddLog(ch, $"Mac cut-off size : {Math.Abs(tmp_position)}um");
            AddLog(ch, "---------------------------------");
            AddLog(ch, $"Inf/Mac target_code : {inf_tag_code}, {mac_tag_code}um");
            AddLog(ch, "---------------------------------");

            DrvIC.Move(ch, "AF", 2048);
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { AF_IC_Setting[1] });

            AFPOSVT = (byte)((4095 - posvt + 2) >> 4);      // for SU2810
            AFNEGVT = (byte)((negvt + 2) >> 4);

            AddLog(ch, $"posvt({posvt}) negvt({negvt}) POSVT({AFPOSVT}) NEGVT({AFNEGVT})");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { AFPOSVT });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { AFNEGVT });
            DrvIC.AK7314_memory_update(ch, 1);
            DrvIC.AK7314_memory_update(ch, 5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            DrvIC.AK7314_Mode(ch, 1);
            if (Option.SaveRawData)
            {
                StreamWriter sw = null;
                string dateDir = STATIC.CreateDateDir();
                if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                string path = dateDir + $"AF_EPA_CODE.csv";

                if (!File.Exists(path))
                {
                    sw = File.AppendText(path);
                    string s = $"SPL No, Date, Time, INF Code, MAC Code";
                    sw.WriteLine(s);
                    sw.Close();
                }
                sw = File.AppendText(path);
                string data = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                    $"{inf_tag_code},{mac_tag_code}";
                sw.WriteLine(data);
                sw.Close();
            }
            AddLog(ch, "<<<  AF EPA End  >>>");
          

            DrvIC.OISOn(ch, "X", false);
            DrvIC.OISOn(ch, "Y", false);
            Thread.Sleep(100);

            //AF LinComp
            AddLog(ch, "<<<  AF Lin. Comp Start  >>>");
            bool LinRes = AFLinComp(ch, 8, 4088, 34, 0, 0, 6, 6, 0, (int)stroke);
            AddLog(ch, "<<<  AF Lin. Comp End  >>>");
            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);
            DrvIC.OISOn(ch, "X", false);
            DrvIC.OISOn(ch, "Y", false);
            Wait(100);

            LEDs_All_On(0, false);
            if(!LinRes)
            {
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
                return;
            }
            PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = stroke;
            ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });

        }

        //private void Act_AFInit(int ch, string testItem)
        //{
        //    byte[] rbuf = new byte[1];
        //    FindResult res = new FindResult();
        //    double[] zVal = new double[2];

          
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    //AF OpenLoop Seq 추가
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
        //    rbuf[0] = (byte)(rbuf[0] & 0x7F);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
        //    Wait(50);
        //    AddLog(ch, $"AF Openloop Stroke Check");

        //    LEDs_All_On(0, true);
        //    for (int i = 0; i < 11; i++)
        //    {
        //        DrvIC.Move(ch, "AF", 4095);
        //        Wait(50);
        //        res = Measure();
        //        zVal[0] = res.cz[0];
        //        DrvIC.Move(ch, "AF", 0);
        //        Wait(50);
        //        res = Measure();
        //        zVal[1] = res.cz[0];
                
        //        AddLog(ch, $"{i + 1} : {Math.Abs(zVal[1] - zVal[0]).ToString("F3")}");

        //    }
        //    LEDs_All_On(0, false);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });

        //    AF_EPA_Reset(ch);
        //    AF_LinearityComp_Reset(ch);
        //    AddLog(ch, "PID parameter setting");
        //    for (int i = 0; i < AFPID.Count; i++)
        //    {
        //        Dln.WriteArray(ch, DrvIC.AFSlaveAddr, AFPID[i][0], new byte[] { AFPID[i][1] });
        //    }


        //    AddLog(ch, "Temp register setting");
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { 0x00 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x80 });
        //    Wait(10);
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x70, rbuf);
        //    AddLog(ch, $"Read 0x70 : 0x{rbuf[0].ToString("X")}");


        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, rbuf);

        //    AddLog(ch, "Calibration instruction");
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0C, new byte[] { 0x62 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x18 });
        //    Wait(150);
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x19, rbuf);
        //    AddLog(ch, $"Read 0x19 : 0x{rbuf[0].ToString("X")}");

        //    byte tmpData = (byte)(rbuf[0] * 0.75);
        //    AddLog(ch, $"CalcData : 0x{tmpData.ToString("X")}");

        //    if (tmpData >= 0x00 && tmpData <= 0x30)
        //    {
        //        Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x19, new byte[] { tmpData });
        //    }
        //    else
        //    {
        //        SetError(ch, NonSpecItem.AF_Init);
        //        return;
        //        //Error처리
        //    }
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xF3, new byte[] { 0x1E });
        //    Wait(10);
        //    Store(ch, 0);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    Dln.PowerSequence(0);
        //    AK7314_ICReset(0);
        //    CheckData(ch, 0);
        //}

        void Act_CloseLoopAging(int ch, string testitem, int InspCnt)
        {
            CloseLoopAging(ch);
        }
        //private void Act_AFEPA(int ch, string testItem)
        //{

        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
        //    if(DrvIC.Y2SlaveAddr != 0x00) Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 });

        //    LEDs_All_On(0, true);
        //    FindResult res = new FindResult();
        //    int findcount = 0;

        //    double Target = Condition.AFEPATarget;
        //    int InfCut = 10;
        //    int macCut = 6;
        //    byte[] rbuf2 = new byte[2];
        //    byte[] rbuf = new byte[1];
        //    byte backData = 0;
        //    double InitPos = 0; double EndPos = 0;

        //    //move 0 code Position
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x19, 0x00 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x05, 0x00 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x02, 0x80 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
        //    Wait(100);
        //    //측정하고 값 초기화         
        //    AddLog(ch, $"af pos(t, c) : {0},{DrvIC.ReadHall(ch, "AF")}");
        //    Wait(50);
        //    res = Measure();

        //    InitPos = res.cz[0];
        //    int dir = 1;

        //    int step = 512;
        //    int pos = step;
        //    InfCut = (int)(InitPos + 10);
        //    while (true)
        //    {
                
        //        if(findcount > 50)
        //        {
        //            AddLog(ch, "EPA Find NG");
        //            SetError(ch, NonSpecItem.AF_EPA);
        //            return;
        //        }
        //        DrvIC.Move(ch, "AF", pos);
        //        int a = DrvIC.ReadHall(ch, "AF");
        //        Wait(100);
        //        res = Measure();
              

        //        AddLog(ch, $"Pos:{(int)(res.cz[0] - InitPos)}, Code:{pos}, Step:{step}");

        //        if (res.cz[0] > InfCut + 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 0;
        //                step = step / 2;
        //                pos = pos - step;
        //            }
        //            else
        //            {
        //                dir = 0;
        //                pos = pos - step;
        //            }

        //        }
        //        else if (res.cz[0] < InfCut - 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 1;
        //                pos = pos + step;
        //            }
        //            else
        //            {
        //                dir = 1;
        //                step = step / 2;
        //                pos = pos + step;
        //            }

        //        }
        //        else { break; }
        //        findcount++;

        //    }

        //    int InfPos = pos;
        //    AddLog(ch, $"Inf Code : {InfPos}");

        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
        //    Wait(50);
        //    res = Measure();
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xE6, 0xF0 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFA, 0xF0 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFD, 0x70 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF8 });
        //    Wait(100);
        //    //측정하고 값 초기화, Measure Stroke 구해서 담음
        //    double measureStroke = 0;


        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x84, rbuf2); // check AF Current Hall
        //    AddLog(ch, $"af pos(t, c) : {4095},{DrvIC.ReadHall(ch, "AF")}");
        //    Wait(50);
        //    res = Measure();

        //    EndPos = res.cz[0];
        //    measureStroke = Math.Abs(EndPos - InitPos);
        //    AddLog(ch, $"Full Stroke = {measureStroke.ToString("F3")}");
        //    PassFails[ch].Results[(int)SpecItem.AF_NonEPAStroke].Val = measureStroke;
        //    ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke);
        //    if (measureStroke - Target - 10 > 6) macCut = (int)(measureStroke - Target - 10);
        //    AddLog(ch, $"Find macCut = {macCut}");

        //    dir = 0;
        //    step = 512;
        //    pos = 4095 - step;
        //    macCut = (int)(EndPos - macCut);
        //    findcount = 0;
        //    while (true)
        //    {
        //        if (findcount > 50)
        //        {
        //            AddLog(ch, "EPA Find NG");
        //            SetError(ch, NonSpecItem.AF_EPA);
        //            return;
        //        }
        //        DrvIC.Move(ch, "AF", pos);
        //        Wait(100);
        //        res = Measure();

        //        AddLog(ch, $"Pos:{(int)(res.cz[0] - EndPos)}, Code:{pos}, Step:{step}");
        //        //측정하고 값 기입
        //        if (res.cz[0] > macCut + 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 0;
        //                step = step / 2;
        //                pos = pos - step;
        //            }
        //            else
        //            {
        //                dir = 0;
        //                pos = pos - step;
        //            }

        //        }
        //        else if (res.cz[0] < macCut - 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 1;
        //                pos = pos + step;
        //            }
        //            else
        //            {
        //                dir = 1;
        //                step = step / 2;
        //                pos = pos + step;
        //            }

        //        }
        //        else { break; }
        //        findcount++;

        //    }
        //    int macPos = pos;
        //    AddLog(ch, $"Mac Code : {macPos}");
        //    //   Inf, Mac EPA 기입 계산

        //    byte POSVT = (byte)((4096 - macPos) / 16); byte NEGVT = (byte)(InfPos / 16);

        //    //   byte POSVT = (byte)((-Condition.AFPOSVT) / 16); byte NEGVT = (byte)(Condition.AFNEGVT / 16);

        //    //     AddLog(ch, $"POSVT = {Condition.AFPOSVT}, NEGVT = {Condition.AFNEGVT}");
        //    AddLog(ch, $"0x0E : 0x{POSVT.ToString("X")}, 0x0F : 0x{NEGVT.ToString("X")}");


        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Wait(5);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { POSVT });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { NEGVT });
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
        //    backData = rbuf[0];
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0x80) });//0x0B값 읽어서 백업해야하는지 확인

        //    DrvIC.Move(ch, "AF", AFCenter);

        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
        //    Wait(100);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
        //    Wait(200);
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
        //    if ((byte)(rbuf[0] & 0x04) != 0x00)
        //    {
        //        SetError(ch, NonSpecItem.AF_EPA);
        //        return;
        //    }
          
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    CheckData(ch, 0);

        //    if (Option.SaveRawData)
        //    {
        //        StreamWriter sw = null;
        //        string dateDir = STATIC.CreateDateDir();
        //        if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
        //        string path = dateDir + $"AF_EPA_CODE.csv";
              
        //        if (!File.Exists(path))
        //        {
        //            sw = File.AppendText(path);
        //            string s = $"SPL No, Date, Time, INF Code, MAC Code";
        //            sw.WriteLine(s);
        //            sw.Close();
        //        }
        //        sw = File.AppendText(path);
        //        string data = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
        //            $"{InfPos},{macPos}";
        //        sw.WriteLine(data);
        //        sw.Close();
        //    }

        //}
        //private void Act_OISEPA(int ch, string testItem)
        //{
        //    byte[] rbuf = new byte[1];
        //    byte backData = 0;

        //    int Xposvt = -Condition.XPOSVT, Xnegvt = Condition.XNEGVT, Yposvt = -Condition.YPOSVT, Ynegvt = Condition.YNEGVT;
        //    AddLog(ch, $"X POSVT = {Xposvt}, X NEGVT = {Xnegvt}");
        //    AddLog(ch, $"Y POSVT = {Yposvt}, Y NEGVT = {Ynegvt}");

        //    AddLog(ch, $"X = 0x0E : 0x{((Xposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Xnegvt / 4) & 0x03).ToString("X")}");
        //    AddLog(ch, $"Y = 0x0E : 0x{((Yposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Ynegvt / 4) & 0x03).ToString("X")}");

        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
        //    Wait(5);
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { (byte)((Xposvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { (byte)((Xnegvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { (byte)(((Xposvt / 4) & 0x03 << 2) | ((Xnegvt) & 0x03)) });

        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { (byte)((Yposvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { (byte)((Ynegvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { (byte)(((Yposvt / 4) & 0x03 << 2) | ((Ynegvt) & 0x03)) });


        //    Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0B, rbuf);
        //    backData = rbuf[0];
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
        //    Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0B, rbuf);
        //    backData = rbuf[0];
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
        //    Wait(120);

        //    Store(ch, 1);
        //    Store(ch, 2);
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
        //}

   
     
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
                Wait(100);
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
                Wait(100);
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            }



           
        }
        
        void CloseLoopAging(int ch)
        {
            int AFMin = Condition.CLAgingAFMin, AFMax = Condition.CLAgingAFMax, OISMin = Condition.CLAgingOISMin, OISMax = Condition.CLAgingOISMax, count = Condition.CLAgingCount;
            int delay = 1000 / Condition.CLAgingFreq / 2;
            int[] check_hall = new int[3];

            AddLog(ch, "<<<  XYZ Aging Start  >>>");
            AddLog(ch, $"Frequency : {Condition.CLAgingFreq}");
            AddLog(ch, $"Aging Count : {count}");
            AddLog(ch, $"AF Range : {AFMin} - {AFMax}");
            AddLog(ch, $"OIS Range : {OISMin} - {OISMax}");

            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            //DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);

            for (int i = 0; i < count; i++)
            {
                DrvIC.Move(ch, "AF", AFMin);
                DrvIC.Move(ch, "X", OISMin);
                DrvIC.Move(ch, "Y", OISMin);
                Wait(delay);
                DrvIC.Move(ch, "AF", AFMax);
                DrvIC.Move(ch, "X", OISMax);
                DrvIC.Move(ch, "Y", OISMax);
            }
      

            DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Wait(delay);
            //   Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            AddLog(ch, "<<<  XYZ Aging End  >>>");

            PassFails[0].Results[(int)SpecItem.XYZAging].Val = 1;
            ShowDataResults(ch, (int)SpecItem.XYZAging, (int)SpecItem.XYZAging, InspType.Normal, new double[] { });

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
        void Act_OISLinComp(int ch, string testItem, int InspCnt)
        {
            bool resX = false;
            bool resY = false;
            if (m_ChannelOn[ch]) resX = OISLinComp(ch, 0);
            if (m_ChannelOn[ch]) resY = OISLinComp(ch, 1);
            if(!resX || !resY)
            {
                PassFails[0].Results[(int)SpecItem.XYLinearComp].Val = 10;
                ShowDataResults(ch, (int)SpecItem.XYLinearComp, (int)SpecItem.XYLinearComp, InspType.Normal, new double[] { });
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.XYLinearComp].Val = 0;
                ShowDataResults(ch, (int)SpecItem.XYLinearComp, (int)SpecItem.XYLinearComp, InspType.Normal, new double[] { });
            }
        
        }
        bool OISLinComp(int ch, int axis)
        {
            AddLog(ch, "<<<  OIS Linearity comp. Start  >>>");


            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            string AxisName = axis == 0 ? "X" : "Y";
            float[] dbTargetPosi = new float[33];
            float[] dbLensPosi = new float[33];
            int[] dbHalldata = new int[33];
            float RefData = 0;
            byte[] ucResultCoef = new byte[13];
            int temp_table = Condition.OISLincompCodeMargin, step = 128;
            step = (4096 - 2 * Condition.OISLincompCodeMargin) / Condition.OISLincompStep;

            LEDs_All_On(0, true);

            OIS_LinearityComp_Reset(ch, axis);
            DrvIC.AK7314_Mode(ch, 1);
            DrvIC.Move(ch, "AF", BestAFPos); Thread.Sleep(50);
            AddLog(ch, $"Best AF for linear_comp : {BestAFPos}");

            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Wait(50);

            FindResult tmpres = new FindResult();

            AddLog(ch, $"Target\tDisplacement\tReadHall");
            AddLog(ch, "---------------------------------");

            for (int i = 0; i < Condition.OISLincompStep + 1; i++)
            {
                if (temp_table > 4095) temp_table = 4095;
                dbTargetPosi[i] = temp_table;

                if (axis == 0) { DrvIC.Move(ch, "X", (int)dbTargetPosi[i]); DrvIC.Move(ch, "Y", 2048); }
                else if (axis == 1) { DrvIC.Move(ch, "X", 2048); DrvIC.Move(ch, "Y", (int)dbTargetPosi[i]); }
                if (i == 0) Wait(100);
                else Wait(30);
                Wait(20);
                dbHalldata[i] = DrvIC.ReadHall(ch, AxisName);
                tmpres = Measure();
                if (axis == 0)
                {
                    if (i != 0) dbLensPosi[i] = (float)(tmpres.cx[0] - RefData);
                    else { dbLensPosi[i] = 0; RefData = (float)tmpres.cx[0]; }
                }
                else
                {
                    if (i != 0) dbLensPosi[i] = (float)(tmpres.cy[0] - RefData);
                    else { dbLensPosi[i] = 0; RefData = (float)tmpres.cy[0]; }
                }
                temp_table += step;
                AddLog(ch, $"{dbTargetPosi[i]}\t{dbLensPosi[i].ToString("F2")}\t{dbHalldata[i]}");
                if (i > 1 && dbHalldata[i] <= dbHalldata[i - 1])
                {
                    AddLog(ch, "OIS Linearity comp. error.");
                    
                    return false;

                }
            }
            AddLog(ch, "---------------------------------");


            byte pvt = 0, nvt = 0;
            byte[] rbuf = new byte[1];
            int ignInf = 0;             
            int ignMac = 0;             
            int numLinCompData;
            int[] linCoef = new int[OISLinCompCoef.NUM_COEF];  
            float resError = 0;

            if (axis == 0)
            {
                pvt = (byte)Condition.OISLincompXEPAPos;
                nvt = (byte)Condition.OISLincompXEPANeg;
            }
            else
            {
                pvt = (byte)Condition.OISLincompYEPAPos;
                nvt = (byte)Condition.OISLincompYEPANeg;
            }


            Dln.ReadArray(ch, addr, 0x0E, rbuf);
            pvt = rbuf[0];
            Dln.ReadArray(ch, addr, 0x0F, rbuf);
            nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");

            OISLinCompCoef coef = new OISLinCompCoef();
          
            int res = coef.LinCompMain(dbTargetPosi, dbLensPosi, dbTargetPosi.Length, pvt, nvt, ignInf, ignMac, ref linCoef, ref resError);
            if (res != 0)
            {
                AddLog(ch, $"Linearity Comp Fail");

                
                return false;
            }
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, addr, 0x2A, new byte[] { (byte)linCoef[0] });
            Dln.WriteArray(ch, addr, 0x2B, new byte[] { (byte)linCoef[1] });
            Dln.WriteArray(ch, addr, 0x2C, new byte[] { (byte)linCoef[2] });
            Dln.WriteArray(ch, addr, 0x2D, new byte[] { (byte)linCoef[3] });
            Dln.WriteArray(ch, addr, 0x2E, new byte[] { (byte)linCoef[4] });
            Dln.WriteArray(ch, addr, 0x2F, new byte[] { (byte)linCoef[5] });
            Dln.WriteArray(ch, addr, 0x30, new byte[] { (byte)linCoef[6] });
            Dln.WriteArray(ch, addr, 0x31, new byte[] { (byte)linCoef[7] });
            Dln.WriteArray(ch, addr, 0x32, new byte[] { (byte)linCoef[8] });
            Dln.WriteArray(ch, addr, 0x33, new byte[] { (byte)linCoef[9] });
            Dln.WriteArray(ch, addr, 0x34, new byte[] { (byte)linCoef[10] });
            Dln.WriteArray(ch, addr, 0x35, new byte[] { (byte)linCoef[11] });
            Dln.WriteArray(ch, addr, 0x36, new byte[] { (byte)linCoef[12] });

            bool result = DrvIC.AK7326_memory_update(ch, (byte)axis, 0);
            if(!result)
            {
                AddLog(ch, $"Linearity Comp Fail");

               
                return false;
            }
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x01 });
            Wait(200);
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x02 });
            Wait(250);
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x04 });
            Wait(200);
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x08 });
            Wait(200);
            string s = $"0x2A : 0x{linCoef[0].ToString("X")}, 0x2B : 0x{linCoef[1].ToString("X")}, 0x2C : 0x{linCoef[2].ToString("X")}, 0x2D : 0x{linCoef[3].ToString("X")}, 0x2E : 0x{linCoef[4].ToString("X")}\r\n" +
             $"0x2F : 0x{linCoef[5].ToString("X")}, 0x30 : 0x{linCoef[6].ToString("X")}, 0x31 : 0x{linCoef[7].ToString("X")}, 0x32 : 0x{linCoef[8].ToString("X")}, 0x33 : 0x{linCoef[9].ToString("X")}\r\n" +
             $"0x34 : 0x{linCoef[10].ToString("X")}, 0x35 : 0x{linCoef[11].ToString("X")}, 0x36 : 0x{linCoef[12].ToString("X")}";

            AddLog(ch, s);

           
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            LEDs_All_On(0, false);
            AddLog(ch, "<<<  OIS Linearity comp. End  >>>");
            return true;
        }
        bool AFLinComp(int ch, int startpos, int endpos, int step, int margin_start, int margin_end, int s_value, int e_value, int linear_spec, int init_stroke)
        {
            int NUM_COEF = 13;
            FindResult tmpres = new FindResult();
            float[] targPosi = new float[step + 1]; // Array for storing target position data
            float[] lensPosi = new float[step + 1]; // Array for storing lens position data
            int[] readHall = new int[step + 1];
            float[] refLensPosi = new float[step + 1];
            int valueStepsize = step - s_value - e_value;
            float[] valueLensPosi = new float[valueStepsize + 1];
            float refStepsize = 0, gap = 0, valueStep = 0, valuegap = 0;
            float max_gap = 0, max_valuegap = 0;

         
            int ignInf = 0;   
            int ignMac = 0;   
            int numLinCompData;

            float RefData = 0;
            byte[] rbuf = new byte[1];
            int temp_table = endpos;
            int step_size = (endpos - startpos) / step;

            int[] linCoef = new int[NUM_COEF]; // Array for storing line compensation coefficients
            int pVtNew;    // Recalculation "POSVT" after linearity compensation
            int nVtNew;    // Recalculation "NEGVT" after linearity compensation
            float resError = 0;   // Variable for storing residual error after linearity compensation


            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0E, rbuf);
            byte pvt = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0F, rbuf);
            byte nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");
            AddLog(ch, $"Step Size : {step_size}");

            DrvIC.AK7314_Mode(ch, 1);
            DrvIC.Move(ch, "AF", endpos);
            Thread.Sleep(200);
            DrvIC.OISOn(ch, "X", false);
            DrvIC.OISOn(ch, "Y", false);
            Wait(200);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            for (int i = 0; i < 13; i++)
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x30 + i, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

            
            AddLog(ch, $"Target\tReadHall\tPos");
            for (int i = step; i >= 0; i--)
            { // making input position table
                targPosi[i] = (float)temp_table;
                DrvIC.Move(ch, "AF", (int)targPosi[i]);
                Wait(150);
                readHall[i] = DrvIC.ReadHall(ch, "AF");
                tmpres = Measure();
                if (i != step) lensPosi[i] = (float)tmpres.cz[0] - RefData;
                else { lensPosi[i] = 0; RefData = (float)tmpres.cz[0]; }

             
                temp_table -= step_size; // From end to start
                AddLog(ch, $"{targPosi[i]}\t{readHall[i]}\t{lensPosi[i].ToString("F2")}");
            }
            valueStep = (lensPosi[step - e_value] - lensPosi[s_value]) / (valueStepsize);
            valueLensPosi[0] = lensPosi[s_value];
            valueLensPosi[valueStepsize] = lensPosi[s_value + valueStepsize];

            AddLog(ch, "");
            AddLog(ch, "=== Linearity check ===");
            AddLog(ch, $"ValueStepSize = {valueStepsize}");
            AddLog(ch, $"ValueStep = {valueStep}");
            AddLog(ch, "=======================");
            AddLog(ch, $"{lensPosi[s_value].ToString("F3")}, {valueLensPosi[0].ToString("F3")}");

            for (int i = 1; i < valueStepsize; i++)
            {
                valueLensPosi[i] = valueLensPosi[i - 1] + valueStep;
                valuegap = valueLensPosi[i] - lensPosi[i + s_value];
                if (valuegap >= 0) {}
                else { valuegap *= -1; }
                AddLog(ch, $"{lensPosi[i + s_value].ToString("F3")}, {valueLensPosi[i].ToString("F3")}, {valuegap.ToString("F3")}");
                if (max_valuegap < valuegap) max_valuegap = valuegap;

            }
            AddLog(ch, $"{lensPosi[valueStepsize + s_value].ToString("F3")}, {valueLensPosi[valueStepsize].ToString("F3")}");
            AddLog(ch, $"max valuegap= {max_valuegap.ToString("F3")}");

            if(max_valuegap > linear_spec)
            {
                if(targPosi.Length == lensPosi.Length)
                {
                    AFLinCompCoef coef = new AFLinCompCoef();
                    int[] lincoef = new int[AFLinCompCoef.NUM_COEF];
                    numLinCompData = lensPosi.Length;
                    AddLog(ch, $"numLinCompData = {numLinCompData}");
                    int res = coef.LinCompMain(targPosi, lensPosi, numLinCompData, pvt, nvt, ignInf, ignMac, ref lincoef, ref resError);
                    if(res != 0)
                    {
                        AddLog(ch, $"Linearity Comp Fail");
                      
                        return false;
                    }
                    string s = $"0x30 : 0x{lincoef[0].ToString("X")}, 0x31 : 0x{lincoef[1].ToString("X")}, 0x32 : 0x{lincoef[2].ToString("X")}, 0x33 : 0x{lincoef[3].ToString("X")}, 0x34 : 0x{lincoef[4].ToString("X")}\r\n" +
                     $"0x35 : 0x{lincoef[5].ToString("X")}, 0x36 : 0x{lincoef[6].ToString("X")}, 0x37 : 0x{lincoef[7].ToString("X")}, 0x38 : 0x{lincoef[8].ToString("X")}, 0x39 : 0x{lincoef[9].ToString("X")}\r\n" +
                     $"0x3A : 0x{lincoef[10].ToString("X")}, 0x3B : 0x{lincoef[11].ToString("X")}, 0x3C : 0x{lincoef[12].ToString("X")}";

                    AddLog(ch, s);
                    DrvIC.Move(ch, "AF", AFCenter);

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
                    DrvIC.AK7314_memory_update(ch, 1);
                    DrvIC.AK7314_memory_update(ch, 3);
                    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

                    int btm_position = 0, top_position = 0, measured_stroke = 0, spec_stroke = 0;
                    DrvIC.Ak7314_soft_move(ch, 0, 10);
                    tmpres = Measure();
                    btm_position = (int)tmpres.cz[0];
                    DrvIC.Ak7314_soft_move(ch, 4095, 10);
                    tmpres = Measure();
                    top_position = (int)tmpres.cz[0];
                    measured_stroke = Math.Abs(btm_position - top_position);
                    spec_stroke = init_stroke * 8 / 10;
                    AddLog(ch, $"stroke : {measured_stroke}");
                    if(measured_stroke < spec_stroke)
                    {
                        AddLog(ch, $"stroke NG  (spec : over cal stroke 80%)");                        
                      
                        return false;
                    }

                }
            }
            else
            {
                AddLog(ch, $"Linearity Comp Fail");           
                return false;
            }
            DrvIC.AK7314_IC_Data(ch);
            return true;
        }
        void Act_FindBestAFPosition(int ch, string testitem, int InspCnt, bool IsTwice)
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

        void Act_OISHallCalubration(int ch, string testItem, int InspCnt)
        {
            byte[] rbuf = new byte[1];
            AddLog(ch, "");
            AddLog(ch, "<<<  OIS Hall Calibration Start  >>>");
            DrvIC.AK7326_IC_Data(ch);
            DrvIC.AK7314_Mode(ch, 1);
            DrvIC.Move(ch, "AF", BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");

            AddLog(ch, $"Auto calibration");
            for (int i = 0; i < 2; i++)
            {
                int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                int index = i == 0 ? 1 : 2;
                Dln.WriteArray(ch, slaveAddr, 0x02, new byte[] { 0x40 });
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x3B });

                for (int j = 0; j < OIS_Set.Count; j++)
                    Dln.WriteArray(ch, slaveAddr, OIS_Set[j][0], new byte[] { OIS_Set[j][index] });

                for (int j = 0; j < OIS_reg.Count; j++)
                    Dln.WriteArray(ch, slaveAddr, OIS_reg[j][0], new byte[] { OIS_reg[j][index] });

                for (int j = 0; j < OISPID.Count; j++)
                    Dln.WriteArray(ch, slaveAddr, OISPID[j][0], new byte[] { OISPID[j][index] });
                DrvIC.AK7326_IC_Mode(ch, 0, 0);
                DrvIC.AK7326_IC_Mode(ch, 1, 0);
                Wait(50);
                for (int j = 0; j < 3; j++)
                {
                    Dln.WriteArray(ch, slaveAddr, 0x02, new byte[] { 0x09 });
                    Wait(220);
                }
                Dln.WriteArray(ch, slaveAddr, 0x19, new byte[] { 0x88 });
                Dln.WriteArray(ch, slaveAddr, 0x5D, new byte[] { 0x68 });
                byte[] calData = new byte[2];
                Dln.ReadArray(ch, slaveAddr, 0x04, rbuf);
                calData[0] = rbuf[0];
                Dln.ReadArray(ch, slaveAddr, 0x06, rbuf);
                calData[1] = rbuf[0];
                if (((calData[0] < 0x7F) && (calData[1] > 0x7F)) || ((calData[0] > 0x7F) && (calData[1] < 0x7F)))
                {
                    if (i == 0) AddLog(ch, $"OIS Cal X -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} OK");
                    else AddLog(ch, $"OIS Cal Y -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} OK");

                }
                else
                {
                    if (i == 0) AddLog(ch, $"OIS Cal X -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} NG");
                    else AddLog(ch, $"OIS Cal Y -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} NG");
                }

                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x01 }); Wait(170);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x02 }); Wait(270);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x04 }); Wait(170);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x08 }); Wait(120);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x10 }); Wait(70);
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x00 });
                DrvIC.AK7326_IC_Mode(ch, 0, 1);
                DrvIC.AK7326_IC_Mode(ch, 1, 1);
            }

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x03, rbuf);
            byte check_3f_x = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x03, rbuf);
            byte check_3f_y = rbuf[0];
            AddLog(ch, $"Need to check 0x3F : {check_3f_x.ToString("X2")}, {check_3f_y.ToString("X2")}");
            if (check_3f_x != 0x85 || check_3f_y != 0x85)
            {
                PassFails[0].Results[(int)SpecItem.XYHallCalibration].Val = 10;
                ShowDataResults(ch, (int)SpecItem.XYHallCalibration, (int)SpecItem.XYHallCalibration, InspType.OKNG, new double[] { });
                //AddLog(ch, "0x3F register, wrong parameter");
                //SetError(ch, NonSpecItem.OIS_HallCalibration);              
                return;
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.XYHallCalibration].Val = 0;
                ShowDataResults(ch, (int)SpecItem.XYHallCalibration, (int)SpecItem.XYHallCalibration, InspType.OKNG, new double[] { });
            }
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);
            AddLog(ch, "<<<  OIS Hall Calibration End  >>>");
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

        public void ServoDecenter(int ch, string name, int InspCnt)
        {
            AddLog(ch, "<<<  OIS X Servo Decenter Start  >>>");


            FindResult[] fX = new FindResult[2] { new FindResult(), new FindResult() };
            FindResult[] fY = new FindResult[2] { new FindResult(), new FindResult() };

            LEDs_All_On(0, true);
            
          
            DrvIC.Move(ch, "AF", Condition.ServoDecenterAFPos);
            Wait(300);
            AddLog(ch, $"AF Position : {DrvIC.ReadHall(ch, "AF")}");

            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Wait(200);
            fX[0] = Measure();

            STATIC.DrvIC.OISOn(0, "X", false);
            STATIC.DrvIC.OISOn(0, "Y", false);
            Wait(Condition.ServoDecenterDelay);

            fX[1] = Measure();
            PassFails[0].Results[(int)SpecItem.x_ServoDecenter].Val = fX[1].cx[0] - fX[0].cx[0];
            AddLog(ch, $"Decenter X = {(fX[1].cx[0] - fX[0].cx[0]).ToString("F2")}");
            AddLog(ch, "<<<  OIS X Servo Decenter End  >>>");
            AddLog(ch, "");
            AddLog(ch, "<<<  OIS Y Servo Decenter Start  >>>");

            DrvIC.Move(ch, "AF", Condition.ServoDecenterAFPos);
            Wait(100);
            AddLog(ch, $"AF Position : {DrvIC.ReadHall(ch, "AF")}");


            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Wait(200);
            fY[0] = Measure();

            STATIC.DrvIC.OISOn(0, "X", false);
            STATIC.DrvIC.OISOn(0, "Y", false);
            Wait(Condition.ServoDecenterDelay);
            fY[1] = Measure();

            PassFails[0].Results[(int)SpecItem.y_ServoDecenter].Val = fY[0].cy[0] - fY[1].cy[0];
            ShowDataResults(0, (int)SpecItem.x_ServoDecenter, (int)SpecItem.y_ServoDecenter, InspType.Normal, new double[] { });
            AddLog(ch, $"Decenter Y = {(fY[0].cy[0] - fY[1].cy[0]).ToString("F2")}");
            LEDs_All_On(0, false);
            AddLog(ch, "<<<  OIS Y Servo Decenter End  >>>");

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
        void AutoTest(int ch, string testItem, int InspCnt)
        {

            try
            {
                byte METM = 0, WSEC = 0;
                byte result = 0;
                int sinetest_X = 0, sinetest_Y = 0;
                int ringing_X = 0, ringing_Y = 0;
                int sinetest_X1 = 0, sinetest_Y1 = 0;
                bool result_x1, result_y1, result_x2, result_y2;

                byte sine_result = 0, ringing_result = 0;
                byte sine_result_2nd;
                byte[] rbuf = new byte[1];
                byte[] rbuf2 = new byte[2];

                AddLog(ch, "<<<  OIS Auto test Start  >>>");
                for (int i = 0; i < 3; i++)
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE });
                DrvIC.AK7326_PM_set_slave(ch, 2);
                AddLog(ch, "AK7326 Autotest get started.");
                AddLog(ch, "Sinewave Test Error Count Spec = 0");
                AddLog(ch, "Ringing Test Stabilize Time Spec = 100");
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
                DrvIC.Move(ch, "AF", 2048);
                Wait(100);
                AddLog(ch, $"AF Pos : {DrvIC.ReadHall(ch, "AF")}");
               

                AddLog(ch, "<<<  OIS Auto test End  >>>");
                for (int i = 0; i < 5; i++)
                {
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x53 });
                    Wait(2);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x60, new byte[] { (byte)Condition.AutoTest_THD });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x61, new byte[] { 0 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x62, new byte[] { 5 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x63, new byte[] { (byte)Condition.AutoTest_AMP });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x3E, new byte[] { (byte)Condition.AutoTest_AMP });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x64, new byte[] { 18 });

                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 });
                    Wait(700);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 });
                    Wait(1);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x00 });
                    Wait(2);

                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, rbuf);
                    sine_result = (byte)(0x0F & rbuf[0]);
                    if (sine_result == 0x00) { AddLog(ch, $"index : {i}"); break; }

                }
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9A, rbuf);
                sinetest_X1 = rbuf[0];
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9B, rbuf);
                sinetest_Y1 = rbuf[0];
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE4, rbuf2);
                SinewaveXMaxDiff = sinetest_X = ((rbuf2[0] << 8)+ rbuf2[1]) >> 4;
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE6, rbuf2);
                SinewaveYMaxDiff = sinetest_Y = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;

                if (sine_result != 0)
                {
                    AddLog(ch, $"Error flag : 0x{sine_result.ToString("X2")}");
                    AddLog(ch, $"Sinetest NG Error Count - x-diff : {sinetest_X1}, y-diff : {sinetest_Y1}");
                    AddLog(ch, $"Sinetest NG Max Diff - x-diff : {sinetest_X}, y-diff : {sinetest_Y}");
                }
                else
                {
                    AddLog(ch, $"Sinetest Error Count - x-diff : {sinetest_X1}, y-diff : {sinetest_Y1}");
                    AddLog(ch, $"Sinetest Max Diff - x-diff : {sinetest_X}, y-diff : {sinetest_Y}");
                    AddLog(ch, $"Sinewave is passed : 0x{sine_result.ToString("X2")}");
                }
                Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE });

                METM = 100;
                WSEC = 50;

                for (int i = 0; i < 5; i++)
                {
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x23 });
                    Wait(2);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x65, new byte[] { (byte)Condition.AutoTest_ErrTHD });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x66, new byte[] { (byte)Condition.AutoTest_InitPos });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x68, new byte[] { METM });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x69, new byte[] { WSEC });

                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 });
                    Wait(250);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 });
                    Wait(1);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x00 });
                    Wait(2);
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, rbuf);
                    ringing_result = (byte)(0x0F & rbuf[0]);
                    if (ringing_result == 0x00) { AddLog(ch, $"index : {i}"); break; }

                }

                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9C, rbuf);
                RingingXStabilizer = ringing_X = (METM + WSEC) - rbuf[0];
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9D, rbuf);
                RingingYStabilizer = ringing_Y = (METM + WSEC) - rbuf[0];

                if (ringing_result != 0)
                {
                    AddLog(ch, $"Error flag : 0x{ringing_result.ToString("X2")}");
                    AddLog(ch, $"Ringing NG Time - X : {ringing_X}, Y : {ringing_Y}");
                }
                else
                {

                    AddLog(ch, $"Ringing NG Time - X : {ringing_X}, Y : {ringing_Y}");
                    AddLog(ch, $"Ringing test is passed : 0x{ringing_result.ToString("X2")}");
                }
                Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE });

                PassFails[ch].Results[(int)SpecItem.AutoTestRes].Val = sine_result + ringing_result;
                ShowDataResults(ch, (int)SpecItem.AutoTestRes, (int)SpecItem.AutoTestRes, InspType.Normal, new double[] { });

                if (Option.SaveRawData)
                {
                    StreamWriter sw = null;
                    string dateDir = STATIC.CreateDateDir();
                    if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                    string path = dateDir + $"OIS_AutoTest.csv";

                    if (!File.Exists(path))
                    {
                        sw = File.AppendText(path);
                        string s = $"SPL No, Date, Time, SINE_NG_cnt_X, SINE_NG_cnt_Y, SINE_Diff_Max_X, SINE_Diff_Max_Y, RNG_NG_cnt_X, RNG_NG_cnt_Y,";
                        sw.WriteLine(s);
                        sw.Close();
                    }
                    sw = File.AppendText(path);
                    string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                        $"{sinetest_X1},{sinetest_Y1},{SinewaveXMaxDiff},{SinewaveYMaxDiff},{ringing_X},{ringing_Y}";
                    sw.WriteLine(dt);
                    sw.Close();
                }

            }
            catch
            {
                PassFails[ch].Results[(int)SpecItem.AutoTestRes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.AutoTestRes, (int)SpecItem.AutoTestRes, InspType.Normal, new double[] { });
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE })) return;

            }
        }
        void OISSensitivityTest(int ch, string testItem, int InspCnt)
        {

            int[] xCode = new int[] { 2048, 0, 4095, 0, 4095 };
            int[] yCode = new int[] { 2048, 0, 0, 4095, 4095 };
            byte[] rbuf = new byte[1];

            List<byte> xVal = new List<byte>();
            List<byte> yVal = new List<byte>();
            //List<int> xHall = new List<int>();
            //List<int> yHall = new List<int>();
            List<int> checkRegX = new List<int>();
            List<int> checkRegY = new List<int>();

            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            //Wait(100);
            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            for (int i = 0; i < xCode.Length; i++)
            {
                //DrvIC.Move(ch, "X", xCode[i]);
                //DrvIC.Move(ch, "Y", yCode[i]);
                //Wait(Condition.OISSensDelayTime);
                //Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x79, rbuf);
                xVal.Add(0);
                //Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x79, rbuf);
                yVal.Add(0);
                //xHall.Add(DrvIC.ReadHall(ch, "X"));              
                //yHall.Add(DrvIC.ReadHall(ch, "Y"));e
                checkRegX.Add(0);
                checkRegY.Add(0);
            }

            for (int i = 0; i < xVal.Count; i++)
            {
                AddLog(ch, $"{i * 2}, 0x{xVal[i].ToString("X2")}, 0x{yVal[i].ToString("X2")} ({xCode[i]}, {yCode[i]})");
                AddLog(ch, $"{i * 2 + 1}, 0x{checkRegX[i].ToString("X2")}, 0x{checkRegY[i].ToString("X2")} ({xCode[i]}, {yCode[i]})");
            }

            PassFails[ch].Results[(int)SpecItem.OISSensitivityTestRes].Val = 1;
            ShowDataResults(ch, (int)SpecItem.OISSensitivityTestRes, (int)SpecItem.OISSensitivityTestRes, InspType.Normal, new double[] { });

            if (Option.SaveRawData)
            {
                StreamWriter sw = null;
                string dateDir = STATIC.CreateDateDir();
                if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                string path = dateDir + $"OIS_SENS_MODE_CHECK.csv";
                
                if (!File.Exists(path))
                {
                    sw = File.AppendText(path);
                    string s = $"SPL No, Date, Time, 1_X_MID, 1_Y_MID, 1_XH_MID, 1_YH_MID, 2_X_MIN, 2_Y_MIN, 2_XH_MIN, 2_YH_MIN, " +
                        $"3_X_MAX, 3_Y_MIN, 3_XH_MAX, 3_YH_MIN, 4_X_MIN, 4_Y_MAX, 4_XH_MIN, 4_YH_MAX, 5_X_MAX, 5_Y_MAX, 5_XH_MAX, 5_YH_MAX,";
                    sw.WriteLine(s);
                    sw.Close();
                }
                sw = File.AppendText(path);
                //string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                //    $"{checkRegX[0]}, {checkRegY[0]}, {xHall[0]}, {yHall[0]}, {checkRegX[1]}, {checkRegY[1]}, {xHall[1]}, {yHall[1]}, {checkRegX[2]}, {checkRegY[2]}, {xHall[2]}, {yHall[2]}," +
                //    $"{checkRegX[3]}, {checkRegY[3]}, {xHall[3]}, {yHall[3]}, {checkRegX[4]}, {checkRegY[4]}, {xHall[4]}, {yHall[4]}";
                //sw.WriteLine(dt);
                sw.Close();
            }
        }
        private void Act_OISShift2(int port, string testItem, int InspCnt)
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
            {
                PassFails[ch].Results[(int)SpecItem.HallShiftVerify].Val = 1;
                ShowDataResults(ch, (int)SpecItem.HallShiftVerify, (int)SpecItem.HallShiftVerify, InspType.OKNG, new double[] { });
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.HallShiftVerify].Val = 0;
                ShowDataResults(ch, (int)SpecItem.HallShiftVerify, (int)SpecItem.HallShiftVerify, InspType.OKNG, new double[] { });
            }

            if (Option.SaveRawData)
            {
                StreamWriter sw = null;
                string dateDir = STATIC.CreateDateDir();
                if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                string path = dateDir + $"OIS_Shift.csv";

                if (!File.Exists(path))
                {
                    sw = File.AppendText(path);
                    string s = $"SPL No, Date, Time, X_STD, Y_STD, X Max Diff, Y Max Diff, X Max, Y Max, X Min, Y Min,";
                    sw.WriteLine(s);
                    sw.Close();
                }
                sw = File.AppendText(path);
                string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                    $"{RefX}, {RefY}, {xDiffMax}, {yDiffMax}, {XDiff.Max()}, {YDiff.Max()}, {XDiff.Min()}, {YDiff.Min()}";

                sw.WriteLine(dt);
                sw.Close();
            }


        }
        //private void Act_OISShift3(int ch, string testItem)
        //{
        //    int stdCode = Condition.DriftStdCode; int startCode = Condition.DriftStartCode; int endCode = Condition.DriftEndCode;
        //    int stepVal = Condition.DriftStepValue; int stepDelay = Condition.DriftStepDelay;

        //    AddLog(ch, $"<<<  OIS Shift Verify Start  >>>");
        //    AddLog(ch, $"AF X/Y Drift Test Start..");
        //    AddLog(ch, $"StartCode : {startCode}, EndCode : {endCode}");
        //    AddLog(ch, $"MoveStep : {stepVal}, MoveDelay : {stepDelay}");

        //    int HCAL_Check_MIN = startCode + (stepVal - 1);
        //    int HCAL_Check_MAX = endCode - (stepVal - 1);

        //    stdCode -= 2048;
        //    startCode -= 2048;
        //    endCode -= 2048;

        //    DrvIC.Move(ch, "AF", startCode);
        //    Wait(30);
        //    DrvIC.OIS_drift_test_mode_init(ch, true);
        //    Wait(50);
        //    if (stepDelay < 2) stepDelay = 2;
        //    stepDelay -= 2;
        //    int AFcodeSTD = stdCode + 2048;
        //    int i = stdCode;
        //    int softStep = 0;
        //    while(true)
        //    {
        //        softStep = (startCode - i) / 2;
        //        if ((-5 < softStep) && (softStep < 5))
        //            break;
        //        i += softStep;
        //        DrvIC.Move(ch, "AF", i);
        //        Wait(stepDelay);
        //    }
        //    DrvIC.Move(ch, "AF", startCode);





        //    if (xDiffMax > Condition.DriftTestSpec || yDiffMax > Condition.DriftTestSpec)
        //    { SetError(ch, NonSpecItem.DriftTestNG); return; }
        //    if (Option.SaveRawData)
        //    {
        //        StreamWriter sw = null;
        //        string dateDir = STATIC.CreateDateDir();
        //        if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
        //        string path = dateDir + $"OIS_Shift.csv";

        //        if (!File.Exists(path))
        //        {
        //            sw = File.AppendText(path);
        //            string s = $"SPL No, Date, Time, X_STD, Y_STD, X Max Diff, Y Max Diff, X Max, Y Max, X Min Y Min,";
        //            sw.WriteLine(s);
        //            sw.Close();
        //        }
        //        sw = File.AppendText(path);
        //        string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
        //            $"{RefX}, {RefY}, {xDiffMax}, {yDiffMax}, {XDiff.Max()}, {YDiff.Max()}, {XDiff.Min()}, {YDiff.Min()}";

        //        sw.WriteLine(dt);
        //        sw.Close();
        //    }


        //}
        void AFPID_Verify(int ch, string testItem, int InspCnt)
        {
            byte index, wdata, rdata;
            byte akm_ID;
            byte[] IC_SETTING_AKM7314_Addr = new byte[]{ 0x0A, 0x0B, 0x0C, 0x08, 0x09 };
            byte[] rbuf = new byte[1];
            bool res = true;

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            DrvIC.AK7314_memory_update(ch, 5);

            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x03, rbuf);
            int afid = rbuf[0];
            if (afid != 0x1E)
            {
                AddLog(ch, $"Error, AF IC is not AK7314, 0x{afid.ToString("X2")}");
                res = false;
                PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes, InspType.Normal, new double[] { });
                return;
            }
            for (int i = 0; i < IC_SETTING_AKM7314_Addr.Length; i++)
            {
                if (i == 2) continue;
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, IC_SETTING_AKM7314_Addr[i], rbuf);
                if (AF_IC_Setting[i] != rbuf[0])
                {
                    AddLog(ch, $"Addr. : 0x{IC_SETTING_AKM7314_Addr[i].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{AF_IC_Setting[i].ToString("X2")}");
                    AddLog(ch, "AF PID Verify Fail");
                    PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 2;
                    ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes, InspType.Normal, new double[] { });
                    return;
                }
            }
            for (int i = 0; i < AFPID.Count; i++)
            {
                if (AFPID[i][0] == 0x19) continue;
                if (AFPID[i][0] == 0xC8) continue;
                if (AFPID[i][0] == 0xC9) continue;
                if (AFPID[i][0] == 0xCB) continue;
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, AFPID[i][0], rbuf);
                if (AFPID[i][1] != rbuf[0])
                {
                    AddLog(ch, $"Addr. : 0x{AFPID[i][0].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{AFPID[i][1].ToString("X2")}");
                    AddLog(ch, "AF PID Verify Fail");
                    PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 3;
                    ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes, InspType.Normal, new double[] { });
                    return;

                }
            }
            PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 0;
            ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes, InspType.Normal, new double[] { });
        }
        void OIS_PIDVerify(int ch, string testItem, int InspCnt)
        {
            bool res = true;
            byte[] rbuf = new byte[1];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0xFE, rbuf); byte M_XVer = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0xFE, rbuf); byte M_YVer = rbuf[0];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0xFF, rbuf); byte C_Ver = rbuf[0];
            AddLog(ch, $"X PID Ver : {M_XVer}, Class : {C_Ver} (MX51)");
            AddLog(ch, $"Y PID Ver : {M_YVer}, Class : {C_Ver} (MX51)");

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x03, rbuf);
            if (rbuf[0] != 0x85)
            {
                AddLog(ch, $"Error, OIS IC is not AK7326, 0x{rbuf[0].ToString("X2")}");
                res = false;
                PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                string AxisName = i == 0 ? "X" : "Y";
                int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                AddLog(ch, $"OIS {AxisName}, Setting register verification");
                for (int index = 0; index < OIS_Set.Count; index++)
                {
                    if (OIS_Set[index][0] == 0x0B) continue;
                    if (OIS_Set[index][0] == 0x5D) continue;
                    if (OIS_Set[index][0] == 0x24) continue;
                    if (OIS_Set[index][0] == 0x25) continue;

                    byte wdata = OIS_Set[index][1 + i];
                    Dln.ReadArray(ch, slaveAddr, OIS_Set[index][0], rbuf);
                    if(wdata != rbuf[0])
                    {
                        AddLog(ch, $"{AxisName} Addr. : 0x{OIS_Set[index][0].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{wdata.ToString("X2")}");
                        AddLog(ch, "OIS PID Verify Fail");
                        PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 2;
                        ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });
                        return;
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                string AxisName = i == 0 ? "X" : "Y";
                int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                AddLog(ch, $"OIS {AxisName}, PID register verification");
                for (int index = 0; index < OISPID.Count; index++)
                {
                    if (OISPID[index][0] == 0x28) continue;
                    if (OISPID[index][0] == 0x2A) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x2B) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x2C) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x2D) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x2E) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x2F) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x30) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x31) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x32) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x33) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x34) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x35) continue;    // Lin. Comp.
                    if (OISPID[index][0] == 0x36) continue;    // Lin. Comp.

                    if (OISPID[index][0] == 0x50) continue;

                    byte wdata = OISPID[index][1 + i];
                    Dln.ReadArray(ch, slaveAddr, OISPID[index][0], rbuf);
                    if (wdata != rbuf[0])
                    {
                        AddLog(ch, $"{AxisName} Addr. : 0x{OISPID[index][0].ToString("X2")}, rdata : 0x{rbuf[0].ToString("X2")}, wdata : 0x{wdata.ToString("X2")}");
                        AddLog(ch, "OIS PID Verify Fail");
                        PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 3;
                        ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });
                        return;
                    }
                }
            }
            PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 0;
            ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });
        }
      
        void IME_Test(int ch, string testItem, int InspCnt)
        {
            try
            {
                bool xres = false;
                bool yres = false;
                AddLog(ch, $"<<<  IME Test Start  >>>");

                Dln.PowerSequence(0);
                DrvIC.AK7326_IC_reset(ch);
                DrvIC.AK7314_IC_reset(ch);
                Wait(50);
                DrvIC.AK7326_IC_reset(ch);
                Wait(50);
                DrvIC.AK7314_Mode(ch, 1);
                DrvIC.OISOn(ch, "X", true);
                DrvIC.OISOn(ch, "Y", true);
                Wait(50);

                int OISStroke = Condition.IMEOISStroke;

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
                    xres = false;

                }
                else
                {
                    xres = true;

                }
                if ((YIME < Condition.IMEMinThd) || (YIME > Condition.IMEMaxThd)) // -220 ~ 220
                {
                    AddLog(ch, "Y IME Test NG");
                    yres = false;

                }
                else
                {
                    yres = true;

                }

                g_IME[0] = XIME; g_IME[1] = YIME;
                change_autosensitivitymode(ch, XIME, YIME);

                if (xres && yres)
                {
                    PassFails[ch].Results[(int)SpecItem.OISIMERes].Val = 0;
                    ShowDataResults(ch, (int)SpecItem.OISIMERes, (int)SpecItem.OISIMERes, InspType.Normal, new double[] { });
                }
                else
                {
                    PassFails[ch].Results[(int)SpecItem.OISIMERes].Val = 1;
                    ShowDataResults(ch, (int)SpecItem.OISIMERes, (int)SpecItem.OISIMERes, InspType.Normal, new double[] { });
                }


                if (Option.SaveRawData)
                {
                    StreamWriter sw = null;
                    string dateDir = STATIC.CreateDateDir();
                    if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                    string path = dateDir + $"OIS_IC_Mount_Error.csv";

                    if (!File.Exists(path))
                    {
                        sw = File.AppendText(path);
                        string s = $"SPL No, Date, Time, OIS Stroke, X P Reg, X N Reg, Y P Reg, Y N Reg, X PCAL, X NCAL, Y PCAL, Y NCAL, X IME, Y IME";
                        sw.WriteLine(s);
                        sw.Close();
                    }
                    sw = File.AppendText(path);
                    string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                        $"{OISStroke},{X_PNCAL[0]},{X_PNCAL[1]},{Y_PNCAL[0]},{Y_PNCAL[1]},{XPCAL},{XNCAL},{YPCAL},{YNCAL},{XIME},{YIME}";
                    sw.WriteLine(dt);
                    sw.Close();
                }
                AddLog(ch, $"<<<  IME Test End  >>>");
            }
            catch
            {
                PassFails[ch].Results[(int)SpecItem.OISIMERes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.OISIMERes, (int)SpecItem.OISIMERes, InspType.Normal, new double[] { });
            }
            

        }
        void change_autosensitivitymode(int ch, int x_ime, int y_ime)
        {
            int imeTH = 130;
            byte[] xbuf = new byte[2];
            byte[] ybuf = new byte[2];
            byte[] rbuf = new byte[1];

            AddLog(ch, $"x_ime : {x_ime}, y_ime : {y_ime}, abs(x_ime) : {Math.Abs(x_ime)}, abs(y_ime) : {Math.Abs(y_ime)}");

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x24, rbuf); xbuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x25, rbuf); xbuf[1] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x24, rbuf); ybuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x25, rbuf); ybuf[1] = rbuf[0];

            AddLog(ch, $"[Before] Auto Sensitivity mode");
            AddLog(ch, $"0x{xbuf[0].ToString("X2")}, 0x{xbuf[1].ToString("X2")}");
            AddLog(ch, $"0x{ybuf[0].ToString("X2")}, 0x{ybuf[1].ToString("X2")}");

            if ((Math.Abs(x_ime) >= imeTH) || (Math.Abs(y_ime) >= imeTH))
            {
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                Wait(10);
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x24, new byte[] { 0x5A });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x25, new byte[] { 0x22 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x03, new byte[] { 0x02 });
                Wait(300);
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x24, new byte[] { 0x50 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x25, new byte[] { 0x1E });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x03, new byte[] { 0x02 });
                Wait(300);
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            }

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x24, rbuf); xbuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x25, rbuf); xbuf[1] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x24, rbuf); ybuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x25, rbuf); ybuf[1] = rbuf[0];

            AddLog(ch, $"[After] Auto Sensitivity mode");
            AddLog(ch, $"0x{xbuf[0].ToString("X2")}, 0x{xbuf[1].ToString("X2")}");
            AddLog(ch, $"0x{ybuf[0].ToString("X2")}, 0x{ybuf[1].ToString("X2")}");

        }

        bool AK7314_ICReset(int ch)
        {
            byte[] rbuf = new byte[1];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
            Wait(100);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) != 0x00)
            {
           
                AddLog(ch, "Store fail");
                return false;
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Wait(50);
            return true;
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
            DrvIC.AK7326_IC_reset(ch);
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

            DrvIC.AK7326_PM_set_slave(ch, axis);
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

        void throughFRA(int ch, string testItem, int InspCnt)
        {
            double gain = 0;
            gain = throughFRA_gain(ch, 0);
            PassFails[ch].Results[(int)SpecItem.ThroughPeak_X_Gain].Val = gain;
            ShowDataResults(ch, (int)SpecItem.ThroughPeak_X_Gain, (int)SpecItem.ThroughPeak_X_Gain, InspType.OnlyMax, new double[] { });
            gain = throughFRA_gain(ch, 1);
            PassFails[ch].Results[(int)SpecItem.ThroughPeak_Y_Gain].Val = gain;
            ShowDataResults(ch, (int)SpecItem.ThroughPeak_Y_Gain, (int)SpecItem.ThroughPeak_Y_Gain, InspType.OnlyMax, new double[] { });
        }

        void OISPhasemargin(int ch, string testItem, int InspCnt)
        {
            double freq = 0, pm = 0;
            (freq, pm) = OISPM(ch, 0);
           /* PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq].Val = freq;*/ PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = pm;
            ShowDataResults(ch, (int)SpecItem.FRAX_PhaseMargin, (int)SpecItem.FRAX_PhaseMargin, InspType.Normal, new double[] { });
            (freq, pm) = OISPM(ch, 1);
            /*PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq].Val = freq;*/ PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = pm;
            ShowDataResults(ch, (int)SpecItem.FRAY1_PhaseMargin, (int)SpecItem.FRAY1_PhaseMargin, InspType.Normal, new double[] { });
        }
        void OISLoopGain(int ch, string testItem, int InspCnt)
        {
            double gain = 0;
            gain = LoopGain(ch, 0);
            PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val = gain;
            ShowDataResults(ch, (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz, InspType.Normal, new double[] { });

            gain = LoopGain(ch, 1);
            PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val = gain;
            ShowDataResults(ch, (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz, InspType.Normal, new double[] { });

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

            //int startFreq = axis == 0 ? Condition.iXChirpFrom : Condition.iYChirpFrom;
            //int finalFreq = axis == 0 ? Condition.iYChirpTo : Condition.iYChirpTo;
            //int minphase = axis == 0 ? Condition.PMXMinPhase : Condition.PMYMinPhase;
            //int gainTH = axis == 0 ? Condition.PMXGainTH : Condition.PMYGainTH;
            int amp = axis == 0 ? (int)Condition.iLoppgainXAmp : (int)Condition.iLoppgainYAmp;

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

        void AFGainMargin(int ch, string testItem, int InspCnt)
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
            ShowDataResults(ch, (int)SpecItem.FRAAF_GainMargin, (int)SpecItem.FRAAF_GainMargin, InspType.Normal, new double[] { });
        }

        void AFPhaseMargin(int ch, string testItem, int InspCnt)
        {
            double resFreq = 0, respm = 0, res4dbpm = 0;
            int freqval, freqtemp = 0, gaintemp, freqpm = 0, oldfreq;
            int[] before_after_zero_freq = new int[2];
            double gainval = 0, pmval, phaestemp, prepm = 0, PM4dB;
            double[] before_after_zero_gain = new double[2];
            byte backup, flag_2nd = 0;
            byte fra_en;
            bool dB4PhaseFouund = false;
            bool PhaseFouund = false;

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


                if (!PhaseFouund && gainval > 0)
                {
                    respm = ((gainval * prepm) - (before_after_zero_gain[0] * pmval)) / (gainval - before_after_zero_gain[0]);
                    resFreq = (int)(((gainval * before_after_zero_freq[0]) - (before_after_zero_gain[0] * freqpm)) / (gainval - before_after_zero_gain[0]));
                    before_after_zero_freq[1] = freqval;
                    before_after_zero_gain[1] = gainval;
                    PhaseFouund = true;
                    if (dB4PhaseFouund)
                        break;
                }
                if (!dB4PhaseFouund && gainval >= -4 && before_after_zero_gain[0] <= -4)
                {
                    //pm1 + (targetGain - gain1) * (pm2 - pm1) / (gain2 - gain1);
                    res4dbpm = prepm + ((-4) - before_after_zero_gain[0]) * (pmval - prepm) / (gainval - before_after_zero_gain[0]);
                    //  res4dbpm = ((gainval * prepm) - (before_after_zero_gain[0] * pmval)) / (gainval - before_after_zero_gain[0]);
                    dB4PhaseFouund = true;
                    if (PhaseFouund) break;
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
            AddLog(ch, $"-4dB Phase Margin = {res4dbpm.ToString("F0")}");

            DrvIC.FRAModeDisable(ch);
            AK7314_ICReset(ch);
         //   PassFails[ch].Results[(int)SpecItem.FRAAF_PMFreq].Val = resFreq;
            PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = respm;
            PassFails[ch].Results[(int)SpecItem.FRAAF_4dB_PhaseMargin].Val = res4dbpm;

            ShowDataResults(ch, (int)SpecItem.FRAAF_PhaseMargin, (int)SpecItem.FRAAF_4dB_PhaseMargin, InspType.Normal, new double[] { });
        }
        void WriteUserMem(int ch, int res)
        {
            try
            {
                var now = STATIC.LogDate;
                var year = now.Year - 2000;
                var month = now.Month;
                var day = now.Day;
                var hour = now.Hour;
                var minute = now.Minute;
                var second = now.Second;
                // X Mem
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                byte[] xWriteData = new byte[32];

                //if (PassFails[ch].Results[(int)SpecItem.OISX_Ratedstroke].Val >= 760 && PassFails[ch].Results[(int)SpecItem.OISX_Ratedstroke].Val <= 770)
                //    xWriteData[1] = (770 / 10);
                //else xWriteData[1] = (byte)(PassFails[ch].Results[(int)SpecItem.OISX_Ratedstroke].Val / 10);
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
                xWriteData[30] = (byte)OISPIDVer;
                xWriteData[31] = 0x33;

                for (int i = 0; i < xWriteData.Length; i++)
                {
                    int addr = 0xE0 + i;
                    if (addr == 0xF8 || addr == 0xE2 || addr == 0xE3 || addr == 0xE0) continue;
                    DrvIC.AK7326_EEPROM_Writecheck(ch, 0, (byte)addr, xWriteData[i]);
                }

                //Y Mem
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                byte[] yWriteData = new byte[32];
                //  yWriteData[0] = (byte)(PassFails[ch].Results[(int)SpecItem.OISY_Rolling].Val * 10);

                //if (PassFails[ch].Results[(int)SpecItem.OISY_Ratedstroke].Val >= 760 && PassFails[ch].Results[(int)SpecItem.OISY_Ratedstroke].Val <= 770)
                //    yWriteData[1] = 770 / 10;
                //else yWriteData[1] = (byte)(PassFails[ch].Results[(int)SpecItem.OISY_Ratedstroke].Val / 10);
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
                yWriteData[30] = (byte)OISPIDVer;


                for (int i = 0; i < yWriteData.Length; i++)
                {
                    int addr = 0xE0 + i;
                    if (addr == 0xF8 || addr == 0xFF || addr == 0xE2 || addr == 0xE3 || addr == 0xE0) continue;
                    DrvIC.AK7326_EEPROM_Writecheck(ch, 1, (byte)addr, yWriteData[i]);
                }

                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x0 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });

                byte[] temp_pm = new byte[3];
                byte[] decrypt_oispm = new byte[2];
                byte decrypt_afpm = 0x00;
                byte[] rbuf = new byte[1];
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0xF9, rbuf);
                temp_pm[0] = rbuf[0];
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0xF9, rbuf);
                temp_pm[1] = rbuf[0];
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0xFD, rbuf);
                temp_pm[2] = rbuf[0];

                decrypt_oispm[0] = (byte)(temp_pm[0] ^ 0x54 ^ 0xF9);
                decrypt_oispm[1] = (byte)(temp_pm[1] ^ 0x54 ^ 0xF9);
                decrypt_afpm = (byte)(temp_pm[2] ^ 0x54 ^ 0xFD);
                byte[] OISPM = new byte[2] { (byte)(PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val),
                 (byte)(PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val) };
                byte afPM = (byte)(PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val);


                AddLog(ch, $"[ORI]{OISPM[0]}, {OISPM[1]}, {afPM}, [DEC]{decrypt_oispm[0]}, {decrypt_oispm[1]}, {decrypt_afpm}");
                for (int i = 0; i < 2; i++)
                {
                    if (OISPM[i] != decrypt_oispm[i])
                    {
                        AddLog(ch, $"[OIS Encryption Error] axis:{i}, result_oispm:{OISPM[i]}, decrypt_oispm:{decrypt_oispm[i]}");
                        res = 0x09;
                    }
                }
                if (afPM != decrypt_afpm)
                {
                    AddLog(ch, $"[AF Encryption Error]result_afpm:{afPM}, decrypt_afpm:{decrypt_afpm}");
                    res = 0x09;
                }

                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0xE4, rbuf);
                byte xShift = rbuf[0];
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0xE4, rbuf);
                byte yShift = rbuf[0];
                if (xShift != 1 && yShift != 1)
                {
                    AddLog(ch, $" Test result : Fail! X_Shift_Flag : {xShift} , Y_Shift_Flag : {yShift}");
                    res = 0x09;
                }
                //AF Mem
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });


                byte[] AFWriteAddr = new byte[] { 0xF0, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB };
                byte[] AFWriteData = new byte[AFWriteAddr.Length];

                // 기존 데이터 입력
                AFWriteData[0] = (byte)res;
                //if (PassFails[ch].Results[(int)SpecItem.AF_Ratedstroke].Val >= 760 && PassFails[ch].Results[(int)SpecItem.AF_Ratedstroke].Val <= 770)
                //    AFWriteData[1] = 770 / 4;
                //else AFWriteData[1] = (byte)(PassFails[ch].Results[(int)SpecItem.AF_Ratedstroke].Val / 4);
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
                AFWriteData[10] = (byte)AFPIDVer;

                for (int i = 0; i < AFWriteAddr.Length; i++)
                {
                    DrvIC.AK7314_EEPROM_Writecheck(ch, AFWriteAddr[i], AFWriteData[i]);

                }
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

                Dln.PowerSequence(ch);
                AK7314_ICReset(ch);
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

                for (int i = 0; i < 7; i++)
                {
                    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0xF4 + i, rbuf);
                    STATIC.ActID += $"{rbuf[0].ToString("X2")}";
                }

                byte[] xCheckData = new byte[32];
                byte[] yCheckData = new byte[32];
                byte[] afCheckData = new byte[AFWriteAddr.Length];



                AddLog(ch, "X Nvm Data Check");

                for (int i = 0; i < xCheckData.Length; i++)
                {
                    int addr = 0xE0 + i;
                    if (addr == 0xF8 || addr == 0xE2 || addr == 0xE3 || addr == 0xE0) continue;
                    Dln.ReadArray(ch, DrvIC.XSlaveAddr, addr, rbuf);
                    AddLog(ch, $"Addr : 0x{addr.ToString("X2")}, WData : 0x{xWriteData[i].ToString("X2")}, RData : 0x{rbuf[0].ToString("X2")}");
                    if (xWriteData[i] != rbuf[0])
                    {
                        if (PassFails[ch].FirstFailIndex == 0)
                        {
                            AddLog(ch, "NVM Verify NG");
                            PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 1;
                            ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });

                        }


                    }
                }

                AddLog(ch, "Y Nvm Data Check");
                for (int i = 0; i < yCheckData.Length; i++)
                {
                    int addr = 0xE0 + i;
                    if (addr == 0xF8 || addr == 0xFF || addr == 0xE2 || addr == 0xE3 || addr == 0xE0) continue;
                    Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, addr, rbuf);
                    AddLog(ch, $"Addr : 0x{addr.ToString("X2")}, WData : 0x{yWriteData[i].ToString("X2")}, RData : 0x{rbuf[0].ToString("X2")}");
                    if (yWriteData[i] != rbuf[0])
                    {
                        if (PassFails[ch].FirstFailIndex == 0)
                        {
                            AddLog(ch, "NVM Verify NG");
                            PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 1;
                            ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });

                        }

                    }
                }

                AddLog(ch, "AF Nvm Data Check");
                for (int i = 0; i < afCheckData.Length; i++)
                {
                    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, AFWriteAddr[i], rbuf);
                    AddLog(ch, $"Addr : 0x{AFWriteAddr[i].ToString("X2")}, WData : 0x{AFWriteData[i].ToString("X2")}, RData : 0x{rbuf[0].ToString("X2")}");
                    if (AFWriteData[i] != rbuf[0])
                    {
                        if (PassFails[ch].FirstFailIndex == 0)
                        {
                            AddLog(ch, "NVM Verify NG");
                            PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 1;
                            ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes, InspType.Normal, new double[] { });

                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Form f = Application.OpenForms["F_Main"];
                if (f != null)
                {
                    if (f.InvokeRequired)
                    {
                        f.BeginInvoke(new Action(() =>
                            MessageBox.Show(f, ex.ToString(), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    }
                    else
                    {
                        MessageBox.Show(f, ex.ToString(), "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // 메인폼을 못 찾았을 때 (owner 없이 표시)
                    MessageBox.Show(ex.ToString(), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }



                if (m_ChannelOn[ch] && PassFails[0].FirstFailIndex == 0)
                {
                    m_ChannelOn[ch] = false;
                    PassFails[0].FirstFailIndex = -999;
                    PassFails[0].FirstFail = "Check UserMem Setting";
                }
           
            }


        }

        public static void Wait(int ms)
        {
            //       Thread.Sleep(ms);
            ms = ms * 1000;
            Stopwatch startNew = Stopwatch.StartNew();

            long usDelayTick = (ms * Stopwatch.Frequency) / 1000000;

            while (startNew.ElapsedTicks < usDelayTick) ;



            //if (ms <= 0)
            //    return;

            //var sw = Stopwatch.StartNew();

            //// 목표 tick (ms → tick)
            //double targetTicks = ms * (double)Stopwatch.Frequency / 1000.0;

            //while (true)
            //{
            //    double elapsedTicks = sw.ElapsedTicks;
            //    double remainingTicks = targetTicks - elapsedTicks;

            //    if (remainingTicks <= 0)
            //        break;

            //    // 남은 tick → ms로 환산
            //    double remainingMs = remainingTicks * 1000.0 / Stopwatch.Frequency;

            //    if (remainingMs > 5.0)
            //    {
            //        // 아직 여유가 많으면 1ms씩 Sleep하면서 CPU 양보
            //        Thread.Sleep(1);
            //    }
            //    else if (remainingMs > 1.0)
            //    {
            //        // 1~5ms 남은 구간: 가벼운 SpinWait로 세밀히 접근
            //        Thread.SpinWait(500); // 값은 환경에 맞게 조절 가능
            //    }
            //    else
            //    {
            //        // 1ms 이하 남은 구간: 매우 짧게 busy-wait로 마무리
            //        // (Stopwatch 해상도에 가까운 정밀도)
            //        // 여기서는 불필요한 연산 없이 루프만 돎
            //        while (sw.ElapsedTicks < targetTicks)
            //        {
            //            // tight spin
            //        }
            //        break;
            //    }
            //}
        }

        #endregion
    }
}
