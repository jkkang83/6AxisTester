using Basler.Pylon;
using Dln;
using FAutoLearn;
using Matrox.MatroxImagingLibrary;
using MotorizedStage_SK_PI;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using OpenCvSharp.Flann;
//using OpenCvSharp;
using S2System.Vision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static alglib;
using static alglib.apserv;
using static CSH030Ex.FManage;
using static CSH030Ex.FVision;
using static FAutoLearn.FAutoLearn;
using static FAutoLearn.FZMath;
using static MotorizedStage_SK_PI.F_Motion_SK_PI;
using static S2System.Vision.MILlib;
using Axis = MotorizedStage_SK_PI.Axis;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Reflection.Emit;

namespace CSH030Ex
{
    public partial class FVision : Form
    {

        private Global m__G;
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

        public delegate void DelegateMotorSetHome6D();
        public delegate void DelegateMotorMoveHome6D();

        public delegate void DelegateMotorMoveOrigin6D();
        public delegate void DelegateMotorMoveOriginHexapod();

        public delegate void DelegateMotorMoveAbsAxis(Axis axis, double pos);
        public delegate void DelegateMotorMoveAbs6D(double x, double y, double z, double tx, double ty, double tz);

        public delegate void DelegateMotorJogRun(Axis axis, bool dir, SpeedLevel speedLevel);
        public delegate void DelegateMotorJogStop(Axis axis);

        public delegate void DelegateMotorSetSpeedAxis(Axis axis, SpeedLevel speedlevel);
        public delegate void DelegateMotorSetSpeed6D(SpeedLevel speedlevel);


        public delegate void DelegateMotorSetPivot(double x, double y, double z);
        public delegate void DelegateMotorSetHCS(double tx, double ty, double tz);

        public delegate double DelegateMotorCurPosAxis(Axis axis);
        public delegate double[] DelegateMotorCurPos6D();
        public delegate double[] DelegateMotorCurPosHexpod();

        public delegate void DelegateHexapodRotate(double tx, double ty, double tz);
        public delegate void DelegateMotorXYZ(double x, double y, double z);


        DelegateMotorSetHome6D MotorSetHome6D;
        DelegateMotorMoveHome6D MotorMoveHome6D;

        DelegateMotorMoveOrigin6D MotorMoveOrigin6D;
        DelegateMotorMoveOriginHexapod MotorMoveOriginHexapod;

        DelegateMotorMoveAbsAxis MotorMoveAbsAxis;
        DelegateMotorMoveAbs6D MotorMoveAbs6D;  //  mm, arcmin

        DelegateMotorJogRun MotorJogRun;
        DelegateMotorJogStop MotorJogStop;

        DelegateMotorSetSpeedAxis MotorSetSpeedAxis;
        DelegateMotorSetSpeed6D MotorSetSpeed6D;

        DelegateMotorSetPivot MotorSetPivot;
        DelegateMotorSetHCS MotorSetHCS;

        DelegateMotorCurPosAxis MotorCurPosAxis;
        DelegateMotorCurPos6D MotorCurPos6D;
        DelegateMotorCurPosHexpod MotorCurPosHexapod;

        DelegateHexapodRotate HexapodRotate;
        DelegateMotorXYZ MotorXYZ;


        //  public void PrepareRemoteCalibration()
        //  public void SingleFindMark()
        //  public void RemoteCalibration(string strAxis, int skipCount)
        //  MotorHome
        //  MotorMove6D
        //  MotorMoveAxisAbs

        public void RegisterMotorDelegates(
            DelegateMotorSetHome6D fmotorSetHome6D,
            DelegateMotorMoveHome6D fmotorMoveHome6D,

            DelegateMotorMoveOrigin6D fmotorMoveOrigin6D,
            DelegateMotorMoveOriginHexapod fmotorMoveOrigin6Hexapod,

            DelegateMotorMoveAbsAxis fmotorMoveAbsAxis,
            DelegateMotorMoveAbs6D fmotorMoveAbs6D,

            DelegateMotorJogRun fmotorJogRun,
            DelegateMotorJogStop fmotorJogStop,

            DelegateMotorSetSpeedAxis fmotorSetSpeedAxis,
            DelegateMotorSetSpeed6D fmotorSetSpeed6D,

            DelegateMotorSetPivot fmotorSetPivot,
            DelegateMotorSetHCS fmotorSetHCS,

            DelegateMotorCurPosAxis fmotorCurPosAxis,
            DelegateMotorCurPos6D fmotorCurPos6D,
            DelegateMotorCurPosHexpod fmotorCurPosHexapod,

            DelegateHexapodRotate fhexapodRotate,
            DelegateMotorXYZ fmotorXYZ

            )
        {
            MotorSetHome6D = fmotorSetHome6D;
            MotorMoveHome6D = fmotorMoveHome6D;

            MotorMoveOrigin6D = fmotorMoveOrigin6D;
            MotorMoveOriginHexapod = fmotorMoveOrigin6Hexapod;

            MotorMoveAbsAxis = fmotorMoveAbsAxis;
            MotorMoveAbs6D = fmotorMoveAbs6D;

            MotorJogRun = fmotorJogRun;
            MotorJogStop = fmotorJogStop;

            MotorSetSpeedAxis = fmotorSetSpeedAxis;
            MotorSetSpeed6D = fmotorSetSpeed6D;

            MotorSetPivot = fmotorSetPivot;
            MotorSetHCS = fmotorSetHCS;

            MotorCurPosAxis = fmotorCurPosAxis;
            MotorCurPos6D = fmotorCurPos6D;
            MotorCurPosHexapod = fmotorCurPosHexapod;

            HexapodRotate = fhexapodRotate;
            MotorXYZ = fmotorXYZ;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
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
            AfR
            this.BackColor = Color.FromArgb(96, 96, 100);
            //MessageBox.Show("aaa");
            //if (m__G!=null)
            SetEdgeBand(m__G.sRecipe.iEdgeBand);

            //else
            //    SetEdgeBand();

            //MessageBox.Show("bbb");
            tbInfo.BringToFront();
            tbVsnLog.BringToFront();

            rbCalZ.Checked = true;
            //ChartMTF.Hide();
            //LoadscaleNTheta();
            if (m__G.oCam[0].mFAL != null)
                if (m__G.oCam[0].mFAL.mFZM != null)
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);

            groupBox4.Hide();
            btnChangeCrop.Show();
            cbZaxis.Hide();
            cbTiltAxis.Hide();
            btnSaveOrgPosition.Hide();
            rbCalZ.Checked = true;
            rbFromOrg.Checked = true;
            cbBench.Checked = true;
            cbBench.Checked = false;

            tbXrange.Text = "0";
            tbYrange.Text = "0";
            tbZrange.Text = "0";
            tbTXrange.Text = "0";
            tbTYrange.Text = "0";
            tbTZrange.Text = "0";

            tbXstep.Text = "3";
            tbYstep.Text = "3";
            tbZstep.Text = "3";
            tbTXstep.Text = "3";
            tbTYstep.Text = "3";
            tbTZstep.Text = "3";

            cbSkipFindFidOrg.Checked = false;
            InitializeHexpodPivot();
            if (m__G.m_bCalibrationModel)
            {
                m__G.mGageCounter?.OpenAllport();
            }
        }

        public string camID0 = "";
        public string camID1 = "";
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
                        button11.Enabled = false;
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
                        button11.Enabled = true;

