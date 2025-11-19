using Basler.Pylon;
using FAutoLearn;
using MathNet.Numerics.LinearAlgebra;
using Matrox.MatroxImagingLibrary;

using Microsoft.Win32;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Flann;


using S2System.Vision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static alglib;

//using static alglib;
//using static alglib.apserv;
using static FAutoLearn.FAutoLearn;
using static FAutoLearn.FZMath;

using static S2System.Vision.MILlib; 
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;

namespace FZ4P
{
    public partial class FVision : Form
    {
        public DLN Dln { get { return STATIC.Dln; } }
        public Process Process { get { return STATIC.Process; } }
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public Option Option { get { return STATIC.Rcp.Option; } }
        public Global m__G;
        private Camera[] BaslerCam = new Camera[2];
        public F_Main MyOwner = null;
       
        public int mTmpCount = 0;

        //public int m_TiltLUTcount = 100; //  Default Value
        //public double[] m_AFYawLUTL   = new double[100];
        //public double[] m_ZMYawLUTL   = new double[100];
        //public double[] m_AFPitchLUTL = new double[100];
        //public double[] m_ZMPitchLUTL = new double[100];
        //public double[] m_AFYawLUTR   = new double[100];
        //public double[] m_ZMYawLUTR   = new double[100];
        //public double[] m_AFPitchLUTR = new double[100];
        //public double[] m_ZMPitchLUTR = new double[100];

        private const int CAM1 = 0;
        private const int CAM2 = 1;
        private const int MODEL0 = 0;
        private const int MODEL1 = 1;
        private const int MODEL2 = 2;
        private const int MODEL3 = 3;
        private const int MODEL4 = 4;
        private const int MODEL5 = 5;
        private const int MODEL6 = 6;
        private const int MODEL7 = 7;
        private const int MODEL8 = 8;
        public string MarkPatternFile = "C90_140.bmp";
        //int m_replayIndex = 0;

        // 마크 위치 저장 변수
        //public Point[] m_iCam1_Mark_BoxP1 = new Point[6];
        //public Point[] m_iCam1_Mark_BoxP2 = new Point[6];

        //public Point m_iCam2_Mark1_BoxP1;
        //public Point m_iCam2_Mark1_BoxP2;
        //public Point m_iCam2_Mark2_BoxP1;
        //public Point m_iCam2_Mark2_BoxP2;
        int[] ROIVerticalRange = new int[6];
        //public double ZoomFactor = 1.8; //  960 x 360 => 1.8 배로 표시, 860 x 342 => 1.895 배 표시 
        public double ZoomFactor = 0.795;
        public bool bRefresh = false;
        public double[] mLEDcurrent = new double[4] { 0, 0, 0, 0 };   //  4.56 for 220usec exposure
        public bool[] m_bSettlingROI = new bool[2] { false, false };
        public int mOptThresh = 23;
        public int mStepResponseThresh = 29;    //  mStepResponseThresh = mOptThresh - 16;ㄹ
        bool[] m_IdleSearchROI = new bool[2];
        public int m_FocusedLED = 0;
        public bool m_bSaveOrgROI = false;
        public bool m_bAllLEDOn = false;

        public int m_TopViewThresh = 10;
        public int m_TopViewMarkMax = 30;
        public int m_SideViewThresh = 5;
        public int m_SideViewMarkMin = 8;
        public int m_BlobAreaMin = 100;
        public int m_BlobAreaMax = 800;

        public double m_FakeMark = 0.38;
        public int m_FovStep = 10;

        int mTriggerGrabbedFrame = 0;
        double mTriggerGrabbedFPS = 0;
        double mHowLongItTook = 0;
        public bool m_bPrismCoordinateSystem = false;
        public struct VROI
        {
            public int top;
            public int bottom;
        };
        public struct AREA
        {
            public int top;
            public int bottom;
            public int left;
            public int right;
        };

        public VROI[] mOrgROI = new VROI[2];
        public AREA[] mAbsMark = new AREA[24];
        public AREA[] mCurMark = new AREA[24];

        public const int mHROI = 1800; // hor-ROI
        public const int mVROI = 342; // hor-ROI

        public int[] m_curBoxYmin = new int[24];        //  current Box Y min of Model,2 found
        public int[] m_curBoxYmax = new int[24];        //  current Box Y max of Model,2 found
        public int[] v_OrgROIH_min = new int[2] { 600, 600 };  //  800 => 8.8mm
        public int[] v_OrgROIH_width = new int[2] { mHROI - 1, mHROI - 1 };    //hor-ROI
        public int[] v_OrgROIV_min = new int[2] { mVROI, mVROI };  //  280 => 3.08mm
        public int[] v_OrgROIV_height = new int[2] { mVROI - 1, mVROI - 1 };
        //public int[] v_OrgROIV_min       = new int[2] { 380 , 380 };  //  280 => 3.08mm
        //public int[] v_OrgROIV_height       = new int[2] { 380 - 1 , 380 - 1  };

        public const int ABSEXPOSURE = 73;
        public int[] v_OrgExposure = new int[2] { ABSEXPOSURE, ABSEXPOSURE };     //  1000 FPS 달성을 위한 최대 노출시간 
        public int[] v_OrgStlExposure = new int[2] { ABSEXPOSURE, ABSEXPOSURE };
        public int[] v_OrgGain = new int[2] { 66, 66 };          //  40mm 1, 68mm 12
        public int[] v_OrgStlGain = new int[2] { 66, 66 };       //  40mm 20, 68mm 46
        public double[] m_Yscale = new double[4] { 1, 1, 1, 1 };
        public bool m_bPrepareCLAFCal = false;
        public bool mLoaded = false;
        public bool[] mSocketLoaded = new bool[2] { false, false };
        public bool m_bLEDMarkCheck = false;
        public bool mbSaveFailImage = false;

      

    

        List<Button> CalBtnGroup;


        public FVision()
        {
            InitializeComponent();
            //ReadVisionParam();
        }

        public int GetTriggerGrabbedFrame()
        {
            return mTriggerGrabbedFrame;
        }
        public double GetTriggerGrabbedFPS()
        {
            return mTriggerGrabbedFPS;
        }
        public double GetHowLongItTook()
        {
            return mHowLongItTook;
        }
        public void SetTriggerGrabbedFrame(int fn)
        {
            mTriggerGrabbedFrame = fn;
        }
        public void SetTriggerGrabbedFPS(double fps)
        {
            mTriggerGrabbedFPS = fps;
        }
        public void SetHowLongItTook(double time)
        {
            mHowLongItTook = time;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void FVision_Load(object sender, EventArgs e)
        {
            try
            {
                m__G = Global.GetInstance();
                cbContinuosMode.Enabled = false;
                //string which = "true";
                //int i = 0;

                Thread threadInitBalser = new Thread(() => InitBaslerCam());
                threadInitBalser.Start();
                //InitBaslerCam();

                System.Drawing.Point[] pts =  {
                                new System.Drawing.Point( 2,  2),
                                new System.Drawing.Point(69,  2),
                                new System.Drawing.Point(69,  25),
                                new System.Drawing.Point(35,  48),
                                new System.Drawing.Point(35,  48),
                                new System.Drawing.Point(2, 25),
                          };
                // Make the GraphicsPath.
                GraphicsPath polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(pts);
                Region polygon_region = new Region(polygon_path);
                btnFOVDown.Region = polygon_region;
                btnFOVDown.SetBounds(
                    btnFOVDown.Location.X,
                    btnFOVDown.Location.Y, pts[2].X + 4, pts[3].Y + 4);

                System.Drawing.Point[] ptsU =  {
                                new System.Drawing.Point(2,  48),
                                new System.Drawing.Point(2,  24),
                                new System.Drawing.Point(35,  2),
                                new System.Drawing.Point(35,  2),
                                new System.Drawing.Point(69,  25),
                                new System.Drawing.Point(69, 48),
                          };
                // Make the GraphicsPath.
                polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(ptsU);
                polygon_region = new Region(polygon_path);
                btnFOVUp.Region = polygon_region;
                btnFOVUp.SetBounds(
                    btnFOVUp.Location.X,
                    btnFOVUp.Location.Y, ptsU[4].X + 4, ptsU[5].Y + 4);

                System.Drawing.Point[] ptsL =  {
                                new System.Drawing.Point(2,  35),
                                new System.Drawing.Point(24,  2),
                                new System.Drawing.Point(48,  2),
                                new System.Drawing.Point(48,  69),
                                new System.Drawing.Point(24,  69),
                                new System.Drawing.Point(2, 35),
                          };
                // Make the GraphicsPath.
                polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(ptsL);
                polygon_region = new Region(polygon_path);
                btnFOVLeft.Region = polygon_region;
                btnFOVLeft.SetBounds(
                    btnFOVLeft.Location.X,
                    btnFOVLeft.Location.Y, ptsL[2].X + 4, ptsL[4].Y + 4);

                System.Drawing.Point[] ptsR =  {
                                new System.Drawing.Point(2,  2),
                                new System.Drawing.Point(24,  2),
                                new System.Drawing.Point(48,  35),
                                new System.Drawing.Point(48,  35),
                                new System.Drawing.Point(24,  69),
                                new System.Drawing.Point(2, 69),
                          };
                // Make the GraphicsPath.
                polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(ptsR);
                polygon_region = new Region(polygon_path);
                btnFOVRight.Region = polygon_region;
                btnFOVRight.SetBounds(
                    btnFOVRight.Location.X,
                    btnFOVRight.Location.Y, ptsR[2].X + 4, ptsR[5].Y + 4);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            radioButton10Step.Checked = true;
            cbLiveWithMarks.Checked = false;
            rb1step.Checked = true;
            this.BackColor = Color.FromArgb(96, 96, 100);
            //MessageBox.Show("aaa");
            //if (m__G!=null)
            SetEdgeBand(Condition.iEdgeBand);

            //else
            //    SetEdgeBand();

            //MessageBox.Show("bbb");
            tbInfo.BringToFront();
            tbVsnLog.BringToFront();

            //ChartMTF.Hide();
            //LoadscaleNTheta();
            if (m__G.oCam[0].mFAL != null)
                if (m__G.oCam[0].mFAL.mFZM != null)
                {
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);
                }

            groupBox4.Hide();
            btnChangeCrop.Show();
          
            btnSaveOrgPosition.Hide();

            InitializeHexpodPivot();
            if (m__G.m_bCalibrationModel)
            {
                m__G.mGageCounter?.OpenAllport();
                MAX_TRGGRAB_COUNT = 3000;
            }
            else
            {
             
            }

          
            ScanName = "AF Scan";
          
        }
        public void BufferInit()
        {
            int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].mTargetTriggerCount = 3000;
            int frmCnt = 3000;
            //for (int i = 0; i < m__G.oCam[0].mTargetTriggerCount; i++)
            //    m__G.oCam[0].GrabB(i);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt + 1];


            m__G.oCam[0].SetTriggeredframeCount(frmCnt);

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.oCam[0].ForceTriggerTime();
            m__G.oCam[0].mTrgBufLength = 3000;

            ChangeFiducialMark(m__G.mFAL.mCandidateIndex);
            SetDefaultMarkConfig(true);

            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            //ProcessVisionData(frmCnt, m__G.mMaxThread);
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;
        }
        public string camID0 = "";
        public string camID1 = "";
        public bool mHybridStageAvailable = true;
        public void InitBaslerCam()
        {
            //string which = "true";
            int i = 0;

            int[] lMarkGap = new int[4] { 11000, 11000, 11000, 11000 };
            if (!mLoaded)
            {
                if (m__G == null)
                    m__G = Global.GetInstance();

                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = false;
                        btnAllLEDOn.Enabled = false;
                        btnLEDDOWN.Enabled = false;
                        btnLEDUP.Enabled = false;
                        btnHalt2.Enabled = false;
                     
                    });

                int camCount = 2;


                if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt"))
                {
                    MessageBox.Show("Camera ID not exists.");
                    return;
                }
                StreamReader sr = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt");
                string fullText = sr.ReadToEnd();
                sr.Close();
                string[] camIDs = fullText.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                camID0 = "";
                foreach (string lstr in camIDs)
                {
                    try
                    {
                        BaslerCam[0] = new Camera(lstr);  //"22727087"
                                                          //BaslerCam[0].CameraOpened += Configuration.AcquireSingleFrame;
                        BaslerCam[0].Open();
                        camID0 = lstr;
                        break;
                    }
                    catch
                    {
                        ;
                    }
                }
                if (camID0 == "")
                {
                    MessageBox.Show("Camera ID is not found. Check Camera ID and Restart Application.");
                    return;
                }
                else
                {
                    StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt");
                    wr.WriteLine(camID0);
                    foreach (string lstr in camIDs)
                    {
                        if (lstr != camID0)
                            wr.WriteLine(lstr);
                    }
                    wr.Close();
                }
                //StreamReader sr = new StreamReader("CameraID.txt");
                //camID0 = sr.ReadLine();
                //sr.Close();
                //try
                //{
                //    BaslerCam[0] = new Camera(camID0);  //"22727087"
                //                                        //BaslerCam[0].CameraOpened += Configuration.AcquireSingleFrame;
                //    BaslerCam[0].Open();
                //}
                //catch
                //{
                //    MessageBox.Show("Camera ID is not correct. Check Camera ID and Restart Application.");
                //    return;
                //}
                m__G.SetTesterID(camID0);
                ReadOrgROI(camCount);

                BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

                //BaslerCam[1] = null;
                m__G.mCamCount = 1;
                //tbVsnLog.Text += "\t m__G.mCamCount  = " + m__G.mCamCount;
                //BaslerCam[0].Parameters[PLCamera.UserSetLoad].Execute();
                //BaslerCam[0].Parameters[PLCamera.UserSetSave].Execute();

                if (rbLED1.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        rbLED1.Checked = true;
                    });
                }


                m_FocusedLED = 0;


                for (i = 0; i < m__G.mCamCount; i++)
                    SetNewROIXY(i, v_OrgROIH_min[i], v_OrgROIH_min[i] + v_OrgROIH_width[i], v_OrgROIV_min[i], v_OrgROIV_min[i] + v_OrgROIV_height[i]);

                //ReadZeroGap(m__G.mCamCount);
                //ReadCalibrationTiltData();

                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        m__G.oCam[0].SelectWindow(panelCam0.Handle);
                        //panelCam0.Size = new Size((int)(mHROI*1.833), 627);
                        //panelCam0.Location = new Point(1940 - (int)(mHROI * 1.833), 4);
                    });
                else
                {
                    m__G.oCam[0].SelectWindow(panelCam0.Handle);
                    //panelCam0.Size = new Size((int)(mHROI * 1.833), 627);
                    //panelCam0.Location = new Point(1940 - (int)(mHROI * 1.833), 4);
                }

                m__G.oCam[0].DisplayZoom(ZoomFactor, ZoomFactor);

                BaslerCam[0].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
                BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(true);
                //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
                BaslerCam[0].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[0]);
                BaslerCam[0].Parameters[PLCamera.GammaEnable].SetValue(true);

                m__G.oCam[0].SetBlobAreaMinMax(m_BlobAreaMin, m_BlobAreaMax);
                string strScaleRotation = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNOpticalR.txt";
                double stop = 0;
                double sside = 0;
                double rtop = 0;
                double rside = 0;

                m__G.oCam[0].LoadScaleNOpticalRotation(strScaleRotation, ref stop, ref sside, ref rtop, ref rside);

                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = true;
                        btnAllLEDOn.Enabled = true;
                        btnLEDDOWN.Enabled = true;
                        btnLEDUP.Enabled = true;
                        btnHalt2.Enabled = true;
                      

                        radioButton10Step.Checked = true;
                        rb1step.Checked = true;
                        cbContinuosMode.Enabled = true;
                    });
            }
            TransferModelFileList();
            SetRawGainNGamma(Condition.iRawGain, Condition.iGamma);
            SetExposure(0, Condition.iExposure);
            LoadBackbroundNoise();
            LoadScaleNTheta();
            //LoadTXTYZeroOffset();
            SetDefaultMarkConfig();
            //string ZLUTfile = m__G.m_RootDirectory + "\\DoNotTouch\\ZLUT_" + camID0 + ".txt";
            //GetZLUT(ZLUTfile);
            string strTXTYTZoffset = LoadTXTYZeroOffset();
            //STATIC.fManage.("CSH ID " + camID0 + "\t" + strTXTYTZoffset);

            //Regstry Write ====
            Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("6AxisTester");
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\6AxisTester", true);

            if (reg.GetValue(camID0) == null)
                reg.SetValue(camID0, DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss"));
            reg.Close();
            //==================

            // 241206   // YLUT 적용안함
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            mLoaded = true;

            //m__G.oCam[0].RegisterDelegates(m__G.fGraph.mDriverIC.AckSignal);


            // Crop Pos File Load & Init
            bool isLoad = m__G.oCam[0].LoadCropPosFromXml(camID0);
            m__G.oCam[0].InitCrop(!isLoad);
            CropCgap = m__G.oCam[0].CropCgap;

            string motorHomeFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageHomePos{camID0}.txt";
            string motorCSFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageCSPos{camID0}.txt";
            string motorSppedFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageSpeed{camID0}.txt";
         
        }
      
        public int GetMasterZeroCount()
        {
            string cPath = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero";
            return Directory.GetFiles(cPath).Length;
        }
        public int GetMasterZeroIndex()
        {
            int curIndex;
            string cPath = m__G.m_RootDirectory + "\\DoNotTouch\\PreviousOffsetIndex.txt";
            StreamReader rd = new StreamReader(cPath);
            curIndex = int.Parse(rd.ReadLine());
            rd.Close();
            return curIndex;
        }
        public void SetMasterZeroIndex(int index)
        {
            string cPath = m__G.m_RootDirectory + "\\DoNotTouch\\PreviousOffsetIndex.txt";
            StreamWriter sw = new StreamWriter(cPath);
            sw.WriteLine(index);
            sw.Close();
        }
        public void InitMasterZeroList()
        {

            string[] files = Directory.GetFiles(m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero");

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    MasterList.Items.Clear();
                    for (int j = 0; j < files.Length; j++)
                    {
                        MasterList.Items.Add(Path.GetFileName(files[j]));
                    }

                });
            else
            {
                MasterList.Items.Clear();
                for (int j = 0; j < files.Length; j++)
                {
                    MasterList.Items.Add(Path.GetFileName(files[j]));
                }
            }
        }
        public void SetEdgeBand(int nband = 7)
        {
            if (m__G == null) return;
            if (m__G.oCam[0] == null) return;
            if (m__G.oCam[0].mFAL == null) return;
            m__G.oCam[0].mFAL.mCalcEffBand = nband;
        }
        public void SetRawGainNGamma(int lGain, double lGamma)
        {
            if (BaslerCam[0] == null) return;
            BaslerCam[0].Parameters[PLCamera.GainRaw].SetValue(lGain);
            BaslerCam[0].Parameters[PLCamera.Gamma].SetValue(lGamma);
            BaslerCam[0].Parameters[PLCamera.GammaEnable].SetValue(true);
        }
        public void EnableBtns(bool bEnable)
        {
            if (bEnable)
            {
                //btnSetAbsZero.Enabled = false;
            }
            else
            {
                //btnSetAbsZero.Enabled = true;
            }

            //SlowlyChk.Checked = false; //2021.08.31 added
            rbLED2.Checked = true; //2021.08.31 added
            rb1step.Checked = true;
            cbSetTXTYwithMaster.Checked = false;

            MasterList.Enabled = false;
            btnDeleteMaster.Enabled = false;
            btnAddMaster.Enabled = false;
            btnInitialTilt.Enabled = false;
            btnSetMasterTilt.Enabled = false;
            tbMasterTX.Enabled = false;
            tbMasterTY.Enabled = false;

            m_FocusedLED = 0;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam1_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[0].DrawClear();
            //    m__G.oCam[0].nBoxP1.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[0].nBoxP1.Y = (int)(e.Y / ZoomFactor);
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[0].nBoxP2.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[0].nBoxP2.Y = (int)(e.Y / ZoomFactor);

            //    m__G.oCam[0].DrawClear();
            //    m__G.oCam[0].DrawDCCross(Brushes.Red);
            //    m__G.oCam[0].DrawDCBox(m__G.oCam[0].nBoxP1, m__G.oCam[0].nBoxP2, Brushes.Yellow);
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam2_MouseDown(object sender, MouseEventArgs e)
        {
            //if (m__G.mCamCount < 2) return;
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[1].DrawClear();
            //    m__G.oCam[1].nBoxP1.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[1].nBoxP1.Y = (int)(e.Y / ZoomFactor);
            //}
        }

        //public void ClearImgBuf()
        //{
        //    m__G.oCam[0].DrawClear();
        //    m__G.oCam[CAM2].DrawClear();
        //}


        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam2_MouseMove(object sender, MouseEventArgs e)
        {
            //if (m__G.mCamCount < 2) return;
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[1].nBoxP2.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[1].nBoxP2.Y = (int)(e.Y / ZoomFactor);
            //    m__G.oCam[1].DrawClear();
            //    m__G.oCam[1].DrawDCCross(Brushes.Red);
            //    m__G.oCam[1].DrawDCBox(m__G.oCam[1].nBoxP1, m__G.oCam[1].nBoxP2, Brushes.Yellow);
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnLive2_Click(object sender, EventArgs e)
        {
            //label1.Text = "Number of Camera : " + m__G.mCamCount.ToString();
            //cbContinuosMode.Checked = true;
            //Thread.Sleep(200);

            StartLive();

            //m__G.oCam[1].DrawDC_Circle(Brushes.Red, 200);    // DrawCircle khkim_170920
            //Imae Crop === 
            //Task.Factory.StartNew(() => 
            //{
            //});

            //m__G.oCam[0].CropImage(0);

        }

        public void StartLive()
        {
            cbContinuosMode.Checked = true;
            Thread.Sleep(200);

            bHaltLive = false;
            m__G.mbSuddenStop[0] = true;
            m__G.mDoingStatus = "Checking Vision";

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            Process.LEDs_All_On(0, true);
            btnAllLEDOn.ForeColor = Color.White;

            m_bAllLEDOn = true;
            //label6.Location = new Point(10, 140);
            //label6.Text = "Live On";
            //m__G.oCam[0].DrawDC_Circle(Brushes.Red, 200);    // DrawCircle khkim_170920
            m__G.oCam[0].DrawAllRectangles();

            if (cbLiveWithMarks.Checked && bLiveFindMark == false)
            {
                // 250326 폰트 설정 cbLiveWithMarks_CheckedChanged()에서 이동
                tbInfo.Font = new Font("Calibri", 14, FontStyle.Bold);
                Task.Run(() => LiveFindMark());
            }
            else
            {
                tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);

                if (m__G.mCamCount > 1)
                {
                    m__G.oCam[1].ClearDisp();
                    m__G.oCam[1].LiveA();
                }
                else
                {
                    m__G.oCam[0].ClearDisp();
                    m__G.oCam[0].LiveA();

                }
            }
        }

        bool bLiveFindMark = false;

        public void LiveFindMark()
        {
            //if (!bLiveFindMark)
            //    return;

            // 250326 이미 LiveFindMark 중 이라면 return 으로 조건 변경
            if (bLiveFindMark)
                return;

            m__G.mDoingStatus = "LiveFindMark";

            // 데이터 mCalibrationFullData에 저장해서 SetMasterTilt할때 마지막 데이터 사용
            mCalibrationFullData.Clear();
            double[] lCalibrationData = new double[23];


            Process.LEDs_All_On(0, true);
            Thread.Sleep(10);

            bLiveFindMark = true;
            int fcnt = 0;

            int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].mTargetTriggerCount = 3000;
            int frmCnt = 3000;
            //for (int i = 0; i < m__G.oCam[0].mTargetTriggerCount; i++)
            //    m__G.oCam[0].GrabB(i);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt + 1];


            m__G.oCam[0].SetTriggeredframeCount(frmCnt);

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.oCam[0].ForceTriggerTime();
            m__G.oCam[0].mTrgBufLength = 3000;

            ChangeFiducialMark(m__G.mFAL.mCandidateIndex);
            SetDefaultMarkConfig(true);

            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            ProcessVisionData(frmCnt, m__G.mMaxThread);
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;


            while (!bHaltLive)
            {
                m__G.oCam[0].DrawClear();
                DrawMarkPositions();
                m__G.oCam[0].DrawAllRectangles();

                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                m__G.oCam[0].GrabB(fcnt);
                FindMarks(fcnt++);
                if (fcnt == 10000)
                    fcnt = 0;

                m__G.oCam[0].mFAL.RecoverFromBackupFMI();


                if (tbInfo.InvokeRequired)
                {
                    tbInfo.BeginInvoke(new Action(() =>
                    {
                        string lstr = tbInfo.Text;
                        string[] lineStr = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        if (lineStr.Length > 6)
                        {
                            lstr = "";
                            for (int i = 0; i < 6; i++)
                                lstr += lineStr[lineStr.Length - 6 + i] + "\r\n";


                            tbInfo.Text = lstr;
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        }
                    }));
                }
                else
                {
                    string lstr = tbInfo.Text;
                    string[] lineStr = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (lineStr.Length > 6)
                    {
                        lstr = "";
                        for (int i = 0; i < 6; i++)
                            lstr += lineStr[lineStr.Length - 6 + i] + "\r\n";


                        tbInfo.Text = lstr;
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                }

                Thread.Sleep(180);
                if (!cbLiveWithMarks.Checked)
                    break;
            }
            // bHaltLive = false;
            bLiveFindMark = false;

        }
        public double mExpectedYfromSideNStpTopNS = 0;
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnGrab2_Click(object sender, EventArgs e)
        {


            //SetDefaultMarkConfig(true);
            //DrawMarkPositions();
        }

        public void DrawMarkPositions()
        {
            //  Default Mark Position

            m__G.mbSuddenStop[0] = true;        //  왜 ?
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in DrawMarkPositions()");
            System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
                new System.Drawing.Point( 730, 78 ),
                new System.Drawing.Point( 234, 93 ),
                new System.Drawing.Point( 730, 255 ),
                new System.Drawing.Point( 234, 275 ),
                new System.Drawing.Point( 439, 294 ),
                new System.Drawing.Point( 532, 294 ) };

            mExpectedYfromSideNStpTopNS = markPos[5].Y - markPos[0].Y;
            //m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

            if (m__G.mFAL != null)
            {

                //string markPosFile = m__G.mFAL.GetFileNameOfMarkPosOnPanel();
                //if (File.Exists(markPosFile))
                //{
                //    StreamReader sr = new StreamReader(markPosFile);
                //    string allLines = sr.ReadToEnd();
                //    sr.Close();
                //    List<Point> mPos = new List<Point>();
                //    string[] eachLine = allLines.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //    for (int i = 0; i < eachLine.Length; i++)
                //    {
                //        if (eachLine[i].Length < 3)
                //            continue;
                //        string[] xypos = eachLine[i].Split(',');
                //        if (xypos.Length < 2)
                //            continue;
                //        Point lp = new Point();
                //        lp.X = int.Parse(xypos[0]);
                //        lp.Y = int.Parse(xypos[1]);
                //        mPos.Add(lp);
                //    }
                //    mExpectedYfromSideNStpTopNS = Math.Abs(mPos[0].Y - mPos[mPos.Count - 1].Y);
                //    if (mPos.Count > 0)
                //    {
                //        markPos = mPos.ToArray();
                //    }
                //}
                m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);
                m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
            }
            //m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
        }
        public System.Drawing.Point[] mStdMarkPos = new System.Drawing.Point[6];


        public void Wait1ms(int ms = 1)
        {
            double res = 0;
            for (int i = 0; i < 4000000 * ms; i++)
            {
                res = Math.Abs(Math.Sin(i));
                res = Math.Sqrt(Math.Sin(Math.Log10(res)));
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        // 처음 시작할때 Live 상태 아니므로 bHaltLive는 true
        //bool bHaltLive = false;
        public bool bHaltLive = true;
        public void GrabHalt()
        {
            //label6.Text = "";
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].HaltA();
            bHaltLive = true;
            btnAllLEDOn.ForeColor = Color.SlateGray;
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
            Thread.Sleep(100);
            Process.LEDs_All_On(0, false);

            //m__G.oCam[0].ClearDisp();
            bThreadManualFindMarks = false;
        }
        private void btnHalt2_Click(object sender, EventArgs e)
        {
            GrabHalt();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        //Graphics gr;
        private void btnClear1_Click(object sender, EventArgs e)
        {
            tbVsnLog.Text = "";
            tbInfo.Text = "";
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].DrawClear();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public bool RestoreROI()
        {
            for (int i = 0; i < 2; i++)
            {
                SetNewROIY(i, v_OrgROIV_min[i], (v_OrgROIV_min[i] + v_OrgROIV_height[i]), v_OrgExposure[i]);   //  재시도??
            }
            return true;
        }

        public bool AdjustROI(int Cam)
        {
            if (mAbsMark[Cam].top - mAbsMark[Cam].bottom < m__G.mVROI[Cam])
                return SetNewROIY(Cam, mAbsMark[Cam].bottom, mAbsMark[Cam].top, v_OrgExposure[Cam]);
            else
                return SetNewROIY(Cam, mAbsMark[Cam].bottom, mAbsMark[Cam].bottom + m__G.mVROI[Cam], v_OrgExposure[Cam]);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------

        private async void btnOISXReplay_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].ClearDisp();
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].ClearDisp();
                m__G.oCam[1].DrawClear();
            }

            Button bt = (Button)sender;

            bt.Enabled = false;
            await Task.Factory.StartNew(() =>
            {
                m__G.oCam[0].ReplayBufToDisp(ScanName, 1);
            });

            bt.Enabled = true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Process_OISXReplay(int port, int interval = 10)
        {
            //for (int n = 0; n < m__G.m_AFPeakTimeIndex+1; n += 1)

            int nOldinterval = interval;///2021.08.31 Slowlychk added

            if (m__G.oCam[0].dAFZM_FrameCount > 0)
            {
                for (int n = 0; n < m__G.oCam[0].dAFZM_FrameCount; n++)
                {
                    m__G.oCam[0].BufCopy2Disp_OISX(n);
                    if (m__G.mCamCount > 1)
                        m__G.oCam[1].BufCopy2Disp_OISX(n);

                    Thread.Sleep(interval);
                }
            }
            else
            {
                //for (int n = 0; n < m__G.fGraph.mAF_FrameCount; n++)
                //{
                //    m__G.oCam[0].BufCopy2Disp_OISX(n);

                //    Thread.Sleep(interval);
                //}
            }

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnOISXReplay.Enabled = true;
                    btnOISXStepReplay.Enabled = true;
                });
            }
            else
            {
                btnOISXReplay.Enabled = true;
                btnOISXStepReplay.Enabled = true;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------

        private void btnOISYReplay_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].ClearDisp();
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].ClearDisp();
                m__G.oCam[1].DrawClear();
            }
            tbVsnLog.Text += "AF Step Total : " + m__G.oCam[0].dAFStep_FrameCount.ToString() + "frame \r\n";
            //btnOISYReplay.Enabled = false;

            int stepInterval = 20;
            if (m__G.oCam[0].dAFStep_FrameCount < 50)
                stepInterval = 200;

            //if (SlowlyChk.Checked) stepInterval *= 10;

            Thread ThreadReplayL = new Thread(() => Process_AFStepReplay(stepInterval));
            ThreadReplayL.Start();
            //m_replayIndex = 0;

            //Thread ThreadReplayL = new Thread(() => Process_OISYReplay(0));
            //ThreadReplayL.Start();
            //if (m__G.mCamCount > 1)
            //{
            //    Thread ThreadReplayR = new Thread(() => Process_OISYReplay(1));
            //    ThreadReplayR.Start();
            //}
            //m_replayIndex = 0;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Process_AFStepReplay(int delay)
        {

            for (int n = 0; n < m__G.oCam[0].dAFStep_FrameCount; n++)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbVsnLog.Text += "AF Step : " + n.ToString() + "frame Show \r\n";
                    });
                }
                else
                    tbVsnLog.Text += "AF Step : " + n.ToString() + "frame Show \r\n";

                m__G.oCam[0].BufCopy2Disp_XStep(n);
                if (m__G.mCamCount > 1)
                    m__G.oCam[1].BufCopy2Disp_XStep(n);
                Thread.Sleep(1 + delay);
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void GetCurrentROIY(int Cam, ref int ROImin, ref int ROImax, ref int exposureTime)
        {
            long tmp = 0;
            tmp = BaslerCam[Cam].Parameters[PLCamera.Width].GetValue();
            ROImin = (int)tmp;


            tmp = BaslerCam[Cam].Parameters[PLCamera.Height].GetValue();
            ROImax = (int)tmp;


            double tmpb = BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].GetValue();
            exposureTime = (int)tmpb;
        }



        public bool RefreshBasler(int port)
        {
            BaslerCam[port].Close();
            Thread.Sleep(100);
            BaslerCam[port].Open();
            Thread.Sleep(200);

            BaslerCam[port].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
            //if (m__G.mCamCount < 2)
            //    BaslerCam[port].Parameters[PLCamera.ReverseX].SetValue(true);
            //else
            //    BaslerCam[port].Parameters[PLCamera.ReverseX].SetValue(false);

            //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
            return true;
        }
        public bool RefreshBasler()
        {
            BaslerCam[0].Close();
            if (m__G.mCamCount > 1)
                BaslerCam[1].Close();

            Thread.Sleep(100);
            BaslerCam[0].Open();
            if (m__G.mCamCount > 1)
                BaslerCam[1].Open();
            Thread.Sleep(200);

            BaslerCam[0].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
            if (m__G.mCamCount > 1)
                BaslerCam[1].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");

            //BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(true);
            //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
            //if (m__G.mCamCount > 1)
            //{
            //    BaslerCam[1].Parameters[PLCamera.ReverseX].SetValue(true);
            //    BaslerCam[1].Parameters[PLCamera.ReverseY].SetValue(false);
            //}
            //else
            //{
            //    BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(false);
            //}

            return true;
        }

        public bool ShiftROI(int Cam, int dx, int dy)
        {
            v_OrgROIH_min[Cam] = ((v_OrgROIH_min[Cam] + dx) / 8) * 8;

            if (v_OrgROIH_min[Cam] < 1)
            {
                v_OrgROIH_min[Cam] = 0;
            }
            int lROIHmax = v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam];
            if (lROIHmax > 2078)
            {
                v_OrgROIH_min[Cam] = 2079 - v_OrgROIH_width[Cam];
            }

            v_OrgROIV_min[Cam] = v_OrgROIV_min[Cam] + dy;
            if (v_OrgROIV_min[Cam] < 1)
            {
                v_OrgROIV_min[Cam] = 1;
            }
            int lROIVmax = v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam];
            if (lROIVmax > 1078)
            {
                v_OrgROIV_min[Cam] = 1024 - v_OrgROIV_height[Cam];
            }

            SetNewROIXY(Cam, v_OrgROIH_min[Cam], v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam], v_OrgROIV_min[Cam], v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]);
            SaveOrgROI(1);

            return true;
        }

        public bool SetNewROIXY(int Cam)
        {
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam], v_OrgROIV_min[Cam], v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]);
            return true;
        }

        public int GetROIY(int Cam)
        {
            long res = BaslerCam[Cam].Parameters[PLCamera.OffsetY].GetValue();           // 사이즈 width 조절
            return (int)res;
        }
        public bool SetNewROIXY(int Cam, int fovL, int fovR, int fovU, int fovD /* L < R , U < D */)
        {
            int height = fovD - fovU + 1;
            int width = fovR - fovL + 1;
            int top = fovU;
            int left = fovL;

            if (width + left > 2040)
                left = 2040 - width;
            if (height + fovU > 1079)
                top = 1079 - height;

            BaslerCam[Cam].Parameters[PLCamera.OffsetX].SetValue(0);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(0);       // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.Width].SetValue(width);           // 사이즈 width 조절      
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(height);         // 사이즈 Height 조절       
            BaslerCam[Cam].Parameters[PLCamera.OffsetX].SetValue(left);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(top);       // 사이즈 Height 조절

            //BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(v_OrgExposure[Cam]);            // 노출시간 조절, usec
            return true;
        }
        public void ChangeROIHeight(int Cam, int newHeight)
        {
            long oldOffsetY = BaslerCam[Cam].Parameters[PLCamera.OffsetY].GetValue();           // 사이즈 width 조절
            if (oldOffsetY + newHeight >= 1080)
                oldOffsetY -= (oldOffsetY + newHeight - 1080);

            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(oldOffsetY);       // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(newHeight);           // 사이즈 width 조절      khkim191106
        }
        public void ChangeROIYOffsetY(int Cam, int offsetY)
        {
            long oldHeight = BaslerCam[Cam].Parameters[PLCamera.Height].GetValue();           // 사이즈 width 조절
            if (offsetY + oldHeight >= 1080)
                offsetY = (int)(1080 - oldHeight);

            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(offsetY);       // 사이즈 Height 조절
        }
        public void ChangeROIYOffsetDeltaY(int Cam, int offsetDeltaY)
        {
            long oldHeight = BaslerCam[Cam].Parameters[PLCamera.Height].GetValue();           // 사이즈 width 조절
            long oldOffsetY = BaslerCam[Cam].Parameters[PLCamera.OffsetY].GetValue();           // 사이즈 width 조절
            if (oldOffsetY + offsetDeltaY + oldHeight >= 1080)
                oldOffsetY = (int)(1080 - oldHeight);
            else
                oldOffsetY += offsetDeltaY;

            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(oldOffsetY);       // 사이즈 Height 조절
        }


        public void SetExposure(int Cam, int expTime, int gainRaw = -1)
        {
            if (BaslerCam[Cam] == null) return;
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(expTime);            // 노출시간 조절
            //if (gainRaw > 33 && gainRaw < 61)
            //    BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(gainRaw);
        }

        public void SetOrgExposure(int Cam)
        {
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(v_OrgExposure[Cam]);            // 노출시간 조절, usec
            //BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[Cam]);           // 사이즈 Height 조절
        }

        public void SetExpGain(int Cam, int expTime/*, int analog_gain = 29*/)
        {
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(expTime);            // 노출시간 조절, usec
            //BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[Cam]);           // 사이즈 Height 조절
        }

        public bool SetNewROIY(int Cam, int ROImin, int ROImax, int expTime/*, int analog_gain = 29*/)
        {
            BaslerCam[Cam].Parameters[PLCamera.Width].SetValue(40);           // 사이즈 width 조절      khkim191106
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(10);         // 사이즈 Height 조절       khkim191106

            //BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[Cam]);           // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.Width].SetValue(v_OrgROIH_width[Cam] + 1);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(v_OrgROIV_height[Cam] + 1);         // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetX].SetValue(v_OrgROIH_min[Cam]);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(ROImin);       // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(expTime);            // 노출시간 조절, usec

            return true;
        }

        public void ShowCalParam()
        {
            //tb_AutoCalPXX1.Text = m__G.Cal_xx[0].ToString("F4");
            //tb_AutoCalPYX1.Text = m__G.Cal_yx[0].ToString("F4");
            //tb_AutoCalPXY1.Text = m__G.Cal_xy[0].ToString("F4");
            //tb_AutoCalPYY1.Text = m__G.Cal_yy[0].ToString("F4");

        }

        public void ClearCam(int numCam = 1) // 17년 1월 6일 이전버전
        {
            m__G.oCam[0].ClearDisp();
            if (m__G.mCamCount > 1)
                m__G.oCam[1].ClearDisp();
        }

        private void btnOISXStepReplay_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].ClearDisp();
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].ClearDisp();
                m__G.oCam[1].DrawClear();
            }
            btnOISXReplay.Enabled = false;
            btnOISXStepReplay.Enabled = false;


            Thread ThreadReplayL = new Thread(() => Process_OISXReplay(0, 1));
            ThreadReplayL.Start();
            //m_replayIndex = 0;

        }

        ////private void btnOISYStepReplay_Click(object sender, EventArgs e)
        ////{
        ////    m__G.oCam[0].ClearDisp();
        ////    m__G.oCam[0].DrawClear();
        ////    if (m__G.mCamCount > 1)
        ////    {
        ////        m__G.oCam[1].ClearDisp();
        ////        m__G.oCam[1].DrawClear();
        ////    }
        ////    tbVsnLog.Text += "Zoom Step : " + m__G.oCam[0].dAFStep_FrameCount.ToString() + "frame \r\n";

        ////    Thread ThreadReplayL = new Thread(() => Process_ZMStepReplay(20));
        ////    ThreadReplayL.Start();
        ////    m_replayIndex = 0;

        ////}
        //private void Process_ZMStepReplay(int delay)
        //{

        //    for (int n = 0; n < m__G.oCam[0].dZoomStep_FrameCount; n++)
        //    {
        //        m__G.oCam[0].BufCopy2Disp_ZMStep(n);
        //        if (m__G.mCamCount > 1)
        //            m__G.oCam[1].BufCopy2Disp_ZMStep(n);
        //        Thread.Sleep(1 + delay);
        //    }
        //}

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        //public void LEDOn(int port)
        //{
        //    m__G.fGraph.Drive_LED(0, mLEDcurrent[0]);
        //    m__G.fGraph.Drive_LED(1, mLEDcurrent[1]);
        //}

        //public void LEDOff(int port)
        //{
        //    m__G.fGraph.Drive_LED(0, 0);
        //    m__G.fGraph.Drive_LED(1, 0);
        //}
        private void btnLEDUP_Click(object sender, EventArgs e)
        {
            //m_FocusedLED = 0;
            //MessageBox.Show("Focus Led : " + m_FocusedLED.ToString());
            m__G.mDoingStatus = "Checking Vision";

            int ch = m_FocusedLED;

            if (ch == 1)
            {
                if (tbLedLeft.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedLeft.Text);
            }
            else
            {
                if (tbLedRight.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedRight.Text);
            }

            if (mLEDcurrent[ch] > 0)
                mLEDcurrent[ch] -= 0.01;

            //m__G.oCam[0].HaltA();
            //if (m__G.mCamCount > 1)
            //    m__G.oCam[1].HaltA();

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            //m__G.fGraph.Drive_LED(ch, mLEDcurrent[ch]);

            tbLedLeft.Text = mLEDcurrent[1].ToString("F3");
            tbLedRight.Text = mLEDcurrent[0].ToString("F3");

            if (bHaltLive) StartLive();
            else
            {
                Process.LEDs_All_On(0, true);
            }

        }

        private void btnLEDDOWN_Click(object sender, EventArgs e)
        {

            //m_FocusedLED = 0;
            //MessageBox.Show("Focus Led : " + m_FocusedLED.ToString());
            m__G.mDoingStatus = "Checking Vision";

            int ch = m_FocusedLED;

            if (ch == 1)
            {
                if (tbLedLeft.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedLeft.Text);
            }
            else
            {
                if (tbLedRight.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedRight.Text);
            }


            if (mLEDcurrent[ch] < 5)
                mLEDcurrent[ch] += 0.01;

            //m__G.oCam[0].HaltA();
            //if (m__G.mCamCount > 1)
            //    m__G.oCam[1].HaltA();

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            //m__G.fGraph.Drive_LED(ch, mLEDcurrent[ch]);

            tbLedLeft.Text = mLEDcurrent[1].ToString("F3");   //  Left
            tbLedRight.Text = mLEDcurrent[0].ToString("F3");   //  Right

            if (bHaltLive) StartLive();
            else
            {
                Process.LEDs_All_On(0, true);
            }
        }

        private void btnFOVUp_Click(object sender, EventArgs e)
        {
         

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();


            if (v_OrgROIV_min[Cam] > m_FovStep)
            {
                v_OrgROIV_min[Cam] -= m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }

        private void btnFOVDown_Click(object sender, EventArgs e)
        {
            

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            int lROIVmax = v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam];
            if (lROIVmax < (1088 - m_FovStep))
            {
                v_OrgROIV_min[Cam] += m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }

        private void btnFOVLeft_Click(object sender, EventArgs e)
        {
          

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            if (v_OrgROIH_min[Cam] > m_FovStep)
            {
                v_OrgROIH_min[Cam] -= m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }

        private void btnFOVRight_Click(object sender, EventArgs e)
        {
           

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            int lROIHmax = v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam];
            if (lROIHmax < (2088 - m_FovStep))
            {
                v_OrgROIH_min[Cam] += m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }
        public void SaveOrgROI(int camCount = 2)
        {
            while (m_bSaveOrgROI)
                Thread.Sleep(10);

            m_bSaveOrgROI = true;

            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\LastOrgROI" + camID0 + ".txt";
            StreamWriter wr = new StreamWriter(filename);
            for (int i = 0; i < camCount; i++)
            {
                wr.WriteLine(v_OrgROIH_min[i].ToString());
                wr.WriteLine((v_OrgROIH_min[i] + v_OrgROIH_width[i]).ToString());
                wr.WriteLine(v_OrgROIV_min[i].ToString());
                wr.WriteLine((v_OrgROIV_min[i] + v_OrgROIV_height[i]).ToString());
            }
            wr.Close();
            // YLUT 적용안함.
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            m_bSaveOrgROI = false;
        }

        //public void ReadVisionParam()
        //{
        //    string filename = "VisionParam.txt";
        //    if (!File.Exists(filename)) return;
        //    StreamReader sr = new StreamReader(filename);
        //    string allData = sr.ReadToEnd();
        //    sr.Close();
        //    string[] mylines = allData.Split("\n".ToCharArray());

        //    string[] figures = mylines[0].Split('\t');
        //    m_TopViewThresh = Convert.ToInt32(figures[0]);
        //    figures = mylines[1].Split('\t');
        //    m_TopViewMarkMax = Convert.ToInt32(figures[0]);
        //    figures = mylines[2].Split('\t');
        //    m_SideViewThresh = Convert.ToInt32(figures[0]);
        //    figures = mylines[3].Split('\t');
        //    m_SideViewMarkMin = Convert.ToInt32(figures[0]);
        //    figures = mylines[4].Split('\t');
        //    m_BlobAreaMin = Convert.ToInt32(figures[0]);
        //    figures = mylines[5].Split('\t');
        //    m_BlobAreaMax = Convert.ToInt32(figures[0]);
        //    figures = mylines[6].Split('\t');
        //    m_FakeMark = Convert.ToDouble(figures[0]);
        //}

        public void ReadOrgROI(int camCount = 2)
        {
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\LastOrgROI" + camID0 + ".txt";
            if (!File.Exists(filename)) return;
            StreamReader sr = new StreamReader(filename);
            int tmp = 0;
            for (int i = 0; i < camCount; i++)
            {
                v_OrgROIH_min[i] = Convert.ToInt32(sr.ReadLine());
                tmp = Convert.ToInt32(sr.ReadLine());
                v_OrgROIV_min[i] = Convert.ToInt32(sr.ReadLine());
                tmp = Convert.ToInt32(sr.ReadLine());
            }
            sr.Close();
        }
        //public void SaveZeroGap(int camCount = 2)
        //{
        //    string filename = m__G.m_RootDirectory + "\\DoNotTouch\\LastGap.txt";
        //    StreamWriter wr = new StreamWriter(filename);

        //    for (int i = 0; i < camCount; i++)
        //    {
        //        //m__G.mZeroXgap[0, 0] = mxL1gap[0] = LcamL2xgap;    //  Left Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[0, 1] = mxL2gap[0] = LcamL3xgap;    //  Left Cam Lens 3, X gap between Mark
        //        //m__G.mZeroXgap[1, 0] = mxL1gap[1] = RcamL2xgap;    //  Right Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[1, 1] = mxL2gap[1] = RcamL3xgap;    //  Right Cam Lens 3, X gap between Mark

        //        wr.WriteLine(m__G.mZeroXgap[i, 0].ToString("F4"));
        //        wr.WriteLine(m__G.mZeroYgap[i, 0].ToString("F4"));
        //        wr.WriteLine(m__G.mZeroXgap[i, 1].ToString("F4"));
        //        wr.WriteLine(m__G.mZeroYgap[i, 1].ToString("F4"));
        //    }
        //    wr.Close();
        //}
        //public void ReadZeroGap(int camCount = 2)
        //{
        //    string filename = "LastGap.txt";
        //    if (!File.Exists(filename)) return;
        //    StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default);
        //    for (int i = 0; i < camCount; i++)
        //    {
        //        //m__G.mZeroXgap[0, 0] = mxL1gap[0] = LcamL2xgap;    //  Left Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[0, 1] = mxL2gap[0] = LcamL3xgap;    //  Left Cam Lens 3, X gap between Mark
        //        //m__G.mZeroXgap[1, 0] = mxL1gap[1] = RcamL2xgap;    //  Right Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[1, 1] = mxL2gap[1] = RcamL3xgap;    //  Right Cam Lens 3, X gap between Mark

        //        m__G.mZeroXgap[i, 0] = Convert.ToDouble(sr.ReadLine()); //  Lens 2  dx
        //        m__G.mZeroYgap[i, 0] = Convert.ToDouble(sr.ReadLine()); //  Lens 2  dy
        //        m__G.mZeroXgap[i, 1] = Convert.ToDouble(sr.ReadLine()); //  Lens 3  dx
        //        m__G.mZeroYgap[i, 1] = Convert.ToDouble(sr.ReadLine()); //  Lens 3  dy
        //    }
        //    sr.Close();

        //}

        private void btnLoadUnloadR_Click(object sender, EventArgs e)
        {

        }

        public List<double[]> mCalibrationFullData = new List<double[]>();  //  (um, min)
        public List<double[]> mGageFullData = new List<double[]>();         //  (um)
        public List<double[]> mPrismTXTYTZ = new List<double[]>();         //  (um)
        public List<double[]> mStdevTXTYTZ = new List<double[]>();  //  (um, min)
        //public List<double[]> mProbeTZ = new List<double[]>();

        //public void JHMotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        //{
        //    m__G.mDoingStatus = "Checking Vision";

        //    int mavNum = 4;

        //    m__G.oCam[0].mTargetTriggerCount = 3000;
        //    m__G.oCam[0].dAFZM_FrameCount = 9;
        //    m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
        //    double[] gageData = null;

        //    for (int mi = 0; mi < 5; mi++)
        //        m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

        //    /////////////////////////////////////////////////////////////////////////
        //    // 자화로 대체
        //    // gageData = 현재 값 읽어오기;
        //    ///////////////////////////////////////////////////////////////////////////////

        //    for (int i = 0; i < mavNum; i++)
        //        m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

        //    if (IsOrg)
        //    {
        //        m__G.oCam[0].mFAL.LoadFMICandidate();
        //        m__G.oCam[0].mFAL.BackupFMI();
        //        SetDefaultMarkConfig(true);
        //    }

        //    double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
        //    double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

        //    m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

        //    //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
        //    string[] strtmp = new string[2] { "", "" };
        //    m__G.mbSuddenStop[0] = false;
        //    TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

        //    double[] lCalibrationData = new double[23];

        //    int ci = 0;
        //    strtmp[ci] = "";
        //    m__G.mFAL.mCandidateIndex = ci;

        //    if (IsOrg)
        //        ChangeFiducialMark(ci);

        //    if (IsOrg)
        //    {

        //        m__G.oCam[0].PrepareFineCOG();
        //        m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
        //        m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기
        //    }
        //    m__G.oCam[0].FineCOG(false, 1, 0);    // 마크찾기
        //    m__G.oCam[0].FineCOG(false, 2, 0);    // 마크찾기
        //    m__G.oCam[0].FineCOG(false, 3, 0);    // 마크찾기
        //    m__G.oCam[0].FineCOG(false, 4, 0);    // 마크찾기

        //    if (ci != 0)
        //        m__G.mFAL.mFZM.mbCompY = ci;
        //    else
        //        m__G.mFAL.mFZM.mbCompY = 0;
        //    double sx = 0;
        //    double sy = 0;
        //    double sz = 0;
        //    double tx = 0;
        //    double ty = 0;
        //    double tz = 0;

        //    for (int findex = 1; findex < mavNum + 1; findex++)
        //    {
        //        //NthMeasure(findex, true);

        //        //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

        //        sx += m__G.oCam[0].mC_pX[findex] * umscale;
        //        sy += m__G.oCam[0].mC_pY[findex] * umscale;
        //        sz += m__G.oCam[0].mC_pZ[findex] * umscale;
        //        tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  radian to minute
        //        ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  radian to minute
        //        tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  radian to minute
        //    }
        //    lCalibrationData[0] = sx;
        //    lCalibrationData[1] = sy;
        //    lCalibrationData[2] = sz;
        //    lCalibrationData[3] = tx;
        //    lCalibrationData[4] = ty;
        //    lCalibrationData[5] = tz;

        //    strtmp[ci] = Nth.ToString() + "\t"
        //           + lCalibrationData[0].ToString("F2") + "\t"
        //           + lCalibrationData[1].ToString("F2") + "\t"
        //           + lCalibrationData[2].ToString("F2") + "\t"
        //           + lCalibrationData[3].ToString("F2") + "\t"
        //           + lCalibrationData[4].ToString("F2") + "\t"
        //           + lCalibrationData[5].ToString("F2") + "\t";

        //    double[] xavg = new double[12];
        //    double[] yavg = new double[12];
        //    for (int findex = 1; findex < mavNum + 1; findex++)
        //    {
        //        for (int i = 0; i < 12; i++)
        //        {
        //            if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
        //            xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
        //            yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
        //        }
        //    }
        //    int kk = 0;
        //    for (int i = 0; i < 12; i++)
        //    {
        //        if (xavg[i] == 0) continue;
        //        strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

        //        lCalibrationData[6 + 2 * kk] = xavg[i];
        //        lCalibrationData[6 + 2 * kk + 1] = yavg[i];
        //        kk++;
        //    }
        //    if (gageData != null)
        //    {
        //        if (gageData.Length == 6)
        //        {
        //            // 자화 부호 맞춰야함.
        //            lCalibrationData[16] = gageData[0];   // um
        //            lCalibrationData[17] = gageData[1];   // um
        //            lCalibrationData[18] = gageData[2];   // um
        //            lCalibrationData[19] = gageData[3]; // min          // TX   acr min
        //            lCalibrationData[20] = gageData[4]; // min          // TY   acr min
        //            lCalibrationData[21] = gageData[5]; // min          // TZ   acr min

        //            //  출력은 um, arcmin
        //            strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1")
        //                 + "\t" + lCalibrationData[19].ToString("F1") + "\t" + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1");
        //        }
        //    }
        //    if (ci == 0 && IsSave)
        //    {
        //        mCalibrationFullData.Add(lCalibrationData);
        //        mGageFullData.Add(gageData);
        //    }


        //    if (InvokeRequired)
        //    {
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            DrawMarkDetected();
        //            //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
        //            if (ltextBox[0].Text.Length > 7000)
        //                ltextBox[0].Text = strtmp[0] + "\r\n";
        //            else
        //                ltextBox[0].Text += strtmp[0] + "\r\n";

        //            ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
        //            ltextBox[0].ScrollToCaret();
        //        });
        //    }
        //    else
        //    {
        //        DrawMarkDetected();
        //        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

        //        if (ltextBox[0].Text.Length > 7000)
        //            ltextBox[0].Text = strtmp[0] + "\r\n";
        //        else
        //            ltextBox[0].Text += strtmp[0] + "\r\n";

        //        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
        //        ltextBox[0].ScrollToCaret();
        //    }
        //    //if (InvokeRequired)
        //    //{
        //    //    BeginInvoke((MethodInvoker)delegate
        //    //    {
        //    //        if (ltextBox[1].Text.Length > 7000)
        //    //            ltextBox[1].Text = strtmp[1] + "\r\n";
        //    //        else
        //    //            ltextBox[1].Text += strtmp[1] + "\r\n";

        //    //        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
        //    //        ltextBox[1].ScrollToCaret();
        //    //    });
        //    //}
        //    //else
        //    //{
        //    //    if (ltextBox[1].Text.Length > 2000)
        //    //        ltextBox[1].Text = strtmp[1] + "\r\n";
        //    //    else
        //    //        ltextBox[1].Text += strtmp[1] + "\r\n";

        //    //    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
        //    //    ltextBox[1].ScrollToCaret();
        //    //}

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();
        //    m__G.mDoingStatus = "IDLE";
        //    m__G.mIDLEcount = 0;
        //}

        public double GetStdevOfArray(double[] x, int startIndex, int count)
        {
            double res = 0;
            double mean = 0;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                mean += x[i];
            }
            mean /= count;
            double SumOfSquare = 0;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                SumOfSquare += Math.Pow(x[i] - mean, 2);
            }
            res = Math.Sqrt(SumOfSquare / count);
            return res;
        }
        public void MotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 16;  //4
            if (m__G.m_bPrismCS)
                mavNum = 4;  //4

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;
            //double[] gageData2 = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////
            //double[] lProbeTZ = new double[2];
            //  Grab 과 가장 가까운 시점에 gage 를 읽어들인다.
            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;

                if (m__G.m_bCalibrationModel)
                {
                    gageData = m__G.mGageCounter.ReadPortAll();
                    // Probe TZ 튀는 현상 디버깅
                    //lProbeTZ[0] = m__G.mGageCounter.probeTZ[0];  
                    //lProbeTZ[1] = m__G.mGageCounter.probeTZ[1];
                    //if (mGageFullData.Count != 0)
                    //{
                    //    if (m__G.m_bPrismCS)
                    //    {
                    //        double error = Math.Abs(mGageFullData[mGageFullData.Count - 1][3] - gageData[3]);

                    //        if (error > 2.62)
                    //        { 
                    //            gageData = m__G.mGageCounter.ReadPortAll();
                    //        }
                    //    }
                    //}
                }
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            //if ( m__G.m_bPrismCS )
            //{
            //    if (m__G.mGageCounter != null)
            //    {
            //        m__G.mGageCounter.m__G = m__G;
            //        if (m__G.m_bCalibrationModel)
            //            gageData2 = m__G.mGageCounter.ReadPortAll();
            //    }
            //}

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  pixel to um

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[22]; // 33
            double[] lStdevTXTYTZ = new double[6];
            double[] lPrismTXTYTZ = new double[3];

            //* 250404 Probe Z(TY1, TY2) 디버깅 *//
            // [0. Probe TY1, TY2 평균] , [1. X거리, Y거리에 따른 Z 보정 (최종)]
            //double[] probeZ = new double[2];
            //***********************************//


            int ci = 0;
            strtmp[ci] = "";
            m__G.mFAL.mCandidateIndex = ci;

            if (IsOrg)
                ChangeFiducialMark(ci);

            if (IsOrg)
            {

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
                m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기
            }

            //if (m__G.m_bPrismCS)
            //{
            //    Task[] taskArray = new Task[5];

            //    m__G.fManage.AddViewLog(string.Format("FineCOG Parallel\r\n"));
            //    int count = mavNum + 1;
            //    Task task1 = Task.Run(() => {
            //        for (int findex = 1; findex < mavNum + 1; findex+=5)
            //            m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
            //    });
            //    taskArray[0] = task1;
            //    Task task2 = Task.Run(() => {
            //        for (int findex = 2; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 1);    // 마크찾기
            //    });
            //    taskArray[1] = task2;
            //    Task task3 = Task.Run(() => {
            //        for (int findex = 3; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 2);    // 마크찾기
            //    });
            //    taskArray[2] = task3;
            //    Task task4 = Task.Run(() => {
            //        for (int findex = 4; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 3);    // 마크찾기
            //    });
            //    taskArray[3] = task4;
            //    Task task5 = Task.Run(() => {
            //        for (int findex = 5; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 4);    // 마크찾기
            //    });
            //    taskArray[4] = task5;
            //    Task.WaitAll(taskArray);
            //}
            //else
            //{
            for (int findex = 1; findex < mavNum + 1; findex++)
                m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
            //}


            if (ci != 0)
                m__G.mFAL.mFZM.mbCompY = ci;
            else
                m__G.mFAL.mFZM.mbCompY = 0;

            double sx = 0;
            double sy = 0;
            double sz = 0;
            double tx = 0;
            double ty = 0;
            double tz = 0;

            //double[] lPrismTXTYTZ = new double[3];
            double[] lProbePrismTXTYTZ = new double[3];
            //double[] lErrorPrismTXTYTZ = new double[3];
            bool bGageOn = false;

            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                //NthMeasure(findex, true);

                //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                sx += m__G.oCam[0].mC_pX[findex] * umscale;
                sy += m__G.oCam[0].mC_pY[findex] * umscale;
                sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  Radian to minute / mavNum
                ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  Radian to minute / mavNum
                tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  Radian to minute / mavNum
            }
            if (m__G.m_bPrismCS)
            {
                lStdevTXTYTZ[0] = GetStdevOfArray(m__G.oCam[0].mC_pTX, 1, mavNum) * RAD_To_MIN; //  Radian to Arcminute
                lStdevTXTYTZ[1] = GetStdevOfArray(m__G.oCam[0].mC_pTY, 1, mavNum) * RAD_To_MIN;
                lStdevTXTYTZ[2] = GetStdevOfArray(m__G.oCam[0].mC_pTZ, 1, mavNum) * RAD_To_MIN;

                //  CSHead 좌표계 기준으로 계산된 평균치만 Prism 좌표계로 변환해서 lPrismTXTYTZ 에 저장
                lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                tx = lPrismTXTYTZ[1];
            }

            lCalibrationData[0] = sx;//-sx;
            lCalibrationData[1] = sy;//sy;
            lCalibrationData[2] = sz;//-sz;
            lCalibrationData[3] = tx;//tx;
            lCalibrationData[4] = ty;//-ty;
            lCalibrationData[5] = tz;//-tz;

            //strtmp[ci] = Nth.ToString() + "\t" + sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
            strtmp[ci] = Nth.ToString() + "\t"
                   + lCalibrationData[0].ToString("F2") + "\t"
                   + lCalibrationData[1].ToString("F2") + "\t"
                   + lCalibrationData[2].ToString("F2") + "\t"
                   + lCalibrationData[3].ToString("F2") + "\t"
                   + lCalibrationData[4].ToString("F2") + "\t"
                   + lCalibrationData[5].ToString("F2") + "\t";

            double[] xavg = new double[12];
            double[] yavg = new double[12];
            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                    xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                    yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                }
            }
            int kk = 0;
            for (int i = 0; i < 12; i++)
            {
                if (xavg[i] == 0) continue;
                strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                lCalibrationData[6 + 2 * kk] = xavg[i];
                lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                kk++;
            }
            if (gageData != null)
            {
                if (gageData.Length == 7)
                {
                    bGageOn = true;

                    double[] XYTz = new double[3];
                    XYTz[0] = gageData[0];
                    XYTz[1] = gageData[1];
                    XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                    double[] TxTyZ = new double[3];
                    TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                    TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                    TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                    double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);

                    lCalibrationData[16] = XYTz[0]; // um
                    lCalibrationData[17] = XYTz[1]; // um
                    lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                    lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian -> min으로 통일
                    lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian -> min으로 통일
                    lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian -> min으로 통일  241216 부호 변경

                    //if (m__G.m_bPrismCS)
                    //{
                    //    double[] XYTz2 = new double[3];
                    //    double[] TxTyZ2 = new double[3];
                    //    XYTz2[0] = gageData2[0];
                    //    XYTz2[1] = gageData2[1];
                    //    XYTz2[2] = Math.Atan((gageData2[2] - gageData2[3]) / 45000);  //  45mm
                    //    TxTyZ2[0] = Math.Atan((gageData2[4] - (gageData2[5] + gageData2[6]) / 2) / 83000);  //  83mm
                    //    TxTyZ2[1] = Math.Atan((gageData2[5] - gageData2[6]) / 120000);  //  120mm
                    //    TxTyZ2[2] = (gageData2[5] + gageData2[6]) / 2;  //  120mm
                    //    double compZ2 = ProbeZcompensationForTXTY(XYTz2[0], XYTz2[1], TxTyZ2[2], TxTyZ2[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ2[1] /*- mPorg.TY * MIN_To_RAD*/);

                    //    //double[] lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(TxTyZ2[0], TxTyZ2[1], XYTz2[2]);

                    //    lCalibrationData[16] = (XYTz[0]   + XYTz2[0] )/2     ; // um
                    //    lCalibrationData[17] = (XYTz[1]   + XYTz2[1] )/2     ; // um
                    //    lCalibrationData[18] = (compZ     + compZ2   )/2     ;// TxTyZ[2]; // um
                    //    lCalibrationData[19] = (TxTyZ[0]  + TxTyZ2[0]) * RAD_To_MIN / 2;       // TX   radian -> min으로 통일
                    //    lCalibrationData[20] = (TxTyZ[1]  + TxTyZ2[1]) * RAD_To_MIN / 2;       // TY   radian -> min으로 통일
                    //    lCalibrationData[21] = (-XYTz[2]  - XYTz2[2] ) * RAD_To_MIN / 2;       // TZ   radian -> min으로 통일  241216 부호 변경

                    //    lStdevTXTYTZ[3] = Math.Abs(0.7071*(TxTyZ[0] - TxTyZ2[0])) * RAD_To_MIN; //  Radian to Arcminute
                    //    lStdevTXTYTZ[4] = Math.Abs(0.7071*(TxTyZ[1] - TxTyZ2[1])) * RAD_To_MIN;
                    //    lStdevTXTYTZ[5] = Math.Abs(0.7071*(XYTz[2] - XYTz2[2])) * RAD_To_MIN;
                    //}

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [0. Probe TY1, TY2 평균]
                    //probeZ[0] = TxTyZ[2];               //  저장 0
                    //***********************************//

                    //
                    // Hexapod
                    //
                    //lCalibrationData[16] = gageData[0] * 1000; //ofx - XYTz[0]) * 1000;     // um
                    //lCalibrationData[17] = -gageData[1] * 1000; //ofy + XYTz[1]) * 1000;     // um
                    //lCalibrationData[18] = -gageData[2] * 1000; //TxTyZ[2] * 1000;   // um
                    //lCalibrationData[19] = -gageData[3] * Math.PI / 180; //xTyZ[0];           // TX   radian
                    //lCalibrationData[20] = gageData[4] * Math.PI / 180; //TxTyZ[1];          // TY   radian
                    //lCalibrationData[21] = -gageData[5] * Math.PI / 180; //XYTz[2];           // TZ   radian
                    //
                    //

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [1. X거리, Y거리에 따른 Z 보정 (최종)]
                    //probeZ[1] = compZ;               //  저장 1
                    //lCalibrationData[22] = m__G.mGageCounter.probeTY1[0];  // [0.첫 Probe 값] 
                    //lCalibrationData[23] = m__G.mGageCounter.probeTY2[0];
                    //lCalibrationData[24] = m__G.mGageCounter.probeTY1[1];  // [1.X, Y이동, Z회전에 따른 Glass 기울기에 대한 Probe 보정]
                    //lCalibrationData[25] = m__G.mGageCounter.probeTY2[1];
                    //lCalibrationData[26] = m__G.mGageCounter.probeTY1[2];  //  [2.Glass 두께에 따른 Probe보정]
                    //lCalibrationData[27] = m__G.mGageCounter.probeTY2[2];
                    //lCalibrationData[28] = probeZ[0];  // [0.Probe TY1, TY2 평균] 
                    //lCalibrationData[29] = probeZ[1];  // [1.X거리, Y거리에 따른 Z 보정(최종)]
                    //lCalibrationData[30] = m__G.mGageCounter.probeTX[0];
                    //lCalibrationData[31] = m__G.mGageCounter.probeTX[1];
                    //lCalibrationData[32] = m__G.mGageCounter.probeTX[2];
                    //***********************************//

                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + lCalibrationData[19].ToString("F1") + "\t"
                        + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1") + "\t";


                    //double[] motorCurpos = MotorCurPos6D();
                    //strtmp[ci] += $"{motorCurpos[0]:F1}\t{motorCurpos[1]:F1}\t{motorCurpos[2]:F1}\t{motorCurpos[3]:F1}\t{motorCurpos[4]:F1}\t{motorCurpos[5]:F1}";


                }

                if (m__G.m_bPrismCS)
                {
                    // lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[3], lCalibrationData[4], lCalibrationData[5], true);

                    if (bGageOn)
                    {
                        lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[19], lCalibrationData[20], lCalibrationData[21], true, true);
                        lProbePrismTXTYTZ[1] = lCalibrationData[19];

                        /////////////////////////////////////////////////////////////////////////////////////////////////////
                        /////////////////////////////////////////////////////////////////////////////////////////////////////
                        ////  개별데이터  저장목적
                        //if ( File.Exists(mDataFile100))
                        //{
                        //    string saveStr = "";
                        //    StreamWriter writer = File.AppendText(mDataFile100);
                        //    for (int findex = 1; findex < mavNum + 1; findex++)
                        //    {
                        //        tx = m__G.oCam[0].mC_pTX[findex] * minscale * mavNum;   //  Radian to minute / mavNum
                        //        ty = m__G.oCam[0].mC_pTY[findex] * minscale * mavNum;   //  Radian to minute / mavNum
                        //        tz = m__G.oCam[0].mC_pTZ[findex] * minscale * mavNum;   //  Radian to minute / mavNum
                        //        double [] lPrismTXTYTZ100 = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                        //        tx = lPrismTXTYTZ100[1];
                        //        saveStr = lProbePrismTXTYTZ[0].ToString("F5") + "," + lProbePrismTXTYTZ[1].ToString("F5") + "," + lProbePrismTXTYTZ[2].ToString("F5") + "," +
                        //                    lPrismTXTYTZ100[0].ToString("F5") + "," + lPrismTXTYTZ100[1].ToString("F5") + "," + lPrismTXTYTZ100[2].ToString("F5") + ",";
                        //        writer.WriteLine(saveStr);
                        //    }
                        //    writer.Close();
                        //    ///////////////////////////////////////////////////////////////////////////////////////////////////
                        //    ///////////////////////////////////////////////////////////////////////////////////////////////////

                        //}

                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5") + "\t" +
                                        lProbePrismTXTYTZ[0].ToString("F5") + "\t" + lProbePrismTXTYTZ[1].ToString("F5") + "\t" + lProbePrismTXTYTZ[2].ToString("F5");
                    }
                    else
                    {
                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                    }
                }
            }
            if (ci == 0 && IsSave)
            {
                mCalibrationFullData.Add(lCalibrationData);
                mGageFullData.Add(gageData);
                mStdevTXTYTZ.Add(lStdevTXTYTZ);
                mPrismTXTYTZ.Add(lPrismTXTYTZ);
                // 디버깅
                //mProbeTZ.Add(lProbeTZ);

            }


            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                    if (ltextBox[0].Text.Length > 10000)
                        ltextBox[0].Text = strtmp[0] + "\r\n";
                    else
                        ltextBox[0].Text += strtmp[0] + "\r\n";

                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                });
            }
            else
            {
                DrawMarkDetected();
                //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

                if (ltextBox[0].Text.Length > 10000)
                    ltextBox[0].Text = strtmp[0] + "\r\n";
                else
                    ltextBox[0].Text += strtmp[0] + "\r\n";

                ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                ltextBox[0].ScrollToCaret();
            }
            //if (InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate
            //    {
            //        if (ltextBox[1].Text.Length > 7000)
            //            ltextBox[1].Text = strtmp[1] + "\r\n";
            //        else
            //            ltextBox[1].Text += strtmp[1] + "\r\n";

            //        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
            //        ltextBox[1].ScrollToCaret();
            //    });
            //}
            //else
            //{
            //    if (ltextBox[1].Text.Length > 2000)
            //        ltextBox[1].Text = strtmp[1] + "\r\n";
            //    else
            //        ltextBox[1].Text += strtmp[1] + "\r\n";

            //    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
            //    ltextBox[1].ScrollToCaret();
            //}

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void MotorizedFindMarksAnosis(ref double[] lCalibrationData, int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 16;  //4
            if (m__G.m_bPrismCS)
                mavNum = 100;  //4

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;
            double[] gageData2 = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            //  Grab 과 가장 가까운 시점에 gage 를 읽어들인다.
            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;
                if (m__G.m_bCalibrationModel)
                    gageData = m__G.mGageCounter.ReadPortAll();
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            if (m__G.m_bPrismCS)
            {
                if (m__G.mGageCounter != null)
                {
                    m__G.mGageCounter.m__G = m__G;
                    if (m__G.m_bCalibrationModel)
                        gageData2 = m__G.mGageCounter.ReadPortAll();
                }
            }

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  pixel to um

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lStdevTXTYTZ = new double[6];
            double[] lPrismTXTYTZ = new double[3];

            //* 250404 Probe Z(TY1, TY2) 디버깅 *//
            // [0. Probe TY1, TY2 평균] , [1. X거리, Y거리에 따른 Z 보정 (최종)]
            //double[] probeZ = new double[2];
            //***********************************//

            int ci = 0;
            strtmp[ci] = "";
            m__G.mFAL.mCandidateIndex = ci;

            if (IsOrg)
                ChangeFiducialMark(ci);

            if (IsOrg)
            {

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
                m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기
            }

            if (m__G.m_bPrismCS)
            {
                Task[] taskArray = new Task[5];

                //m__G.fManage.AddViewLog(string.Format("FineCOG Parallel\r\n"));
                int count = mavNum + 1;
                Task task1 = Task.Run(() => {
                    for (int findex = 1; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
                });
                taskArray[0] = task1;
                Task task2 = Task.Run(() => {
                    for (int findex = 2; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 1);    // 마크찾기
                });
                taskArray[1] = task2;
                Task task3 = Task.Run(() => {
                    for (int findex = 3; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 2);    // 마크찾기
                });
                taskArray[2] = task3;
                Task task4 = Task.Run(() => {
                    for (int findex = 4; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 3);    // 마크찾기
                });
                taskArray[3] = task4;
                Task task5 = Task.Run(() => {
                    for (int findex = 5; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 4);    // 마크찾기
                });
                taskArray[4] = task5;
                Task.WaitAll(taskArray);
            }
            else
            {
                for (int findex = 1; findex < mavNum + 1; findex++)
                    m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
            }


            if (ci != 0)
                m__G.mFAL.mFZM.mbCompY = ci;
            else
                m__G.mFAL.mFZM.mbCompY = 0;

            double sx = 0;
            double sy = 0;
            double sz = 0;
            double tx = 0;
            double ty = 0;
            double tz = 0;

            //double[] lPrismTXTYTZ = new double[3];
            double[] lProbePrismTXTYTZ = new double[3];
            //double[] lErrorPrismTXTYTZ = new double[3];
            //bool bGageOn = false;

            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                //NthMeasure(findex, true);

                //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                sx += m__G.oCam[0].mC_pX[findex] * umscale;
                sy += m__G.oCam[0].mC_pY[findex] * umscale;
                sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  Radian to minute / mavNum
                ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  Radian to minute / mavNum
                tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  Radian to minute / mavNum
            }
            if (m__G.m_bPrismCS)
            {
                lStdevTXTYTZ[0] = GetStdevOfArray(m__G.oCam[0].mC_pTX, 1, mavNum) * RAD_To_MIN; //  Radian to Arcminute
                lStdevTXTYTZ[1] = GetStdevOfArray(m__G.oCam[0].mC_pTY, 1, mavNum) * RAD_To_MIN;
                lStdevTXTYTZ[2] = GetStdevOfArray(m__G.oCam[0].mC_pTZ, 1, mavNum) * RAD_To_MIN;

                lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                tx = lPrismTXTYTZ[1];
            }


            lCalibrationData[0] = sx;//-sx;
            lCalibrationData[1] = sy;//sy;
            lCalibrationData[2] = sz;//-sz;
            lCalibrationData[3] = tx;//tx;
            lCalibrationData[4] = ty;//-ty;
            lCalibrationData[5] = tz;//-tz;

            //strtmp[ci] = Nth.ToString() + "\t" + sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
            strtmp[ci] = Nth.ToString() + "\t"
                   + lCalibrationData[0].ToString("F2") + "\t"
                   + lCalibrationData[1].ToString("F2") + "\t"
                   + lCalibrationData[2].ToString("F2") + "\t"
                   + lCalibrationData[3].ToString("F2") + "\t"
                   + lCalibrationData[4].ToString("F2") + "\t"
                   + lCalibrationData[5].ToString("F2") + "\t";

            double[] xavg = new double[12];
            double[] yavg = new double[12];
            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                    xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                    yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                }
            }
            int kk = 0;
            for (int i = 0; i < 12; i++)
            {
                if (xavg[i] == 0) continue;
                strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                lCalibrationData[6 + 2 * kk] = xavg[i];
                lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                kk++;
            }
            if (gageData != null)
            {
                if (gageData.Length == 7)
                {
                    double[] XYTz = new double[3];
                    XYTz[0] = gageData[0];
                    XYTz[1] = gageData[1];
                    XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                    double[] TxTyZ = new double[3];
                    TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                    TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                    TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                    double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);

                    lCalibrationData[16] = XYTz[0]; // um
                    lCalibrationData[17] = XYTz[1]; // um
                    lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                    lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian -> min으로 통일
                    lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian -> min으로 통일
                    lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian -> min으로 통일  241216 부호 변경

                    if (m__G.m_bPrismCS)
                    {
                        double[] XYTz2 = new double[3];
                        double[] TxTyZ2 = new double[3];
                        XYTz2[0] = gageData2[0];
                        XYTz2[1] = gageData2[1];
                        XYTz2[2] = Math.Atan((gageData2[2] - gageData2[3]) / 45000);  //  45mm
                        TxTyZ2[0] = Math.Atan((gageData2[4] - (gageData2[5] + gageData2[6]) / 2) / 83000);  //  83mm
                        TxTyZ2[1] = Math.Atan((gageData2[5] - gageData2[6]) / 120000);  //  120mm
                        TxTyZ2[2] = (gageData2[5] + gageData2[6]) / 2;  //  120mm
                        double compZ2 = ProbeZcompensationForTXTY(XYTz2[0], XYTz2[1], TxTyZ2[2], TxTyZ2[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ2[1] /*- mPorg.TY * MIN_To_RAD*/);

                        //double[] lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(TxTyZ2[0], TxTyZ2[1], XYTz2[2]);

                        lCalibrationData[16] = (XYTz[0] + XYTz2[0]) / 2; // um
                        lCalibrationData[17] = (XYTz[1] + XYTz2[1]) / 2; // um
                        lCalibrationData[18] = (compZ + compZ2) / 2;// TxTyZ[2]; // um
                        lCalibrationData[19] = (TxTyZ[0] + TxTyZ2[0]) * RAD_To_MIN / 2;       // TX   radian -> min으로 통일
                        lCalibrationData[20] = (TxTyZ[1] + TxTyZ2[1]) * RAD_To_MIN / 2;       // TY   radian -> min으로 통일
                        lCalibrationData[21] = (-XYTz[2] - XYTz2[2]) * RAD_To_MIN / 2;       // TZ   radian -> min으로 통일  241216 부호 변경

                        lStdevTXTYTZ[3] = Math.Abs(0.7071 * (TxTyZ[0] - TxTyZ2[0])) * RAD_To_MIN; //  Radian to Arcminute
                        lStdevTXTYTZ[4] = Math.Abs(0.7071 * (TxTyZ[1] - TxTyZ2[1])) * RAD_To_MIN;
                        lStdevTXTYTZ[5] = Math.Abs(0.7071 * (XYTz[2] - XYTz2[2])) * RAD_To_MIN;
                    }

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [0. Probe TY1, TY2 평균]
                    //probeZ[0] = TxTyZ[2];               //  저장 0
                    //***********************************//






                    //
                    // Hexapod
                    //
                    //lCalibrationData[16] = gageData[0] * 1000; //ofx - XYTz[0]) * 1000;     // um
                    //lCalibrationData[17] = -gageData[1] * 1000; //ofy + XYTz[1]) * 1000;     // um
                    //lCalibrationData[18] = -gageData[2] * 1000; //TxTyZ[2] * 1000;   // um
                    //lCalibrationData[19] = -gageData[3] * Math.PI / 180; //xTyZ[0];           // TX   radian
                    //lCalibrationData[20] = gageData[4] * Math.PI / 180; //TxTyZ[1];          // TY   radian
                    //lCalibrationData[21] = -gageData[5] * Math.PI / 180; //XYTz[2];           // TZ   radian
                    //
                    //

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [1. X거리, Y거리에 따른 Z 보정 (최종)]
                    //probeZ[1] = compZ;               //  저장 1
                    //lCalibrationData[22] = m__G.mGageCounter.probeTY1[0];  // [0.첫 Probe 값] 
                    //lCalibrationData[23] = m__G.mGageCounter.probeTY2[0];
                    //lCalibrationData[24] = m__G.mGageCounter.probeTY1[1];  // [1.X, Y이동, Z회전에 따른 Glass 기울기에 대한 Probe 보정]
                    //lCalibrationData[25] = m__G.mGageCounter.probeTY2[1];
                    //lCalibrationData[26] = m__G.mGageCounter.probeTY1[2];  //  [2.Glass 두께에 따른 Probe보정]
                    //lCalibrationData[27] = m__G.mGageCounter.probeTY2[2];
                    //lCalibrationData[28] = probeZ[0];  // [0.Probe TY1, TY2 평균] 
                    //lCalibrationData[29] = probeZ[1];  // [1.X거리, Y거리에 따른 Z 보정(최종)]
                    //lCalibrationData[30] = m__G.mGageCounter.probeTX[0];
                    //lCalibrationData[31] = m__G.mGageCounter.probeTX[1];
                    //lCalibrationData[32] = m__G.mGageCounter.probeTX[2];
                    //***********************************//

                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + lCalibrationData[19].ToString("F1") + "\t"
                        + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1") + "\t";


                    //double[] motorCurpos = MotorCurPos6D();
                    //strtmp[ci] += $"{motorCurpos[0]:F1}\t{motorCurpos[1]:F1}\t{motorCurpos[2]:F1}\t{motorCurpos[3]:F1}\t{motorCurpos[4]:F1}\t{motorCurpos[5]:F1}";


                }

                //if (m__G.m_bPrismCS)
                //{
                //    lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[3], lCalibrationData[4], lCalibrationData[5], true);
                //    if (bGageOn)
                //    {
                //        lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[19], lCalibrationData[20], lCalibrationData[21], true, true);
                //        lProbePrismTXTYTZ[1] = lCalibrationData[19];


                //        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5") + "\t" +
                //                        lProbePrismTXTYTZ[0].ToString("F5") + "\t" + lProbePrismTXTYTZ[1].ToString("F5") + "\t" + lProbePrismTXTYTZ[2].ToString("F5");
                //    }
                //    else
                //    {
                //        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                //    }
                //}
            }

            //mCalibrationFullData[0] = lCalibrationData;
            if (ci == 0 && IsSave)
            {
                mCalibrationFullData.Add(lCalibrationData);
                mGageFullData.Add(gageData);
                mStdevTXTYTZ.Add(lStdevTXTYTZ);
                mPrismTXTYTZ.Add(lPrismTXTYTZ);

            }

            if (!m__G.m_bHideAllGraph)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();
                        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                        if (ltextBox[0].Text.Length > 10000)
                            ltextBox[0].Text = strtmp[0] + "\r\n";
                        else
                            ltextBox[0].Text += strtmp[0] + "\r\n";

                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

                    if (ltextBox[0].Text.Length > 10000)
                        ltextBox[0].Text = strtmp[0] + "\r\n";
                    else
                        ltextBox[0].Text += strtmp[0] + "\r\n";

                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void RemoteManualFindMark()
        {
            //m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            //m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(50);
            ManualFindMarks(0, false, false);

            Process.LEDs_All_On(0, false);
        }

        public void JHManualFindMarks(int Nth, bool IsShowResult = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 4;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            /// 다음 자화로 대체 필요
            // gageData = 현재 값 읽어오기;
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();
            SetDefaultMarkConfig(true);

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[23];

            for (int ci = 0; ci < numFMIcandidate; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                strtmp[ci] = "";
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);
                ProcessVisionData(mavNum + 1, 2);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;
                double sx = 0;
                double sy = 0;
                double sz = 0;
                double tx = 0;
                double ty = 0;
                double tz = 0;

                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    sx += m__G.oCam[0].mC_pX[findex] * umscale;
                    sy += m__G.oCam[0].mC_pY[findex] * umscale;
                    sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx += m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty += m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz += m__G.oCam[0].mC_pTZ[findex] * minscale;
                }
                m__G.oCam[0].mC_pX[0] = sx;
                m__G.oCam[0].mC_pY[0] = sy;
                m__G.oCam[0].mC_pZ[0] = sz;
                m__G.oCam[0].mC_pTX[0] = tx;
                m__G.oCam[0].mC_pTY[0] = ty;
                m__G.oCam[0].mC_pTZ[0] = tz;

                //  부호 원상복귀
                lCalibrationData[0] = sx;//-sx;
                lCalibrationData[1] = sy;//sy;
                lCalibrationData[2] = sz;//-sz;
                lCalibrationData[3] = tx;//tx;
                lCalibrationData[4] = ty;//-ty;
                lCalibrationData[5] = tz;//-tz;
                if (IsShowResult)
                    strtmp[ci] = Nth.ToString() + "\t"
                                + lCalibrationData[0].ToString("F2") + "\t"
                                + lCalibrationData[1].ToString("F2") + "\t"
                                + lCalibrationData[2].ToString("F2") + "\t"
                                + lCalibrationData[3].ToString("F2") + "\t"
                                + lCalibrationData[4].ToString("F2") + "\t"
                                + lCalibrationData[5].ToString("F2") + "\t";
                double[] xavg = new double[12];
                double[] yavg = new double[12];
                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                        xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                        yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                    }
                }
                int kk = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (xavg[i] == 0) continue;
                    if (IsShowResult)
                        strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                    lCalibrationData[6 + 2 * kk] = xavg[i];
                    lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                    kk++;
                }
                if (gageData != null && gageData.Length > 0)
                {
                    if (gageData.Length == 6)
                    {
                        // 자화 부호, 단위 맞춰야함.
                        lCalibrationData[16] = gageData[0];     // um
                        lCalibrationData[17] = -gageData[1];    // um
                        lCalibrationData[18] = -gageData[2];   // um
                        lCalibrationData[19] = -gageData[3];    // * Math.PI / 180; //xTyZ[0];           // TX   radian
                        lCalibrationData[20] = gageData[4]; // * Math.PI / 180; //TxTyZ[1];          // TY   radian
                        lCalibrationData[21] = -gageData[5];    // * Math.PI / 180; //XYTz[2];           // TZ   radian

                        if (IsShowResult)
                            strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (3437.7 * lCalibrationData[19]).ToString("F1") + "\t"
                                + (3437.7 * lCalibrationData[20]).ToString("F1") + "\t" + (3437.7 * lCalibrationData[21]).ToString("F1");
                    }
                }

                if (ci == 0)
                {
                    mCalibrationFullData.Add(lCalibrationData);
                    mGageFullData.Add(lCalibrationData);
                }
            }


            if (IsShowResult)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();

                        ltextBox[0].Text += strtmp[0] + "\r\n";
                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();

                    ltextBox[0].Text += strtmp[0] + "\r\n";
                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
                //if (InvokeRequired)
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        ltextBox[1].Text += strtmp[1] + "\r\n";
                //        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                //        ltextBox[1].ScrollToCaret();
                //    });
                //}
                //else
                //{
                //    ltextBox[1].Text += strtmp[1] + "\r\n";
                //    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                //    ltextBox[1].ScrollToCaret();
                //}
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void ManualFindMarks(int Nth, bool IsShowResult = true, bool fromFirst = false)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 2;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////
            /// 다음 HEXAPOD 로 대체 필요
            /// 
            //gageData = MotorCurLogicPos6D();

            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;
                if (m__G.m_bCalibrationModel)
                    gageData = m__G.mGageCounter.ReadPortAll(); //  gageData[6] = { X1,X2,Y1,Y2,TX,TY1,TY2 }
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            if (fromFirst)
            {
                for (int i = 0; i < mavNum + 1; i++)
                    m__G.oCam[0].GrabB(i, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지
            }
            else
            {
                for (int i = 0; i < mavNum; i++)
                    m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지
            }

            //string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            //m__G.oCam[0].SaveImageBuf(fname, -1);
            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();
            SetDefaultMarkConfig(true);

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[23];
            double[] lPrismTXTYTZ = new double[3];
            double[] lProbePrismTXTYTZ = new double[3];
            double[] lErrorPrismTXTYTZ = new double[3];
            bool bGageOn = false;

            for (int ci = 0; ci < numFMIcandidate; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                strtmp[ci] = "";
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);
                ProcessVisionData(mavNum + 1, 2);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;
                double sx = 0;
                double sy = 0;
                double sz = 0;
                double tx = 0;
                double ty = 0;
                double tz = 0;

                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    //NthMeasure(findex, true);

                    //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                    sx += m__G.oCam[0].mC_pX[findex] * umscale;
                    sy += m__G.oCam[0].mC_pY[findex] * umscale;
                    sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx += m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty += m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz += m__G.oCam[0].mC_pTZ[findex] * minscale;
                }
                m__G.oCam[0].mC_pX[0] = sx;
                m__G.oCam[0].mC_pY[0] = sy;
                m__G.oCam[0].mC_pZ[0] = sz;
                m__G.oCam[0].mC_pTX[0] = tx;
                m__G.oCam[0].mC_pTY[0] = ty;
                m__G.oCam[0].mC_pTZ[0] = tz;

                //  부호 원상복귀
                lCalibrationData[0] = sx;//-sx;
                lCalibrationData[1] = sy;//sy;
                lCalibrationData[2] = sz;//-sz;
                lCalibrationData[3] = tx;//tx;
                lCalibrationData[4] = ty;//-ty;
                lCalibrationData[5] = tz;//-tz;


                if (IsShowResult)
                    //strtmp[ci] = Nth.ToString() + "\t" + sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
                    strtmp[ci] = Nth.ToString() + "\t"
                                + lCalibrationData[0].ToString("F2") + "\t"
                                + lCalibrationData[1].ToString("F2") + "\t"
                                + lCalibrationData[2].ToString("F2") + "\t"
                                + lCalibrationData[3].ToString("F2") + "\t"
                                + lCalibrationData[4].ToString("F2") + "\t"
                                + lCalibrationData[5].ToString("F2") + "\t";

                double[] xavg = new double[12];
                double[] yavg = new double[12];
                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                        xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                        yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                    }
                }
                int kk = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (xavg[i] == 0) continue;
                    if (IsShowResult)
                        strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                    lCalibrationData[6 + 2 * kk] = xavg[i];
                    lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                    kk++;
                }
                if (gageData != null && gageData.Length > 0)
                {
                    if (gageData.Length == 7)
                    {
                        bGageOn = true;
                        //
                        //  Old Formula
                        //
                        //lCalibrationData[16] = -gageData[0]; //  X
                        //lCalibrationData[17] = gageData[2]; //  Y
                        //if (mbUseZ123)
                        //    lCalibrationData[18] = -(gageData[4] + gageData[5] + gageData[6]) / 3; //  Z
                        //else
                        //    lCalibrationData[18] = -(gageData[5] + gageData[6]) / 2; //  Z

                        //lCalibrationData[19] = -(gageData[4] - (gageData[5] + gageData[6]) / 2) / 55000; //  TX, gageData[3] is inversed, radian
                        //lCalibrationData[20] = -(gageData[5] - gageData[6]) / 110000; //  TY, radian
                        //lCalibrationData[21] = -(gageData[1] - gageData[0] - (gageData[3] - gageData[2])) / (80000 * 0.999325049); //  TZ, radian
                        //lCalibrationData[22] = gageData[3]; //  Y

                        //
                        //  New Formula
                        //
                        //  32mm : X probe offset from center along X axis
                        //  47mm : Y probe offset from center along Y axis
                        //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 40, m__G.oCam[0].mFAL.mFZM.mProbeXRx, m__G.oCam[0].mFAL.mFZM.mProbeYRy);    // 32.3 //32.306);
                        //double[] TxTyZ = m__G.oCam[0].mFAL.CalcTXTYZfromProbes(gageData[5] / 1000, gageData[6] / 1000, gageData[4] / 1000, XYTz[0], XYTz[1], XYTz[2]);
                        double[] XYTz = new double[3];
                        XYTz[0] = gageData[0];
                        XYTz[1] = gageData[1];
                        XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                        double[] TxTyZ = new double[3];
                        TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                        TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                        TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                        //double ofx = m__G.oCam[0].mFAL.mFZM.mProbeYRy - 32;   //  Fiducial Mark Position relative to Probe Position
                        //double ofy = m__G.oCam[0].mFAL.mFZM.mProbeXRx - 32;   //  Fiducial Mark Position relative to Probe Position


                        //lCalibrationData[16] = (ofx - XYTz[0]) * 1000; // um
                        //lCalibrationData[17] = (ofy + XYTz[1]) * 1000; // um
                        //lCalibrationData[18] = -TxTyZ[2] * 1000; // um
                        //lCalibrationData[19] = TxTyZ[0];       // TX   radian
                        //lCalibrationData[20] = -TxTyZ[1];       // TY   radian
                        //lCalibrationData[21] = XYTz[2];       // TZ   radian

                        //double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0], TxTyZ[1]);
                        double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);
                        lCalibrationData[16] = XYTz[0]; // um
                        lCalibrationData[17] = XYTz[1]; // um
                        lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                        //Point3d compRes = XYZcompensationAboutZPivots(new Point3d(XYTz[0], XYTz[1], TxTyZ[2]), TxTyZ[0], TxTyZ[1]);
                        //lCalibrationData[16] = compRes.X;//XYTz[0]; // um
                        //lCalibrationData[17] = compRes.Y;//XYTz[1]; // um
                        //lCalibrationData[18] = compRes.Z;// compZ;// TxTyZ[2]; // um
                        lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian
                        lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian
                        lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian  // 241216 TZ 부호변경
                    }

                    if (IsShowResult)
                        strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (lCalibrationData[19]).ToString("F1") + "\t"
                            + (lCalibrationData[20]).ToString("F1") + "\t" + (lCalibrationData[21]).ToString("F1");
                }


                if (IsShowResult && m__G.m_bPrismCS)
                {
                    if (bGageOn)
                    {
                        lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[19], lCalibrationData[20], lCalibrationData[21], true, true);
                        lProbePrismTXTYTZ[1] = lCalibrationData[19];

                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5") + "\t" +
                                        lProbePrismTXTYTZ[0].ToString("F5") + "\t" + lProbePrismTXTYTZ[1].ToString("F5") + "\t" + lProbePrismTXTYTZ[2].ToString("F5");
                    }
                    else
                    {
                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                    }
                }

                //
                //HexaPod Data
                //
                //
                //
                //lCalibrationData[16] = gageData[0] * 1000; //ofx - XYTz[0]) * 1000;     // um
                //lCalibrationData[17] = -gageData[1] * 1000; //ofy + XYTz[1]) * 1000;     // um
                //lCalibrationData[18] = -gageData[2] * 1000; //TxTyZ[2] * 1000;   // um
                //lCalibrationData[19] = -gageData[3] * Math.PI / 180; //xTyZ[0];           // TX   radian
                //lCalibrationData[20] = gageData[4] * Math.PI / 180; //TxTyZ[1];          // TY   radian
                //lCalibrationData[21] = -gageData[5] * Math.PI / 180; //XYTz[2];           // TZ   radian
                //
                //
                //if (IsShowResult)
                //        strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (3437.7 * lCalibrationData[19]).ToString("F1") + "\t"
                //            + (3437.7 * lCalibrationData[20]).ToString("F1") + "\t" + (3437.7 * lCalibrationData[21]).ToString("F1");


                if (ci == 0)
                {
                    mCalibrationFullData.Add(lCalibrationData);
                    mGageFullData.Add(lCalibrationData);
                    mPrismTXTYTZ.Add(lPrismTXTYTZ);

                }
            }


            if (IsShowResult)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();
                        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                        tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                        ltextBox[0].Text += strtmp[0] + "\r\n";
                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                    tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                    ltextBox[0].Text += strtmp[0] + "\r\n";
                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
                //if (InvokeRequired)
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        ltextBox[1].Text += strtmp[1] + "\r\n";
                //        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                //        ltextBox[1].ScrollToCaret();
                //    });
                //}
                //else
                //{
                //    ltextBox[1].Text += strtmp[1] + "\r\n";
                //    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                //    ltextBox[1].ScrollToCaret();
                //}
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }

        public void DrawMarkDetected(bool withID = false)
        {
            try
            {


                Mat tmpImg = new Mat();
                Mat lOverlayedImg = new Mat();
                Cv2.CvtColor(m__G.oCam[0].mFAL.mSourceImg[0], lOverlayedImg, ColorConversionCodes.GRAY2RGB);

                sFiducialMark lfMark = null;
                ref OpenCvSharp.Point[] fMarkPos = ref m__G.oCam[0].mDetectedMarkPos[0];
                int lMarkCount = fMarkPos.Length;
                int lModelScale = m__G.oCam[0].mFAL.mFidMarkTop[0].MScale;
                int lwidth = 0;
                int lheight = 0;
                OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();

                for (int i = 0; i < lMarkCount; i++)
                {
                    if (fMarkPos[i].X == 0 && fMarkPos[i].Y == 0)
                        continue;

                    if (i < 3)
                    {
                        lwidth = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Width;
                        lheight = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Height;
                    }
                    else
                    {
                        lwidth = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Width;
                        lheight = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Height;
                    }

                    lrc.X = fMarkPos[i].X * lModelScale;
                    lrc.Y = fMarkPos[i].Y * lModelScale;
                    lrc.Width = lModelScale * lwidth;
                    lrc.Height = lModelScale * lheight;

                    lOverlayedImg.Rectangle(lrc, Scalar.Cyan, 1);

                    if (m__G.oCam[0].mbDrawReference)
                    {
                        int x = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].X;
                        int y = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].Y;
                        Cv2.Line(lOverlayedImg, x - 10, y, x + 10, y, Scalar.OrangeRed, 1, LineTypes.Link4);
                        Cv2.Line(lOverlayedImg, x, y - 10, x, y + 10, Scalar.OrangeRed, 1, LineTypes.Link4);
                    }
                }
                if (withID)
                {
                    // ID Mark Position
                    lrc.X = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].X * m__G.oCam[0].mFAL.mModelScale + 38 + 13);
                    lrc.Y = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].Y * m__G.oCam[0].mFAL.mModelScale + 3);
                    lrc.Width = 39;
                    lrc.Height = 36;// (int)(180 - (m__G.oCam[0].mDetectedMarkPos[0][0].Y + m__G.oCam[0].mDetectedMarkPos[0][1].Y) * m__G.oCam[0].mFAL.mModelScale / 2 - 10);
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                    lrc.X += 49;
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                    lrc.X += 49;
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                    lrc.X += 49;
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                }


                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(lOverlayedImg);
                pictureBox2.Image = myImage;
            }
            catch { }
        }

        public void DrawMarkDetectedWithDummyShift(int dummyX, int dummyY, bool withID = false)
        {
            Mat tmpImg = new Mat();
            Mat lOverlayedImg = new Mat();
            Cv2.CvtColor(m__G.oCam[0].mFAL.mSourceImg[0], lOverlayedImg, ColorConversionCodes.GRAY2RGB);

            sFiducialMark lfMark = null;
            ref OpenCvSharp.Point[] fMarkPos = ref m__G.oCam[0].mDetectedMarkPos[0];
            int lMarkCount = fMarkPos.Length;
            int lModelScale = m__G.oCam[0].mFAL.mFidMarkTop[0].MScale;
            int lwidth = 0;
            int lheight = 0;
            OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();

            for (int i = 0; i < lMarkCount; i++)
            {
                if (fMarkPos[i].X == 0 && fMarkPos[i].Y == 0)
                    continue;

                if (i < 3)
                {
                    lwidth = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Width;
                    lheight = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Height;
                }
                else
                {
                    lwidth = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Width;
                    lheight = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Height;
                }

                lrc.X = fMarkPos[i].X * lModelScale;
                lrc.Y = fMarkPos[i].Y * lModelScale;
                lrc.Width = lModelScale * lwidth;
                lrc.Height = lModelScale * lheight;

                //lOverlayedImg.Rectangle(lrc, Scalar.Cyan, 1);

                if (m__G.oCam[0].mbDrawReference)
                {
                    int x = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].X;
                    int y = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].Y;
                    Cv2.Line(lOverlayedImg, x - 10, y, x + 10, y, Scalar.OrangeRed, 1, LineTypes.Link4);
                    Cv2.Line(lOverlayedImg, x, y - 10, x, y + 10, Scalar.OrangeRed, 1, LineTypes.Link4);
                }
                Cv2.Line(lOverlayedImg, dummyX - 8, dummyY, dummyX + 8, dummyY, Scalar.Lime, 1, LineTypes.Link4);
                Cv2.Line(lOverlayedImg, dummyX, dummyY - 8, dummyX, dummyY + 8, Scalar.Lime, 1, LineTypes.Link4);
            }
            if (withID)
            {
                // ID Mark Position
                lrc.X = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].X * m__G.oCam[0].mFAL.mModelScale + 39 + 13);
                lrc.Y = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].Y * m__G.oCam[0].mFAL.mModelScale + 5);
                lrc.Width = 39;
                lrc.Height = (int)(180 - (m__G.oCam[0].mDetectedMarkPos[0][0].Y + m__G.oCam[0].mDetectedMarkPos[0][1].Y) * m__G.oCam[0].mFAL.mModelScale / 2 - 10);
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                lrc.X += 49;
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                lrc.X += 49;
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                lrc.X += 49;
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
            }


            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(lOverlayedImg);
            pictureBox2.Image = myImage;
        }

        bool bThreadManualFindMarks = false;
        bool bFinishThreadManualFindMarks = true;

        private void btnFindMarks_Click(object sender, EventArgs e)
        {
            GrabToFindMark();
        }

        public void GrabToFindMark(bool isContinue = true)
        {
            if (isContinue)
            {
                if (!bThreadManualFindMarks)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnLive2.Enabled = false;
                            button10.Enabled = false;
                        });
                    }
                    else
                    {
                        btnLive2.Enabled = false;
                        button10.Enabled = false;
                    }
                    m__G.oCam[0].HaltA();
                    bHaltLive = true;
                    IsLiveCropStop = true;
                    bThreadManualFindMarks = true;
                    mCalibrationFullData = new List<double[]>();

                    //if (m__G.mGageCounter != null)
                    //    m__G.mGageCounter.OpenAllport();  // 250210 주석처리

                    Task.Run(() => ThreadManualFindMarks(1000));
                }
                else
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnLive2.Enabled = true;
                            button10.Enabled = true;
                        });
                    }
                    else
                    {
                        btnLive2.Enabled = true;
                        button10.Enabled = true;
                    }

                    bThreadManualFindMarks = false;

                    while (!bFinishThreadManualFindMarks)
                        Thread.Sleep(100);

                    //if (m__G.mGageCounter != null)
                    //    m__G.mGageCounter.CloseAllport();// 250210 주석처리

                    btnFindMarks.Text = "Grab to Find Marks";
                }
            }
            else
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = false;
                        button10.Enabled = false;
                    });
                }
                else
                {
                    btnLive2.Enabled = false;
                    button10.Enabled = false;
                }


                m__G.oCam[0].HaltA();
                bHaltLive = true;
                IsLiveCropStop = true;
                bThreadManualFindMarks = true;
                mCalibrationFullData = new List<double[]>();
                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

                //m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                //m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));

                Process.LEDs_All_On(0 , true);

                ManualFindMarks(0, true, false);

                Process.LEDs_All_On(0, false);
                Thread.Sleep(500);

                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = true;
                        button10.Enabled = true;
                    });
                }
                else
                {
                    btnLive2.Enabled = true;
                    button10.Enabled = true;
                }

                bThreadManualFindMarks = false;

                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

                btnFindMarks.Text = "Grab to Find Marks";
            }
        }
        public int mAutoCalibrationIndex = 0;
        public void PrepareRemoteCalibration()
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  mAutoCalibrationIndex 에 data index 가 들어감.
            //  원격 Calibration 하려는 경우 본 함수 가장 먼저 호출 한 뒤, 매 이동 직후 SingleFindMark() 호출.
            //  마지막 SingleFindMark() 호출 뒤 RemoteCalibration() 호출
            mCalibrationFullData = new List<double[]>();
            mAutoCalibrationIndex = 0;
        }

        //public void RemoteCalibration(string strAxis, int skipCount)
        //{
        //    //  strAxis 은 "Z", "Y', "X", "TX", "TY" 를 넣을 수 있다.
        //    //  Calibraition 순서는 "Z" -> "Z"  -> "Y" -> "Y" -> "X" -> "X" -> "TX" -> "TY"
        //    //  각 보정결과를 확인해야 하므로 보전 전 - 후 로 실시한다.

        //    //  ORG 위치에서 한쪽 끝으로 이동하면서 얻어진 데이터( skipCount )는 삭제한다.
        //    for (int i = 0; i < skipCount; i++)
        //        mCalibrationFullData.RemoveAt(0);

        //    //JH_SK_CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
        //    CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
        //}

        public void SingleFindMark(bool IsSave = true)
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  SingleFindMark is used for External Calibration
            Process.LEDs_All_On(0, true);
            Thread.Sleep(50);   //  Wait LED Power Up.
            if (mAutoCalibrationIndex == 0)
                MotorizedFindMarks(mAutoCalibrationIndex, true, IsSave);
            else
                MotorizedFindMarks(mAutoCalibrationIndex, false, IsSave);

            mAutoCalibrationIndex++;
            Process.LEDs_All_On(0, false);
        }
        public double[] AnosisCalData = new double[22]; // 33
        public void SingleFindMarkAnosis(bool IsSave = true)
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  SingleFindMark is used for External Calibration
            Process.LEDs_All_On(0, true);
            Thread.Sleep(100);   //  Wait LED Power Up.

            if (mAutoCalibrationIndex == 0)
                MotorizedFindMarksAnosis(ref AnosisCalData, mAutoCalibrationIndex, true, IsSave);
            else
                MotorizedFindMarksAnosis(ref AnosisCalData, mAutoCalibrationIndex, false, IsSave);

            mAutoCalibrationIndex++;
            Process.LEDs_All_On(0, false);
        }

        public void ThreadManualFindMarks(int maxCnt = 500)
        {
            bFinishThreadManualFindMarks = false;

            for (int i = 0; i < maxCnt; i++)
            {
                Process.LEDs_All_On(0, true);
                ManualFindMarks(i, true, true);

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////   임시
                //string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab" + i.ToString() + "_1.bmp";
                //m__G.oCam[0].SaveGrabbedImage(1, fileName);
                //fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab" + i.ToString() + "_2.bmp";
                //m__G.oCam[0].SaveGrabbedImage(2, fileName);
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                Process.LEDs_All_On(0, false);
                Thread.Sleep(500);
                if (!bThreadManualFindMarks)
                    break;
            }

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnFindMarks.Text = "Grab to Find Marks";
                });
            }
            else
            {
                btnFindMarks.Text = "Grab to Find Marks";
            }

            bThreadManualFindMarks = false;
            bFinishThreadManualFindMarks = true;
        }
        public string RemoteGrab()
        {
            DrawMarkPositions();

            m__G.oCam[0].GrabB(0);

            string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            m__G.oCam[0].SaveGrabbedImage(0, fname);
            //string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            //m__G.oCam[0].SaveImageBuf(fname);
            return fname;
        }
        public void FindMarks(int index = 1)
        {
            string strtmp = "";
            m__G.oCam[0].PrepareFineCOG();

            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            m__G.oCam[0].FineCOG(true, index, 0, true);

            if (index == 0)
                return;

            FAutoLearn.FZMath.Point2D[] mpts = new FAutoLearn.FZMath.Point2D[5];

            mpts[0] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][0].X, m__G.oCam[0].mAzimuthPts[index][0].Y);

            mpts[1] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][4].X, m__G.oCam[0].mAzimuthPts[index][4].Y);

            mpts[2] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][6].X, m__G.oCam[0].mAzimuthPts[index][6].Y);

            mpts[3] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][8].X, m__G.oCam[0].mAzimuthPts[index][8].Y);

            mpts[4] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][10].X, m__G.oCam[0].mAzimuthPts[index][10].Y);

            m__G.oCam[0].PointTo6DMotion(index, mpts);

            double lx, ly, lz;
            double ltx, lty, ltz;
            lx = m__G.oCam[0].mC_pX[index] * 5.5 / Global.LensMag;    //  Pixel to um
            ly = m__G.oCam[0].mC_pY[index] * 5.5 / Global.LensMag;    //  Pixel to um
            lz = m__G.oCam[0].mC_pZ[index] * 5.5 / Global.LensMag;    //  Pixel to um ////////////////////////////////////////// ZLUT 적용 검토

            ltx = m__G.oCam[0].mC_pTX[index] * 180 * 60 / Math.PI;    //  radian to min
            lty = m__G.oCam[0].mC_pTY[index] * 180 * 60 / Math.PI;    //  radian to min
            ltz = m__G.oCam[0].mC_pTZ[index] * 180 * 60 / Math.PI;    //  radian to min

            strtmp += lx.ToString("F2") + "\t" + ly.ToString("F2") + "\t" + lz.ToString("F2") + "\t| " + ltx.ToString("F2") + "\t" + lty.ToString("F2") + "\t" + ltz.ToString("F2") + "\t| ";


            double[] lCalibrationData = new double[23];

            lCalibrationData[0] = lx;
            lCalibrationData[1] = ly;
            lCalibrationData[2] = lz;
            lCalibrationData[3] = ltx;
            lCalibrationData[4] = lty;
            lCalibrationData[5] = ltz;

            mCalibrationFullData.Add(lCalibrationData);



            //for (int i = 0; i < 12; i++)
            //{
            //    if (m__G.oCam[0].mAzimuthPts[1][i].X == 0) continue;
            //    strtmp += m__G.oCam[0].mAzimuthPts[1][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[1][i].Y.ToString("F3") + "\t";
            //}
            double XfromNtoS = Math.Abs(m__G.oCam[0].mAzimuthPts[index][8].X - m__G.oCam[0].mAzimuthPts[index][10].X);
            double YfromNStoE = Math.Abs((m__G.oCam[0].mAzimuthPts[index][0].Y + m__G.oCam[0].mAzimuthPts[index][4].Y) / 2 - m__G.oCam[0].mAzimuthPts[index][6].Y);
            double YfromSideNStoTopNS = Math.Abs((m__G.oCam[0].mAzimuthPts[index][0].Y + m__G.oCam[0].mAzimuthPts[index][4].Y) / 2 - (m__G.oCam[0].mAzimuthPts[index][8].Y + m__G.oCam[0].mAzimuthPts[index][10].Y) / 2);

            if (lz > 0)
                strtmp += "X N-S\t" + XfromNtoS.ToString("F3") + "\tY NS-E\t" + YfromNStoE.ToString("F3") + "\tMove close by " + lz.ToString("F0") + "um\r\n";
            else
                strtmp += "X N-S\t" + XfromNtoS.ToString("F3") + "\tY NS-E\t" + YfromNStoE.ToString("F3") + "\tMove away by " + (-lz).ToString("F0") + "um\r\n";

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += strtmp;
                    tbInfo.SelectionStart = tbInfo.Text.Length;
                    tbInfo.ScrollToCaret();
                });
            }
            else
            {
                tbInfo.Text += strtmp;
                tbInfo.SelectionStart = tbInfo.Text.Length;
                tbInfo.ScrollToCaret();
            }
        }

        public void SetDefaultMarkConfig(bool IsDraw = false)
        {
            System.Drawing.Point[] markPos = null;

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);        //  CropGap 이 적용되지 않은 상태의 결과를 반환한다.
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);   //  CropGap 이 적용되지 않은 상태의 데이터
            m__G.mFAL.SetMarkNorm();                                //  CropGap 이 적용되지 않은 상태의 데이터
            //if (IsDraw)
            //{
            //    m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
            //    m__G.oCam[0].DrawCSHCross(Brushes.Red);
            //}
        }

        private void btnLoadBmpFindMark_Click(object sender, EventArgs e)
        {
            m__G.mFAL.mCandidateIndex = 0;
            m__G.oCam[0].DrawClear();
            string sFilePath = Path.GetFullPath(m__G.m_RootDirectory + "\\Result\\RawData");
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "bmp";
            openFile.InitialDirectory = sFilePath;
            openFile.Multiselect = true;

            openFile.Filter = "BMP(*.bmp)|*.bmp";
            if (openFile.ShowDialog() != DialogResult.OK)
                return;

            m__G.oCam[0].mFAL.ClearCommonImgFile();
            //  영상들의 처리에 앞서서 반드시 들어가야 한다.
            //  Mark 가 업데이트 되면 반드시 수행, 영상크기가 확정되어야 한다.
            int findex = 0;
            System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
                new System.Drawing.Point( 730, 78 ),
                new System.Drawing.Point( 234, 93 ),
                new System.Drawing.Point( 730, 255 ),
                new System.Drawing.Point( 234, 275 ),
                new System.Drawing.Point( 439, 294 ),
                new System.Drawing.Point( 532, 294 ) };

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].ForceTriggerTime();

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
            m__G.mFAL.SetMarkNorm();
            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            if (tbMaxThread.Text.Length > 0)
                m__G.mMaxThread = int.Parse(tbMaxThread.Text);

            if (tbBreakIndex.Text.Length > 0)
                m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[5000];

            if (openFile.FileNames.Length == 1)
            {
                m__G.oCam[0].mFAL.LoadBMPtoBufN(openFile.FileNames[0], 0);
                m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

                int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
                for (int ci = 0; ci < numFMIcandidate; ci++)
                {
                    m__G.mFAL.mCandidateIndex = ci;
                    ChangeFiducialMark(ci);

                    m__G.oCam[0].mFAL.mbGetHistogram = true;
                    m__G.oCam[0].FineCOG(true, 0, 0, false, true, false, true);
                    m__G.oCam[0].mFAL.mbGetHistogram = false;

                    string strtmp = openFile.FileNames[0] + ">> \r\n";

                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[0][i].X == 0) continue;
                        strtmp += m__G.oCam[0].mAzimuthPts[0][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[0][i].Y.ToString("F3") + "\t";
                    }
                    strtmp += "\r\nContrast\t";
                    for (int i = 0; i < 5; i++)
                        strtmp += m__G.oCam[0].mFAL.mEffectiveContrast[i].ToString() + "\t";

                    tbVsnLog.Text += strtmp + "( > 20 )\r\n";
                }
                // ScaleNOpticalRotation();
            }
            else
            {
                long startTime = 0;
                long endTime = 0;
                SupremeTimer.QueryPerformanceFrequency(ref m__G.TimerFrequency);
                int i = 0;
                m__G.oCam[0].mTrgBufLength = openFile.FileNames.Length;
                foreach (string filename in openFile.FileNames)
                {
                    //m__G.oCam[0].LoadBMPtoBufN(filename, i++);
                    m__G.oCam[0].mFAL.LoadBMPtoBufN(filename, i++);
                }
                m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

                int numFile = i;

                SupremeTimer.QueryPerformanceCounter(ref startTime);
                string strtmp = "";

                int maxThread = m__G.mMaxThread;
                if (numFile < 20)
                    maxThread = 1;
                //else if (numFile < 26)
                //    maxThread = numFile/2;

                int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
                strtmp = "";
                for (int ci = 0; ci < numFMIcandidate; ci++)
                {
                    //////////////////////////////////////////////////////////////
                    /////   모델 2개 추적하기위한 모델 변경 관련 코드
                    //////////////////////////////////////////////////////////////
                    m__G.mFAL.mCandidateIndex = ci;
                    if (ci == 1)
                    {
                        m__G.mFAL.GetMarkPosOnPanel(out markPos);
                        m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
                        m__G.mFAL.SetMarkNorm();
                    }
                    else if (ci == 0)
                        m__G.mFAL.SetDefaultMarkNorm();

                    //////////////////////////////////////////////////////////////
                    /////   아래로는 공통
                    //////////////////////////////////////////////////////////////
                    m__G.mbSuddenStop[0] = false;
                    int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
                    m__G.oCam[0].mTargetTriggerCount = numFile;
                    //m__G.oCam[0].mTrgBufLength = 3000;
                    m__G.oCam[0].SetTriggeredframeCount(numFile);



                    ProcessVisionData(numFile, maxThread, true);



                    m__G.mbSuddenStop[0] = false;
                    m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;

                    double umscale = 5.5 / Global.LensMag;                           //  rad to min
                    double minscale = 180 / Math.PI * 60;                           //  rad to min

                    for (int fileCnt = 0; fileCnt < numFile; fileCnt++)
                    {
                        strtmp += fileCnt.ToString() + "\t" + (umscale * m__G.oCam[0].mC_pX[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mC_pY[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mC_pZ[fileCnt]).ToString("F2")
                             + "\t" + (minscale * m__G.oCam[0].mC_pTX[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mC_pTY[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mC_pTZ[fileCnt]).ToString("F2") + "\t";
                        for (i = 0; i < 12; i++)
                        {
                            if (m__G.oCam[0].mAzimuthPts[fileCnt][i].X == 0) continue;
                            strtmp += m__G.oCam[0].mAzimuthPts[fileCnt][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[fileCnt][i].Y.ToString("F3") + "\t";
                        }
                        strtmp += "\r\n";
                    }
                    strtmp += "\r\n";
                    //////////////////////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////

                }

                m__G.mFAL.SetDefaultMarkNorm();
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();


                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ellapse = 1000 * (endTime - startTime) / (double)(m__G.TimerFrequency);
                tbVsnLog.Text += strtmp;
                tbVsnLog.Text += "\r\nEllapsed Time : " + ellapse.ToString("F3") + " msec for processing " + numFile.ToString() + " Frames, LED : " + mLEDcurrent[0].ToString("F2") + " " + mLEDcurrent[1].ToString("F2") + "\r\n";

                mTriggerGrabbedFrame = numFile;
                MyOwner.WriteResultBin();
                //MyOwner.WriteResult();

            }
            //  Default Mark Position
            DrawMarkDetected();
            m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
            tbVsnLog.Text += "Finish\r\n";
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();
        }

        //private string ParallelFindMark(int istart, int iend, int iBuf)
        //{
        //    //for (int findex = istart; findex < iend; findex++)
        //    //    m__G.oCam[0].FineCOG(findex, iBuf);

        //    m__G.oCam[0].FineCOG(true, istart, iBuf);
        //    for (int findex = istart + 1; findex < iend; findex++)
        //        m__G.oCam[0].FineCOG(false, findex, iBuf);

        //    return iend.ToString();
        //}


        public void CalcVisionData(int cam, int ci, int ce, int cstep, int iBuf, bool IsFile = false)
        {
            int i = 0;
            mb_FinishCalcVision[iBuf] = false;
            try
            {
                bool res = false;
                int si = ci;
                int se = ce;
                int sstep = cstep;
                if (ci > ce)
                {
                    for (i = ci; i >= ce; i -= cstep)    //  Skip 0 ~ 59
                    {
                        if (m__G.mbSuddenStop[0])   //  연산도 중단함.
                            break;
                        try
                        {
                            res = m__G.oCam[cam].FineCOG(false, i, iBuf, false, true, false, IsFile);    // 마크찾기
                        }
                        catch (Exception ex)
                        {
                            ;
                        }
                        if (res)
                            mDebugCalcVisionCount[iBuf]++;
                    }
                }
                else
                {
                    for (i = ci; i < ce; i += cstep)    //  Skip 0 ~ 59
                    {
                        if (i == 0)
                            continue;

                        if (m__G.mbSuddenStop[0])   //  연산도 중단함.
                            break;

                        res = m__G.oCam[cam].FineCOG(false, i, iBuf, false, true, false, IsFile);    // 마크찾기
                        if (res)
                            mDebugCalcVisionCount[iBuf]++;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                //m__G.fManage.AddViewLog(ex.ToString() + "\r\n");
            }
            mb_FinishCalcVision[iBuf] = true;
        }

      
     

        private void btnBack_Click(object sender, EventArgs e)
        {
            //m__G.fManage.ManualToPlot();
            if (!bHaltLive)
                GrabHalt();

            //cbContinuosMode.Checked = false;
            cbSaveNthImg.Checked = false;
            IsLiveCropStop = true;
            bThreadManualFindMarks = false;

            //this.Hide();
            //if (MyOwner.m_bAdmin)
            //{
            //    MyOwner.ShowAdminMode();
            //}
            //else
            //{
            //    MyOwner.ShowOperatorMode();
            //}
            BufferInit();
            STATIC.State = (int)STATIC.STATE.Manage;
        }

        private void rbLED1_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLED1.Checked)
            {
                m_FocusedLED = 1;
            }
        }

        private void rbLED2_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLED2.Checked)
            {
                m_FocusedLED = 0;

            }

        }
        public int saveCount = 0;
        private void btnAllLEDOn_Click(object sender, EventArgs e)
        {
            if (!m_bAllLEDOn)
            {
                m__G.mDoingStatus = "Checking Vision";

                Process.LEDs_All_On(0, true);

                m_bAllLEDOn = true;

                btnAllLEDOn.ForeColor = Color.White;
            }
            else
            {
                Process.LEDs_All_On(0, false);

                m_bAllLEDOn = false;
                btnAllLEDOn.ForeColor = Color.SlateGray;
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (tbImgNumber.Text == "")
            {
                MessageBox.Show("Please input the image number!");
                return;
            }

            m__G.oCam[0].mFAL.LoadFMICandidate();

            int imgIndex = Convert.ToInt16(tbImgNumber.Text);

            if (imgIndex == 0)
            {
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                m__G.oCam[0].mFAL.BackupFMI();
            }
            m__G.mFAL.mCandidateIndex = 0;
            ChangeFiducialMark(0);

            pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].LoadCropMat(imgIndex));

            string strtmp = NthMeasure(imgIndex, ScanName);

            DrawMarkDetected(true);

            strtmp += "\r\n" + imgIndex.ToString() + "\t";
            for (int i = 0; i < 12; i++)
            {
                if (m__G.oCam[0].mAzimuthPts[imgIndex][i].X == 0) continue;
                strtmp += m__G.oCam[0].mAzimuthPts[imgIndex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[imgIndex][i].Y.ToString("F3") + "\t";
            }
            strtmp += ">\t";
            for (int i = 0; i < 5; i++)
                strtmp += m__G.oCam[0].m_sMRinstant[i].mMTF.ToString("F0") + "\t";

            tbInfo.Text += strtmp + "\r\n";/*+ m__G.oCam[0].mGrabAbsTiming[imgIndex].ToString("F5")*/

            tbInfo.SelectionStart = tbInfo.Text.Length;
            tbInfo.ScrollToCaret();

            int nextFrame = 0;
            if (rb1step.Checked)
                nextFrame = imgIndex + 1;
            else
                nextFrame = imgIndex + 5;
            //if (nextFrame < 0)
            //    nextFrame = 0;

            tbImgNumber.Text = nextFrame.ToString();
        }

        public string NthMeasure(int imgIndex, string name, bool bAccu = false, bool isShow = false)
        {
            //if ((m__G.fGraph.mAF_FrameCount-1) < imgIndex) return;

            double[] sYaw = new double[4];
            double[] sPitch = new double[4];
            double[] cx = new double[8];
            double[] cy = new double[8];

            double[] strokeL = new double[2];
            double[] yawL = new double[2];
            double[] pitchL = new double[2];

            double[] strokeR = new double[2];
            double[] yawR = new double[2];
            double[] pitchR = new double[2];
            System.Drawing.Point lptLT = new System.Drawing.Point(0, 0);
            System.Drawing.Point lptRB = new System.Drawing.Point(0, 0);

            //long nFound = 0;

            if (!bAccu)
            {
                m__G.oCam[0].DrawClear();
            }

            m__G.oCam[0].ReplayBuftoCommon(name, imgIndex);

            m__G.oCam[0].mTrgBufLength = m__G.oCam[0].mTargetTriggerCount;
            //tbVsnLog.Text += "Target Length = " + m__G.oCam[0].mTrgBufLength.ToString();
            m__G.oCam[0].DispCommonImage(imgIndex);

            if (!bAccu)
                tbVsnLog.Text = "";

            //tbVsnLog.Text += "Target Length = " + m__G.oCam[0].mTrgBufLength.ToString();

            m__G.oCam[0].mMatroxMsg = "";
            //m__G.oCam[0].DispCommonImage(imgIndex);


            //m__G.oCam[0].ResizeGrab(imgIndex);

            if (isShow)
            {
                string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\";
                if (!Directory.Exists(fileName))
                    Directory.CreateDirectory(fileName);

                //string compressedFileName = fileName + "c" + imgIndex.ToString() + ".bmp";
                fileName += "Ana" + imgIndex.ToString() + ".bmp";
                m__G.oCam[0].SaveGrabbedImage(imgIndex, fileName);
                //m__G.oCam[0].SaveCompressedImage(imgIndex, compressedFileName);
            }

            if (tbBreakIndex.Text.Length > 0)
                m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);

            //if (m__G.oCam[0].mFAL.mBreakIndex == imgIndex)
            //    IsShow = cbSaveNthImg.Checked;

            if (imgIndex == 0)
                m__G.oCam[0].FineCOG(true, imgIndex, 0, isShow, true, true);// ref sYaw, ref sPitch, ref cx, ref cy, ref nFound, cbSaveNthImg.Checked , 0, m__G.sModelName);   // 마크찾기
            else
                m__G.oCam[0].FineCOG(false, imgIndex, 0, isShow, true, true);// ref sYaw, ref sPitch, ref cx, ref cy, ref nFound, cbSaveNthImg.Checked , 0, m__G.sModelName);   // 마크찾기


            //for (int p = 0; p < 12; p++)
            //{
            //    //if (p == 4) strtmp += "\r\n";
            //    //strtmp += cx[p].ToString("F3") + "\t" + cy[p].ToString("F3") + " \t";
            //    if (m__G.oCam[0].mAzimuthPts[imgIndex][p].X < 1) continue;

            //    lptLT.X = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].X - 5);
            //    lptLT.Y = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].Y - 5);
            //    lptRB.X = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].X + 5);
            //    lptRB.Y = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].Y + 5);
            //    m__G.oCam[0].DrawDCBox(lptLT, lptRB, Brushes.Red);
            //}
            string strtmp = m__G.oCam[0].mMatroxMsg;

            return strtmp;
        }
        public void UptoMeasureTxTyTz(int imgIndex)
        {

            try
            {
                FindResult result = MeasureTxTyTz(imgIndex, ScanName, false, cbSaveNthImg.Checked);

                string strtmp = "";

                strtmp += "\r\n" + imgIndex.ToString() + "\t";
                for (int i = 0; i < 12; i++)
                {
                    if (m__G.oCam[0].mAzimuthPts[imgIndex][i].X == 0) continue;
                    strtmp += m__G.oCam[0].mAzimuthPts[imgIndex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[imgIndex][i].Y.ToString("F3") + "\t";
                }
                strtmp += ">\t";
                for (int i = 0; i < 5; i++)
                    strtmp += m__G.oCam[0].m_sMRinstant[i].mMTF.ToString("F0") + "\t";

                tbInfo.Text += strtmp + "\r\n";
            }
            catch
            {
                MessageBox.Show("Input Correct Image Number, Then Retry.");
                return;
            }
        }
        public static double mTZtoY1Y2 = 0.660316;
        public double umscale = 5.5 / Global.LensMag;                           //  rad to min
        public double minscale = 180 / Math.PI * 60;                           //  rad to min
        public string ScanName = "";
        public FindResult MeasureTxTyTz(int imgIndex, string name = "", bool bAccu = true, bool isShow = false)
        {

            m__G.oCam[0].mFAL.LoadFMICandidate();

            if (imgIndex == 0)
            {
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                m__G.oCam[0].mFAL.BackupFMI();
            }
            m__G.mFAL.mCandidateIndex = 0;
            ChangeFiducialMark(0);

            //if (name == "") m__G.oCam[0].ReplayBuftoCommon(ScanName, imgIndex);
            //else m__G.oCam[0].ReplayBuftoCommon(name, imgIndex);

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();

            NthMeasure(imgIndex, name, bAccu, isShow);

            FindResult result = new FindResult();
            if(Option.XYPosReverse)
            {
                result.cy[0] = m__G.oCam[0].mC_pX[imgIndex] * umscale;
                result.cx[0] = m__G.oCam[0].mC_pY[imgIndex] * umscale;
                result.cz[0] = m__G.oCam[0].mC_pZ[imgIndex] * umscale;
                result.ty[0] = m__G.oCam[0].mC_pTX[imgIndex] * minscale;
                result.tx[0] = m__G.oCam[0].mC_pTY[imgIndex] * minscale;
                result.tz[0] = m__G.oCam[0].mC_pTZ[imgIndex] * minscale;
                result.cy1[0] = result.cy[0] + result.tz[0] * mTZtoY1Y2;
                result.cy2[0] = result.cy[0] - result.tz[0] * mTZtoY1Y2;
            }
            else
            {
                result.cx[0] = m__G.oCam[0].mC_pX[imgIndex] * umscale;
                result.cy[0] = m__G.oCam[0].mC_pY[imgIndex] * umscale;
                result.cz[0] = m__G.oCam[0].mC_pZ[imgIndex] * umscale;
                result.tx[0] = m__G.oCam[0].mC_pTX[imgIndex] * minscale;
                result.ty[0] = m__G.oCam[0].mC_pTY[imgIndex] * minscale;
                result.tz[0] = m__G.oCam[0].mC_pTZ[imgIndex] * minscale;
                result.cy1[0] = result.cy[0] + result.tz[0] * mTZtoY1Y2;
                result.cy2[0] = result.cy[0] - result.tz[0] * mTZtoY1Y2;
            }

            if (Option.XDirReverse)
            {
                result.cx[0] *= -1;
                result.tx[0] *= -1;
            }
                
            if (Option.YDirReverse)
            {
                result.cy[0] *= -1;
                result.cy1[0] *= -1;
                result.cy2[0] *= -1;
                result.ty[0] *= -1;
            }
                
            if(Option.AFDirReverse)
            {
                result.cz[0] *= -1;
                result.tz[0] *= -1;
            }
               

            return result;
        }
 
        private void radioButton10Step_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton10Step.Checked)
                m_FovStep = 10;

        }

        private void radioButton1Step_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1Step.Checked)
                m_FovStep = 1;

        }

        private void cbSaveNthImg_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnUptoNthMesure_Click(object sender, EventArgs e)
        {
            UptoNthMesure();
        }

        public void ChangeFiducialMark(int mID)
        {
            System.Drawing.Point[] markPos = null;

            if (mID != 0)
            {
                m__G.mFAL.GetMarkPosOnPanel(out markPos);
                m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
                m__G.mFAL.SetMarkNorm();
            }
            else
            {
                m__G.mFAL.SetDefaultMarkNorm();
            }
        }
        public void UptoNthMesure(int extfrmCnt = 0)
        {
            int imgIndex = 0;
            try
            {
                if (extfrmCnt == 0)
                    imgIndex = Convert.ToInt16(tbImgNumber.Text);
            }
            catch
            {
                MessageBox.Show("Input Correct Image Number, Then Retry.");
                return;
            }
            for(int i = 0; i < imgIndex; i++)
                m__G.oCam[0].ReplayBuftoCommon(ScanName, i);
            string strtmp = "";
            tbInfo.Text = "";
            if (tbMaxThread.Text.Length > 0)
                m__G.mMaxThread = int.Parse(tbMaxThread.Text);

            if (tbBreakIndex.Text.Length > 0)
                m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);

            if (cbCompatibility.Checked == true)
                m__G.oCam[0].mFAL.mCheckCompatibility = true;
            else
                m__G.oCam[0].mFAL.mCheckCompatibility = false;

            m__G.oCam[0].mFAL.LoadFMICandidate();

            m__G.oCam[0].mFAL.BackupFMI();
            //m__G.oCam[0].mFAL.mFastMode = m__G.m_bEulerRotation;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23

            long beginTime = 0;
            long endTime = 0;
            long lTimerFrequency = 0;
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            double sx = 0;
            double sy = 0;
            double sz = 0;
            double tx = 0;
            double ty = 0;
            double tz = 0;
            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double umscale = 5.5 / Global.LensMag;                           //  rad to min

            SetDefaultMarkConfig(false);

            int lmaxThread = m__G.mMaxThread;

            m__G.oCam[0].mTargetTriggerCount = imgIndex;

            int frmCnt = m__G.oCam[0].mTargetTriggerCount;

            //tbVsnLog.Text += "Target Trigger Count = " + frmCnt.ToString() + "\r\n";

            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].SetTriggeredframeCount(frmCnt);
            m__G.oCam[0].SetSaveLostMarkFrame(false);
            if (extfrmCnt == 0)
            {
                if (imgIndex > m__G.oCam[0].mTargetTriggerCount)
                    imgIndex = m__G.oCam[0].mTargetTriggerCount;
            }
            else
                imgIndex = frmCnt;


            bool IsShow = cbSaveNthImg.Checked;

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            //System.Drawing.Point[] markPos = null;
            strtmp = "";
            string[] strGrp = new string[2] { "conc", "Std" };
            for (int ci = 0; ci < numFMIcandidate; ci++)
            //for (int ci = 0; ci < 1; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                ///

                //Save 위치 변경

                for (int findex = 0; findex < imgIndex; findex++)
                {
                    if (IsShow)
                    {
                        string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\";
                        if (!Directory.Exists(fileName))
                            Directory.CreateDirectory(fileName);

                        fileName += "Ana" + findex.ToString() + ".bmp";
                        m__G.oCam[0].SaveGrabbedImage(findex, fileName);
                    }
                }

                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;

                SupremeTimer.QueryPerformanceCounter(ref beginTime);
                ProcessVisionData(imgIndex, lmaxThread);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ellapsedTime = (endTime - beginTime) / (double)(lTimerFrequency);

                strtmp = strGrp[ci % 2] + "#\tLed\tX\tY\tZ\tTX\tTY\tTZ\tX1\tY1\tX2\tY2\tX3\tY3\tX4\tY4\tX5\tY5";
                for (int findex = 0; findex < imgIndex; findex++)
                {
                    //if (IsShow)
                    //{
                    //    string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\";
                    //    if (!Directory.Exists(fileName))
                    //        Directory.CreateDirectory(fileName);

                    //    fileName += "Ana" + findex.ToString() + ".bmp";
                    //    m__G.oCam[0].SaveGrabbedImage(findex, fileName);
                    //}

                    //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";
                    strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mGrabAbsTiming[findex].ToString("F5") + "\t";

                    sx = m__G.oCam[0].mC_pX[findex] * umscale;
                    sy = m__G.oCam[0].mC_pY[findex] * umscale;
                    sz = m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx = m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty = m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz = m__G.oCam[0].mC_pTZ[findex] * minscale;
                    strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                        strtmp += m__G.oCam[0].mAzimuthPts[findex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[findex][i].Y.ToString("F3") + "\t";
                    }
                    if (findex % 100 == 99)
                    {
                        if (ci == 0)
                        {
                            tbInfo.Text += strtmp;
                        }
                        else
                        {
                            tbVsnLog.Text += strtmp;
                        }
                        strtmp = "";
                    }
                }
                tbInfo.Text += strtmp + "\r\n" + ellapsedTime.ToString("F3") + "sec";
            }

            m__G.mFAL.mCandidateIndex = 0;
            ChangeFiducialMark(0);
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();

            tbInfo.SelectionStart = tbInfo.Text.Length;
            tbInfo.ScrollToCaret();
            //tbImgNumber.Text = (imgIndex + 1).ToString();
        }

        public bool mDoneWriteRun = false;


        private void btnFOVUp_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVUp.BackgroundImage = global::FZ4P.Properties.Resources.ArrU2;
        }

        private void btnFOVUp_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVUp.BackgroundImage = global::FZ4P.Properties.Resources.ArrU;
          
        }

        private void btnFOVLeft_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::FZ4P.Properties.Resources.ArrL2;
          
        }

        private void btnFOVLeft_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::FZ4P.Properties.Resources.ArrL;
          
        }

        private void btnFOVDown_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::FZ4P.Properties.Resources.ArrD2;

          
        }

        private void btnFOVDown_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::FZ4P.Properties.Resources.ArrD;
          
        }

        private void btnFOVRight_MouseDown(object sender, MouseEventArgs e)
        {

            btnFOVRight.BackgroundImage = global::FZ4P.Properties.Resources.ArrR2;

          
        }

        private void btnFOVRight_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVRight.BackgroundImage = global::FZ4P.Properties.Resources.ArrR;
            
        }

      
        private void button8_Click(object sender, EventArgs e)
        {
            if (tbImgNumber.Text == "")
            {
                MessageBox.Show("Please input the image number!");
                return;
            }

            m__G.oCam[0].mFAL.LoadFMICandidate();

            int imgIndex = Convert.ToInt16(tbImgNumber.Text);

            if (imgIndex == 0)
            {
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                m__G.oCam[0].mFAL.BackupFMI();
            }

            pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].LoadCropMat(imgIndex));

            string strtmp = NthMeasure(imgIndex, ScanName);

            DrawMarkDetected(true);

            strtmp += "\r\n" + imgIndex.ToString() + "\t";
            for (int i = 0; i < 12; i++)
            {
                if (m__G.oCam[0].mAzimuthPts[imgIndex][i].X == 0) continue;
                strtmp += m__G.oCam[0].mAzimuthPts[imgIndex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[imgIndex][i].Y.ToString("F3") + "\t";
            }

            tbInfo.Text += strtmp;

            tbInfo.SelectionStart = tbVsnLog.Text.Length;
            tbInfo.ScrollToCaret();

            int nextFrame = 0;
            if (rb1step.Checked)
                nextFrame = imgIndex - 1;
            else
                nextFrame = imgIndex - 5;
            if (nextFrame < 0)
                nextFrame = 0;

            tbImgNumber.Text = nextFrame.ToString();
        }
        private void btnMouseEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }
        private void btnMouseHover(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }
        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }
        private void btnMouseHover(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }

        private void btnAutoLearn_Click(object sender, EventArgs e)
        {
            if (m__G.mFAL == null)
            {
                return;
            }
            m__G.mFAL.Show();
            m__G.mFAL.BringToFront();
            m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //mFAL.Size = new Size(1920, 1045);
            m__G.mFAL.Location = new System.Drawing.Point(0, 0);

            Thread ThreadWaitFALConfirmed = new Thread(() => WaitFALConfirmed());
            ThreadWaitFALConfirmed.Start();

        }
        private void WaitFALConfirmed()
        {
            m__G.mDoingStatus = "Model Configuration";

            while (true)
            {
                Thread.Sleep(50);
                if (m__G.mFAL.mbConfirmed)
                    break;
            }
            m__G.mFAL.mbConfirmed = false;

            m__G.oCam[0].ResetModelScale(1.0 / m__G.mFAL.mModelScale);
            m__G.mDoingStatus = "Checking Vision";
        }
  
     

   
        private void tbVsnLog_TextChanged(object sender, EventArgs e)
        {

        }

       

        bool mStopMonitorMTF = false;
  
        double[][] mSeriesMTF = new double[12][];

       
        private void button4_Click_1(object sender, EventArgs e)
        {

            ScaleNOpticalRotation();

        }
        public void ScaleNOpticalRotation()
        {

            double scaleTop = 0;
            double scaleSide = 0;
            double rotTop = 0;
            double rotSide = 0;

            if (!m__G.oCam[0].FineCOG(true, 0, 0))
            {
                tbVsnLog.Text += "fail to detect marks.\r\n";
            }
            else
            {
                //  획득된 마크 좌표와, 마크모델에 저장되어있는 실측마크좌표를 이용해 Scale 을 구하고 \\DoNotTouch\\ScaleNOpticalR.txt 에 저장한다.
                m__G.oCam[0].FindScaleNOpticalRotation(0, m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNOpticalR.txt");
                m__G.oCam[0].GetScaleNOpticalR(ref scaleTop, ref scaleSide, ref rotTop, ref rotSide);
                //  Master Mockup 의 설계치 기준으로 Scale 및 광학계 회전각도를 계산한다.

                string lstr = "";
                lstr += "Scale Top\t" + scaleTop.ToString("F4") + "\r\n";
                lstr += "Scale Side\t" + scaleSide.ToString("F4") + "\r\n";
                lstr += "Rotation Top\t" + rotTop.ToString("F4") + "\r\n";
                lstr += "Rotation Side\t" + rotSide.ToString("F4") + "\r\n";

                tbVsnLog.Text += lstr;
            }
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();
        }

        bool[] mb_FinishCalcVision = new bool[30];
        public int[] mDebugCalcVisionCount = new int[30];
        public double ProcessVisionData(int count, int HowManyThread = 10 /* Max 16 */, bool IsFile = false)
        {
            if (m__G.mbSuddenStop[0])   //  연산 시작도 안함.
            {
                m__G.oCam[0].mFinishVisionData = true;  //  맞다
                return 0;
            }

            m__G.oCam[0].mFinishVisionData = false;
            double ltime = 0;
            long lTimerFrequency = 1000;
            long startTime = 0;
            long endTime = 0;
            SupremeTimer.QueryPerformanceCounter(ref startTime);

            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.


            m__G.oCam[0].FineCOG(true, 0, 0, false, true, false, IsFile);    // 마크찾기
            //m__G.fManage.AddViewLog(string.Format("FineCOG 0\r\n"));
            if (count < HowManyThread)
            {
                mb_FinishCalcVision[0] = false;
                mDebugCalcVisionCount[0] = 0;
                m__G.oCam[0].FineCOG(true, 0, 0, false, true, false, IsFile);    // 마크찾기
                if (count > 1)
                {
                    CalcVisionData(0, 0, count, 1, 0, IsFile);
                }
                mb_FinishCalcVision[0] = true;
                m__G.oCam[0].mFinishVisionData = true;    //  맞다
                SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                ltime = (endTime - startTime) / (double)(lTimerFrequency);
                return ltime;
            }


            Task[] Task_CalcVisionData = new Task[HowManyThread];

            //int halfCnt = count / 2;
            //int lastIndx = count - 1;
            //int halfThread = HowManyThread / 2;
            //HowManyThread = halfThread * 2;

            List<int> taskIndices = new List<int>();
            List<int> lastImgIndex = new List<int>();
            for (int i = 0; i < HowManyThread; i++)
            {
                mb_FinishCalcVision[i] = false;
                mDebugCalcVisionCount[i] = 0;
                //mb_FinishCalcVision[halfThread + i] = false;
                //mDebugCalcVisionCount[halfThread + i] = 0;

                taskIndices.Add(i);
                //lastImgIndex.Add(lastIndx - i);
            }

            if (HowManyThread <= 1)
                CalcVisionData(0, 0, count, 1, 1, IsFile);
            else
            {
                //m__G.fManage.AddViewLog(string.Format("FineCOG Parallel\r\n"));

                Parallel.ForEach(taskIndices, taskIndex =>
                {
                    CalcVisionData(0, taskIndex, count, HowManyThread, taskIndex, IsFile);
                });
            }

            m__G.oCam[0].mFinishVisionData = true;   //  맞다

            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
            SupremeTimer.QueryPerformanceCounter(ref endTime);
            ltime = (endTime - startTime) / (double)(lTimerFrequency);

            return ltime;
        }


        //public void CalcVisionData(int cam, int ci, int ce, int iBuf, out double[] dPosX, out double[] dPosY, out double[] dPosZ, out double[] dTX, out double[] dTY, out double[] dTZ)
        //{
        //    dPosX = new double[ce - ci];
        //    dPosY = new double[ce - ci];
        //    dPosZ = new double[ce - ci];
        //    dTX = new double[ce - ci];
        //    dTY = new double[ce - ci];
        //    dTZ = new double[ce - ci];
        //    int i = 0;
        //    mb_FinishCalcVision[iBuf] = false;
        //    try
        //    {
        //        for (i = ci; i < ce; i++)    //  Skip 0 ~ 59
        //            m__G.oCam[cam].FineCOG(i, iBuf, ref dPosX[i], ref dPosY[i], ref dPosZ[i], ref dTX[i], ref dTX[i], ref dTX[i]);    // 마크찾기

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //    mb_FinishCalcVision[iBuf] = true;
        //}

        private void cbContinuosMode_CheckedChanged(object sender, EventArgs e)
        {
            m__G.oCam[0].HaltA();

            btnLive2.Enabled = false;
            btnAllLEDOn.Enabled = false;
            btnLEDDOWN.Enabled = false;
            btnLEDUP.Enabled = false;
            btnHalt2.Enabled = false;
          
            cbContinuosMode.Enabled = false;

            if (cbContinuosMode.Checked)
                CameraReset(2, true);
            else
                CameraReset(2, false);

            btnLive2.Enabled = true;
            btnAllLEDOn.Enabled = true;
            btnLEDDOWN.Enabled = true;
            btnLEDUP.Enabled = true;
            btnHalt2.Enabled = true;
           
            cbContinuosMode.Enabled = true;

            System.Diagnostics.Process currentProc = System.Diagnostics.Process.GetCurrentProcess();
            long memoryUsed = currentProc.PrivateMemorySize64;
            tbVsnLog.Text += "Memory\t" + (memoryUsed / 1000000.0).ToString("F3") + "\tMB is used by the Application\r\n";
        }

        public void CameraReset(int grabgerType = 1, bool IsContinuous = true)
        {
            String[] strFileName = new string[2] { "", "" };
            //if (m__G.oCam[0].IsInit == true)
            //    m__G.oCam[0].Free();

            //m__G.oCam[0] = new MILlib(1.0);

            //Thread.Sleep(100);

            string strPath = m__G.m_RootDirectory + "\\RunData\\";
            int lHroi = (m__G.mHROI / 10) * 10;

            strFileName[0] = strPath + "Continuous_10tap_" + m__G.mVROI[1].ToString() + "_" + lHroi.ToString() + "R.dcf";
            strFileName[1] = strPath + "ExtTrg_10tap_" + m__G.mVROI[1].ToString() + "_" + lHroi.ToString() + "R.dcf";

            if (!File.Exists(strFileName[0]))
            {
                tbVsnLog.Text += strFileName[0] + " not found. Fail to change mode.";
                return;
            }


            string lSystemName = "M_SYSTEM_SOLIOS";
            if (grabgerType == 2)
                lSystemName = "M_SYSTEM_RADIENTEVCL";

            if (!m__G.m_bSwap)
            {
                //MessageBox.Show("Camera Normal");
                if (IsContinuous)
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("Off");


                    m__G.oCam[0].ChangeDataFormat(0, strFileName[0]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 0, 0, strFileName[0]);  //  COM4  
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
                else
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

                    m__G.oCam[0].ChangeDataFormat(0, strFileName[1]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 0, 0, strFileName[1]);  //  COM4  
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
            }
            else
            {
                //MessageBox.Show("Camera Swapped");
                if (IsContinuous)
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("Off");
                    m__G.oCam[0].ChangeDataFormat(0, strFileName[0]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 1, 0, strFileName[0]);
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
                else
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

                    m__G.oCam[0].ChangeDataFormat(0, strFileName[1]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 0, 0, strFileName[1]);  //  COM4  
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
            }
            //m__G.oCam[0].SelectWindow(panelCam0.Handle);

            //m__G.mFAL.Show();
            //m__G.mFAL.ShowMarkDGV();
            //m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //m__G.mFAL.Location = new Point(0, 0);
            //m__G.mFAL.Hide();
        }

        private void btnFitFOV_Click(object sender, EventArgs e)
        {
            /////////////////////////////////////////////////////
            /////////////////////////////////////////////////////
            //  To Check Memory Leakage 
            //for ( int i=0; i< 100; i++)
            //{
            //    cbContinuosMode.Checked = true;
            //    Thread.Sleep(1000);
            //    while (btnGrab2.Enabled != true)
            //        Thread.Sleep(100);

            //    Thread.Sleep(500);

            //    cbContinuosMode.Checked = false;
            //    Thread.Sleep(1000);
            //    while (btnGrab2.Enabled != true)
            //        Thread.Sleep(100);

            //    Thread.Sleep(500);

            //}
            //return;
            /////////////////////////////////////////////////////
            /////////////////////////////////////////////////////


            //  960 - 360 continuous 모드로 변경한다
            //  여기서 각 마크의 위치를 찾는다. 
            //  마크 위치만 찾 Extract6D() 는 수행하지 않는다.
            //  마크 위치에 따라 FOV 위치가 342 pixel 상/하단에서 마진을 동일하게 가지도록 미세 조정한다.
            //  FOV DYOffet = YOffsetCur - YOffsetOld
            //  960 - 342 trigger mode 로 변경한다.
            Thread threadFitFOV = new Thread(() => FitFOV());
            threadFitFOV.Start();

        }

        public void FitFOV()
        {
            //  960 - 360 continuous 모드로 변경한다
            //  여기서 각 마크의 위치를 찾는다. 
            //  마크 위치만 찾 Extract6D() 는 수행하지 않는다.
            //  마크 위치에 따라 FOV 위치가 342 pixel 상/하단에서 마진을 동일하게 가지도록 미세 조정한다.
            //  FOV DYOffet = YOffsetCur - YOffsetOld
            //  960 - 342 trigger mode 로 변경한다.
            //  영상 획득해서 보여준다.

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    cbContinuosMode.Checked = true;
                    panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, 627);
                });
            else
            {
                cbContinuosMode.Checked = true;
                panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, 627);
            }

            Thread.Sleep(1000);

            Process.LEDs_All_On(0, true);
            Thread.Sleep(50);
            m__G.oCam[0].GrabB(0);

            m__G.oCam[0].FineCOG(true, 0, 0, true, false);

            string strtmp = "";

            List<double> markYtop = new List<double>();
            List<double> markYbtm = new List<double>();

            for (int i = 0; i < 12; i++)
            {
                if (m__G.oCam[0].mAzimuthPts[0][i].X == 0) continue;
                strtmp += m__G.oCam[0].mAzimuthPts[0][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[0][i].Y.ToString("F3") + "\t";


            }
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < m__G.oCam[0].mFAL.mFidMarkSide.Count; j++)
                {
                    if (m__G.oCam[0].m_sMR[i].Azimuth == m__G.oCam[0].mFAL.mFidMarkSide[j].Azimuth)
                    {
                        double ltop = m__G.oCam[0].m_sMR[i].pos.Y - m__G.oCam[0].mFAL.mFidMarkSide[j].modelSize.Height / 2;
                        markYtop.Add(ltop);
                        double lbtm = m__G.oCam[0].m_sMR[i].pos.Y + m__G.oCam[0].mFAL.mFidMarkSide[j].modelSize.Height / 2;
                        markYbtm.Add(lbtm);
                    }
                }

                for (int j = 0; j < m__G.oCam[0].mFAL.mFidMarkTop.Count; j++)
                {
                    if (m__G.oCam[0].m_sMR[i].Azimuth == (m__G.oCam[0].mFAL.mFidMarkTop[j].Azimuth + 8))
                    {
                        double lbtm = m__G.oCam[0].m_sMR[i].pos.Y + m__G.oCam[0].mFAL.mFidMarkTop[j].modelSize.Height / 2;
                        markYbtm.Add(lbtm);
                    }
                }
            }
            double[] lmarkYtop = markYtop.ToArray();
            double[] lmarkYbtm = markYbtm.ToArray();
            Array.Sort(lmarkYtop);
            Array.Sort(lmarkYbtm);
            double ytop = lmarkYtop[0];
            double ybtm = lmarkYbtm[lmarkYbtm.Length - 1];

            //  영상 획득은 YROI = 360 pixel 으로 획득한 뒤 mVROI 에 맞춰서 FOV 를 이동한다.
            int fovShiftY = (int)((ytop - 10) / Math.Sin(40 / 180.0 * Math.PI) - (mVROI - ybtm - 10)) / 2;
            v_OrgROIV_min[0] += fovShiftY;
            ChangeROIYOffsetDeltaY(0, fovShiftY);
            SaveOrgROI(1);

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += "FOV Shift Delta Y = " + fovShiftY.ToString() + "\r\n";
                    tbInfo.SelectionStart = tbInfo.Text.Length;
                    tbInfo.ScrollToCaret();
                    cbContinuosMode.Checked = false;
                    panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, (int)(panelCam0.Size.Height * mVROI / 360.0));
                });
            else
            {
                tbInfo.Text += "FOV Shift Delta Y = " + fovShiftY.ToString() + "\r\n";
                tbInfo.SelectionStart = tbInfo.Text.Length;
                tbInfo.ScrollToCaret();
                cbContinuosMode.Checked = false;
                panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, (int)(panelCam0.Size.Height * mVROI / 360.0));
            }

            Thread.Sleep(1000);

            m__G.oCam[0].GrabB(0);
            SaveOrgROI(1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            m__G.mDoingStatus = "Checking Vision";
            for (int i = 0; i < 50; i++)
                m__G.oCam[0].GrabB(i);



            m__G.oCam[0].CalcBackgroundNoise(0, 50, 0);

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoise.csv");
            for (int i = 0; i < m__G.oCam[0].nSizeX; i++)
                wr.WriteLine(m__G.oCam[0].mBackgroundNoise[i].ToString());
            wr.Close();

            wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoiseY.csv");
            for (int i = 0; i < m__G.oCam[0].nSizeY; i++)
                wr.WriteLine(m__G.oCam[0].mBackgroundNoiseY[i].ToString());
            wr.Close();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void LoadBackbroundNoise()
        {
            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoise.csv")) return;

            try
            {
                StreamReader rd = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoise.csv");
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                m__G.oCam[0].mBackgroundNoise = new int[eachLine.Length];
                for (int i = 0; i < eachLine.Length; i++)
                    m__G.oCam[0].mBackgroundNoise[i] = byte.Parse(eachLine[i]);

            }
            catch (Exception e)
            {
                return;
            }
            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoiseY.csv")) return;

            try
            {
                StreamReader rd = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoiseY.csv");
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                m__G.oCam[0].mBackgroundNoiseY = new int[eachLine.Length];
                for (int i = 0; i < eachLine.Length; i++)
                    m__G.oCam[0].mBackgroundNoiseY[i] = byte.Parse(eachLine[i]);

            }
            catch (Exception e)
            {
                return;
            }
            m__G.oCam[0].mFAL.SetBackgroundNoise(m__G.oCam[0].mBackgroundNoise, m__G.oCam[0].mBackgroundNoiseY);
        }

        string[] mModelFileList = null;
        public void SetModelFileList(string[] flist)
        {
            mModelFileList = new string[flist.Length];
            for (int i = 0; i < flist.Length; i++)
            {
                mModelFileList[i] = flist[i];
            }
        }
        public void TransferModelFileList()
        {
            if (m__G == null)
                return;

            if (m__G.oCam[0] == null)
                return;

            if (m__G.oCam[0].mFAL == null)
                return;

            m__G.oCam[0].mFAL.SetFMICandidates(mModelFileList);
        }

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    //  Default Mark Position
        //    System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
        //        new System.Drawing.Point( 730, 78 ),
        //        new System.Drawing.Point( 234, 93 ),
        //        new System.Drawing.Point( 730, 255 ),
        //        new System.Drawing.Point( 234, 275 ),
        //        new System.Drawing.Point( 439, 294 ),
        //        new System.Drawing.Point( 532, 294 ) };

        //    m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

        //    if (m__G.mFAL != null)
        //    {
        //        string markPosFile = m__G.mFAL.GetFileNameOfMarkPosOnPanel();
        //        if (File.Exists(markPosFile))
        //        {
        //            StreamReader sr = new StreamReader(markPosFile);
        //            string allLines = sr.ReadToEnd();
        //            sr.Close();
        //            List<System.Drawing.Point> mPos = new List<System.Drawing.Point>();
        //            string[] eachLine = allLines.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //            for (int i = 0; i < eachLine.Length; i++)
        //            {
        //                if (eachLine[i].Length < 3)
        //                    continue;
        //                string[] xypos = eachLine[i].Split(',');
        //                if (xypos.Length < 2)
        //                    continue;
        //                System.Drawing.Point lp = new System.Drawing.Point();
        //                lp.X = int.Parse(xypos[0]);
        //                lp.Y = int.Parse(xypos[1]);
        //                mPos.Add(lp);
        //            }
        //            if (mPos.Count > 0)
        //            {
        //                markPos = mPos.ToArray();
        //            }
        //        }
        //    }

        //    m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
        //}

        private void cbEnhancedLED_CheckedChanged(object sender, EventArgs e)
        {

        }

        public double ms_sinTheta = 0.64278761;    //  sin(40deg)
        public double[] ms_scaleX = new double[3] { 0, 1, 0 };
        public double[] ms_scaleY = new double[3] { 0, 1, 0 };
        public double[] ms_scaleZ = new double[3] { 0, 1, 0 };
        public double[] ms_scaleTX = new double[3] { 0, 1, 0 };
        public double[] ms_scaleTY = new double[3] { 0, 1, 0 };
        public double[] ms_scaleTZ = new double[3] { 0, 1, 0 };
        public double ms_EastViewYPscale = 1.0;
        public double[] ms_XtoYbyView = new double[3];
        public double[] ms_XtoZbyView = new double[3];
        public double[] ms_XtoTXbyView = new double[3];
        public double[] ms_XtoTYbyView = new double[3];
        public double[] ms_XtoTZbyView = new double[3];
        public double[] ms_YtoXbyView = new double[3];
        public double[] ms_YtoZbyView = new double[3];
        public double[] ms_YtoTXbyView = new double[3];
        public double[] ms_YtoTYbyView = new double[3];
        public double[] ms_YtoTZbyView = new double[3];
        public double[] ms_ZtoXbyView = new double[3];
        public double[] ms_ZtoYbyView = new double[3];
        public double[] ms_ZtoTXbyView = new double[3];
        public double[] ms_ZtoTYbyView = new double[3];
        public double[] ms_ZtoTZbyView = new double[3];
        public double[] ms_TXtoTYbyView = new double[3];
        public double[] ms_TXtoTZbyView = new double[3];
        public double[] ms_TYtoTXbyView = new double[3];
        public double[] ms_TYtoTZbyView = new double[3];
        public double[] ms_TZtoTXbyView = new double[3];
        public double[] ms_TZtoTYbyView = new double[3];
        public double[] ms_XJtoXbyView = new double[2];
        public double[] ms_YJtoYbyView = new double[2];
        public double[] ms_ZJtoZbyView = new double[2];
        public double[] ms_TZtoZbyView = new double[3];
        //private void btnCalcScales_Click(object sender, EventArgs e)
        //{
        //    double rX_NtoS = 0;
        //    //double rY_NStoE = 0;
        //    double rY_dY = 0;
        //    double rZ_dZ = 0;

        //    double mX_NtoS = 0;
        //    double mYside_dY = 0;
        //    double mY_dY = 0;
        //    double mYside_dZ = 0;


        //    try
        //    {
        //        if (tb_rX_NtoS.Text.Length > 0)
        //            rX_NtoS = double.Parse(tb_rX_NtoS.Text);
        //        if (tb_rY_dY.Text.Length > 0)
        //            rY_dY = double.Parse(tb_rY_dY.Text);
        //        if (tb_rZ_dZ.Text.Length > 0)
        //            rZ_dZ = double.Parse(tb_rZ_dZ.Text);

        //        if (tb_mX_NtoS.Text.Length > 0)
        //            mX_NtoS = double.Parse(tb_mX_NtoS.Text);        //  Top View 에서 N 과 S 마크간 X 거리 입력할 것, scaleX 무시하고 pixel 단위로 입력할 것
        //        if (tb_mYside_dY.Text.Length > 0)
        //            mYside_dY = double.Parse(tb_mYside_dY.Text);  //  Side View 에서 N-S 와 E 마크간 Y 거리 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것
        //        if (tb_mY_dY.Text.Length > 0)
        //            mY_dY = double.Parse(tb_mY_dY.Text);            //  Y 이동에 따른 Top View 에서의 Y 변동량 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것
        //        if (tb_mYside_dZ.Text.Length > 0)
        //            mYside_dZ = double.Parse(tb_mYside_dZ.Text);        //  Z 이동에 따른 Side View 에서의 Y 변동량 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것

        //        mX_NtoS = mX_NtoS * 0.0055 / Global.LensMag;
        //        mYside_dY = mYside_dY * 0.0055 / Global.LensMag;
        //        mY_dY = mY_dY * 0.0055 / Global.LensMag;
        //        mYside_dZ = mYside_dZ * 0.0055 / Global.LensMag;

        //        ms_scaleX = rX_NtoS / mX_NtoS;
        //        ms_scaleY = rY_dY / mY_dY;
        //        ms_sinTheta = mYside_dY / rY_dY;
        //        double cosTheta = Math.Sqrt(1 - ms_sinTheta * ms_sinTheta);
        //        ms_scaleZ = rZ_dZ / (mYside_dZ * ms_scaleY / cosTheta);

        //        tbScaleX.ForeColor = Color.Black;
        //        tbScaleY.ForeColor = Color.Black;
        //        tbScaleZ.ForeColor = Color.Black;
        //        tbSinTheta.ForeColor = Color.Black;

        //        tbScaleX.Text = ms_scaleX.ToString("F4");
        //        tbScaleY.Text = ms_scaleY.ToString("F4");
        //        tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //        tbSinTheta.Text = ms_sinTheta.ToString("F4");
        //    }
        //    catch (Exception exc)
        //    {
        //        ;
        //    }

        //}

        private void btnUpdateScales_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                 ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                 ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                 ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                 ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                 ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );
            if (ms_sinTheta > 0)
                m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
            else
                m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

            SaveScaleNTheta();

            tbInfo.Size = new System.Drawing.Size(tbInfo.Size.Width + 400, tbInfo.Size.Height);
            tbInfo.Location = new System.Drawing.Point(tbInfo.Location.X - 400, tbInfo.Location.Y);
            tbVsnLog.Size = new System.Drawing.Size(tbVsnLog.Size.Width + 400, tbVsnLog.Size.Height);
            tbVsnLog.Location = new System.Drawing.Point(tbVsnLog.Location.X - 400, tbVsnLog.Location.Y);
            bScaleUpdating = false;

            m__G.oCam[0].DrawClear();
            DrawMarkPositions();
        }

        public void SaveScaleNTheta()
        {
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);

            wr.WriteLine($"{ms_sinTheta}");

            wr.WriteLine($"{ms_scaleX[0]:E5}\t{ms_scaleX[1]:E5}\t{ms_scaleX[2]:E5}\t// Tab 분리, X scale : aX^2 + bX + c");
            wr.WriteLine($"{ms_scaleY[0]:E5}\t{ms_scaleY[1]:E5}\t{ms_scaleY[2]:E5}\t// Tab 분리. Y scale");
            wr.WriteLine($"{ms_scaleZ[0]:E5}\t{ms_scaleZ[1]:E5}\t{ms_scaleZ[2]:E5}\t// Tab 분리, Z scale");
            wr.WriteLine($"{ms_scaleTX[0]:E5}\t{ms_scaleTX[1]:E5}\t{ms_scaleTX[2]:E5}\t// Tab 분리, TX scale");
            wr.WriteLine($"{ms_scaleTY[0]:E5}\t{ms_scaleTY[1]:E5}\t{ms_scaleTY[2]:E5}\t// Tab 분리, TY scale");
            wr.WriteLine($"{ms_scaleTZ[0]:E5}\t{ms_scaleTZ[1]:E5}\t{ms_scaleTZ[2]:E5}\t// Tab 분리, TZ scale");

            wr.WriteLine($"{ms_EastViewYPscale:E5}\t// Tab 분리, EastView Y pixel Scale");

            wr.WriteLine($"{ms_XtoYbyView[0]:E5}\t{ms_XtoYbyView[1]:E5}\t{ms_XtoYbyView[2]:E5}\t// Tab 분리, X to Y coef");
            wr.WriteLine($"{ms_XtoZbyView[0]:E5}\t{ms_XtoZbyView[1]:E5}\t{ms_XtoZbyView[2]:E5}\t// Tab 분리, X to Z coef");
            wr.WriteLine($"{ms_XtoTXbyView[0]:E5}\t{ms_XtoTXbyView[1]:E5}\t{ms_XtoTXbyView[2]:E5}\t// Tab 분리, X to TX coef");
            wr.WriteLine($"{ms_XtoTYbyView[0]:E5}\t{ms_XtoTYbyView[1]:E5}\t{ms_XtoTYbyView[2]:E5}\t// Tab 분리, X to TY coef");
            wr.WriteLine($"{ms_XtoTZbyView[0]:E5}\t{ms_XtoTZbyView[1]:E5}\t{ms_XtoTZbyView[2]:E5}\t// Tab 분리, X to TZ coef");

            wr.WriteLine($"{ms_YtoXbyView[0]:E5}\t{ms_YtoXbyView[1]:E5}\t{ms_YtoXbyView[2]:E5}\t// Tab 분리, Y to X coef");
            wr.WriteLine($"{ms_YtoZbyView[0]:E5}\t{ms_YtoZbyView[1]:E5}\t{ms_YtoZbyView[2]:E5}\t// Tab 분리, Y to Z coef");
            wr.WriteLine($"{ms_YtoTXbyView[0]:E5}\t{ms_YtoTXbyView[1]:E5}\t{ms_YtoTXbyView[2]:E5}\t// Tab 분리, Y to TX coef");
            wr.WriteLine($"{ms_YtoTYbyView[0]:E5}\t{ms_YtoTYbyView[1]:E5}\t{ms_YtoTYbyView[2]:E5}\t// Tab 분리, Y to TY coef");
            wr.WriteLine($"{ms_YtoTZbyView[0]:E5}\t{ms_YtoTZbyView[1]:E5}\t{ms_YtoTZbyView[2]:E5}\t// Tab 분리, Y to TZ coef");

            wr.WriteLine($"{ms_ZtoXbyView[0]:E5}\t{ms_ZtoXbyView[1]:E5}\t{ms_ZtoXbyView[2]:E5}\t// Tab 분리, Z to X coef");
            wr.WriteLine($"{ms_ZtoYbyView[0]:E5}\t{ms_ZtoYbyView[1]:E5}\t{ms_ZtoYbyView[2]:E5}\t// Tab 분리, Z to Y coef");
            wr.WriteLine($"{ms_ZtoTXbyView[0]:E5}\t{ms_ZtoTXbyView[1]:E5}\t{ms_ZtoTXbyView[2]:E5}\t// Tab 분리, Z to TX coef");
            wr.WriteLine($"{ms_ZtoTYbyView[0]:E5}\t{ms_ZtoTYbyView[1]:E5}\t{ms_ZtoTYbyView[2]:E5}\t// Tab 분리, Z to TY coef");
            wr.WriteLine($"{ms_ZtoTZbyView[0]:E5}\t{ms_ZtoTZbyView[1]:E5}\t{ms_ZtoTZbyView[2]:E5}\t// Tab 분리, Z to TZ coef");

            wr.WriteLine($"{ms_TXtoTYbyView[0]:E5}\t{ms_TXtoTYbyView[1]:E5}\t{ms_TXtoTYbyView[2]:E5}\t// Tab 분리, TX to TY coef");
            wr.WriteLine($"{ms_TXtoTZbyView[0]:E5}\t{ms_TXtoTZbyView[1]:E5}\t{ms_TXtoTZbyView[2]:E5}\t// Tab 분리, TX to TZ coef");
            wr.WriteLine($"{ms_TYtoTXbyView[0]:E5}\t{ms_TYtoTXbyView[1]:E5}\t{ms_TYtoTXbyView[2]:E5}\t// Tab 분리, TY to TX coef");
            wr.WriteLine($"{ms_TYtoTZbyView[0]:E5}\t{ms_TYtoTZbyView[1]:E5}\t{ms_TYtoTZbyView[2]:E5}\t// Tab 분리, TY to TZ coef");
            wr.WriteLine($"{ms_TZtoTXbyView[0]:E5}\t{ms_TZtoTXbyView[1]:E5}\t{ms_TZtoTXbyView[2]:E5}\t// Tab 분리, TZ to TX coef");
            wr.WriteLine($"{ms_TZtoTYbyView[0]:E5}\t{ms_TZtoTYbyView[1]:E5}\t{ms_TZtoTYbyView[2]:E5}\t// Tab 분리, TZ to TY coef");

            wr.WriteLine($"{ms_XJtoXbyView[0]:E5}\t{ms_XJtoXbyView[1]:E5}\t// Tab 분리, XY XZ to X coef");
            wr.WriteLine($"{ms_YJtoYbyView[0]:E5}\t{ms_YJtoYbyView[1]:E5}\t// Tab 분리, YX Y to X coef");
            wr.WriteLine($"{ms_ZJtoZbyView[0]:E5}\t{ms_ZJtoZbyView[1]:E5}\t// Tab 분리, ZX ZY to X coef");
            wr.WriteLine($"{ms_TZtoZbyView[0]:E5}\t{ms_TZtoZbyView[1]:E5}\t{ms_TZtoZbyView[2]:E5}\t// Tab 분리, TZ to Z coef");
            wr.Close();

            AddVsnLog("Saved scales");

        }
        // 옛날 스테이지 Scale
        public bool SKLoadScaleNTheta()
        {
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                //  파일이 없으면 기본값을 저장한 기본 파일을 생성해준다.
                StreamWriter orgwr = new StreamWriter(scaleFile);
                string istr = "1.00\t// Tab 분리, X scale : aX^2 + bX + c\r\n" +
                              "1.00\t// Tab 분리, Y scalea : aY^2 + bY + c\r\n" +
                              "1.00\t// Tab 분리, Z scale\r\n" +
                              "0.64278761\r\n" +
                              "1.00\t// Tab 분리, TX scale\r\n" +
                              "1.00\t// Tab 분리, TY scale\r\n" +
                              "0.00\t// Tab 분리, Z to X coef\r\n" +
                              "0.00\t// Tab 분리, Z to Y coef\r\n" +
                              "0.00\t// Tab 분리, Y to X coef\r\n" +
                              "0.00\t// Tab 분리, Y to Z coef : aY^2 + bY + c\r\n" +
                              "0.00\t// Tab 분리, X to Y coef\r\n" +
                              "0.00\t// Tab 분리, X to Z coef\r\n" +
                              "1.00\t// Tab 분리, EastView Y pixel Scale\r\n" +
                              "0.00\t// Tab 분리, X to TX coef\r\n" +
                              "0,00\t// Tab 분리, Y to TY coef\r\n" +
                              "0.00\t// Tab 분리, FX0 of 6 axis stage, default value = 0, X Offset of the center of Fiducial Mark\r\n" +
                              "0.00\t// Tab 분리, FY0 of 6 axis stage, default value = 0, Y Offset of the center of Fiducial Mark\r\n" +
                              "55.00\t// / Tab 분리, L1 of 6 axis stage TY\r\n" +
                              "55.00\t// / Tab 분리, L2 of 6 axis stage TY\r\n" +
                              "0.00\t// / Tab 분리, Y Offset of Probe TY\r\n" +
                              "55.00\t// / Tab 분리, Y Offset of Probe TX\r\n" +
                              "32.30\t// / Tab 분리, Probe X Rx\r\n" +
                              "32.30\t// / Tab 분리, Probe Y Ry\r\n";
                orgwr.Write(istr);
                orgwr.Close();
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 4)
                    return false;

                double[][] dScales = new double[][]
                {
                    ms_scaleX, ms_scaleY, ms_scaleZ, new double[3], ms_scaleTX, ms_scaleTY,
                    ms_ZtoXbyView, ms_ZtoYbyView,
                    ms_YtoXbyView, ms_YtoZbyView,
                    ms_XtoYbyView, ms_XtoZbyView, new double[3], ms_XtoTXbyView, ms_YtoTXbyView,
                };

                double[] dCenterOfFiducialMarkOffset = new double[8]
                {
                    0.0, 0.0, 55.0, 55.0, 0.0, 55.0, 32.0, 32.0
                };

                for (int i = 0; i < eachLine.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (i == 3)
                    {
                        ms_sinTheta = double.Parse(strdata[0]);
                    }
                    else if (i == 12)
                    {
                        ms_EastViewYPscale = double.Parse(strdata[0]);
                    }
                    else if (i < 15)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            dScales[i][0] = double.Parse(strdata[0]);
                            dScales[i][1] = double.Parse(strdata[1]);
                            dScales[i][2] = double.Parse(strdata[2]);
                        }
                        else
                        {
                            dScales[i][0] = 0.0;
                            dScales[i][1] = double.Parse(strdata[0]);
                            dScales[i][2] = 0.0;
                        }
                    }
                    else if (i < 23)
                    {

                        dCenterOfFiducialMarkOffset[i - 15] = double.Parse(strdata[0]);
                    }
                }
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );

                double Fx = dCenterOfFiducialMarkOffset[0];
                double Fy = dCenterOfFiducialMarkOffset[1];
                double L1 = dCenterOfFiducialMarkOffset[2];
                double L2 = dCenterOfFiducialMarkOffset[3];
                double txL1 = dCenterOfFiducialMarkOffset[4];
                double txL2 = dCenterOfFiducialMarkOffset[5];
                double Rx = dCenterOfFiducialMarkOffset[6];
                double Ry = dCenterOfFiducialMarkOffset[7];

                m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2, txL1, txL2, Rx, Ry);

                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                //m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");

            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        // 자화 스테이지 Scale
        public bool JHLoadScaleNtheta()
        {
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                //  파일이 없으면 기본값을 저장한 기본 파일을 생성해준다.
                StreamWriter orgwr = new StreamWriter(scaleFile);
                string istr = "1.00\t// Tab 분리, X scale : aX^2 + bX + c\r\n" +
                                "1.00\t// Tab 분리, Y scalea : aY^2 + bY + c\r\n" +
                                "1.00\t// Tab 분리, Z scale\r\n" +
                                "0.64278761\r\n" +
                                "1.00\t// Tab 분리, TX scale: aTX^2 + bTX + c\r\n" +
                                "1.00\t// Tab 분리, TY scale\r\n" +
                                "0.00\t// Tab 분리, Z to X coef\r\n" +
                                "0.00\t// Tab 분리, Z to Y coef\r\n" +
                                "0.00\t// Tab 분리, Y to X coef\r\n" +
                                "0.00\t// Tab 분리, Y to Z coef : aY^2 + bY + c\r\n" +
                                "0.00\t// Tab 분리, X to Y coef\r\n" +
                                "0.00\t// Tab 분리, X to Z coef: aX^2 + bX + c\r\n" +
                                "1.00\t// Tab 분리, EastView Y pixel Scale\r\n" +
                                "0.00\t// Tab 분리, X to TX coef\r\n" +
                                "0.00\t// Tab 분리, Y to TX coef\r\n";
                orgwr.Write(istr);
                orgwr.Close();
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 4)
                    return false;

                double[][] dScales = new double[][]
                {
                    ms_scaleX, ms_scaleY, ms_scaleZ, new double[3], ms_scaleTX, ms_scaleTY,
                    ms_ZtoXbyView, ms_ZtoYbyView,
                    ms_YtoXbyView, ms_YtoZbyView,
                    ms_XtoYbyView, ms_XtoZbyView, new double[3], ms_XtoTXbyView, ms_YtoTXbyView
                };

                for (int i = 0; i < dScales.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (i == 3)
                    {
                        ms_sinTheta = double.Parse(strdata[0]);
                    }
                    else if (i == 12)
                    {
                        ms_EastViewYPscale = double.Parse(strdata[0]);
                    }
                    else
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            dScales[i][0] = double.Parse(strdata[0]);
                            dScales[i][1] = double.Parse(strdata[1]);
                            dScales[i][2] = double.Parse(strdata[2]);
                        }
                        else
                        {
                            dScales[i][0] = 0.0;
                            dScales[i][1] = double.Parse(strdata[0]);
                            dScales[i][2] = 0.0;
                        }
                    }
                }
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );
                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                //m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        // 하이드리드 스테이지 Scale
        public bool LoadScaleNTheta()
        {
            //MessageBox.Show("LoadscaleNTheta called ");
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                InitializeScaleNTheta();
                SaveScaleNTheta();
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 29)
                    return false;
                string[] eachData = new string[eachLine.Length];

                double[][] dScales = new double[33][]
                {
                    new double[3] ,ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, new double[3],
                    ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                    ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                    ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                    ms_TXtoTYbyView, ms_TXtoTZbyView,
                    ms_TYtoTXbyView, ms_TYtoTZbyView,
                    ms_TZtoTXbyView, ms_TZtoTYbyView,
                    ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                    ms_TZtoZbyView
                };

                for (int i = 0; i < eachLine.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (i == 0)
                    {
                        ms_sinTheta = double.Parse(strdata[0]);
                    }
                    else if (i == 7)
                    {
                        ms_EastViewYPscale = double.Parse(strdata[0]);

                    }
                    else if (i < 32 && i > 28)
                    {
                        dScales[i][0] = double.Parse(strdata[0]);
                        dScales[i][1] = double.Parse(strdata[1]);
                    }
                    else
                    {
                        if (strdata.Length > 3)
                        {
                            dScales[i][0] = double.Parse(strdata[0]);
                            dScales[i][1] = double.Parse(strdata[1]);
                            dScales[i][2] = double.Parse(strdata[2]);
                        }
                        else
                        {
                            dScales[i][0] = 0.0;
                            dScales[i][1] = double.Parse(strdata[0]);
                            dScales[i][2] = 0.0;
                        }
                    }
                }
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                 ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                 ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                 ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                 ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                 ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );
                AddVsnLog("Loaded scales");

                long comCalV = (long)(Math.Pow(10, 15) * (1 - 7.0 / (dScales.SelectMany(arr => arr).Sum() + ms_sinTheta + ms_EastViewYPscale)));

                AddVsnLog("CompressedCalibrationValue : " + comCalV);

                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                //m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");

                bool isUncalibrated = false;
                // default scaleNtheta
                //if ((ms_scaleX[0] == 0 && ms_scaleX[1] == 1 && ms_scaleX[0] == 0) ||
                //    ms_EastViewYPscale == 1 || ms_TZtoZbyView[1] == 0)
                if ((ms_scaleX[0] == 0 && ms_scaleX[1] == 1 && ms_scaleX[0] == 0) ||
                    ms_EastViewYPscale == 1)
                {
                    isUncalibrated = true;
                }

                SettbUnCalibratedInfoVisible(isUncalibrated);
                STATIC.fManage.SettbUncalibratedInfoVisible(isUncalibrated);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        bool bScaleUpdateValid = false;
        bool bDedicatedScaleUpdateValid = false;
        bool bScaleUpdating = false;
        private void tbInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = true;

            if (e.KeyCode == Keys.D)
                bDedicatedScaleUpdateValid = true;
        }

        //private void tbInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    if (bScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 410, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X + 410, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 410, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X + 410, tbVsnLog.Location.Y);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");

        //            //tbPairedMarkScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbPairedMarkScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbPairedMarkScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbPairedMarkSinTheta.Text = ms_sinTheta.ToString("F4");
        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X - 410, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X - 410, tbVsnLog.Location.Y);
        //            bScaleUpdating = false;
        //        }
        //    }else if (bDedicatedScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");

        //            //tbPairedMarkScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbPairedMarkScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbPairedMarkScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbPairedMarkSinTheta.Text = ms_sinTheta.ToString("F4");
        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = false;
        //        }
        //    }
        //}

        private void tbVsnLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = true;
        }

        //private void tbVsnLog_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    if (bScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 400, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X + 400, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 400, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X + 400, tbVsnLog.Location.Y);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");
        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 400, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X - 400, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 400, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X - 400, tbVsnLog.Location.Y);
        //            bScaleUpdating = false;
        //        }
        //    }
        //    else if (bDedicatedScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");

        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = false;
        //        }
        //    }
        //}

        private void tbInfo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = false;

        }

        private void tbVsnLog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = false;

        }

        private void cbLiveWithMarks_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLiveWithMarks.Checked)
            {

                // 250326 Live with Mark 타이밍 수정
                // tbInfo.Font = new Font("Calibri", 14, FontStyle.Bold);
                // bLiveFindMark = true;
                //Task.Run(() => LiveFindMark());              
            }
            else
            {
                // 250326 Live with Mark 타이밍 수정
                // bLiveFindMark = false;
                tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
            }

            // 250326 Live with Mark 타이밍 수정
            // 이미 Live 중 cbLiveWithMarks를 변경했을 때
            if (!bHaltLive)
            {
                StartLive();
            }

        }

        public double[] mDYforScale = new double[10];
        //private void btnYNmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Y 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Y 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Side View N/S 마크의 Y 좌표 평균을 저장한다.

        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    double tX = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    m__G.oCam[0].DrawClear();
        //    for ( int i=0; i<5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;
        //    }
        //    mDYforScale[0] = tY / 10;
        //    mDYforScale[1] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    tb_mX_NtoS.Text = (tX / 5).ToString("F3");

        //    StartLive();

        //}

        //private void btnYPmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Y 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Y 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Side View N/S 마크의 Y 좌표 평균을 저장한다.

        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    double tX = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;
        //    }
        //    mDYforScale[2] = tY / 10;
        //    mDYforScale[3] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    double dYonTop = Math.Abs(mDYforScale[2] - mDYforScale[0]);
        //    double dYonSide = Math.Abs(mDYforScale[3] - mDYforScale[1]);

        //    double ntX = double.Parse(tb_mX_NtoS.Text);
        //    tX = (ntX + tX / 5) / 2;
        //    tb_mX_NtoS.Text = tX.ToString("F3");
        //    tb_mYside_dY.Text = dYonSide.ToString("F3");
        //    tb_mY_dY.Text = dYonTop.ToString("F3");

        //    StartLive();

        //}

        //private void btnZNmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Z 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Z 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Side View N/S/E 마크의 Y 좌표 평균을 저장한다.
        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    m__G.oCam[0].DrawClear();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //    }
        //    mDYforScale[4] = tY / 10;
        //    mDYforScale[5] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    StartLive();

        //}

        //private void btnZPmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Z 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Z 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Side View N/S/E 마크의 Y 좌표 평균을 저장한다.
        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //    }
        //    mDYforScale[6] = tY / 10;
        //    mDYforScale[7] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    double dYforDZonTop = Math.Abs(mDYforScale[6] - mDYforScale[4]);
        //    double dYforDZonSide = Math.Abs(mDYforScale[7] - mDYforScale[5]);

        //    dYforDZonSide = dYforDZonSide - dYforDZonTop * Math.Sin(40 / 180.0 * Math.PI);
        //    tb_mYside_dZ.Text = dYforDZonSide.ToString("F3");

        //    StartLive();

        //}

        //private void btnPairedMarkUpdateScale_Click(object sender, EventArgs e)
        //{
        //    ms_scaleX = double.Parse(tbPairedMarkScaleX.Text);
        //    ms_scaleY = double.Parse(tbPairedMarkScaleY.Text);
        //    ms_scaleZ = double.Parse(tbPairedMarkScaleZ.Text);
        //    ms_sinTheta = double.Parse(tbPairedMarkSinTheta.Text);

        //    m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ);
        //    if (ms_sinTheta > 0)
        //        m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
        //    else
        //        m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

        //    SaveScaleNTheta();

        //    tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //    tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //    bScaleUpdating = false;
        //    m__G.oCam[0].DrawClear();
        //    DrawMarkPositions();
        //}

        //private void btnMeasurePairedMark_Click(object sender, EventArgs e)
        //{
        //    //  1차 각 ROI 를 상측으로 50% 이동해서 5회 측정 평균 구한다.
        //    //  2차 각 ROI 를 하측으로 50% 이동해서 5회 측정 평균 구한다.
        //    //   ROI 를 원상복귀 시킨다.
        //    //   Top View 에서 N-S 마크들 간 X 거리의 평균치를 계산한다.
        //    //   Top View 에서 Paired 마크들 간 Y 거리의 평균치를 계산한다.
        //    //   Side View 에서 Paired 마크들 간 Y 거리의 평균치를 계산한다.
        //    //   결과치를 tbAvgXfromNtoSTop, tbAvgYbetweenPairTop, tbAvgYbetweenPairSide 에 표시한다.

        //    m__G.mDoingStatus = "Checking Vision";
        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double[] tY = new double[2];
        //    double[] sY = new double[2];
        //    double[] tX = new double[2];
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    m__G.oCam[0].DrawClear();
        //    //  상측으로 ROI 이동한 상태에서 상측 mark 만 측정
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        //foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkSide)
        //        //{
        //        //    OpenCvSharp.Rect rc = lFmark.searchRoi;
        //        //    rc.Y = rc.Y - rc.Height / 5;
        //        //    lFmark.searchRoi.Y = rc.Y;
        //        //}
        //        //foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkTop)
        //        //{
        //        //    OpenCvSharp.Rect rc = lFmark.searchRoi;
        //        //    rc.Y = rc.Y - rc.Height / 5;
        //        //    lFmark.searchRoi.Y = rc.Y;
        //        //}
        //        FindMarks();
        //        tY[0] += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY[0] += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX[0] += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;

        //        //  하측으로 ROI 이동한 상태에서 상측 mark 만 측정

        //        OpenCvSharp.Rect[] rc = new OpenCvSharp.Rect[12];
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkSide)
        //        {
        //            rc[lFmark.Azimuth] = new OpenCvSharp.Rect();
        //            rc[lFmark.Azimuth] = lFmark.searchRoi;
        //            lFmark.searchRoi.Y = (int)(m__G.oCam[0].mAzimuthPts[1][lFmark.Azimuth].Y  + 3);
        //        }
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkTop)
        //        {
        //            rc[lFmark.Azimuth + 8] = new OpenCvSharp.Rect();
        //            rc[lFmark.Azimuth + 8] = lFmark.searchRoi;
        //            lFmark.searchRoi.Y = (int)(m__G.oCam[0].mAzimuthPts[1][lFmark.Azimuth + 8].Y  + 3);
        //        }
        //        FindMarks();
        //        tY[1] += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;  
        //        sY[1] += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX[1] += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;
        //                //  ROI 복구
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkSide)
        //        {
        //            lFmark.searchRoi.Y = rc[lFmark.Azimuth].Y;
        //        }
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkTop)
        //        {
        //            lFmark.searchRoi.Y = rc[lFmark.Azimuth + 8].Y;
        //        }
        //    }

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    double avgXfromXtoS = (tX[0] + tX[1]) / 10;
        //    double avgYbetweenPairTop = Math.Abs((tY[0] - tY[1]) / 10);
        //    double avgYbetweenPairSide = Math.Abs((sY[0] - sY[1]) / 10);

        //    tbAvgXfromNtoSTop.Text = avgXfromXtoS.ToString("F4");
        //    tbAvgYbetweenPairTop.Text = avgYbetweenPairTop.ToString("F4");
        //    tbAvgYbetweenPairSide.Text = avgYbetweenPairSide.ToString("F4");

        //    m__G.mDoingStatus = "IDLE";

        //    StartLive();
        //}

        //private void btnPairedMarkCalcScale_Click(object sender, EventArgs e)
        //{
        //    double rAvgX_NtoS = 0;
        //    double rAvgY_betweenPair = 0;

        //    double mX_NtoS = 0;
        //    double mY_betweenPairTop = 0;
        //    double mY_betweenPairSide = 0;

        //    try
        //    {
        //        if (tbRealAvgXfromNtoS.Text.Length > 0)
        //            rAvgX_NtoS = double.Parse(tbRealAvgXfromNtoS.Text);
        //        if (tbRealAvgYbetweenPair.Text.Length > 0)
        //            rAvgY_betweenPair = double.Parse(tbRealAvgYbetweenPair.Text);

        //        if (tbAvgXfromNtoSTop.Text.Length > 0)
        //            mX_NtoS = double.Parse(tbAvgXfromNtoSTop.Text);        //  Top View 에서 N 과 S 마크간 X 거리 입력할 것, scaleX 무시하고 pixel 단위로 입력할 것
        //        if (tbAvgYbetweenPairTop.Text.Length > 0)
        //            mY_betweenPairTop = double.Parse(tbAvgYbetweenPairTop.Text);  //  Side View 에서 N-S 와 E 마크간 Y 거리 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것
        //        if (tbAvgYbetweenPairSide.Text.Length > 0)
        //            mY_betweenPairSide = double.Parse(tbAvgYbetweenPairSide.Text);            //  Y 이동에 따른 Top View 에서의 Y 변동량 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것

        //        mX_NtoS = mX_NtoS * 0.0055 / Global.LensMag;
        //        mY_betweenPairTop = mY_betweenPairTop * 0.0055 / Global.LensMag;
        //        mY_betweenPairSide = mY_betweenPairSide * 0.0055 / Global.LensMag;

        //        ms_scaleX = rAvgX_NtoS / mX_NtoS;
        //        ms_scaleY = rAvgY_betweenPair / mY_betweenPairTop;
        //        ms_sinTheta = mY_betweenPairSide / mY_betweenPairTop;
        //        double cosTheta = Math.Sqrt(1 - ms_sinTheta * ms_sinTheta);
        //        double cos40 = Math.Cos(40 / 180.0 * Math.PI);
        //        ms_scaleZ = ms_scaleY;  // = cos40 / cosTheta ;   //  ( 1/cosTheta ) / ( 1/cos40 ) = cos40 / cosTheta
        //                                                    // ms_sinTheta 를 Update 하고나면 Zscale 이 별도 없다.
        //                                                    // 이론적으로보면 Theta 와 Y scale 로 결정된다.
        //                                                    // 이후 발생하는 오차는 영상의 Z 축과 제품의 Z 축이 서로 달라서 발생하는 오차뿐이다.

        //        tbPairedMarkScaleX.ForeColor = Color.Black;
        //        tbPairedMarkScaleY.ForeColor = Color.Black;
        //        tbPairedMarkScaleZ.ForeColor = Color.Black;
        //        tbPairedMarkSinTheta.ForeColor = Color.Black;

        //        tbPairedMarkScaleX.Text = ms_scaleX.ToString("F4");
        //        tbPairedMarkScaleY.Text = ms_scaleY.ToString("F4");
        //        tbPairedMarkScaleZ.Text = ms_scaleZ.ToString("F4");
        //        tbPairedMarkSinTheta.Text = ms_sinTheta.ToString("F4");
        //    }
        //    catch (Exception exc)
        //    {
        //        ;
        //    }
        //}
        public string SaveScreenShot(string strHost)
        {
            if (m__G == null)
                return "";  //  초기화이전

            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            string pngname = strHost + dtNow.ToString("yyMMddhhmmss") + ".png";
            string sScreenCapturePath = m__G.m_RootDirectory + "\\User_ScreenShot\\" + pngname;
            string sDir = m__G.m_RootDirectory + "\\User_ScreenShot";
            Bitmap memoryImage;
            memoryImage = new Bitmap(1920, 1080);
            System.Drawing.Size s = new System.Drawing.Size(memoryImage.Width, memoryImage.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);


            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
            memoryImage.Save(sScreenCapturePath);
            return sScreenCapturePath;
        }


        private void btnSetMasterTilt_Click(object sender, EventArgs e)
        {
            if (!bLiveFindMark && !bThreadManualFindMarks)
            {
                MessageBox.Show("This button is only available in 'Live with Marks' or 'Grab to Find Marks'", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m__G.mDoingStatus = "Checking Vision";
            m__G.mDoingStatus = "IDLE";

            // 250326 Live with Mark에서만 가능 -> Grab to Find Mark에서도 가능으로 변경
            // 모두 SetOffSet, SignTXTY 리셋하고 GrabToFindMark나 LivewithMark를 다시 찍고 그결과로 OFFSET 값을 구하기. 로 변경

            double orgMasterX = double.Parse(tbMasterX.Text);   // um
            double orgMasterY = double.Parse(tbMasterY.Text);   // um
            double orgMasterZ = double.Parse(tbMasterZ.Text);   // um
            double orgMasterTx = double.Parse(tbMasterTX.Text); // min
            double orgMasterTy = double.Parse(tbMasterTY.Text); // min
            double orgMasterTz = double.Parse(tbMasterTZ.Text); // min

            // 현재 SetSignTXTY의 부호 고려. SignTX가 - 일때 사용자가 tbMasterTX.Text에 n을 입력하면 singn 초기화 값 기준인 orgMasterX -n으로 적용해야함.
            if (m__G.m_bXTiltReverse)
            {
                orgMasterTx *= -1;
            }

            if (m__G.m_bYTiltReverse)
            {
                orgMasterTy *= -1;
            }

            // offset, sign 초기화
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);


            int cntData = mCalibrationFullData.Count;

            while (mCalibrationFullData.Count <= cntData)
            {
                Thread.Sleep(200);
            }

            int lastIdx = mCalibrationFullData.Count - 1;
            double[] lastData = mCalibrationFullData[lastIdx];

            double lx = lastData[0];
            double ly = lastData[1];
            double lz = lastData[2];
            double ltx = lastData[3];
            double lty = lastData[4];
            double ltz = lastData[5];

            ////  ltx, lty, ltz 는 측정값 (min)
            ////  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)
            ////  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
            SaveTXTYZeroOffset(lx, ly, lz, ltx, lty, ltz, orgMasterX, orgMasterY, orgMasterZ, orgMasterTx, orgMasterTy, orgMasterTz, true);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);



            //DrawMarkPositions();

            //m__G.oCam[0].GrabB(1);

            //string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\SetZeroGrab.bmp";
            //m__G.oCam[0].SaveImageBuf(fname);

            //m__G.oCam[0].GrabB(2);
            //m__G.oCam[0].GrabB(3);
            //m__G.oCam[0].GrabB(4);
            //m__G.oCam[0].GrabB(5);

            //m__G.oCam[0].mFAL.LoadFMICandidate();
            //m__G.oCam[0].mFAL.BackupFMI();

            // X, Y, Z 추가 25.03.25
            //double lx = 0;
            //double ly = 0;
            //double lz = 0;  
            //double ltx = 0;
            //double lty = 0;
            //double ltz = 0;

            //MessageBox.Show("Call SetTXTYOffset 1");
            // m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
            // m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);

            //for (int i = 1; i < 6; i++)
            //{
            //    FindMarks(i);

            //    lx += m__G.oCam[0].mC_pX[i]; /** 5.5 / Global.LensMag;    //  Pixel to um*/
            //    ly += m__G.oCam[0].mC_pY[i]; /** 5.5 / Global.LensMag;    //  Pixel to um*/
            //    lz += m__G.oCam[0].mC_pZ[i]; /** 5.5 / Global.LensMag;    //  Pixel to um*/
            //    ltx += m__G.oCam[0].mC_pTX[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            //    lty += m__G.oCam[0].mC_pTY[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            //    ltz += m__G.oCam[0].mC_pTZ[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            //}

            //lx = lx / 5;
            //ly = ly / 5;
            //lz = lz / 5;
            //ltx = ltx / 5;
            //lty = lty / 5;
            //ltz = ltz / 5;

            //double masterTx = 0;
            //double masterTy = 0;
            //double masterTz = 0;

            //double orgMasterX = double.Parse(tbMasterX.Text);
            //double orgMasterY = double.Parse(tbMasterY.Text);
            //double orgMasterZ = double.Parse(tbMasterZ.Text);
            //double orgMasterTx = double.Parse(tbMasterTX.Text);
            //double orgMasterTy = double.Parse(tbMasterTY.Text);
            //double orgMasterTz = double.Parse(tbMasterTZ.Text);

            //masterTz = orgMasterTz;
            //try
            //{
            //    if (m__G.m_bXTiltReverse)
            //        masterTx = -orgMasterTx;
            //    else
            //        masterTx = orgMasterTx;
            //}
            //catch (Exception err)
            //{
            //    tbMasterTX.Text = "0";
            //}
            //try
            //{
            //    if (m__G.m_bYTiltReverse)
            //        masterTy = -orgMasterTy;
            //    else
            //        masterTy = orgMasterTy;
            //}
            //catch (Exception err)
            //{
            //    tbMasterTY.Text = "0";
            //}

            ////if (m__G.m_bXTiltReverse)
            ////    masterTx = -masterTx;
            ////if (m__G.m_bXTiltReverse)
            ////    masterTy = -masterTy;

            ////  ltx, lty, ltz 는 측정값 (radian)
            ////  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)  
            ////  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
            //SaveTXTYZeroOffset(lx, ly, lz, ltx, lty, ltz, masterTx, masterTy, masterTz, orgMasterX, orgMasterY, orgMasterZ, orgMasterTx, orgMasterTy, orgMasterTz);
            //m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);

            //m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        }
        // offset X, Y, Z 추가 25.03.25
        public void SaveTXTYZeroOffset(double x, double y, double z, double tx, double ty, double tz, double orgMasterX, double orgMasterY, double orgMasterZ, double orgMasterTx, double orgMasterTy, double orgMasterTz, bool isNew = false)
        {

            //MessageBox.Show("Call SetTXTYOffset 2");
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset((x - orgMasterX) * Um_To_Pixel, (y - orgMasterY) * Um_To_Pixel, (z - orgMasterZ) * Um_To_Pixel, (tx - orgMasterTx) * MIN_To_RAD, (ty - orgMasterTy) * MIN_To_RAD, (tz - orgMasterTz) * MIN_To_RAD);

            string filename = "";
            int Index = GetMasterZeroIndex();
            filename = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero\\" + Index.ToString() + "_" + camID0 + ".txt";



            //string filename = sFileDir + "\\DoNotTouch\\TXTYTZoffset_" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);
            wr.WriteLine(x.ToString());
            wr.WriteLine(y.ToString());
            wr.WriteLine(z.ToString());
            wr.WriteLine(tx.ToString());
            wr.WriteLine(ty.ToString());
            wr.WriteLine(tz.ToString());

            wr.WriteLine(orgMasterX.ToString());
            wr.WriteLine(orgMasterY.ToString());
            wr.WriteLine(orgMasterZ.ToString());
            wr.WriteLine(orgMasterTx.ToString());
            wr.WriteLine(orgMasterTy.ToString());
            wr.WriteLine(orgMasterTz.ToString());
            wr.Close();
        }

        // offset X, Y, Z 추가 25.03.25
        public string LoadTXTYZeroOffset()
        {
            try
            {
                string sPath = m__G.m_RootDirectory + "\\DoNotTouch";
                if (!Directory.Exists(sPath)) Directory.CreateDirectory(sPath);
                string cPath = sPath + "\\PreviousOffsetIndex.txt";

                if (!File.Exists(cPath))
                {
                    StreamWriter sw = new StreamWriter(cPath);
                    sw.WriteLine("0");
                    sw.Close();
                }

                int curIndex = GetMasterZeroIndex();

                string sFile = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero";
                if (!Directory.Exists(sFile)) Directory.CreateDirectory(sFile);
                int count = GetMasterZeroCount();
                if (count < 1)
                {
                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
                    SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
                    return "X Y Z TX TY TZ offset = 0, 0, 0, 0, 0, 0";
                }

                sFile += "\\" + curIndex.ToString() + "_" + camID0.ToString() + ".txt";
                StreamReader rd = new StreamReader(sFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                double x = double.Parse(eachLine[0]);   // um
                double y = double.Parse(eachLine[1]);   // um
                double z = double.Parse(eachLine[2]);   // um
                double tx = double.Parse(eachLine[3]);  // min
                double ty = double.Parse(eachLine[4]);  // min
                double tz = double.Parse(eachLine[5]);  // min

                double orgMasterX = double.Parse(eachLine[6]);  // um
                double orgMasterY = double.Parse(eachLine[7]);  // um
                double orgMasterZ = double.Parse(eachLine[8]);  // um
                double orgMasterTx = double.Parse(eachLine[9]); // min
                double orgMasterTy = double.Parse(eachLine[10]);    // min
                double orgMasterTz = double.Parse(eachLine[11]);    // min

                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset((x - orgMasterX) * Um_To_Pixel, (y - orgMasterY) * Um_To_Pixel, (z - orgMasterZ) * Um_To_Pixel, (tx - orgMasterTx) * MIN_To_RAD, (ty - orgMasterTy) * MIN_To_RAD, (tz - orgMasterTz) * MIN_To_RAD);

                tbMasterX.Text = orgMasterX.ToString("F2");
                tbMasterY.Text = orgMasterY.ToString("F2");
                tbMasterZ.Text = orgMasterZ.ToString("F2");
                tbMasterTX.Text = (m__G.m_bXTiltReverse ? -orgMasterTx : orgMasterTx).ToString("F2");
                tbMasterTY.Text = (m__G.m_bYTiltReverse ? -orgMasterTy : orgMasterTy).ToString("F2");
                tbMasterTZ.Text = orgMasterTz.ToString("F2");

                return $"X Y Z TX TY TZ offset = {orgMasterX:F2}, {orgMasterY:F2}, {orgMasterZ:F2}, {orgMasterTx:F2}, {orgMasterTy:F2}, {orgMasterTz:F2}";
            }
            catch
            {
                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);

                return "X Y Z TX TY TZ offset = 0, 0, 0, 0, 0, 0";
            }
        }

        ////public OpenCvSharp.Point2d[] mZLUT = null;

        ////public double ApplyZLUT(double Z)
        ////{
        ////    return Z;

        ////    //  Obsolete since 231024
        ////    //if (mZLUT == null)
        ////    //    return Z;

        ////    //int len = mZLUT.Length;
        ////    //for (int i = 0; i < len - 1; i++)
        ////    //{
        ////    //    //  Linear Interpolation
        ////    //    if ( (mZLUT[i].X - Z) * (mZLUT[i + 1].X - Z) < 0)
        ////    //    {
        ////    //        double delta = mZLUT[i].Y + (Z - mZLUT[i].X) * (mZLUT[i + 1].Y - mZLUT[i].Y) / (mZLUT[i + 1].X - mZLUT[i].X);
        ////    //        //MessageBox.Show(mZLUT[i].X.ToString("F3") + " " + Z.ToString() + " " + mZLUT[i + 1].X.ToString("F3") + " " + delta.ToString("F3"));
        ////    //        return Z - delta;
        ////    //    }
        ////    //}

        ////    //if (Z < mZLUT[0].X)
        ////    //    return Z - mZLUT[0].Y;

        ////    //if (Z > mZLUT[len - 1].X)
        ////    //    return Z - mZLUT[len - 1].Y;

        ////    //return Z;
        ////}

        ////public bool GetZLUT(string filename)
        ////{
        ////    mZLUT = null;

        ////    //MessageBox.Show("ZLUTfile = " + filename);    //  파일명 제대로 들어오는지 디버깅
        ////    if (!File.Exists(filename))
        ////        return false;

        ////    List<OpenCvSharp.Point2d> lp = new List<OpenCvSharp.Point2d>();
        ////    StreamReader sr = new StreamReader(filename);
        ////    string allstr = sr.ReadToEnd();
        ////    sr.Close();
        ////    string[] eachLine = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        ////    string debuglstr = "";
        ////    foreach (string lstr in eachLine)
        ////    {
        ////        string[] strElements = lstr.Split('\t');
        ////        double x = double.Parse(strElements[0]);
        ////        double y = double.Parse(strElements[1]);
        ////        OpenCvSharp.Point2d pt = new OpenCvSharp.Point2d(x, y);
        ////        lp.Add(pt);
        ////        debuglstr += pt.X.ToString("F3") + "," + pt.Y.ToString("F3") + "\r\n";
        ////    }
        ////    mZLUT = lp.ToArray();
        ////    //MessageBox.Show(debuglstr);   //  제대로 읽었는지 디버깅


        ////    //double res = ApplyZLUT(700);


        ////    DateTime datetime = DateTime.Now;
        ////    DateTimeOffset datetimeOffset = new DateTimeOffset(datetime);
        ////    long unixTime = datetimeOffset.ToUnixTimeSeconds();

        ////    return true;

        ////}

        private void button6_Click_1(object sender, EventArgs e)
        {
            SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
        }

        string mBinPath = "";
        private void btnOpenResultBin_Click(object sender, EventArgs e)
        {
            string sFilePath = m__G.m_RootDirectory + "\\Data\\";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "dat";
            openFile.Multiselect = true;
            if (mBinPath == "")
                openFile.InitialDirectory = sFilePath;
            else
                openFile.InitialDirectory = mBinPath;

            openFile.Filter = "Dat(*.dat)|*.dat";
            TextBox[] lTBX = new TextBox[2];
            lTBX[0] = tbInfo;
            lTBX[1] = tbVsnLog;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (openFile.FileNames.Length == 0)
                    return;

                tbVsnLog.Text = "";
                string[] sFileName = new string[openFile.FileNames.Length];
                for (int i = 0; i < openFile.FileNames.Length; i++)
                {
                    sFileName[i] = openFile.FileNames[i];
                    if (!File.Exists(sFileName[i]))
                        return;
                    string lstr = MyOwner.ReadResultBin(sFileName[i]);
                    if (sFileName[i].Contains("_0_"))
                        lTBX[0].Text += lstr + "\r\n";
                    else
                        lTBX[1].Text += lstr + "\r\n";
                }
                mBinPath = sFileName[0].Substring(0, sFileName[0].LastIndexOf("\\"));
            }
        }

        private void cbSetTXTYwithMaster_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSetTXTYwithMaster.Checked)
            {
                MasterList.Enabled = true;
                btnDeleteMaster.Enabled = true;
                btnAddMaster.Enabled = true;
                btnInitialTilt.Enabled = true;
                btnSetMasterTilt.Enabled = true;
                tbMasterX.Enabled = true;
                tbMasterY.Enabled = true;
                tbMasterZ.Enabled = true;
                tbMasterTX.Enabled = true;
                tbMasterTY.Enabled = true;
                tbMasterTZ.Enabled = true;

            }
            else
            {
                MasterList.Enabled = false;
                btnDeleteMaster.Enabled = false;
                btnAddMaster.Enabled = false;
                btnInitialTilt.Enabled = false;
                btnSetMasterTilt.Enabled = false;
                tbMasterX.Enabled = false;
                tbMasterY.Enabled = false;
                tbMasterZ.Enabled = false;
                tbMasterTX.Enabled = false;
                tbMasterTY.Enabled = false;
                tbMasterTZ.Enabled = false;
            }
        }

        private void tbInfo_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sFilePath = m__G.m_RootDirectory + "\\Data\\";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "dat";
            openFile.Multiselect = true;
            openFile.InitialDirectory = sFilePath;

            openFile.Filter = "Dat(*.dat)|*.dat";
            TextBox[] lTBX = new TextBox[2];
            lTBX[0] = tbInfo;
            lTBX[1] = tbVsnLog;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (openFile.FileNames.Length == 0)
                    return;

                tbVsnLog.Text = "";
                string[] sFileName = new string[openFile.FileNames.Length];
                for (int i = 0; i < openFile.FileNames.Length; i++)
                {
                    sFileName[i] = openFile.FileNames[i];
                    if (!File.Exists(sFileName[i]))
                        return;
                    string lstr = MyOwner.ReadResultPos(sFileName[i]);
                    if (sFileName[i].Contains("_1_"))
                        lTBX[1].Text += lstr + "\r\n";
                    else
                        lTBX[0].Text += lstr + "\r\n";
                }
            }
        }
        public void GrabInitalMark()
        {
            if (!bHaltLive)
                GrabHalt();

            LoadScaleNTheta();
            LoadTXTYZeroOffset();
            // 241206 YLUT 적용안함.
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[1];

            Process.LEDs_All_On(0, true);
            Thread.Sleep(50);

            Mat lCropImg = m__G.oCam[0].GrabLoadCropImg(0, false);
            pictureBox2.Image = BitmapConverter.ToBitmap(lCropImg);    //  Grab & Crop
            string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            lCropImg.SaveImage(fileName);     //  Crop & Save

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();
            SetDefaultMarkConfig(true);
            //DrawMarkPositions();

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].ForceTriggerTime();

            int findex = 0;
            System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
                new System.Drawing.Point( 730, 78 ),
                new System.Drawing.Point( 234, 93 ),
                new System.Drawing.Point( 730, 255 ),
                new System.Drawing.Point( 234, 275 ),
                new System.Drawing.Point( 439, 294 ),
                new System.Drawing.Point( 532, 294 ) };

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
            m__G.mFAL.SetMarkNorm();
            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
            string strtmp = "";
            //for (int i = 0;i< markPos.Length; i++)
            //{
            //    if (markPos[i] == null)
            //        continue;
            //    if (markPos[i].X == 0)
            //        continue;

            //    strtmp += markPos[i].X.ToString("F2") + "\t" + markPos[i].Y.ToString("F2") + "\t";
            //}
            //tbVsnLog.Text = strtmp + "\r\n";
            //MessageBox.Show("AAAA");
            double minscale = (180 / Math.PI * 60);                           //  rad to min
            double umscale = (5.5 / Global.LensMag);                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            strtmp = "";
            m__G.mbSuddenStop[0] = false;

            // Constrast 50보다 작을때 MsgBox
            bool isLowContrast = false;

            for (int ci = 0; ci < numFMIcandidate; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;
                double sx = 0;
                double sy = 0;
                double sz = 0;
                double tx = 0;
                double ty = 0;
                double tz = 0;

                double[] lPrismTXTYTZ = new double[3];
                double[] lProbePrismTXTYTZ = new double[3];
                double[] lErrorPrismTXTYTZ = new double[3];

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].mFAL.mbGetHistogram = true;
                NthMeasure(0, "");
                FindIDmark(lCropImg);
                SetMasterZeroIndex(mMarkID);
                //txtMsaterNum.Text = mMarkID.ToString();
                if (txtMsaterNum.InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        txtMsaterNum.Text = mMarkID.ToString();
                    });
                else
                {
                    txtMsaterNum.Text = mMarkID.ToString();
                }


                m__G.oCam[0].mFAL.mbGetHistogram = false;

                sx = m__G.oCam[0].mC_pX[0] * umscale;
                sy = m__G.oCam[0].mC_pY[0] * umscale;
                sz = m__G.oCam[0].mC_pZ[0] * umscale;
                tx = m__G.oCam[0].mC_pTX[0] * minscale;
                ty = m__G.oCam[0].mC_pTY[0] * minscale;
                tz = m__G.oCam[0].mC_pTZ[0] * minscale;

                strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t\tContrast\t";




                for (int i = 0; i < 5; i++)
                {
                    var constrast = m__G.oCam[0].mFAL.mEffectiveContrast[i];

                    if (constrast < 50)
                    {
                        isLowContrast = true;
                    }

                    strtmp += constrast.ToString() + "\t";

                }

                strtmp += "( > 20 )";

                if (m__G.m_bPrismCS)
                {
                    m__G.oCam[0].mFAL.mFZM.SetPrismZeroTXTZ(0, 0, 0);  // init에서는 ConvertTXTYTZofCSHtoPrism를 하기전 PrismZeroTXTZ를 0으로 초기화
                    lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                    m__G.oCam[0].mFAL.mFZM.SetPrismZeroTXTZ(lPrismTXTYTZ[0], lPrismTXTYTZ[1], lPrismTXTYTZ[2]);

                    strtmp += "\tPCS\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                }
            }
            DrawMarkDetected(true);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += strtmp + "\r\n";
                });
            }
            else
            {
                tbInfo.Text += strtmp + "\r\n";
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;

            bHaltLive = true;
            IsLiveCropStop = true;

            if (isLowContrast)
            {
                //MessageBox.Show($"Contrast below 50 detected. Please check the Recipe.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public int mMarkID = 0;
        public void FindIDmark(Mat cropImg)
        {
            Rect IDmarkRegion = new Rect();
            IDmarkRegion.X = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].X * m__G.oCam[0].mFAL.mModelScale + 38);
            IDmarkRegion.Y = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].Y * m__G.oCam[0].mFAL.mModelScale + 3);
            IDmarkRegion.Width = (int)(m__G.oCam[0].mDetectedMarkPos[0][0].X - m__G.oCam[0].mDetectedMarkPos[0][1].X - 9) * m__G.oCam[0].mFAL.mModelScale;
            IDmarkRegion.Height = 36;// (int)(180 - (m__G.oCam[0].mDetectedMarkPos[0][0].Y + m__G.oCam[0].mDetectedMarkPos[0][1].Y) * m__G.oCam[0].mFAL.mModelScale / 2);
            if (IDmarkRegion.X < 1 || IDmarkRegion.Y < 1 || IDmarkRegion.Width < 1 || IDmarkRegion.Height < 1)
                mMarkID = 0;
            else
            {
                Mat IDmarkImg = cropImg.SubMat(IDmarkRegion);
                //Cv2.ImShow("A", IDmarkImg);
                mMarkID = m__G.oCam[0].mFAL.FindMarkID(IDmarkImg);
            }
        }

        public Point2d[] mMarkShift = new Point2d[3];
        // public Point2d[] mMarkShift = new Point2d[3]; 
        public byte[] MakeMarkShift()
        {
            byte[] dataBuf = new byte[8 * 6];
            int curCount = 0;
            byte[] data;
            data = BitConverter.GetBytes(mMarkShift[0].X);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[0].Y);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[1].X);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[1].Y);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[2].X);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[2].Y);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            //curCount += data.Length;

            return dataBuf;
        }
        public void FindCarrierToDummyShift()
        {
            CameraReset(2, true);
            Process.LEDs_All_On(0, true);
            Thread.Sleep(50);

            ManualFindMarks(0, false, false);   //  Fiducial Mark 찾기

            SetExposure(0, 700);

            Thread.Sleep(10);
            Point2d lCarrierRefPt = FineCarrierRef();   //   기준점 찾기
            //Point2d lCarrierRefPt = new Point2d();

            SetOrgExposure(0); //  노출시간 원상복귀

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    DrawMarkDetectedWithDummyShift((int)lCarrierRefPt.X, (int)lCarrierRefPt.Y);
                });
            }
            else
            {
                DrawMarkDetectedWithDummyShift((int)lCarrierRefPt.X, (int)lCarrierRefPt.Y);
            }
            Thread.Sleep(10);

            mMarkShift[0].X = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleX[1] * ((m__G.oCam[0].mAzimuthPts[1][8].X + m__G.oCam[0].mAzimuthPts[1][8].X) / 2 - lCarrierRefPt.X);
            mMarkShift[0].Y = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleY[1] * ((m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][8].Y) / 2 - lCarrierRefPt.Y);
            mMarkShift[1].X = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleX[1] * ((m__G.oCam[0].mAzimuthPts[1][10].X + m__G.oCam[0].mAzimuthPts[1][10].X) / 2 - lCarrierRefPt.X + 260);
            mMarkShift[1].Y = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleY[1] * ((m__G.oCam[0].mAzimuthPts[1][10].Y + m__G.oCam[0].mAzimuthPts[1][10].Y) / 2 - lCarrierRefPt.Y);
            //mMarkShift[2].X = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleX[1] * ((m__G.oCam[0].mAzimuthPts[1][6].X + m__G.oCam[0].mAzimuthPts[1][6].X) / 2 - lCarrierRefPt.X + 130);
            //mMarkShift[2].Y = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleY[1] * ((m__G.oCam[0].mAzimuthPts[1][6].Y + m__G.oCam[0].mAzimuthPts[1][6].Y) / 2 - lCarrierRefPt.Y);
            string lstr = "CarrierShift(um) >\t" + mMarkShift[0].X.ToString("F1") + "\t " + mMarkShift[0].Y.ToString("F1") + "\t "
                        + mMarkShift[1].X.ToString("F1") + "\t " + mMarkShift[1].Y.ToString("F1") + "\t "
                        + mMarkShift[2].X.ToString("F1") + "\t " + mMarkShift[2].Y.ToString("F1") + "\tfrom (" + lCarrierRefPt.X.ToString("F3") + "," + lCarrierRefPt.Y.ToString("F3") + ")\r\n";
            tbInfo.Text += lstr;

            CameraReset(2, false);
        }
        public int FineCarrierCount = 0;
        public Point2d FineCarrierRef()
        {
            // 예전 기준점 찾기
            ////int TopNX = (int)m__G.oCam[0].mAzimuthPts[1][8].X;
            ////int TopNY = (int)m__G.oCam[0].mAzimuthPts[1][8].Y;
            ////Rect refRoiRegion = new Rect();
            ////refRoiRegion.X = TopNX + 51;
            ////refRoiRegion.Y = TopNY + 6;
            ////refRoiRegion.Width = 44;
            ////refRoiRegion.Height = 44;

            Point2d res = new Point2d();
            double[] cx = new double[2];
            double[] cy = new double[2];
            bool cornerFound = false;
            bool cornerFound2 = false;

            Rect refRoiRegion = new Rect();
            refRoiRegion.X = 665;
            refRoiRegion.Y = 240;
            refRoiRegion.Width = 60;
            refRoiRegion.Height = 90;

            m__G.oCam[0].GrabB(1, true);
            m__G.oCam[0].GrabB(2, true);
            //   영상 2개 확보해서 Avg 취한다.

            Mat lCropImg1 = m__G.oCam[0].LoadCropImgWide(1);
            Mat lCropImg2 = m__G.oCam[0].LoadCropImgWide(2);

            Mat refRoiImg1 = lCropImg1.SubMat(refRoiRegion);
            Mat refRoiImg2 = lCropImg2.SubMat(refRoiRegion);

            string fPath = string.Format("D:\\TestImg_lCropImg1_{0}.bmp", FineCarrierCount);
            lCropImg1.SaveImage(fPath);
            FineCarrierCount++;
            //lCropImg2.SaveImage("D:\\TestImg_lCropImg2.bmp");
            //Cv2.ImShow("A", lCropImg1);
            //Cv2.WaitKey();
            //Cv2.DestroyWindow("A");

            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////
            ///
            ////   다음은 저장된 시험용 영상을 이용하는 경우
            ///
            //string sFilePath = Path.GetFullPath("C:\\6AxisTester\\Result\\RawData");
            //if (!Directory.Exists(sFilePath))
            //    Directory.CreateDirectory(sFilePath);

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            //openFileDialog1.Multiselect = true;
            //openFileDialog1.InitialDirectory = sFilePath;
            //openFileDialog1.FilterIndex = 2;
            //if (openFileDialog1.ShowDialog() != DialogResult.OK)
            //    return new Point2d();

            //for ( int i=0; i< openFileDialog1.FileNames.Length; i++ )
            //{
            //    lCropImg1 = new Mat(openFileDialog1.FileNames[i], ImreadModes.Grayscale);
            //    lCropImg2 = new Mat();
            //    lCropImg1.CopyTo(lCropImg2);
            //    lCropImg1.CopyTo(m__G.oCam[0].mFAL.mSourceImg[0]);

            //    refRoiImg1 = lCropImg1.SubMat(refRoiRegion);
            //    refRoiImg2 = lCropImg2.SubMat(refRoiRegion);

            //    cornerFound = CalcTopRight(refRoiImg1, ref cx[0], ref cy[0]);

            //    if (cornerFound )
            //        res = new Point2d((refRoiRegion.X + cx[0]), (refRoiRegion.Y + cy[0]));

            //    string lstr = i.ToString() + "\t" + res.X.ToString("F3") + "\t" + res.Y.ToString("F3") + "\r\n";
            //    tbInfo.Text += lstr;
            //}

            ////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////
            //Cv2.ImShow("B", refRoiImg1);
            //Cv2.WaitKey();
            //Cv2.DestroyWindow("B");
            ////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////


            //  우측 경계 - 기울기 최대 X 좌표,   상측 경계 - 기울기 최대 Y 좌표
            cornerFound = CalcTopRight(refRoiImg1, ref cx[0], ref cy[0]);
            cornerFound2 = CalcTopRight(refRoiImg2, ref cx[1], ref cy[1]);

            if (cornerFound && cornerFound2)
                res = new Point2d((refRoiRegion.X + (cx[0] + cx[1]) / 2), (refRoiRegion.Y + (cy[0] + cy[1]) / 2));

            return res;
        }

        public bool CalcTopRight(Mat lSourceImg, ref double cx, ref double cy)
        {
            byte[] imgBuf = null;
            lSourceImg.GetArray(out imgBuf);
            //Cv2.ImShow("A", lSourceImg);
            //Cv2.WaitKey();
            //Cv2.DestroyWindow("A");

            int nSizeX = lSourceImg.Width;
            int nSizeY = lSourceImg.Height;
            int xmin = 0;
            int xmax = nSizeX - 1;
            int ymin = 0;
            int ymax = nSizeY - 1;
            int xmid = (xmin + xmax) / 2;
            int ymid = (ymin + ymax) / 2;
            int i = 0;
            int j = ymid;
            bool returnValue = true;
            try
            {
                //  사각형의 각 Edge 영역을 대략 찾은 뒤 
                //  각 영역의 중심점을 계산하여 영역을 보정하고, 
                //  보정한 영역에 대해서 중심점을 다시 계산하기를 반복한다.
                //  Top View 의 경우 X 중심점은 X영역 결정 및 Stroke 기준 역할을 하고, y 중심점은 Yaw 계산에 활용된다,.
                double sum = 0;
                double psum = 0;
                double pv = 0;


                //cx = xmid;
                //cy = ymid;
                //return;
                int maxdP = 0;
                int dP = 0;
                int ddP = 0;
                int roiR = 0;  //  Right
                int roiT = 0;  //  Top
                double[] mx = new double[2];
                double[] my = new double[2];
                double[] residueH = new double[2];  //  0 : Left,   1: Right
                double[] residueV = new double[2];  //  0 : Top,    1: Bottom
                //  Edge Search 영역 획득
                int Pleft = 0;
                int Pright = 0;
                int upperArea = 0;
                int lowerArea = 0;
                double middleArea = 0;

                //  Right
                maxdP = 0;
                int xmidR = xmax - 20;

                for (i = xmax - 2; i > xmidR; i--)
                {
                    dP = 0;
                    Pleft = 0;
                    Pright = 0;
                    for (j = ymin; j < ymax; j++)
                    {
                        Pleft += imgBuf[i - 2 + j * nSizeX] + imgBuf[i - 1 + j * nSizeX];
                        Pright += imgBuf[i + 2 + j * nSizeX] + imgBuf[i + 1 + j * nSizeX];
                    }

                    //dP = Pleft - Pright; //  왼쪽이 밝으므로 양수  -> 기존 기준
                    dP = Pright - Pleft; //  오른쪽이 밝으므로 양수  -> 수정 기준 250609

                    if (dP > maxdP)
                    {
                        maxdP = dP;
                        roiR = i - 1;
                    }
                }
                //  Top
                maxdP = 0;
                //int[] dpY = new int[ymid - ymin + 2];
                //string llstr = "";
                for (j = ymin + 4; j < ymin + 34; j++)
                {
                    dP = 0;
                    upperArea = 0;
                    lowerArea = 0;
                    middleArea = 0;
                    for (i = 0; i <= roiR; i++)
                    {
                        //middleArea += imgBuf[i + j * nSizeX];
                        upperArea += imgBuf[i + (j - 1) * nSizeX] + imgBuf[i + (j - 2) * nSizeX] + imgBuf[i + (j - 3) * nSizeX] + imgBuf[i + (j - 4) * nSizeX];
                        lowerArea += imgBuf[i + (j + 1) * nSizeX] + imgBuf[i + (j + 2) * nSizeX] + imgBuf[i + (j + 3) * nSizeX] + imgBuf[i + (j + 4) * nSizeX];
                        ddP = upperArea - lowerArea;
                        dP += ddP;
                    }
                    //upperArea = upperArea / roiR;
                    //lowerArea = lowerArea / roiR;
                    //middleArea = middleArea / roiR;

                    if (dP > maxdP) // 내측 밝고 외측 어두운 경계
                    {
                        maxdP = dP;
                        roiT = j;
                    }
                }

                ///////////////////////
                ///////////////////////
                //  Edge 영역별 COG 계산
                maxdP = 0;

                double weight = 1;
                for (int k = 0; k < 3; k++)
                {
                    //  Right
                    psum = 0;
                    sum = 0;
                    xmax = Math.Min(roiR + 5, nSizeX - 3);
                    for (j = roiT + 3; j <= roiT + 53; j++)
                    {
                        //if (mIsDebug && k == 4)
                        //    lstr = j.ToString() + ",";
                        for (i = roiR - 4; i <= xmax; i++)
                        {
                            if (i == (roiR - 4))
                                weight = 1 - residueH[1];
                            else if (i == (roiR + 4))
                                weight = residueH[1];
                            else
                                weight = 1;

                            //pv = imgBuf[i - 1 + j * nSizeX] - imgBuf[i + 1 + j * nSizeX];   //  왼쪽이 밝을 때
                            pv = imgBuf[i + 1 + j * nSizeX] - imgBuf[i - 1 + j * nSizeX] + imgBuf[i + 2 + j * nSizeX] - imgBuf[i - 2 + j * nSizeX];   //  오른쪽이 밝을 때

                            if (pv < 0)
                                pv = 0;

                            psum += weight * pv * i;
                            sum += weight * pv;
                            //if (mIsDebug && k == 4)
                            //    lstr += p_Value[iBuf][i + j * nSizeX].ToString() + ",";
                        }
                        //if (mIsDebug && k == 4)
                        //    wr.WriteLine(lstr);
                    }
                    //if (mIsDebug && k == 4)
                    //    wr.Close();
                    if (sum > 0)
                    {
                        mx[1] = psum / sum;
                        roiR = (int)(mx[1] + 0.5);
                        residueH[1] = mx[1] - roiR;
                    }
                    else
                        mx[1] = roiR;

                    //  Top
                    psum = 0;
                    sum = 0;
                    ymin = Math.Max(2, roiT - 7);

                    for (j = ymin; j <= roiT + 7; j++)
                    {
                        if (j >= nSizeY || j < 1)
                            continue;
                        //if (mIsDebug && k == 4)
                        //    lstr = j.ToString() + ",";

                        if (j == (roiT - 7))
                            weight = 1 - residueV[0];
                        else if (j == (roiT + 7))
                            weight = residueV[0];
                        else
                            weight = 1;

                        for (i = 0; i < roiR; i++)
                        {
                            if (i == nSizeX)
                                break;

                            pv = imgBuf[i + (j - 2) * nSizeX] + imgBuf[i + (j - 1) * nSizeX] - imgBuf[i + (j + 1) * nSizeX] - imgBuf[i + (j + 2) * nSizeX];
                            if (pv < 0) pv = 0;
                            psum += weight * pv * j;
                            sum += weight * pv;
                            //if (mIsDebug && k == 4)
                            //    lstr += p_Value[iBuf][i + j * nSizeX].ToString() + ",";
                        }
                        //if (mIsDebug && k == 4)
                        //    wr.WriteLine(lstr);
                    }
                    //if (mIsDebug && k == 4)
                    //    wr.Close();
                    if (sum > 0)
                    {
                        my[0] = psum / sum;
                        roiT = (int)(my[0] + 0.5);
                        residueV[0] = my[0] - roiT;
                    }
                    else
                        my[0] = roiT;

                    cx = mx[1];
                    cy = my[0];
                }
                //MessageBox.Show("m_nCam=" + m_nCam.ToString() + " X " + mx[0].ToString("F1") + "-" + mx[1].ToString("F1") + " Y " + my[0].ToString("F1") + "-" + my[1].ToString("F1") + "\r\nL R T B"
                //    + roiL.ToString() + "-" + roiR.ToString() + " " + roiT.ToString() + " " + roiB.ToString());
                //  cx cy 저장
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void Grab_Initial_Click(object sender, EventArgs e)
        {
            GrabInitalMark();

            //FindCarrierToDummyShift();
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            int camCount = 2;


            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt"))
            {
                MessageBox.Show("Camera ID not exists.");
                return;
            }
            try
            {
                BaslerCam[0].Close();
                Thread.Sleep(100);
                BaslerCam[0].Open();
            }
            catch
            {
                ;
            }
            ReadOrgROI(camCount);

            BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

            m__G.mCamCount = 1;

            m_FocusedLED = 0;


            for (int i = 0; i < m__G.mCamCount; i++)
                SetNewROIXY(i, v_OrgROIH_min[i], v_OrgROIH_min[i] + v_OrgROIH_width[i], v_OrgROIV_min[i], v_OrgROIV_min[i] + v_OrgROIV_height[i]);

            //ReadZeroGap(m__G.mCamCount);
            //ReadCalibrationTiltData();

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    m__G.oCam[0].SelectWindow(panelCam0.Handle);
                });
            else
                m__G.oCam[0].SelectWindow(panelCam0.Handle);

            m__G.oCam[0].DisplayZoom(ZoomFactor, ZoomFactor);

            BaslerCam[0].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
            BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(true);
            //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
            BaslerCam[0].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[0]);
            BaslerCam[0].Parameters[PLCamera.GammaEnable].SetValue(true);

            m__G.oCam[0].SetBlobAreaMinMax(m_BlobAreaMin, m_BlobAreaMax);
            string strScaleRotation = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNOpticalR.txt";
            double stop = 0;
            double sside = 0;
            double rtop = 0;
            double rside = 0;

            m__G.oCam[0].LoadScaleNOpticalRotation(strScaleRotation, ref stop, ref sside, ref rtop, ref rside);
            SetExposure(0, Condition.iExposure);
            SetRawGainNGamma(Condition.iRawGain, Condition.iGamma);
            SetEdgeBand(Condition.iEdgeBand);

            cbContinuosMode.Enabled = true;

        }
        public void ReadSerialPort()
        {
            //BinaryReader br = new BinaryReader(fs);


            // Create a new SerialPort object with default settings.
            SerialPort _serialPort = new SerialPort();
            _serialPort.PortName = "COM1";
            _serialPort.BaudRate = 19200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            List<byte> allBytes = null;

            string cmdReadPosition = "GA01\r\n";
            char[] data = cmdReadPosition.ToCharArray();
            _serialPort.Write(data, 0, data.Length);
            Thread.Sleep(1);

            _serialPort.BaseStream.Flush();
            _serialPort.DiscardInBuffer();
            int c = 1;
            while (c != 0)
            {
                c = _serialPort.ReadByte();
                allBytes.Add((byte)c);
            }
            string resStr = allBytes.ToString();

            _serialPort.Close();
        }

        public int[] ExtractStablizedIndex(double[][] measure, int focusedAxis)
        {
            List<int> resIndex = new List<int>();
            int numLine = measure.Length;
            int i = 2;
            double preDelta = 0;
            double oldDelta = 0;
            double postDelta = 0;
            bool settled = false;
            while (i < numLine - 1)
            {
                //  측정이 제대로 안된 점이 있는 경우 Skip
                bool bValidLine = true;
                for (int di = 6; di < 16; di++)
                {
                    if (measure[i][di] == 0)
                    {
                        bValidLine = false;
                        break;
                    }
                }
                if (!bValidLine)
                {
                    i += 2;
                    continue;
                }

                oldDelta = Math.Abs(measure[i - 2][focusedAxis] - measure[i][focusedAxis]);
                preDelta = Math.Abs(measure[i - 1][focusedAxis] - measure[i][focusedAxis]);
                postDelta = Math.Abs(measure[i][focusedAxis] - measure[i + 1][focusedAxis]);
                if (oldDelta < 0.4 && preDelta < 0.4)
                {
                    if (postDelta > 0.5)
                    {
                        resIndex.Add(i);
                        i += 2;
                        settled = false;
                        continue;
                    }
                    else if (postDelta < 0.4)
                    {
                        i++;
                        settled = true;
                        continue;
                    }
                    else if (postDelta > 0.4)
                    {
                        if (Math.Abs(measure[i - 1][focusedAxis] - measure[i + 1][focusedAxis]) > 0.4)
                        {
                            resIndex.Add(i);
                            i += 2;
                            settled = false;
                            continue;
                        }
                        else
                        {
                            i++;
                            settled = true;
                            continue;
                        }
                    }
                }
                else
                    settled = false;

                i++;
            }
            if (settled == true)
                resIndex.Add(numLine - 1);

            return resIndex.ToArray();
        }

        double mZCalAvgY1Y2pp = 0;
        double mZCalY3pp = 0;
        double mYCalAvgY1Y2pp = 0;
        double mYCalY3pp = 0;
        double mEstimatedEastViewYscale = 0;

        //public void JH_SK_CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        //{
        //    if (m__G.oCam[0].mFAL.mFZM == null)
        //    {
        //        MessageBox.Show("mFZM not loaded.");
        //        return;
        //    }

        //    string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
        //    string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
        //    if (!Directory.Exists(AdminPathName))
        //        Directory.CreateDirectory(AdminPathName);

        //    int fullLength = measure.Length;
        //    StreamWriter wr = null;
        //    //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

        //    //  안정화 유효데이터를 추출한다.
        //    //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

        //    List<double[]> stablizedData = new List<double[]>();

        //    int effLength = 0;
        //    //double a = 0;
        //    //double b = 0;
        //    int[] effIndex = null;
        //    FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sTXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTY = new FZMath.Point2D[effLength];

        //    double[] XtoXab = new double[3];
        //    double[] YtoYab = new double[3];
        //    double[] ZtoZab = new double[3];
        //    double[] TXtoTXab = new double[3];
        //    double[] TYtoTYab = new double[3];
        //    double[] TZtoTZab = new double[3];

        //    double[] XtoYab = new double[3];
        //    double[] XtoZab = new double[3];
        //    double[] XtoTXab = new double[2];
        //    double[] XtoTYab = new double[2];
        //    double[] XtoTZab = new double[2];

        //    double[] YtoXab = new double[3];
        //    double[] YtoZab = new double[3];
        //    double[] YtoTXab = new double[2];
        //    double[] YtoTYab = new double[2];
        //    double[] YtoTZab = new double[2];

        //    double[] ZtoXab = new double[3];
        //    double[] ZtoYab = new double[3];
        //    double[] ZtoTXab = new double[3];
        //    double[] ZtoTYab = new double[3];
        //    double[] ZtoTZab = new double[3];

        //    double[] TXtoTYab = new double[3];
        //    double[] TXtoTZab = new double[3];

        //    double[] TYtoTXab = new double[3];
        //    double[] TYtoTZab = new double[3];

        //    double[] TZtoTXab = new double[3];
        //    double[] TZtoTYab = new double[3];

        //    if (!IsRemote)
        //    {
        //        switch (axis)
        //        {
        //            case "Z":
        //                effIndex = ExtractStablizedIndex(measure, 2);
        //                break;

        //            case "X":
        //                effIndex = ExtractStablizedIndex(measure, 0);
        //                break;
        //            case "Y":
        //                effIndex = ExtractStablizedIndex(measure, 1);
        //                break;
        //            case "TX":
        //                effIndex = ExtractStablizedIndex(measure, 3);
        //                break;
        //            case "TY":
        //                effIndex = ExtractStablizedIndex(measure, 4);
        //                break;
        //            default:
        //                break;
        //        }
        //        effLength = effIndex.Length;

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sZZ[i] = new FZMath.Point2D();
        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();
        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();
        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        if (effLength == 0)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    tbInfo.Text += "Stabilized data not found\r\n";
        //                    tbInfo.SelectionStart = tbInfo.Text.Length;
        //                    tbInfo.ScrollToCaret();
        //                });
        //            }
        //            else
        //            {
        //                tbInfo.Text += "Stabilized data not found\r\n";
        //                tbInfo.SelectionStart = tbInfo.Text.Length;
        //                tbInfo.ScrollToCaret();
        //            }
        //            return;

        //        }
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            double[] lstbData = new double[22];
        //            for (int j = 0; j < 22; j++)
        //                lstbData[j] = measure[effIndex[i]][j];

        //            stablizedData.Add(lstbData);
        //        }
        //    }
        //    else
        //    {
        //        //  Remote or Auto Calibration
        //        effLength = measure.Length;

        //        szy1 = new FZMath.Point2D[effLength];
        //        szy2 = new FZMath.Point2D[effLength];
        //        szy3 = new FZMath.Point2D[effLength];

        //        sZZ = new FZMath.Point2D[effLength];
        //        sXX = new FZMath.Point2D[effLength];
        //        sYY = new FZMath.Point2D[effLength];
        //        sTXTX = new FZMath.Point2D[effLength];
        //        sTYTY = new FZMath.Point2D[effLength];
        //        sTZTZ = new FZMath.Point2D[effLength];

        //        sXtoY = new FZMath.Point2D[effLength];
        //        sXtoZ = new FZMath.Point2D[effLength];
        //        sXtoTX = new FZMath.Point2D[effLength];
        //        sXtoTY = new FZMath.Point2D[effLength];
        //        sXtoTZ = new FZMath.Point2D[effLength];

        //        sYtoX = new FZMath.Point2D[effLength];
        //        sYtoZ = new FZMath.Point2D[effLength];
        //        sYtoTX = new FZMath.Point2D[effLength];
        //        sYtoTY = new FZMath.Point2D[effLength];
        //        sYtoTZ = new FZMath.Point2D[effLength];

        //        sZtoX = new FZMath.Point2D[effLength];
        //        sZtoY = new FZMath.Point2D[effLength];
        //        sZtoTX = new FZMath.Point2D[effLength];
        //        sZtoTY = new FZMath.Point2D[effLength];
        //        sZtoTZ = new FZMath.Point2D[effLength];

        //        sTXtoTY = new FZMath.Point2D[effLength];
        //        sTXtoTZ = new FZMath.Point2D[effLength];

        //        sTYtoTX = new FZMath.Point2D[effLength];
        //        sTYtoTZ = new FZMath.Point2D[effLength];

        //        sTZtoTX = new FZMath.Point2D[effLength];
        //        sTZtoTY = new FZMath.Point2D[effLength];

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sZZ[i] = new FZMath.Point2D();
        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoY[i] = new FZMath.Point2D();
        //            sXtoZ[i] = new FZMath.Point2D();
        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();

        //            sYtoX[i] = new FZMath.Point2D();
        //            sYtoZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();

        //            sZtoX[i] = new FZMath.Point2D();
        //            sZtoY[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();

        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();

        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        for (int i = 0; i < effLength; i++)
        //            stablizedData.Add(measure[i]);
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////
        //    //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
        //    StreamWriter lwr = null;
        //    double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

        //    if (!IsRemote)
        //    {
        //        lwr = new StreamWriter(AdminPathName + "FullData.csv");
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        int k = 0;
        //        for (int i = 0; i < fullLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //                slstr += measure[i][j].ToString("F5") + ",";
        //            if (i == effIndex[k])
        //            {
        //                slstr += "*";
        //                k++;
        //            }
        //            lwr.WriteLine(slstr);
        //            if (k == effLength)
        //                break;
        //        }
        //        lwr.Close();
        //    }

        //    string calName = axis;
        //    if (isAutoCalibrationEastView)
        //    {
        //        calName += "_EastView";
        //    }

        //    string strStabilizedFile = "";
        //    if (mAutoCalibrationCount % 2 == 0)
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
        //    else
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

        //    try
        //    {
        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    catch (Exception e)
        //    {
        //        Int64 lnow = (DateTime.Now.ToBinary()) % 1000000;
        //        if (mAutoCalibrationCount % 2 == 0)
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
        //        else
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    if (lwr != null)
        //    {
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //            {
        //                //if (j < 19 || j == 22)
        //                slstr += stablizedData[i][j].ToString("F5") + ",";
        //                //else
        //                //{
        //                //    //slstr += (RAD_To_MIN * stablizedData[i][j]).ToString("F5") + ",";
        //                //    slstr += (stablizedData[i][j]).ToString("F5") + ",";
        //                //}
        //            }

        //            lwr.WriteLine(slstr);
        //        }
        //        lwr.Close();
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////

        //    //  axis 따라서 List 에 저장된 데이터를 처리한다.
        //    double[] p2ndCoef = new double[3];
        //    double[] p2ndCoef2 = new double[3];
        //    double[] p2ndCoef3 = new double[3];
        //    int fovYoffset = GetROIY(0);
        //    string lstr = "";
        //    switch (axis)
        //    {
        //        case "Z":
        //            //  axis == "Z" : YLUT 의 경우 
        //            //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
        //            //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
        //            //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

        //            //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

        //            double a1 = 0;
        //            double b1 = 0;
        //            double a2 = 0;
        //            double b2 = 0;
        //            double a3 = 0;
        //            double b3 = 0;

        //            mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

        //            //  stablizedData[i][16] : X
        //            //  stablizedData[i][17] : Y
        //            //  stablizedData[i][18] : Z
        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
        //            double[] LUT1_tmp = new double[effLength];
        //            double[] LUT1 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

        //            LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
        //            LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

        //            //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
        //            //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
        //            //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
        //            double[] LUT2_tmp = new double[effLength];
        //            double[] LUT2 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

        //            LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
        //            LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

        //            //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
        //            //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
        //            //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
        //            //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
        //                szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
        //            double[] LUT3_tmp = new double[effLength];
        //            double[] LUT3 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

        //            LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
        //            LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

        //            //  Z scale
        //            // 241206 YLUT 제거 후, Z scale 2차로 변경
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sZZ[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
        //                sZZ[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
        //            }
        //            //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szz, effLength, ref a, ref b);
        //            //a = a * 0.9993; // 0.9992; // 헥사포드 Cal 변경
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZZ, effLength, ref ZtoZab);

        //            //a = (a - 1) * 0.4 + 1; 
        //            //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
        //            //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
        //            //  Scale 만 조정해가면서 수차례 반복 필요
        //            //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


        //            if (mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView)
        //            {
        //                string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
        //                string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
        //                wr = new StreamWriter(srcFile);
        //                wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + ZtoZab[1].ToString());
        //                wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
        //                for (int i = 0; i < effLength; i++)
        //                {
        //                    wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
        //                }
        //                wr.Close();
        //                System.IO.File.Copy(srcFile, destFile, true);
        //            }

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  Z to X 계산
        //            //  Z vs X - Xprobe , Z vs Y - Yprobe                 
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
        //                //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
        //                sZtoX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
        //                sZtoY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage

        //                sZtoTX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sZtoTY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sZtoTZ[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref ZtoXab[0], ref ZtoXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref ZtoYab[0], ref ZtoYab[1]);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTX, effLength, ref ZtoTXab[0], ref ZtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTY, effLength, ref ZtoTYab[0], ref ZtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTZ, effLength, ref ZtoTZab[0], ref ZtoTZab[1]);

        //            if (!isAutoCalibrationEastView)
        //            {
        //                // ZtoX, ZtoY 수정
        //                //double aZtoX = 0;
        //                //double aZtoY = 0;
        //                //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref aZtoX, ref b);
        //                //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref aZtoY, ref b);
        //                lstr = $"ZZ Scale\t{ZtoZab[0]:E5}\t{ZtoZab[1]:E5}\t{ZtoZab[2]:E5}\r\n";
        //                lstr += $"ZtoX\t{ZtoXab[0]:E5}\r\n";
        //                lstr += $"ZtoY\t{ZtoYab[0]:E5}\r\n";


        //                if (mAutoCalibrationCount % 2 == 0)
        //                {
        //                    string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                    StreamReader sr = new StreamReader(scaleNthetaFile);
        //                    string allstr = sr.ReadToEnd();
        //                    sr.Close();
        //                    string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    string[] strZscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[2] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZscaleLine.Length; i++)
        //                        allLines[2] += strZscaleLine[i];

        //                    string[] strZtoXLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[6] = ZtoXab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoXLine.Length; i++)
        //                        allLines[6] += strZtoXLine[i];

        //                    string[] strZtoYLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[7] = ZtoYab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoYLine.Length; i++)
        //                        allLines[7] += strZtoYLine[i];

        //                    wr = new StreamWriter(scaleNthetaFile);
        //                    for (int i = 0; i < allLines.Length; i++)
        //                    {
        //                        wr.WriteLine(allLines[i]);
        //                    }
        //                    wr.Close();
        //                }
        //            }


        //            //////////////////////////////////////////////////////////////////////////////////

        //            break;
        //        case "X":
        //            //  Axis = 1 : X scale 확인 및 저장
        //            //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
        //            //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
        //            //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
        //                sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값
        //            }
        //            //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref a, ref b);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref XtoXab);  //  A[0]X^2 + A[1]X + A[2]

        //            lstr = "XX Scale,\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  X to Y ／Ｚ　계산
        //            //  X vs Y - Yprobe , X vs Z - Zprobe

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][18]);
        //                //sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);
        //                sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
        //                sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage

        //                sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sXtoTY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sXtoTZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoY, effLength, ref XtoYab[0], ref XtoYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref XtoZab);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTX, effLength, ref XtoTXab[0], ref XtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTY, effLength, ref XtoTYab[0], ref XtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTZ, effLength, ref XtoTZab[0], ref XtoTZab[1]);

        //            lstr += "XtoY,\t" + XtoYab[0].ToString("E5") + "\r\n";
        //            lstr += "XtoZ,\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTX,\t" + XtoTXab[0].ToString("E5") + "\r\n";

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  X to Y ／Ｚ　계산
        //            //  X vs Y - Yprobe , X vs Z - Zprobe
        //            //////FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
        //            //////for (int i = 0; i < effLength; i++)
        //            //////{
        //            //////    sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3]);
        //            //////}

        //            wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
        //            //for (int i = 0; i < effLength; i++)
        //            //    lstr += sXtoZ[i].X.ToString("F4") + "," + sXtoZ[i].Y.ToString("F4") + "\r\n";
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                string[] strXXscaleLine = allLines[0].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[0] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXXscaleLine.Length; i++)
        //                    allLines[0] += strXXscaleLine[i];

        //                string[] strXtoYLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[10] = XtoYab[0].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoYLine.Length; i++)
        //                    allLines[10] += strXtoYLine[i];

        //                string[] strXtoZLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[11] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoZLine.Length; i++)
        //                    allLines[11] += strXtoZLine[i];

        //                string[] strXtoTXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[13] = XtoTXab[0].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTXLine.Length; i++)
        //                    allLines[13] += strXtoTXLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        case "Y":
        //            mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
        //            mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

        //            //  Axis = 2 : Y scale 확인 및 저장
        //            //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
        //                sYY[i].X = stablizedData[i][1];
        //            }
        //            //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref a, ref b);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref YtoYab);

        //            lstr = "YY Scale,\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  X to Y ／Ｚ　계산
        //            //  X vs Y - Yprobe , X vs Z - Zprobe

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][17]);    //  X - probe X
        //                //sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);   //  Z - probe Z
        //                sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
        //                sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage

        //                sYtoTX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sYtoTY[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sYtoTZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoX, effLength, ref YtoXab[0], ref YtoXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref YtoZab);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTX, effLength, ref YtoTXab[0], ref YtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTY, effLength, ref YtoTYab[0], ref YtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTZ, effLength, ref YtoTZab[0], ref YtoTZab[1]);


        //            lstr += "YtoX,\t" + YtoXab[0].ToString("E5") + "\r\n";
        //            lstr += "YtoZ,\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoTX,\t" + YtoTXab[0].ToString("E5") + "\r\n";

        //            if (isAutoCalibrationEastView)
        //            {
        //                lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
        //            }

        //            wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////                    
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        //                sr.Close();
        //                if (isAutoCalibrationEastView)
        //                {
        //                    string[] strEastScaleLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[12] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
        //                    for (int i = 1; i < strEastScaleLine.Length; i++)
        //                        allLines[12] += strEastScaleLine[i];
        //                }
        //                else
        //                {
        //                    string[] strYYscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[1] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYYscaleLine.Length; i++)
        //                        allLines[1] += strYYscaleLine[i];

        //                    string[] strYtoXLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[8] = YtoXab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoXLine.Length; i++)
        //                        allLines[8] += strYtoXLine[i];

        //                    string[] strYtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[9] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoZLine.Length; i++)
        //                        allLines[9] += strYtoZLine[i];

        //                    string[] strYtoTXLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[14] = YtoTXab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTXLine.Length; i++)
        //                        allLines[14] += strYtoTXLine[i];
        //                }

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
        //        case "TX":
        //            //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
        //            //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTXTX[i].Y = stablizedData[i][19]; // RAD_To_MIN;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTXTX[i].X = stablizedData[i][3];

        //                sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref TXtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTXtoTY, effLength, ref TXtoTYab[0], ref TXtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTXtoTZ, effLength, ref TXtoTZab[0], ref TXtoTZab[1]);


        //            lstr += "TX Scale,\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
        //            wr.WriteLine("TX Scale,\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n");
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXscaleLine.Length; i++)
        //                    allLines[4] += strTXscaleLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        case "TY":
        //            //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
        //            //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTYTY[i].Y = stablizedData[i][20];// * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTYTY[i].X = stablizedData[i][4];

        //                sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][3] - stablizedData[i][19]);
        //                sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][5] - stablizedData[i][21]);
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYTY, effLength, ref TYtoTYab[0], ref TYtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYtoTX, effLength, ref TYtoTXab[0], ref TYtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYtoTZ, effLength, ref TYtoTZab[0], ref TYtoTZab[1]);

        //            lstr += "TY Scale,\t" + TYtoTYab[0].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
        //            wr.WriteLine("TY Scale," + TYtoTYab[0].ToString());
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[5] = TYtoTYab[0].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYscaleLine.Length; i++)
        //                    allLines[5] += strTYscaleLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            tbCalibration.Text += lstr;
        //        });
        //    else
        //        tbCalibration.Text += lstr;

        //}

        //public void CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        //{
        //    if (m__G.oCam[0].mFAL.mFZM == null)
        //    {
        //        MessageBox.Show("mFZM not loaded.");
        //        return;
        //    }

        //    string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
        //    string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
        //    if (!Directory.Exists(AdminPathName))
        //        Directory.CreateDirectory(AdminPathName);

        //    int fullLength = measure.Length;
        //    StreamWriter wr = null;
        //    //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

        //    //  안정화 유효데이터를 추출한다.
        //    //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

        //    List<double[]> stablizedData = new List<double[]>();

        //    int effLength = 0;
        //    int[] effIndex = null;
        //    FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sTXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTY = new FZMath.Point2D[effLength];

        //    double[] XtoXab = new double[3];
        //    double[] YtoYab = new double[3];
        //    double[] ZtoZab = new double[3];
        //    double[] TXtoTXab = new double[3];
        //    double[] TYtoTYab = new double[3];
        //    double[] TZtoTZab = new double[3];

        //    double[] XtoYab = new double[3];
        //    double[] XtoZab = new double[3];
        //    double[] XtoTXab = new double[3];
        //    double[] XtoTYab = new double[3];
        //    double[] XtoTZab = new double[3];

        //    double[] YtoXab = new double[3];
        //    double[] YtoZab = new double[3];
        //    double[] YtoTXab = new double[3];
        //    double[] YtoTYab = new double[3];
        //    double[] YtoTZab = new double[3];

        //    double[] ZtoXab = new double[3];
        //    double[] ZtoYab = new double[3];
        //    double[] ZtoTXab = new double[3];
        //    double[] ZtoTYab = new double[3];
        //    double[] ZtoTZab = new double[3];

        //    double[] TXtoTYab = new double[3];
        //    double[] TXtoTZab = new double[3];

        //    double[] TYtoTXab = new double[3];
        //    double[] TYtoTZab = new double[3];

        //    double[] TZtoTXab = new double[3];
        //    double[] TZtoTYab = new double[3];

        //    if (!IsRemote)
        //    {
        //        switch (axis)
        //        {
        //            case "Z":
        //                effIndex = ExtractStablizedIndex(measure, 2);
        //                break;

        //            case "X":
        //                effIndex = ExtractStablizedIndex(measure, 0);
        //                break;
        //            case "Y":
        //                effIndex = ExtractStablizedIndex(measure, 1);
        //                break;
        //            case "TX":
        //                effIndex = ExtractStablizedIndex(measure, 3);
        //                break;
        //            case "TY":
        //                effIndex = ExtractStablizedIndex(measure, 4);
        //                break;
        //            case "TZ":
        //                effIndex = ExtractStablizedIndex(measure, 5);
        //                break;
        //            default:
        //                break;
        //        }
        //        effLength = effIndex.Length;

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sZZ[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();
        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();
        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        if (effLength == 0)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    tbInfo.Text += "Stabilized data not found\r\n";
        //                    tbInfo.SelectionStart = tbInfo.Text.Length;
        //                    tbInfo.ScrollToCaret();
        //                });
        //            }
        //            else
        //            {
        //                tbInfo.Text += "Stabilized data not found\r\n";
        //                tbInfo.SelectionStart = tbInfo.Text.Length;
        //                tbInfo.ScrollToCaret();
        //            }
        //            return;

        //        }
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            double[] lstbData = new double[22];
        //            for (int j = 0; j < 22; j++)
        //                lstbData[j] = measure[effIndex[i]][j];

        //            stablizedData.Add(lstbData);
        //        }
        //    }
        //    else
        //    {
        //        //  Remote or Auto Calibration
        //        effLength = measure.Length;

        //        szy1 = new FZMath.Point2D[effLength];
        //        szy2 = new FZMath.Point2D[effLength];
        //        szy3 = new FZMath.Point2D[effLength];

        //        sXX = new FZMath.Point2D[effLength];
        //        sYY = new FZMath.Point2D[effLength];
        //        sZZ = new FZMath.Point2D[effLength];
        //        sTXTX = new FZMath.Point2D[effLength];
        //        sTYTY = new FZMath.Point2D[effLength];
        //        sTZTZ = new FZMath.Point2D[effLength];

        //        sXtoY = new FZMath.Point2D[effLength];
        //        sXtoZ = new FZMath.Point2D[effLength];
        //        sXtoTX = new FZMath.Point2D[effLength];
        //        sXtoTY = new FZMath.Point2D[effLength];
        //        sXtoTZ = new FZMath.Point2D[effLength];

        //        sYtoX = new FZMath.Point2D[effLength];
        //        sYtoZ = new FZMath.Point2D[effLength];
        //        sYtoTX = new FZMath.Point2D[effLength];
        //        sYtoTY = new FZMath.Point2D[effLength];
        //        sYtoTZ = new FZMath.Point2D[effLength];

        //        sZtoX = new FZMath.Point2D[effLength];
        //        sZtoY = new FZMath.Point2D[effLength];
        //        sZtoTX = new FZMath.Point2D[effLength];
        //        sZtoTY = new FZMath.Point2D[effLength];
        //        sZtoTZ = new FZMath.Point2D[effLength];

        //        sTXtoTY = new FZMath.Point2D[effLength];
        //        sTXtoTZ = new FZMath.Point2D[effLength];

        //        sTYtoTX = new FZMath.Point2D[effLength];
        //        sTYtoTZ = new FZMath.Point2D[effLength];

        //        sTZtoTX = new FZMath.Point2D[effLength];
        //        sTZtoTY = new FZMath.Point2D[effLength];

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sZZ[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoY[i] = new FZMath.Point2D();
        //            sXtoZ[i] = new FZMath.Point2D();
        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();

        //            sYtoX[i] = new FZMath.Point2D();
        //            sYtoZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();

        //            sZtoX[i] = new FZMath.Point2D();
        //            sZtoY[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();

        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();

        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        for (int i = 0; i < effLength; i++)
        //            stablizedData.Add(measure[i]);
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////
        //    //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
        //    StreamWriter lwr = null;
        //    double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

        //    if (!IsRemote)
        //    {
        //        lwr = new StreamWriter(AdminPathName + "FullData.csv");
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        int k = 0;
        //        for (int i = 0; i < fullLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //                slstr += measure[i][j].ToString("F5") + ",";
        //            if (i == effIndex[k])
        //            {
        //                slstr += "*";
        //                k++;
        //            }
        //            lwr.WriteLine(slstr);
        //            if (k == effLength)
        //                break;
        //        }
        //        lwr.Close();
        //    }

        //    string calName = axis;
        //    if (isAutoCalibrationEastView)
        //    {
        //        calName += "_EastView";
        //    }

        //    string strStabilizedFile = "";
        //    if (mAutoCalibrationCount % 2 == 0)
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
        //    else
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

        //    try
        //    {
        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    catch (Exception e)
        //    {
        //        Int64 lnow = (DateTime.Now.ToBinary()) % 1000000;
        //        if (mAutoCalibrationCount % 2 == 0)
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
        //        else
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    if (lwr != null)
        //    {
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //            {
        //                //if (j < 19 || j == 22)
        //                slstr += stablizedData[i][j].ToString("F5") + ",";
        //                //else
        //                //{
        //                //    slstr += (RAD_To_MIN * stablizedData[i][j]).ToString("F5") + ",";
        //                //}
        //            }

        //            lwr.WriteLine(slstr);
        //        }
        //        lwr.Close();
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////

        //    //  axis 따라서 List 에 저장된 데이터를 처리한다.
        //    //double[] p2ndCoef = new double[3];
        //    //double[] p2ndCoef2 = new double[3];
        //    //double[] p2ndCoef3 = new double[3];
        //    int fovYoffset = GetROIY(0);
        //    string lstr = "";
        //    switch (axis)
        //    {
        //        case "Z":
        //            //  axis == "Z" : YLUT 의 경우 
        //            //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
        //            //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
        //            //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

        //            //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

        //            double a1 = 0;
        //            double b1 = 0;
        //            double a2 = 0;
        //            double b2 = 0;
        //            double a3 = 0;
        //            double b3 = 0;

        //            mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

        //            //  stablizedData[i][16] : X
        //            //  stablizedData[i][17] : Y
        //            //  stablizedData[i][18] : Z
        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
        //            double[] LUT1_tmp = new double[effLength];
        //            double[] LUT1 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

        //            LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
        //            LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

        //            //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
        //            //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
        //            //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
        //            double[] LUT2_tmp = new double[effLength];
        //            double[] LUT2 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

        //            LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
        //            LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

        //            //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
        //            //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
        //            //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
        //            //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
        //                szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
        //            double[] LUT3_tmp = new double[effLength];
        //            double[] LUT3 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

        //            LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
        //            LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

        //            //  Z scale
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sZZ[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
        //                sZZ[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZZ, effLength, ref ZtoZab);    // 2차로 변경
        //            //ZtoZab[0] = ZtoZab[0] * 0.9993; // 0.9992; // 헥사포드 Cal 변경
        //            //a = (a - 1) * 0.4 + 1; 
        //            //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
        //            //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
        //            //  Scale 만 조정해가면서 수차례 반복 필요
        //            //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


        //            if (mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView)
        //            {
        //                string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
        //                string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
        //                wr = new StreamWriter(srcFile);
        //                wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + ZtoZab[1].ToString());
        //                wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
        //                for (int i = 0; i < effLength; i++)
        //                {
        //                    wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
        //                }
        //                wr.Close();
        //                System.IO.File.Copy(srcFile, destFile, true);
        //            }

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  Z to X 계산
        //            //  Z vs X - Xprobe , Z vs Y - Yprobe
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
        //                //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
        //                sZtoX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
        //                sZtoY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage
        //                sZtoTX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sZtoTY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sZtoTZ[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }

        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoX, effLength, ref ZtoXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoY, effLength, ref ZtoYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTX, effLength, ref ZtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTY, effLength, ref ZtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTZ, effLength, ref ZtoTZab);

        //            if (!isAutoCalibrationEastView)
        //            {
        //                lstr = "ZZ Scale\t" + ZtoZab[0].ToString("E5") + ",\r\n" + ZtoZab[1].ToString("E5") + ",\t" + ZtoZab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoX\t" + ZtoXab[0].ToString("E5") + ",\t" + ZtoXab[1].ToString("E5") + ",\t" + ZtoXab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoY\t" + ZtoYab[0].ToString("E5") + ",\t" + ZtoYab[1].ToString("E5") + ",\t" + ZtoYab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoTX\t" + ZtoTXab[0].ToString("E5") + ",\t" + ZtoTXab[1].ToString("E5") + ",\t" + ZtoTXab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoTY\t" + ZtoTYab[0].ToString("E5") + ",\t" + ZtoTYab[1].ToString("E5") + ",\t" + ZtoTYab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoTZ\t" + ZtoTZab[0].ToString("E5") + ",\t" + ZtoTZab[1].ToString("E5") + ",\t" + ZtoTZab[2].ToString("E5") + "\r\n";

        //                if (mAutoCalibrationCount % 2 == 0)
        //                {
        //                    string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                    StreamReader sr = new StreamReader(scaleNthetaFile);
        //                    string allstr = sr.ReadToEnd();
        //                    sr.Close();
        //                    string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    // Z
        //                    string[] strZscaleLine = allLines[3].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[3] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZscaleLine.Length; i++)
        //                        allLines[3] += strZscaleLine[i];
        //                    // Z TO X
        //                    string[] strZtoXLine = allLines[18].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[18] = ZtoXab[0].ToString("E5") + "\t" + ZtoXab[1].ToString("E5") + "\t" + ZtoXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoXLine.Length; i++)
        //                        allLines[18] += strZtoXLine[i];
        //                    // Z TO Y
        //                    string[] strZtoYLine = allLines[19].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[19] = ZtoYab[0].ToString("E5") + "\t" + ZtoYab[1].ToString("E5") + "\t" + ZtoYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoYLine.Length; i++)
        //                        allLines[19] += strZtoYLine[i];
        //                    // Z TO TX
        //                    string[] strZtoTXLine = allLines[20].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[20] = ZtoTXab[0].ToString("E5") + "\t" + ZtoTXab[1].ToString("E5") + "\t" + ZtoTXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoTXLine.Length; i++)
        //                        allLines[20] += strZtoTXLine[i];
        //                    // Z TO TY
        //                    string[] strZtoTYLine = allLines[21].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[21] = ZtoTYab[0].ToString("E5") + "\t" + ZtoTYab[1].ToString("E5") + "\t" + ZtoTYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoTYLine.Length; i++)
        //                        allLines[21] += strZtoTYLine[i];
        //                    // Z TO TZ
        //                    string[] strZtoTZLine = allLines[22].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[22] = ZtoTZab[0].ToString("E5") + "\t" + ZtoTZab[1].ToString("E5") + "\t" + ZtoTZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoTZLine.Length; i++)
        //                        allLines[22] += strZtoTZLine[i];

        //                    wr = new StreamWriter(scaleNthetaFile);
        //                    for (int i = 0; i < allLines.Length; i++)
        //                    {
        //                        wr.WriteLine(allLines[i]);
        //                    }
        //                    wr.Close();
        //                }
        //            }


        //            //////////////////////////////////////////////////////////////////////////////////

        //            break;
        //        case "X":
        //            //  Axis = 1 : X scale 확인 및 저장
        //            //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
        //            //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
        //            //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
        //                sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값

        //                sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
        //                sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage

        //                sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sXtoTY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sXtoTZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }

        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref XtoXab);  //  A[0]X^2 + A[1]X + A[2]
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoY, effLength, ref XtoYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref XtoZab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTX, effLength, ref XtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTY, effLength, ref XtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTZ, effLength, ref XtoTZab);

        //            lstr = "XX Scale\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoY\t" + XtoYab[0].ToString("E5") + ",\t" + XtoYab[1].ToString("E5") + ",\t" + XtoYab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoZ\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTX\t" + XtoTXab[0].ToString("E5") + ",\t" + XtoTXab[1].ToString("E5") + ",\t" + XtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTY\t" + XtoTYab[0].ToString("E5") + ",\t" + XtoTYab[1].ToString("E5") + ",\t" + XtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTZ\t" + XtoTZab[0].ToString("E5") + ",\t" + XtoTZab[1].ToString("E5") + ",\t" + XtoTZab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // X
        //                string[] strXXscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[1] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXXscaleLine.Length; i++)
        //                    allLines[1] += strXXscaleLine[i];
        //                // X TO Y
        //                string[] strXtoYLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[8] = XtoYab[0].ToString("E5") + "\t" + XtoYab[1].ToString("E5") + "\t" + XtoYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoYLine.Length; i++)
        //                    allLines[8] += strXtoYLine[i];
        //                // X TO Z
        //                string[] strXtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[9] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoZLine.Length; i++)
        //                    allLines[9] += strXtoZLine[i];
        //                // X TO TX
        //                string[] strXtoTXLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[10] = XtoTXab[0].ToString("E5") + "\t" + XtoTXab[1].ToString("E5") + "\t" + XtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTXLine.Length; i++)
        //                    allLines[10] += strXtoTXLine[i];
        //                // X TO TY
        //                string[] strXtoTYLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[11] = XtoTYab[0].ToString("E5") + "\t" + XtoTYab[1].ToString("E5") + "\t" + XtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTYLine.Length; i++)
        //                    allLines[11] += strXtoTYLine[i];
        //                // X TO TZ
        //                string[] strXtoTZLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[12] = XtoTZab[0].ToString("E5") + "\t" + XtoTZab[1].ToString("E5") + "\t" + XtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTZLine.Length; i++)
        //                    allLines[12] += strXtoTZLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        case "Y":
        //            mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
        //            mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

        //            //  Axis = 2 : Y scale 확인 및 저장
        //            //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
        //                sYY[i].X = stablizedData[i][1];

        //                sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
        //                sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage

        //                sYtoTX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sYtoTY[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sYtoTZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref YtoYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoX, effLength, ref YtoXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref YtoZab);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTX, effLength, ref YtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTY, effLength, ref YtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTZ, effLength, ref YtoTZab);

        //            lstr = "YY Scale\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoX\t" + YtoXab[0].ToString("E5") + ",\t" + YtoXab[1].ToString("E5") + ",\t" + YtoXab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoZ\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";

        //            lstr += "YtoTX\t" + YtoTXab[0].ToString("E5") + ",\t" + YtoTXab[1].ToString("E5") + ",\t" + YtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoTY\t" + YtoTYab[0].ToString("E5") + ",\t" + YtoTYab[1].ToString("E5") + ",\t" + YtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoTZ\t" + YtoTZab[0].ToString("E5") + ",\t" + YtoTZab[1].ToString("E5") + ",\t" + YtoTZab[2].ToString("E5") + "\r\n";

        //            if (isAutoCalibrationEastView)
        //            {
        //                lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
        //            }

        //            wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////                    
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        //                sr.Close();
        //                if (isAutoCalibrationEastView)
        //                {
        //                    // East View Sclae
        //                    string[] strEastScaleLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[7] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
        //                    for (int i = 1; i < strEastScaleLine.Length; i++)
        //                        allLines[7] += strEastScaleLine[i];
        //                }
        //                else
        //                {
        //                    // Y
        //                    string[] strYYscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[2] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYYscaleLine.Length; i++)
        //                        allLines[2] += strYYscaleLine[i];
        //                    // Y TO X
        //                    string[] strYtoXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[13] = YtoXab[0].ToString("E5") + "\t" + YtoXab[1].ToString("E5") + "\t" + YtoXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoXLine.Length; i++)
        //                        allLines[13] += strYtoXLine[i];
        //                    // Y TO Z
        //                    string[] strYtoZLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[14] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoZLine.Length; i++)
        //                        allLines[14] += strYtoZLine[i];
        //                    // Y TO TX
        //                    string[] strYtoTXLine = allLines[15].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[15] = YtoTXab[0].ToString("E5") + "\t" + YtoTXab[1].ToString("E5") + "\t" + YtoTXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTXLine.Length; i++)
        //                        allLines[15] += strYtoTXLine[i];
        //                    // Y TO TY
        //                    string[] strYtoTYLine = allLines[16].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[16] = YtoTYab[0].ToString("E5") + "\t" + YtoTYab[1].ToString("E5") + "\t" + YtoTYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTYLine.Length; i++)
        //                        allLines[16] += strYtoTYLine[i];
        //                    // Y TO TZ
        //                    string[] strYtoTZLine = allLines[17].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[17] = YtoTZab[0].ToString("E5") + "\t" + YtoTZab[1].ToString("E5") + "\t" + YtoTZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTZLine.Length; i++)
        //                        allLines[17] += strYtoTZLine[i];
        //                }

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
        //        case "TX":
        //            //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
        //            //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTXTX[i].Y = stablizedData[i][19]; // * RAD_To_MIN;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTXTX[i].X = stablizedData[i][3];

        //                sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref TXtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXtoTY, effLength, ref TXtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXtoTZ, effLength, ref TXtoTZab);

        //            lstr += "TX Scale\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "TXtoTY\t" + TXtoTYab[0].ToString("E5") + ",\t" + TXtoTYab[1].ToString("E5") + ",\t" + TXtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "TXtoTZ\t" + TXtoTZab[0].ToString("E5") + ",\t" + TXtoTZab[1].ToString("E5") + ",\t" + TXtoTZab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // TX
        //                string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXscaleLine.Length; i++)
        //                    allLines[4] += strTXscaleLine[i];
        //                // TX TO TY
        //                string[] strTXtoTYLine = allLines[23].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[23] = TXtoTYab[0].ToString("E5") + "\t" + TXtoTYab[1].ToString("E5") + "\t" + TXtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXtoTYLine.Length; i++)
        //                    allLines[23] += strTXtoTYLine[i];
        //                // TX TO TZ
        //                string[] strTXtoTZLine = allLines[24].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[24] = TXtoTZab[0].ToString("E5") + "\t" + TXtoTZab[1].ToString("E5") + "\t" + TXtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXtoTZLine.Length; i++)
        //                    allLines[24] += strTXtoTZLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        case "TY":
        //            //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
        //            //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTYTY[i].Y = stablizedData[i][20]; // * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTYTY[i].X = stablizedData[i][4];

        //                sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][3] - stablizedData[i][19]);
        //                sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][5] - stablizedData[i][21]);
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYTY, effLength, ref TYtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYtoTX, effLength, ref TYtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYtoTZ, effLength, ref TYtoTZab);

        //            lstr += "TY Scale\t" + TYtoTYab[0].ToString("E5") + ",\t" + TYtoTYab[1].ToString("E5") + ",\t" + TYtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "TYtoTX\t" + TYtoTXab[0].ToString("E5") + ",\t" + TYtoTXab[1].ToString("E5") + ",\t" + TYtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "TYtoTZ\t" + TYtoTZab[0].ToString("E5") + ",\t" + TYtoTZab[1].ToString("E5") + ",\t" + TYtoTZab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // TY
        //                string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[5] = TYtoTYab[0].ToString("E5") + "\t" + TYtoTYab[1].ToString("E5") + "\t" + TYtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYscaleLine.Length; i++)
        //                    allLines[5] += strTYscaleLine[i];
        //                // TY TO TX
        //                string[] strTYtoTXLine = allLines[25].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[25] = TYtoTXab[0].ToString("E5") + "\t" + TYtoTXab[1].ToString("E5") + "\t" + TYtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYtoTXLine.Length; i++)
        //                    allLines[25] += strTYtoTXLine[i];
        //                // TY TO TZ
        //                string[] strTYtoTZLine = allLines[26].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[26] = TYtoTZab[0].ToString("E5") + "\t" + TYtoTZab[1].ToString("E5") + "\t" + TYtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYtoTZLine.Length; i++)
        //                    allLines[26] += strTYtoTZLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        case "TZ":
        //            //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
        //            //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTZTZ[i].Y = stablizedData[i][21]; // * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTZTZ[i].X = stablizedData[i][5];

        //                sTZtoTX[i] = new FZMath.Point2D(sTZTZ[i].X, stablizedData[i][3] - stablizedData[i][19]);
        //                sTZtoTY[i] = new FZMath.Point2D(sTZTZ[i].X, stablizedData[i][4] - stablizedData[i][20]);
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZTZ, effLength, ref TZtoTZab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZtoTX, effLength, ref TZtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZtoTY, effLength, ref TZtoTYab);

        //            lstr += "TZ Scale\t" + TZtoTZab[0].ToString("E5") + ",\t" + TZtoTZab[1].ToString("E5") + ",\t" + TZtoTZab[2].ToString("E5") + "\r\n";
        //            lstr += "TZtoTX\t" + TZtoTXab[0].ToString("E5") + ",\t" + TZtoTXab[1].ToString("E5") + ",\t" + TZtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "TZtoTY\t" + TZtoTYab[0].ToString("E5") + ",\t" + TZtoTYab[1].ToString("E5") + ",\t" + TZtoTYab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // TZ
        //                string[] strTZscaleLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[6] = TZtoTZab[0].ToString("E5") + "\t" + TZtoTZab[1].ToString("E5") + "\t" + TZtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTZscaleLine.Length; i++)
        //                    allLines[6] += strTZscaleLine[i];
        //                // TZ TO TX
        //                string[] strTZtoTXLine = allLines[27].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[27] = TZtoTXab[0].ToString("E5") + "\t" + TZtoTXab[1].ToString("E5") + "\t" + TZtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTZtoTXLine.Length; i++)
        //                    allLines[27] += strTZtoTXLine[i];
        //                // TZ TO TY
        //                string[] strTZtoTYLine = allLines[28].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[28] = TZtoTYab[0].ToString("E5") + "\t" + TZtoTYab[1].ToString("E5") + "\t" + TZtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTZtoTYLine.Length; i++)
        //                    allLines[28] += strTZtoTYLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            tbCalibration.Text += lstr;
        //        });
        //    else
        //        tbCalibration.Text += lstr;

        //}

        //public enum Axis { X, Y, Z, TX, TY, TZ }

        //private void btnSingleCal_Click(object sender, EventArgs e)
        //{
        //    if (!mAutoCalibrationRun)
        //    {
        //        mAutoCalibrationRun = true;
        //        btnSingleCal.Text = "Stop Single Cal.";
        //    }
        //    else
        //    {
        //        mAutoCalibrationRun = false;
        //        btnSingleCal.Text = "Single Cal.";
        //        return;
        //    }

        //    mAutoCalibrationCount = 1;  // Sngle Cal.은 수동으로 적용하고 결과 확인용
        //    tbCalibration.Text = "";

        //    isAutoCalibrationEastView = false;
        //    if (rbCalEastView.Checked)
        //    {
        //        // East View Translation (Z -> Y)
        //        isAutoCalibrationEastView = true;

        //        // East View에서 Z Oneway Stroke
        //        double zOnewayStroke = 1750;
        //        if (tbZMaxStroke.Text.Length > 1)
        //            zOnewayStroke = double.Parse(tbZMaxStroke.Text);

        //        // East View에서 Y Oneway Stroke
        //        double yOnewayStroke = 1900;
        //        if (tbMaxStroke.Text.Length > 1)
        //            yOnewayStroke = double.Parse(tbMaxStroke.Text);


        //        //LoadscaleNTheta();
        //        LoadScaleNTheta();

        //        // 241206 YLUT 적요안함.
        //        // m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);

        //        Task.Run(() =>
        //        {
        //            // Z Translation
        //            mCalibrationFullData.Clear();
        //            mGageFullData.Clear();
        //            AutoCalibrationOld(Axis.Z, zOnewayStroke);
        //            // Y Translation
        //            mCalibrationFullData.Clear();
        //            mGageFullData.Clear();
        //            AutoCalibrationOld(Axis.Y, yOnewayStroke);

        //            mAutoCalibrationRun = false;
        //            this.Invoke(new Action(() =>
        //            {
        //                btnSingleCal.Text = "Single Cal.";
        //            }));
        //        });
        //    }
        //    else
        //    {
        //        // Selected Aixs Translation
        //        Axis axis;
        //        if (rbCalZ.Checked)
        //            axis = Axis.Z;
        //        else if (rbCalX.Checked)
        //            axis = Axis.X;
        //        else if (rbCalY.Checked)
        //            axis = Axis.Y;
        //        else if (rbCalTX.Checked)
        //            axis = Axis.TX;
        //        else if (rbCalTY.Checked)
        //            axis = Axis.TY;
        //        else if (rbCalTZ.Checked)
        //            axis = Axis.TZ;
        //        else
        //            return;

        //        // One Way Stroke (X,Y,Z :um), (TX,TY,TZ : ?)
        //        double onewayStroke = 1900;
        //        if (tbMaxStroke.Text.Length > 1)
        //            onewayStroke = double.Parse(tbMaxStroke.Text);


        //        LoadScaleNTheta();
        //        // 241206 YLUT 적용안함.
        //        //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
        //        Task.Run(() =>
        //        {
        //            mCalibrationFullData.Clear();
        //            AutoCalibrationOld(axis, onewayStroke);

        //            mAutoCalibrationRun = false;
        //            this.Invoke(new Action(() =>
        //            {
        //                btnSingleCal.Text = "Single Cal.";
        //            }));
        //        });
        //    }
        //}

        private void btnCheckFovBalance_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].GrabB(0, true);
            m__G.oCam[0].ShowBalancingImage();
            SetDefaultMarkConfig(true);

        }
        bool IsLiveCropStop = false;
        private void button10_Click_1(object sender, EventArgs e)
        {
            IsLiveCropStop = false;
            m__G.oCam[0].DrawAllRectangles();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    if (pictureBox2.InvokeRequired)
                        BeginInvoke((MethodInvoker)delegate
                        {
                            pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].LoadCropImgFromLive(0, m__G.oCam[0].mbDrawReference));
                        });
                    else
                        pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].LoadCropImgFromLive(0, m__G.oCam[0].mbDrawReference));

                    if (IsLiveCropStop)
                        break;
                }

            });
        }



        private void button12_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].DrawClear();
            IsLiveCropStop = true;
        }


        #region Crop Pos
        private (int, int) CheckSelectedPos()
        {
            int index;
            if (rdoPosA.Checked) index = 1;
            else if (rdoPosB.Checked) index = 0;
            else index = 2;

            int step = 1;
            if (chkPixel5.Checked) step = 5;

            return (index, step);
        }

        private void btnUpPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].UpPos(index, step);
        }
        private void btnDownPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].DownPos(index, step);
        }
        private void btnLeftPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].LeftPos(index, step);
        }

        private void btnRightPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].RightPos(index, step);
        }

        private int CropABgap
        {
            get
            {
                return m__G.oCam[0].CropABgap;
            }
        }
        private int CropCgap
        {
            get
            {
                try
                {
                    return Convert.ToInt32(tbDistancePosCD.Text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return m__G.oCam[0].CropCgap;
                }
            }
            set
            {
                if (tbDistancePosCD.InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbDistancePosCD.Text = value.ToString();
                    });
                else
                    tbDistancePosCD.Text = value.ToString();
            }
        }

        private void btnWidenPosCD_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (chkPixel5.Checked) step = 5;

            m__G.oCam[0].WidenPos(step);
            CropCgap = m__G.oCam[0].CropCgap;
        }

        private void btnNarrowPosCD_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (chkPixel5.Checked) step = 5;

            m__G.oCam[0].NarrowPos(step);
            CropCgap = m__G.oCam[0].CropCgap;
        }


        private void tbDistancePosCD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                m__G.oCam[0].AdjustDistancePos(CropCgap);
                CropCgap = m__G.oCam[0].CropCgap;
            }
        }

        private void tbDistancePosCD_Leave(object sender, EventArgs e)
        {
            m__G.oCam[0].AdjustDistancePos(CropCgap);
            CropCgap = m__G.oCam[0].CropCgap;
        }
        #endregion

        private void rdoPosA_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            m__G.oCam[0].SaveCropPosToXml();
            groupBox4.Hide();
            btnChangeCrop.Show();
        }

        private void btnChangeCrop_Click(object sender, EventArgs e)
        {
            btnChangeCrop.Hide();
            groupBox4.Show();
        }

        private void cbDrawReference_CheckedChanged(object sender, EventArgs e)
        {
            System.Drawing.Point[] markPos = null;

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);        //  CropGap 이 적용되지 않은 상태의 결과를 반환한다.
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);   //  CropGap 이 적용되지 않은 상태의 데이터
            m__G.mFAL.SetMarkNorm();
            m__G.oCam[0].mbDrawReference = cbDrawReference.Checked;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void PogoPinUnloadBtn_Click(object sender, EventArgs e)
        {
            //m__G.fGraph.mDriverIC.SocketTest(0, false);
        }

        private void BaseDownBtn_Click(object sender, EventArgs e)
        {
            //m__G.fGraph.mDriverIC.SocketTest(2, false);
            //m__G.fGraph.mDriverIC.SocketTest(0, false);
            //Thread.Sleep(300);

            //m__G.fGraph.mDriverIC.SocketTest(1, false);
        }

        private void SidePushUnloadBtn_Click(object sender, EventArgs e)
        {
            //m__G.fGraph.mDriverIC.SocketTest(2, false);
        }

        private void PogoPinloadBtn_Click(object sender, EventArgs e)
        {
            //m__G.fGraph.mDriverIC.SocketTest(0, true);
        }

        private void SidePushloadBtn_Click(object sender, EventArgs e)
        {
            //m__G.fGraph.mDriverIC.SocketTest(2, true);
        }

        private void BaseUpBtn_Click(object sender, EventArgs e)
        {
            //m__G.fGraph.mDriverIC.SocketTest(2, false);
            //m__G.fGraph.mDriverIC.SocketTest(0, false);
            //Thread.Sleep(300);

            //m__G.fGraph.mDriverIC.SocketTest(1, true);
        }

        private void MotionStageBtn_Click(object sender, EventArgs e)
        {
            //if (m__G.mMotion == null)
            //{
            //    return;
            //}
            ////m__G.mMotion.Show();
            ////m__G.mMotion.BringToFront();
            ///
            //if (m__G.f_PIMotion == null) return;

            //m__G.f_PIMotion.Show();
            //m__G.f_PIMotion.BringToFront();

         
        }

        public int mAutoCalibrationCount = 0;
        public bool isAutoCalibrationEastView = false;
        bool motorizedMeasurementRun = false;
        bool motorizedMeasurementAbort = false;

        public void InitializeScaleNTheta()
        {
            ms_scaleX = new double[3] { 0, 1, 0 };
            ms_scaleY = new double[3] { 0, 1, 0 };
            ms_scaleZ = new double[3] { 0, 1, 0 };
            ms_scaleTX = new double[3] { 0, 1, 0 };
            ms_scaleTY = new double[3] { 0, 1, 0 };
            ms_scaleTZ = new double[3] { 0, 1, 0 };
            ms_EastViewYPscale = 1.0;
            ms_XtoYbyView = new double[3];
            ms_XtoZbyView = new double[3];
            ms_XtoTXbyView = new double[3];
            ms_XtoTYbyView = new double[3];
            ms_XtoTZbyView = new double[3];
            ms_YtoXbyView = new double[3];
            ms_YtoZbyView = new double[3];
            ms_YtoTXbyView = new double[3];
            ms_YtoTYbyView = new double[3];
            ms_YtoTZbyView = new double[3];
            ms_ZtoXbyView = new double[3];
            ms_ZtoYbyView = new double[3];
            ms_ZtoTXbyView = new double[3];
            ms_ZtoTYbyView = new double[3];
            ms_ZtoTZbyView = new double[3];
            ms_TXtoTYbyView = new double[3];
            ms_TXtoTZbyView = new double[3];
            ms_TYtoTXbyView = new double[3];
            ms_TYtoTZbyView = new double[3];
            ms_TZtoTXbyView = new double[3];
            ms_TZtoTYbyView = new double[3];
            ms_XJtoXbyView = new double[2];
            ms_YJtoYbyView = new double[2];
            ms_ZJtoZbyView = new double[2];
            ms_TZtoZbyView = new double[3];
        }
        //public void AutoCalibrationWrapperOld(Axis axis, double onewayStrokeUm)
        //{
        //    mCalibrationFullData.Clear();
        //    AutoCalibrationOld(axis, onewayStrokeUm);

        //    mAutoCalibrationCount++;

        //    mCalibrationFullData.Clear();
        //    string smgzYLUT = "";
        //    if (LoadScaleNTheta())
        //        smgzYLUT = "ScaleNTheta" + m__G.mCamID0.ToString() + " is loaded\r\n";
        //    else
        //        smgzYLUT = "Failt to load ScaleNTheta" + m__G.mCamID0.ToString() + " \r\n";

        //    // 241206  YLUT 적용 안함.
        //    //if (m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]))
        //    //    smgzYLUT += "YLUT" + m__G.mCamID0.ToString() + " is loaded\r\n";
        //    //else
        //    //    smgzYLUT += "Failt to load YLUT" + m__G.mCamID0.ToString() + " \r\n";

        //    if (tbInfo.InvokeRequired)
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            tbInfo.Text += smgzYLUT;
        //        });
        //    else
        //        tbInfo.Text += smgzYLUT;


        //    AutoCalibrationOld(axis, onewayStrokeUm);
        //}

        //public void AutoCalibrationEastView(double yOnewayStrokeUm, double zOnewayStrokeUm)
        //{
        //    mCalibrationFullData.Clear();
        //    AutoCalibrationOld(Axis.Z, zOnewayStrokeUm);
        //    mCalibrationFullData.Clear();
        //    AutoCalibrationOld(Axis.Y, yOnewayStrokeUm);

        //    mAutoCalibrationCount++;

        //    mCalibrationFullData.Clear();
        //    LoadScaleNTheta();
        //    // 241206 YLUT 적용안함.
        //    //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
        //    AutoCalibrationOld(Axis.Z, zOnewayStrokeUm);
        //    mCalibrationFullData.Clear();
        //    AutoCalibrationOld(Axis.Y, yOnewayStrokeUm);
        //}


        bool mbUseZ123 = false;
        //public void AutoCalibrationOld(Axis axis, double onewayStrokeUm)
        //{
        //    if (!mAutoCalibrationRun)
        //        return;

        //    if (axis == Axis.Z)
        //        mbUseZ123 = true;
        //    else
        //        mbUseZ123 = false;

        //    //  초기위치가 ORG 인 것으로 가정한다.
        //    mAutoCalibrationIndex = 0;



        //    //if (m__G.mGageCounter != null)
        //    //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

        //    // 이동 속도 Nomal로 설정
        //    MotorSetSpeed6D(SpeedLevel.Normal);
        //    // PI Motion 단위 mm
        //    //double onewayStrokePulse = onewayStrokeUm * 0.001;    (onewayStrokeUm = mm)

        //    // SK Motion 단위 0.01um
        //    // double onewayStrokePulse = onewayStrokeUm  * 100

        //    // SK Motion, PI Motion 단위 um로 통일.
        //    // double onewayStrokePulse = 필요없어짐.

        //    MotorMoveOriginHexapod();
        //    MotorSetPivot(0, 0, 0);
        //    if (LoadOQCcondition())
        //    {
        //        MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //    }
        //    else
        //        MotorMoveHome6D();

        //    LoadPivotXYZ();
        //    LoadFidorg();
        //    SingleFindMark();   // 초기(현재) 위치에서 측정
        //    mGageFullData.Clear();
        //    mCalibrationFullData.Clear();
        //    switch (axis)
        //    {
        //        case Axis.TX:
        //            FindPivot(1);
        //            ChangePivotXYZ(1);  //  저장
        //            MotorMoveOriginHexapod();
        //            MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);
        //            MotorSetHCS(0, 0, mHCSrotation.Z);
        //            mGageFullData.Clear();
        //            mCalibrationFullData.Clear();
        //            break;
        //        case Axis.TY:
        //            FindPivot(2);
        //            ChangePivotXYZ(2);  //  저장
        //            MotorMoveOriginHexapod();
        //            MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
        //            MotorSetHCS(0, 0, mHCSrotation.Z);
        //            mGageFullData.Clear();
        //            mCalibrationFullData.Clear();
        //            break;
        //        case Axis.TZ:
        //            ////////////////////////////////////////////////////////////
        //            /// 2024.12.21
        //            /// 앞선 Calibration 에 의해 Z pivot 이 달라졌을 수 있음에 따라 Z pivot 을 다시 마크 중심으로 옮긴 뒤 Calibration 한다.
        //            FindPivot(3);
        //            ChangePivotXYZ(3);  //  저장
        //            ////////////////////////////////////////////////////////////
        //            MotorMoveOriginHexapod();
        //            MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
        //            MotorSetHCS(0, 0, mHCSrotation.Z);
        //            mGageFullData.Clear();
        //            mCalibrationFullData.Clear();
        //            break;
        //        default:
        //            break;
        //    }

        //    string hexPosFile = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\HexPos.csv";
        //    double[] HexCurPos = MotorCurPosHexapod();
        //    string strHexCurPos = $"Before Calibration,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
        //    File.AppendAllText(hexPosFile, strHexCurPos);


        //    double orgPos = MotorCurPosAxis(axis);    // 현재 위치 um
        //    SingleFindMark();   // 초기(현재) 위치에서 측정

        //    if (!mAutoCalibrationRun) { return; }
        //    double pos = orgPos - (onewayStrokeUm) / 3;     // 이동할 위치 um
        //    MotorMoveAbsAxis(axis, pos);   // 이동
        //    Thread.Sleep(300);  // 스테이지 안정화 될때까지 기다리기
        //    SingleFindMark();   // 측정

        //    if (!mAutoCalibrationRun) { return; }
        //    pos = orgPos - 2 * onewayStrokeUm / 3;
        //    MotorMoveAbsAxis(axis, pos);
        //    Thread.Sleep(300);
        //    SingleFindMark();


        //    if (!mAutoCalibrationRun) { return; }
        //    if (axis < Axis.TX)
        //    {
        //        pos = orgPos - onewayStrokeUm - 300;
        //    }
        //    else
        //    {
        //        pos = orgPos - onewayStrokeUm - 30;
        //    }
        //    MotorMoveAbsAxis(axis, pos);
        //    Thread.Sleep(200);

        //    if (axis < Axis.TX)
        //    {
        //        pos = orgPos - onewayStrokeUm - 150;
        //    }
        //    else
        //    {
        //        pos = orgPos - onewayStrokeUm - 15;
        //    }
        //    MotorMoveAbsAxis(axis, pos);
        //    Thread.Sleep(200);

        //    if (!mAutoCalibrationRun) { return; }
        //    if (axis < Axis.TX)
        //    {
        //        pos = orgPos - onewayStrokeUm - 10;
        //    }
        //    else
        //    {
        //        pos = orgPos - onewayStrokeUm - 6;
        //    }
        //    MotorMoveAbsAxis(axis, pos);
        //    Thread.Sleep(300);

        //    if (!mAutoCalibrationRun) { return; }
        //    SingleFindMark();
        //    Thread.Sleep(200);
        //    SingleFindMark();


        //    // 진짜 측정 시작
        //    if (!mAutoCalibrationRun) { return; }
        //    pos = orgPos - onewayStrokeUm;

        //    //MotorMoveAbsAxis(axis, pos);
        //    switch (axis)
        //    {
        //        case Axis.X:
        //            MotorMoveAbs6D(pos, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //            break;
        //        case Axis.Y:
        //            MotorMoveAbs6D(mCSHorg.X, pos, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //            break;
        //        case Axis.Z:
        //            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, pos, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //            break;
        //        case Axis.TX:
        //            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, pos, 0, 0);  //  Move TX (arcmin)
        //            break;
        //        case Axis.TY:
        //            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, pos, 0);  //  Move TX (arcmin)
        //            break;
        //        case Axis.TZ:
        //            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, pos);  //  Move TX (arcmin)
        //            break;
        //    }

        //    if (axis == Axis.Y)
        //        Thread.Sleep(600);
        //    else if (axis == Axis.X || axis == Axis.Z)
        //        Thread.Sleep(600);
        //    else
        //        Thread.Sleep(200);
        //    SingleFindMark();

        //    // double dStrokeUm = onewayStrokePulse / (onewayStrokeUm * 0.01);  // 헥사포드
        //    double dStrokeUm;
        //    if (axis < Axis.TX)
        //    {
        //        // X, Y, Z => 100um
        //        dStrokeUm = 100; // 50um
        //    }
        //    else
        //    {
        //        // TX, TY, TZ => min
        //        dStrokeUm = 12;  // 0.1 deg -> 6 min  // TX : 0.2 deg -> 12min
        //    }

        //    double movingStroke = -onewayStrokeUm;
        //    while (movingStroke < onewayStrokeUm)
        //    {
        //        if (!mAutoCalibrationRun) { return; }

        //        pos += dStrokeUm;
        //        movingStroke += dStrokeUm;
        //        //MotorMoveAbsAxis(axis, pos);
        //        switch (axis)
        //        {
        //            case Axis.X:
        //                MotorMoveAbs6D(pos, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //                break;
        //            case Axis.Y:
        //                MotorMoveAbs6D(mCSHorg.X, pos, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //                break;
        //            case Axis.Z:
        //                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, pos, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
        //                break;
        //            case Axis.TX:
        //                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, pos, 0, 0);  //  Move TX (arcmin)
        //                break;
        //            case Axis.TY:
        //                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, pos, 0);  //  Move TX (arcmin)
        //                break;
        //            case Axis.TZ:
        //                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, pos);  //  Move TX (arcmin)
        //                break;
        //        }
        //        if (axis == Axis.Y)
        //            Thread.Sleep(600);
        //        else if (axis == Axis.X || axis == Axis.Z)
        //            Thread.Sleep(600);
        //        else
        //            Thread.Sleep(200);
        //        SingleFindMark();
        //    }

        //    //if (m__G.mGageCounter != null)
        //    //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

        //    // Home 위치로 복귀
        //    MotorMoveAbsAxis(axis, orgPos);
        //    MotorMoveOriginHexapod();

        //    HexCurPos = MotorCurPosHexapod();
        //    strHexCurPos = $"After Calibration,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
        //    File.AppendAllText(hexPosFile, strHexCurPos);
        //    //double zpos = MotorCurPosAxis(iAxis);

        //    string sAxis = axis.ToString();
        //    RemoteCalibration(sAxis, 5);
        //}


        // 첫 Cal
 
  
    
    
  
    
       
    
        public void AppendMeasuredData(List<List<double[]>> stabilizedDataList, string fileName)
        {
            // if (collectedData == null || collectedData.Count == 0) return null;
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;

            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return;
            }

            string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            if (!Directory.Exists(AdminPathName))
                Directory.CreateDirectory(AdminPathName);

            // 결과 파일 저장
            string strStabilizedFile = $"{AdminPathName}StabilizedData_{camID0}_{fileName}.csv";

            string slstr = "";
            if (!File.Exists(strStabilizedFile))
            {
                if (!m__G.m_bPrismCS)
                    slstr += "#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ\r\n";
                else
                    slstr += "#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ,prismTX,prismTY,prismTZ,pprismTX,pprismTY,pprismTZ,epTX,epTY,epTZ\r\n";
            }

            double[] lProbePrismTXTYTZ = new double[3];
            double[] lErrorPrismTXTYTZ = new double[3];
            for (int i = 0; i < stabilizedDataList.Count; i++)
            {
                List<double[]> stabilizedData = stabilizedDataList[i];
                if (stabilizedData != null)
                {
                    for (int j = 0; j < stabilizedData.Count; j++)
                    {
                        slstr += j.ToString() + "," +
                                   string.Join(",", stabilizedData[j].Take(22)
                                   .Select(x => x.ToString("F5"))) + ",";

                        slstr += (stabilizedData[j][0] - stabilizedData[j][16]).ToString("F5") + "," +
                                 (stabilizedData[j][1] - stabilizedData[j][17]).ToString("F5") + "," +
                                 (stabilizedData[j][2] - stabilizedData[j][18]).ToString("F5") + "," +
                                 (stabilizedData[j][3] - stabilizedData[j][19]).ToString("F5") + "," +
                                 (stabilizedData[j][4] - stabilizedData[j][20]).ToString("F5") + "," +
                                 (stabilizedData[j][5] - stabilizedData[j][21]).ToString("F5") + ",";

                        if (m__G.m_bPrismCS)
                        {
                            lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(stabilizedData[j][19], stabilizedData[j][20], stabilizedData[j][21], true, true);
                            lProbePrismTXTYTZ[1] = stabilizedData[j][19];   //  원래 있어야하는 코드

                            lErrorPrismTXTYTZ[0] = mPrismTXTYTZ[j][0] - lProbePrismTXTYTZ[0];
                            lErrorPrismTXTYTZ[1] = mPrismTXTYTZ[j][1] - lProbePrismTXTYTZ[1];  //  CSH_Prism_TY - Probe_TX
                            lErrorPrismTXTYTZ[2] = mPrismTXTYTZ[j][2] - lProbePrismTXTYTZ[2];

                            slstr += mPrismTXTYTZ[j][0].ToString("F5") + "," + mPrismTXTYTZ[j][1].ToString("F5") + "," + mPrismTXTYTZ[j][2].ToString("F5") + "," +
                                     lProbePrismTXTYTZ[0].ToString("F5") + "," + lProbePrismTXTYTZ[1].ToString("F5") + "," + lProbePrismTXTYTZ[2].ToString("F5") + "," +
                                     lErrorPrismTXTYTZ[0].ToString("F5") + "," + lErrorPrismTXTYTZ[1].ToString("F5") + "," + lErrorPrismTXTYTZ[2].ToString("F5") + ",";

                            if (j == 0)
                            {
                                slstr += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",";
                            }

                        }

                        slstr += "\r\n";
                    }
                }
            }
            File.AppendAllText(strStabilizedFile, slstr);

        }

        public bool mbMotorizedStage = false;

      

        private void btnSaveOrgPosition_Click(object sender, EventArgs e)
        {
          
        }

        private void btnGobackOrg_Click(object sender, EventArgs e)
        {
           
        }

     
        public struct VolumetricTP
        {
            public Point3d Pt;
            public bool bSubOn;
            public VolumetricTP(double x, double y, double z, bool subOn = true)
            {
                Pt.X = x; Pt.Y = y; Pt.Z = z;
                bSubOn = subOn;
            }
        }
        public struct VolumetricTP6D
        {
            public double X;
            public double Y;
            public double Z;
            public double TX;
            public double TY;
            public double TZ;
            public int pivotAxis;   //  0 는 측정, 1은 TX, 2 는 TY, 3 은 TZ
            //  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
            //  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
            //  pivotAxis 가 0 이면 이동후 측정한다.
            public VolumetricTP6D(double x, double y, double z, double tx, double ty, double tz, int pivot = 0)
            {
                X = x; Y = y; Z = z; TX = tx; TY = ty; TZ = tz;
                pivotAxis = pivot;

            }
        }
        public VolumetricTP[] mVMPts = null;
        public VolumetricTP[] mVMTPts = null;

        public int mAutoFullRange = 0;  // 0 ~ 100, 0 은 Auto 가 아닌 경우
        private void btnApplyVolumetricMeasure_Click(object sender, EventArgs e)
        {
            mAutoFullRange = 0;
            ApplyVolumetricMeasureOld();
        }

        public VolumetricTP6D[] mVMPts6d = null;
        public void ApplyVolumetricMeasureOld()
        {
            double timeEst = 0;
            double[][] tpList = new double[15][];

            ////////////////////////////////////////////////////////////////////////////////////////
            //  Hybrid Stage
            double[] tpX = new double[11] { -1400, -1350, -1300, -1250, -1200, -1150, -1100, -1050, -1000, -950, 0 };

            //	Y		Z		T1		T2		T3		T4		T5	
            // 아예 회전이 없는 경우

            tpList[0] = new double[5] { -0.080, -0.070, 0, 0, 0 };
            tpList[1] = new double[5] { -0.040, -0.070, 0, 0, 0 };
            tpList[2] = new double[5] { 0, -0.070, 0, 0, 0 };
            tpList[3] = new double[5] { 0.040, -0.070, 0, 0, 0 };
            tpList[4] = new double[5] { 0.080, -0.070, 0, 0, 0 };

            tpList[5] = new double[5] { -0.0140, 0, 0, 0, 0 };
            tpList[6] = new double[5] { -0.070, 0, 0, 0, 0 };
            tpList[7] = new double[5] { 0, 0, 0, 0, 0 };
            tpList[8] = new double[5] { 0.070, 0, 0, 0, 0 };
            tpList[9] = new double[5] { 0.0140, 0, 0, 0, 0 };

            tpList[10] = new double[5] { -0.080, 0.070, 0, 0, 0 };
            tpList[11] = new double[5] { -0.040, 0.070, 0, 0, 0 };
            tpList[12] = new double[5] { 0, 0.070, 0, 0, 0 };
            tpList[13] = new double[5] { 0.040, 0.070, 0, 0, 0 };
            tpList[14] = new double[5] { 0.080, 0.070, 0, 0, 0 };

            //tpList[0] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            //tpList[1] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            //tpList[2] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            //tpList[3] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            //tpList[4] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            //tpList[5] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            //tpList[6] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            //tpList[7] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            //tpList[8] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            //tpList[9] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            //tpList[10] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            //tpList[11] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            //tpList[12] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            //tpList[13] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            //tpList[14] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };


            List<VolumetricTP6D> lmVMPts6d = new List<VolumetricTP6D>();
            VolumetricTP6D lpmid = new VolumetricTP6D();
            double tXprev = 0;
            int tpListLength = 3;
            int tpListLast = tpListLength - 1;
            for (int i = 0; i < 11; i++)
            {
                //lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) / 3, tpList[0][0] / 3, tpList[0][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                //lmVMPts6d.Add(lpmid);
                //lpmid = new VolumetricTP6D(tXprev + 2 * (tpX[i] - tXprev) / 3, 2 * tpList[0][0] / 3, 2 * tpList[0][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                //lmVMPts6d.Add(lpmid);
                if (i == 0)
                {
                    lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) / 3, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                    lpmid = new VolumetricTP6D(tXprev + 2 * (tpX[i] - tXprev) / 3, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                    lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) - 300, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                    lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) - 10, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                }

                //for (int j = 0; j < tpList.Length; j++)
                for (int j = 0; j < 1; j++)
                {
                    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0);
                    lmVMPts6d.Add(lp1);
                    if (j == 5 || j == 10)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  TX
                    //VolumetricTP6D lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 1);
                    //lmVMPts6d.Add(lpmid1);
                    //lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][2] / 2, 0, 0, -1);
                    //lmVMPts6d.Add(lpmid1);
                    //for (int ti = 2; ti < tpListLength; ti++)
                    //{
                    //    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][ti], 0, 0);
                    //    lmVMPts6d.Add(lp1);
                    //}
                    //VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][tpListLast] / 2, 0, 0, -1);
                    //lmVMPts6d.Add(lpmid2);
                    //  End of TX
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////  TY
                    //////lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 2);
                    //////lmVMPts6d.Add(lpmid2);
                    //////lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][2] / 2, 0, -1);
                    //////lmVMPts6d.Add(lpmid2);
                    //////for (int ti = 2; ti < tpListLength; ti++)
                    //////{
                    //////    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][ti], 0);
                    //////    lmVMPts6d.Add(lp1);
                    //////}
                    //////VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][tpListLast] / 2, 0, -1);
                    //////lmVMPts6d.Add(lpmid3);
                    ////////  End of TX
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////  TZ
                    //////lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 3);
                    //////lmVMPts6d.Add(lpmid3);
                    //////lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][2] / 2, -1);
                    //////lmVMPts6d.Add(lpmid3);
                    //////for (int ti = 2; ti < tpListLength; ti++)
                    //////{
                    //////    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][ti]);
                    //////    lmVMPts6d.Add(lp1);
                    //////}

                    if (j == 4 || j == 9 || j == 14)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 2 * tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                }
                //lpmid = new VolumetricTP6D((2 * tpX[i] + tpX[i + 1]) / 3, 0, 0, 0, 0, 0, -3);
                //tXprev = (2 * tpX[i] + tpX[i + 1]) / 3;
                //lmVMPts6d.Add(lpmid);
            }
            lpmid = new VolumetricTP6D(tXprev / 2, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\" + "XYZ.csv");
            for (int i = 0; i < mVMPts6d.Length; i++)
            {
                wr.WriteLine(mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + "," + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + "," + mVMPts6d[i].pivotAxis.ToString("F0"));
            }
            wr.Close();


            tbVsnLog.Text = (mVMPts6d.Length).ToString() + " points.";
        }

        public void ApplyVolumetricMeasure()
        {
            double timeEst = 0;
            double[][] tpList = new double[21][];

            ////////////////////////////////////////////////////////////////////////////////////////
            //  Hybrid Stage
            double[] tpX = new double[10] { -550, -1100, -1700, -1550, -1400, -700, 0, 700, 1400, 0 };
            //double[] tpX = new double[10] { 550, 1100, 1700, 1550, 1400, 700, 0, -700, -1400, 0 }; // 거꾸로

            //	Y		Z		T1		T2		T3		T4		T5	
            // y 거꾸로
            tpList[0] = new double[7] { 1000, -800, 0, 0, 0, 0, 0 };
            tpList[1] = new double[7] { 900, -750, 0, 0, 0, 0, 0 };
            tpList[2] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };
            tpList[3] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            tpList[4] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            tpList[5] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            tpList[6] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };

            tpList[7] = new double[7] { 1600, 0, 0, 0, 0, 0, 0 };
            tpList[8] = new double[7] { 1500, 0, 0, 0, 0, 0, 0 };
            tpList[9] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };
            tpList[10] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            tpList[11] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            tpList[12] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            tpList[13] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };

            tpList[14] = new double[7] { 1000, 700, 0, 0, 0, 0, 0 };
            tpList[15] = new double[7] { 900, 700, 0, 0, 0, 0, 0 };
            tpList[16] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };
            tpList[17] = new double[7] { 400, 700, -76, -38, 0, 38, 76 };
            tpList[18] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            tpList[19] = new double[7] { -400, 700, -76, -38, 0, 38, 76 };
            tpList[20] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };

            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[1] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            //tpList[2] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            //tpList[3] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            //tpList[4] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            //tpList[5] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            //tpList[6] = new double[7] { -1500, 0, 0, 0, 0, 0, 0 };
            //tpList[7] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            //tpList[8] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            //tpList[9] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            //tpList[10] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            //tpList[11] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            //tpList[12] = new double[7] { -900, 700, 0, 0, 0, 0, 0 };
            //tpList[13] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            //tpList[14] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            //tpList[15] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            //tpList[16] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            //tpList[17] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };


            List<VolumetricTP6D> lmVMPts6d = new List<VolumetricTP6D>();
            VolumetricTP6D lpmid = new VolumetricTP6D();
            double tXprev = 0;
            int tpListLength = 7;
            int tpListLast = tpListLength - 1;

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  TX
                    VolumetricTP6D lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 1);
                    lmVMPts6d.Add(lpmid1);
                    lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][2] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid1);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][ti], 0, 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][tpListLast] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    //  End of TX
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TY
                    lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 2);
                    lmVMPts6d.Add(lpmid2);
                    lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][2] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][ti], 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][tpListLast] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid3);
                    ////////  End of TY
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TZ
                    lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 3);
                    lmVMPts6d.Add(lpmid3);
                    lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][2] / 2, -1);
                    lmVMPts6d.Add(lpmid3);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][ti]);
                        lmVMPts6d.Add(lp1);
                    }
                    ////////  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 2 * tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                }
                if (i < 8)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\" + "XYZ.csv");
            for (int i = 0; i < mVMPts6d.Length; i++)
            {
                wr.WriteLine(mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + "," + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + "," + mVMPts6d[i].pivotAxis.ToString("F0"));
            }
            wr.Close();


            tbVsnLog.Text += "\r\n" + (mVMPts6d.Length).ToString() + " points.";
        }
        public void ApplyVolumetricMeasure3step()
        {
            double timeEst = 0;
            double[][] tpList = new double[21][];

            ////////////////////////////////////////////////////////////////////////////////////////
            //  Hybrid Stage
            double[] tpX = new double[10] { -550, -1100, -1700, -1550, -1400, -700, 0, 700, 1400, 0 };

            //	Y		Z		T1		T2		T3		T4		T5	

            tpList[0] = new double[7] { -1000, -700, 0, 0, 0, 0, 0 };
            tpList[1] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            tpList[2] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            tpList[3] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            tpList[4] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            tpList[5] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            tpList[6] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            tpList[7] = new double[7] { -1600, 0, 0, 0, 0, 0, 0 };
            tpList[8] = new double[7] { -1500, 0, 0, 0, 0, 0, 0 };
            tpList[9] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            tpList[10] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            tpList[11] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            tpList[12] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            tpList[13] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            tpList[14] = new double[7] { -1000, 700, 0, 0, 0, 0, 0 };
            tpList[15] = new double[7] { -900, 700, 0, 0, 0, 0, 0 };
            tpList[16] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            tpList[17] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            tpList[18] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            tpList[19] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            tpList[20] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };

            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[1] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            //tpList[2] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            //tpList[3] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            //tpList[4] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            //tpList[5] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            //tpList[6] = new double[7] { -1500, 0, 0, 0, 0, 0, 0 };
            //tpList[7] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            //tpList[8] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            //tpList[9] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            //tpList[10] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            //tpList[11] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            //tpList[12] = new double[7] { -900, 700, 0, 0, 0, 0, 0 };
            //tpList[13] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            //tpList[14] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            //tpList[15] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            //tpList[16] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            //tpList[17] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };


            List<VolumetricTP6D> lmVMPts6d = new List<VolumetricTP6D>();
            VolumetricTP6D lpmid = new VolumetricTP6D();
            double tXprev = 0;
            int tpListLength = 7;
            int tpListLast = tpListLength - 1;

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  TX
                    VolumetricTP6D lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 1);
                    lmVMPts6d.Add(lpmid1);
                    lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][2] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid1);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][ti], 0, 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][tpListLast] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    //  End of TX
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                if (i < 9)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -8);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TY
                    VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 2);
                    lmVMPts6d.Add(lpmid2);
                    lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][2] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][ti], 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][tpListLast] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid3);
                    ////////  End of TY
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                if (i < 9)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -7);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TZ
                    VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 3);
                    lmVMPts6d.Add(lpmid3);
                    lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][2] / 2, -1);
                    lmVMPts6d.Add(lpmid3);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][ti]);
                        lmVMPts6d.Add(lp1);
                    }
                    ////////  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 2 * tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                if (i < 9)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\" + "XYZ.csv");
            for (int i = 0; i < mVMPts6d.Length; i++)
            {
                wr.WriteLine(mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + "," + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + "," + mVMPts6d[i].pivotAxis.ToString("F0"));
            }
            wr.Close();


            tbVsnLog.Text = (mVMPts6d.Length).ToString() + " points.";
        }

        private CancellationTokenSource volumetricCts;

    

        public Point3d[] mHexapodPivots = new Point3d[3];
        public Point2d mHexapodHplane = new Point2d();  //  TX by degree, TY by degree

        public void InitializeHexpodPivot()
        {
            mHexapodPivots[0] = new Point3d(0, 1.439438857, -14.10421867); //  TX PIVOT
            mHexapodPivots[1] = new Point3d(-0.019249611, 0, -14.03564737); //  TY PIVOT
            mHexapodPivots[2] = new Point3d(0.096426953, 0.384410326, 0);  //  TZ PIVOT
        }
        public void LoadHexpodPivots()
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\Pivot" + camID0 + ".txt";
            if (!File.Exists(pivotFile))
                return;

            StreamReader rr = new StreamReader(pivotFile);
            string lstr = rr.ReadToEnd();
            rr.Close();

            string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 3; i++)
            {
                string[] elements = allLines[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mHexapodPivots[i].X = double.Parse(elements[0]);
                mHexapodPivots[i].Y = double.Parse(elements[1]);
                mHexapodPivots[i].Z = double.Parse(elements[2]);
            }
            if (allLines.Length < 4)
            {
                mCommonPivot.X = (mHexapodPivots[1].X + mHexapodPivots[2].X) / 2;
                mCommonPivot.Y = (mHexapodPivots[2].Y + mHexapodPivots[0].Y) / 2;
                mCommonPivot.Z = (mHexapodPivots[0].Z + mHexapodPivots[1].Z) / 2;
                return;
            }

            string[] commonElements = allLines[4].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mCommonPivot.X = double.Parse(commonElements[0]);
            mCommonPivot.Y = double.Parse(commonElements[1]);
            mCommonPivot.Z = double.Parse(commonElements[2]);
        }

   
        public void ClearHexapodPivots()
        {
            mHexapodPivots[0] = new Point3d(0, 0, -14000);
            mHexapodPivots[1] = new Point3d(0, 0, -14000);
            mHexapodPivots[2] = new Point3d(0, 0, -14000);
        }
     
     


   
   

  

       

       
        public Point3d mCommonPivot = new Point3d(0, 0, 0);


        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        ///    OQC Procedure
        ///    
        /// </summary>

        public VolumetricTP6D mPorg = new VolumetricTP6D();     //  um
        public VolumetricTP6D mCSHorg = new VolumetricTP6D();     //  um
        public double[] mCSHorgProbe6D = new double[6];     //  um
        public Point3d mCSHorgProbe = new Point3d();     //  um
        public Point3d mFidorg = new Point3d();     //  um
        public Point3d mHCSrotation = new Point3d();    //  min

    
    
      
     

  
    

        const double Pixel_To_Um = 5.5 / Global.LensMag;
        const double Um_To_Pixel = Global.LensMag / 5.5;

        double MIN_To_RAD = Math.PI / (180 * 60);
        double RAD_To_MIN = 180 * 60 / Math.PI;

        public double ProbeZcompensationForTXTY(double px, double py, double pz, double pTXrad, double pTYrad)
        {
            double resZ = 0;
            //double resZold = 0;

            if (!mbFigorgLoaded)
                //return resZ;
                return pz;  // 보정 적용 안된 초기값 반환: EastView할때는 mbFigorgLoaded false임.

            //  mCSHorgProbe 에서 Probe 값을 (0,0,0) 으로 리셋하지 않고 그 이후로도 Probe 값을 리셋하지 않는 경우
            //  즉 mCSHorg 를 찾기 전에 Probe 값을 리셋하고 이후로는 Probe 값을 리셋하지 않는 경우 다음 식 적용
            //double curXold = - px - mCSHorgProbe.X + mFidorg.X;   //  um   //  수정 전
            //double curX = (px - mCSHorgProbe.X) + mFidorg.X;   //  um    //  수정 후
            double curX = -(px - mCSHorgProbe.X) + mFidorg.X;   //  um    //  수정 후
            double curY = py - mCSHorgProbe.Y + mFidorg.Y;   //  um

            //  mCSHorgProbe 에서 Probe 값을 (0,0,0) 으로 리셋했고 그 이후에 mFidorg 를 측정했으므로 mFidorg 로부터 현재위치까지의 거리는 다음 식이 맞다.
            //double curX = - px + mFidorg.X;   //  um   
            //double curY = py + mFidorg.Y;   //  um

            double pT = Math.Sqrt(pTXrad * pTXrad + pTYrad * pTYrad);

            //resZ -= Math.Sin(pTYrad) * curX;
            //resZ -= Math.Sin(pTXrad) * curY;

            resZ -= Math.Tan(pTYrad) * curX;
            resZ -= Math.Tan(pTXrad) * curY;
            resZ += 1510 * (1 / Math.Cos(pT) - 1);

            //resZold -= Math.Sin(pTYrad) * curXold;
            //resZold -= Math.Sin(pTXrad) * curY;
            //resZold += 1510 * (1 / Math.Cos(pT) - 1);

            //return resZold + pz;
            return resZ + pz;
        }
        public Point3d XYZcompensationAboutZPivots(Point3d ProbeXYZ, double TXrad, double TYrad)
        {
            //mPorg = new VolumetricTP6D(0, 0, 0, 0, 0, 0);
            //mFidorg = new Point3d(3, 0, 0);    //  (TX,TY,TZ) = (mPorg.TX, mPorg.TY, mPorg.TZ) , mFidorg 는 XYZ stage 의 이동에 따라 변하거나 하지 않는다.
            //                                   //  이 좌표는 XYZ stage 가 Porg 일 때 Center of fiducial mark 의 XYZ stage 에서의 좌표이다.
            //mHexapodPivots[0] = new Point3d(0, -1, 0);   //  X pivot
            //mHexapodPivots[1] = new Point3d(-1, 0, 0);   //  Y pivot
            //mHexapodPivots[2] = new Point3d(1, 1, 0);   //  Y pivot


            //  Y 회전 먼저, X 회전 나중에
            double TX = TXrad;// * MIN_To_RAD;
            double TY = TYrad;// * MIN_To_RAD;
            Point3d Vy = new Point3d();
            Point3d Vy1 = new Point3d();
            Vy.X = mFidorg.X + mHexapodPivots[1].X - mHexapodPivots[2].X;   //  Pivot Y 의 물리적 위치
            Vy.Y = mFidorg.Y + mHexapodPivots[1].Y - mHexapodPivots[2].Y;
            Vy.Z = mFidorg.Z + mHexapodPivots[1].Z - mHexapodPivots[2].Z;

            Vy1.X = Vy.X + ProbeXYZ.X;
            Vy1.Y = Vy.Y;// + shift.Y;
            Vy1.Z = Vy.Z;// + shift.Z;

            Point3d F2 = new Point3d();
            double[,] Rty = new double[3, 3];
            double[] Pz_Py = new double[3];
            Pz_Py[0] = mHexapodPivots[2].X - mHexapodPivots[1].X;
            Pz_Py[1] = mHexapodPivots[2].Y - mHexapodPivots[1].Y;
            Pz_Py[2] = mHexapodPivots[2].Z - mHexapodPivots[1].Z;
            m__G.oCam[0].mFAL.mFZM.RotationXYZ(0, TYrad, 0, ref Rty);
            //  ~ 여기까지 검증됨
            double[] RtyPz_Py = new double[3];
            m__G.oCam[0].mFAL.mFZM.MatrixCross(ref Rty, ref Pz_Py, ref RtyPz_Py, 3);
            F2.X = RtyPz_Py[0] + Vy1.X;
            F2.Y = RtyPz_Py[1] + Vy1.Y;
            F2.Z = RtyPz_Py[2] + Vy1.Z;
            double dZty = Vy1.Z - Vy1.Z / Math.Cos(TY) + Vy1.X * Math.Tan(TY);



            Point3d Vx = new Point3d();
            Point3d Vx1 = new Point3d();

            //  Pivot X 가 앞선 TY 회전에 의해 이동하는 것을 가정한 경우 : 가능성 낮음
            //  순수한 회전에 의해 fiducial mark 중심이 이동한 만큼 pivot 도 이동했다고 보는 경우
            //  이것은 Y 축 회전각도별 X Pivot 의 좌표를 측정함으로써 검증 가능.
            //Vx.X = F2.X + mHexapodPivots[0].X - mHexapodPivots[2].X;
            //Vx.Y = F2.Y + mHexapodPivots[0].Y - mHexapodPivots[2].Y;
            //Vx.Z = F2.Z + mHexapodPivots[0].Z - mHexapodPivots[2].Z;

            //  Pivot 은 어떤 회전운동에도 관계없이 일정하게 유지되는 것을 가정한 경우 : HEXAPOD 구동 원리상 회전운동이 회전중심을 병진이동시키지 않는다는 전제.
            Vx.X = mFidorg.X + mHexapodPivots[0].X - mHexapodPivots[2].X;
            Vx.Y = mFidorg.Y + mHexapodPivots[0].Y - mHexapodPivots[2].Y;
            Vx.Z = mFidorg.Z + mHexapodPivots[0].Z - mHexapodPivots[2].Z;

            Vx1.X = Vx.X;// + shift.X;
            Vx1.Y = Vx.Y + ProbeXYZ.Y;
            Vx1.Z = Vx.Z;// + shift.Z;

            Point3d F3 = new Point3d();
            Point3d F4 = new Point3d();
            double[,] Rtx = new double[3, 3];
            double[] F2_F_Pz_Px = new double[3];    //  F2 - F + Pz - Px
            F2_F_Pz_Px[0] = F2.X - mFidorg.X + mHexapodPivots[2].X - mHexapodPivots[0].X;
            F2_F_Pz_Px[1] = F2.Y - mFidorg.Y + mHexapodPivots[2].Y - mHexapodPivots[0].Y;
            F2_F_Pz_Px[2] = F2.Z - mFidorg.Z + mHexapodPivots[2].Z - mHexapodPivots[0].Z;
            m__G.oCam[0].mFAL.mFZM.RotationXYZ(TXrad, 0, 0, ref Rtx);
            double[] RtxPz_Px = new double[3];
            m__G.oCam[0].mFAL.mFZM.MatrixCross(ref Rtx, ref F2_F_Pz_Px, ref RtxPz_Px, 3);
            F4.X = RtxPz_Px[0] + Vx1.X;
            F4.Y = RtxPz_Px[1] + Vx1.Y;
            F4.Z = RtxPz_Px[2] + Vx1.Z;

            double dZtytx = (dZty - Vx1.Y * Math.Sin(TX) + Vx1.Z * Math.Cos(TX) - Vx1.Z) / Math.Cos(TX);
            //double dZtytx = (dZty - Vx1.Y * Math.Sin(TX) + Vx1.Z * Math.Cos(TX) - Vx1.Z) / Math.Cos(TX);

            Point3d res = new Point3d();
            res.X = F4.X;
            res.Y = F4.Y;
            res.Z = ProbeXYZ.Z + F4.Z - dZtytx;
            double pT = Math.Sqrt(TXrad * TXrad + TYrad * TYrad);
            res.Z += 1510 * (1 / Math.Cos(pT) - 1);

            return res;
        }

       
    

        public bool LoadOQCcondition()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";
            if (!File.Exists(oqcFile))
                return false;
            StreamReader rd = new StreamReader(oqcFile);
            string strAll = rd.ReadToEnd();
            rd.Close();
            string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] oqcElements = allLines[0].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mPorg.X = double.Parse(oqcElements[0]);
            mPorg.Y = double.Parse(oqcElements[1]);
            mPorg.Z = double.Parse(oqcElements[2]);
            mPorg.TX = double.Parse(oqcElements[3]);
            mPorg.TY = double.Parse(oqcElements[4]);
            mPorg.TZ = double.Parse(oqcElements[5]);
            oqcElements = allLines[1].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHCSrotation.X = double.Parse(oqcElements[0]);
            mHCSrotation.Y = double.Parse(oqcElements[1]);
            mHCSrotation.Z = double.Parse(oqcElements[2]);
            oqcElements = allLines[2].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mCSHorg.X = double.Parse(oqcElements[0]);
            mCSHorg.Y = double.Parse(oqcElements[1]);
            mCSHorg.Z = double.Parse(oqcElements[2]);
            mCSHorg.TX = double.Parse(oqcElements[3]);
            mCSHorg.TY = double.Parse(oqcElements[4]);
            mCSHorg.TZ = double.Parse(oqcElements[5]);
            if (allLines.Length > 4 && allLines[3].Length > 3)
            {
                oqcElements = allLines[3].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mCSHorgProbe.X = double.Parse(oqcElements[0]);
                mCSHorgProbe.Y = double.Parse(oqcElements[1]);
                mCSHorgProbe.Z = double.Parse(oqcElements[2]);

                oqcElements = allLines[4].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mFidorg.X = double.Parse(oqcElements[0]);
                mFidorg.Y = double.Parse(oqcElements[1]);
                mFidorg.Z = double.Parse(oqcElements[2]);

                mbFigorgLoaded = true;
            }
            else
            {
                mbFigorgLoaded = false;
            }
            return true;
        }
        public bool LoadPivotXYZ()
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
            if (!File.Exists(pivotFile))
                return false;
            StreamReader rd = new StreamReader(pivotFile);
            string strAll = rd.ReadToEnd();
            rd.Close();
            string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] pivotElements = allLines[0].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHexapodPivots[0].X = double.Parse(pivotElements[0]);
            mHexapodPivots[0].Y = double.Parse(pivotElements[1]);
            mHexapodPivots[0].Z = double.Parse(pivotElements[2]);
            pivotElements = allLines[1].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHexapodPivots[1].X = double.Parse(pivotElements[0]);
            mHexapodPivots[1].Y = double.Parse(pivotElements[1]);
            mHexapodPivots[1].Z = double.Parse(pivotElements[2]);
            pivotElements = allLines[2].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHexapodPivots[2].X = double.Parse(pivotElements[0]);
            mHexapodPivots[2].Y = double.Parse(pivotElements[1]);
            mHexapodPivots[2].Z = double.Parse(pivotElements[2]);
            return true;
        }
        public bool ChangePivotXYZ(int axis)
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
            if (!File.Exists(pivotFile))
                return false;
            StreamReader rd = new StreamReader(pivotFile);
            string strAll = rd.ReadToEnd();
            rd.Close();
            string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            axis -= 1;
            allLines[axis] = mHexapodPivots[axis].X.ToString("F7") + "\t" + mHexapodPivots[axis].Y.ToString("F7") + "\t" + mHexapodPivots[axis].Z.ToString("F7");
            StreamWriter wr = new StreamWriter(pivotFile);
            wr.WriteLine(allLines[0]);
            wr.WriteLine(allLines[1]);
            wr.WriteLine(allLines[2]);
            wr.Close();

            return true;

        }

        //public bool ChangePivotZ()
        //{
        //    string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
        //    if (!File.Exists(pivotFile))
        //        return false;
        //    StreamReader rd = new StreamReader(pivotFile);
        //    string strAll = rd.ReadToEnd();
        //    rd.Close();
        //    string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    allLines[2] = mHexapodPivots[2].X.ToString("F7") + "\t" + mHexapodPivots[2].Y.ToString("F7") + "\t" + mHexapodPivots[2].Z.ToString("F7");
        //    StreamWriter wr = new StreamWriter(pivotFile);
        //    wr.WriteLine(allLines[0]);
        //    wr.WriteLine(allLines[1]);
        //    wr.WriteLine(allLines[2]);
        //    wr.Close();

        //    return true;

        //}

        public bool mbFigorgLoaded = false;

        public void SaveFidorg()
        {
            string FidorgFile = m__G.m_RootDirectory + "\\DoNotTouch\\Fidorg.txt";
            string mstr = /*"Fidorg \t" + */mFidorg.X.ToString("F7") + "\t" + mFidorg.Y.ToString("F7") + "\t" + mFidorg.Z.ToString("F7") + "\r\n";
            StreamWriter wr = new StreamWriter(FidorgFile);
            wr.Write(mstr);
            wr.Close();
        }
        private void button19_Click(object sender, EventArgs e)
        {
        }

        public void PivotRepeatability()
        {
            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport();

            //string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";
            //if (!File.Exists(oqcFile))
            //    FindAllOrgs();

            //bool bOQCloaded = LoadOQCcondition();
            //string lstr = "";
            //for (int i = 0; i < 10; i++)
            //{
            //    MotorSetHCS(0, 0, 0);
            //    MotorSetPivot(0, 0, 0);
            //    MotorSetSpeed6D(SpeedLevel.Normal);

            //    if (bOQCloaded)
            //        MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
            //    else
            //        MotorMoveHome6D();

            //    MotorSetHCS(0, 0, mHCSrotation.Z);
            //    ClearHexapodPivots();
            //    //FindPivot(1);
            //    //FindPivot(2);
            //    FindPivot(3);
            //    lstr += "X Pivot \t" + mHexapodPivots[0].X.ToString("F7") + "\t" + mHexapodPivots[0].Y.ToString("F7") + "\t" + mHexapodPivots[0].Z.ToString("F7") + "\t"
            //                  + "Y Pivot \t" + mHexapodPivots[1].X.ToString("F7") + "\t" + mHexapodPivots[1].Y.ToString("F7") + "\t" + mHexapodPivots[1].Z.ToString("F7") + "\t"
            //                  + "Z Pivot \t" + mHexapodPivots[2].X.ToString("F7") + "\t" + mHexapodPivots[2].Y.ToString("F7") + "\t" + mHexapodPivots[2].Z.ToString("F7") + "\r\n"
            //                  ;
            //}
            //string pivotRepeatabilityFile = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\PivotRepeatability" + m__G.mCamID0 + ".txt";
            //StreamWriter wr = new StreamWriter(pivotRepeatabilityFile);
            //wr.Write(lstr);
            //wr.Close();


        }

        private void SaveCSHorg()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";
            if (!File.Exists(oqcFile))
                return;
            StreamReader rd = new StreamReader(oqcFile);
            string lstr = rd.ReadToEnd();
            rd.Close();

            string[] allLine = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            allLine[2] = mCSHorg.X.ToString() + "\t" + mCSHorg.Y.ToString() + "\t" + mCSHorg.Z.ToString() + "\t" +
                    mCSHorg.TX.ToString() + "\t" + mCSHorg.TY.ToString() + "\t" + mCSHorg.TZ.ToString();
            string strCSHProbe = mCSHorgProbe.X.ToString() + "\t" + mCSHorgProbe.Y.ToString() + "\t" + mCSHorgProbe.Z.ToString();

            StreamWriter wr = new StreamWriter(oqcFile);
            wr.WriteLine(allLine[0]);
            wr.WriteLine(allLine[1]);
            wr.WriteLine(allLine[2]);
            wr.WriteLine(strCSHProbe);
            wr.Close();
        }

        private void SaveOQCCondition()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";

            using (StreamWriter wr = new StreamWriter(oqcFile))
            {
                wr.WriteLine($"{mPorg.X}\t{mPorg.Y}\t{mPorg.Z}\t{mPorg.TX}\t{mPorg.TY}\t{mPorg.TZ}");
                wr.WriteLine($"{mHCSrotation.X}\t{mHCSrotation.Y}\t{mHCSrotation.Z}");
                wr.WriteLine($"{mCSHorg.X}\t{mCSHorg.Y}\t{mCSHorg.Z}\t{mCSHorg.TX}\t{mCSHorg.TY}\t{mCSHorg.TZ}");
                wr.WriteLine($"{mCSHorgProbe.X}\t{mCSHorgProbe.Y}\t{mCSHorgProbe.Z}");
                wr.WriteLine($"{mFidorg.X:F7}\t{mFidorg.Y:F7}\t{mFidorg.Z:F7}");
            }
        }
     

      


   

        public void SavePivots()
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
            using (StreamWriter wr = new StreamWriter(pivotFile))
            {
                // X Pivot
                wr.WriteLine($"{mHexapodPivots[0].X:F7}\t{mHexapodPivots[0].Y:F7}\t{mHexapodPivots[0].Z:F7}");
                // Y Pivot
                wr.WriteLine($"{mHexapodPivots[1].X:F7}\t{mHexapodPivots[1].Y:F7}\t{mHexapodPivots[1].Z:F7}");
                // Z Pivot
                wr.WriteLine($"{mHexapodPivots[2].X:F7}\t{mHexapodPivots[2].Y:F7}\t{mHexapodPivots[2].Z:F7}");
            }
        }
     

        private void FVision_Shown(object sender, EventArgs e)
        {
            //InitMasterZeroList();
            //MasterList.SelectedIndex = GetMasterZeroIndex();
        }

    
   

        private void AddVsnLog(string lstr)
        {
            if (tbVsnLog.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbVsnLog.Text += lstr + "\r\n";
                    tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
                    tbVsnLog.ScrollToCaret();
                });
            else
            {
                tbVsnLog.Text += lstr + "\r\n";
                tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
                tbVsnLog.ScrollToCaret();
            }

        }

        private void SettbUnCalibratedInfoVisible(bool visible)
        {
            tbUncalibratedInfo.BeginInvoke(new Action(() => { tbUncalibratedInfo.Visible = visible; }));
        }



        private void FVision_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

   

        private void FVision_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m__G.mGageCounter != null)
            {
                if (m__G.m_bCalibrationModel)
                {
                    m__G.mGageCounter.CloseAllport();
                    m__G.mGageCounter.DisposAllport();
                }
            }
        }

        private void btnAddMaster_Click(object sender, EventArgs e)
        {
            //if (MasterList.Items.Count >= 3) { return; }
            SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
            InitMasterZeroList();
            MasterList.SelectedIndex = GetMasterZeroCount() - 1;
        }

        private void MasterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MasterList.SelectedItem == null) return;
            string selectedItem = MasterList.SelectedItem.ToString();
            int index = MasterList.SelectedIndex;
            if (index == 0)
                SetMasterZeroIndex(0);
            else
                SetMasterZeroIndex(3);

            txtMsaterNum.Text = GetMasterZeroIndex().ToString();
        }

        private void btnDeleteMaster_Click(object sender, EventArgs e)
        {
            if (MasterList.SelectedItem == null || MasterList.Items.Count <= 0) return;
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero\\" + MasterList.SelectedItem.ToString();
            File.Delete(filename);
            InitMasterZeroList();
            MasterList.SelectedIndex = GetMasterZeroCount() - 1;
        }

        private void FVision_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                InitMasterZeroList();
                UpdateSensorState();
                InitConstatus();
                //MasterList.SelectedIndex = GetMasterZeroIndex();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cbContinuosMode.Checked = false;

            FindCarrierToDummyShift();

            CameraReset(2, true);
        }

        public double[] mPhiThetaPsi = new double[3];
    

        public string mDataFile100 = "";
        private CancellationTokenSource prism45Cts;

     
     

        public Point3d[] PrismCSRotations = new Point3d[3];
        public Point3d[] PrismCSPivots = new Point3d[3];



   
   
        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton bt = (System.Windows.Forms.RadioButton)sender;

            ScanName = bt.Text;

            switch (ScanName)
            {
                case "AF Scan":
                    tbImgNumber.Text = m__G.oCam[0].AFRelayCnt.ToString();
                    break;
                case "OIS X Scan":
                    tbImgNumber.Text = m__G.oCam[0].XRelayCnt.ToString();
                    break;
                case "OIS Y Scan":
                    tbImgNumber.Text = m__G.oCam[0].YRelayCnt.ToString();
                    break;
            }
        }

        private void btnCoverDn_Click(object sender, EventArgs e)
        {
            if (Dln.isMoving || Dln.IsRun) return;
            Dln.isMoving = true;
            Dln.CoverDn();
            Dln.isMoving = false;
        }

        private void btnCoverUp_Click(object sender, EventArgs e)
        {
            if (Dln.isMoving || Dln.IsRun) return;
            Dln.isMoving = true;
            Dln.CoverUp();
            Dln.isMoving = false;
        }

        private void btnSocketLd_Click(object sender, EventArgs e)
        {
            Stopwatch st = new Stopwatch();
            if (Dln.isMoving || Dln.IsRun) return;
            Dln.isMoving = true;
            Dln.CoverUp();
            Thread.Sleep(500);
            Dln.LoadSocket();
            if (Option.SocketSensorUse)
            {
                st.Start();
                while (!Dln.GetGpioStatus(12) || Dln.GetGpioStatus(13))
                {
                    if (st.ElapsedMilliseconds > 3000) { MessageBox.Show("Check Socket Sensor Status"); Dln.isMoving = false; return; }
                    Thread.Sleep(10);
                }
                st.Stop();
            }
            else Thread.Sleep(2000);
            UpdateSensorState();
            Dln.isMoving = false;

        }
        private void btnSocketUd_Click(object sender, EventArgs e)
        {
            Stopwatch st = new Stopwatch();
            if (Dln.isMoving || Dln.IsRun) return;
            Dln.isMoving = true;
            Dln.CoverUp();
            Thread.Sleep(500);
            Dln.UnloadSocket();
            if (Option.SocketSensorUse)
            {
                st.Start();
                while (Dln.GetGpioStatus(12) || !Dln.GetGpioStatus(13))
                {
                    if (st.ElapsedMilliseconds > 3000) { MessageBox.Show("Check Socket Sensor Status"); Dln.isMoving = false; return; }
                    Thread.Sleep(10);
                }
                st.Stop();
            }
            else Thread.Sleep(500);
            UpdateSensorState();
            Dln.isMoving = false;
        }
     

        void LoadSample()
        {
            try
            {
                Stopwatch st = new Stopwatch();

                
                Dln.CoverUp();
                Thread.Sleep(500);
                Dln.LoadSocket();
                if (Option.SocketSensorUse)
                {
                    st.Start();
                    while (!Dln.GetGpioStatus(12) || Dln.GetGpioStatus(13))
                    {
                        if (st.ElapsedMilliseconds > 3000) { MessageBox.Show("Check Socket Sensor Status"); Dln.isMoving = false; return; }
                        Thread.Sleep(10);
                    }
                    st.Stop();
                    Thread.Sleep(300);
                }
                else Thread.Sleep(2000);
                UpdateSensorState();
                Dln.CoverDn();
                Thread.Sleep(500);
                UpdateConState();

                Dln.isMoving = false;
            }
            catch { Dln.isMoving = false; }
        }
        void UnloadSample()
        {
            try
            {
                Stopwatch st = new Stopwatch();
                Dln.CoverUp();
                Thread.Sleep(500);
                Dln.UnloadSocket();
                if (Option.SocketSensorUse)
                {
                    st.Start();
                    while (Dln.GetGpioStatus(12) || !Dln.GetGpioStatus(13))
                    {
                        if (st.ElapsedMilliseconds > 3000) { MessageBox.Show("Check Socket Sensor Status"); Dln.isMoving = false; return; }
                        Thread.Sleep(10);
                    }
                    st.Stop();
                }
                else Thread.Sleep(500);
                UpdateSensorState();
                InitConstatus();
               
                Dln.isMoving = false;
            }
            catch { Dln.isMoving = false; }
        }

        private void btnSPLLD_Click(object sender, EventArgs e)
        {
            if (Dln.isMoving || Dln.IsRun) return;
            Dln.isMoving = true;
            Task.Run(() => LoadSample());
        }

        private void btnSPLUD_Click(object sender, EventArgs e)
        {
            if (Dln.isMoving || Dln.IsRun) return;
            Dln.isMoving = true;
            Task.Run(() => UnloadSample());
        }

        void UpdateSensorState()
        {
            if (Option.SocketSensorUse)
            {
                if (Dln.GetGpioStatus(12) && !Dln.GetGpioStatus(13))
                {
                    if (lbSocketState.InvokeRequired)
                    {
                        lbSocketState.BeginInvoke((MethodInvoker)delegate
                        {
                            lbSocketState.BackColor = Color.Lime;
                        });
                    }
                    else
                        lbSocketState.BackColor = Color.Lime;
                }
                else if (!Dln.GetGpioStatus(12) && Dln.GetGpioStatus(13))
                {
                    if (lbSocketState.InvokeRequired)
                    {
                        lbSocketState.BeginInvoke((MethodInvoker)delegate
                        {
                            lbSocketState.BackColor = Color.White;
                        });
                    }
                    else
                        lbSocketState.BackColor = Color.White;
                }
                else
                {
                    if (lbSocketState.InvokeRequired)
                    {
                        lbSocketState.BeginInvoke((MethodInvoker)delegate
                        {
                            lbSocketState.BackColor = Color.DarkGray;
                        });
                    }
                    else
                        lbSocketState.BackColor = Color.DarkGray;
                }
            }
            else
            {
                if (lbSocketState.InvokeRequired)
                {
                    lbSocketState.BeginInvoke((MethodInvoker)delegate
                    {
                        lbSocketState.BackColor = Color.White;
                    });
                }
                else
                    lbSocketState.BackColor = Color.White;
            }
        }
        void UpdateConState()
        {

            bool Constate = true;

            if (!Process.Dln.WriteArray(0, Process.DrvIC.AFOriginAddr, 0x02, new byte[] { 0x40 }) && !Process.Dln.WriteArray(0, Process.DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            if (!Process.Dln.WriteArray(0, Process.DrvIC.XOriginAddr, 0x02, new byte[] { 0x40 }) && !Process.Dln.WriteArray(0, Process.DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            if (!Process.Dln.WriteArray(0, Process.DrvIC.Y1OriginAddr, 0x02, new byte[] { 0x40 }) && !Process.Dln.WriteArray(0, Process.DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            if (Process.DrvIC.Y2SlaveAddr != 0x00)
            {
                if (!Process.Dln.WriteArray(0, Process.DrvIC.Y2OriginAddr, 0x02, new byte[] { 0x40 })
                    && !Process.Dln.WriteArray(0, Process.DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            }

            if (Constate)
            {
                if (lbConState.InvokeRequired)
                {
                    lbConState.BeginInvoke((MethodInvoker)delegate
                    {
                        lbConState.BackColor = Color.Lime;
                    });
                }
                else lbConState.BackColor = Color.Lime;

            }
            else
            {
                if (lbConState.InvokeRequired)
                {
                    lbConState.BeginInvoke((MethodInvoker)delegate
                    {
                        lbConState.BackColor = Color.White;
                    });
                }
                else lbConState.BackColor = Color.White;

            }

        }
        void InitConstatus()
        {
            if (lbConState.InvokeRequired)
            {
                lbConState.BeginInvoke((MethodInvoker)delegate
                {
                    lbConState.BackColor = Color.White;
                });
            }
            else lbConState.BackColor = Color.White;

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Process.Dln.WriteArray(0, Process.DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Process.Dln.WriteArray(0, Process.DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Process.Dln.WriteArray(0, Process.DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            STATIC.DrvIC.Move(0, "AF", 2048);
            STATIC.DrvIC.Move(0, "X", 2048);
            STATIC.DrvIC.Move(0, "Y", 2048);
        }

        private void btnPowerOn_Click(object sender, EventArgs e)
        {
            STATIC.Dln.PowerOnOff(0, true);
        }

        private void btnPowerOff_Click(object sender, EventArgs e)
        {
            STATIC.Dln.PowerOnOff(0, false);
        }
    }
}