using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ4P
{
    public partial class Process
    {

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
         
            ItemList.Add(new ActItems() { Name = "Gain@10Hz", Func = Act_GaindB10Hz, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Gain@10Hz", Func = Act_GaindB10Hz, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Phase Margin", Func = Act_Phase_Margin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Phase Margin High", Func = Act_Phase_Margin_High, IsMulti = true });
            //      ItemList.Add(new ActItems() { Name = "Gain Margin", Func = Act_Gain_Margin, IsMulti = true });      
            ItemList.Add(new ActItems() { Name = "AF ScanAging", Func = Act_AFScanAging });
            ItemList.Add(new ActItems() { Name = "AF PreDriving", Func = Act_PreAFDriving });
            ItemList.Add(new ActItems() { Name = "OIS Shift", Func = Act_OISShift, IsMulti = true });
         
        }

        #region AddSeq
        private void Act_AFOpenLoopAging(int ch, string testItem)
        {
            AFOpenLoopAging(0);
        }
        void Act_AFScanAging(int ch, string testItem)
        {
            AddLog(ch, "<<<  AF Scan aging Start  >>>");
            AddLog(ch, $"Start aging {Condition.AFSCanAgingCount} cycle for AF Driving");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", AFCenter);
            Thread.Sleep(100);

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
                    Thread.Sleep(Condition.AFScanAgingDelay);
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
                    Thread.Sleep(Condition.AFPreDrvDelay);
                    if (j == 4)
                    {
                        STATIC.fVision.m__G.oCam[0].Grab(0);
                        res = STATIC.fVision.MeasureTxTyTz(0);
                        MtoM[0] = res.cz[0];
                    }
                    if (j == 8)
                    {
                        STATIC.fVision.m__G.oCam[0].Grab(0);
                        res = STATIC.fVision.MeasureTxTyTz(0);
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


            AddLog(ch, "Setting register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { 0xE2 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0A, new byte[] { 0x73 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x08, new byte[] { 0x85 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x09, new byte[] { 0x8C });

            AF_EPA_Reset(ch);
            AF_LinearityComp_Reset(ch);

            //PID Update - 나중에 파일로 처리
            AddLog(ch, "PID parameter setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x10, new byte[] { 0x2C });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x11, new byte[] { 0x47 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x12, new byte[] { 0x96 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x13, new byte[] { 0x24 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x14, new byte[] { 0x18 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x15, new byte[] { 0x26 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x16, new byte[] { 0x20 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x17, new byte[] { 0x4B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x18, new byte[] { 0x14 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1B, new byte[] { 0x6E });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1C, new byte[] { 0xDC });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1D, new byte[] { 0xCD });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1E, new byte[] { 0xCD });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1F, new byte[] { 0x1F });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x20, new byte[] { 0x11 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x21, new byte[] { 0x10 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x22, new byte[] { 0x0A });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x23, new byte[] { 0x32 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x24, new byte[] { 0xC4 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x25, new byte[] { 0xF5 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x26, new byte[] { 0xCD });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x27, new byte[] { 0xC3 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x28, new byte[] { 0x71 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x29, new byte[] { 0xDF });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2A, new byte[] { 0x34 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2B, new byte[] { 0x88 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2C, new byte[] { 0x8E });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2D, new byte[] { 0x21 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2E, new byte[] { 0x3D });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2F, new byte[] { 0xB5 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC0, new byte[] { 0x10 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC1, new byte[] { 0x6E });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC2, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC3, new byte[] { 0xBA });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC4, new byte[] { 0xD0 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC5, new byte[] { 0x46 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC6, new byte[] { 0xD7 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC7, new byte[] { 0x50 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC8, new byte[] { 0x09 });

            AddLog(ch, "Function register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCA, new byte[] { 0x46 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCB, new byte[] { 0xD8 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCC, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCD, new byte[] { 0x32 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3D, new byte[] { 0x06 });

            AddLog(ch, "Temp register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x80 });
            Thread.Sleep(50);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x70, rbuf);
            AddLog(ch, $"Read 0x70 : 0x{rbuf[0].ToString("X")}");


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, rbuf);

            AddLog(ch, "Calibration instruction");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x18 });
            Thread.Sleep(150);
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
            Thread.Sleep(30);
            Store(ch, 0);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.PowerOnOff(0, false);
            Thread.Sleep(200);
            Dln.PowerOnOff(0, true);
            Thread.Sleep(100);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            CheckData(ch, 0);
        }

        void Act_CloseLoopAging(int ch, string testitem)
        {
            CloseLoopAging(0, Condition.CLAgingMode);
        }
        private void Act_AFEPA(int ch, string testItem)
        {


            LEDs_All_On(0, true);
            FindResult res = new FindResult();


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
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x19, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x05, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x02, 0x80 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
            Thread.Sleep(100);
            //측정하고 값 초기화         
            for (int i = 0; i < 5; i++)
            {
                AddLog(ch, $"af pos(t, c) : {0},{DrvIC.ReadHall(ch, "AF")}");
                Thread.Sleep(50);
            }

            STATIC.fVision.m__G.oCam[0].Grab(0);
            res = STATIC.fVision.MeasureTxTyTz(0);

            InitPos = res.cz[0];
            int dir = 1;

            int step = 512;
            int pos = step;
            InfCut = (int)(InitPos + 10);
            while (true)
            {
                DrvIC.Move(ch, "AF", pos);
                Thread.Sleep(100);
                STATIC.fVision.m__G.oCam[0].Grab(0);
                res = STATIC.fVision.MeasureTxTyTz(0);

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

            }

            int InfPos = pos;
            AddLog(ch, $"Inf Code : {InfPos}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xE6, 0xF0 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFA, 0xF0 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFD, 0x70 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF8 });
            Thread.Sleep(100);
            //측정하고 값 초기화, Measure Stroke 구해서 담음
            double measureStroke = 0;


            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x84, rbuf2); // check AF Current Hall
            for (int i = 0; i < 5; i++)
            {
                AddLog(ch, $"af pos(t, c) : {4095},{DrvIC.ReadHall(ch, "AF")}");
                Thread.Sleep(50);
            }
            STATIC.fVision.m__G.oCam[0].Grab(0);
            res = STATIC.fVision.MeasureTxTyTz(0);

            EndPos = res.cz[0];
            measureStroke = Math.Abs(EndPos - InitPos);
            AddLog(ch, $"Full Stroke = {measureStroke.ToString("F3")}");
            if (measureStroke - Target - 10 > 6) macCut = (int)(measureStroke - Target - 10);
            AddLog(ch, $"Find macCut = {macCut}");

            dir = 0;
            step = 512;
            pos = 4095 - step;
            macCut = (int)(EndPos - macCut);
            while (true)
            {

                DrvIC.Move(ch, "AF", pos);
                Thread.Sleep(100);
                STATIC.fVision.m__G.oCam[0].Grab(0);
                res = STATIC.fVision.MeasureTxTyTz(0);

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

            }
            int macPos = pos;
            AddLog(ch, $"Mac Code : {macPos}");
            //   Inf, Mac EPA 기입 계산

            byte POSVT = (byte)((4096 - macPos) / 16); byte NEGVT = (byte)(InfPos / 16);

            //   byte POSVT = (byte)((-Condition.AFPOSVT) / 16); byte NEGVT = (byte)(Condition.AFNEGVT / 16);

            //     AddLog(ch, $"POSVT = {Condition.AFPOSVT}, NEGVT = {Condition.AFNEGVT}");
            AddLog(ch, $"0x0E : 0x{POSVT.ToString("X")}, 0x0F : 0x{NEGVT.ToString("X")}");


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { POSVT });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { NEGVT });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0x80) });//0x0B값 읽어서 백업해야하는지 확인

            DrvIC.Move(ch, "AF", AFCenter);

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
            Thread.Sleep(100);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
            Thread.Sleep(200);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) == 0x00)
            { }
            else
            {
                SetError(ch, NonSpecItem.AF_EPA);
                return;
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
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
            Thread.Sleep(5);
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
            Thread.Sleep(120);

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
                Thread.Sleep(100);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x02 });
                Thread.Sleep(200);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x04 });
                Thread.Sleep(200);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x08 });
                Thread.Sleep(100);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
                Thread.Sleep(200);
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
                Thread.Sleep(150);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x02 });
                Thread.Sleep(230);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x04 });
                Thread.Sleep(120);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x08 });
                Thread.Sleep(100);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x10 });
                Thread.Sleep(50);
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
        void OIS_LinearityComp_Reset(int ch)
        {
            AddLog(ch, "OIS Linearity Comp Reset");
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
        }
        void AFOpenLoopAging(int ch)
        {
            byte[] rbuf = new byte[1];
            byte DataBackup = 0x00;
            int delay = 1000000 / Condition.AFOpenLoopFreq / 2 / 1000;

            //OIS On
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            //AF OpenLoop Aging Seq
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            DataBackup = rbuf[0];
            rbuf[0] = (byte)(rbuf[0] & 0x7F);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            AddLog(ch, $"OpenLoop Range : {0} - {4095}");
            AddLog(ch, $"OpenLoop Freq : {Condition.AFOpenLoopFreq}");
            AddLog(ch, $"OpenLoop Count : {Condition.AFOpenLoopCount}");
            for (int i = 0; i < Condition.AFOpenLoopCount; i++)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF0 });
                Thread.Sleep(delay);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
                Thread.Sleep(delay);
            }

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { DataBackup });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
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
            Thread.Sleep(100);
            if (mode == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DrvIC.Move(ch, "AF", AFMin);
                    DrvIC.Move(ch, "X", OISMin);
                    DrvIC.Move(ch, "Y", OISMin);
                    Thread.Sleep(delay);
                    DrvIC.Move(ch, "AF", AFMax);
                    DrvIC.Move(ch, "X", OISMax);
                    DrvIC.Move(ch, "Y", OISMax);
                    Thread.Sleep(delay);
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
                    Thread.Sleep(delay);
                    DrvIC.Move(ch, "AF", AFMax);
                    DrvIC.Move(ch, "X", rnd.Next(OISMin, OISMax));
                    DrvIC.Move(ch, "Y", rnd.Next(OISMin, OISMax));
                    Thread.Sleep(delay);
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
            int addr = 0x00;
            string s = string.Empty;
            switch (axis)
            {
                case 0:
                    addr = DrvIC.AFSlaveAddr;
                    break;
                case 1:
                    addr = DrvIC.XSlaveAddr;
                    break;
                case 2:
                    addr = DrvIC.Y1SlaveAddr;
                    break;
            }
            for (int i = 0; i < 256; i++)
            {
                Dln.ReadArray(ch, addr, 0x00 + i, rbuf);
                data[i] = rbuf[0];

            }
            for (int i = 0; i < 16; i++)
            {
                s += $"0x{(16 * i).ToString("X2")}~0x{(16 * i + 15).ToString("X2")} : " +
                     $"{data[16 * i].ToString("X2")}{data[16 * i + 1].ToString("X2")}{data[16 * i + 2].ToString("X2")}{data[16 * i + 3].ToString("X2")}  " +
                     $"{data[16 * i + 4].ToString("X2")}{data[16 * i + 5].ToString("X2")}{data[16 * i + 6].ToString("X2")}{data[16 * i + 7].ToString("X2")}  " +
                     $"{data[16 * i + 8].ToString("X2")}{data[16 * i + 9].ToString("X2")}{data[16 * i + 10].ToString("X2")}{data[16 * i + 11].ToString("X2")}  " +
                     $"{data[16 * i + 12].ToString("X2")}{data[16 * i + 13].ToString("X2")}{data[16 * i + 14].ToString("X2")}{data[16 * i + 15].ToString("X2")}\r\n";
            }

            AddLog(ch, s);

        }
        void Act_OISLinComp(int ch, string testitem)
        {
            int addr = testitem.Contains("X") ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            string Axis = testitem.Contains("X") ? "X" : "Y";
            int axisint = testitem.Contains("X") ? 1 : 2;

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
            Thread.Sleep(100);

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
                Thread.Sleep(delay);
                STATIC.fVision.m__G.oCam[0].Grab(0);
                tmpres = STATIC.fVision.MeasureTxTyTz(0);
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
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(10);
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
                Thread.Sleep(delay);
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
            int res = coef.LinCompMain(target.ToArray(), data.ToArray(), data.Count, pvt, nvt, 0, 0, ref lincoef, ref resError);

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
        }
        void Act_FindBestAFPosition(int ch, string testitem)
        {

            int[] step = new int[9] { 0, 511, 1023, 1535, 2047, 2559, 3071, 3585, 4095 };
            int[] hallX = new int[9];
            int[] hallY = new int[9];

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", 200);
            Thread.Sleep(50);
            DrvIC.Move(ch, "AF", 0);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });

            //중간 셋팅값 확인 

            //
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            Thread.Sleep(100);

            for (int i = 0; i < 9; i++)
            {
                int[] tmphallX = new int[6];
                int[] tmphallY = new int[6];
                DrvIC.Move(ch, "AF", step[i]);
                Thread.Sleep(100);
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

            #region PID

            List<byte[]> PID = new List<byte[]>();
            PID.Add(new byte[3] { 0x10, 0x55, 0x50 });
            PID.Add(new byte[3] { 0x11, 0x2D, 0x2D });
            PID.Add(new byte[3] { 0x12, 0xFA, 0xFA });
            PID.Add(new byte[3] { 0x13, 0x19, 0x19 });
            PID.Add(new byte[3] { 0x14, 0x1E, 0x1E });
            PID.Add(new byte[3] { 0x15, 0x50, 0x50 });
            PID.Add(new byte[3] { 0x16, 0x25, 0x25 });
            PID.Add(new byte[3] { 0x17, 0x6E, 0x6E });
            PID.Add(new byte[3] { 0x18, 0xF3, 0xF4 });
            PID.Add(new byte[3] { 0x1A, 0xC2, 0xC3 });
            PID.Add(new byte[3] { 0x1B, 0xA0, 0xEE });
            PID.Add(new byte[3] { 0x1C, 0x7D, 0x7C });
            PID.Add(new byte[3] { 0x1D, 0x5C, 0x0B });
            PID.Add(new byte[3] { 0x1E, 0x39, 0x3D });
            PID.Add(new byte[3] { 0x1F, 0x9B, 0x00 });
            PID.Add(new byte[3] { 0x20, 0x8B, 0x83 });
            PID.Add(new byte[3] { 0x21, 0x8A, 0xF5 });
            PID.Add(new byte[3] { 0x22, 0x3A, 0x3F });
            PID.Add(new byte[3] { 0x23, 0xDF, 0x12 });
            PID.Add(new byte[3] { 0x27, 0x92, 0x92 });
            PID.Add(new byte[3] { 0x28, 0x92, 0x92 });
            PID.Add(new byte[3] { 0x29, 0x18, 0x18 });
            PID.Add(new byte[3] { 0x2A, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2B, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2C, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2D, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2E, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2F, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x30, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x31, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x32, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x33, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x34, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x35, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x36, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x37, 0xFC, 0x04 });
            PID.Add(new byte[3] { 0x50, 0xEF, 0xEF });
            PID.Add(new byte[3] { 0x51, 0xFF, 0xFF });
            PID.Add(new byte[3] { 0x52, 0x40, 0x40 });
            PID.Add(new byte[3] { 0x53, 0x28, 0x1E });
            PID.Add(new byte[3] { 0x54, 0x01, 0x01 });
            PID.Add(new byte[3] { 0x55, 0x78, 0x50 });
            PID.Add(new byte[3] { 0x56, 0x7D, 0x8C });
            PID.Add(new byte[3] { 0x57, 0xFA, 0xFA });
            PID.Add(new byte[3] { 0x58, 0xFA, 0xFA });
            PID.Add(new byte[3] { 0x59, 0x2D, 0x2D });
            PID.Add(new byte[3] { 0x5A, 0x50, 0x3C });
            PID.Add(new byte[3] { 0x5B, 0xFF, 0xFF });
            PID.Add(new byte[3] { 0x5C, 0x32, 0x32 });

            #endregion


            byte[] rbuf = new byte[2];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            //Set I2C Volt = 1.8V
            DrvIC.Move(ch, "AF", BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            Thread.Sleep(100);

            AddLog(ch, $"X/Y Setting register setting");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0b, new byte[] { 0x02 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x08, new byte[] { 0x08 });


            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0b, new byte[] { 0x04 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x08, new byte[] { 0x08 });

            //set i2c volt = 1.2V
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { 0x12 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { 0x14 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x08, new byte[] { 0x09 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x08, new byte[] { 0x09 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x09, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x09, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x24, new byte[] { 0x6C });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x24, new byte[] { 0x6C });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x25, new byte[] { 0x2F });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x25, new byte[] { 0x2F });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5D, new byte[] { 0x60 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5D, new byte[] { 0x60 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5F, new byte[] { 0x04 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x60, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x60, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x61, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x61, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6C, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6C, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xD8, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xD8, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xD9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xD9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDA, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDA, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDB, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDB, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDC, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDC, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDD, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDD, new byte[] { 0x00 });

            AddLog(ch, $"X/Y Register initial setting");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x3E, new byte[] { 0x85 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x3E, new byte[] { 0x85 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xFE, new byte[] { 0x0A });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xFE, new byte[] { 0x0A });
            Thread.Sleep(30);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xFF, new byte[] { 0x33 });
            Thread.Sleep(30);


            OIS_EPA_Reset(ch);
            OIS_LinearityComp_Reset(ch);


            AddLog(ch, $"X/Y PID parameter setting");
            for (int i = 0; i < PID.Count; i++)
            {
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, PID[i][0], new byte[] { PID[i][1] });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, PID[i][0], new byte[] { PID[i][2] });
            }

            AddLog(ch, $"X/Y Calibration instruction");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x09 });
            Thread.Sleep(150);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x19, new byte[] { 0x88 });
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x04, rbuf);
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x06, rbuf);

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x09 });
            Thread.Sleep(150);
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x19, new byte[] { 0x88 });
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x04, rbuf);
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x06, rbuf);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5D, new byte[] { 0x68 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5D, new byte[] { 0x68 });
            Store(ch, 1);
            Store(ch, 2);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.PowerOnOff(0, false);
            Thread.Sleep(200);
            Dln.PowerOnOff(0, true);
            Thread.Sleep(100);
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

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) return;
            if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) return; }
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 })) return;
            DrvIC.Move(ch, "AF", BestAFPos);
            //DrvIC.Move(ch, "X", 2048);
            //DrvIC.Move(ch, "Y1", 2048);
            //DrvIC.Move(ch, "Y2", 2048);
            Thread.Sleep(200);

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
                    startFreq -= (int)(startFreq * (Condition.iFRAstep / 100f));
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
                    startFreq -= (int)(startFreq * (Condition.iFRAstep / 100f));
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
                        startFreq -= (int)(startFreq * (Condition.iFRAstep / 100f));
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
            Thread.Sleep(200);

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
            int ch = port * 2;
            LEDs_All_On(port, true);
            FindResult[] fX = new FindResult[2] { new FindResult(), new FindResult() };
            FindResult[] fY = new FindResult[2] { new FindResult(), new FindResult() };
            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].Grab(0);
            fX[0] = STATIC.fVision.MeasureTxTyTz(0);

            STATIC.DrvIC.OISOn(0, "X", false);
            Thread.Sleep(500);

            STATIC.fVision.m__G.oCam[0].Grab(0);
            fX[1] = STATIC.fVision.MeasureTxTyTz(0);


            PassFails[0].Results[(int)SpecItem.x_ServoDecenter].Val = fX[0].cx[0] - fX[1].cx[0];


            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].Grab(0);
            fY[0] = STATIC.fVision.MeasureTxTyTz(0);

            STATIC.DrvIC.OISOn(0, "Y", false);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].Grab(0);
            fY[1] = STATIC.fVision.MeasureTxTyTz(0);

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
            Thread.Sleep(100);


            int[] code = new int[] { 0, 512, 1024, 1536, 2048, 2560, 3072, 3584, 4092 };


            DrvIC.Move(0, "AF", BestAFPos - 100);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", BestAFPos - 50);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", BestAFPos);
            Thread.Sleep(100);
            STATIC.fVision.m__G.oCam[port].Grab(0);
            res = STATIC.fVision.MeasureTxTyTz(0);

            RefX = res.cx[0];
            RefY = res.cy[0];



            DrvIC.Move(0, "AF", 100);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 50);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 0);
            Thread.Sleep(100);
            for (int i = 0; i < code.Length; i++)
            {
                resList.Add(new FindResult());
                DrvIC.Move(0, "AF", code[i]);
                Thread.Sleep(100);
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
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 50);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 0);
            Thread.Sleep(100);


            for (int i = 0; i < code.Length; i++)
            {
                resList2.Add(new FindResult());

                DrvIC.Move(0, "AF", code[i]);
                DrvIC.Move(0, "X", OISCenter + hallcompx[i]);
                DrvIC.Move(0, "Y", OISCenter + hallcompy[i]);
                Thread.Sleep(100);

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

            PassFails[0].Results[(int)SpecItem.x_Shift].Val = shiftX[xValMaxIndex];
            PassFails[0].Results[(int)SpecItem.y_Shift].Val = shiftY[yValMaxIndex];
            PassFails[0].Results[(int)SpecItem.x_Limit].Val = hallcompx[xLimitMaxIndex];
            PassFails[0].Results[(int)SpecItem.y_Limit].Val = hallcompy[yLimitMaxIndex];
            ShowDataResults(0, (int)SpecItem.x_Shift, (int)SpecItem.y_Limit);

            LEDs_All_On(port, false);
        }

        #endregion
    }
}