                        radioButton10Step.Checked = true;
                        rb1step.Checked = true;
                        cbContinuosMode.Enabled = true;

                    });
            }
            TransferModelFileList();
            SetRawGainNGamma(m__G.sRecipe.iRawGain, m__G.sRecipe.iGamma);
            SetExposure(0, m__G.sRecipe.iExposure);
            LoadBackbroundNoise();
            LoadScaleNTheta();
            LoadTXTYZeroOffset();
            SetDefaultMarkConfig();
            //string ZLUTfile = m__G.m_RootDirectory + "\\DoNotTouch\\ZLUT_" + camID0 + ".txt";
            //GetZLUT(ZLUTfile);
            string strTXTYTZoffset = LoadTXTYZeroOffset();
            m__G.fManage.AddViewLog("CSH ID " + camID0 + "\t" + strTXTYTZoffset);

            //Regstry Write ====
            Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("CSHTest");
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\CSHTest", true);

            if (reg.GetValue(camID0) == null)
                reg.SetValue(camID0, DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss"));
            reg.Close();
            //==================

            // 241206   // YLUT 적용안함
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            mLoaded = true;

            m__G.oCam[0].RegisterDelegates(m__G.fGraph.mDriverIC.AckSignal);


            // Crop Pos File Load & Init
            bool isLoad = m__G.oCam[0].LoadCropPosFromXml(camID0);
            m__G.oCam[0].InitCrop(!isLoad);
            CropCgap = m__G.oCam[0].CropCgap;

            string motorHomeFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageHomePos{camID0}.txt";
            string motorCSFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageCSPos{camID0}.txt";
            string motorSppedFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageSpeed{camID0}.txt";
            // 모션
            //
            // SK 스테이지
            //m__G.mMotion = new F_Motion(motorFilePath);
            //RegisterMotorDelegates(m__G.mMotion.LogicalOrg, m__G.mMotion.SetLogicalOrg, m__G.mMotion.MoveABS6D, m__G.mMotion.MoveABS,m__G.mMotion.GetCurPos, m__G.mMotion.GetCurPos6D,m__G.mMotion.JogMove, m__G.mMotion.JogStop, m__G.mMotion.SetSpeed6D);
            //
            // PI 스테이지
            //m__G.f_PIMotion = new F_PIMotion(motorFilePath);
            //RegisterMotorDelegates(F_PIMotion._pi.SetLogicHome6D,
            //                        F_PIMotion._pi.MoveLogicHome6D,
            //                        F_PIMotion._pi.MoveAbsAxis,
            //                        F_PIMotion._pi.MoveAbs6D,
            //                        F_PIMotion._pi.GetCurMechaPosAxis,
            //                        F_PIMotion._pi.GetCurMechaPos6D,
            //                        F_PIMotion._pi.GetCurLogicPosAxis,
            //                        F_PIMotion._pi.GetCurLogicPos6D,
            //                        F_PIMotion._pi.JogRun,
            //                        F_PIMotion._pi.JogStop,
            //                        F_PIMotion._pi.SetPivot3D,
            //                        null);
            //LoadHexpodPivots();
            // SK + PI 스테이지
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    m__G.fMotion = new F_Motion_SK_PI(motorHomeFilePath, motorCSFilePath, motorSppedFilePath);
                }));
            }
            else
            {
                m__G.fMotion = new F_Motion_SK_PI(motorHomeFilePath, motorCSFilePath, motorSppedFilePath);
            }

            RegisterMotorDelegates(m__G.fMotion.SetHome6DFormCurPos,
                                   m__G.fMotion.MoveHome6D,

                                   m__G.fMotion.MoveOrigin6D,
                                   m__G.fMotion.MoveOriginHexapod,

                                   m__G.fMotion.MoveAbsAxis,
                                   m__G.fMotion.MoveAbs6D,

                                   m__G.fMotion.JogRun,
                                   m__G.fMotion.JogStop,

                                   m__G.fMotion.SetSpeedAxis,
                                   m__G.fMotion.SetSpeed6D,

                                   m__G.fMotion.SetPivot,
                                   m__G.fMotion.SetCoordinateSystem,

                                   m__G.fMotion.GetCurPosAxis,
                                   m__G.fMotion.GetCurPos6D,
                                   m__G.fMotion.GetCurPosHexapod,

                                   m__G.fMotion.MoveHexapodRotation,
                                   m__G.fMotion.MoveSkXYZ

                                   );
            if (m__G.m_bCalibrationModel)
            {
                m__G.fMotion.ConnectSKPI();
                m__G.fMotion.mInitialMsg = "Connection completed.";
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbVsnLog.Text = "Hybrid Stage connection complete.";
                    });
                }
                else
                    tbVsnLog.Text = "Hybrid Stage connection complete.";
            }
            // JH 스테이지
            //RegisterMotorDelegates(m__G.fManage.M_C,
            //            m__G.fManage.M_H,
            //            m__G.fManage.M_M,
            //            m__G.fManage.M_A,
            //            m__G.fManage.M_G,
            //            m__G.fManage.M_P,
            //            null,
            //            null,
            //            m__G.fManage.M_J,
            //            m__G.fManage.M_E,
            //            null,
            //            null);

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //private void FVision_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //}

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

            button6.Enabled = false;
            button3.Enabled = false;
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
            m__G.mbSuddenStop[0] = true;        //  왜 ?
            m__G.mDoingStatus = "Checking Vision";

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            m__G.fGraph.Drive_LEDs(mLEDcurrent[0], mLEDcurrent[1]);
            btnAllLEDOn.ForeColor = Color.White;

            m_bAllLEDOn = true;
            //label6.Location = new Point(10, 140);
            //label6.Text = "Live On";
            //m__G.oCam[0].DrawDC_Circle(Brushes.Red, 200);    // DrawCircle khkim_170920
            m__G.oCam[0].DrawAllRectangles();

            if (cbLiveWithMarks.Checked && bLiveFindMark == false)
                Task.Run(() => LiveFindMark());
            else
            {
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
            if (!bLiveFindMark)
                return;

            m__G.mDoingStatus = "LiveFindMark";

            m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);
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

            //m__G.fVision.ProcessVisionData(frmCnt, m__G.mMaxThread);
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

                string lstr = tbInfo.Text;
                string[] lineStr = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (lineStr.Length > 6)
                {
                    lstr = "";
                    for (int i = 0; i < 6; i++)
                        lstr += lineStr[lineStr.Length - 6 + i] + "\r\n";

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbInfo.Text = lstr;
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        });
                    }
                    else
                    {
                        tbInfo.Text = lstr;
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                }

                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                Thread.Sleep(180);
                if (!cbLiveWithMarks.Checked)
                    break;
            }
            bHaltLive = false;
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
        bool bHaltLive = false;
        public void GrabHalt()
        {
            //label6.Text = "";
            m__G.oCam[0].HaltA();
            bHaltLive = true;
            btnAllLEDOn.ForeColor = Color.SlateGray;
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
            Thread.Sleep(100);
            m__G.fGraph.Drive_LEDs(0, 0);

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

        private void btnOISXReplay_Click(object sender, EventArgs e)
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

            Thread ThreadReplayL = new Thread(() => Process_OISXReplay(0, 0));
            ThreadReplayL.Start();
            //m_replayIndex = 0;

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
                for (int n = 0; n < m__G.fGraph.mAF_FrameCount; n++)
                {
                    m__G.oCam[0].BufCopy2Disp_OISX(n);

                    Thread.Sleep(interval);
                }
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

        public void LEDOn(int port)
        {
            m__G.fGraph.Drive_LED(0, mLEDcurrent[0]);
            m__G.fGraph.Drive_LED(1, mLEDcurrent[1]);
        }

        public void LEDOff(int port)
        {
            m__G.fGraph.Drive_LED(0, 0);
            m__G.fGraph.Drive_LED(1, 0);
        }
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
            m__G.fGraph.Drive_LED(ch, mLEDcurrent[ch]);

            tbLedLeft.Text = mLEDcurrent[1].ToString("F3");
            tbLedRight.Text = mLEDcurrent[0].ToString("F3");

            //StartLive();

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
            m__G.fGraph.Drive_LED(ch, mLEDcurrent[ch]);

            tbLedLeft.Text = mLEDcurrent[1].ToString("F3");   //  Left
            tbLedRight.Text = mLEDcurrent[0].ToString("F3");   //  Right

            //StartLive();
        }

        private void btnFOVUp_Click(object sender, EventArgs e)
        {
            if (cbMotorized.Checked) return;    // Motor JOG Move

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
            if (cbMotorized.Checked) return;    // Motor JOG Move

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
            if (cbMotorized.Checked) return;    // Motor JOG Move

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
            if (cbMotorized.Checked) return;    // Motor JOG Move

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
        public void JHMotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 4;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            /////////////////////////////////////////////////////////////////////////
            // 자화로 대체
            // gageData = 현재 값 읽어오기;
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[23];

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
            m__G.oCam[0].FineCOG(false, 1, 0);    // 마크찾기
            m__G.oCam[0].FineCOG(false, 2, 0);    // 마크찾기
            m__G.oCam[0].FineCOG(false, 3, 0);    // 마크찾기
            m__G.oCam[0].FineCOG(false, 4, 0);    // 마크찾기

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
                tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  radian to minute
                ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  radian to minute
                tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  radian to minute
            }
            lCalibrationData[0] = sx;
            lCalibrationData[1] = sy;
            lCalibrationData[2] = sz;
            lCalibrationData[3] = tx;
            lCalibrationData[4] = ty;
            lCalibrationData[5] = tz;

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
                if (gageData.Length == 6)
                {
                    // 자화 부호 맞춰야함.
                    lCalibrationData[16] = gageData[0];   // um
                    lCalibrationData[17] = gageData[1];   // um
                    lCalibrationData[18] = gageData[2];   // um
                    lCalibrationData[19] = gageData[3]; // min          // TX   acr min
                    lCalibrationData[20] = gageData[4]; // min          // TY   acr min
                    lCalibrationData[21] = gageData[5]; // min          // TZ   acr min

                    //  출력은 um, arcmin
                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1")
                         + "\t" + lCalibrationData[19].ToString("F1") + "\t" + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1");
                }
            }
            if (ci == 0 && IsSave)
            {
                mCalibrationFullData.Add(lCalibrationData);
                mGageFullData.Add(gageData);
            }


            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                    if (ltextBox[0].Text.Length > 7000)
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

                if (ltextBox[0].Text.Length > 7000)
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

        public void MotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 16;  //4

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////
            ///// 다음 HEXAPOD 로 대체 필요
            //gageData = MotorCurLogicPos6D();

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

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[25]; // 22

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
            for (int findex = 1; findex < mavNum + 1; findex++)
            {
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

                    ////////////////////////////////////////////////////////////////////////////
                    //  여기 수정 필요    20241213. PM 5:45
                    //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 40, m__G.oCam[0].mFAL.mFZM.mProbeXRx, m__G.oCam[0].mFAL.mFZM.mProbeYRy);   // 40   // 32.2 // 32.2
                    //double[] TxTyZ = m__G.oCam[0].mFAL.CalcTXTYZfromProbes(gageData[5] / 1000, gageData[6] / 1000, gageData[4] / 1000, XYTz[0], XYTz[1], XYTz[2]);
                    double[] XYTz = new double[3];
                    XYTz[0] = gageData[0];
                    XYTz[1] = gageData[1];
                    XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                    double[] TxTyZ = new double[3];
                    TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                    TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                    TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                    //double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0], TxTyZ[1]);
                    double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);
                    lCalibrationData[16] = XYTz[0]; // um
                    lCalibrationData[17] = XYTz[1]; // um
                    lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                    //Point3d compRes = XYZcompensationAboutZPivots(new Point3d(XYTz[0], XYTz[1], TxTyZ[2]), TxTyZ[0], TxTyZ[1]);
                    //lCalibrationData[16] = compRes.X;//XYTz[0]; // um
                    //lCalibrationData[17] = compRes.Y;//XYTz[1]; // um
                    //lCalibrationData[18] = compRes.Z;// compZ;// TxTyZ[2]; // um
                    lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian -> min으로 통일
                    lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian -> min으로 통일
                    lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian -> min으로 통일  241216 부호 변경
                    lCalibrationData[22] = gageData[4];
                    lCalibrationData[23] = gageData[5];
                    lCalibrationData[24] = gageData[6];

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

                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + lCalibrationData[19].ToString("F1") + "\t"
                        + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1");

                }
            }
            if (ci == 0 && IsSave)
            {
                mCalibrationFullData.Add(lCalibrationData);
                mGageFullData.Add(gageData);
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

        public void RemoteManualFindMark()
        {
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(50);
            ManualFindMarks(0, false, false);

            m__G.fGraph.Drive_LEDs(0, 0);
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

                        ltextBox[0].Text += strtmp[0] + "\r\n";
                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

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

        public void DrawMarkDetected()
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

                OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();
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
                    btnLive2.Enabled = false;
                    m__G.oCam[0].HaltA();
                    bHaltLive = true;
                    button10.Enabled = false;
                    IsLiveCropStop = true;
                    bThreadManualFindMarks = true;
                    mCalibrationFullData = new List<double[]>();

                    //if (m__G.mGageCounter != null)
                    //    m__G.mGageCounter.OpenAllport();  // 250210 주석처리

                    Task.Run(() => ThreadManualFindMarks(1000));
                }
                else
                {
                    btnLive2.Enabled = true;
                    button10.Enabled = true;
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
                btnLive2.Enabled = false;
                m__G.oCam[0].HaltA();
                bHaltLive = true;
                button10.Enabled = false;
                IsLiveCropStop = true;
                bThreadManualFindMarks = true;
                mCalibrationFullData = new List<double[]>();
                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

                m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
                ManualFindMarks(0, true, false);

                m__G.fGraph.Drive_LEDs(0, 0);
                Thread.Sleep(500);

                btnLive2.Enabled = true;
                button10.Enabled = true;
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

        public void RemoteCalibration(string strAxis, int skipCount)
        {
            //  strAxis 은 "Z", "Y', "X", "TX", "TY" 를 넣을 수 있다.
            //  Calibraition 순서는 "Z" -> "Z"  -> "Y" -> "Y" -> "X" -> "X" -> "TX" -> "TY"
            //  각 보정결과를 확인해야 하므로 보전 전 - 후 로 실시한다.

            //  ORG 위치에서 한쪽 끝으로 이동하면서 얻어진 데이터( skipCount )는 삭제한다.
            for (int i = 0; i < skipCount; i++)
                mCalibrationFullData.RemoveAt(0);

            //JH_SK_CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
            CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
        }
        public void SingleFindMark(bool IsSave = true)
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  SingleFindMark is used for External Calibration
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(100);   //  Wait LED Power Up.
            if (mAutoCalibrationIndex == 0)
                MotorizedFindMarks(mAutoCalibrationIndex, true, IsSave);
            else
                MotorizedFindMarks(mAutoCalibrationIndex, false, IsSave);

            mAutoCalibrationIndex++;
            m__G.fGraph.Drive_LEDs(0, 0);
        }

        public void ThreadManualFindMarks(int maxCnt = 500)
        {
            bFinishThreadManualFindMarks = false;

            for (int i = 0; i < maxCnt; i++)
            {
                m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
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

                m__G.fGraph.Drive_LEDs(0, 0);
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





                m__G.oCam[0].LoadBMPtoBuf0(openFile.FileNames[0]);






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
                    m__G.oCam[0].LoadBMPtoBufN(filename, i++);
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
                    m__G.fVision.ProcessVisionData(numFile, maxThread);
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


        public void CalcVisionData(int cam, int ci, int ce, int cstep, int iBuf)
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
                            res = m__G.oCam[cam].FineCOG(false, i, iBuf);    // 마크찾기
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

                        res = m__G.oCam[cam].FineCOG(false, i, iBuf);    // 마크찾기
                        if (res)
                            mDebugCalcVisionCount[iBuf]++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            mb_FinishCalcVision[iBuf] = true;
        }


        private void ScanFOV()
        {
            int Cam = m_FocusedLED;
            int l_OrgROIV_min = v_OrgROIV_min[Cam];

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            int i = 1;
            for (; i < 1069 - v_OrgROIV_height[Cam]; i += 10)
            {
                v_OrgROIV_min[Cam] = i;
                tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
                SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
                Thread.Sleep(30);
            }
            for (; i > 10; i -= 10)
            {
                v_OrgROIV_min[Cam] = i;
                tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
                SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
                Thread.Sleep(30);
            }
            v_OrgROIV_min[Cam] = l_OrgROIV_min;
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
        }

        private void btnLEDDOWN_R_Click(object sender, EventArgs e)
        {
        }

        private void btnLEDUP_R_Click(object sender, EventArgs e)
        {
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            //m__G.fManage.ManualToPlot();
            cbContinuosMode.Checked = false;
            cbSaveNthImg.Checked = false;
            IsLiveCropStop = true;
            bThreadManualFindMarks = false;

            this.Hide();
            if (MyOwner.m_bAdmin)
            {
                MyOwner.ShowAdminMode();
            }
            else
            {
                MyOwner.ShowOperatorMode();
            }
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

        private void rbLED3_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void rbLED4_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void btnLoadUnloadAll_Click(object sender, EventArgs e)
        {
            if (!mSocketLoaded[0])
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "01LD";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                m__G.fGraph.socket_IN(0);
                mSocketLoaded[0] = true;
                if (m__G.mCamCount > 1)
                {
                    m__G.fGraph.socket_IN(1);
                    mSocketLoaded[1] = true;
                }
            }
            else
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "01UD";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                m__G.fGraph.socket_OUT(0);
                mSocketLoaded[0] = false;
                if (m__G.mCamCount > 1)
                {
                    m__G.fGraph.socket_OUT(1);
                    mSocketLoaded[1] = false;
                }
            }
        }

        private void btnFailLED1_Click(object sender, EventArgs e)
        {
            if (m__G.mChannelCount < 2)
                return;
            m__G.fGraph.mDriverIC.SetFailLED(0, true);
            Thread.Sleep(1000);
            m__G.fGraph.mDriverIC.SetFailLED(0, false);
        }

        private void btnFailLED2_Click(object sender, EventArgs e)
        {
            if (m__G.mChannelCount < 2)
                return;
            m__G.fGraph.mDriverIC.SetFailLED(1, true);
            Thread.Sleep(1000);
            m__G.fGraph.mDriverIC.SetFailLED(1, false);
        }

        private void btnFailLED3_Click(object sender, EventArgs e)
        {

        }

        private void btnFailLED4_Click(object sender, EventArgs e)
        {

        }

        public void LEDMarkCheck()
        {
            m_bLEDMarkCheck = true;

            int i = 0;
            while (m_bLEDMarkCheck)
            {
                if ((i % 2) == 0)
                    m__G.fGraph.Drive_LED(0, mLEDcurrent[0]);
                else
                    m__G.fGraph.Drive_LED(0, 0);
                if (m__G.mChannelCount > 1)
                {
                    if (((i / 2) % 2) == 0)
                        m__G.fGraph.Drive_LED(1, mLEDcurrent[1]);
                    else
                        m__G.fGraph.Drive_LED(1, 0);
                }

                if (m__G.mCamCount > 1)
                {
                    if (((i / 4) % 2) == 0)
                        m__G.fGraph.Drive_LED(2, mLEDcurrent[2]);
                    else
                        m__G.fGraph.Drive_LED(2, 0);

                    if (((i / 8) % 2) == 0)
                        m__G.fGraph.Drive_LED(3, mLEDcurrent[3]);
                    else
                        m__G.fGraph.Drive_LED(3, 0);
                }

                Thread.Sleep(100);
                i++;
            }
            m__G.fGraph.Drive_LED(0, 0);
            if (m__G.mChannelCount > 1)
                m__G.fGraph.Drive_LED(1, 0);
            if (m__G.mCamCount > 1)
            {
                m__G.fGraph.Drive_LED(2, 0);
                m__G.fGraph.Drive_LED(3, 0);
            }
        }

        private void btnLoadUnloadL_Click(object sender, EventArgs e)
        {
            if (m_FocusedLED > 1)
            {
                m_FocusedLED = 0;
            }

            int port = m_FocusedLED / 2;
            if (!mSocketLoaded[port])
            {
                m__G.fGraph.socket_IN(port);
                mSocketLoaded[port] = true;
                mDoneWriteRun = false;
            }
            else
            {
                m__G.fGraph.socket_OUT(port);
                mSocketLoaded[port] = false;
                mDoneWriteRun = false;
            }

        }
        public int saveCount = 0;
        private void btnAllLEDOn_Click(object sender, EventArgs e)
        {
            if (!m_bAllLEDOn)
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "02ON";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                m__G.mDoingStatus = "Checking Vision";

                //  CSH035 적용 시 
                m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));

                m_bAllLEDOn = true;

                btnAllLEDOn.ForeColor = Color.White;
            }
            else
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "02OF";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                //m__G.oCam[0].GrabA();
                //if (m__G.mCamCount > 1)
                //    m__G.oCam[1].GrabA();

                //  TZAF 검사기 적용 시
                //m__G.fGraph.mDriverIC.SetLEDpowers(0, 0, m__G.mCamCount);

                //  CSH035 적용 시 
                m__G.fGraph.Drive_LEDs(0, 0);

                //m__G.fVision.SetOrgExposure(0);
                //if (m__G.mCamCount > 1)
                //    m__G.fVision.SetOrgExposure(1);
                m_bAllLEDOn = false;
                btnAllLEDOn.ForeColor = Color.SlateGray;
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
            }
        }
        public int MeasureTXTY(ref double[] l_StrokeL, ref double[] l_StrokeR, ref double[] l_YawL, ref double[] l_YawR, ref double[] l_PitchL, ref double[] l_PitchR)
        {
            double[] sYaw = new double[4];
            double[] sPitch = new double[4];
            double[] cx = new double[8];
            double[] cy = new double[8];
            double[] sYaw2 = new double[4];
            double[] sPitch2 = new double[4];
            double[] cx2 = new double[8];
            double[] cy2 = new double[8];
            int res0 = 0;

            m__G.oCam[0].GrabA();

            m__G.oCam[0].FineCOG(true, 0, 0);

            return res0;
        }

        public void ToViet(bool IsToViet = true)
        {
            if (IsToViet)
            {
                btnBack.Text = "lùi lại sau";
                btnLive2.Text = "Video trực tiếp";
                btnHalt2.Text = "tạm dừng lại";
                btnClear1.Text = "khởi tạo";
                btnOISXReplay.Text = "phát lại X";
                btnOISXStepReplay.Text = "phát lại Step X";
                btnAllLEDOn.Text = "All LED OnOff";
                btnFOVUp.Text = "lên FOV";
                btnFOVLeft.Text = "Bên trái FOV";
                btnFOVRight.Text = "bên phải FOV";
                btnFOVDown.Text = "phía dưới FOV";
                btnFindMarks.Text = "tìm kiếm Marks";
                //btnSetAbsZero.Text = "đặt số không with Master Spl";

                rbLED1.Text = "Bên trái LED";
                rbLED2.Text = "bên phải LED";
            }
            else
            {
                btnBack.Text = "Back";
                btnLive2.Text = "Live";
                btnHalt2.Text = "Halt";
                btnClear1.Text = "Clear";
                btnOISXReplay.Text = "X Replay";
                btnOISXStepReplay.Text = "AF Step Replay	";
                btnAllLEDOn.Text = "All LED OnOff";
                btnFOVUp.Text = "FOV Up";
                btnFOVLeft.Text = "FOV Right";
                btnFOVRight.Text = "FOV Left";
                btnFOVDown.Text = "FOV Down";
                btnFindMarks.Text = "Find Marks";
                //btnSetAbsZero.Text = "Set Zero with Master Spl";

                rbLED1.Text = "LED Left";
                rbLED2.Text = "LED Right";
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


            string strtmp = NthMeasure(imgIndex);

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

        public string NthMeasure(int imgIndex, bool bAccu = false)
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

            m__G.oCam[0].mTrgBufLength = m__G.oCam[0].mTargetTriggerCount;
            //tbVsnLog.Text += "Target Length = " + m__G.oCam[0].mTrgBufLength.ToString();
            m__G.oCam[0].DispCommonImage(imgIndex);

            if (!bAccu)
                tbVsnLog.Text = "";

            //tbVsnLog.Text += "Target Length = " + m__G.oCam[0].mTrgBufLength.ToString();
            bool IsShow = cbSaveNthImg.Checked;

            m__G.oCam[0].mMatroxMsg = "";
            //m__G.oCam[0].DispCommonImage(imgIndex);


            //m__G.oCam[0].ResizeGrab(imgIndex);

            if (IsShow)
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

            if (m__G.oCam[0].mFAL.mBreakIndex == imgIndex)
                IsShow = cbSaveNthImg.Checked;

            if (imgIndex == 0)
                m__G.oCam[0].FineCOG(true, imgIndex, 0, IsShow, true, true);// ref sYaw, ref sPitch, ref cx, ref cy, ref nFound, cbSaveNthImg.Checked , 0, m__G.sModelName);   // 마크찾기
            else
                m__G.oCam[0].FineCOG(false, imgIndex, 0, IsShow, true, true);// ref sYaw, ref sPitch, ref cx, ref cy, ref nFound, cbSaveNthImg.Checked , 0, m__G.sModelName);   // 마크찾기

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

        public void Process_AutoLED(bool IsFlipped = false)
        {

            double[] sPitch = new double[4];
            double[] sYaw = new double[4];
            double[] cx = new double[4];
            double[] cy = new double[4];

            double[] strokeL = new double[4];
            double[] yawL = new double[4];
            double[] pitchL = new double[4];
            double[] refBin = new double[4];
            double leftBin = 0;
            double rightBin = 0;

            //long nFound = 0;

            double Lpower = 2.63;
            double Rpower = 2.63;
            int limit = 16;
            while (limit-- > 0)
            {
                m__G.oCam[0].DrawClear();
                if (m__G.mCamCount > 1)
                {
                    m__G.oCam[1].DrawClear();
                }

                m__G.fGraph.Drive_LED(0, Lpower);
                if (m__G.mCamCount > 1)
                    m__G.fGraph.Drive_LED(1, Rpower);

                Thread.Sleep(20);

                if (m__G.mCamCount > 1)
                    m__G.oCam[1].GrabA(1);

                m__G.oCam[0].GrabA(1);
                m__G.fGraph.Drive_LEDs(0, 0);

                refBin = new double[4];
                m__G.oCam[0].GetRefBrightness(cx, cy, ref refBin);

                leftBin = (refBin[0] + refBin[1]) / 2.0;
                rightBin = (refBin[2] + refBin[3]) / 2.0;
                tbVsnLog.Text += Lpower.ToString("F2") + " - " + leftBin.ToString("F3") + " : " + Rpower.ToString("F2") + " - " + rightBin.ToString("F3") + "\r\n";
                if (!IsFlipped)
                {
                    if (leftBin < 23 && Lpower < 2.78)
                        Lpower += 0.01;
                    if (rightBin < 23 && Rpower < 2.78)
                        Rpower += 0.01;
                }
                else
                {
                    if (leftBin < 23 && Rpower < 2.78)
                        Rpower += 0.01;
                    if (rightBin < 23 && Lpower < 2.78)
                        Lpower += 0.01;
                }

                if (leftBin > 23 && rightBin > 23)
                    break;

                if (Lpower > 2.74 && Rpower > 2.74)
                    break;
            }
            tbVsnLog.Text += "Auto LED Power : " + Lpower.ToString("F2") + "\t" + Rpower.ToString("F2") + "\r\n";
        }

        private void button4_Click(object sender, EventArgs e)
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
            //m__G.oCam[0].mFAL.mFastMode = m__G.m_bFastMode;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23

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
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;

                SupremeTimer.QueryPerformanceCounter(ref beginTime);
                m__G.fVision.ProcessVisionData(imgIndex, lmaxThread);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ellapsedTime = (endTime - beginTime) / (double)(lTimerFrequency);

                strtmp = strGrp[ci % 2] + "#\tLed\tX\tY\tZ\tTX\tTY\tTZ\tX1\tY1\tX2\tY2\tX3\tY3\tX4\tY4\tX5\tY5";
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
            btnFOVUp.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrU2;

            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }

            if (mbMotorizedStage)
            {
                if (cbZaxis.Checked)
                {
                    MotorJogRun(Axis.Z, false, speedLevel);   // Z
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TX, true, speedLevel);   // TX
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, true, speedLevel);   // TZ
                }
                else
                {
                    MotorJogRun(Axis.Y, true, speedLevel);   // Y
                }
            }
        }

        private void btnFOVUp_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVUp.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrU;
            if (mbMotorizedStage)
            {
                //MotorJogStop();

                if (cbZaxis.Checked)
                {
                    MotorJogStop(Axis.Z);
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TX);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.Y);
                }
            }
        }

        private void btnFOVLeft_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrL2;
            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }


            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TY, false, speedLevel);   // TY
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, false, speedLevel);  // TZ
                }
                else
                {
                    MotorJogRun(Axis.X, true, speedLevel);   // X
                }
            }
        }

        private void btnFOVLeft_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrL;
            if (mbMotorizedStage)
            {
                //MotorJogStop();

                if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TY);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.X);
                }
            }
        }

        private void btnFOVDown_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrD2;

            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }


            if (mbMotorizedStage)
            {

                if (cbZaxis.Checked)
                {
                    MotorJogRun(Axis.Z, true, speedLevel);  // Z
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TX, false, speedLevel);  // TX
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, false, speedLevel);  // TZ
                }
                else
                {
                    MotorJogRun(Axis.Y, false, speedLevel);  // Y
                }

            }
        }

        private void btnFOVDown_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrD;
            if (mbMotorizedStage)
            {
                // MotorJogStop();

                if (cbZaxis.Checked)
                {
                    MotorJogStop(Axis.Z);
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TX);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.Y);
                }
            }
        }

        private void btnFOVRight_MouseDown(object sender, MouseEventArgs e)
        {

            btnFOVRight.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrR2;

            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }


            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TY, true, speedLevel);   // TY
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, true, speedLevel);   // TZ
                }
                else
                {
                    MotorJogRun(Axis.X, false, speedLevel);   // X
                }
            }
        }

        private void btnFOVRight_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVRight.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrR;
            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TY);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.X);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

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

            string strtmp = NthMeasure(imgIndex);
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
        private void button11_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].HaltA();
            m__G.mDoingStatus = "Checking Vision";

            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            //cbContinuosMode.Checked = true;
            Thread.Sleep(100);

            if (tbFrameToGrab.Text.Length > 0)
                m__G.oCam[0].mTargetTriggerCount = Convert.ToInt32(tbFrameToGrab.Text);
            else
                m__G.oCam[0].mTargetTriggerCount = 1000;

            if (m__G.oCam[0].mTargetTriggerCount > 3000)
                m__G.oCam[0].mTargetTriggerCount = 3000;

            m__G.oCam[0].mRequestedTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].dAFZM_FrameCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].mTrgBufLength = m__G.oCam[0].mTargetTriggerCount;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[m__G.oCam[0].dAFZM_FrameCount];

            m__G.mbSuddenStop[0] = true;        //  왜 ?
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in button11()");
            int lgrabbedFrame = 0;
            double frameRate = 0;

            //SetOrgExposure(0);

            if (cbContinuosMode.Checked)
            {
                tbVsnLog.Text += "Target Trigger Count = " + m__G.oCam[0].mTargetTriggerCount.ToString();
                for (int i = 0; i < m__G.oCam[0].mTargetTriggerCount; i++)
                    m__G.oCam[0].GrabB(i, true);

                tbGrabbedFrame.Text = m__G.oCam[0].mTargetTriggerCount.ToString();
                label10.Text = " ~ frame/sec";
                //m__G.fVision.SetOrgExposure(0);
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;

                tbVsnLog.Text = "Continuous Mode Grab.\r\n";
                tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
                tbVsnLog.ScrollToCaret();
                return;
            }


            //m__G.oCam[0].mFinishVisionData = true;
            m__G.oCam[0].mFinishVisionData = false;
            m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);
            m__G.oCam[0].mFinishVisionData = false;

            m__G.fGraph.Drive_LEDs(0, 0);

            tbGrabbedFrame.Text = lgrabbedFrame.ToString();
            tbImgNumber.Text = m__G.oCam[0].mTargetTriggerCount.ToString();
            label10.Text = frameRate.ToString("F1") + " frame/sec";

            //m__G.fVision.SetOrgExposure(0);
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;

            string lstr = "";
            for (int i = 0; i < lgrabbedFrame; i++)
                lstr += m__G.oCam[0].mGrabAbsTiming[i].ToString("F4") + "\r\n";

            tbVsnLog.Text = lstr;
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();
        }

        public void btnTriggerGrab()
        {
            m__G.mbSuddenStop[0] = true;        //  왜 ?
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in btnTriggerGrab()");
            int lgrabbedFrame = 0;
            double frameRate = 0;

            //SetOrgExposure(0);

            m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);

            //lgrabbedFrame = m__G.oCam[1].ExternalTriggerOrg(ref frameRate);

            tbGrabbedFrame.Text = lgrabbedFrame.ToString();
            label10.Text = frameRate.ToString("F1") + " frame/sec";

            //m__G.fVision.SetOrgExposure(0);

            string lstr = "";
            for (int i = 0; i < lgrabbedFrame; i++)
                lstr += m__G.oCam[0].mGrabAbsTiming[i].ToString("F4") + "\r\n";

            tbInfo.Text = lstr;
            tbInfo.SelectionStart = tbInfo.Text.Length;
            tbInfo.ScrollToCaret();
        }

        //private void button13_Click(object sender, EventArgs e)
        //{
        //    m__G.oCam[0].ClearDisp();
        //    m__G.oCam[0].DrawClear();
        //    if (m__G.mCamCount > 1)
        //    {
        //        m__G.oCam[1].ClearDisp();
        //        m__G.oCam[1].DrawClear();
        //    }

        //    //button13.Enabled = false;

        //    Thread ThreadReplayL = new Thread(() => Process_TriggerReplay(0));
        //    ThreadReplayL.Start();
        //    //m_replayIndex = 0;
        //}
        //public void Process_TriggerReplay(int num)
        //{
        //    int max = m__G.oCam[0].dAFZM_FrameCount;
        //    for (int n = 0; n < max; n++)
        //    {
        //        m__G.oCam[0].BufCopy2Disp_TRG(n);
        //    }
        //    //button13.Enabled = true;
        //}

        private void tbVsnLog_TextChanged(object sender, EventArgs e)
        {

        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    m__G.fGraph.Drive_LEDs(mLEDcurrent[0], mLEDcurrent[1]);
        //    m__G.fVision.SetOrgExposure(0);

        //    SupremeTimer.QueryPerformanceFrequency(ref m__G.TimerFrequency);

        //    long startTime = 0;
        //    long endTime = 0;
        //    double Ellapsed = 0;

        //    SupremeTimer.QueryPerformanceCounter(ref startTime);
        //    double[] avgBin = new double[1000];
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        m__G.oCam[0].GrabA(i);
        //        m__G.oCam[0].GetAvgBin(i, ref avgBin[i]);
        //    }

        //    m__G.fGraph.Drive_LEDs(0, 0);
        //    SupremeTimer.QueryPerformanceCounter(ref endTime);
        //    Ellapsed = 1000000 * (endTime - startTime) / (double)(m__G.TimerFrequency);
        //    string lstr = "";
        //    for (int i = 0; i < 1000; i++)
        //        lstr += avgBin[i].ToString("F3") + "\r\n";

        //    tbVsnLog.Text = lstr;
        //    tbVsnLog.Text += "Ellapsed " + Ellapsed.ToString("F0") + "usec " + mLEDcurrent[0].ToString("F2") + " " + mLEDcurrent[1].ToString("F2") + "\r\n";
        //    tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
        //    tbVsnLog.ScrollToCaret();
        //}

        bool mStopMonitorMTF = false;
        //private void btnMonitorMTF_Click(object sender, EventArgs e)
        //{
        //    if (btnMonitorMTF.Text == "Stop Monitor MTF")
        //    {
        //        mStopMonitorMTF = true;
        //        btnMonitorMTF.Text = "Monitor MTF";
        //        ChartMTF.Hide();
        //        m__G.mDoingStatus = "IDLE";
        //        m__G.mIDLEcount = 0;

        //    }
        //    else
        //    {
        //        mStopMonitorMTF = false;
        //        btnMonitorMTF.Text = "Stop Monitor MTF";
        //        ChartMTF.Show();
        //        Thread threadMonitorMTF = new Thread(() => MonitorMTF());
        //        threadMonitorMTF.Start();
        //        m__G.mDoingStatus = "Checking Vision";
        //    }
        //}
        double[][] mSeriesMTF = new double[12][];

        //public void MonitorMTF()
        //{
        //    for (int i = 0; i < 12; i++)
        //        mSeriesMTF[i] = new double[1000];

        //    int waitTime = 0;
        //    while (true)
        //    {
        //        if (m__G.fVision.mLoaded)
        //            break;
        //        Thread.Sleep(200);
        //        if (waitTime++ > 50)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    mStopMonitorMTF = true;
        //                    btnMonitorMTF.Text = "Monitor MTF";
        //                });
        //            }
        //            return;
        //        }
        //    }
        //    waitTime = 0;
        //    while (true)
        //    {
        //        if (m__G.oCam[0].mFAL.mFAutoLearnLoaded)
        //            break;
        //        Thread.Sleep(200);
        //        if (waitTime++ > 50)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    mStopMonitorMTF = true;
        //                    btnMonitorMTF.Text = "Monitor MTF";
        //                });
        //            }
        //            return;
        //        }
        //    }
        //    //m__G.fVision.SetOrgExposure(0);

        //    int index = 0;
        //    //int cindex = 0;
        //    int effFrameCount = 0;
        //    bool[] res = new bool[12] { true, true, true, true, true, true, true, true, true, true, true, true };
        //    Thread[] threadRealTimeVisionData = new Thread[12];

        //    m__G.oCam[0].GrabA_User(0);
        //    m__G.oCam[0].FineCOG(true, 0, 0);
        //    index++;
        //    //int oldindex = 0;
        //    //int indexThread = 0;
        //    int updatePlot = 0;
        //    long lTimerFrequency = 0;
        //    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

        //    //double lRealTimeUnit = 8.0 / 500.0;    //  Default Value
        //    double[] maMTF = new double[7];
        //    while (!mStopMonitorMTF)
        //    {
        //        int indexNow = index % 12;
        //        int index9999 = index % 1000 + 1;

        //        m__G.oCam[0].GrabB(3000);

        //        if (mStopMonitorMTF)
        //            break;

        //        m__G.oCam[0].FineMTF(false, index9999, 0);

        //        updatePlot++;
        //        //    //  MTF 그래프를 그리기 위해 측정된 데이터를 그래프 버퍼에 복사해준다.
        //        //if (index < 1000)
        //        //{
        //        //    for (int i = 0; i < 12; i++)
        //        //        Array.Copy(m__G.oCam[0].mBufMTF[i], 0, mSeriesMTF[i], 0, index);
        //        //}
        //        //else
        //        //{
        //        //    //Array.Copy(m__G.oCam[0].mC_pX, index9999, mStroke[0], 0, 1000 - (index9999));
        //        //    for (int i = 0; i < 12; i++)
        //        //        Array.Copy(m__G.oCam[0].mBufMTF[i], index9999, mSeriesMTF[i], 0, 1000 - (index9999));

        //        //}

        //        for (int i = 0; i < 6; i++)
        //            maMTF[i] += m__G.oCam[0].mBufMTF[i][index9999];

        //        if (mStopMonitorMTF)
        //            break;

        //        if (updatePlot % 20 == 1)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    //PlotChartMTF(effFrameCount);
        //                    string lstr = "";
        //                    for (int i = 0; i < 6; i++)
        //                    {
        //                        lstr += maMTF[i].ToString("F0") + "\t";
        //                        maMTF[6] += maMTF[i];
        //                    }

        //                    tbVsnLog.Text += lstr + "\t" + maMTF[6].ToString("F0") + "\r\n";
        //                    tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
        //                    tbVsnLog.ScrollToCaret();
        //                    for (int i = 0; i < 7; i++)
        //                        maMTF[i] = 0;

        //                });
        //            }
        //        }
        //        Thread.Sleep(1);
        //        if (mStopMonitorMTF)
        //            break;

        //        index++;
        //        if (effFrameCount < 1000)
        //            effFrameCount++;
        //        else
        //            effFrameCount = 1000;

        //    }
        //    m__G.fGraph.Drive_LEDs(0, 0);
        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();
        //    m__G.oCam[0].dAFZM_FrameCount = index % 1000;
        //    m__G.mbSuddenStop[0] = false;
        //}

        //public void PlotChartMTF(int frameCnt)
        //{
        //    if (ChartMTF.InvokeRequired)
        //    {
        //        ChartMTF.Invoke(new MethodInvoker(delegate ()
        //        {
        //            _PlotChartMTF(frameCnt);
        //        }));
        //    }
        //    else
        //        _PlotChartMTF(frameCnt);
        //    //ChartMTF
        //}
        //public void _PlotChartMTF(int frameCnt)
        //{
        //    ChartMTF.Series.Clear();


        //    if (frameCnt == 0)
        //    {
        //        ChartMTF.Series.Add("Default");
        //        ChartMTF.Series[0].Points.AddXY(1, 0);
        //        return;
        //    }
        //    for (int i = 0; i < frameCnt; i++)
        //    {
        //        for (int which = 0; which < 12; which++)
        //        {
        //            if (mSeriesMTF[which][i] == 0) continue;
        //            ChartMTF.Series[0].Points.AddXY(i / 1000.0, mSeriesMTF[which][i]); //  FWD
        //        }
        //    }
        //    ChartMTF.ChartAreas[0].AxisX.Minimum = 0;
        //    ChartMTF.ChartAreas[0].AxisX.Maximum = (frameCnt / 1000.0);
        //    ChartMTF.ChartAreas[0].AxisX.Interval = 0.1;
        //    ChartMTF.ChartAreas[0].AxisY.Minimum = 0;
        //    ChartMTF.ChartAreas[0].AxisY.Maximum = 1;
        //    ChartMTF.ChartAreas[0].AxisY.Interval = 0.1;

        //    ChartMTF.Titles.Clear();
        //    ChartMTF.Titles.Add("MTF");


        //}
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


            m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기

            if (count < HowManyThread)
            {
                mb_FinishCalcVision[0] = false;
                mDebugCalcVisionCount[0] = 0;
                m__G.oCam[0].FineCOG(true, 0, 0, true, false, IsFile);    // 마크찾기
                if (count > 1)
                {
                    CalcVisionData(0, 0, count, 1, 0);
                }
                mb_FinishCalcVision[0] = true;
                m__G.oCam[0].mFinishVisionData = true;    //  맞다
                SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                ltime = (endTime - startTime) / (double)(lTimerFrequency);
                return ltime;
            }

            Task[] Task_CalcVisionData = new Task[HowManyThread];

            int halfCnt = count / 2;
            int lastIndx = count - 1;
            int halfThread = HowManyThread / 2;
            HowManyThread = halfThread * 2;

            List<int> taskIndices = new List<int>();
            List<int> lastImgIndex = new List<int>();
            for (int i = 0; i < halfThread; i++)
            {
                mb_FinishCalcVision[i] = false;
                mDebugCalcVisionCount[i] = 0;
                mb_FinishCalcVision[halfThread + i] = false;
                mDebugCalcVisionCount[halfThread + i] = 0;

                taskIndices.Add(i);
                lastImgIndex.Add(lastIndx - i);
            }

            if (HowManyThread <= 1)
                CalcVisionData(0, 0, count, 1, 1);
            else
            {
                Parallel.ForEach(taskIndices, taskIndex =>
                {
                    CalcVisionData(0, taskIndex, halfCnt, halfThread, taskIndex * 2);
                    CalcVisionData(0, lastIndx - taskIndex, halfCnt, halfThread, taskIndex * 2 + 1);
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
            button11.Enabled = false;
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
            button11.Enabled = true;
            cbContinuosMode.Enabled = true;

            Process currentProc = Process.GetCurrentProcess();
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

            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
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

            TextAppendTbInfo("Saved scales");

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

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");

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

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");
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
                TextAppendTbInfo("Loaded scales");

                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");

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
                tbInfo.Font = new Font("Calibri", 14, FontStyle.Bold);
                bLiveFindMark = true;
                Task.Run(() => LiveFindMark());
            }
            else
            {
                bLiveFindMark = false;
                tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
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


        private void button3_Click(object sender, EventArgs e)
        {
            m__G.mDoingStatus = "Checking Vision";
            //m__G.mDoingStatus = "IDLE";
            DrawMarkPositions();

            m__G.oCam[0].GrabB(1);

            string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\SetZeroGrab.bmp";
            m__G.oCam[0].SaveImageBuf(fname);

            m__G.oCam[0].GrabB(2);
            m__G.oCam[0].GrabB(3);
            m__G.oCam[0].GrabB(4);
            m__G.oCam[0].GrabB(5);

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();

            double ltx = 0;
            double lty = 0;
            double ltz = 0;

            //MessageBox.Show("Call SetTXTYOffset 1");
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);

            for (int i = 1; i < 6; i++)
            {
                FindMarks(i);
                //lx = m__G.oCam[0].mC_pX[i] * 5.5 / Global.LensMag;    //  Pixel to um
                //ly = m__G.oCam[0].mC_pY[i] * 5.5 / Global.LensMag;    //  Pixel to um
                //lz = m__G.oCam[0].mC_pZ[i] * 5.5 / Global.LensMag;    //  Pixel to um
                ltx += m__G.oCam[0].mC_pTX[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
                lty += m__G.oCam[0].mC_pTY[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
                ltz += m__G.oCam[0].mC_pTZ[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            }
            ltx = ltx / 5;
            lty = lty / 5;
            ltz = ltz / 5;
            double masterTx = 0;
            double masterTy = 0;
            double masterTz = 0;
            double orgMasterTx = double.Parse(tbMasterTX.Text);
            double orgMasterTy = double.Parse(tbMasterTY.Text);
            double orgMasterTz = double.Parse(tbMasterTZ.Text);
            masterTz = orgMasterTz;
            try
            {
                if (m__G.m_bXTiltReverse)
                    masterTx = -orgMasterTx;
                else
                    masterTx = orgMasterTx;
            }
            catch (Exception err)
            {
                tbMasterTX.Text = "0";
            }
            try
            {
                if (m__G.m_bYTiltReverse)
                    masterTy = -orgMasterTy;
                else
                    masterTy = orgMasterTy;
            }
            catch (Exception err)
            {
                tbMasterTY.Text = "0";
            }

            //if (m__G.m_bXTiltReverse)
            //    masterTx = -masterTx;
            //if (m__G.m_bXTiltReverse)
            //    masterTy = -masterTy;

            //  ltx, lty, ltz 는 측정값 (radian)
            //  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)
            //  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
            SaveTXTYZeroOffset(ltx, lty, ltz, masterTx, masterTy, masterTz, orgMasterTx, orgMasterTy, orgMasterTz);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        }
        public void SaveTXTYZeroOffset(double tx, double ty, double tz, double masterTx, double masterTy, double masterTz, double orgMasterTx, double orgMasterTy, double orgMasterTz)
        {
            //MessageBox.Show("Call SetTXTYOffset 2");
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(tx - masterTx * MIN_To_RAD, ty - masterTy * MIN_To_RAD, tz - masterTz * MIN_To_RAD);

            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\TXTYTZoffset_" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);
            wr.WriteLine(tx.ToString());
            wr.WriteLine(ty.ToString());
            wr.WriteLine(tz.ToString());

            wr.WriteLine(orgMasterTx.ToString());
            wr.WriteLine(orgMasterTy.ToString());
            wr.WriteLine(orgMasterTz.ToString());
            wr.Close();
        }
        public string LoadTXTYZeroOffset()
        {
            //MessageBox.Show("LoadscaleNTheta called ");
            string txtyZeroFile = m__G.m_RootDirectory + "\\DoNotTouch\\TXTYTZoffset_" + camID0 + ".txt";
            if (!File.Exists(txtyZeroFile))
            {
                //MessageBox.Show("Call SetTXTYOffset 3");
                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0);
                //m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);
                return "TX TY TZ offset = 0,0,0";
            }
            try
            {
                StreamReader rd = new StreamReader(txtyZeroFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 3)
                    return "TX TY TZ offset = 0,0,0";
                double tx = double.Parse(eachLine[0]);
                double ty = double.Parse(eachLine[1]);
                double tz = double.Parse(eachLine[2]);
                double masterTx = 0;
                double masterTy = 0;
                double masterTz = 0;
                double orgMasterTx = 0;
                double orgMasterTy = 0;
                double orgMasterTz = 0;
                if (eachLine.Length > 3)
                {
                    orgMasterTx = double.Parse(eachLine[3]);
                    orgMasterTy = double.Parse(eachLine[4]);
                    if (eachLine.Length > 5)
                        if (eachLine[5].Length > 0)
                            orgMasterTz = double.Parse(eachLine[5]);    //  없는 경우 대비

                    if (m__G.m_bXTiltReverse)
                        masterTx = -orgMasterTx;
                    else
                        masterTx = orgMasterTx;
                    if (m__G.m_bYTiltReverse)
                        masterTy = -orgMasterTy;
                    else
                        masterTy = orgMasterTy;

                    masterTz = orgMasterTz;

                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(tx - masterTx * MIN_To_RAD, ty - masterTy * MIN_To_RAD, tz - masterTz * MIN_To_RAD);
                }
                tbMasterTX.Text = orgMasterTx.ToString("F2");
                tbMasterTY.Text = orgMasterTy.ToString("F2");
                tbMasterTZ.Text = orgMasterTz.ToString("F2");
                return "TX TY TZ offset = " + orgMasterTx.ToString("F2") + "," + orgMasterTy.ToString("F2") + "," + orgMasterTz.ToString("F2");
                //m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ);
            }
            catch (Exception e)
            {

            }
            return "TX TY offset = 0,0";
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
            SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0);
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
                button6.Enabled = true;
                button3.Enabled = true;
                tbMasterTX.Enabled = true;
                tbMasterTY.Enabled = true;
            }
            else
            {
                button6.Enabled = false;
                button3.Enabled = false;
                tbMasterTX.Enabled = false;
                tbMasterTY.Enabled = false;
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
            LoadScaleNTheta();
            LoadTXTYZeroOffset();
            // 241206 YLUT 적용안함.
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[1];

            pictureBox2.Image = m__G.oCam[0].LoadCropImg(0);
            string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            m__G.oCam[0].SaveGrabbedImage(0, fileName);

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

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].mFAL.mbGetHistogram = true;
                NthMeasure(0);
                m__G.oCam[0].mFAL.mbGetHistogram = false;

                sx = m__G.oCam[0].mC_pX[0] * umscale;
                sy = m__G.oCam[0].mC_pY[0] * umscale;
                sz = m__G.oCam[0].mC_pZ[0] * umscale;
                tx = m__G.oCam[0].mC_pTX[0] * minscale;
                ty = m__G.oCam[0].mC_pTY[0] * minscale;
                tz = m__G.oCam[0].mC_pTZ[0] * minscale;
                strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t\tContrast\t";
                for (int i = 0; i < 5; i++)
                    strtmp += m__G.oCam[0].mFAL.mEffectiveContrast[i].ToString() + "\t";

                strtmp += "( > 20 )";


            }
            DrawMarkDetected();

            tbInfo.Text += strtmp + "\r\n";

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;

            bHaltLive = true;
            IsLiveCropStop = true;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            GrabInitalMark();
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
            SetExposure(0, m__G.sRecipe.iExposure);
            SetRawGainNGamma(m__G.sRecipe.iRawGain, m__G.sRecipe.iGamma);
            SetEdgeBand(m__G.sRecipe.iEdgeBand);

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
        public void JH_SK_CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        {
            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return;
            }

            string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
            if (!Directory.Exists(AdminPathName))
                Directory.CreateDirectory(AdminPathName);

            int fullLength = measure.Length;
            StreamWriter wr = null;
            //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

            //  안정화 유효데이터를 추출한다.
            //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

            List<double[]> stablizedData = new List<double[]>();

            int effLength = 0;
            //double a = 0;
            //double b = 0;
            int[] effIndex = null;
            FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTZTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sTXtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTXtoTZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYtoTZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTZtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTZtoTY = new FZMath.Point2D[effLength];

            double[] XtoXab = new double[3];
            double[] YtoYab = new double[3];
            double[] ZtoZab = new double[3];
            double[] TXtoTXab = new double[3];
            double[] TYtoTYab = new double[3];
            double[] TZtoTZab = new double[3];

            double[] XtoYab = new double[3];
            double[] XtoZab = new double[3];
            double[] XtoTXab = new double[2];
            double[] XtoTYab = new double[2];
            double[] XtoTZab = new double[2];

            double[] YtoXab = new double[3];
            double[] YtoZab = new double[3];
            double[] YtoTXab = new double[2];
            double[] YtoTYab = new double[2];
            double[] YtoTZab = new double[2];

            double[] ZtoXab = new double[3];
            double[] ZtoYab = new double[3];
            double[] ZtoTXab = new double[3];
            double[] ZtoTYab = new double[3];
            double[] ZtoTZab = new double[3];

            double[] TXtoTYab = new double[3];
            double[] TXtoTZab = new double[3];

            double[] TYtoTXab = new double[3];
            double[] TYtoTZab = new double[3];

            double[] TZtoTXab = new double[3];
            double[] TZtoTYab = new double[3];

            if (!IsRemote)
            {
                switch (axis)
                {
                    case "Z":
                        effIndex = ExtractStablizedIndex(measure, 2);
                        break;

                    case "X":
                        effIndex = ExtractStablizedIndex(measure, 0);
                        break;
                    case "Y":
                        effIndex = ExtractStablizedIndex(measure, 1);
                        break;
                    case "TX":
                        effIndex = ExtractStablizedIndex(measure, 3);
                        break;
                    case "TY":
                        effIndex = ExtractStablizedIndex(measure, 4);
                        break;
                    default:
                        break;
                }
                effLength = effIndex.Length;

                for (int i = 0; i < effLength; i++)
                {
                    szy1[i] = new FZMath.Point2D();
                    szy2[i] = new FZMath.Point2D();
                    szy3[i] = new FZMath.Point2D();

                    sZZ[i] = new FZMath.Point2D();
                    sXX[i] = new FZMath.Point2D();
                    sYY[i] = new FZMath.Point2D();
                    sTXTX[i] = new FZMath.Point2D();
                    sTYTY[i] = new FZMath.Point2D();
                    sTZTZ[i] = new FZMath.Point2D();

                    sXtoTX[i] = new FZMath.Point2D();
                    sXtoTY[i] = new FZMath.Point2D();
                    sXtoTZ[i] = new FZMath.Point2D();
                    sYtoTX[i] = new FZMath.Point2D();
                    sYtoTY[i] = new FZMath.Point2D();
                    sYtoTZ[i] = new FZMath.Point2D();
                    sZtoTX[i] = new FZMath.Point2D();
                    sZtoTY[i] = new FZMath.Point2D();
                    sZtoTZ[i] = new FZMath.Point2D();

                    sTXtoTY[i] = new FZMath.Point2D();
                    sTXtoTZ[i] = new FZMath.Point2D();
                    sTYtoTX[i] = new FZMath.Point2D();
                    sTYtoTZ[i] = new FZMath.Point2D();
                    sTZtoTX[i] = new FZMath.Point2D();
                    sTZtoTY[i] = new FZMath.Point2D();
                }

                if (effLength == 0)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbInfo.Text += "Stabilized data not found\r\n";
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        });
                    }
                    else
                    {
                        tbInfo.Text += "Stabilized data not found\r\n";
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                    return;

                }
                for (int i = 0; i < effLength; i++)
                {
                    double[] lstbData = new double[22];
                    for (int j = 0; j < 22; j++)
                        lstbData[j] = measure[effIndex[i]][j];

                    stablizedData.Add(lstbData);
                }
            }
            else
            {
                //  Remote or Auto Calibration
                effLength = measure.Length;

                szy1 = new FZMath.Point2D[effLength];
                szy2 = new FZMath.Point2D[effLength];
                szy3 = new FZMath.Point2D[effLength];

                sZZ = new FZMath.Point2D[effLength];
                sXX = new FZMath.Point2D[effLength];
                sYY = new FZMath.Point2D[effLength];
                sTXTX = new FZMath.Point2D[effLength];
                sTYTY = new FZMath.Point2D[effLength];
                sTZTZ = new FZMath.Point2D[effLength];

                sXtoY = new FZMath.Point2D[effLength];
                sXtoZ = new FZMath.Point2D[effLength];
                sXtoTX = new FZMath.Point2D[effLength];
                sXtoTY = new FZMath.Point2D[effLength];
                sXtoTZ = new FZMath.Point2D[effLength];

                sYtoX = new FZMath.Point2D[effLength];
                sYtoZ = new FZMath.Point2D[effLength];
                sYtoTX = new FZMath.Point2D[effLength];
                sYtoTY = new FZMath.Point2D[effLength];
                sYtoTZ = new FZMath.Point2D[effLength];

                sZtoX = new FZMath.Point2D[effLength];
                sZtoY = new FZMath.Point2D[effLength];
                sZtoTX = new FZMath.Point2D[effLength];
                sZtoTY = new FZMath.Point2D[effLength];
                sZtoTZ = new FZMath.Point2D[effLength];

                sTXtoTY = new FZMath.Point2D[effLength];
                sTXtoTZ = new FZMath.Point2D[effLength];

                sTYtoTX = new FZMath.Point2D[effLength];
                sTYtoTZ = new FZMath.Point2D[effLength];

                sTZtoTX = new FZMath.Point2D[effLength];
                sTZtoTY = new FZMath.Point2D[effLength];

                for (int i = 0; i < effLength; i++)
                {
                    szy1[i] = new FZMath.Point2D();
                    szy2[i] = new FZMath.Point2D();
                    szy3[i] = new FZMath.Point2D();

                    sZZ[i] = new FZMath.Point2D();
                    sXX[i] = new FZMath.Point2D();
                    sYY[i] = new FZMath.Point2D();
                    sTXTX[i] = new FZMath.Point2D();
                    sTYTY[i] = new FZMath.Point2D();
                    sTZTZ[i] = new FZMath.Point2D();

                    sXtoY[i] = new FZMath.Point2D();
                    sXtoZ[i] = new FZMath.Point2D();
                    sXtoTX[i] = new FZMath.Point2D();
                    sXtoTY[i] = new FZMath.Point2D();
                    sXtoTZ[i] = new FZMath.Point2D();

                    sYtoX[i] = new FZMath.Point2D();
                    sYtoZ[i] = new FZMath.Point2D();
                    sYtoTX[i] = new FZMath.Point2D();
                    sYtoTY[i] = new FZMath.Point2D();
                    sYtoTZ[i] = new FZMath.Point2D();

                    sZtoX[i] = new FZMath.Point2D();
                    sZtoY[i] = new FZMath.Point2D();
                    sZtoTX[i] = new FZMath.Point2D();
                    sZtoTY[i] = new FZMath.Point2D();
                    sZtoTZ[i] = new FZMath.Point2D();

                    sTXtoTY[i] = new FZMath.Point2D();
                    sTXtoTZ[i] = new FZMath.Point2D();

                    sTYtoTX[i] = new FZMath.Point2D();
                    sTYtoTZ[i] = new FZMath.Point2D();

                    sTZtoTX[i] = new FZMath.Point2D();
                    sTZtoTY[i] = new FZMath.Point2D();
                }

                for (int i = 0; i < effLength; i++)
                    stablizedData.Add(measure[i]);
            }

            //////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////
            //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
            StreamWriter lwr = null;
            double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

            if (!IsRemote)
            {
                lwr = new StreamWriter(AdminPathName + "FullData.csv");
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
                int k = 0;
                for (int i = 0; i < fullLength; i++)
                {
                    string slstr = i.ToString() + ",";
                    for (int j = 0; j < 23; j++)
                        slstr += measure[i][j].ToString("F5") + ",";
                    if (i == effIndex[k])
                    {
                        slstr += "*";
                        k++;
                    }
                    lwr.WriteLine(slstr);
                    if (k == effLength)
                        break;
                }
                lwr.Close();
            }

            string calName = axis;
            if (isAutoCalibrationEastView)
            {
                calName += "_EastView";
            }

            string strStabilizedFile = "";
            if (mAutoCalibrationCount % 2 == 0)
                strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
            else
                strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

            try
            {
                lwr = new StreamWriter(strStabilizedFile);
            }
            catch (Exception e)
            {
                Int64 lnow = (DateTime.Now.ToBinary()) % 1000000;
                if (mAutoCalibrationCount % 2 == 0)
                    strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
                else
                    strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

                lwr = new StreamWriter(strStabilizedFile);
            }
            if (lwr != null)
            {
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
                for (int i = 0; i < effLength; i++)
                {
                    string slstr = i.ToString() + ",";
                    for (int j = 0; j < 23; j++)
                    {
                        //if (j < 19 || j == 22)
                        slstr += stablizedData[i][j].ToString("F5") + ",";
                        //else
                        //{
                        //    //slstr += (RAD_To_MIN * stablizedData[i][j]).ToString("F5") + ",";
                        //    slstr += (stablizedData[i][j]).ToString("F5") + ",";
                        //}
                    }

                    lwr.WriteLine(slstr);
                }
                lwr.Close();
            }

            //////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////

            //  axis 따라서 List 에 저장된 데이터를 처리한다.
            double[] p2ndCoef = new double[3];
            double[] p2ndCoef2 = new double[3];
            double[] p2ndCoef3 = new double[3];
            int fovYoffset = GetROIY(0);
            string lstr = "";
            switch (axis)
            {
                case "Z":
                    //  axis == "Z" : YLUT 의 경우 
                    //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
                    //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
                    //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

                    //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

                    double a1 = 0;
                    double b1 = 0;
                    double a2 = 0;
                    double b2 = 0;
                    double a3 = 0;
                    double b3 = 0;

                    mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
                    mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

                    //  stablizedData[i][16] : X
                    //  stablizedData[i][17] : Y
                    //  stablizedData[i][18] : Z
                    //  stablizedData[i][19] : TX   rad
                    //  stablizedData[i][20] : TY   rad
                    //  stablizedData[i][21] : TZ   rad

                    for (int i = 0; i < effLength; i++)
                    {
                        //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
                        szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
                    double[] LUT1_tmp = new double[effLength];
                    double[] LUT1 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

                    LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
                    LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

                    //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
                    //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
                    //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
                        szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
                    double[] LUT2_tmp = new double[effLength];
                    double[] LUT2 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

                    LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
                    LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

                    //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
                    //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
                    //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
                    //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
                        szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
                    double[] LUT3_tmp = new double[effLength];
                    double[] LUT3 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

                    LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
                    LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

                    //  Z scale
                    // 241206 YLUT 제거 후, Z scale 2차로 변경
                    for (int i = 0; i < effLength; i++)
                    {
                        sZZ[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
                        sZZ[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
                    }
                    //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szz, effLength, ref a, ref b);
                    //a = a * 0.9993; // 0.9992; // 헥사포드 Cal 변경
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZZ, effLength, ref ZtoZab);

                    //a = (a - 1) * 0.4 + 1; 
                    //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
                    //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
                    //  Scale 만 조정해가면서 수차례 반복 필요
                    //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


                    if (mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView)
                    {
                        string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
                        string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
                        wr = new StreamWriter(srcFile);
                        wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + ZtoZab[1].ToString());
                        wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
                        for (int i = 0; i < effLength; i++)
                        {
                            wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
                        }
                        wr.Close();
                        System.IO.File.Copy(srcFile, destFile, true);
                    }

                    /////////////////////////////////////////////////////////////////////////////////
                    //  Z to X 계산
                    //  Z vs X - Xprobe , Z vs Y - Yprobe                 
                    for (int i = 0; i < effLength; i++)
                    {
                        //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
                        //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
                        sZtoX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
                        sZtoY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage

                        sZtoTX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
                        sZtoTY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sZtoTZ[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }

                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref ZtoXab[0], ref ZtoXab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref ZtoYab[0], ref ZtoYab[1]);

                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTX, effLength, ref ZtoTXab[0], ref ZtoTXab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTY, effLength, ref ZtoTYab[0], ref ZtoTYab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTZ, effLength, ref ZtoTZab[0], ref ZtoTZab[1]);

                    if (!isAutoCalibrationEastView)
                    {
                        // ZtoX, ZtoY 수정
                        //double aZtoX = 0;
                        //double aZtoY = 0;
                        //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref aZtoX, ref b);
                        //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref aZtoY, ref b);
                        lstr = $"ZZ Scale\t{ZtoZab[0]:E5}\t{ZtoZab[1]:E5}\t{ZtoZab[2]:E5}\r\n";
                        lstr += $"ZtoX\t{ZtoXab[0]:E5}\r\n";
                        lstr += $"ZtoY\t{ZtoYab[0]:E5}\r\n";


                        if (mAutoCalibrationCount % 2 == 0)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            string[] strZscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[2] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZscaleLine.Length; i++)
                                allLines[2] += strZscaleLine[i];

                            string[] strZtoXLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[6] = ZtoXab[0].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoXLine.Length; i++)
                                allLines[6] += strZtoXLine[i];

                            string[] strZtoYLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[7] = ZtoYab[0].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoYLine.Length; i++)
                                allLines[7] += strZtoYLine[i];

                            wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                        }
                    }


                    //////////////////////////////////////////////////////////////////////////////////

                    break;
                case "X":
                    //  Axis = 1 : X scale 확인 및 저장
                    //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
                    //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
                    //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
                        sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값
                    }
                    //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref a, ref b);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref XtoXab);  //  A[0]X^2 + A[1]X + A[2]

                    lstr = "XX Scale,\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";

                    /////////////////////////////////////////////////////////////////////////////////
                    //  X to Y ／Ｚ　계산
                    //  X vs Y - Yprobe , X vs Z - Zprobe

                    for (int i = 0; i < effLength; i++)
                    {
                        //sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][18]);
                        //sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);
                        sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
                        sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage

                        sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
                        sXtoTY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sXtoTZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoY, effLength, ref XtoYab[0], ref XtoYab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref XtoZab);

                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTX, effLength, ref XtoTXab[0], ref XtoTXab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTY, effLength, ref XtoTYab[0], ref XtoTYab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTZ, effLength, ref XtoTZab[0], ref XtoTZab[1]);

                    lstr += "XtoY,\t" + XtoYab[0].ToString("E5") + "\r\n";
                    lstr += "XtoZ,\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
                    lstr += "XtoTX,\t" + XtoTXab[0].ToString("E5") + "\r\n";

                    /////////////////////////////////////////////////////////////////////////////////
                    //  X to Y ／Ｚ　계산
                    //  X vs Y - Yprobe , X vs Z - Zprobe
                    //////FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
                    //////for (int i = 0; i < effLength; i++)
                    //////{
                    //////    sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3]);
                    //////}

                    wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
                    //for (int i = 0; i < effLength; i++)
                    //    lstr += sXtoZ[i].X.ToString("F4") + "," + sXtoZ[i].Y.ToString("F4") + "\r\n";
                    wr.Write(lstr);
                    wr.Close();

                    //////////////////////////////////////////////////////////////////////////////////
                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] strXXscaleLine = allLines[0].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[0] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXXscaleLine.Length; i++)
                            allLines[0] += strXXscaleLine[i];

                        string[] strXtoYLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[10] = XtoYab[0].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoYLine.Length; i++)
                            allLines[10] += strXtoYLine[i];

                        string[] strXtoZLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[11] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoZLine.Length; i++)
                            allLines[11] += strXtoZLine[i];

                        string[] strXtoTXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[13] = XtoTXab[0].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoTXLine.Length; i++)
                            allLines[13] += strXtoTXLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;

                case "Y":
                    mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
                    mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
                    mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

                    //  Axis = 2 : Y scale 확인 및 저장
                    //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
                        sYY[i].X = stablizedData[i][1];
                    }
                    //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref a, ref b);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref YtoYab);

                    lstr = "YY Scale,\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
                    /////////////////////////////////////////////////////////////////////////////////
                    //  X to Y ／Ｚ　계산
                    //  X vs Y - Yprobe , X vs Z - Zprobe

                    for (int i = 0; i < effLength; i++)
                    {
                        //sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][17]);    //  X - probe X
                        //sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);   //  Z - probe Z
                        sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
                        sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage

                        sYtoTX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
                        sYtoTY[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sYtoTZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoX, effLength, ref YtoXab[0], ref YtoXab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref YtoZab);

                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTX, effLength, ref YtoTXab[0], ref YtoTXab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTY, effLength, ref YtoTYab[0], ref YtoTYab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTZ, effLength, ref YtoTZab[0], ref YtoTZab[1]);


                    lstr += "YtoX,\t" + YtoXab[0].ToString("E5") + "\r\n";
                    lstr += "YtoZ,\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";
                    lstr += "YtoTX,\t" + YtoTXab[0].ToString("E5") + "\r\n";

                    if (isAutoCalibrationEastView)
                    {
                        lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
                    }

                    wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    //////////////////////////////////////////////////////////////////////////////////                    
                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        sr.Close();
                        if (isAutoCalibrationEastView)
                        {
                            string[] strEastScaleLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[12] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
                            for (int i = 1; i < strEastScaleLine.Length; i++)
                                allLines[12] += strEastScaleLine[i];
                        }
                        else
                        {
                            string[] strYYscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[1] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYYscaleLine.Length; i++)
                                allLines[1] += strYYscaleLine[i];

                            string[] strYtoXLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[8] = YtoXab[0].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoXLine.Length; i++)
                                allLines[8] += strYtoXLine[i];

                            string[] strYtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[9] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoZLine.Length; i++)
                                allLines[9] += strYtoZLine[i];

                            string[] strYtoTXLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[14] = YtoTXab[0].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoTXLine.Length; i++)
                                allLines[14] += strYtoTXLine[i];
                        }

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;

                //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
                case "TX":
                    //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
                    //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

                    //  stablizedData[i][19] : TX   rad
                    //  stablizedData[i][20] : TY   rad
                    //  stablizedData[i][21] : TZ   rad

                    for (int i = 0; i < effLength; i++)
                    {
                        sTXTX[i].Y = stablizedData[i][19]; // RAD_To_MIN;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTXTX[i].X = stablizedData[i][3];

                        sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref TXtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTXtoTY, effLength, ref TXtoTYab[0], ref TXtoTYab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTXtoTZ, effLength, ref TXtoTZab[0], ref TXtoTZab[1]);


                    lstr += "TX Scale,\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
                    wr.WriteLine("TX Scale,\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n");
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTXscaleLine.Length; i++)
                            allLines[4] += strTXscaleLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                case "TY":
                    //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
                    //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sTYTY[i].Y = stablizedData[i][20];// * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTYTY[i].X = stablizedData[i][4];

                        sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][3] - stablizedData[i][19]);
                        sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][5] - stablizedData[i][21]);
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYTY, effLength, ref TYtoTYab[0], ref TYtoTYab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYtoTX, effLength, ref TYtoTXab[0], ref TYtoTXab[1]);
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYtoTZ, effLength, ref TYtoTZab[0], ref TYtoTZab[1]);

                    lstr += "TY Scale,\t" + TYtoTYab[0].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
                    wr.WriteLine("TY Scale," + TYtoTYab[0].ToString());
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[5] = TYtoTYab[0].ToString("E5") + "\t//";
                        for (int i = 1; i < strTYscaleLine.Length; i++)
                            allLines[5] += strTYscaleLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                default:
                    break;
            }
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbCalibration.Text += lstr;
                });
            else
                tbCalibration.Text += lstr;

        }
        public void CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        {
            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return;
            }

            string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
            if (!Directory.Exists(AdminPathName))
                Directory.CreateDirectory(AdminPathName);

            int fullLength = measure.Length;
            StreamWriter wr = null;
            //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

            //  안정화 유효데이터를 추출한다.
            //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

            List<double[]> stablizedData = new List<double[]>();

            int effLength = 0;
            int[] effIndex = null;
            FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTZTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXtoTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYtoTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

            FZMath.Point2D[] sTXtoTY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTXtoTZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYtoTZ = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTZtoTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTZtoTY = new FZMath.Point2D[effLength];

            double[] XtoXab = new double[3];
            double[] YtoYab = new double[3];
            double[] ZtoZab = new double[3];
            double[] TXtoTXab = new double[3];
            double[] TYtoTYab = new double[3];
            double[] TZtoTZab = new double[3];

            double[] XtoYab = new double[3];
            double[] XtoZab = new double[3];
            double[] XtoTXab = new double[3];
            double[] XtoTYab = new double[3];
            double[] XtoTZab = new double[3];

            double[] YtoXab = new double[3];
            double[] YtoZab = new double[3];
            double[] YtoTXab = new double[3];
            double[] YtoTYab = new double[3];
            double[] YtoTZab = new double[3];

            double[] ZtoXab = new double[3];
            double[] ZtoYab = new double[3];
            double[] ZtoTXab = new double[3];
            double[] ZtoTYab = new double[3];
            double[] ZtoTZab = new double[3];

            double[] TXtoTYab = new double[3];
            double[] TXtoTZab = new double[3];

            double[] TYtoTXab = new double[3];
            double[] TYtoTZab = new double[3];

            double[] TZtoTXab = new double[3];
            double[] TZtoTYab = new double[3];

            if (!IsRemote)
            {
                switch (axis)
                {
                    case "Z":
                        effIndex = ExtractStablizedIndex(measure, 2);
                        break;

                    case "X":
                        effIndex = ExtractStablizedIndex(measure, 0);
                        break;
                    case "Y":
                        effIndex = ExtractStablizedIndex(measure, 1);
                        break;
                    case "TX":
                        effIndex = ExtractStablizedIndex(measure, 3);
                        break;
                    case "TY":
                        effIndex = ExtractStablizedIndex(measure, 4);
                        break;
                    case "TZ":
                        effIndex = ExtractStablizedIndex(measure, 5);
                        break;
                    default:
                        break;
                }
                effLength = effIndex.Length;

                for (int i = 0; i < effLength; i++)
                {
                    szy1[i] = new FZMath.Point2D();
                    szy2[i] = new FZMath.Point2D();
                    szy3[i] = new FZMath.Point2D();

                    sXX[i] = new FZMath.Point2D();
                    sYY[i] = new FZMath.Point2D();
                    sZZ[i] = new FZMath.Point2D();
                    sTXTX[i] = new FZMath.Point2D();
                    sTYTY[i] = new FZMath.Point2D();
                    sTZTZ[i] = new FZMath.Point2D();

                    sXtoTX[i] = new FZMath.Point2D();
                    sXtoTY[i] = new FZMath.Point2D();
                    sXtoTZ[i] = new FZMath.Point2D();
                    sYtoTX[i] = new FZMath.Point2D();
                    sYtoTY[i] = new FZMath.Point2D();
                    sYtoTZ[i] = new FZMath.Point2D();
                    sZtoTX[i] = new FZMath.Point2D();
                    sZtoTY[i] = new FZMath.Point2D();
                    sZtoTZ[i] = new FZMath.Point2D();

                    sTXtoTY[i] = new FZMath.Point2D();
                    sTXtoTZ[i] = new FZMath.Point2D();
                    sTYtoTX[i] = new FZMath.Point2D();
                    sTYtoTZ[i] = new FZMath.Point2D();
                    sTZtoTX[i] = new FZMath.Point2D();
                    sTZtoTY[i] = new FZMath.Point2D();
                }

                if (effLength == 0)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbInfo.Text += "Stabilized data not found\r\n";
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        });
                    }
                    else
                    {
                        tbInfo.Text += "Stabilized data not found\r\n";
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                    return;

                }
                for (int i = 0; i < effLength; i++)
                {
                    double[] lstbData = new double[22];
                    for (int j = 0; j < 22; j++)
                        lstbData[j] = measure[effIndex[i]][j];

                    stablizedData.Add(lstbData);
                }
            }
            else
            {
                //  Remote or Auto Calibration
                effLength = measure.Length;

                szy1 = new FZMath.Point2D[effLength];
                szy2 = new FZMath.Point2D[effLength];
                szy3 = new FZMath.Point2D[effLength];

                sXX = new FZMath.Point2D[effLength];
                sYY = new FZMath.Point2D[effLength];
                sZZ = new FZMath.Point2D[effLength];
                sTXTX = new FZMath.Point2D[effLength];
                sTYTY = new FZMath.Point2D[effLength];
                sTZTZ = new FZMath.Point2D[effLength];

                sXtoY = new FZMath.Point2D[effLength];
                sXtoZ = new FZMath.Point2D[effLength];
                sXtoTX = new FZMath.Point2D[effLength];
                sXtoTY = new FZMath.Point2D[effLength];
                sXtoTZ = new FZMath.Point2D[effLength];

                sYtoX = new FZMath.Point2D[effLength];
                sYtoZ = new FZMath.Point2D[effLength];
                sYtoTX = new FZMath.Point2D[effLength];
                sYtoTY = new FZMath.Point2D[effLength];
                sYtoTZ = new FZMath.Point2D[effLength];

                sZtoX = new FZMath.Point2D[effLength];
                sZtoY = new FZMath.Point2D[effLength];
                sZtoTX = new FZMath.Point2D[effLength];
                sZtoTY = new FZMath.Point2D[effLength];
                sZtoTZ = new FZMath.Point2D[effLength];

                sTXtoTY = new FZMath.Point2D[effLength];
                sTXtoTZ = new FZMath.Point2D[effLength];

                sTYtoTX = new FZMath.Point2D[effLength];
                sTYtoTZ = new FZMath.Point2D[effLength];

                sTZtoTX = new FZMath.Point2D[effLength];
                sTZtoTY = new FZMath.Point2D[effLength];

                for (int i = 0; i < effLength; i++)
                {
                    szy1[i] = new FZMath.Point2D();
                    szy2[i] = new FZMath.Point2D();
                    szy3[i] = new FZMath.Point2D();

                    sXX[i] = new FZMath.Point2D();
                    sYY[i] = new FZMath.Point2D();
                    sZZ[i] = new FZMath.Point2D();
                    sTXTX[i] = new FZMath.Point2D();
                    sTYTY[i] = new FZMath.Point2D();
                    sTZTZ[i] = new FZMath.Point2D();

                    sXtoY[i] = new FZMath.Point2D();
                    sXtoZ[i] = new FZMath.Point2D();
                    sXtoTX[i] = new FZMath.Point2D();
                    sXtoTY[i] = new FZMath.Point2D();
                    sXtoTZ[i] = new FZMath.Point2D();

                    sYtoX[i] = new FZMath.Point2D();
                    sYtoZ[i] = new FZMath.Point2D();
                    sYtoTX[i] = new FZMath.Point2D();
                    sYtoTY[i] = new FZMath.Point2D();
                    sYtoTZ[i] = new FZMath.Point2D();

                    sZtoX[i] = new FZMath.Point2D();
                    sZtoY[i] = new FZMath.Point2D();
                    sZtoTX[i] = new FZMath.Point2D();
                    sZtoTY[i] = new FZMath.Point2D();
                    sZtoTZ[i] = new FZMath.Point2D();

                    sTXtoTY[i] = new FZMath.Point2D();
                    sTXtoTZ[i] = new FZMath.Point2D();

                    sTYtoTX[i] = new FZMath.Point2D();
                    sTYtoTZ[i] = new FZMath.Point2D();

                    sTZtoTX[i] = new FZMath.Point2D();
                    sTZtoTY[i] = new FZMath.Point2D();
                }

                for (int i = 0; i < effLength; i++)
                    stablizedData.Add(measure[i]);
            }

            //////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////
            //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
            StreamWriter lwr = null;
            double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

            if (!IsRemote)
            {
                lwr = new StreamWriter(AdminPathName + "FullData.csv");
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
                int k = 0;
                for (int i = 0; i < fullLength; i++)
                {
                    string slstr = i.ToString() + ",";
                    for (int j = 0; j < 23; j++)
                        slstr += measure[i][j].ToString("F5") + ",";
                    if (i == effIndex[k])
                    {
                        slstr += "*";
                        k++;
                    }
                    lwr.WriteLine(slstr);
                    if (k == effLength)
                        break;
                }
                lwr.Close();
            }

            string calName = axis;
            if (isAutoCalibrationEastView)
            {
                calName += "_EastView";
            }

            string strStabilizedFile = "";
            if (mAutoCalibrationCount % 2 == 0)
                strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
            else
                strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

            try
            {
                lwr = new StreamWriter(strStabilizedFile);
            }
            catch (Exception e)
            {
                Int64 lnow = (DateTime.Now.ToBinary()) % 1000000;
                if (mAutoCalibrationCount % 2 == 0)
                    strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
                else
                    strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

                lwr = new StreamWriter(strStabilizedFile);
            }
            if (lwr != null)
            {
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
                for (int i = 0; i < effLength; i++)
                {
                    string slstr = i.ToString() + ",";
                    for (int j = 0; j < 23; j++)
                    {
                        //if (j < 19 || j == 22)
                        slstr += stablizedData[i][j].ToString("F5") + ",";
                        //else
                        //{
                        //    slstr += (RAD_To_MIN * stablizedData[i][j]).ToString("F5") + ",";
                        //}
                    }

                    lwr.WriteLine(slstr);
                }
                lwr.Close();
            }

            //////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////

            //  axis 따라서 List 에 저장된 데이터를 처리한다.
            //double[] p2ndCoef = new double[3];
            //double[] p2ndCoef2 = new double[3];
            //double[] p2ndCoef3 = new double[3];
            int fovYoffset = GetROIY(0);
            string lstr = "";
            switch (axis)
            {
                case "Z":
                    //  axis == "Z" : YLUT 의 경우 
                    //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
                    //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
                    //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

                    //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

                    double a1 = 0;
                    double b1 = 0;
                    double a2 = 0;
                    double b2 = 0;
                    double a3 = 0;
                    double b3 = 0;

                    mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
                    mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

                    //  stablizedData[i][16] : X
                    //  stablizedData[i][17] : Y
                    //  stablizedData[i][18] : Z
                    //  stablizedData[i][19] : TX   rad
                    //  stablizedData[i][20] : TY   rad
                    //  stablizedData[i][21] : TZ   rad

                    for (int i = 0; i < effLength; i++)
                    {
                        //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
                        szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
                    double[] LUT1_tmp = new double[effLength];
                    double[] LUT1 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

                    LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
                    LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

                    //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
                    //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
                    //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
                        szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
                    double[] LUT2_tmp = new double[effLength];
                    double[] LUT2 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

                    LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
                    LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

                    //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
                    //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
                    //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
                    //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
                        szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
                    double[] LUT3_tmp = new double[effLength];
                    double[] LUT3 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

                    LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
                    LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

                    //  Z scale
                    for (int i = 0; i < effLength; i++)
                    {
                        sZZ[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
                        sZZ[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZZ, effLength, ref ZtoZab);    // 2차로 변경
                    //ZtoZab[0] = ZtoZab[0] * 0.9993; // 0.9992; // 헥사포드 Cal 변경
                    //a = (a - 1) * 0.4 + 1; 
                    //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
                    //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
                    //  Scale 만 조정해가면서 수차례 반복 필요
                    //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


                    if (mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView)
                    {
                        string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
                        string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
                        wr = new StreamWriter(srcFile);
                        wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + ZtoZab[1].ToString());
                        wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
                        for (int i = 0; i < effLength; i++)
                        {
                            wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
                        }
                        wr.Close();
                        System.IO.File.Copy(srcFile, destFile, true);
                    }

                    /////////////////////////////////////////////////////////////////////////////////
                    //  Z to X 계산
                    //  Z vs X - Xprobe , Z vs Y - Yprobe
                    for (int i = 0; i < effLength; i++)
                    {
                        //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
                        //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
                        sZtoX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
                        sZtoY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage
                        sZtoTX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
                        sZtoTY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sZtoTZ[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }

                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoX, effLength, ref ZtoXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoY, effLength, ref ZtoYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTX, effLength, ref ZtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTY, effLength, ref ZtoTYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTZ, effLength, ref ZtoTZab);

                    if (!isAutoCalibrationEastView)
                    {
                        lstr = "ZZ Scale\t" + ZtoZab[0].ToString("E5") + ",\r\n" + ZtoZab[1].ToString("E5") + ",\t" + ZtoZab[2].ToString("E5") + "\r\n";
                        lstr += "ZtoX\t" + ZtoXab[0].ToString("E5") + ",\t" + ZtoXab[1].ToString("E5") + ",\t" + ZtoXab[2].ToString("E5") + "\r\n";
                        lstr += "ZtoY\t" + ZtoYab[0].ToString("E5") + ",\t" + ZtoYab[1].ToString("E5") + ",\t" + ZtoYab[2].ToString("E5") + "\r\n";
                        lstr += "ZtoTX\t" + ZtoTXab[0].ToString("E5") + ",\t" + ZtoTXab[1].ToString("E5") + ",\t" + ZtoTXab[2].ToString("E5") + "\r\n";
                        lstr += "ZtoTY\t" + ZtoTYab[0].ToString("E5") + ",\t" + ZtoTYab[1].ToString("E5") + ",\t" + ZtoTYab[2].ToString("E5") + "\r\n";
                        lstr += "ZtoTZ\t" + ZtoTZab[0].ToString("E5") + ",\t" + ZtoTZab[1].ToString("E5") + ",\t" + ZtoTZab[2].ToString("E5") + "\r\n";

                        if (mAutoCalibrationCount % 2 == 0)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // Z
                            string[] strZscaleLine = allLines[3].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[3] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZscaleLine.Length; i++)
                                allLines[3] += strZscaleLine[i];
                            // Z TO X
                            string[] strZtoXLine = allLines[18].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[18] = ZtoXab[0].ToString("E5") + "\t" + ZtoXab[1].ToString("E5") + "\t" + ZtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoXLine.Length; i++)
                                allLines[18] += strZtoXLine[i];
                            // Z TO Y
                            string[] strZtoYLine = allLines[19].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[19] = ZtoYab[0].ToString("E5") + "\t" + ZtoYab[1].ToString("E5") + "\t" + ZtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoYLine.Length; i++)
                                allLines[19] += strZtoYLine[i];
                            // Z TO TX
                            string[] strZtoTXLine = allLines[20].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[20] = ZtoTXab[0].ToString("E5") + "\t" + ZtoTXab[1].ToString("E5") + "\t" + ZtoTXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoTXLine.Length; i++)
                                allLines[20] += strZtoTXLine[i];
                            // Z TO TY
                            string[] strZtoTYLine = allLines[21].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[21] = ZtoTYab[0].ToString("E5") + "\t" + ZtoTYab[1].ToString("E5") + "\t" + ZtoTYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoTYLine.Length; i++)
                                allLines[21] += strZtoTYLine[i];
                            // Z TO TZ
                            string[] strZtoTZLine = allLines[22].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[22] = ZtoTZab[0].ToString("E5") + "\t" + ZtoTZab[1].ToString("E5") + "\t" + ZtoTZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoTZLine.Length; i++)
                                allLines[22] += strZtoTZLine[i];

                            wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                        }
                    }


                    //////////////////////////////////////////////////////////////////////////////////

                    break;
                case "X":
                    //  Axis = 1 : X scale 확인 및 저장
                    //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
                    //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
                    //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
                        sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값

                        sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
                        sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage

                        sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
                        sXtoTY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sXtoTZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }

                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref XtoXab);  //  A[0]X^2 + A[1]X + A[2]
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoY, effLength, ref XtoYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref XtoZab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTX, effLength, ref XtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTY, effLength, ref XtoTYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTZ, effLength, ref XtoTZab);

                    lstr = "XX Scale\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";
                    lstr += "XtoY\t" + XtoYab[0].ToString("E5") + ",\t" + XtoYab[1].ToString("E5") + ",\t" + XtoYab[2].ToString("E5") + "\r\n";
                    lstr += "XtoZ\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
                    lstr += "XtoTX\t" + XtoTXab[0].ToString("E5") + ",\t" + XtoTXab[1].ToString("E5") + ",\t" + XtoTXab[2].ToString("E5") + "\r\n";
                    lstr += "XtoTY\t" + XtoTYab[0].ToString("E5") + ",\t" + XtoTYab[1].ToString("E5") + ",\t" + XtoTYab[2].ToString("E5") + "\r\n";
                    lstr += "XtoTZ\t" + XtoTZab[0].ToString("E5") + ",\t" + XtoTZab[1].ToString("E5") + ",\t" + XtoTZab[2].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    //////////////////////////////////////////////////////////////////////////////////
                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        // X
                        string[] strXXscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[1] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXXscaleLine.Length; i++)
                            allLines[1] += strXXscaleLine[i];
                        // X TO Y
                        string[] strXtoYLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[8] = XtoYab[0].ToString("E5") + "\t" + XtoYab[1].ToString("E5") + "\t" + XtoYab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoYLine.Length; i++)
                            allLines[8] += strXtoYLine[i];
                        // X TO Z
                        string[] strXtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[9] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoZLine.Length; i++)
                            allLines[9] += strXtoZLine[i];
                        // X TO TX
                        string[] strXtoTXLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[10] = XtoTXab[0].ToString("E5") + "\t" + XtoTXab[1].ToString("E5") + "\t" + XtoTXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoTXLine.Length; i++)
                            allLines[10] += strXtoTXLine[i];
                        // X TO TY
                        string[] strXtoTYLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[11] = XtoTYab[0].ToString("E5") + "\t" + XtoTYab[1].ToString("E5") + "\t" + XtoTYab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoTYLine.Length; i++)
                            allLines[11] += strXtoTYLine[i];
                        // X TO TZ
                        string[] strXtoTZLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[12] = XtoTZab[0].ToString("E5") + "\t" + XtoTZab[1].ToString("E5") + "\t" + XtoTZab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoTZLine.Length; i++)
                            allLines[12] += strXtoTZLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;

                case "Y":
                    mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
                    mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
                    mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

                    //  Axis = 2 : Y scale 확인 및 저장
                    //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
                        sYY[i].X = stablizedData[i][1];

                        sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
                        sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage

                        sYtoTX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
                        sYtoTY[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sYtoTZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref YtoYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoX, effLength, ref YtoXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref YtoZab);

                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTX, effLength, ref YtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTY, effLength, ref YtoTYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTZ, effLength, ref YtoTZab);

                    lstr = "YY Scale\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
                    lstr += "YtoX\t" + YtoXab[0].ToString("E5") + ",\t" + YtoXab[1].ToString("E5") + ",\t" + YtoXab[2].ToString("E5") + "\r\n";
                    lstr += "YtoZ\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";

                    lstr += "YtoTX\t" + YtoTXab[0].ToString("E5") + ",\t" + YtoTXab[1].ToString("E5") + ",\t" + YtoTXab[2].ToString("E5") + "\r\n";
                    lstr += "YtoTY\t" + YtoTYab[0].ToString("E5") + ",\t" + YtoTYab[1].ToString("E5") + ",\t" + YtoTYab[2].ToString("E5") + "\r\n";
                    lstr += "YtoTZ\t" + YtoTZab[0].ToString("E5") + ",\t" + YtoTZab[1].ToString("E5") + ",\t" + YtoTZab[2].ToString("E5") + "\r\n";

                    if (isAutoCalibrationEastView)
                    {
                        lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
                    }

                    wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    //////////////////////////////////////////////////////////////////////////////////                    
                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        sr.Close();
                        if (isAutoCalibrationEastView)
                        {
                            // East View Sclae
                            string[] strEastScaleLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[7] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
                            for (int i = 1; i < strEastScaleLine.Length; i++)
                                allLines[7] += strEastScaleLine[i];
                        }
                        else
                        {
                            // Y
                            string[] strYYscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[2] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYYscaleLine.Length; i++)
                                allLines[2] += strYYscaleLine[i];
                            // Y TO X
                            string[] strYtoXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[13] = YtoXab[0].ToString("E5") + "\t" + YtoXab[1].ToString("E5") + "\t" + YtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoXLine.Length; i++)
                                allLines[13] += strYtoXLine[i];
                            // Y TO Z
                            string[] strYtoZLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[14] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoZLine.Length; i++)
                                allLines[14] += strYtoZLine[i];
                            // Y TO TX
                            string[] strYtoTXLine = allLines[15].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[15] = YtoTXab[0].ToString("E5") + "\t" + YtoTXab[1].ToString("E5") + "\t" + YtoTXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoTXLine.Length; i++)
                                allLines[15] += strYtoTXLine[i];
                            // Y TO TY
                            string[] strYtoTYLine = allLines[16].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[16] = YtoTYab[0].ToString("E5") + "\t" + YtoTYab[1].ToString("E5") + "\t" + YtoTYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoTYLine.Length; i++)
                                allLines[16] += strYtoTYLine[i];
                            // Y TO TZ
                            string[] strYtoTZLine = allLines[17].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[17] = YtoTZab[0].ToString("E5") + "\t" + YtoTZab[1].ToString("E5") + "\t" + YtoTZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoTZLine.Length; i++)
                                allLines[17] += strYtoTZLine[i];
                        }

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;

                //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
                case "TX":
                    //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
                    //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

                    //  stablizedData[i][19] : TX   rad
                    //  stablizedData[i][20] : TY   rad
                    //  stablizedData[i][21] : TZ   rad

                    for (int i = 0; i < effLength; i++)
                    {
                        sTXTX[i].Y = stablizedData[i][19]; // * RAD_To_MIN;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTXTX[i].X = stablizedData[i][3];

                        sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
                        sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref TXtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXtoTY, effLength, ref TXtoTYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXtoTZ, effLength, ref TXtoTZab);

                    lstr += "TX Scale\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";
                    lstr += "TXtoTY\t" + TXtoTYab[0].ToString("E5") + ",\t" + TXtoTYab[1].ToString("E5") + ",\t" + TXtoTYab[2].ToString("E5") + "\r\n";
                    lstr += "TXtoTZ\t" + TXtoTZab[0].ToString("E5") + ",\t" + TXtoTZab[1].ToString("E5") + ",\t" + TXtoTZab[2].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        // TX
                        string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTXscaleLine.Length; i++)
                            allLines[4] += strTXscaleLine[i];
                        // TX TO TY
                        string[] strTXtoTYLine = allLines[23].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[23] = TXtoTYab[0].ToString("E5") + "\t" + TXtoTYab[1].ToString("E5") + "\t" + TXtoTYab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTXtoTYLine.Length; i++)
                            allLines[23] += strTXtoTYLine[i];
                        // TX TO TZ
                        string[] strTXtoTZLine = allLines[24].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[24] = TXtoTZab[0].ToString("E5") + "\t" + TXtoTZab[1].ToString("E5") + "\t" + TXtoTZab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTXtoTZLine.Length; i++)
                            allLines[24] += strTXtoTZLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                case "TY":
                    //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
                    //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sTYTY[i].Y = stablizedData[i][20]; // * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTYTY[i].X = stablizedData[i][4];

                        sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][3] - stablizedData[i][19]);
                        sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][5] - stablizedData[i][21]);
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYTY, effLength, ref TYtoTYab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYtoTX, effLength, ref TYtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYtoTZ, effLength, ref TYtoTZab);

                    lstr += "TY Scale\t" + TYtoTYab[0].ToString("E5") + ",\t" + TYtoTYab[1].ToString("E5") + ",\t" + TYtoTYab[2].ToString("E5") + "\r\n";
                    lstr += "TYtoTX\t" + TYtoTXab[0].ToString("E5") + ",\t" + TYtoTXab[1].ToString("E5") + ",\t" + TYtoTXab[2].ToString("E5") + "\r\n";
                    lstr += "TYtoTZ\t" + TYtoTZab[0].ToString("E5") + ",\t" + TYtoTZab[1].ToString("E5") + ",\t" + TYtoTZab[2].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        // TY
                        string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[5] = TYtoTYab[0].ToString("E5") + "\t" + TYtoTYab[1].ToString("E5") + "\t" + TYtoTYab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTYscaleLine.Length; i++)
                            allLines[5] += strTYscaleLine[i];
                        // TY TO TX
                        string[] strTYtoTXLine = allLines[25].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[25] = TYtoTXab[0].ToString("E5") + "\t" + TYtoTXab[1].ToString("E5") + "\t" + TYtoTXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTYtoTXLine.Length; i++)
                            allLines[25] += strTYtoTXLine[i];
                        // TY TO TZ
                        string[] strTYtoTZLine = allLines[26].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[26] = TYtoTZab[0].ToString("E5") + "\t" + TYtoTZab[1].ToString("E5") + "\t" + TYtoTZab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTYtoTZLine.Length; i++)
                            allLines[26] += strTYtoTZLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                case "TZ":
                    //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
                    //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sTZTZ[i].Y = stablizedData[i][21]; // * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTZTZ[i].X = stablizedData[i][5];

                        sTZtoTX[i] = new FZMath.Point2D(sTZTZ[i].X, stablizedData[i][3] - stablizedData[i][19]);
                        sTZtoTY[i] = new FZMath.Point2D(sTZTZ[i].X, stablizedData[i][4] - stablizedData[i][20]);
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZTZ, effLength, ref TZtoTZab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZtoTX, effLength, ref TZtoTXab);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZtoTY, effLength, ref TZtoTYab);

                    lstr += "TZ Scale\t" + TZtoTZab[0].ToString("E5") + ",\t" + TZtoTZab[1].ToString("E5") + ",\t" + TZtoTZab[2].ToString("E5") + "\r\n";
                    lstr += "TZtoTX\t" + TZtoTXab[0].ToString("E5") + ",\t" + TZtoTXab[1].ToString("E5") + ",\t" + TZtoTXab[2].ToString("E5") + "\r\n";
                    lstr += "TZtoTY\t" + TZtoTYab[0].ToString("E5") + ",\t" + TZtoTYab[1].ToString("E5") + ",\t" + TZtoTYab[2].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        // TZ
                        string[] strTZscaleLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[6] = TZtoTZab[0].ToString("E5") + "\t" + TZtoTZab[1].ToString("E5") + "\t" + TZtoTZab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTZscaleLine.Length; i++)
                            allLines[6] += strTZscaleLine[i];
                        // TZ TO TX
                        string[] strTZtoTXLine = allLines[27].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[27] = TZtoTXab[0].ToString("E5") + "\t" + TZtoTXab[1].ToString("E5") + "\t" + TZtoTXab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTZtoTXLine.Length; i++)
                            allLines[27] += strTZtoTXLine[i];
                        // TZ TO TY
                        string[] strTZtoTYLine = allLines[28].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[28] = TZtoTYab[0].ToString("E5") + "\t" + TZtoTYab[1].ToString("E5") + "\t" + TZtoTYab[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTZtoTYLine.Length; i++)
                            allLines[28] += strTZtoTYLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                default:
                    break;
            }
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbCalibration.Text += lstr;
                });
            else
                tbCalibration.Text += lstr;

        }

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
                            pictureBox2.Image = m__G.oCam[0].LoadCropImg(0);
                        });
                    else
                        pictureBox2.Image = m__G.oCam[0].LoadCropImg(0);

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
            m__G.fGraph.mDriverIC.SocketTest(0, false);
        }

        private void BaseDownBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, false);
            m__G.fGraph.mDriverIC.SocketTest(0, false);
            Thread.Sleep(300);

            m__G.fGraph.mDriverIC.SocketTest(1, false);
        }

        private void SidePushUnloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, false);
        }

        private void PogoPinloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(0, true);
        }

        private void SidePushloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, true);
        }

        private void BaseUpBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, false);
            m__G.fGraph.mDriverIC.SocketTest(0, false);
            Thread.Sleep(300);

            m__G.fGraph.mDriverIC.SocketTest(1, true);
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

            if (m__G.fMotion == null) return;
            m__G.fMotion.Show();
            m__G.fMotion.BringToFront();
        }

        public int mAutoCalibrationCount = 0;
        public bool mAutoCalibrationRun = false;
        public bool isAutoCalibrationEastView = false;

        //private void btnAutoCal_Click(object sender, EventArgs e)
        //{
        //    if (!mAutoCalibrationRun)
        //    {
        //        mAutoCalibrationRun = true;
        //        btnAutoCal.Text = "Stop Auto Calibration";
        //    }
        //    else
        //    {
        //        mAutoCalibrationRun = false;
        //        btnAutoCal.Text = "Auto Cal Before-After";
        //        return;
        //    }

        //    mAutoCalibrationCount = 0;
        //    tbCalibration.Text = "";

        //    isAutoCalibrationEastView = false;
        //    if (rbCalEastView.Checked)
        //    {
        //        isAutoCalibrationEastView = true;

        //        // East View에서 Z Oneway Stroke
        //        double zOnewayStroke = 1750;
        //        if (tbZMaxStroke.Text.Length > 1)
        //            zOnewayStroke = double.Parse(tbZMaxStroke.Text);

        //        // East View에서 Y Oneway Stroke
        //        double yOnewayStroke = 1900;
        //        if (tbMaxStroke.Text.Length > 1)
        //            yOnewayStroke = double.Parse(tbMaxStroke.Text);

        //        Task.Run(() =>
        //        {
        //            AutoCalibrationEastView(yOnewayStroke, zOnewayStroke);
        //            mAutoCalibrationRun = false;
        //            this.Invoke(new Action(() =>
        //            {
        //                btnAutoCal.Text = "Auto Cal Before-After";
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

        //        double onewayStroke = 1900;
        //        if (tbMaxStroke.Text.Length > 1)
        //            onewayStroke = double.Parse(tbMaxStroke.Text);

        //        Task.Run(() =>
        //        {
        //            AutoCalibrationWrapperOld(axis, onewayStroke);
        //            mAutoCalibrationRun = false;
        //            this.Invoke(new Action(() =>
        //            {
        //                btnAutoCal.Text = "Auto Cal Before-After";
        //            }));
        //        });

        //    }
        //}

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
        private void AutoCalibration()
        {
            TextAppendTbInfo("Start auto calibration");

            // ScaleNTheta 초기화 (EastViewScale은 유지)
            double eastViewYPscale = m__G.oCam[0].mFAL.mFZM.mEastviewYPscale;
            InitializeScaleNTheta();
            ms_EastViewYPscale = eastViewYPscale;
            m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                             ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                             ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                             ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                             ms_TXtoTYbyView, ms_TXtoTZbyView,
                                             ms_TYtoTXbyView, ms_TYtoTZbyView,
                                             ms_TZtoTXbyView, ms_TZtoTYbyView,
                                             ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                             ms_TZtoZbyView);
            TextAppendTbInfo("Reset all scales except for EastViewYP scale");
            SaveScaleNTheta();


            // OQC 
            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find CSHorg.");
            FindCSHorg();   // 엉뚱한 위치에서 FindPorg시작하는거 방지용

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Porg.");
            FindPorg();

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find HCS Rotation Psi.");
            FindHCSrotationPsi();

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find X pivot.");
            FindPivot(1);
            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Y pivot.");
            FindPivot(2);
            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Z pivot.");
            FindPivot(3);
            SavePivots();


            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find CSHorg, Reset Probe.");
            FindCSHorg(true);   // Probe 리셋

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Fidorg");
            FindFidorg();

            SaveOQCCondition();

            // 측정 시작
            TextAppendTbInfo("Start baseline measurement");
            AxisCalibration(Axis.Z, 1750, false, true, false); // 1750
            LoadScaleNTheta();
            AxisCalibration(Axis.Y, 1900, false, true, false);  // 1900
            LoadScaleNTheta();
            AxisCalibration(Axis.X, 1900, false, true, false);  // 1900
            LoadScaleNTheta();
            AxisCalibration(Axis.TY, 200, false, true, false);
            LoadScaleNTheta();
            AxisCalibration(Axis.TX, 160, false, true, false);  // 160
            LoadScaleNTheta();
            AxisCalibration(Axis.TZ, 200, false, true, false);
            TextAppendTbInfo("Finish  baseline measurement");

            TextAppendTbInfo("Start verification measurement");
            LoadScaleNTheta();

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find X pivot.");
            FindPivot(1);
            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Y pivot.");
            FindPivot(2);
            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Z pivot.");
            FindPivot(3);
            SavePivots();

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find CSHorg");
            FindCSHorg();

            if (!mAutoCalibrationRun) return;
            AddVsnLog("Start to find Fidorg");
            FindFidorg();

            SaveOQCCondition();

            AxisCalibration(Axis.Z, 1750, true, false, false);
            AxisCalibration(Axis.Y, 1900, true, false, false);
            AxisCalibration(Axis.X, 1900, true, false, false);
            AxisCalibration(Axis.TY, 200, true, false, false);
            AxisCalibration(Axis.TX, 160, true, false, false);
            AxisCalibration(Axis.TZ, 200, true, false, false);

            TextAppendTbInfo("Finsh verification measurement");
            TextAppendTbInfo("Finsh auto calibration");
        }
        private void ReAutoCalibration()
        {
            if (LoadScaleNTheta())
            {
                TextAppendTbInfo($"Loaded scales");
            }
            else
            {
                TextAppendTbInfo($"Fail to load ScaleNTheta{m__G.mCamID0}");
                return;
            }

            if (!LoadOQCcondition())
            {
                MessageBox.Show("OQC condition must be ready!");
                return;
            }
            // Cal 할때는 FidOrg 영향이 없음  -> x, y, z 움직일때는 rotation없어서 영향x, tx,ty,tz 움직일때는 z관련 scale 없어서 영향x 
            // -> tx,ty,tz 움직일때 z 데이터 확인 하기 위해 LoadFidorg

            AxisCalibration(Axis.Z, 1750, true, true, true);
            AxisCalibration(Axis.Y, 1900, true, true, true);
            AxisCalibration(Axis.X, 1900, true, true, true);
            AxisCalibration(Axis.TY, 200, true, true, true);
            AxisCalibration(Axis.TX, 160, true, true, true);
            AxisCalibration(Axis.TZ, 200, true, true, true);

            TextAppendTbInfo("End Cal");
        }
        private void EastViewCalibration(bool isRemote)
        {

            // East View에서 Z Oneway Stroke
            double zOnewayStroke = 1750;
            double yOnewayStroke = 1900;

            List<double[]> collectedData = new List<double[]>();
            List<double[]> measuredData = null;

            // Z Translation
            if (!mAutoCalibrationRun) return;
            collectedData.Add(null);
            measuredData = ScanAxis(Axis.Z, zOnewayStroke);
            collectedData.AddRange(measuredData);

            // Y Translation
            if (!mAutoCalibrationRun) return;
            collectedData.Add(null);
            measuredData = ScanAxis(Axis.Y, yOnewayStroke);
            collectedData.AddRange(measuredData);

            RemoteEastViewYPCalibration(collectedData, isRemote);
        }
        public void AxisCalibration(Axis axis, double onewayStrokeUm, bool isSingle, bool isRemote, bool isReCal)
        {
            // isRecal = true, isRemote = true : 원점에서 axis축 이동 1회 측정, 기존 scaleNtheta에 1차만 업데이트(scaleNthe 로드 필요)
            // isRecal = true, isRemote = false : 원점에서 axis축 이동 1회 측정
            // isRecal = false, isRemote = true : 다른 위치에서 axis축 이동 5회 측정, scaleNtheta에 업데이트(scaleNthe 초기화 필요)
            // isRecal = false, isRemote = false : 다른 위치에서 axis축 이동 5회 측정

            if (!mAutoCalibrationRun)
                return;

            TextAppendTbInfo($"Start {axis}-axis Measurement");
            List<double[]> collectedData = new List<double[]>();
            List<double[]> measuredData = null;
            switch (axis)
            {
                case Axis.X:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (!mAutoCalibrationRun) break;
                            collectedData.Add(null);

                            switch (i)
                            {
                                case 0:
                                    measuredData = ScanAxis(axis, onewayStrokeUm);
                                    break;
                                case 1:
                                    // y 1000에서 x 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.Y, 900);    // 1000
                                    break;
                                case 2:
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.Y, -900);
                                    break;
                                case 3:
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.Z, 900);    // 1000
                                    break;
                                case 4:
                                    // z 1000에서 x 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.Z, -900);
                                    break;
                            }
                            collectedData.AddRange(measuredData);
                            if (isSingle) break;   // 재 Cal은 원점에서 x 이동만
                        }
                        break;
                    }
                case Axis.Y:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (!mAutoCalibrationRun) break;
                            collectedData.Add(null);

                            switch (i)
                            {
                                case 0:
                                    // y 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm);
                                    break;
                                case 1:
                                    // x 1000에서 y 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.X, 900);    // 1000
                                    break;
                                case 2:
                                    // x -1000에서 y 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.X, -900);
                                    break;
                                case 3:
                                    measuredData = ScanAxis(axis, 700, Axis.Z, 600);    // 700 600
                                    break;
                                case 4:
                                    // z 1000에서 x 이동하면서 측정
                                    measuredData = ScanAxis(axis, 700, Axis.Z, -600);
                                    break;
                            }
                            collectedData.AddRange(measuredData);
                            if (isSingle) break;
                        }
                        break;
                    }
                case Axis.Z:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (!mAutoCalibrationRun) break;
                            collectedData.Add(null);

                            switch (i)
                            {
                                case 0:
                                    // z 이동하면서 측정       
                                    measuredData = ScanAxis(axis, onewayStrokeUm);
                                    break;
                                case 1:
                                    // x 1000에서 z 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.X, 900);    // 1000
                                    break;
                                case 2:
                                    // x -1000에서 z 이동하면서 측정
                                    measuredData = ScanAxis(axis, onewayStrokeUm, Axis.X, -900);
                                    break;
                                case 3:
                                    // y 600에서 z 이동하면서 측정
                                    measuredData = ScanAxis(axis, 700, Axis.Y, 600);    // 600
                                    break;
                                case 4:
                                    // y -600에서 z 이동하면서 측정
                                    measuredData = ScanAxis(axis, 700, Axis.Y, -600);
                                    break;
                            }
                            collectedData.AddRange(measuredData);
                            if (isSingle) break;
                        }
                        break;
                    }
                case Axis.TX:
                case Axis.TY:
                case Axis.TZ:
                    {
                        collectedData.Add(null);
                        measuredData = ScanAxis(axis, onewayStrokeUm);
                        collectedData.AddRange(measuredData);
                        break;
                    }
            }
            TextAppendTbInfo($"End {axis}-axis Measurement");
            RemoteAxisCalibration(axis, collectedData, isRemote, isReCal);
        }
        public List<double[]> ScanAxis(Axis axis, double onewayStroke, Axis? axis2 = null, double posAxis2 = 0)
        {
            List<double[]> measuredData = new List<double[]>();

            // Hexapod
            if (!mAutoCalibrationRun) { return measuredData; }
            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);

            if (!mAutoCalibrationRun) { return measuredData; }
            mGageFullData.Clear();
            mCalibrationFullData.Clear();

            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

            // 초기위치 (0,0,0)로 이동
            if (!mAutoCalibrationRun) { return measuredData; }

            if (axis < Axis.TX)
            {
                //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);
                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);   // Porg -> 0 : X to Z 보상계수가 부정확한 것으로 나타나는 원인 확인을 위해 비교

            }
            else
            {
                // TX, TY, TZ
                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
                int pivotAxis = (int)axis - 2;

                // FindOQC에서 FindPivot를 함으로 생략  // 01.23.2025 Pivot 변화 미미함 확인.
                //FindPivot(pivotAxis); 
                //ChangePivotXYZ(pivotAxis);  //  저장
                //MotorMoveOriginHexapod();

                MotorSetPivot(mHexapodPivots[pivotAxis - 1].X, mHexapodPivots[pivotAxis - 1].Y, mHexapodPivots[pivotAxis - 1].Z);
                MotorSetHCS(0, 0, -mHCSrotation.Z);
            }

            //double[] x = MotorCurPosHexapod();
            //// 헥사포드 200 시프트 한 위치에서 테스트
            //m__G.fMotion.MoveHexapodAxis(Axis.X, x[0] + 200);
            //x = MotorCurPosHexapod();
            //m__G.fMotion.MoveRelAxis(Axis.X, -200);

            mAutoCalibrationIndex = 0;
            SingleFindMark();


            if (axis2 != null && posAxis2 != 0)
            {
                // axis2의 posAxis2로 이동
                if (!mAutoCalibrationRun) { return measuredData; }
                double orgPos2 = MotorCurPosAxis((Axis)axis2);
                for (int i = 1; i < 4; i++)    // axis2 1/3씩 이동
                {
                    double pos2 = orgPos2 + i * (posAxis2 / 3);
                    MotorMoveAbsAxis((Axis)axis2, pos2);
                    SingleFindMark();
                }
            }

            // axis  onewayStroke 1/3씩 이동
            if (!mAutoCalibrationRun) { return measuredData; }
            double orgPos = MotorCurPosAxis(axis);

            double pos = orgPos - (onewayStroke) / 3;

            MotorMoveAbsAxis(axis, pos);
            Thread.Sleep(300);  // 스테이지 안정화 될때까지 기다리기
            SingleFindMark();   // 측정

            if (!mAutoCalibrationRun) { return measuredData; }
            pos = orgPos - 2 * onewayStroke / 3;
            MotorMoveAbsAxis(axis, pos);
            Thread.Sleep(300);
            SingleFindMark();

            // backlash 제거
            if (!mAutoCalibrationRun) { return measuredData; }
            if (axis < Axis.TX)
            {
                pos = orgPos - onewayStroke - 300;
            }
            else
            {
                pos = orgPos - onewayStroke - 15;
            }
            MotorMoveAbsAxis(axis, pos);
            Thread.Sleep(200);
            var gageData = m__G.mGageCounter.ReadPortAll();
            if (!mAutoCalibrationRun) { return measuredData; }
            if (axis < Axis.TX)
            {
                pos = orgPos - onewayStroke - 200;
            }
            else
            {
                pos = orgPos - onewayStroke - 10;
            }
            MotorMoveAbsAxis(axis, pos);
            Thread.Sleep(300);
            gageData = m__G.mGageCounter.ReadPortAll();
            if (!mAutoCalibrationRun) { return measuredData; }
            if (axis < Axis.TX)
            {
                pos = orgPos - onewayStroke - 100;
            }
            else
            {
                pos = orgPos - onewayStroke - 5;
            }
            MotorMoveAbsAxis(axis, pos);
            Thread.Sleep(300);
            gageData = m__G.mGageCounter.ReadPortAll();

            // 누적된 데이터 Clear
            mGageFullData.Clear();
            mCalibrationFullData.Clear();

            // 진짜 측정 시작
            if (!mAutoCalibrationRun) { return measuredData; }
            pos = orgPos - onewayStroke;
            MotorMoveAbsAxis(axis, pos);
            if (axis < Axis.TX)
            {
                Thread.Sleep(600);
            }
            else
            {
                Thread.Sleep(200);
            }
            SingleFindMark();

            // X, Y, Z => 100um
            double dStroke = 100; // 50um

            if (axis < Axis.TX)
            {
                // X, Y, Z => 100um
                dStroke = 100; // 50um
            }
            else
            {
                // TX, TY, TZ => min
                dStroke = 12;  // 0.1 deg -> 6 min  // TX : 0.2 deg -> 12min
            }

            //double movingStroke = onewayStroke;
            //while (movingStroke > -onewayStroke)
            double movingStroke = -onewayStroke;
            while (movingStroke < onewayStroke)
            {
                if (!mAutoCalibrationRun)
                {
                    measuredData = mCalibrationFullData.ToList();
                    return measuredData;
                }
                pos += dStroke;
                movingStroke += dStroke;
                MotorMoveAbsAxis(axis, pos);
                if (axis < Axis.TX)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Thread.Sleep(200);
                };
                SingleFindMark();
            }
            ////////////////////////////////////////////////////
            ////////////////////////////////////////////////////
            ///역방향 측정 시 다음 활용 가능
            //while (movingStroke > -onewayStroke)
            //{
            //    if (!mAutoCalibrationRun)
            //    {
            //        measuredData = mCalibrationFullData.ToList();
            //        return measuredData;
            //    }
            //    pos -= dStroke;
            //    movingStroke -= dStroke;
            //    MotorMoveAbsAxis(axis, pos);
            //    if (axis < Axis.TX)
            //    {
            //        Thread.Sleep(1000);
            //    }
            //    else
            //    {
            //        Thread.Sleep(200);
            //    };
            //    SingleFindMark();
            //}
            ////////////////////////////////////////////////////
            ////////////////////////////////////////////////////


            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

            // CSH 0,0,0 위치로 복귀
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);
            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);

            // 측정 데이터 반환
            measuredData = mCalibrationFullData.ToList();
            return measuredData;

            //// 결과 파일 저장
            //StreamWriter wr = null;
            //DateTime lnow = DateTime.Now;
            //try
            //{
            //    wr = new StreamWriter($"C:\\CSHTest\\DoNotTouch\\Admin\\StabilizedData_{axis}{axis2}_{posAxis2}.csv");
            //}
            //catch (Exception e)
            //{
            //    wr = new StreamWriter($"C:\\CSHTest\\DoNotTouch\\Admin\\StabilizedData_{axis}{axis2}_{posAxis2}_{lnow:hhmmss}.csv");
            //}
            //double[][] stablizedData = mCalibrationFullData.ToArray();

            //wr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ");
            //for (int i = 0; i < stablizedData.Length; i++)
            //{
            //    string slstr = i.ToString() + ",";
            //    for (int j = 0; j < 23; j++)
            //    {
            //        slstr += stablizedData[i][j].ToString("F5") + ",";
            //    }

            //    wr.WriteLine(slstr);
            //}
            //wr.Close();
        }
        public void RemoteEastViewYPCalibration(List<double[]> collectedData, bool isRemote)
        {

            var stabilizedDataList = SaveCalibrationData(collectedData, isRemote, "EastViewYP");
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;

            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
            string lstr = "";

            //  stablizedData[i][0] : X
            //  stablizedData[i][1] : Y
            //  stablizedData[i][2] : Z
            //  stablizedData[i][3 : TX   
            //  stablizedData[i][4 : TY   
            //  stablizedData[i][5] : TZ   
            //  stablizedData[i][16] : X
            //  stablizedData[i][17] : Y
            //  stablizedData[i][18] : Z
            //  stablizedData[i][19] : TX   
            //  stablizedData[i][20] : TY   
            //  stablizedData[i][21] : TZ   


            if (stabilizedDataList.Count < 2) return;
            // Z
            List<double[]> stabilizedData = stabilizedDataList[0];
            int effLength = stabilizedData.Count;
            mZCalAvgY1Y2pp = Math.Abs(stabilizedData[0][7] - stabilizedData[effLength - 1][7] + stabilizedData[0][9] - stabilizedData[effLength - 1][9]) / 2;
            mZCalY3pp = Math.Abs(stabilizedData[0][11] - stabilizedData[effLength - 1][11]);
            // Y
            stabilizedData = stabilizedDataList[1];
            effLength = stabilizedData.Count;
            mYCalAvgY1Y2pp = Math.Abs(stabilizedData[0][7] - stabilizedData[effLength - 1][7] + stabilizedData[0][9] - stabilizedData[effLength - 1][9]) / 2;
            mYCalY3pp = Math.Abs(stabilizedData[0][11] - stabilizedData[effLength - 1][11]);

            mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);
            lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";


            if (isRemote)
            {
                // 저장
                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                StreamReader sr = new StreamReader(scaleNthetaFile);
                string allstr = sr.ReadToEnd();
                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                sr.Close();

                string[] strEastScaleLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                allLines[7] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
                for (int i = 1; i < strEastScaleLine.Length; i++)
                    allLines[7] += strEastScaleLine[i];

                StreamWriter wr = new StreamWriter(scaleNthetaFile);
                for (int i = 0; i < allLines.Length; i++)
                {
                    wr.WriteLine(allLines[i]);
                }
                wr.Close();
                TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
            }

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbCalibration.Text += lstr;
                });
            else
                tbCalibration.Text += lstr;
        }
        public void RemoteAxisCalibration(Axis axis, List<double[]> collectedData, bool IsRemote, bool IsRecal)
        {
            var stabilizedDataList = SaveCalibrationData(collectedData, IsRemote, axis.ToString());
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;

            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
            string lstr = "";

            switch (axis)
            {
                //  stablizedData[i][0] : X
                //  stablizedData[i][1] : Y
                //  stablizedData[i][2] : Z
                //  stablizedData[i][3 : TX   
                //  stablizedData[i][4 : TY   
                //  stablizedData[i][5] : TZ   
                //  stablizedData[i][16] : X
                //  stablizedData[i][17] : Y
                //  stablizedData[i][18] : Z
                //  stablizedData[i][19] : TX   
                //  stablizedData[i][20] : TY   
                //  stablizedData[i][21] : TZ   
                case Axis.X:
                    {
                        List<double[]> stabilizedData = stabilizedDataList[0];
                        int effLength = stabilizedData.Count;

                        var sXX = new FZMath.Point2D[effLength];
                        var sXtoY = new FZMath.Point2D[effLength];
                        var sXtoZ = new FZMath.Point2D[effLength];
                        var sXtoTX = new FZMath.Point2D[effLength];
                        var sXtoTY = new FZMath.Point2D[effLength];
                        var sXtoTZ = new FZMath.Point2D[effLength];

                        double[] sY = new double[effLength];
                        double[] sZ = new double[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            double x = stabilizedData[i][0];    // CSH X 값
                            sXX[i] = new FZMath.Point2D(x, stabilizedData[i][16]); // probe X
                            sXtoY[i] = new FZMath.Point2D(x, stabilizedData[i][1] - stabilizedData[i][17]);   //  CSH Y - probe Y from 6 axis stage
                            sXtoZ[i] = new FZMath.Point2D(x, stabilizedData[i][2] - stabilizedData[i][18]);   //  CSH Z - probe Z from 6 axis stage
                            sXtoTX[i] = new FZMath.Point2D(x, stabilizedData[i][3] - stabilizedData[i][19]);   //  CSH TX - probe TX from 6 axis stage
                            sXtoTY[i] = new FZMath.Point2D(x, stabilizedData[i][4] - stabilizedData[i][20]);   //  CSH TY - probe TY from 6 axis stage
                            sXtoTZ[i] = new FZMath.Point2D(x, stabilizedData[i][5] - stabilizedData[i][21]);   //  CSH TZ - probe TZ from 6 axis stage

                            sY[i] = stabilizedData[i][1];
                            sZ[i] = stabilizedData[i][2];
                        }

                        double[] XtoXab = new double[3];
                        double[] XtoYab = new double[3];
                        double[] XtoZab = new double[3];
                        double[] XtoTXab = new double[3];
                        double[] XtoTYab = new double[3];
                        double[] XtoTZab = new double[3];

                        double Yavg = sY.Average();
                        double Zavg = sZ.Average();

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXX, effLength, ref XtoXab[1], ref XtoXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoY, effLength, ref XtoYab[1], ref XtoYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoZ, effLength, ref XtoZab[1], ref XtoZab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTX, effLength, ref XtoTXab[1], ref XtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTY, effLength, ref XtoTYab[1], ref XtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTZ, effLength, ref XtoTZab[1], ref XtoTZab[0]);

                            lstr = "XX Scale\t" + XtoXab[1].ToString("E5") + "\r\n";
                            lstr += "XtoY\t" + XtoYab[1].ToString("E5") + "\r\n";
                            lstr += "XtoZ\t" + XtoZab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTX\t" + XtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTY\t" + XtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTZ\t" + XtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    XtoXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleX[i] * XtoXab[i];
                                    XtoYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoYbyView[i] + XtoYab[i];
                                    XtoZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoZbyView[i] + XtoZab[i];
                                    XtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTXbyView[i] + XtoTXab[i];
                                    XtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTYbyView[i] + XtoTYab[i];
                                    XtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTZbyView[i] + XtoTZab[i];
                                }
                                else
                                {
                                    XtoXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleX[i];
                                    XtoYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoYbyView[i];
                                    XtoZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoZbyView[i];
                                    XtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTXbyView[i];
                                    XtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTYbyView[i];
                                    XtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTZbyView[i];
                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sXX, effLength, ref XtoXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sXtoY, effLength, ref XtoYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sXtoZ, effLength, ref XtoZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTX, effLength, ref XtoTXab[1], ref XtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTY, effLength, ref XtoTYab[1], ref XtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTZ, effLength, ref XtoTZab[1], ref XtoTZab[0]);

                            lstr = "XX Scale\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";
                            lstr += "XtoY\t" + XtoYab[0].ToString("E5") + ",\t" + XtoYab[1].ToString("E5") + ",\t" + XtoYab[2].ToString("E5") + "\r\n";
                            lstr += "XtoZ\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
                            lstr += "XtoTX\t" + XtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTY\t" + XtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTZ\t" + XtoTZab[1].ToString("E5") + "\r\n";
                        }

                        // XJtoX 구하기
                        double[] XJtoXab = null;
                        if (!IsRecal)
                        {
                            XJtoXab = new double[2];

                            // XY to X
                            if (stabilizedDataList.Count > 2)
                            {

                                double[] XYtoXab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 1];
                                    effLength = stabilizedData.Count;

                                    sY = new double[effLength];
                                    sXX = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sY[j] = stabilizedData[j][1];    // CSH Y 값
                                        sXX[j] = new FZMath.Point2D(stabilizedData[j][0], stabilizedData[j][16]); // probe X
                                    }

                                    double YavgfromXY = sY.Average();
                                    double[] XtoXabfromeXY = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref XtoXabfromeXY[1], ref XtoXabfromeXY[0]);

                                    XYtoXab[i] = (XtoXabfromeXY[1] - XtoXab[1]) / (YavgfromXY - Yavg);
                                }

                                XJtoXab[0] = XYtoXab.Average();
                            }

                            // XZ to X
                            if (stabilizedDataList.Count == 5)
                            {
                                double[] XZtoXab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 3];
                                    effLength = stabilizedData.Count;

                                    sZ = new double[effLength];
                                    sXX = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sZ[j] = stabilizedData[j][2];    // CSH Z 값
                                        sXX[j] = new FZMath.Point2D(stabilizedData[j][0], stabilizedData[j][16]); // probe Z
                                    }

                                    double ZavgfromXZ = sZ.Average();
                                    double[] XtoXabfromeXZ = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref XtoXabfromeXZ[1], ref XtoXabfromeXZ[0]);

                                    XZtoXab[i] = (XtoXabfromeXZ[1] - XtoXab[1]) / (ZavgfromXZ - Zavg);
                                }

                                XJtoXab[1] = XZtoXab.Average();
                            }

                            lstr += "XYtoX\t" + XJtoXab[0].ToString("E5") + ",\t" + "XZtoX\t" + XJtoXab[1].ToString("E5") + "\r\n";
                        }

                        // ScaleNTheta 업데이트
                        if (IsRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // X
                            string[] strXXscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[1] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strXXscaleLine.Length; i++)
                                allLines[1] += strXXscaleLine[i];
                            // X TO Y
                            string[] strXtoYLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[8] = XtoYab[0].ToString("E5") + "\t" + XtoYab[1].ToString("E5") + "\t" + XtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strXtoYLine.Length; i++)
                                allLines[8] += strXtoYLine[i];
                            // X TO Z
                            string[] strXtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[9] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strXtoZLine.Length; i++)
                                allLines[9] += strXtoZLine[i];
                            // X TO TX
                            string[] strXtoTXLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[10] = $"{0:E5}\t{XtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strXtoTXLine.Length; i++)
                                allLines[10] += strXtoTXLine[i];
                            // X TO TY
                            string[] strXtoTYLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[11] = $"{0:E5}\t{XtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strXtoTYLine.Length; i++)
                                allLines[11] += strXtoTYLine[i];
                            // X TO TZ
                            string[] strXtoTZLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[12] = $"{0:E5}\t{XtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strXtoTZLine.Length; i++)
                                allLines[12] += strXtoTZLine[i];

                            if (XJtoXab != null && !IsRecal)
                            {
                                string[] strXJtoXLine = allLines[29].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                allLines[29] = XJtoXab[0].ToString("E5") + "\t" + XJtoXab[1].ToString("E5") + "\t//";
                                for (int i = 1; i < strXJtoXLine.Length; i++)
                                    allLines[29] += strXJtoXLine[i];
                            }

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.Y:
                    {
                        List<double[]> stabilizedData = stabilizedDataList[0];
                        int effLength = stabilizedData.Count;

                        var sYY = new FZMath.Point2D[effLength];
                        var sYtoX = new FZMath.Point2D[effLength];
                        var sYtoZ = new FZMath.Point2D[effLength];
                        var sYtoTX = new FZMath.Point2D[effLength];
                        var sYtoTY = new FZMath.Point2D[effLength];
                        var sYtoTZ = new FZMath.Point2D[effLength];

                        double[] sX = new double[effLength];
                        double[] sZ = new double[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            double x = stabilizedData[i][1];    // CSH Y 값
                            sYY[i] = new FZMath.Point2D(x, stabilizedData[i][17]); // probe X
                            sYtoX[i] = new FZMath.Point2D(x, stabilizedData[i][0] - stabilizedData[i][16]);   //  CSH X - probe X from 6 axis stage
                            sYtoZ[i] = new FZMath.Point2D(x, stabilizedData[i][2] - stabilizedData[i][18]);   //  CSH Z - probe Z from 6 axis stage
                            sYtoTX[i] = new FZMath.Point2D(x, stabilizedData[i][3] - stabilizedData[i][19]);   //  CSH TX - probe TX from 6 axis stage
                            sYtoTY[i] = new FZMath.Point2D(x, stabilizedData[i][4] - stabilizedData[i][20]);   //  CSH TY - probe TY from 6 axis stage
                            sYtoTZ[i] = new FZMath.Point2D(x, stabilizedData[i][5] - stabilizedData[i][21]);   //  CSH TZ - probe TZ from 6 axis stage

                            sX[i] = stabilizedData[i][0];
                            sZ[i] = stabilizedData[i][2];
                        }

                        double[] YtoYab = new double[3];
                        double[] YtoXab = new double[3];
                        double[] YtoZab = new double[3];
                        double[] YtoTXab = new double[3];
                        double[] YtoTYab = new double[3];
                        double[] YtoTZab = new double[3];

                        double Xavg = sX.Average();
                        double Zavg = sZ.Average();

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYY, effLength, ref YtoYab[1], ref YtoYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoX, effLength, ref YtoXab[1], ref YtoXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoZ, effLength, ref YtoZab[1], ref YtoZab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTX, effLength, ref YtoTXab[1], ref YtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTY, effLength, ref YtoTYab[1], ref YtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTZ, effLength, ref YtoTZab[1], ref YtoTZab[0]);

                            lstr = "YY Scale\t" + YtoYab[1].ToString("E5") + "\r\n";
                            lstr += "YtoX\t" + YtoXab[1].ToString("E5") + "\r\n";
                            lstr += "YtoZ\t" + YtoZab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTX\t" + YtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTY\t" + YtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTZ\t" + YtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    YtoYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleY[i] * YtoYab[i];
                                    YtoXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoXbyView[i] + YtoXab[i];
                                    YtoZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoZbyView[i] + YtoZab[i];
                                    YtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTXbyView[i] + YtoTXab[i];
                                    YtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTYbyView[i] + YtoTYab[i];
                                    YtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTZbyView[i] + YtoTZab[i];
                                }
                                else
                                {
                                    YtoYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleY[i];
                                    YtoXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoXbyView[i];
                                    YtoZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoZbyView[i];
                                    YtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTXbyView[i];
                                    YtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTYbyView[i];
                                    YtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTZbyView[i];
                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sYY, effLength, ref YtoYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sYtoX, effLength, ref YtoXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sYtoZ, effLength, ref YtoZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTX, effLength, ref YtoTXab[1], ref YtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTY, effLength, ref YtoTYab[1], ref YtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTZ, effLength, ref YtoTZab[1], ref YtoTZab[0]);

                            lstr = "YY Scale\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
                            lstr += "YtoX\t" + YtoXab[0].ToString("E5") + ",\t" + YtoXab[1].ToString("E5") + ",\t" + YtoXab[2].ToString("E5") + "\r\n";
                            lstr += "YtoZ\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";

                            lstr += "YtoTX\t" + YtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTY\t" + YtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTZ\t" + YtoTYab[1].ToString("E5") + "\r\n";
                        }

                        // YJtoY 구하기
                        double[] YJtoYab = null;
                        if (!IsRecal)
                        {
                            YJtoYab = new double[2];

                            // YX to Y
                            if (stabilizedDataList.Count > 2)
                            {

                                double[] YXtoYab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 1];
                                    effLength = stabilizedData.Count;

                                    sX = new double[effLength];
                                    sYY = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sX[j] = stabilizedData[j][0];    // CSH X 값
                                        sYY[j] = new FZMath.Point2D(stabilizedData[j][1], stabilizedData[j][17]); // probe Y
                                    }

                                    double XavgfromYX = sX.Average();
                                    double[] YtoYabfromeYX = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref YtoYabfromeYX[1], ref YtoYabfromeYX[0]);

                                    YXtoYab[i] = (YtoYabfromeYX[1] - YtoYab[1]) / (XavgfromYX - Xavg);
                                }

                                YJtoYab[0] = YXtoYab.Average();
                            }

                            // YZ to Y
                            if (stabilizedDataList.Count == 5)
                            {
                                double[] YZtoYab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 3];
                                    effLength = stabilizedData.Count;

                                    sZ = new double[effLength];
                                    sYY = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sZ[j] = stabilizedData[j][2];    // CSH Z 값
                                        sYY[j] = new FZMath.Point2D(stabilizedData[j][1], stabilizedData[j][17]); // probe Y
                                    }

                                    double ZavgfromYZ = sZ.Average();
                                    double[] YtoYabfromeYZ = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref YtoYabfromeYZ[1], ref YtoYabfromeYZ[0]);

                                    YZtoYab[i] = (YtoYabfromeYZ[1] - YtoYab[1]) / (ZavgfromYZ - Zavg);
                                }

                                YJtoYab[1] = YZtoYab.Average();
                            }

                            lstr += "YXtoY\t" + YJtoYab[0].ToString("E5") + ",\t" + "YZtoY\t" + YJtoYab[1].ToString("E5") + "\r\n";
                        }

                        // ScaleNTheta 업데이트
                        if (IsRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            // Y
                            string[] strYYscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[2] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYYscaleLine.Length; i++)
                                allLines[2] += strYYscaleLine[i];
                            // Y TO X
                            string[] strYtoXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[13] = YtoXab[0].ToString("E5") + "\t" + YtoXab[1].ToString("E5") + "\t" + YtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoXLine.Length; i++)
                                allLines[13] += strYtoXLine[i];
                            // Y TO Z
                            string[] strYtoZLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[14] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoZLine.Length; i++)
                                allLines[14] += strYtoZLine[i];
                            // Y TO TX
                            string[] strYtoTXLine = allLines[15].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[15] = $"{0:E5}\t{YtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strYtoTXLine.Length; i++)
                                allLines[15] += strYtoTXLine[i];
                            // Y TO TY
                            string[] strYtoTYLine = allLines[16].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[16] = $"{0:E5}\t{YtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strYtoTYLine.Length; i++)
                                allLines[16] += strYtoTYLine[i];
                            // Y TO TZ
                            string[] strYtoTZLine = allLines[17].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[17] = $"{0:E5}\t{YtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strYtoTZLine.Length; i++)
                                allLines[17] += strYtoTZLine[i];

                            if (YJtoYab != null && !IsRecal)
                            {
                                string[] strYJtoYLine = allLines[30].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                allLines[30] = YJtoYab[0].ToString("E5") + "\t" + YJtoYab[1].ToString("E5") + "\t//";
                                for (int i = 1; i < strYJtoYLine.Length; i++)
                                    allLines[30] += strYJtoYLine[i];
                            }

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.Z:
                    {
                        //  Z scale 구하기
                        List<double[]> stabilizedData = stabilizedDataList[0];
                        int effLength = stabilizedData.Count;

                        FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

                        double[] sX = new double[effLength];
                        double[] sY = new double[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            double x = stabilizedData[i][2];    // CSH Z 값
                            sZZ[i] = new FZMath.Point2D(x, stabilizedData[i][18]); // probe Z
                            sZtoX[i] = new FZMath.Point2D(x, stabilizedData[i][0] - stabilizedData[i][16]);   //  CSH X - probe X from 6 axis stage
                            sZtoY[i] = new FZMath.Point2D(x, stabilizedData[i][1] - stabilizedData[i][17]);   //  CSH Y - probe Y from 6 axis stage
                            sZtoTX[i] = new FZMath.Point2D(x, stabilizedData[i][3] - stabilizedData[i][19]);   //  CSH TX - probe TX from 6 axis stage
                            sZtoTY[i] = new FZMath.Point2D(x, stabilizedData[i][4] - stabilizedData[i][20]);   //  CSH TY - probe TY from 6 axis stage
                            sZtoTZ[i] = new FZMath.Point2D(x, stabilizedData[i][5] - stabilizedData[i][21]);   //  CSH TZ - probe TZ from 6 axis stage

                            sX[i] = stabilizedData[i][0];
                            sY[i] = stabilizedData[i][1];
                        }

                        double[] ZtoZab = new double[3];
                        double[] ZtoXab = new double[3];
                        double[] ZtoYab = new double[3];
                        double[] ZtoTXab = new double[3];
                        double[] ZtoTYab = new double[3];
                        double[] ZtoTZab = new double[3];

                        double Xavg = sX.Average();
                        double Yavg = sY.Average();

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZZ, effLength, ref ZtoZab[1], ref ZtoZab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoX, effLength, ref ZtoXab[1], ref ZtoXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoY, effLength, ref ZtoYab[1], ref ZtoXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTX, effLength, ref ZtoTXab[1], ref ZtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTY, effLength, ref ZtoTYab[1], ref ZtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTZ, effLength, ref ZtoTZab[1], ref ZtoTZab[0]);

                            lstr = "ZZ Scale\t" + ZtoZab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoX\t" + ZtoXab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoY\t" + ZtoYab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTX\t" + ZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTY\t" + ZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTZ\t" + ZtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    ZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleZ[i] * ZtoZab[i];
                                    ZtoXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoXbyView[i] + ZtoXab[i];
                                    ZtoYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoYbyView[i] + ZtoYab[i];
                                    ZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTXbyView[i] + ZtoTXab[i];
                                    ZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTYbyView[i] + ZtoTYab[i];
                                    ZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTZbyView[i] + ZtoTZab[i];
                                }
                                else
                                {
                                    ZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleZ[i];
                                    ZtoXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoXbyView[i];
                                    ZtoYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoYbyView[i];
                                    ZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTXbyView[i];
                                    ZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTYbyView[i];
                                    ZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTZbyView[i];
                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sZZ, effLength, ref ZtoZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sZtoX, effLength, ref ZtoXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sZtoY, effLength, ref ZtoYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTX, effLength, ref ZtoTXab[1], ref ZtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTY, effLength, ref ZtoTYab[1], ref ZtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTZ, effLength, ref ZtoTZab[1], ref ZtoTZab[0]);

                            lstr = "ZZ Scale\t" + ZtoZab[0].ToString("E5") + ",\r\n" + ZtoZab[1].ToString("E5") + ",\t" + ZtoZab[2].ToString("E5") + "\r\n";
                            lstr += "ZtoX\t" + ZtoXab[0].ToString("E5") + ",\t" + ZtoXab[1].ToString("E5") + ",\t" + ZtoXab[2].ToString("E5") + "\r\n";
                            lstr += "ZtoY\t" + ZtoYab[0].ToString("E5") + ",\t" + ZtoYab[1].ToString("E5") + ",\t" + ZtoYab[2].ToString("E5") + "\r\n";
                            lstr += "ZtoTX\t" + ZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTY\t" + ZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTZ\t" + ZtoTZab[1].ToString("E5") + "\r\n";
                        }



                        // ZJtoZ 구하기
                        double[] ZJtoZab = null;
                        if (!IsRecal)
                        {
                            ZJtoZab = new double[2];

                            // ZX to Z
                            if (stabilizedDataList.Count > 2)
                            {

                                double[] ZXtoZab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 1];
                                    effLength = stabilizedData.Count;

                                    sX = new double[effLength];
                                    sZZ = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sX[j] = stabilizedData[j][0];    // CSH X 값
                                        sZZ[j] = new FZMath.Point2D(stabilizedData[j][2], stabilizedData[j][18]); // probe Z
                                    }

                                    double XavgfromZX = sX.Average();
                                    double[] ZtoZabfromeZX = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZZ, effLength, ref ZtoZabfromeZX[1], ref ZtoZabfromeZX[0]);

                                    ZXtoZab[i] = (ZtoZabfromeZX[1] - ZtoZab[1]) / (XavgfromZX - Xavg);
                                }

                                ZJtoZab[0] = ZXtoZab.Average();
                            }

                            // ZY to Z
                            if (stabilizedDataList.Count == 5)
                            {
                                double[] ZYtoZab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 3];
                                    effLength = stabilizedData.Count;

                                    sY = new double[effLength];
                                    sZZ = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sY[j] = stabilizedData[j][1];    // CSH Y 값
                                        sZZ[j] = new FZMath.Point2D(stabilizedData[j][2], stabilizedData[j][18]); // probe Z
                                    }

                                    double ZavgfromZY = sY.Average();
                                    double[] ZtoZabfromeZY = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZZ, effLength, ref ZtoZabfromeZY[1], ref ZtoZabfromeZY[0]);

                                    ZYtoZab[i] = (ZtoZabfromeZY[1] - ZtoZab[1]) / (ZavgfromZY - Yavg);
                                }

                                ZJtoZab[1] = ZYtoZab.Average();
                            }

                            lstr += "ZXtoZ\t" + ZJtoZab[0].ToString("E5") + ",\t" + "ZXtoZ\t" + ZJtoZab[1].ToString("E5") + "\r\n";
                        }

                        // ScaleNTheta 업데이트
                        if (IsRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // Z
                            string[] strZscaleLine = allLines[3].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[3] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZscaleLine.Length; i++)
                                allLines[3] += strZscaleLine[i];
                            // Z TO X
                            string[] strZtoXLine = allLines[18].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[18] = ZtoXab[0].ToString("E5") + "\t" + ZtoXab[1].ToString("E5") + "\t" + ZtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoXLine.Length; i++)
                                allLines[18] += strZtoXLine[i];
                            // Z TO Y
                            string[] strZtoYLine = allLines[19].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[19] = ZtoYab[0].ToString("E5") + "\t" + ZtoYab[1].ToString("E5") + "\t" + ZtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoYLine.Length; i++)
                                allLines[19] += strZtoYLine[i];
                            // Z TO TX
                            string[] strZtoTXLine = allLines[20].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[20] = $"{0:E5}\t{ZtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strZtoTXLine.Length; i++)
                                allLines[20] += strZtoTXLine[i];
                            // Z TO TY
                            string[] strZtoTYLine = allLines[21].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[21] = $"{0:E5}\t{ZtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strZtoTYLine.Length; i++)
                                allLines[21] += strZtoTYLine[i];
                            // Z TO TZ
                            string[] strZtoTZLine = allLines[22].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[22] = $"{0:E5}\t{ZtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strZtoTZLine.Length; i++)
                                allLines[22] += strZtoTZLine[i];

                            if (ZJtoZab != null && !IsRecal)
                            {
                                string[] strZJtoZLine = allLines[31].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                allLines[31] = ZJtoZab[0].ToString("E5") + "\t" + ZJtoZab[1].ToString("E5") + "\t//";
                                for (int i = 1; i < strZJtoZLine.Length; i++)
                                    allLines[31] += strZJtoZLine[i];
                            }

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.TX:
                    {
                        //  TX scale 구하기
                        List<double[]> stabilizedData = stabilizedDataList[0];
                        int effLength = stabilizedData.Count;

                        var sTXTX = new FZMath.Point2D[effLength];
                        var sTXtoTY = new FZMath.Point2D[effLength];
                        var sTXtoTZ = new FZMath.Point2D[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            sTXTX[i] = new FZMath.Point2D(stabilizedData[i][3], stabilizedData[i][19]);
                            sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stabilizedData[i][4] - stabilizedData[i][20]);   //  TY - probe TY from 6 axis stage
                            sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stabilizedData[i][5] - stabilizedData[i][21]);   //  TZ - probe TZ from 6 axis stage
                        }

                        double[] TXtoTXab = new double[3];
                        double[] TXtoTYab = new double[3];
                        double[] TXtoTZab = new double[3];

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXTX, effLength, ref TXtoTXab[1], ref TXtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTY, effLength, ref TXtoTYab[1], ref TXtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTZ, effLength, ref TXtoTZab[1], ref TXtoTZab[0]);

                            lstr = "TX Scale\t" + TXtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TXtoTY\t" + TXtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TXtoTZ\t" + TXtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    TXtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTX[i] * TXtoTXab[i];
                                    TXtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTYbyView[i] + TXtoTYab[i];
                                    TXtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTZbyView[i] + TXtoTZab[i];

                                }
                                else
                                {
                                    TXtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTX[i];
                                    TXtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTYbyView[i];
                                    TXtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTZbyView[i];

                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sTXTX, effLength, ref TXtoTXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTY, effLength, ref TXtoTYab[1], ref TXtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTZ, effLength, ref TXtoTZab[1], ref TXtoTZab[0]);

                            lstr = "TX Scale\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";
                            lstr += "TXtoTY\t" + TXtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TXtoTZ\t" + TXtoTZab[1].ToString("E5") + "\r\n";
                        }



                        // ScaleNTheta 업데이트
                        if (IsRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();

                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // TX
                            string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strTXscaleLine.Length; i++)
                                allLines[4] += strTXscaleLine[i];
                            // TX TO TY
                            string[] strTXtoTYLine = allLines[23].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[23] = $"{0:E5}\t{TXtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTXtoTYLine.Length; i++)
                                allLines[23] += strTXtoTYLine[i];
                            // TX TO TZ
                            string[] strTXtoTZLine = allLines[24].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[24] = $"{0:E5}\t{TXtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTXtoTZLine.Length; i++)
                                allLines[24] += strTXtoTZLine[i];

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.TY:
                    {
                        //  TY scale 구하기
                        List<double[]> stabilizedData = stabilizedDataList[0];
                        int effLength = stabilizedData.Count;

                        var sTYTY = new FZMath.Point2D[effLength];
                        var sTYtoTX = new FZMath.Point2D[effLength];
                        var sTYtoTZ = new FZMath.Point2D[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            sTYTY[i] = new FZMath.Point2D(stabilizedData[i][4], stabilizedData[i][20]);
                            sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stabilizedData[i][3] - stabilizedData[i][19]);
                            sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stabilizedData[i][5] - stabilizedData[i][21]);
                        }

                        double[] TYtoTYab = new double[3];
                        double[] TYtoTXab = new double[3];
                        double[] TYtoTZab = new double[3];

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYTY, effLength, ref TYtoTYab[1], ref TYtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTX, effLength, ref TYtoTXab[1], ref TYtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTZ, effLength, ref TYtoTZab[1], ref TYtoTZab[0]);
                            lstr = "TY Scale\t" + TYtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TYtoTX\t" + TYtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TYtoTZ\t" + TYtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    TYtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTY[i] * TYtoTYab[i];
                                    TYtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTXbyView[i] + TYtoTXab[i];
                                    TYtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTZbyView[i] + TYtoTZab[i];

                                }
                                else
                                {
                                    TYtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTY[i];
                                    TYtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTXbyView[i];
                                    TYtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTZbyView[i];

                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sTYTY, effLength, ref TYtoTYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTX, effLength, ref TYtoTXab[1], ref TYtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTZ, effLength, ref TYtoTZab[1], ref TYtoTZab[0]);

                            lstr = "TY Scale\t" + TYtoTYab[0].ToString("E5") + ",\t" + TYtoTYab[1].ToString("E5") + ",\t" + TYtoTYab[2].ToString("E5") + "\r\n";
                            lstr += "TYtoTX\t" + TYtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TYtoTZ\t" + TYtoTZab[1].ToString("E5") + "\r\n";
                        }


                        // ScaleNTheta 업데이트
                        if (IsRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();

                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // TY
                            string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[5] = TYtoTYab[0].ToString("E5") + "\t" + TYtoTYab[1].ToString("E5") + "\t" + TYtoTYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strTYscaleLine.Length; i++)
                                allLines[5] += strTYscaleLine[i];
                            // TY TO TX
                            string[] strTYtoTXLine = allLines[25].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[25] = $"{0:E5}\t{TYtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTYtoTXLine.Length; i++)
                                allLines[25] += strTYtoTXLine[i];
                            // TY TO TZ
                            string[] strTYtoTZLine = allLines[26].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[26] = $"{0:E5}\t{TYtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTYtoTZLine.Length; i++)
                                allLines[26] += strTYtoTZLine[i];

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");

                        }
                        break;
                    }
                case Axis.TZ:
                    {
                        //  TZ scale 구하기
                        List<double[]> stabilizedData = stabilizedDataList[0];
                        int effLength = stabilizedData.Count;

                        var sTZTZ = new FZMath.Point2D[effLength];
                        var sTZtoTX = new FZMath.Point2D[effLength];
                        var sTZtoTY = new FZMath.Point2D[effLength];
                        var sTZtoZ = new Point2D[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            sTZTZ[i] = new FZMath.Point2D(stabilizedData[i][5], stabilizedData[i][21]);
                            sTZtoTX[i] = new FZMath.Point2D(sTZTZ[i].X, stabilizedData[i][3] - stabilizedData[i][19]);
                            sTZtoTY[i] = new FZMath.Point2D(sTZTZ[i].X, stabilizedData[i][4] - stabilizedData[i][20]);
                            sTZtoZ[i] = new FZMath.Point2D(sTZTZ[i].X, stabilizedData[i][2] - stabilizedData[i][18]);
                        }

                        double[] TZtoTZab = new double[3];
                        double[] TZtoTXab = new double[3];
                        double[] TZtoTYab = new double[3];
                        double[] TZtoZab = new double[3];

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZTZ, effLength, ref TZtoTZab[1], ref TZtoTZab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTX, effLength, ref TZtoTXab[1], ref TZtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTY, effLength, ref TZtoTYab[1], ref TZtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoZ, effLength, ref TZtoZab[1], ref TZtoZab[0]);

                            lstr = "TZ Scale\t" + TZtoTZab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTX\t" + TZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTY\t" + TZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoZ\t" + TZtoZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    TZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTZ[i] * TZtoTZab[i];
                                    TZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTXbyView[i] + TZtoTXab[i];
                                    TZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTYbyView[i] + TZtoTYab[i];
                                    TZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoZbyView[i] + TZtoTYab[i];

                                }
                                else
                                {
                                    TZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTZ[i];
                                    TZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTXbyView[i];
                                    TZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTYbyView[i];
                                    TZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoZbyView[i];


                                }
                            }

                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sTZTZ, effLength, ref TZtoTZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTX, effLength, ref TZtoTXab[1], ref TZtoTXab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTY, effLength, ref TZtoTYab[1], ref TZtoTYab[0]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoZ, effLength, ref TZtoZab[1], ref TZtoZab[0]);

                            lstr = "TZ Scale\t" + TZtoTZab[0].ToString("E5") + ",\t" + TZtoTZab[1].ToString("E5") + ",\t" + TZtoTZab[2].ToString("E5") + "\r\n";
                            lstr += "TZtoTX\t" + TZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTY\t" + TZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTY\t" + TZtoZab[1].ToString("E5") + "\r\n";
                        }


                        // ScaleNTheta 업데이트
                        if (IsRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();

                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            // TZ
                            string[] strTZscaleLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[6] = TZtoTZab[0].ToString("E5") + "\t" + TZtoTZab[1].ToString("E5") + "\t" + TZtoTZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strTZscaleLine.Length; i++)
                                allLines[6] += strTZscaleLine[i];
                            // TZ TO TX
                            string[] strTZtoTXLine = allLines[27].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[27] = $"{0:E5}\t{TZtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTZtoTXLine.Length; i++)
                                allLines[27] += strTZtoTXLine[i];
                            // TZ TO TY
                            string[] strTZtoTYLine = allLines[28].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[28] = $"{0:E5}\t{TZtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTZtoTYLine.Length; i++)
                                allLines[28] += strTZtoTYLine[i];

                            // TZ TO Z
                            string[] strTZtoZLine = allLines[32].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[32] = $"{0:E5}\t{TZtoZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTZtoZLine.Length; i++)
                                allLines[32] += strTZtoZLine[i];

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            TextAppendTbInfo($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
            }

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbCalibration.Text += lstr;
                });
            else
                tbCalibration.Text += lstr;
        }
        public List<List<double[]>> SaveCalibrationData(List<double[]> collectedData, bool IsRemote, string axis)
        {
            if (collectedData == null || collectedData.Count == 0) return null;

            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return null;
            }

            string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            if (!Directory.Exists(AdminPathName))
                Directory.CreateDirectory(AdminPathName);

            // 결과 파일 저장
            string strStabilizedFile = $"{AdminPathName}StabilizedData_{axis}_{camID0}_";
            if (IsRemote)
            {
                strStabilizedFile += "Before.csv";
            }
            else
            {
                strStabilizedFile += "After.csv";
            }

            StreamWriter lwr = null;
            try
            {
                lwr = new StreamWriter(strStabilizedFile);
            }
            catch
            {
                strStabilizedFile = strStabilizedFile.Replace(".csv", $"_{DateTime.Now:yyMMdd_HHmmss}.csv");
                lwr = new StreamWriter(strStabilizedFile);
            }
            if (lwr == null) return null;

            List<List<double[]>> stabilizedDataList = new List<List<double[]>>();
            List<double[]> measuredData = null;

            for (int i = 0; i < collectedData.Count; i++)
            {
                if (collectedData[i] == null)
                {
                    if (measuredData != null)
                    {
                        stabilizedDataList.Add(measuredData);
                    }

                    measuredData = new List<double[]>();
                    lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,PTX,PTY1,PTY2,eX,eY,eZ,eTX,eTY,eTZ");
                }
                else
                {
                    measuredData.Add(collectedData[i]);
                    string slstr = i.ToString() + "," + string.Join(",", collectedData[i].Select((x) => x.ToString("F5"))) + ",";
                    slstr += (collectedData[i][0] - collectedData[i][16]).ToString("F5") + "," +
                             (collectedData[i][1] - collectedData[i][17]).ToString("F5") + "," +
                             (collectedData[i][2] - collectedData[i][18]).ToString("F5") + "," +
                             (collectedData[i][3] - collectedData[i][19]).ToString("F5") + "," +
                             (collectedData[i][4] - collectedData[i][20]).ToString("F5") + "," +
                             (collectedData[i][5] - collectedData[i][21]).ToString("F5");
                    lwr.WriteLine(slstr);
                }
            }
            if (measuredData.Count != 0)
            {
                stabilizedDataList.Add(measuredData);
            }
            lwr.Close();

            return stabilizedDataList;
        }


        public bool mbMotorizedStage = false;

        private void cbMotorized_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMotorized.Checked)
            {
                mbMotorizedStage = true;
                cbZaxis.Show();
                cbRaxis.Show();
                cbTiltAxis.Show();
                btnSaveOrgPosition.Show();
            }
            else
            {
                mbMotorizedStage = false;
                cbZaxis.Hide();
                cbRaxis.Hide();
                cbTiltAxis.Hide();
                btnSaveOrgPosition.Hide();
            }
        }

        private void btnSaveOrgPosition_Click(object sender, EventArgs e)
        {
            MotorSetHome6D();

            //double[] curpos = MotorCurPos6axis();
            //StreamWriter wr = new StreamWriter(sFilePath);
            //for ( int i=0; i< curpos.Length; i++)
            //{
            //    wr.WriteLine(curpos[i].ToString());
            //}
            //wr.Close();
        }
        public void LoadStageOrg()
        {
            //string sFilePath = m__G.m_RootDirectory + "\\DoNotTouch\\StageOrg.txt";
            //double[] curpos = new double[6];
            //StreamReader rd = new StreamReader(sFilePath);
            //string lstr = rd.ReadToEnd();
            //rd.Close();

            //string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < allLines.Length; i++)
            //{
            //    curpos[i] = double.Parse(allLines[i]);
            //}
            //return curpos;
        }

        private void cbZaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbZaxis.Checked)
            {
                cbTiltAxis.Checked = false;
                cbRaxis.Checked = false;
            }
            //else
            //{
            //    cbTiltAxis.Enabled = true;
            //    cbRaxis.Enabled = true;
            //}
        }

        private void cbRaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRaxis.Checked)
            {
                cbTiltAxis.Checked = false;
                cbZaxis.Checked = false;
            }
            //else
            //{
            //    cbTiltAxis.Enabled = true;
            //    cbZaxis.Enabled = true;
            //}
        }

        private void cbTiltAxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTiltAxis.Checked)
            {
                cbRaxis.Checked = false;
                cbZaxis.Checked = false;
            }
            //else
            //{
            //    cbRaxis.Enabled = true;
            //    cbZaxis.Enabled = true;
            //}
        }

        private void btnGobackOrg_Click(object sender, EventArgs e)
        {
            MotorMoveHome6D();
            MotorSetPivot(0, 0, 0);
            string hexPosFile = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\HexPos.csv";
            double[] HexCurPos = MotorCurPosHexapod();
            string strHexCurPos = $"After Home,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            File.AppendAllText(hexPosFile, strHexCurPos);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
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


            tbVMEstTime.Text = (mVMPts6d.Length).ToString() + " points.";
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
            tpList[17] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            tpList[18] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            tpList[19] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
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


            tbVMEstTime.Text = (mVMPts6d.Length).ToString() + " points.";
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


            tbVMEstTime.Text = (mVMPts6d.Length).ToString() + " points.";
        }

        private void button14_Click(object sender, EventArgs e)
        {
            mbStopVolumetricMeasure = false;
            Task.Run(() => VolumetricMeasure());

        }

        public bool mbStopVolumetricMeasure = false;

        public Point3d[] mHexapodPivots = new Point3d[3];
        public Point2d mHexapodHplane = new Point2d();  //  TX by degree, TY by degree

        public void InitializeHexpodPivot()
        {
            mHexapodPivots[0] = new Point3d(0, 1.439438857, 14.10421867); //  TX PIVOT
            mHexapodPivots[1] = new Point3d(-0.019249611, 0, 14.03564737); //  TY PIVOT
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

        public void VolumetricMeasure()
        {
            mAutoCalibrationIndex = 0;
            //MotorSpeedSet(1, 1, 1, 1, 1, 1);

            //int pi = 0;
            int fullCnt = 0;

            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport();

            mCalibrationFullData.Clear();

            LoadScaleNTheta();
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);
            if (LoadOQCcondition())
            {
                //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);  //  Move TX (arcmin)
            }
            else
                MotorMoveHome6D();

            double[] orgPos = MotorCurPos6D();


            int i = 0;
            int imax = mVMPts6d.Length;
            bool IsSave = true;
            bool degbug = false;

            double[] curPos = null;
            double Ycommand = 0;
            double YcommandOld = 0;
            YcommandOld = mVMPts6d[0].Y + orgPos[1];

            ////MotorMoveAbs6D( mVMPts6d[0].X  + mCSHorg.X - 200,
            ////                mVMPts6d[0].Y  + mCSHorg.Y,
            ////                mVMPts6d[0].Z  + mCSHorg.Z,
            ////                mVMPts6d[0].TX + mPorg.TX,
            ////                mVMPts6d[0].TY + mPorg.TY,
            ////                mVMPts6d[0].TZ + mPorg.TZ);
            //HexapodRotate(mVMPts6d[0].TX + mPorg.TX,
            //                mVMPts6d[0].TY + mPorg.TY,
            //                mVMPts6d[0].TZ + mPorg.TZ);

            //MotorMoveAbsAxis(Axis.X, mVMPts6d[0].X + mCSHorg.X - 300);   // 이동
            //Thread.Sleep(800);  //  동작 완료 후 측정 확보위해 600msec 필수
            //MotorMoveAbsAxis(Axis.X, mVMPts6d[0].X + mCSHorg.X - 10);   // 이동
            //Thread.Sleep(300);  //  동작 완료 후 측정 확보위해 600msec 필수

            AddVsnLog("Test Points : " + imax.ToString());
            int measuredPointCount = 0;
            string VMfileName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\VM" + fullCnt.ToString() + ".csv";
            StreamWriter wr = new StreamWriter("C:\\CSHTest\\DoNotTouch\\Admin\\X_Vrfy.csv");
            wr.Close();

            int LastPivot = 0;
            //FindPivot(1);

            FindPivot(1);
            FindPivot(2);
            FindPivot(3);
            SavePivots();

            FindCSHorg();
            FindFidorg();

            SaveOQCCondition();



            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            mCalibrationFullData.Clear();
            mGageFullData.Clear();
            double[][] lTXdata = null;
            double[][] lTYdata = null;

            bool bPivotChanged = false;

            while (true)
            {
                if (mbStopVolumetricMeasure)
                    break;
                Ycommand = mVMPts6d[i].Y + orgPos[1];

                ////MotorMoveAbs6D( mVMPts6d[i].X  + mCSHorg.X,
                ////                mVMPts6d[i].Y  + mCSHorg.Y,
                ////                mVMPts6d[i].Z  + mCSHorg.Z,
                ////                mVMPts6d[i].TX + mPorg.TX,
                ////                mVMPts6d[i].TY + mPorg.TY,
                ////                mVMPts6d[i].TZ + mPorg.TZ);

                ////MotorMoveAbsAxis(Axis.X, mVMPts6d[i].X + mCSHorg.X);   // 이동

                MotorXYZ(mVMPts6d[i].X + mCSHorg.X,
                         mVMPts6d[i].Y + mCSHorg.Y,
                         mVMPts6d[i].Z + mCSHorg.Z);

                //HexapodRotate(mVMPts6d[i].TX /*+ mPorg.TX,
                //                mVMPts6d[i].TY /*+ mPorg.TY*/,
                //                mVMPts6d[i].TZ /*+ mPorg.TZ*/);

                double[] HexCurPos;

                if (mVMPts6d[i].TX == 0 && mVMPts6d[i].TY == 0 && mVMPts6d[i].TZ == 0)
                {
                    //  (TX, TY, TZ) == (0, 0,0) 이면 Calibration 때와 같은 조건이 되도록 만든다.
                    HexapodRotate(0, 0, 0);
                    MotorSetPivot(0, 0, 0);
                    //HexapodRotate(mPorg.TX, mPorg.TY, mPorg.TZ);
                    bPivotChanged = true;
                    string hexPosFile = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\HexPos.csv";
                    HexCurPos = MotorCurPosHexapod();
                    File.AppendAllText(hexPosFile, i + ","
                                     + mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + ","
                                     + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + ","
                                     + HexCurPos[0].ToString("F3") + "," + HexCurPos[1].ToString("F3") + "," + HexCurPos[2].ToString("F3") + ","
                                     + HexCurPos[3].ToString("F3") + "," + HexCurPos[4].ToString("F3") + "," + HexCurPos[5].ToString("F3") + "," + "\r\n");

                    if (Math.Abs(HexCurPos[0]) > 1 || Math.Abs(HexCurPos[1]) > 1 || Math.Abs(HexCurPos[2]) > 1)
                    {
                        int changed = 0;
                    }
                }
                else
                {
                    if (bPivotChanged == true && LastPivot > 0)
                    {
                        HexapodRotate(0, 0, 0);
                        MotorSetPivot(mHexapodPivots[LastPivot - 1].X, mHexapodPivots[LastPivot - 1].Y, mHexapodPivots[LastPivot - 1].Z);
                        bPivotChanged = false;
                    }
                    HexapodRotate(mVMPts6d[i].TX /*+ mPorg.TX*/,
                                    mVMPts6d[i].TY /*+ mPorg.TY*/,
                                    mVMPts6d[i].TZ /*+ mPorg.TZ*/);

                }

                Thread.Sleep(400);  //  동작 완료 후 측정 확보위해 600msec 필수
                //if (YcommandOld != Ycommand)
                //    Thread.Sleep(400);  //  동작 완료 후 측정 확보위해 600msec 필수
                YcommandOld = Ycommand;

                IsSave = mVMPts6d[i].pivotAxis == 0 ? true : false;

                if (mVMPts6d[i].pivotAxis == -8)
                {
                    lTXdata = mCalibrationFullData.ToArray();
                    FindPivot(2);
                    mCalibrationFullData.Clear();
                    mGageFullData.Clear();
                }
                else if (mVMPts6d[i].pivotAxis == -7)
                {
                    lTYdata = mCalibrationFullData.ToArray();
                    FindPivot(3);
                    mCalibrationFullData.Clear();
                    mGageFullData.Clear();
                }

                if (mVMPts6d[i].pivotAxis > -10)
                    SingleFindMark(IsSave);

                //if (bPivotChanged)
                //{
                //    HexapodRotate(0, 0, 0);
                //    if (LastPivot<4 && LastPivot>0)
                //        MotorSetPivot(mHexapodPivots[LastPivot-1].X, mHexapodPivots[LastPivot-1].Y, mHexapodPivots[LastPivot-1].Z);
                //}
                HexCurPos = MotorCurPosHexapod();

                //  다음은 Hexapod 에서만 유효
                if (!IsSave)
                {
                    switch (mVMPts6d[i].pivotAxis)
                    {
                        case 1: //  TX pivot
                            MotorMoveOriginHexapod();
                            Thread.Sleep(200);
                            MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);
                            LastPivot = 1;
                            break;
                        case 2: //  TY pivot
                            MotorMoveOriginHexapod();
                            Thread.Sleep(200);
                            MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
                            LastPivot = 2;
                            break;
                        case 3: //  TZ pivot
                            MotorMoveOriginHexapod();
                            Thread.Sleep(200);
                            MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
                            LastPivot = 3;
                            break;
                        default:
                            //MotorMoveOriginHexapod();
                            //Thread.Sleep(200);
                            //MotorSetPivot(0, 0, 0);
                            break;

                    }
                }

                //Thread.Sleep(10);
                if (IsSave)
                    fullCnt++;

                if (measuredPointCount < mCalibrationFullData.Count)
                {
                    string llstr = "";
                    for (int j = 0; j < 22; j++)
                    {
                        //if (j > 5 && j < 16)
                        //    continue;

                        if (j < 19)
                            llstr += mCalibrationFullData[measuredPointCount][j].ToString("F5") + ",";
                        else
                        {
                            //slstr += stablizedData[i][j]).ToString("F5") + ",";  //  stablizedData[i][19 ~ 21] Probe TX, TY, TZ 가 arc min 으로 제공될 때
                            llstr += mCalibrationFullData[measuredPointCount][j].ToString("F5") + ",";  //  stablizedData[i][19 ~ 21] Probe TX, TY, TZ 가 radian 으로 제공될 때
                        }
                    }
                    File.AppendAllText(VMfileName, llstr + HexCurPos[0].ToString("F3") + "," + HexCurPos[1].ToString("F3") + "," + HexCurPos[2].ToString("F3") + ","
                                                         + HexCurPos[3].ToString("F3") + "," + HexCurPos[4].ToString("F3") + "," + HexCurPos[5].ToString("F3") + "," + "\r\n");
                    measuredPointCount = mCalibrationFullData.Count;
                }

                i++;
                if (i >= imax)
                    break;

            }
            if (lTYdata != null)
                mCalibrationFullData.InsertRange(0, lTYdata);
            if (lTXdata != null)
                mCalibrationFullData.InsertRange(0, lTXdata);
            string fileName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            double[][] stablizedData = mCalibrationFullData.ToArray();  //  um, min
            double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

            StreamWriter lwr = null;
            DateTime lnow = DateTime.Now;
            try
            {
                lwr = new StreamWriter(fileName + "VolumetrixMeasure" + fullCnt.ToString() + ".csv");
            }
            catch (Exception e)
            {
                lwr = new StreamWriter(fileName + "VolumetrixMeasure" + fullCnt.ToString() + "_" + lnow.ToString("hhmmss") + ".csv");
            }
            lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ");
            for (i = 0; i < fullCnt; i++)
            {
                string slstr = i.ToString() + ",";
                for (int j = 0; j < 22; j++)
                {
                    //if (j > 5 && j < 16)
                    //    continue;
                    if (j < 16)
                        slstr += stablizedData[i][j].ToString("F5") + ",";  //  stablizedData[i][19 ~ 21] Probe TX, TY, TZ 가 radian 으로 제공될 때
                    else
                        slstr += (stablizedData[i][j] - mCSHorgProbe6D[j - 16]).ToString("F5") + ",";  //  stablizedData[i][19 ~ 21] Probe TX, TY, TZ 가 radian 으로 제공될 때

                }
                slstr += (stablizedData[i][0] - stablizedData[i][16]).ToString("F5") + "," +
                         (stablizedData[i][1] - stablizedData[i][17]).ToString("F5") + "," +
                         (stablizedData[i][2] - stablizedData[i][18]).ToString("F5") + "," +
                         (stablizedData[i][3] - stablizedData[i][19]).ToString("F5") + "," +
                         (stablizedData[i][4] - stablizedData[i][20]).ToString("F5") + "," +
                         (stablizedData[i][5] - stablizedData[i][21]).ToString("F5");
                lwr.WriteLine(slstr);
            }
            lwr.Close();

            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

        }

        public void ClearHexapodPivots()
        {
            mHexapodPivots[0] = new Point3d(0, 0, -14000);
            mHexapodPivots[1] = new Point3d(0, 0, -14000);
            mHexapodPivots[2] = new Point3d(0, 0, -14000);
        }
        public Point3d FindPivot(int axis)
        {
            mCalibrationFullData.Clear();
            mHexapodPivots[axis - 1] = new Point3d(0, 0, 0);

            // 원점으로 이동
            //MotorMoveHome6D();

            // 헥사포드 Origin 위치로 이동
            SingleFindMark(true);
            double[] orgPos = MotorCurPos6D();
            MotorMoveOriginHexapod();
            Thread.Sleep(500);
            orgPos = MotorCurPos6D();

            SingleFindMark(true);
            MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]); //  -244.1   // -30
            Thread.Sleep(500);
            SingleFindMark(true);

            double[] curPos = null;
            Point3d lPivot = new Point3d();

            double[] X0 = new double[2];
            double[] X1 = new double[2];
            double[,] RX = new double[2, 2];
            double[] RX0_X1 = new double[2];
            double[] resPivot = new double[2];
            int itrCnt = 0;
            double dx = 0;
            double dy = 0;
            double dz = 0;
            //mbFigorgLoaded = false;
            switch (axis) //  X axis
            {
                case 1:
                    double[] tagetPos;
                    double angle; // deg

                    tagetPos = new double[5] { -205, -200, 0, 160, 0 };   //  min   //  Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.
                    angle = tagetPos[3] - tagetPos[1];
                    mHexapodPivots[0].Z = -14000;
                    MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);   // Pivot (0,0,-14000)에서 시작

                    while (itrCnt++ < 10)
                    {
                        mCalibrationFullData.Clear();
                        mGageFullData.Clear();
                        lPivot = new Point3d();
                        for (int i = 0; i < 5; i++)
                        {
                            MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]); //  -244.1   // -30
                            if (i < 4)
                            {
                                if (i == 1 || i == 2 || i == 3)
                                    Thread.Sleep(600);
                                SingleFindMark(true);
                            }
                        }
                        ///////////////////////////////////////////////////////////////////
                        dy = mCalibrationFullData[3][1] - mCalibrationFullData[1][1];
                        dz = mCalibrationFullData[3][2] - mCalibrationFullData[1][2];
                        angle = mCalibrationFullData[3][19] - mCalibrationFullData[1][19];  //  min
                        if (Math.Abs(dy) < 0.1 && Math.Abs(dz) < 0.1)
                            break;

                        // Pivto XA 계산 
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = mCalibrationFullData[1][1]; //  Y pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um
                        X1[0] = mCalibrationFullData[3][1]; //  Y pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um


                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.Y = mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        lPivot.Z = mHexapodPivots[0].Z + (resPivot[1] - X0[1]);
                        ///////////////////////////////////////////////////////////////////
                        dy = mCalibrationFullData[2][1] - mCalibrationFullData[1][1];
                        dz = mCalibrationFullData[2][2] - mCalibrationFullData[1][2];
                        angle = mCalibrationFullData[2][19] - mCalibrationFullData[1][19];  //  min

                        // Pivto XA 계산 
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = mCalibrationFullData[1][1]; //  Y pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um
                        X1[0] = mCalibrationFullData[2][1]; //  Y pos um
                        X1[1] = -mCalibrationFullData[2][2]; //  Z pos um


                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.Y += mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        lPivot.Z += mHexapodPivots[0].Z + (resPivot[1] - X0[1]);
                        ///////////////////////////////////////////////////////////////////
                        dy = mCalibrationFullData[3][1] - mCalibrationFullData[2][1];
                        dz = mCalibrationFullData[3][2] - mCalibrationFullData[2][2];
                        angle = mCalibrationFullData[3][19] - mCalibrationFullData[2][19];  //  min

                        // Pivto XA 계산 
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = mCalibrationFullData[2][1]; //  Y pos um
                        X0[1] = -mCalibrationFullData[2][2]; //  Z pos um
                        X1[0] = mCalibrationFullData[3][1]; //  Y pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um


                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.Y += mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        lPivot.Z += mHexapodPivots[0].Z + (resPivot[1] - X0[1]);

                        mHexapodPivots[0] = new Point3d(orgPos[0], lPivot.Y / 3, lPivot.Z / 3);

                        ///////////////////////////////////////////////////////////////////
                        // 피봇 변경 후 채즉청
                        MotorMoveOriginHexapod();
                        Thread.Sleep(200);
                        MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);
                        //HexCurPos = MotorCurPosHexapod();
                        //strHexCurPos = $"After Motor Set Pivot X,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
                        //File.AppendAllText(hexPosFile, strHexCurPos);
                        Thread.Sleep(10);
                        //  출발 피봇이 가까와지니 오차가 줄어들기는 함.
                        //  1회차에서 
                        //  Y 는 7.6% 과보정
                        //  Z 는 8.1% 미보정
                    }
                    break;
                case 2:

                    tagetPos = new double[5] { -235, -230, 0, 230, 0 };   //  min
                    angle = tagetPos[3] - tagetPos[1];
                    mHexapodPivots[1].Z = -14000;
                    MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);

                    while (itrCnt++ < 10)
                    {
                        mCalibrationFullData.Clear();
                        mGageFullData.Clear();
                        lPivot = new Point3d();
                        for (int i = 0; i < 5; i++)
                        {
                            MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]);
                            if (i < 4)
                            {
                                if (i == 1 || i == 2 || i == 3)
                                    Thread.Sleep(600);
                                SingleFindMark(true);
                            }
                        }
                        ///////////////////////////////////////////////////////////////////
                        dx = mCalibrationFullData[3][0] - mCalibrationFullData[1][0];
                        dz = mCalibrationFullData[3][2] - mCalibrationFullData[1][2];
                        angle = mCalibrationFullData[3][20] - mCalibrationFullData[1][20];  //  min
                        if (Math.Abs(dx) < 0.1 && Math.Abs(dz) < 0.1)
                            break;

                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[1][0]; //  X pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X = mHexapodPivots[1].X + (resPivot[0] - X0[0]);
                        lPivot.Z = mHexapodPivots[1].Z + (resPivot[1] - X0[1]);
                        ///////////////////////////////////////////////////////////////////
                        dx = mCalibrationFullData[2][0] - mCalibrationFullData[1][0];
                        dz = mCalibrationFullData[2][2] - mCalibrationFullData[1][2];
                        angle = mCalibrationFullData[2][20] - mCalibrationFullData[1][20];  //  min

                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[1][0]; //  X pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um

                        X1[0] = -mCalibrationFullData[2][0]; //  X pos um
                        X1[1] = -mCalibrationFullData[2][2]; //  Z pos um
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X += mHexapodPivots[1].X + (resPivot[0] - X0[0]);
                        lPivot.Z += mHexapodPivots[1].Z + (resPivot[1] - X0[1]);
                        ///////////////////////////////////////////////////////////////////
                        dx = mCalibrationFullData[3][0] - mCalibrationFullData[2][0];
                        dz = mCalibrationFullData[3][2] - mCalibrationFullData[2][2];
                        angle = mCalibrationFullData[3][20] - mCalibrationFullData[2][20];  //  min

                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[2][0]; //  X pos um
                        X0[1] = -mCalibrationFullData[2][2]; //  Z pos um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X += mHexapodPivots[1].X + (resPivot[0] - X0[0]);
                        lPivot.Z += mHexapodPivots[1].Z + (resPivot[1] - X0[1]);

                        mHexapodPivots[1] = new Point3d(lPivot.X / 3, orgPos[1], lPivot.Z / 3);

                        ///////////////////////////////////////////////////////////////////
                        // 피봇 변경 후 채즉청
                        MotorMoveOriginHexapod();
                        Thread.Sleep(200);
                        MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
                        Thread.Sleep(10);
                    }
                    break;
                case 3:

                    tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min
                    angle = tagetPos[3] - tagetPos[1];
                    mHexapodPivots[2].Z = -14000;
                    MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);

                    while (itrCnt++ < 10)
                    {
                        mCalibrationFullData.Clear();
                        mGageFullData.Clear();
                        lPivot = new Point3d();
                        for (int i = 0; i < 5; i++)
                        {
                            MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5] + tagetPos[i]);
                            if (i < 4)
                            {
                                if (i == 1 || i == 2 || i == 3)
                                    Thread.Sleep(600);
                                SingleFindMark(true);
                            }
                        }
                        ///////////////////////////////////////////////////////////////////
                        dx = mCalibrationFullData[3][0] - mCalibrationFullData[1][0];
                        dy = mCalibrationFullData[3][1] - mCalibrationFullData[1][1];
                        angle = mCalibrationFullData[3][21] - mCalibrationFullData[1][21];  //  min
                        if (Math.Abs(dx) < 0.1 && Math.Abs(dy) < 0.1)
                            break;

                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[1][0]; //  X pos   um
                        X0[1] = mCalibrationFullData[1][1]; //  Y pos   um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos   um
                        X1[1] = mCalibrationFullData[3][1]; //  Y pos   um
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X = mHexapodPivots[2].X + (resPivot[0] - X0[0]); //  dy 에 영향 준다.
                        lPivot.Y = mHexapodPivots[2].Y - (resPivot[1] - X0[1]); //  dx 에 영향 준다.
                        ///////////////////////////////////////////////////////////////////
                        dx = mCalibrationFullData[2][0] - mCalibrationFullData[1][0];
                        dy = mCalibrationFullData[2][1] - mCalibrationFullData[1][1];
                        angle = mCalibrationFullData[2][21] - mCalibrationFullData[1][21];  //  min

                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[1][0]; //  X pos   um
                        X0[1] = mCalibrationFullData[1][1]; //  Y pos   um

                        X1[0] = -mCalibrationFullData[2][0]; //  X pos   um
                        X1[1] = mCalibrationFullData[2][1]; //  Y pos   um
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X += mHexapodPivots[2].X + (resPivot[0] - X0[0]); //  dy 에 영향 준다.
                        lPivot.Y += mHexapodPivots[2].Y - (resPivot[1] - X0[1]); //  dx 에 영향 준다.
                        ///////////////////////////////////////////////////////////////////
                        dx = mCalibrationFullData[3][0] - mCalibrationFullData[2][0];
                        dy = mCalibrationFullData[3][1] - mCalibrationFullData[2][1];
                        angle = mCalibrationFullData[3][21] - mCalibrationFullData[2][21];  //  min

                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[2][0]; //  X pos   um
                        X0[1] = mCalibrationFullData[2][1]; //  Y pos   um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos   um
                        X1[1] = mCalibrationFullData[3][1]; //  Y pos   um
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X += mHexapodPivots[2].X + (resPivot[0] - X0[0]); //  dy 에 영향 준다.
                        lPivot.Y += mHexapodPivots[2].Y - (resPivot[1] - X0[1]); //  dx 에 영향 준다.

                        mHexapodPivots[2] = new Point3d(lPivot.X / 3, lPivot.Y / 3, orgPos[2]);

                        ///////////////////////////////////////////////////////////////////
                        // 피봇 변경 후 채즉청
                        MotorMoveOriginHexapod();
                        Thread.Sleep(200);
                        MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
                        Thread.Sleep(10);
                    }
                    break;
                default:
                    break;
            }
            lPivot = mHexapodPivots[axis - 1];

            // 피봇 초기화
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);

            return lPivot;
        }

        public Point3d PivotTest(int axis)
        {
            mCalibrationFullData.Clear();
            MotorSetPivot(0, 1, 13.5);
            mHexapodPivots[axis - 1] = new Point3d(0, 1, 13.5);
            mHexapodPivots[axis - 1] = new Point3d(0, 0, 0);

            // 원점으로 이동
            //MotorMoveHome6D();
            Thread.Sleep(500);
            double[] orgPos = MotorCurPos6D();
            double[] curPos = null;
            Point3d lPivot = new Point3d();

            double[] X0 = new double[2];
            double[] X1 = new double[2];
            double[,] RX = new double[2, 2];
            double[] RX0_X1 = new double[2];
            double[] resPivot = new double[2];

            switch (axis) //  X axis
            {
                case 1:
                    for (int i = 0; i < 4; i++)
                    {
                        // 원점에서 -1도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], -1, 0, 0);
                        Thread.Sleep(400);

                        // 원점에서 0도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        // 측정,
                        SingleFindMark(true);

                        // 원점에서 4도
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 4, 0, 0);
                        Thread.Sleep(400);
                        // 측정
                        SingleFindMark(true);

                        // Pivto XA 계산 
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = mCalibrationFullData[0][1] / 1000; //  Y pos mm
                        X0[1] = -mCalibrationFullData[0][2] / 1000; //  Z pos mm
                        X1[0] = mCalibrationFullData[1][1] / 1000; //  Y pos mm
                        X1[1] = -mCalibrationFullData[1][2] / 1000; //  Z pos mm


                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(4 / 180.0 * Math.PI, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.Y = mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        lPivot.Z = mHexapodPivots[0].Z + (resPivot[1] - X0[1]);

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        mHexapodPivots[0] = new Point3d(orgPos[0], lPivot.Y, lPivot.Z);
                        ///////////////////////////////////////////////////////////////////
                        // 피봇 변경 후 채즉청
                        curPos = MotorCurPos6D();
                        MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);
                        Thread.Sleep(400);

                        // 원점에서 -1도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], -1, 0, 0);
                        Thread.Sleep(400);

                        // 원점에서 0도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        // 측정,
                        SingleFindMark(true);
                        // 원점에서 4도
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 4, 0, 0);
                        Thread.Sleep(400);
                        // 측정
                        SingleFindMark(true);

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);
                        orgPos = MotorCurPos6D();
                        if (Math.Abs(mCalibrationFullData[2][1] - mCalibrationFullData[3][1]) < 0.3 && Math.Abs(mCalibrationFullData[2][2] - mCalibrationFullData[3][2]) < 0.3)
                            break;
                        mCalibrationFullData.Clear();
                    }

                    //  출발 피봇이 가까와지니 오차가 줄어들기는 함.
                    //  1회차에서 
                    //  Y 는 7.6% 과보정
                    //  Z 는 8.1% 미보정

                    break;
                case 2:
                    for (int i = 0; i < 4; i++)
                    {
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, -1, 0);
                        Thread.Sleep(400);

                        // 원점에서 0도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        // 측정,
                        SingleFindMark(true);
                        // 원점에서 4도
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 4, 0);
                        Thread.Sleep(400);

                        // 측정
                        SingleFindMark(true);
                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[0][0] / 1000; //  X pos
                        X0[1] = -mCalibrationFullData[0][2] / 1000; //  Z pos
                        X1[0] = -mCalibrationFullData[1][0] / 1000; //  X pos
                        X1[1] = -mCalibrationFullData[1][2] / 1000; //  Z pos
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(4 / 180.0 * Math.PI, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X = mHexapodPivots[1].X + resPivot[0] - X0[0];
                        lPivot.Z = mHexapodPivots[1].Z - (resPivot[1] - X0[1]);

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        mHexapodPivots[1] = new Point3d(lPivot.X, orgPos[1], lPivot.Z);
                        ///////////////////////////////////////////////////////////////////
                        // 피봇 변경 후 채즉청
                        curPos = MotorCurPos6D();
                        MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
                        Thread.Sleep(400);

                        // 원점에서 -1도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, -1, 0);
                        Thread.Sleep(400);

                        // 원점에서 0도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        // 측정,
                        SingleFindMark(true);
                        // 원점에서 4도
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 4, 0);
                        Thread.Sleep(400);
                        // 측정
                        SingleFindMark(true);

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);
                        orgPos = MotorCurPos6D();
                        if (Math.Abs(mCalibrationFullData[2][0] - mCalibrationFullData[3][0]) < 0.3 && Math.Abs(mCalibrationFullData[2][2] - mCalibrationFullData[3][2]) < 0.3)
                            break;
                        mCalibrationFullData.Clear();
                    }
                    break;
                case 3:
                    // 원점에서 -1도 이동
                    for (int i = 0; i < 4; i++)
                    {
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, -1);
                        Thread.Sleep(400);

                        // 원점에서 0도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        // 측정,
                        SingleFindMark(true);
                        // 원점에서 4도
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 4);
                        Thread.Sleep(400);

                        // 측정
                        SingleFindMark(true);
                        // XA 계산 및 출력
                        X0 = new double[2];
                        X1 = new double[2];
                        X0[0] = -mCalibrationFullData[0][0] / 1000; //  X pos
                        X0[1] = mCalibrationFullData[0][1] / 1000; //  Y pos
                        X1[0] = -mCalibrationFullData[1][0] / 1000; //  X pos
                        X1[1] = mCalibrationFullData[1][1] / 1000; //  Y pos
                        RX = new double[2, 2];
                        m__G.mFAL.mFZM.RotationZ2x2(4 / 180.0 * Math.PI, ref RX);
                        RX0_X1 = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);
                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;
                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        resPivot = new double[2];
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);
                        lPivot.X = mHexapodPivots[2].X + (resPivot[0] - X0[0]);
                        lPivot.Y = mHexapodPivots[2].Y + (resPivot[1] - X0[1]);

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        mHexapodPivots[2] = new Point3d(lPivot.X, lPivot.Y, orgPos[2]);
                        ///////////////////////////////////////////////////////////////////
                        // 피봇 변경 후 채즉청
                        curPos = MotorCurPos6D();
                        MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
                        Thread.Sleep(400);

                        // 원점에서 -1도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, -1);
                        Thread.Sleep(400);

                        // 원점에서 0도 이동
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);

                        // 측정,
                        SingleFindMark(true);
                        // 원점에서 4도
                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 4);
                        Thread.Sleep(400);
                        // 측정
                        SingleFindMark(true);

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], 0, 0, 0);
                        Thread.Sleep(400);
                        orgPos = MotorCurPos6D();
                        if (Math.Abs(mCalibrationFullData[2][0] - mCalibrationFullData[3][0]) < 0.3 && Math.Abs(mCalibrationFullData[2][1] - mCalibrationFullData[3][1]) < 0.3)
                            break;
                        mCalibrationFullData.Clear();
                    }

                    break;
                default:
                    break;
            }
            return lPivot;
        }


        private void button15_Click(object sender, EventArgs e)
        {
            mbStopVolumetricMeasure = true;
        }

        public VolumetricTP[] Generate3dTrajectory(double pitchX, double pitchY, double pitchZ, double xFullRange, double yFullRange, double zFullRange)
        {
            //  xFullRange ~ yFullRange 이고 pitchX ~ pitchY 인 경우
            //  회오리 궤적

            List<VolumetricTP> pList = new List<VolumetricTP>();
            int m = (int)((zFullRange / 2) / pitchZ);
            int nxMax = (int)((xFullRange / 2) / pitchX);
            int nyMax = (int)((yFullRange / 2) / pitchY);

            double residueX = xFullRange - 2 * nxMax * pitchX;
            int yinterval = (int)(yFullRange / pitchY + 0.01);

            for (int j = -m; j <= m; j++)
            {
                VolumetricTP p0 = new VolumetricTP(0, 0, j * pitchZ);
                pList.Add(p0);
                if (mAutoFullRange > 0)
                {
                    pitchY = 2 * (1900 - (Math.Abs(p0.Pt.Z / 100) - 240) / 0.8391 - 120) / yinterval;
                }

                for (int k = 1; k <= nxMax; k++)
                {
                    int nx = k;
                    int ny = k;

                    for (int i = 0; i < ny; i++)
                    {
                        VolumetricTP p = new VolumetricTP(nx * pitchX, i * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = nx; i > -nx; i--)
                    {
                        VolumetricTP p = new VolumetricTP(i * pitchX, ny * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = ny; i > -ny; i--)
                    {
                        VolumetricTP p = new VolumetricTP(-nx * pitchX, i * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = -nx; i < nx; i++)
                    {
                        VolumetricTP p = new VolumetricTP(i * pitchX, -ny * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = -ny; i < 0; i++)
                    {
                        VolumetricTP p = new VolumetricTP(nx * pitchX, i * pitchY, j * pitchZ);
                        pList.Add(p);

                    }
                }
                VolumetricTP pOld = pList[pList.Count - 1];
                VolumetricTP p1 = new VolumetricTP(0.75 * pOld.Pt.X, 0.75 * pOld.Pt.Y, 0.75 * pOld.Pt.Z + pitchZ / 4, false);
                VolumetricTP p2 = new VolumetricTP(0.50 * pOld.Pt.X, 0.50 * pOld.Pt.Y, 0.50 * pOld.Pt.Z + pitchZ / 2, false);
                VolumetricTP p3 = new VolumetricTP(0.25 * pOld.Pt.X, 0.25 * pOld.Pt.Y, 0.25 * pOld.Pt.Z + 0.75 * pitchZ, false);
                pList.Add(p1);
                pList.Add(p2);
                pList.Add(p3);
            }

            //double[] x = new double[pList.Count];
            //double[] y = new double[pList.Count];
            //double[] z = new double[pList.Count];
            //for (int i = 0; i < pList.Count; i++)
            //{
            //    x[i] = pList[i].X;
            //    y[i] = pList[i].Y;
            //    z[i] = pList[i].Z;
            //}
            return pList.ToArray();
        }

        private void btnMoveTo_Click(object sender, EventArgs e)
        {
            double x = double.Parse(tbXrange.Text);
            double y = double.Parse(tbYrange.Text);
            double z = double.Parse(tbZrange.Text);
            double tx = double.Parse(tbTXrange.Text);
            double ty = double.Parse(tbTYrange.Text);
            double tz = double.Parse(tbTZrange.Text);
            MotorSetHCS(0, 0, 0);
            if (rbFromOrg.Checked)
                MotorMoveHome6D();
            else
            {
                if (LoadOQCcondition())
                {
                    //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
                    MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
                    HexapodRotate(mPorg.TX, mPorg.TY, mPorg.TZ);    // 피봇 0으로 하고 Porg로 회전시켜야하는데 빠져있음.
                }
                else
                {
                    MessageBox.Show("OQCcondition File could not be loaded.");
                }

            }

            Thread.Sleep(100);
            double[] orgPos = MotorCurPos6D();


            MotorXYZ(x + orgPos[0], y + orgPos[1], z + orgPos[2]);
            HexapodRotate(tx + orgPos[3], ty + orgPos[4], tz + orgPos[5]);

            //MotorMoveAbs6D(x + orgPos[0],
            //               y + orgPos[1],
            //               z + orgPos[2],
            //            tx + orgPos[3],
            //            ty + orgPos[4],
            //            tz + orgPos[5]);
        }

        private void rbCalTX_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "160"; //210
        }

        private void rbCalTY_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "200";//"240";
        }

        private void rbCalTZ_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "200";//240
        }

        private void rbCalZ_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1700";  // 1750
        }

        private void rbCalX_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1900";   // 1900
        }

        private void rbCalY_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1900";   // 1900
        }

        private void button16_Click(object sender, EventArgs e)
        {
            mAutoFullRange = 95;
            //ApplyVolumetricMeasureOld();

            ApplyVolumetricMeasure();    // TXTYTZ -> TXTYTZ -> ... 

            // ApplyVolumetricMeasure3step();  //  TX -> TY -> TZ
        }

        //private void rbCalEastView_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (rbCalEastView.Checked)
        //    {
        //        lblYMaxStroke.Visible = true;
        //        lblZMaxStroke.Visible = true;
        //        tbZMaxStroke.Visible = true;
        //        tbMaxStroke.Text = "1900";
        //        tbZMaxStroke.Text = "1700";
        //    }
        //    else
        //    {
        //        lblYMaxStroke.Visible = false;
        //        lblZMaxStroke.Visible = false;
        //        tbZMaxStroke.Visible = false;
        //    }
        //}

        public Point3d mCommonPivot = new Point3d(0, 0, 0);
        private void button17_Click(object sender, EventArgs e)
        {
            Task.Run(() => FindOQCcondition());
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리


            int axis = -1;
            if (rbCalTX.Checked)
            {
                axis = 0;
            }
            else if (rbCalTY.Checked)
            {
                axis = 1;

            }
            else if (rbCalTZ.Checked)
            {
                axis = 2;
            }

            if (axis == -1) { return; }

            string lstr = "No\tX\tY\tZ\r\n";
            for (int i = 0; i < 11; i++)
            {
                var pivot = FindPivot(axis + 1);
                if (i != 0)
                {
                    lstr += $"{i}\t{pivot.X}\t{pivot.Y}\t{pivot.Z}\r\n";
                }
            }
            lstr += "No\tX\tY\tZ\r\n";
            for (int i = 0; i < 11; i++)
            {
                var pivot = FindPivot(axis + 1);
                if (i != 0)
                {
                    lstr += $"{i}\t{pivot.X}\t{pivot.Y}\t{pivot.Z}\r\n";
                }
            }

            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리
            tbCalibration.Text = lstr;
        }

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

        public double[] FindPorg()
        {
            //  Porg.X, Porg.Y, Porg.TX, Porg.TY 를 찾고 저장한다.  --> 이 값들은 Position Command 값이다. 항상 - 에서 + 로 이동하여 정지시켜야 한다.

            // Set Motion Speed Normal
            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);

            double[] lastError = new double[4];
            double X = 0;
            double Xold = 0;
            double Y = 0;
            double Yold = 0;
            double[] curPos = new double[6];
            double[] orgPos = MotorCurPos6D();
            double[] movingTable = new double[10] { 0, -100, -165, -160, -80, 0, 80, 160, 80, 0 };   //  arc minute
            FZMath.Point2D[] lmsData = new FZMath.Point2D[5];

            // double orgTY = orgPos[3];
            double orgZ = orgPos[2];
            int itrCnt = 0;
            mCalibrationFullData.Clear();
            mGageFullData.Clear();
            double a = 0, b = 0;

            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {

                    MotorMoveAbs6D(orgPos[0] + X, orgPos[1], orgPos[2], orgPos[3], orgPos[4] + movingTable[i], orgPos[5]);  //  Move TY (mm, arcmin)
                    Thread.Sleep(400);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double dZN = mGageFullData[7][5] - mGageFullData[3][5]; // TY -160~160 에서 Probe
                double dZP = mGageFullData[7][6] - mGageFullData[3][6];
                X = 1.04 * 60000 * (dZN + dZP) / (dZN - dZP);
                //  Porg 정의 다시 확인 필요 20241213.

                if (Math.Abs(X) < 0.2 || (Math.Abs(dZN + dZP)) < 0.21)   //  um
                    break;
                X = X + Xold;
                Xold = X;
            }
            lastError[0] = X;
            mPorg.X = orgPos[0] + Xold;   //  um
            AddVsnLog("Porg X error " + X.ToString("F3") + " um");

            orgPos = MotorCurPos6D();   // mPorg.X 적용한 현재 위치
            // double orgTX = orgPos[4];
            orgZ = orgPos[2];
            itrCnt = 0;
            mCalibrationFullData.Clear();
            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(orgPos[0], orgPos[1] - Y, orgPos[2], orgPos[3] + movingTable[i], orgPos[4], orgPos[5]);  //  Move TY (mm, arcmin)
                    //MotorMoveAbs6D(X, Y, 0, movingTable[i], 0, 0);  //  Move TX (mm, arcmin)
                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double dZ4 = (mGageFullData[7][5] + mGageFullData[7][6]) / 2 - (mGageFullData[3][5] + mGageFullData[3][6]) / 2;
                double dZ2 = (mGageFullData[7][4] - mGageFullData[3][4]);

                Y = 1.04 * 83000 * dZ4 / (dZ2 - dZ4);
                if (Math.Abs(Y) < 0.2 || Math.Abs(dZ4) < 0.11) //    (um)
                    break;
                Y = Y + Yold;
                Yold = Y;
            }
            lastError[1] = Y;
            mPorg.Y = orgPos[1] - Yold;   //  mm
            AddVsnLog("Porg Y error " + Y.ToString("F3") + " um");



            movingTable = new double[10] { 0, -400, -850, -800, -400, 0, 400, 800, 400, 0 };
            double TX = 0;
            double TXold = 0;
            double TY = 0;
            double TYold = 0;


            //////////////////////////////////////////////////////////////////////////
            //  Y 구동, Z 변동 측정, TX 조정
            orgPos = MotorCurPos6D();
            double orgY = orgPos[1];
            orgZ = orgPos[2];
            itrCnt = 0;
            mCalibrationFullData.Clear();
            double ZshiftWhileMoveingY = 0;
            double ZshiftWhileMoveingX = 0;
            double[] ZshiftWhileMoveingYDebug = new double[11];
            double[] ZshiftWhileMoveingXDebug = new double[11];
            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(orgPos[0], orgPos[1] + movingTable[i], orgPos[2], orgPos[3] - TX, orgPos[4], orgPos[5]);  //  Move TY (mm, arcmin)
                    //MotorMoveAbs6D(0, movingTable[i], 0, TX, 0, 0);  //  Move Y at the TX (mm, arcmin)
                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double[][] stablizedData = mCalibrationFullData.ToArray();
                for (int i = 0; i < 5; i++)
                {
                    // orgY, orgZ는 오프셋이라 기울기 구할때 의미없음 -> 그리고 모터 명령값이 아니라 gage 데이터를 넣어야함. -> 의미없으므로 그대로 둠.
                    // stablizedData가 아닌 mGageFullData를 사용해서 LMS 구해야함. 수정완료.
                    lmsData[i] = new FZMath.Point2D(mGageFullData[i + 3][1] - orgY, (mGageFullData[i + 3][5] + mGageFullData[i + 3][6]) / 2 - orgZ);    //  TY(min) measured, Z(um) measured // -800~800um
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(lmsData, 5, ref a, ref b);
                ZshiftWhileMoveingY = (mGageFullData[7][5] + mGageFullData[7][6]) / 2 - (mGageFullData[3][5] + mGageFullData[3][6]) / 2;
                ZshiftWhileMoveingYDebug[itrCnt] = ZshiftWhileMoveingY;
                TX = Math.Atan(a) * RAD_To_MIN;    //  rad -> min
                AddVsnLog("TX " + itrCnt.ToString() + "th : " + TX.ToString("F3"));
                if (Math.Abs(TX) < 0.2)
                    break;
                TX = TX + TXold;
                TXold = TX;
            }
            lastError[2] = TX;
            mPorg.TX = orgPos[3] - TXold;//  acr min
            AddVsnLog("Porg TX error " + TX.ToString("F3") + " min. Zshift while moving Y = " + ZshiftWhileMoveingY.ToString("F2"));


            //////////////////////////////////////////////////////////////////////////
            //  X 구동, Z 변동 측정, TY 조정
            orgPos = MotorCurPos6D();
            double orgX = orgPos[0];
            orgZ = orgPos[2];
            itrCnt = 0;
            mCalibrationFullData.Clear();
            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(orgPos[0] + movingTable[i], orgPos[1], orgPos[2], mPorg.TX, orgPos[4] + TY, orgPos[5]);  //  Move TY (mm, arcmin)
                    //MotorMoveAbs6D(movingTable[i], 0, 0, TX, TY, 0);  //  Move X at the TY (mm, arcmin)
                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double[][] stablizedData = mCalibrationFullData.ToArray();
                for (int i = 0; i < 5; i++)
                {
                    //lmsData[i] = new FZMath.Point2D(stablizedData[i + 3][16] - orgX, stablizedData[i + 3][18] - orgZ);    //  TX(min) measured, Z(um) measured
                    lmsData[i] = new FZMath.Point2D(mGageFullData[i + 3][0] - orgX, (mGageFullData[i + 3][5] + mGageFullData[i + 3][6]) / 2 - orgZ);
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(lmsData, 5, ref a, ref b);
                //ZshiftWhileMoveingX = stablizedData[7][18] - stablizedData[3][18];
                ZshiftWhileMoveingX = (mGageFullData[7][5] + mGageFullData[7][6]) / 2 - (mGageFullData[3][5] + mGageFullData[3][6]) / 2;
                ZshiftWhileMoveingXDebug[itrCnt] = ZshiftWhileMoveingX;
                TY = Math.Atan(a) * RAD_To_MIN;    //  rad -> min
                if (Math.Abs(TY) < 0.2)
                    break;
                TY = TY + TYold;
                TYold = TY;
            }
            lastError[3] = TY;
            mPorg.TY = orgPos[4] + TYold;//  acr min
            AddVsnLog("Porg TY error " + TY.ToString("F3") + " min. Zshift while moving X = " + ZshiftWhileMoveingX.ToString("F2"));
            return lastError;

        }
        public void FindHCSrotationPsi()
        {
            //  Hexapod X, Y 축(회전축으로서의 X 축 및 Y 축) 과 Probe 로 정의된 X 축, Y축이 Z 축에 대해서 회전된 형태일 때 이 회전각도를 측정해서 Hexapod Coordinate System 을 변환한다.
            //  Porg 상태에서는 TX 가 변할 때 pZ1 + pZ3 는 변하지 않는다.
            double TZ = 0;
            double TZold = 0;
            double[] curPos = new double[6];
            double[] orgPos = MotorCurPos6D();
            double[] movingTable = new double[10] { 0, -60, -130, -120, -60, 0, 60, 120, 60, 0 };   //  TX min
            FZMath.Point2D[] lmsData = new FZMath.Point2D[5];

            double orgTY = orgPos[3];
            double orgZ = orgPos[2];
            int itrCnt = 0;
            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            orgPos = MotorCurPos6D();
            double orgTX = orgPos[4];
            orgZ = orgPos[2];
            itrCnt = 0;
            double a = 0, b = 0;

            MotorMoveHome6D();
            Thread.Sleep(600);
            orgPos = MotorCurPos6D();

            while (itrCnt++ < 10)
            {
                if (itrCnt > 0)
                {
                    MotorMoveHome6D();

                    Thread.Sleep(600);
                }
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                MotorSetHCS(0, 0, TZ);

                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(mPorg.X, mPorg.Y, orgPos[2], mPorg.TX + movingTable[i], mPorg.TY, orgPos[5]);  //  Move TX (arcmin)

                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }

                double dpZ1 = mGageFullData[7][5] - mGageFullData[3][5];                //  um
                double dpZ3 = mGageFullData[7][6] - mGageFullData[3][6];                //  um
                double dTX = (mCalibrationFullData[7][19] - mCalibrationFullData[3][19]) * MIN_To_RAD; //  min -> rad

                TZ = Math.Asin((dpZ1 - dpZ3) / (120000 * Math.Tan(dTX))) * RAD_To_MIN;  //  Full Angluar Stroke (min)

                if (Math.Abs(TZ) < 0.3 || Math.Abs(dpZ1 - dpZ3) < 0.11) // 
                    break;
                //MotorSetHCS(0, 0, 0);

                TZ = TZ + TZold;
                TZold = TZ;
            }
            mHCSrotation.Z = TZ;    //  min
        }

        public void FindCSHorg(bool resetProbe = false)
        {
            //  TX, TY, TZ command 로 (Porg.TX, Porg.TY, 0)  를 적용한 상태에서
            //  CSHorg 로서 CSHead 의 X, Y, Z 측정값이 (0,0,0)  이 되는 XYZ stage 의 위치값을 찾아 저장한다.  --> stage command 값 - 따라서 반복편차 있음.
            //  CSHead 로 측정된  TX, TY, TZ 를 저장하고 Set TX, TY, TZ Zero 한다.
            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            //MotorMoveHome6D();

            // 동작전 Hexapod 기본설정
            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);

            //Thread.Sleep(400);
            // MotorSetHCS(mHCSrotation.X, mHCSrotation.Y, mHCSrotation.Z); // 필요없음.


            //double X = mPorg.X; // 처음엔 FindPorg하기 전이라 mPorg.X = 0임.
            //double Y = mPorg.Y;
            //double Xnext = X;
            //double Ynext = Y;
            double X = 0;
            double Y = 0;
            double Z = 0;
            double Xnext = 0;
            double Ynext = 0;
            double Znext = 0;
            double TX = 0;
            double TY = 0;
            double TZ = 0;
            double[] orgPos = MotorCurPos6D();

            int itrCnt = 0;
            while (itrCnt < 10)
            {
                //MotorMoveAbs6D(orgPos[0] + Xnext, orgPos[1] + Ynext, orgPos[2] + Znext, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move (mm , arcmin)
                MotorMoveAbs6D(orgPos[0] + Xnext, orgPos[1] + Ynext, orgPos[2] + Znext, 0, 0, 0);  //  Move (mm , arcmin)
                Thread.Sleep(600);
                SingleFindMark(true);
                X = mCalibrationFullData[itrCnt][0]; //   um 
                Y = mCalibrationFullData[itrCnt][1]; //   um
                Z = mCalibrationFullData[itrCnt][2]; //   um
                TX = mCalibrationFullData[itrCnt][3]; //   min 
                TY = mCalibrationFullData[itrCnt][4]; //   min 
                TZ = mCalibrationFullData[itrCnt][5]; //   min 
                if (Math.Abs(X) < 0.1 && Math.Abs(Y) < 0.1 && Math.Abs(Z) < 0.1)    //  Z  200um 상승된 상태를 기준으로 정한다.
                    break;
                Xnext -= X;
                Ynext -= Y;
                Znext -= Z;
                itrCnt++;
            }
            mCSHorg.X = orgPos[0] + Xnext;
            mCSHorg.Y = orgPos[1] + Ynext;
            mCSHorg.Z = orgPos[2] + Znext;
            // 지금은 헥사포드 0인 상태에서, CSH 에서 측정된 각도. 프로브도 이때 Reset -> SetTXTYZero()로 저장해줘야 하는값
            mCSHorg.TX = TX;    //  Porg 상태에서,  CSH 에서 측정된 각도 -> SetTXTYZero() 으로 저장해줘야 하는 값
            mCSHorg.TY = TY;    //  Porg 상태에서,  CSH 에서 측정된 각도 -> SetTXTYZero() 으로 저장해줘야 하는 값
            mCSHorg.TZ = TZ;    //  Porg 상태에서,  CSH 에서 측정된 각도 -> SetTXTYZero() 으로 저장해줘야 하는 값

            if (resetProbe)
            {
                //  mCSHorgProbe 는 Probe Reset 을 하는 경우 Reset 이후에 측정된 값이 저장되어야 하므로 한번 더 측정한다.
                m__G.mGageCounter.SetAllPortZero();
                SingleFindMark(true);

                // FindFidorg로 이동.
                //  기울기에 따른 Z프로브 보상관련으로 사용하는 것.
                // CSHorg위치로 이동후 FindFidorg를 할때 현재 Probe값을 저장해야함.
                //  FindCSH할때 Probe 리셋의 유무에 관계 없이 이후 FindFidorg 할때 모두 필요.
                //mCSHorgProbe.X = mCalibrationFullData[mCalibrationFullData.Count - 1][16];
                //mCSHorgProbe.Y = mCalibrationFullData[mCalibrationFullData.Count - 1][17];
                //mCSHorgProbe.Z = mCalibrationFullData[mCalibrationFullData.Count - 1][18];
            }

            mCSHorgProbe6D[0] = mCalibrationFullData[mCalibrationFullData.Count - 1][16];
            mCSHorgProbe6D[1] = mCalibrationFullData[mCalibrationFullData.Count - 1][17];
            mCSHorgProbe6D[2] = mCalibrationFullData[mCalibrationFullData.Count - 1][18];
            mCSHorgProbe6D[3] = mCalibrationFullData[mCalibrationFullData.Count - 1][19];
            mCSHorgProbe6D[4] = mCalibrationFullData[mCalibrationFullData.Count - 1][20];
            mCSHorgProbe6D[5] = mCalibrationFullData[mCalibrationFullData.Count - 1][21];



            MotorSetHome6D();

            //  Calibration 은 CSHorg 상태를 원점으로 하여 수행해야 한다.
        }
        //  public Point3d FindPivot()

        public void FindHCSrotationAB() //  HCS : Hexapod Coordinate System
        {
            //  Z pivot 을 Porg 위치로 보낸다.
            //  이를 위해서는 CSHorg 상태에서 Z pivot 을 구해서 적용하고(적용하면 Z pivot 이 Center of Fiducial Mark 와 일치하게 됨),
            //  이어서 Porg -> CSHorg 벡터만큼 뒤로 보내서 CSHorg 가 Porg 에 오도록 한다.
            //  
            //  Z 축에 대해서 -2 -> +2 deg 회전시킨다. 
            //  이때 pZ2 의 변화량 -> Beta 구한다
            //  Hexapod 의 회전축으로서의 Z 축이 Probe 로 정의되는 XYZ 좌표계에서 YZ 면과 이루는 각도를 β , 
            //  Hexapod 의 회전축으로서의 Z 축이 Probe 로 정의되는 XYZ 좌표계에서 XZ 면과 이루는 각도를 α ,
            //  α 및 β  를 측정해서 Hexapod Coordinate System 을 변환한다.

            double[] tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min
            double angle = tagetPos[3] - tagetPos[1];
            int itrCnt = 0;
            double[] orgPos = MotorCurPos6D();

            double hcsX = 0;
            double hcsY = 0;
            double hcsXold = 0;
            double hcsYold = 0;

            while (itrCnt++ < 10)
            {

                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                MotorMoveHome6D();
                MotorSetPivot(0, 0, 0);
                Thread.Sleep(100);
                MotorMoveAbs6D(mFidorg.X, mFidorg.Y, mFidorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);
                Thread.Sleep(100);

                MotorSetHCS(hcsX, hcsY, mHCSrotation.Z);

                for (int i = 0; i < 5; i++)
                {
                    MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5] + tagetPos[i]);
                    if (i < 4)
                    {
                        if (i == 1 || i == 3)
                            Thread.Sleep(600);
                        SingleFindMark(true);
                    }
                }
                double dZ1 = mGageFullData[3][5] - mGageFullData[1][5];
                double dZ2 = mGageFullData[3][4] - mGageFullData[1][4];
                double dZ3 = mGageFullData[3][6] - mGageFullData[1][6];
                angle = mCalibrationFullData[3][21] - mCalibrationFullData[1][21];
                double beta = Math.Atan(dZ2 / (83000 * Math.Sin(angle / 180 * Math.PI)));   //  angle to YZ plane
                double alpha = Math.Atan((dZ1 - dZ3) / (120000 * Math.Sin(angle / 180 * Math.PI))); //  anlge to XZ plane

                hcsX = -alpha * RAD_To_MIN;
                hcsY = -beta * RAD_To_MIN;
                if (Math.Abs(hcsX) < 1 && Math.Abs(hcsY) < 1)
                    break;
                hcsX += hcsXold;
                hcsY += hcsYold;
                hcsXold = hcsX;
                hcsYold = hcsY;
            }

            mHCSrotation.X = hcsXold;
            mHCSrotation.Y = hcsYold;

            MotorSetHCS(0, 0, 0);
            MotorMoveHome6D();

        }

        public void FindFidorg()
        {
            //  mFidorg 는 mCSHorg 에 있을 때
            //  mFidorg.Y = Center of Fiducial Mark 로부터 pZ1, Pz3 를 연결하는 직선까지의 거리
            //  mFidorg.X = Center of Fiducial Mark 로부터 pZ1-pZ3 의 중점과 pZ2 를 연결하는 직선까지의 거리

            //  찾는 방법은, ㅡCSHorg 로 이동한 상태에서 

            //  X pivot 중심으로 X 회전하면 Probe Z 에 변위가 발생한다. 이것은 Probe Z 가 Center of Fiducial Mark 에서 Y방향으로 떨어져있기 때문이다.
            //  Probe Z 와 Center of Fiducial Mark 간 Y 방향으로의 이격거리는 Z변위 / tan(phi) 가 된다. 
            //  이때 X pivot 은 Center of Fiducial Mark 에 일치하게 설정되었음을 가정한다

            //  Y pivot 중심으로 Y 회전하면 Probe Z 에 변위가 발생한다. 이것은 Probe Z 가 Center of Fiducial Mark 에서 X방향으로 떨어져있기 때문이다.
            //  Probe Z 와 Center of Fiducial Mark 간 X 방향으로의 이격거리는 Z변위 / tan(theta) 가 된다. 
            //  이때 Y pivot 은 Center of Fiducial Mark 에 일치하게 설정되었음을 가정한다

            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);

            //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
            MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
            HexapodRotate(0, 0, 0);
            Thread.Sleep(200);

            SingleFindMark();

            mCSHorgProbe.X = mGageFullData[0][0];
            mCSHorgProbe.Y = mGageFullData[0][1];
            mCSHorgProbe.Z = (mGageFullData[0][5] + mGageFullData[0][6]) / 2;

            double[] orgPos = MotorCurPos6D();


            //string hexPosFile = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\HexPos.csv";
            //double[] HexCurPos = MotorCurPosHexapod();
            //string strHexCurPos = $"Before FindFidorg,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            //File.AppendAllText(hexPosFile, strHexCurPos);

            double[] tagetPos = new double[5] { -160, -150, 0, 150, 0 };   //  min   //  Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.
            double angle = tagetPos[3] - tagetPos[1];
            MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);
            //MotorSetHCS(0, 0, mHCSrotation.Z); // 수정된 좌표계기준으로 측정하면 오히려 안됨. 측정은 절대좌표계기준으로 해야함.

            int itrCnt = 0;
            mFidorg.Y = 0;
            double Y = 0;
            double dZup = 0;
            double dZdown = 0;
            double dZupSum = 0;
            double dZdownSum = 0;
            while (itrCnt++ < 4)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();

                for (int i = 0; i < 5; i++)
                {
                    //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]); //  -244.1   // -30
                    MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
                    HexapodRotate(orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]);
                    if (i < 4)
                    {
                        if (i == 1 || i == 2 || i == 3)
                            Thread.Sleep(400);
                        SingleFindMark(true);
                    }
                }
                double phi_up = mCalibrationFullData[3][19] - mCalibrationFullData[2][19]; //  TdXup
                double phi_down = mCalibrationFullData[2][19] - mCalibrationFullData[1][19]; //  dTXdown
                dZup = (mGageFullData[3][5] + mGageFullData[3][6] - mGageFullData[2][5] - mGageFullData[2][6]) / 2;//+ 1510*(1/Math.Cos(phi_up * MIN_To_RAD)-1);
                //dZdown = (mGageFullData[2][5] + mGageFullData[2][6] - mGageFullData[1][5] - mGageFullData[1][6]) / 2 - 1510 * (1 / Math.Cos(phi_down * MIN_To_RAD) - 1);
                dZdown = (mGageFullData[2][5] + mGageFullData[2][6] - mGageFullData[1][5] - mGageFullData[1][6]) / 2;// + 1510 * (1 / Math.Cos(phi_down * MIN_To_RAD) - 1);

                double Lup = dZup / Math.Tan(phi_up * MIN_To_RAD);  //  um
                double Ldown = dZdown / Math.Tan(phi_down * MIN_To_RAD);  //  um
                dZupSum += Lup;
                dZdownSum += Ldown;
                Y += (Lup + Ldown) / 2;
            }
            mFidorg.Y = Y / 4;   // 5회 평균

            //  점검
            mbFigorgLoaded = true;
            mCalibrationFullData.Clear();
            mGageFullData.Clear();
            //for (int i = 0; i < 5; i++)
            //{
            //    //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]); //  -244.1   // -30
            //    MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
            //    HexapodRotate(orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]);
            //    if (i < 4)
            //    {
            //        if (i == 1 || i == 2 || i == 3)
            //            Thread.Sleep(200);
            //        SingleFindMark(true);
            //    }
            //}
            //for (int i = 0; i < 5; i++)
            //{
            //    //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y + 500, mCSHorg.Z, orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]); //  -244.1   // -30
            //    MotorXYZ(mCSHorg.X, mCSHorg.Y + 500, mCSHorg.Z);
            //    HexapodRotate(orgPos[3] + tagetPos[i], orgPos[4], orgPos[5]);
            //    if (i < 4)
            //    {
            //        if (i == 1 || i == 2 || i == 3)
            //            Thread.Sleep(200);
            //        SingleFindMark(true);
            //    }
            //}
            AddVsnLog("Fidorg Y " + mFidorg.Y.ToString("F3"));


            MotorMoveOriginHexapod();
            MotorSetPivot(0, 0, 0);
            //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
            MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
            HexapodRotate(0, 0, 0);
            Thread.Sleep(200);
            orgPos = MotorCurPos6D();
            Thread.Sleep(100);

            //HexCurPos = MotorCurPosHexapod();
            //strHexCurPos = $"During FindFidorg,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            //File.AppendAllText(hexPosFile, strHexCurPos);

            tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min   //  Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.
            angle = tagetPos[3] - tagetPos[1];
            MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
            //MotorSetHCS(0, 0, mHCSrotation.Z); // 수정된 좌표계기준으로 측정하면 오히려 안됨. 측정은 절대좌표계기준으로 해야함.
            itrCnt = 0;
            mFidorg.X = 0;
            double X = 0;
            dZupSum = 0;
            dZdownSum = 0;

            while (itrCnt++ < 4)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 5; i++)
                {
                    //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]); //  -244.1   // -30
                    MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
                    HexapodRotate(orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]);
                    if (i < 4)
                    {
                        if (i == 1 || i == 3)
                            Thread.Sleep(400);
                        SingleFindMark(true);
                    }
                }
                double phi_up = mCalibrationFullData[3][20] - mCalibrationFullData[2][20]; //  dTYup
                double phi_down = mCalibrationFullData[2][20] - mCalibrationFullData[1][20]; //  dTYdown

                dZup = (mGageFullData[3][5] + mGageFullData[3][6] - mGageFullData[2][5] - mGageFullData[2][6]) / 2;// + 1510 * (1 / Math.Cos(phi_up * MIN_To_RAD) - 1);
                dZdown = (mGageFullData[2][5] + mGageFullData[2][6] - mGageFullData[1][5] - mGageFullData[1][6]) / 2;// + 1510 * (1 / Math.Cos(phi_down * MIN_To_RAD) - 1);


                double Lup = dZup / Math.Tan(phi_up * MIN_To_RAD);  //  um
                double Ldown = dZdown / Math.Tan(phi_down * MIN_To_RAD);  //  um
                dZupSum += Lup;
                dZdownSum += Ldown;
                X += (Lup + Ldown) / 2;
            }
            mFidorg.X = X / 5;   // 5회 반복 평균,
            mFidorg.Z = 0;

            //  점검
            mCalibrationFullData.Clear();
            mGageFullData.Clear();
            //for (int i = 0; i < 5; i++)
            //{
            //    //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]); //  -244.1   // -30
            //    MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
            //    HexapodRotate(orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]);
            //    if (i < 4)
            //    {
            //        if (i == 1 || i == 3)
            //            Thread.Sleep(200);
            //        SingleFindMark(true);
            //    }
            //}
            //for (int i = 0; i < 5; i++)
            //{
            //    //MotorMoveAbs6D(mCSHorg.X+500, mCSHorg.Y, mCSHorg.Z, orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]); //  -244.1   // -30
            //    MotorXYZ(mCSHorg.X, mCSHorg.Y+500, mCSHorg.Z);
            //    HexapodRotate(orgPos[3], orgPos[4] + tagetPos[i], orgPos[5]);
            //    if (i < 4)
            //    {
            //        if (i == 1 || i == 3)
            //            Thread.Sleep(200);
            //        SingleFindMark(true);
            //    }
            //}
            MotorMoveOriginHexapod();
            //HexCurPos = MotorCurPosHexapod();
            //strHexCurPos = $"After FindFidorg,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            //File.AppendAllText(hexPosFile, strHexCurPos);

            AddVsnLog("Fidorg X " + mFidorg.X.ToString("F3"));
            mbFigorgLoaded = true;
        }

        public double ZcompensationAboutTXTY(double Xmm, double Ymm, double Zmm, double TXmin, double TYmin)
        {
            //  TX 및 TY 의 회전중심이 Center of Fiducial Mark 일 때
            //  아래 적용 시 Probe 측정 Z 값과 CSH 측정 Z 값이 같게 된다.
            double res = Zmm
                + (Ymm + mFidorg.Y / 1000 - mCSHorg.Y) * Math.Tan(TXmin / (60 * 180 / Math.PI))
                + (Xmm + mFidorg.X / 1000 - mCSHorg.X) * Math.Tan(TYmin / (60 * 180 / Math.PI));

            //  TX 및 TY 의 회전중심이 Center of Fiducial Mark 가 아닐 때는
            //  TX 회전중심부터 Center of Fiducial Mark 까지의 벡터를 알아야 한다.
            //  아래 적용 시 Probe 측정 Z 값과 CSH 측정 Z 값이 같게 된다.

            return res;
        }

        double MIN_To_RAD = Math.PI / (180 * 60);
        double RAD_To_MIN = 180 * 60 / Math.PI;

        public double ProbeZcompensationForTXTY(double px, double py, double pz, double pTXrad, double pTYrad)
        {
            double resZ = 0;

            if (!mbFigorgLoaded)
                //return resZ;
                return pz;  // 보정 적용 안된 초기값 반환: EastView할때는 mbFigorgLoaded false임.

            //  mCSHorgProbe 에서 Probe 값을 (0,0,0) 으로 리셋하지 않고 그 이후로도 Probe 값을 리셋하지 않는 경우
            //  즉 mCSHorg 를 찾기 전에 Probe 값을 리셋하고 이후로는 Probe 값을 리셋하지 않는 경우 다음 식 적용
            double curX = -px - mCSHorgProbe.X + mFidorg.X;   //  um
            double curY = py - mCSHorgProbe.Y + mFidorg.Y;   //  um

            //  mCSHorgProbe 에서 Probe 값을 (0,0,0) 으로 리셋했고 그 이후에 mFidorg 를 측정했으므로 mFidorg 로부터 현재위치까지의 거리는 다음 식이 맞다.
            //double curX = - px + mFidorg.X;   //  um   
            //double curY = py + mFidorg.Y;   //  um

            double pT = Math.Sqrt(pTXrad * pTXrad + pTYrad * pTYrad);
            resZ -= Math.Sin(pTYrad) * curX;
            resZ -= Math.Sin(pTXrad) * curY;
            resZ += 1510 * (1 / Math.Cos(pT) - 1);

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

        private void button18_Click(object sender, EventArgs e)
        {
            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

            //  Back Calculation 검증
            //////Point3d res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0), 0, 0);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0.5), 0, 0);

            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0), 90, 0);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0, 0), 90, 0);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0.6, 0), 90, 0);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0.6, 0), 90, 0);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0), 0, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0, 0), 0, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0.6, 0), 0, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0.6, 0), 0, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0, 0), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0.6, 0), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0.6, 0), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0, 0, 0.5), 90, 90);
            //////res = XYZcompensationAboutZPivots(new Point3d(0.4, 0.6, 0.5), 90, 90);
            // ~ 여기까지 검증 완료 241211

            /////
            ///////// FindAllOrgs() 검증

            //////FindAllOrgs();


            ///// 다중 회전 검증
            LoadOQCcondition();
            LoadPivotXYZ();

            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            MotorSetHCS(0, 0, 0);
            MotorSetPivot(0, 0, 0);

            MotorMoveAbs6D(mFidorg.X, mFidorg.Y, mFidorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);
            Thread.Sleep(500);
            double[] orgPos = MotorCurPos6D();
            MotorSetHCS(0, 0, mHCSrotation.Z);

            //  Z 축 회전
            //  Y 축 회전
            //  X 축 회전
            double[] tagetPos = new double[6] { 0, -150, -50, 50, 150, 0 };   //  min   //  중심에서 3축 공동 측정가능 범위
            List<Point3d> lpivots = new List<Point3d>();

            //  임의의 회전상태에서 찾은 Pivot 이 동일할 것.
            for (int i = 0; i < 6; i++)
            {
                MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
                MotorMoveAbs6D(mFidorg.X, mFidorg.Y, mFidorg.Z, mPorg.TX, mPorg.TY + tagetPos[i], mPorg.TZ);
                Point3d aPivot = FindPivot(1);
                lpivots.Add(aPivot);
            }
            for (int i = 0; i < 6; i++)
            {
                MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
                MotorMoveAbs6D(mFidorg.X, mFidorg.Y, mFidorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ + tagetPos[i]);
                Point3d aPivot = FindPivot(1);
                lpivots.Add(aPivot);
            }

            string lstr = "";

            for (int i = 0; i < lpivots.Count; i++)
            {
                lstr += lpivots[i].X.ToString() + "," + lpivots[i].Y.ToString() + "," + lpivots[i].Z.ToString() + "\r\n";
            }
            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\CompoundPivot.csv");
            wr.Write(lstr);
            wr.Close();



            /// 다음은 목적에 관계없이 공통

            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

        }

        private void FindAllOrgs()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";

            mPorg = new VolumetricTP6D();     //  um
            mCSHorg = new VolumetricTP6D();     //  um
            mCSHorgProbe6D = new double[6];     //  um
            mCSHorgProbe = new Point3d();     //  um
            mFidorg = new Point3d();     //  um
            mHCSrotation = new Point3d();    //  min

            AddVsnLog("Start to find CSHorg.");
            //  최초에 CSHorg 잡고 이때만 Probe 를 Reset 한다. 이때의 Probe 상태를 기준으로 Porg 등을 측정 저장한다.
            FindCSHorg(true);

            AddVsnLog("Start to find Porg.");
            FindPorg();
            //FindHCSrotationPsi();
            //  여기서 mPorg 저장
            StreamWriter wr = new StreamWriter(oqcFile);
            wr.WriteLine(mPorg.X.ToString() + "\t" + mPorg.Y.ToString() + "\t" + mPorg.Z.ToString() + "\t" +
                         mPorg.TX.ToString() + "\t" + mPorg.TY.ToString() + "\t" + mPorg.TZ.ToString());
            wr.Close();

            // FindPorg 후 헥사포드의 6축 모두 읽어옴.
            //string hexPosFile = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\HexPos.csv";
            //double[] HexCurPos = MotorCurPosHexapod();
            //string strHexCurPos = $"After FindPorg,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            //File.AppendAllText(hexPosFile, strHexCurPos);

            AddVsnLog("Start to find HCS psi.");
            FindHCSrotationPsi();

            //  여기서 mHCSrotation 저장
            string strHCSrotatio = mHCSrotation.X.ToString() + "\t" + mHCSrotation.Y.ToString() + "\t" + mHCSrotation.Z.ToString() + "\r\n";
            File.AppendAllText(oqcFile, strHCSrotatio);

            // FindHCSrotationPsi 후 헥사포드의 6축 모두 읽어옴
            //HexCurPos = MotorCurPosHexapod();    
            //strHexCurPos = $"After Find HCS Psi,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            //File.AppendAllText(hexPosFile, strHexCurPos);

            AddVsnLog("Start to find CSHorg.");
            FindCSHorg();

            //  여기서 mCHSorg 저장
            string strCSHorg = mCSHorg.X.ToString() + "\t" + mCSHorg.Y.ToString() + "\t" + mCSHorg.Z.ToString() + "\t" +
                                mCSHorg.TX.ToString() + "\t" + mCSHorg.TY.ToString() + "\t" + mCSHorg.TZ.ToString() + "\r\n";
            File.AppendAllText(oqcFile, strCSHorg);
            string strCSHProbe = mCSHorgProbe.X.ToString() + "\t" + mCSHorgProbe.Y.ToString() + "\t" + mCSHorgProbe.Z.ToString();
            File.AppendAllText(oqcFile, strCSHProbe);

            FindFidorg();   // 할려면 Find Pivot 후에 해야함.
            SaveFidorg();
            // FindCSHorg() 후 헥사포드의 6축 모두 읽어옴
            //HexCurPos = MotorCurPosHexapod();  
            //strHexCurPos = $"After Find CSHorg,{HexCurPos[0]},{HexCurPos[1]},{HexCurPos[2]},{HexCurPos[3]},{HexCurPos[4]},{HexCurPos[5]}\n";
            //File.AppendAllText(hexPosFile, strHexCurPos);
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
        private void button20_Click(object sender, EventArgs e)
        {
            LoadOQCcondition();
            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리
            FindCSHorg();
            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

            SaveCSHorg();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            PivotRepeatability();
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
        private void FindOQCcondition()
        {
            m__G.mDoingStatus = "FindOQCcondition";
            //if (m__G.mGageCounter != null)
            //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

            AddVsnLog("Start to find OQC condition of system ID " + m__G.mCamID0);
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";

            FindAllOrgs();


            bool bOQCloaded = LoadOQCcondition();
            AddVsnLog("Load Porg and CSHorg");

            StartLive();

            MotorSetPivot(0, 0, 0);
            MotorSetSpeed6D(SpeedLevel.Normal);

            if (bOQCloaded)
                MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
            else
                MotorMoveHome6D();

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text = "";

                });

            }
            else
            {
                tbInfo.Text = "";
            }

            // tbInfo.Text = "";
            SingleFindMark();
            string cshOrgStr = tbInfo.Text;

            Thread.Sleep(100);
            GrabHalt();


            // MotorSetHCS(0, 0, mHCSrotation.Z);  // 의미 없음 FindPivot 할때 Pivot 설정하면 HCS는 (0,0,0)이 됨
            ClearHexapodPivots();



            AddVsnLog("Start to find X pivot.");
            FindPivot(1);

            AddVsnLog("Start to find Y pivot.");
            FindPivot(2);

            AddVsnLog("Start to find Z pivot.");
            FindPivot(3);

            SavePivots();


            // MotorSetHCS(0, 0, 0);
            // MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);  //  Move TX (arcmin)
            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);  //  Move TX (arcmin)

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text = cshOrgStr;
                });
            }
            else
            {
                tbInfo.Text = cshOrgStr;
            }
            SingleFindMark();

            string lstr = "X Pivot \t" + mHexapodPivots[0].X.ToString("F7") + "\t" + mHexapodPivots[0].Y.ToString("F7") + "\t" + mHexapodPivots[0].Z.ToString("F7") + "\r\n"
                   + "Y Pivot \t" + mHexapodPivots[1].X.ToString("F7") + "\t" + mHexapodPivots[1].Y.ToString("F7") + "\t" + mHexapodPivots[1].Z.ToString("F7") + "\r\n"
                   + "Z Pivot \t" + mHexapodPivots[2].X.ToString("F7") + "\t" + mHexapodPivots[2].Y.ToString("F7") + "\t" + mHexapodPivots[2].Z.ToString("F7") + "\r\n"
                          ;

            //string FidorgFile = m__G.m_RootDirectory + "\\DoNotTouch\\Fidorg.txt";
            //if (!File.Exists(FidorgFile))
            //{
            //    AddVsnLog("Start to find Fidorg.");
            //    FindFidorg();
            //    string mstr = /*"Fidorg \t" + */mFidorg.X.ToString("F7") + "\t" + mFidorg.Y.ToString("F7") + "\t" + mFidorg.Z.ToString("F7") + "\r\n";
            //    lstr += "Fidorg \t" + mFidorg.X.ToString("F7") + "\t" + mFidorg.Y.ToString("F7") + "\t" + mFidorg.Z.ToString("F7") + "\r\n";
            //    StreamWriter wr = new StreamWriter(FidorgFile);
            //    wr.Write(mstr);
            //    wr.Close();
            //}
            //else
            //{
            //    LoadFidorg();
            //}


            //AddVsnLog("Start to find HCS rotation.");
            //FindHCSrotationAB();  //  무의미. 기구적으로 정확함.

            //lstr += "Fidorg \t" + mFidorg.X.ToString("F7") + "\t" + mFidorg.Y.ToString("F7") + "\t" + mFidorg.Z.ToString("F7") + "\r\n"
            //       + "HCS rotation \t" + mHCSrotation.X.ToString("F7") + "\t" + mHCSrotation.Y.ToString("F7") + "\t" + mHCSrotation.Z.ToString("F7") + "\r\n";


            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += lstr;
                });

            }
            else
            {
                tbInfo.Text += lstr;
            }

            //FindAllOrgs();
            //    ////FindPorg();
            //    ////FindHCSrotationPsi();
            //    ////FindCSHorg();
            //FindPivot(1, true);
            //FindPivot(2, true);
            //FindPivot(3, true);
            //FindFidorg();
            AddVsnLog("Finish setting OQC condition.");

            //if (m__G.mGageCounter != null) 
            //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리
            m__G.mDoingStatus = "IDLE";

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
        private void cbBench_CheckedChanged(object sender, EventArgs e)
        {
            if (cbBench.Checked)
            {
                PogoPinUnloadBtn.Show();
                PogoPinloadBtn.Show();
                SidePushUnloadBtn.Show();
                SidePushloadBtn.Show();
                BaseDownBtn.Show();
                BaseUpBtn.Show();
            }
            else
            {
                PogoPinUnloadBtn.Hide();
                PogoPinloadBtn.Hide();
                SidePushUnloadBtn.Hide();
                SidePushloadBtn.Hide();
                BaseDownBtn.Hide();
                BaseUpBtn.Hide();
            }
        }

        private void FVision_Shown(object sender, EventArgs e)
        {
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            LoadOQCcondition();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (!mAutoCalibrationRun)
            {
                mAutoCalibrationRun = true;
                btnScan.Text = "Stop Scan";
            }
            else
            {
                mAutoCalibrationRun = false;
                btnScan.Text = "Start Scan";
                return;
            }

            Axis axis;
            if (rbCalZ.Checked)
                axis = Axis.Z;
            else if (rbCalX.Checked)
                axis = Axis.X;
            else if (rbCalY.Checked)
                axis = Axis.Y;
            else if (rbCalTZ.Checked)
                axis = Axis.TZ;
            else if (rbCalTX.Checked)
                axis = Axis.TX;
            else if (rbCalTY.Checked)
                axis = Axis.TY;
            else
                return;

            double onewayStroke;
            if (tbMaxStroke.Text.Length > 1)
            {
                onewayStroke = double.Parse(tbMaxStroke.Text);
            }
            else
                return;


            Task.Run(() =>
            {

                if (LoadOQCcondition() == false)
                {
                    MessageBox.Show("Fail to Load OQC Condition");
                }
                if (LoadPivotXYZ() == false)
                {
                    MessageBox.Show("Fail to Load Pivot");
                }

                FindCSHorg();
                FindFidorg();
                SaveOQCCondition();

                if (axis == Axis.TX)
                {
                    AddVsnLog("Start to find X pivot.");
                    FindPivot(1); //    FindPivot(1) 여기 있으나 없으나 Z 오차 변화 없음
                }
                else if (axis == Axis.TY)
                {
                    AddVsnLog("Start to find Y pivot.");
                    FindPivot(2); //    FindPivot(1) 여기 있으나 없으나 Z 오차 변화 
                }
                else if (axis == Axis.TZ)
                {
                    AddVsnLog("Start to find Z pivot.");
                    FindPivot(3); //    FindPivot(1) 여기 있으나 없으나 Z 오차 변화 
                }

                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.CloseAllport();// 250210 주석처리

                AxisCalibration(axis, onewayStroke, true, false, false);
                mAutoCalibrationRun = false;
                this.Invoke(new Action(() =>
                {
                    btnScan.Text = "Start scan";
                }));
            });
        }

        private void btnAutoCal_Click(object sender, EventArgs e)
        {
            if (!mAutoCalibrationRun)
            {
                mAutoCalibrationRun = true;
                btnAutoCal.Text = "Stop Cal";
            }
            else
            {
                mAutoCalibrationRun = false;
                btnAutoCal.Text = "Auto Cal";
                return;
            }

            tbCalibration.Text = "";

            Task.Run(() =>
            {
                AutoCalibration();
                mAutoCalibrationRun = false;
                this.Invoke(new Action(() =>
                {
                    btnAutoCal.Text = "Auto Cal";
                }));
            });
        }
        private void btnEastView_Click(object sender, EventArgs e)
        {
            if (!mAutoCalibrationRun)
            {
                mAutoCalibrationRun = true;
                btnEastView.Text = "Stop Cal";
            }
            else
            {
                mAutoCalibrationRun = false;
                btnEastView.Text = "EastView";
                return;
            }

            tbCalibration.Text = "";

            TextAppendTbInfo("Start east-side view calibration");

            Task.Run(() =>
            {
                InitializeScaleNTheta();
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                 ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                 ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                 ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                 ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                 ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView);
                TextAppendTbInfo("Reset scales");
                SaveScaleNTheta();

                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

                FindCSHorg(true);

                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.CloseAllport();// 250210 주석처리

                TextAppendTbInfo("Start baseline measurement");
                EastViewCalibration(true);
                TextAppendTbInfo("Finish  baseline measurement");
                LoadScaleNTheta();
                TextAppendTbInfo("Start verification measurement");
                EastViewCalibration(false);
                TextAppendTbInfo("Finsh verification measurement");
                TextAppendTbInfo("Finish east-side view calibration");
                // ReAutoCalibration();
                mAutoCalibrationRun = false;
                this.Invoke(new Action(() =>
                {
                    btnEastView.Text = "EastView";
                }));
            });


        }



        private void TextAppendTbInfo(string sInfo)
        {
            sInfo += "\r\n";

            if (tbVsnLog.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbVsnLog.Text += sInfo;
                });
            else
                tbVsnLog.Text += sInfo;
        }

        private void FVision_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void btnCheckStrokeRange_Click(object sender, EventArgs e)
        {
            if (!mAutoCalibrationRun)
            {
                mAutoCalibrationRun = true;
            }
            else
            {
                mAutoCalibrationRun = false;
                return;
            }

            Task.Run(() =>
            {
                // Stroke 범위 내에서 측정 가능한지 확인
                FindCSHorg();
                // X
                if (!mAutoCalibrationRun) return;
                double[] orgPos = MotorCurPos6D();
                for (int i = 0; i < 5; i++)
                {
                    switch (i)
                    {
                        case 0:
                            break;
                        case 1:
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] + 900);
                            break;
                        case 2:
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] - 900);
                            break;
                        case 3:
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] + 800);
                            break;
                        case 4:
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] - 800);
                            break;
                    }
                    SingleFindMark();
                    MotorMoveAbsAxis(Axis.X, orgPos[0] + 1900);
                    SingleFindMark();
                    MotorMoveAbsAxis(Axis.X, orgPos[0]);
                    SingleFindMark();
                    MotorMoveAbsAxis(Axis.X, orgPos[0] - 1900);
                    SingleFindMark();
                    MotorMoveAbsAxis(Axis.X, orgPos[0]);
                    SingleFindMark();

                    MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                }
                // Y
                if (!mAutoCalibrationRun) return;
                for (int i = 0; i < 5; i++)
                {
                    switch (i)
                    {
                        case 1:
                            MotorMoveAbsAxis(Axis.X, orgPos[0] + 900);
                            break;
                        case 2:
                            MotorMoveAbsAxis(Axis.X, orgPos[0] - 900);
                            break;
                        case 3:
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] + 600);
                            break;
                        case 4:
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] - 600);
                            break;
                    }
                    switch (i)
                    {
                        case 0:
                        case 1:
                        case 2:
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] + 1900);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1]);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] - 1900);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1]);
                            SingleFindMark();
                            break;
                        case 3:
                        case 4:
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] + 700);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1]);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] - 700);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Y, orgPos[1]);
                            SingleFindMark();
                            break;
                    }
                    MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                }
                // Z
                if (!mAutoCalibrationRun) return;
                for (int i = 0; i < 5; i++)
                {
                    switch (i)
                    {
                        case 1:
                            MotorMoveAbsAxis(Axis.X, orgPos[0] + 900);
                            break;
                        case 2:
                            MotorMoveAbsAxis(Axis.X, orgPos[0] - 900);
                            break;
                        case 3:
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] + 600);
                            break;
                        case 4:
                            MotorMoveAbsAxis(Axis.Y, orgPos[1] - 600);
                            break;
                    }
                    switch (i)
                    {
                        case 0:
                        case 1:
                        case 2:
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] + 1750);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2]);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] - 1750);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2]);
                            SingleFindMark();
                            break;
                        case 3:
                        case 4:
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] + 700);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2]);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2] - 700);
                            SingleFindMark();
                            MotorMoveAbsAxis(Axis.Z, orgPos[2]);
                            SingleFindMark();
                            break;
                    }
                    MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                }

                mAutoCalibrationRun = false;
            });
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
    }
}