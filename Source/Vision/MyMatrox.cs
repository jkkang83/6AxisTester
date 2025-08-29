﻿
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using Matrox.MatroxImagingLibrary;
using System.Threading;

using System.Windows.Forms;
using System.IO.Ports;
using System.Xml;
using OpenCvSharp;

namespace S2System.Vision
{
    using FZ4P;
    using Dln.Exceptions;
    using OpenCvSharp.Extensions;
    using OpenCvSharp.Flann;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Xml.Serialization;
    using static alglib;
    //using static FZ4P.FVision;
    using static FAutoLearn.FAutoLearn;
    using Point = OpenCvSharp.Point;
    using Size = OpenCvSharp.Size;

    public class SupremeTimer
    {
        [DllImport("Kernel32.dll")]
        public static extern int QueryPerformanceCounter(ref Int64 count);

        [DllImport("Kernel32.dll")]
        public static extern int QueryPerformanceFrequency(ref Int64 frequency);
    }

    public class MILlib
    {
        public MicroLibrary.MicroTimer uTimer = null;
        public FAutoLearn.FAutoLearn mFAL = null;

        public const int M_HROI = 1800;

        public static int me = 0;
        public bool m_bAnotherIdle = true;
        //public int MAX_GRAB_COUNT = 0;

        public bool m_bProcess_Vision = false;
        public double FPS;

        public OpenCvSharp.Point2d[][] mAzimuthPts = new OpenCvSharp.Point2d[10000][];
        public OpenCvSharp.Point2d[][] mAzimuthPtsUpper = new OpenCvSharp.Point2d[10000][];
        public OpenCvSharp.Point2d[][] mAzimuthPtsLower = new OpenCvSharp.Point2d[10000][];
        public double[] mAvgLED = new double[10000];

        public bool[] mC_pDone = new bool[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
        public double[] mC_pX = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
        public double[] mC_pY = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
        public double[] mC_pZ = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
        public double[] mC_pTX = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
        public double[] mC_pTY = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
        public double[] mC_pTZ = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view

        public double[][] mBufMTF = new double[12][];
        public bool bNoHostPC = false;

        public delegate void DelegateStandbyTrigger();
        public delegate void DelegateAckSignal(int ch, bool IsHigh);
        DelegateAckSignal AckSignal;
        DelegateStandbyTrigger StandbyTrigger;
        public void RegisterDelegates(DelegateAckSignal fAckSignal)
        {
            AckSignal = fAckSignal;
        }
        public void RegisterDelegatesStandbyTrigger(DelegateStandbyTrigger fStandbyTrigger)
        {
            StandbyTrigger = fStandbyTrigger;
        }

        public void ResetmCpXY()
        {
            mC_pX = new double[10000];
            mC_pY = new double[10000];
        }

        public int mTmpCount = 0;

        public double[] m_Yscale = new double[2] { 1, 1 };

        public long[] GrabT1 = new long[10000];
        //public long[] GrabT2 = new long[8000];

        public int dAFZM_FrameCount = 0;
        public int dAF_FrameCount = 0;
        public int dZoom_FrameCount = 0;
        public int dAFStep_FrameCount = 0;
        public int dZoomStep_FrameCount = 0;

        public long mTimerFrequency = 0;

        public static int allocatedSysNum = 0;
        public static int MAX_DEFAULT_GRAB_COUNT = 2;                     // 

        public static int MAX_TRGGRAB_COUNT = 1000;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_TRGGRAB_6000 = 6050;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_TRGGRAB_2500 = 2550;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_TRGGRAB_1000 = 1050;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_TRGGRAB_500 = 550;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_TRGGRAB_200 = 250;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_TRGGRAB_50 = 100;                     // max 1000Frame/sec * 5sec = 5000
        public static int MAX_GRAB_COUNT = 1000;                     // max 1000Frame/sec * 5sec = 5000
        public string mMatroxMsg = "";

        //int mOptThresh = 32;
        public static int m_CamCnt = 0;

        private static MIL_ID milApplication = MIL.M_NULL;
        private static MIL_ID milRemoteApplication = MIL.M_NULL;
        public MIL_ID milSystem = MIL.M_NULL;
        private MIL_ID milDigitizer = MIL.M_NULL;
        private MIL_ID milDisplay = MIL.M_NULL;
        public MIL_ID milImageDisp = MIL.M_NULL;
        MIL_INT LicenseModules = 0;

        public long IMAGE_THRESHOLD_VALUE = 26;

        /* Minimum and maximum area of blobs. */
        public long MIN_BLOB_AREA = 40;
        public long MAX_BLOB_AREA = 800;

        /* Radius of the smallest particles to keep. */
        public static long MIN_BLOB_RADIUS = 2;

        /* Minimum hole compactness corresponding to a washer. */
        public double MIN_COMPACTNESS = 1.5;

        public MIL_ID MilGraphicList = MIL.M_NULL;                 // Graphic list identifier.
        public MIL_ID MilBinImage = MIL.M_NULL;                    // Binary image buffer identifier.
        public MIL_ID MilBlobResult = MIL.M_NULL;                  // Blob result buffer identifier.ㄹ
        public MIL_ID MilBlobContext = MIL.M_NULL;
        public MIL_INT TotalBlobs = 0;                             // Total number of blobs.
        public MIL_INT BlobsWithHoles = 0;                         // Number of blobs with holes.
        public MIL_INT BlobsWithRoughHoles = 0;                    // Number of blobs with rough holes.
        public MIL_INT mBlobSizeX = 0;                                  // Size X of the source buffer
        public MIL_INT mBlobSizeY = 0;                                  // Size Y of the source buffer
        public double[] mBlobCogX = new double[4];                               // X coordinate of center of gravity.
        public double[] mBlobCogY = new double[4];                               // Y coordinate of center of gravity.
        public double[] mBlobRadius = null;                               // Y coordinate of center of gravity.

        const int MAX_THREAD_NUM = 30;
        public byte[][] p_Value = new byte[MAX_THREAD_NUM][];//[M_HROI * 340];
        public byte[] p_Temp = null;
        public byte[][] q_Value = new byte[MAX_THREAD_NUM][];//[M_HROI / 4 * 340 / 4];    // 1/4 축소이미지
        public static double mModelScale = 1.0 / 3.0;

        public double mWaitLimitForNextTrigger = 1000;  //  msec
        double mWaitLimitForNextTriggerSec = 1.0;
        public struct sSearchModel
        {
            public int width;
            public int height;
            public int conv;
            public int sub;
            public double subconv;
            public int[] img;
            public int[] diffimg;
        }

        public sSearchModel[] mSearchModel = new sSearchModel[4];

        //public int[] m_YawSearchEndY = new int[2];
        //public int[] m_PitchSearchStartY = new int[2];
        //public int[,] mEachThresh = new int[6, 4];

        private MIL_ID[] milSaveBmp = new MIL_ID[1];
        private MIL_ID milTmp = MIL.M_NULL;
        private MIL_ID[] milImageGrab = new MIL_ID[MAX_DEFAULT_GRAB_COUNT];
        public MIL_ID[] milImageData = new MIL_ID[MAX_DEFAULT_GRAB_COUNT];
        private MIL_ID[] milQuaterImg = new MIL_ID[MAX_DEFAULT_GRAB_COUNT];

        public int mTrgBufLength = 10000;
        private MIL_ID[] milCommonImageGrab = new MIL_ID[MAX_TRGGRAB_COUNT];
        private MIL_ID[] milAFRelay = new MIL_ID[MAX_TRGGRAB_1000];
        private MIL_ID[] milXRelay = new MIL_ID[MAX_TRGGRAB_1000];
        private MIL_ID[] milYRelay = new MIL_ID[MAX_TRGGRAB_1000];
        private MIL_ID[] milXLinRelay = new MIL_ID[MAX_TRGGRAB_1000];
        private MIL_ID[] milYLinRelay = new MIL_ID[MAX_TRGGRAB_1000];
        private MIL_ID[] milCommonImageGrab6000 = new MIL_ID[MAX_TRGGRAB_6000];
        public int AFRelayCnt = 0;
        public int XRelayCnt = 0;
        public int YRelayCnt = 0;
        public int MRelayCnt = 0;
        public int XLinRelayCnt = 0;
        public int YLinRelayCnt = 0;

        static private MIL_ID[] milCommonImageResize = new MIL_ID[MAX_TRGGRAB_COUNT];
        //private MIL_ID[] milCommonImageGrab7000 = new MIL_ID[MAX_TRGGRAB_7000];
        //private MIL_ID[] milCommonImageGrab4500 = new MIL_ID[MAX_TRGGRAB_4500];
        //private MIL_ID[] milCommonImageGrab2000 = new MIL_ID[MAX_TRGGRAB_2000];
        //private MIL_ID[] milCommonImageGrab800 = new MIL_ID[MAX_TRGGRAB_800];
        //private MIL_ID[] milCommonImageGrab200 = new MIL_ID[MAX_TRGGRAB_200];
        //private MIL_ID[] milCommonImageGrab50 = new MIL_ID[MAX_TRGGRAB_50];
        int MilGrabBufferListSize = 0;


        //blic MIL_ID[] ZoomImageData = new MIL_ID[MAX_GRAB_COUNT];
        public MIL_ID[] InstantImageData;

        private MIL_ID milOverlayImage = MIL.M_NULL;

        private MIL_ID milEvent = MIL.M_NULL;
        private MIL_ID milThread = MIL.M_NULL;
        long keyColor;

        Graphics gr;
        static Font fontData = new System.Drawing.Font("Calibri", 30, FontStyle.Regular);
        static Font fontOKNG = new System.Drawing.Font("Calibri", 200, FontStyle.Bold);

        int m_nCam = 0;
        bool bChildSystem = false;
        bool bInit = false;
        bool bLive = false;
        public bool bLiveA = false;
        bool OnGrab = false;

        bool bFlipX = false;
        bool bFlipY = false;
        double dZoomFactorX = 2.0;
        double dZoomFactorY = 2.0;
        public int nSizeX = 0;
        public int nSizeY = 0;
        int nSizeYstep = 0;
        //double mMarkGapL = 11000;
        //double mMarkGapR = 11000;

        public int mm_TopViewThresh = 10;
        public int mm_TopViewMarkMax = 30;
        public int mm_SideViewThresh = 5;
        public int mm_SideViewBright = 15;

        public bool m_bUseUserVariables = false;
        public bool m_bYawTiltWihtTopEdge = false;
        public double mm_FakeMark = 0.38;       //(X + InMidTopX,       Y + InMidTopY )에서 Thresh 이하
        //public int mm_InMidTopX = 14;        //(X + InMidTopX,       Y + InMidTopY )에서 Thresh 이하
        //public int mm_InFrontBottomX = 0;    //(X + InFrontBottomX,  Y + InFrontBottomY )에서 Thresh 이하
        //public int mm_InMidBottomX = 14;     //(X + InMidBottomX,    Y + InMidBottomY )에서 Thresh 이하
        //public int mm_OutRearX1 = 29;        //( X + OutReaX1,       Y + OutReaY1 ) 에서 2*Thresh 이상
        //public int mm_OutRearX2 = 30;        //( X + OutRearX2,      Y + OutReaY2 ) 에서 2*Thresh 이상
        //public int mm_OutBottomX = 6;        //( X + OutBottomX,     Y + OutBottomY ) 에서 2*Thresh 이상        
        //public int mm_OutMidTopX = 10;       //( X + OutMidTopX,     Y + OutMidTopY ) 에서 2*Thresh 이상        

        //public int mm_InMidTopY = 0;         //(X + InMidTopX,       Y + InMidTopY )에서 Thresh 이하
        //public int mm_InFrontBottomY = 10;   //(X + InFrontBottomX,  Y + InFrontBottomY )에서 Thresh 이하
        //public int mm_InMidBottomY = 10;     //(X + InMidBottomX,    Y + InMidBottomY )에서 Thresh 이하
        //public int mm_OutRearY1 = 2;         //( X + OutReaX1,       Y + OutReaY1 ) 에서 2*Thresh 이상
        //public int mm_OutRearY2 = 2;         //( X + OutRearX2,      Y + OutReaY2 ) 에서 2*Thresh 이상
        //public int mm_OutBottomY = 22;       //( X + OutBottomX,     Y + OutBottomY ) 에서 2*Thresh 이상        
        //public int mm_OutMidTopY = -10;      //( X + OutMidTopX,     Y + OutMidTopY ) 에서 2*Thresh 이상        


        public double[] mOldAx = new double[6];
        public double[] mOldAy = new double[6];

        //public double[] mMScaleS = new double[2] { 1, 1 };
        //public double[] mMScalePitchaw = new double[2] { 1, 1 };
        //public double[] mMScalePitch = new double[2] { 1, 1 };

        public double[] sArea = new double[100];

        double dVisionScale = 1.0;
        public int nGrabIndex = 0;
        public int nMaxGrabCount = 0;

        public System.Drawing.Point nBoxP1 = new System.Drawing.Point();
        public System.Drawing.Point nBoxP2 = new System.Drawing.Point();

        public string mDataMsg = "";


        //public SerialPort              sPortCam1    = new SerialPort();
        //public SerialPort              sPortCam2    = new SerialPort();

        public class HookDataObject
        {
            public MIL_ID milImageDisp;
            public MIL_ID[] milImageGrab = new MIL_ID[MAX_DEFAULT_GRAB_COUNT];
            public MIL_ID[] milImageData = new MIL_ID[MAX_DEFAULT_GRAB_COUNT];
            public MIL_ID milEvent;
            public MIL_INT milGrabNumber;
            public MIL_INT milGrabIndex;
            public MIL_INT milExit;
            public int m_nCam;
        }

        public HookDataObject userHookData = null;
        GCHandle userHookDataHandle;
        MIL_DIG_HOOK_FUNCTION_PTR userHookFuncDelegate = null;
        MIL_THREAD_FUNCTION_PTR userThreadDelegate = null;

        public class myReverserClass : IComparer
        {

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y)
            {
                return ((new CaseInsensitiveComparer()).Compare(y, x));
            }

        }
        public struct dPoint
        {
            public double x;
            public double y;
        };

        public struct dLine
        {
            public double dSlope;
            public double dYintercept;
        };
        //public IComparer Max2MinComparer;

        public bool IsInit { get { return bInit; } }
        public bool IsLive { get { return bLive; } }
        public bool IsLiveA { get { return bLiveA; } }
        public int SizeX { get { return nSizeX; } }
        public int SizeY { get { return nSizeY; } }

        public double ZoomFactorX
        {
            get { return dZoomFactorX; }
            set { dZoomFactorX = value; }
        }
        public double ZoomFactorY
        {
            get { return dZoomFactorY; }
            set { dZoomFactorY = value; }
        }

        public double VisionScale
        {
            get { return dVisionScale; }
            set { dVisionScale = value; }
        }

        //public void SetXYScale(double Lx, double Ly, double Rx, double Ry)
        //{
        //    mMScaleS[0] = Lx;   //  Left Camera - Yaw
        //    mMScalePitch[0] = Ly;
        //    mMScaleS[1] = Rx;   //  Right Camera - Pitch & Stroke
        //    mMScalePitch[1] = Ry;
        //}
        public void SetBlobAreaMinMax(int lmin, int lmax)
        {
            MIN_BLOB_AREA = lmin;
            MAX_BLOB_AREA = lmax;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public MILlib(double ZoomFactor)
        {
            dZoomFactorX = ZoomFactor;
            dZoomFactorY = ZoomFactor;
            nMaxGrabCount = MAX_GRAB_COUNT;

            m_CamCnt++;


            for (int i = 0; i < 10000; i++)
            {
                mAzimuthPts[i] = new OpenCvSharp.Point2d[12];
                mAzimuthPtsUpper[i] = new OpenCvSharp.Point2d[12];
                mAzimuthPtsLower[i] = new OpenCvSharp.Point2d[12];
            }

            mFAL = new FAutoLearn.FAutoLearn();

            for (int i = 0; i < 12; i++)
            {
                mBufMTF[i] = new double[10000];
                mOldSMR[i] = new FAutoLearn.FAutoLearn.sMarkResult();
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        ~MILlib()
        {
            if (bInit == true)
                Free();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void ClearPoints()
        {
            for (int i = 0; i < 10000; i++)
                mC_pDone[i] = false;
        }

        public bool ChangeDataFormat(MIL_INT DigNum, string DataFormat)
        {
            if (milDigitizer != MIL.M_NULL)
            {
                MIL.MdigFree(milDigitizer);
                milDigitizer = MIL.M_NULL;
            }

            MIL.MdigAlloc(milSystem, DigNum, DataFormat, MIL.M_DEFAULT, ref milDigitizer);
            if (milDigitizer == MIL.M_NULL)
            {
                mMatroxMsg = "No More Digitizer";
                return false;
            }

            MIL.MdigControl(milDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
            MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, 990);                    //  500 일때 Best
            MIL.MdigControl(milDigitizer, MIL.M_IO_DEBOUNCE_TIME + MIL.M_AUX_IO8, 255000); //255000

            return true;
        }
        public bool Init(int lVROI, int lHROI, int lVROIstep, int nCamIndex, string SystemName, MIL_INT SystemNum, MIL_INT DigNum, string DataFormat = "M_RS170", bool FlipX = false, bool FlipY = false)
        {
            me++;
            bool bHaveDigitizer = false;

            m_nCam = nCamIndex;

            if (userHookData == null)
                userHookData = new HookDataObject();

            for (int n = 0; n < MAX_DEFAULT_GRAB_COUNT; n++)
            {
                milImageGrab[n] = MIL.M_NULL;
                milImageData[n] = MIL.M_NULL;
            }

            for (int n = 0; n < MAX_TRGGRAB_COUNT; n++)
                milCommonImageGrab[n] = MIL.M_NULL;

            for (int n = 0; n < MAX_TRGGRAB_1000; n++)
            {
                milAFRelay[n] = MIL.M_NULL;
                milXRelay[n] = MIL.M_NULL;
                milYRelay[n] = MIL.M_NULL;
                milXLinRelay[n] = MIL.M_NULL;
                milYLinRelay[n] = MIL.M_NULL;
            }
            for (int n = 0; n < MAX_TRGGRAB_6000; n++)
                milCommonImageGrab6000[n] = MIL.M_NULL;

            //////// Inquire MIL licenses.
            ////////MIL.MsysInquire(milSystem, MIL.M_OWNER_APPLICATION, ref milRemoteApplication);
            ////////MIL.MappInquire(milRemoteApplication, MIL.M_LICENSE_MODULES, ref LicenseModules);

            //MIL.MappAllocDefault(MIL.M_DEFAULT, ref milApplication, ref milSystem, ref milDisplay, ref milDigitizer, MIL.M_NULL);


            if (MIL.MappInquire(MIL.M_DEFAULT, MIL.M_CURRENT_APPLICATION) == MIL.M_NULL)
            {
                MIL.MappAlloc("M_DEFAULT", MIL.M_DEFAULT, ref milApplication);

                if (milApplication == MIL.M_NULL)
                {
                    mMatroxMsg = "Fail to MIL.MappAlloc()";
                    return false;
                }
            }

            if (MIL.MsysAlloc(MIL.M_DEFAULT, SystemName, SystemNum, MIL.M_DEFAULT, ref milSystem) == MIL.M_NULL)
            {
                MessageBox.Show("Check \"Matrox Imaging Adapter\" in Hardware Management Console.");
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C mmc /b devmgmt.msc";
                process.StartInfo = startInfo;
                process.Start();
                mMatroxMsg = "Fail to MIL.MsysAlloc()";
                return false;
            }
            else
            {
                //MessageBox.Show("MsysControl " + DigNum + " milSystem = " + milSystem.ToString());
                MIL.MsysControl(milSystem, MIL.M_MODIFIED_BUFFER_HOOK_MODE, MIL.M_MULTI_THREAD);
            }
            allocatedSysNum++;

            if (SystemName == "M_SYSTEM_HOST" || SystemName == "M_SYSTEM_VGA" || SystemName == "M_SYSTEM_GPU")
                bHaveDigitizer = false;
            else
                bHaveDigitizer = true;

            if (bHaveDigitizer == true)
            {
                MIL.MdigAlloc(milSystem, DigNum, DataFormat, MIL.M_DEFAULT, ref milDigitizer);
                if (milDigitizer == MIL.M_NULL)
                {
                    mMatroxMsg = "No More Digitizer";
                    return false;
                }

                MIL.MdigControl(milDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
                MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, 990);                    //  500 일때 Best
                MIL.MdigControl(milDigitizer, MIL.M_IO_DEBOUNCE_TIME + MIL.M_AUX_IO8, 255000);

                nSizeX = lHROI;  //  Mirror Tilt OIS
                nSizeY = lVROI;
                nSizeYstep = lVROI - 100;
            }

            //for (int n = 0; n < 20; n++)
            //    MIL.MbufAlloc2d(milSystem, nSizeX/3, nSizeY/3, 8, MIL.M_IMAGE + MIL.M_PROC + MIL.M_NON_PAGED, ref milCommonImageResize[n]);

            for (int n = 0; n < MAX_DEFAULT_GRAB_COUNT; n++)
            {
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED, ref milImageGrab[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX / 4, nSizeY / 4, 8, MIL.M_IMAGE + MIL.M_PROC + MIL.M_NON_PAGED, ref milQuaterImg[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_PROC, ref milImageData[n]);
            }

            //for (int n = 0; n < MAX_GRAB_COUNT; n++)
            //{
            //    MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_PROC, ref ZoomImageData[n]);
            //}

            for (int n = 0; n < MAX_TRGGRAB_COUNT; n++)
            {
                MIL.MbufAlloc2d(milSystem, (int)(nSizeX * mModelScale), (int)(nSizeY * mModelScale), 8, MIL.M_IMAGE + MIL.M_PROC + MIL.M_NON_PAGED, ref milCommonImageResize[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milCommonImageGrab[n]);
                if (milCommonImageGrab[n] != MIL.M_NULL)
                    MIL.MbufClear(milCommonImageGrab[n], 0x40);
                else
                    break;
            }
            for (int n = 0; n < MAX_TRGGRAB_1000; n++)
            {
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milAFRelay[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milXRelay[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milYRelay[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milXLinRelay[n]);
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milYLinRelay[n]);
            }
            for (int n = 0; n < MAX_TRGGRAB_6000; n++)
            {
                MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milCommonImageGrab6000[n]);
                if (milCommonImageGrab6000[n] != MIL.M_NULL)
                    MIL.MbufClear(milCommonImageGrab6000[n], 0x40);
                else
                    break;
            }

            MilGrabBufferListSize = MAX_TRGGRAB_COUNT;
            //for (MilGrabBufferListSize = 0; MilGrabBufferListSize < MAX_TRGGRAB_COUNT; MilGrabBufferListSize++)
            //{
            //    MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_NON_PAGED + MIL.M_PROC, ref milCommonImageGrab[MilGrabBufferListSize]);

            //    if (milCommonImageGrab[MilGrabBufferListSize] != MIL.M_NULL)
            //    {
            //        MIL.MbufClear(milCommonImageGrab[MilGrabBufferListSize], 0x40);
            //        //MIL.MbufClear(milCommonImageGrab[MilGrabBufferListSize], 0xFF);
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}




            MIL.MbufAlloc2d(milSystem, nSizeX, nSizeY, 8, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_DISP + MIL.M_PROC, ref milImageDisp);
            MIL.MbufClear(milImageDisp, 0);

            MIL.MdispAlloc(milSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref milDisplay);

            MIL.MdispZoom(milDisplay, dZoomFactorX, dZoomFactorY);
            MIL.MgraControl(MIL.M_DEFAULT, MIL.M_DRAW_ZOOM_X, dZoomFactorX);
            MIL.MgraControl(MIL.M_DEFAULT, MIL.M_DRAW_ZOOM_Y, dZoomFactorY);

            MIL.MthrAlloc(milSystem, MIL.M_EVENT, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL, ref milEvent);

            userHookData.milImageDisp = milImageDisp;
            userHookData.milImageGrab = milImageGrab;
            userHookData.milImageData = milImageData;
            userHookData.milEvent = milEvent;
            userHookData.milGrabNumber = 0;
            userHookData.milGrabIndex = 0;
            userHookData.milExit = 0;
            userHookData.m_nCam = nCamIndex;

            userHookDataHandle = GCHandle.Alloc(userHookData);
            userHookFuncDelegate = new MIL_DIG_HOOK_FUNCTION_PTR(DigHookFunction);
            userThreadDelegate = new MIL_THREAD_FUNCTION_PTR(MyThread__Live);

            MIL.MthrAlloc(milSystem, MIL.M_THREAD, MIL.M_DEFAULT, userThreadDelegate, GCHandle.ToIntPtr(userHookDataHandle), ref milThread);

            bFlipX = FlipX;
            bFlipY = FlipY;
            bInit = true;

            MIL.MappControl(MIL.M_DEFAULT, MIL.M_ERROR, MIL.M_PRINT_DISABLE);




            // Old Style
            //MIL.MblobAllocFeatureList(milSystem, ref MilBlobContext);
            ////MIL.MblobSelectFeature(MilBlobContext, MIL.M_AREA);
            //MIL.MblobSelectFeature(MilBlobContext, MIL.M_CENTER_OF_GRAVITY);
            //MIL.MblobSelectFeature(MilBlobContext, MIL.M_GRAYSCALE);
            //MIL.MblobSelectFeature(MilBlobContext, MIL.M_BOX);
            ////MIL.MblobAllocResult(milSystem, ref MilBlobResult);
            //PrepareBlob();
            ////PrepareBlob();

            //// New Style
            ////MIL.MblobAlloc(milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBlobContext);  // 대체 실패
            ////MIL.MblobControl(MilBlobContext, MIL.M_AREA, MIL.M_ENABLE); //  대체성공 - 의미 없음 삭제해도 됨
            ////MIL.MblobControl(MilBlobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);  //  대체실패
            //MIL.MblobAllocResult(milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBlobResult);   //  대체검증완료

            //MIL.MblobAllocFeatureList(milSystem, ref MilBlobContext);
            //MIL.MblobSelectFeature(MilBlobContext, MIL.M_AREA);
            //MIL.MblobSelectFeature(MilBlobContext, MIL.M_CENTER_OF_GRAVITY);

            //// Allocate a blob result buffer.
            //MIL.MblobAllocResult(milSystem, ref MilBlobResult);
            //PrepareBlob();

            p_Temp = new byte[nSizeX * nSizeY];

            for (int i = 0; i < MAX_THREAD_NUM; i++)
            {
                p_Value[i] = new byte[M_HROI * nSizeY];
            }

            //Max2MinComparer = new myReverserClass();

            SupremeTimer.QueryPerformanceFrequency(ref mTimerFrequency);
            ClearPoints();

            //ReadVisionParam();
            //ReadYtoPXoffset();

            if (!mFAL.LoadLastFMI())
            {
                mMatroxMsg = "Fail to Load LastFMIfile.txt";
                return false;
            }

            mBackgroundNoise = new int[nSizeX];
            mBackgroundNoiseY = new int[nSizeY];

            PrepareFineCOG();
            return true;
        }

        public void ResetModelScale(double scale)
        {
            if (mModelScale == scale)
                return;

            mModelScale = scale;
            for (int n = 0; n < MAX_TRGGRAB_COUNT; n++)
                if (milCommonImageResize[n] != MIL.M_NULL) MIL.MbufFree(milCommonImageResize[n]);

            for (int n = 0; n < MAX_TRGGRAB_COUNT; n++)
                MIL.MbufAlloc2d(milSystem, (int)(nSizeX * mModelScale), (int)(nSizeY * mModelScale), 8, MIL.M_IMAGE + MIL.M_PROC + MIL.M_NON_PAGED, ref milCommonImageResize[n]);

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void Free()
        {
            if (IsLiveA == true)
                HaltA();

            userHookData.milExit = 1;

            // Fire Event
            MIL.MthrControl(userHookData.milEvent, MIL.M_EVENT_SET, MIL.M_SIGNALED);

            if (milThread != MIL.M_NULL)
            {
                MIL.MthrFree(milThread);
                milThread = MIL.M_NULL;
            }

            userHookDataHandle.Free();

            if (milEvent != MIL.M_NULL)
            {
                MIL.MthrFree(milEvent);
                milEvent = MIL.M_NULL;
            }

            if (milImageDisp != MIL.M_NULL) MIL.MbufFree(milImageDisp);

            //for (int n = 0; n < MAX_GRAB_COUNT; n++)
            //{
            //    if (ZoomImageData[n] != MIL.M_NULL) MIL.MbufFree(ZoomImageData[n]);
            //}

            for (int n = 0; n < MAX_TRGGRAB_COUNT; n++)
            {
                if (milCommonImageGrab[n] != MIL.M_NULL) MIL.MbufFree(milCommonImageGrab[n]);
                if (milCommonImageResize[n] != MIL.M_NULL) MIL.MbufFree(milCommonImageResize[n]);
            }

            for (int n = 0; n < MAX_DEFAULT_GRAB_COUNT; n++)
            {
                if (milImageGrab[n] != MIL.M_NULL) MIL.MbufFree(milImageGrab[n]);
                if (milImageData[n] != MIL.M_NULL) MIL.MbufFree(milImageData[n]);
                if (milQuaterImg[n] != MIL.M_NULL) MIL.MbufFree(milQuaterImg[n]);
            }
            for (int n = 0; n < MAX_TRGGRAB_1000; n++)
            {
                if (milAFRelay[n] != MIL.M_NULL) MIL.MbufFree(milAFRelay[n]);
                if (milXRelay[n] != MIL.M_NULL) MIL.MbufFree(milXRelay[n]);
                if (milYRelay[n] != MIL.M_NULL) MIL.MbufFree(milYRelay[n]);
                if (milXLinRelay[n] != MIL.M_NULL) MIL.MbufFree(milXLinRelay[n]);
                if (milYLinRelay[n] != MIL.M_NULL) MIL.MbufFree(milYLinRelay[n]);
            }
            for (int n = 0; n < MAX_TRGGRAB_6000; n++)
            {
                if (milCommonImageGrab6000[n] != MIL.M_NULL) MIL.MbufFree(milCommonImageGrab6000[n]);
            }
            if (milDisplay != MIL.M_NULL)
            {
                MIL.MdispFree(milDisplay);
                milDisplay = MIL.M_NULL;
            }

            if (milDigitizer != MIL.M_NULL)
            {
                MIL.MdigFree(milDigitizer);
                milDigitizer = MIL.M_NULL;
            }

            if (bChildSystem == false)
            {
                if (milSystem != MIL.M_NULL)
                {
                    MIL.MsysFree(milSystem);
                    milSystem = MIL.M_NULL;
                    allocatedSysNum--;
                    me--;
                }
            }

            if (allocatedSysNum == 0)
            {
                if (milApplication != MIL.M_NULL)
                {
                    MIL.MappFree(milApplication);
                    milApplication = MIL.M_NULL;
                }
            }

            //FinishBlob();
            bInit = false;
        }
        //public void ReadYtoPXoffset()
        //{
        //    StreamReader sr = null;
        //    if (File.Exists("YtoPXoffset.txt"))
        //    {
        //        sr = new StreamReader("YtoPXoffset.txt");
        //        string strTmp1 = sr.ReadLine();
        //        string strTmp2 = sr.ReadLine();
        //        sr.Close();
        //        mYtoPXoffset[0] = Convert.ToInt32(strTmp1);
        //        mYtoPXoffset[1] = Convert.ToInt32(strTmp2);
        //    }
        //    else
        //    {
        //        mYtoPXoffset[0] = 1000;
        //        mYtoPXoffset[1] = 1000;
        //    }
        //}
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------   Display Function  ------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void SelectWindow(IntPtr wndHandle)
        {
            MIL.MdispSelectWindow(milDisplay, milImageDisp, wndHandle);

            MIL.MdispControl(milDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
            MIL.MdispInquire(milDisplay, MIL.M_OVERLAY_ID, ref milOverlayImage);
            MIL.MdispInquire(milDisplay, MIL.M_TRANSPARENT_COLOR, ref keyColor);
            MIL.MdispControl(milDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);
            MIL.MdispControl(milDisplay, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);

            MIL.MdispZoom(milDisplay, dZoomFactorX, dZoomFactorY);
            //MIL.MdispPan(milDisplay, 400, 0);   //  그림을 왼쪽으로 400 이동
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DisplayZoom(double dZoomX, double dZoomY)
        {
            if (dZoomX == 0 || dZoomY == 0)
                MIL.MdispControl(milDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE);
            else
            {
                dZoomFactorX = dZoomX;
                dZoomFactorY = dZoomY;
                MIL.MdispZoom(milDisplay, dZoomFactorX, dZoomFactorY);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DisplayFit()
        {
            MIL.MdispControl(milDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------   Overlay  Draw  ---------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawClear()
        {
            MIL.MbufClear(milOverlayImage, keyColor);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------

        public void DrawCross(double Color)
        {
            MIL.MgraColor(MIL.M_DEFAULT, Color);
            MIL.MgraLine(MIL.M_DEFAULT, milOverlayImage, 0, SizeY / 2, SizeX, SizeY / 2);
            MIL.MgraLine(MIL.M_DEFAULT, milOverlayImage, SizeX / 2, 0, SizeX / 2, SizeY);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------   Overlay DC Draw  -------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawSetDCalloc()
        {
            MIL.MbufControl(milOverlayImage, MIL.M_DC_ALLOC, MIL.M_DEFAULT);
            IntPtr hCustomDC = (IntPtr)MIL.MbufInquire(milOverlayImage, MIL.M_DC_HANDLE, MIL.M_NULL);
            //MessageBox.Show(m_nCam.ToString() + " hCustomDC" + hCustomDC.ToString());
            if (!hCustomDC.Equals(IntPtr.Zero))
            {
                gr = Graphics.FromHdc(hCustomDC);
                //MessageBox.Show("Cam = " + m_nCam);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawSetDCfree()
        {
            if (gr != null)
                gr.Dispose();
            MIL.MbufControl(milOverlayImage, MIL.M_DC_FREE, MIL.M_DEFAULT);
            MIL.MbufControl(milOverlayImage, MIL.M_MODIFIED, MIL.M_DEFAULT);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawDCCross(Brush color)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                Pen pen = new Pen(color, 1);
                gr.DrawLine(pen, 0, SizeY / 2, SizeX, SizeY / 2);   //  시작점 - 끝점
                gr.DrawLine(pen, SizeX / 2, 0, SizeX / 2, SizeY);
                //gr.DrawLine(pen, 1380, 0, 1380, SizeY);
            }

            DrawSetDCfree();
        }

        public void DrawCSHCross(Brush color)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                Pen pen = new Pen(color, 1);
                //gr.DrawLine(pen, M_HROI / 2 - 5, SizeY / 2, M_HROI / 2 + 5, SizeY / 2);   //  시작점 - 끝점
                //gr.DrawLine(pen, SizeX / 2, SizeY / 2 - 5, SizeX / 2, SizeY / 2 + 5);

                //gr.DrawLine(pen, 65, 0, 65, SizeY);
                //gr.DrawLine(pen, SizeX - 65, 0, SizeX - 65, SizeY);

                Pen penS = new Pen(Brushes.Aqua, 1);

                ///////////////////////////////////////////////////////////////////////
                //  Draw Reference Line for Image Sensor PCB
                //gr.DrawLine(penS, M_HROI / 2 - 5, SizeY / 2 - 12, M_HROI / 2 + 5, SizeY / 2 - 12);   //  시작점 - 끝점
                //gr.DrawLine(penS, SizeX / 2, SizeY / 2 - 17, SizeX / 2, SizeY / 2 - 7);

                //gr.DrawLine(pen, 0, SizeY/2 - 40, SizeX / 2 - 130 , SizeY/2 - 40);   //  시작점 - 끝점
                //gr.DrawLine(pen, 0, SizeY/2 - 25, SizeX / 2 - 145 , SizeY/2 - 25);   //  시작점 - 끝점
                //gr.DrawLine(pen, 0, SizeY/2 - 10, SizeX / 2 - 160 , SizeY/2 - 10);   //  시작점 - 끝점

                //gr.DrawLine(pen, SizeX / 2 - 130 , SizeY/2 - 40, SizeX / 2 - 130, nSizeY);
                //gr.DrawLine(pen, SizeX / 2 - 145 , SizeY/2 - 25, SizeX / 2 - 145, nSizeY);
                //gr.DrawLine(pen, SizeX / 2 - 160 , SizeY/2 - 10, SizeX / 2 - 160, nSizeY);

                //gr.DrawLine(pen, SizeX / 2 + 134 , SizeY/2 - 54, SizeX / 2 + 134, nSizeY);   //  시작점 - 끝점
                //gr.DrawLine(pen, SizeX / 2 + 149 , SizeY/2 - 39, SizeX / 2 + 149, nSizeY);   //  시작점 - 끝점
                //gr.DrawLine(pen, SizeX / 2 + 164 , SizeY/2 - 24, SizeX / 2 + 164, nSizeY);   //  시작점 - 끝점

                //gr.DrawLine(pen, SizeX / 2 + 134 , SizeY/2 - 54, nSizeX, SizeY/2 - 54);
                //gr.DrawLine(pen, SizeX / 2 + 149 , SizeY/2 - 39, nSizeX, SizeY/2 - 39);
                //gr.DrawLine(pen, SizeX / 2 + 164 , SizeY/2 - 24, nSizeX, SizeY/2 - 24);

                ///////////////////////////////////////////////////////////////////////
                //  Draw Reference Line for Dummy Lens

                int HalfHwnd = 195;
                int yCenter = SizeY / 2 - 30;

                gr.DrawLine(pen, 0, yCenter - 32, SizeX / 2 - HalfHwnd + 20, yCenter - 32);   //  시작점 - 끝점
                gr.DrawLine(pen, 0, yCenter - 0, SizeX / 2 - HalfHwnd, yCenter - 0);   //  시작점 - 끝점
                gr.DrawLine(pen, 0, yCenter + 32, SizeX / 2 - HalfHwnd - 20, yCenter + 32);   //  시작점 - 끝점

                gr.DrawLine(pen, SizeX / 2 - HalfHwnd + 20, yCenter - 32, SizeX / 2 - HalfHwnd + 20, nSizeY);
                gr.DrawLine(pen, SizeX / 2 - HalfHwnd, yCenter - 0, SizeX / 2 - HalfHwnd, nSizeY);
                gr.DrawLine(pen, SizeX / 2 - HalfHwnd - 20, yCenter + 32, SizeX / 2 - HalfHwnd - 20, nSizeY);

                gr.DrawLine(pen, SizeX / 2 + HalfHwnd - 20, yCenter - 32, SizeX / 2 + HalfHwnd - 20, nSizeY);   //  시작점 - 끝점
                gr.DrawLine(pen, SizeX / 2 + HalfHwnd, yCenter - 0, SizeX / 2 + HalfHwnd, nSizeY);   //  시작점 - 끝점
                gr.DrawLine(pen, SizeX / 2 + HalfHwnd + 20, yCenter + 32, SizeX / 2 + HalfHwnd + 20, nSizeY);   //  시작점 - 끝점

                gr.DrawLine(pen, SizeX / 2 + HalfHwnd - 20, yCenter - 32, nSizeX, yCenter - 32);
                gr.DrawLine(pen, SizeX / 2 + HalfHwnd, yCenter - 0, nSizeX, yCenter - 0);
                gr.DrawLine(pen, SizeX / 2 + HalfHwnd + 20, yCenter + 32, nSizeX, yCenter + 32);
            }

            DrawSetDCfree();
        }

        public void DrawMarkPos(Brush color, System.Drawing.Point[] p)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                int x = 0;
                int y = 0;
                Pen pen = new Pen(color, 1);
                for (int i = 0; i < p.Length; i++)
                {
                    //  그림그릴 때 CropGap 적용
                    //  mark 0, 1 은 위로 CropABgap/2 만큼 이동
                    //  mark 2 은 아래로 CropABgap/2 만큼 이동
                    //  mark 3 은 우측으로 CropCgap/2 만큼 이동
                    //  mark 4 은 좌측으로 CropCgap/2 만큼 이동
                    x = nSizeX / 2 + p[i].X;
                    y = nSizeY / 2 + p[i].Y;
                    switch (i)
                    {
                        case 0:
                        case 1:
                            y -= CropABgap / 2 - 3;// CropABgap / 2;
                            break;
                        case 2:
                            y += CropABgap / 2 - 3;// CropABgap / 2;
                            break;
                        case 3:
                            x += (500 - CropCgap) / 2;
                            //y += 30;
                            break;
                        case 4:
                            x -= (500 - CropCgap) / 2;
                            //y += 30;
                            break;
                    }


                    gr.DrawLine(pen, x - 5, y, x + 5, y);   //  시작점 - 끝점
                    gr.DrawLine(pen, x, y - 5, x, y + 5);
                }
            }

            DrawSetDCfree();
        }
        public void SetStdMarkPos(System.Drawing.Point[] p, ref System.Drawing.Point[] res, int w = -1, int h = -1)
        {
            //  CropGap 을 적용한다.
            mFAL.mMarkPosOnPanel = new System.Drawing.Point[p.Length];
            mFAL.mMarkConvOnPanel = new long[p.Length];

            int cx = nSizeX / 2;
            int cy = nSizeY / 2;
            if (w > 0 && h > 0)
            {
                cx = w / 2;
                cy = h / 2;
            }
            for (int i = 0; i < p.Length; i++)
            {
                res[i].X = cx + p[i].X;
                res[i].Y = cy + p[i].Y;
                switch (i)
                {
                    case 0:
                    case 1:
                        res[i].Y -= CropABgap / 2 - 4; //-3;    // CropABgap / 2;
                        break;
                    case 2:
                        res[i].Y += CropABgap / 2 - 26; //-27;  // CropABgap / 2 - 23;
                        break;
                    case 3:
                        res[i].X += (520 - CropCgap) / 2;
                        //res[i].Y += 26;
                        break;
                    case 4:
                        res[i].X -= (520 - CropCgap) / 2;
                        //res[i].Y += 26;
                        break;
                }
                mFAL.mMarkPosOnPanel[i].X = res[i].X;
                mFAL.mMarkPosOnPanel[i].Y = res[i].Y;
            }
        }

        public void DrawDC_Circle(Brush color, int radius)  // DrawCircle khkim_170920
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                Pen pen = new Pen(color, 1);

                gr.DrawEllipse(pen, SizeX / 2 - radius / 2, SizeY / 2 - radius / 2, radius, radius);   //  x 중심, y 중심, 반경, 반경
            }
            DrawSetDCfree();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawDCCircle(System.Drawing.Point p1, int radius, Brush color)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                Pen pen = new Pen(color, 1);
                gr.DrawEllipse(pen, p1.X, p1.Y, radius, radius);
            }

            DrawSetDCfree();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawDCLine(System.Drawing.Point p1, System.Drawing.Point p2, Brush color)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                Pen pen = new Pen(color, 1);
                gr.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
            }

            DrawSetDCfree();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawDCBox(System.Drawing.Point leftTop, System.Drawing.Point rightBottom, Brush color)
        {

            int lenX = rightBottom.X - leftTop.X;
            int lenY = rightBottom.Y - leftTop.Y;

            if (rightBottom.X > nSizeX - 1) return;
            if (rightBottom.Y > nSizeY - 1) return;
            if (leftTop.X < 0) return;
            if (leftTop.Y < 0) return;

            DrawSetDCalloc();

            try
            {
                if (gr != null)
                {
                    Pen pen = new Pen(color, 1);
                    gr.DrawLine(pen, leftTop.X, leftTop.Y, leftTop.X + lenX, leftTop.Y);           //  ----------
                    gr.DrawLine(pen, leftTop.X + lenX, leftTop.Y, leftTop.X + lenX, rightBottom.Y);           //            |
                    gr.DrawLine(pen, leftTop.X, rightBottom.Y, leftTop.X + lenX, rightBottom.Y);           //  ----------
                    gr.DrawLine(pen, leftTop.X, leftTop.Y, leftTop.X, leftTop.Y + lenY);           //  |
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString() + "\r\n" + leftTop.X.ToString() + " " + leftTop.Y.ToString() + " " + rightBottom.X.ToString() + " " + rightBottom.Y.ToString());
            }

            DrawSetDCfree();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DrawDCText(int xPos, int yPos, string Text, Brush color)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                System.Drawing.Point pt = new System.Drawing.Point(xPos, yPos);
                gr.DrawString(Text, fontData, color, pt);
            }

            DrawSetDCfree();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------   Basic  Function   -----------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void LiveA()
        {
            if (IsLiveA == true)
                HaltA();

            bLiveA = true;

            MIL.MdigGrabContinuous(milDigitizer, milImageDisp);
        }

        #region Crop Img

        // camID
        private string CamID = "";
        // 이미지 사이즈
        private readonly Size srcSize = new Size(1800, 342);
        private readonly Size resultSize = new Size(Global.mMergeImgWidth, Global.mMergeImgHeight);
        // 이미지 3부분으로 구분 Rect
        //private readonly Rect[] mSplitRect = new Rect[3];
        // Crop Rect
        public Rect[] mSrcCropRect = new Rect[4];
        private readonly Rect[] resultRoiRect = new Rect[4];

        public int CropABgap = 0;
        public int CropCgap
        {
            get
            {
                if (mSrcCropRect[3] != null && mSrcCropRect[2] != null)
                    return mSrcCropRect[3].X - mSrcCropRect[2].X;
                else return 0;
            }
        }
        public void InitCrop(bool initSrcRoiPos)
        {
            int center = srcSize.Width / 2 + 90;
            int spliteSize = 540;

            // src 3파트로 구분
            //mSplitRect[0] = new Rect((int)(900 - 1.5 * spliteSize), 0, spliteSize, srcSize.Height);
            //mSplitRect[1] = new Rect((int)(900 - 0.5 * spliteSize), 0, spliteSize, srcSize.Height);
            //mSplitRect[2] = new Rect((int)(900 + 0.5 * spliteSize), 0, spliteSize, srcSize.Height);

            Size[] roiSize = new Size[]
            {
                    //new Size(540, 190), // A   
                    //new Size(250, 190), // B   
                    //new Size(250, 250), // C   
                    //new Size(250, 250)  // D   
                    new Size(550, 190), // Side
                    new Size(260, 190), // Side East
                    new Size(260, 260), // Top Left
                    new Size(260, 260)  // Top Right
            };

            // srcImg ROI 설정
            if (initSrcRoiPos)
            {
                Point[] srcRoiPos = new Point[4];

                // Src이미지에서 ABCD 위치
                //srcRoiPos[0] = new Point(center - roiSize[0].Width * 0.5, 0); // B // side
                //srcRoiPos[1] = new Point(center - spliteSize * 1.5 + (spliteSize - roiSize[1].Width) * 0.5,
                //                             srcSize.Height - roiSize[1].Height); // A // side 
                //srcRoiPos[2] = new Point(center + spliteSize - 40 - roiSize[2].Width, 0);   // C   // top           // C AND D간격 80
                //srcRoiPos[3] = new Point(center + spliteSize + 40, 0);   // D   // top
                srcRoiPos[0] = new Point(srcSize.Width / 3, 0); // Side
                srcRoiPos[1] = new Point(0, srcSize.Height - roiSize[1].Height); // Side East
                srcRoiPos[2] = new Point((srcSize.Width / 3) * 2, 0);   // Top Left
                srcRoiPos[3] = new Point(srcSize.Width - roiSize[3].Width, 0); // Top Right

                for (int i = 0; i < 4; i++)
                {
                    mSrcCropRect[i] = new Rect(srcRoiPos[i].X, srcRoiPos[i].Y, roiSize[i].Width, roiSize[i].Height);
                }

            };

            // cropImg ROI 설정
            Point[] resultRoiPos = new Point[4];

            // Crop이미지에서 ABCD 위치
            resultRoiPos[0] = new Point((resultSize.Width - roiSize[0].Width) * 0.5, 0);    // Side
            resultRoiPos[1] = new Point(roiSize[2].Width, roiSize[0].Height);       //Side East
            resultRoiPos[2] = new Point(0, roiSize[0].Height);          //Top Left
            resultRoiPos[3] = new Point(resultRoiPos[1].X + roiSize[1].Width, roiSize[0].Height);   //Top Right

            for (int i = 0; i < 4; i++)
            {
                resultRoiRect[i] = new Rect(resultRoiPos[i].X, resultRoiPos[i].Y, roiSize[i].Width, roiSize[i].Height);
            }

            SaveCropPosToXml();
        }

        public bool mbDrawReference = false;
        public Mat LoadCropImg(int index)
        {
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[index]);    //
            MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END);
            //MIL.MbufExport("D:\\TestImage.bmp", MIL.M_BMP, milCommonImageGrab[index]);
            byte[] buf = new byte[nSizeX * nSizeY];
            MIL.MbufCopy(milImageDisp, milCommonImageGrab[index]);
            MIL.MbufGet2d(milCommonImageGrab[index], 0, 0, nSizeX, nSizeY, buf);

            Mat src = new Mat(nSizeY, nSizeX, MatType.CV_8UC1, buf);
            //Cv2.ImWrite("D:\\CVImage.bmp", src);
            Mat cropImg = CropImage(src);

            if (mbDrawReference)
            {
                Mat lOverlayedImg = new Mat();
                Cv2.CvtColor(cropImg, lOverlayedImg, ColorConversionCodes.GRAY2RGB);

                for (int i = 0; i < 5; i++)
                {
                    int x = (int)mFAL.mMarkPosOnPanel[i].X;
                    int y = (int)mFAL.mMarkPosOnPanel[i].Y;
                    Cv2.Line(lOverlayedImg, x - 10, y, x + 10, y, Scalar.OrangeRed, 1, LineTypes.Link4);
                    Cv2.Line(lOverlayedImg, x, y - 10, x, y + 10, Scalar.OrangeRed, 1, LineTypes.Link4);
                }
                //return BitmapConverter.ToBitmap(lOverlayedImg);
                return lOverlayedImg;
            }
            else
                //return BitmapConverter.ToBitmap(cropImg);
                return cropImg;
        }
        public Mat LoadCropMat(int index)
        {
            byte[] buf = new byte[nSizeX * nSizeY];
            MIL.MbufCopy(milImageDisp, milCommonImageGrab[index]);


            MIL.MbufGet2d(milCommonImageGrab[index], 0, 0, nSizeX, nSizeY, buf);

            Mat src = new Mat(nSizeY, nSizeX, MatType.CV_8UC1, buf);
            Mat cropImg = CropImage(src);

            return cropImg;
        }
        public void DrawAllRectangles()
        {
            DrawClear();
            //DrawRectangles(Brushes.DimGray, mSplitRect);
            DrawRectangles(Brushes.Lime, mSrcCropRect);
        }
        private void DrawRectangles(Brush color, Rect[] rects)
        {
            DrawSetDCalloc();

            if (gr != null)
            {
                Pen pen = new Pen(color, 2);
                Rectangle[] rectangles = new Rectangle[rects.Length];

                for (int i = 0; i < rects.Length; i++)
                {
                    rectangles[i] = new Rectangle(rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);
                }
                gr.DrawRectangles(pen, rectangles);
            }
            DrawSetDCfree();
        }

        private Mat CropImage(Mat src)
        {
            Mat result = new Mat(resultSize, MatType.CV_8UC1, new Scalar(0));

            for (int i = 0; i < 4; i++)
            {
                using (Mat srcRoi = src.SubMat(mSrcCropRect[i]))
                using (Mat resultRoi = result.SubMat(resultRoiRect[i]))
                {
                    srcRoi.CopyTo(resultRoi);
                }
            }
            return result;
        }

        private void CropImage(int index, int nbuf)
        {
            byte[] buf = new byte[nSizeX * nSizeY];
            // MIL 에서 Mat 로 변환한 뒤 Merge
            MIL.MbufGet2d(milCommonImageGrab[index], 0, 0, nSizeX, nSizeY, buf);
            //if ( index==0)
            //    MIL.MbufExport("C:\\CSHTest\\Result\\RawData\\Image\\Crop.bmp", MIL.M_BMP, milCommonImageGrab[index]);

            Mat src = new Mat(nSizeY, nSizeX, MatType.CV_8UC1, buf);
            mFAL.mSourceImg[nbuf].SetTo(new Scalar(0));
            for (int i = 0; i < 4; i++)
            {
                using (Mat srcRoi = src.SubMat(mSrcCropRect[i]))
                using (Mat resultRoi = mFAL.mSourceImg[nbuf].SubMat(resultRoiRect[i]))
                {
                    srcRoi.CopyTo(resultRoi);
                }
            }
        }
        private void CropImage(int nbuf, byte[] buf)
        {
            Mat src = new Mat(nSizeY, nSizeX, MatType.CV_8UC1, buf);
            for (int i = 0; i < 4; i++)
            {
                using (Mat srcRoi = src.SubMat(mSrcCropRect[i]))
                using (Mat resultRoi = mFAL.mSourceImg[nbuf].SubMat(resultRoiRect[i]))
                {
                    srcRoi.CopyTo(resultRoi);
                }
            }
        }

        // Rect 좌표 이미지 벗어나지않게 
        private void EnsureValidRect(int i)
        {
            if (mSrcCropRect[i].X + mSrcCropRect[i].Width > srcSize.Width) mSrcCropRect[i].X = srcSize.Width - mSrcCropRect[i].Width;
            else if (mSrcCropRect[i].X < 0) mSrcCropRect[i].X = 0;

            if (mSrcCropRect[i].Y + mSrcCropRect[i].Height > srcSize.Height) mSrcCropRect[i].Y = srcSize.Height - mSrcCropRect[i].Height;
            else if (mSrcCropRect[i].Y < 0) mSrcCropRect[i].Y = 0;
        }
        // Crop Rect Move
        public void UpPos(int posIdx, int step)
        {
            mSrcCropRect[posIdx].Y -= step;
            EnsureValidRect(posIdx);

            if (posIdx == 2)
            {
                mSrcCropRect[3].Y -= step;
                EnsureValidRect(3);
            }

            SaveCropPosToXml();
            //LoadCropImg(0);
            DrawAllRectangles();
        }
        public void DownPos(int posIdx, int step)
        {
            mSrcCropRect[posIdx].Y += step;
            EnsureValidRect(posIdx);

            if (posIdx == 2)
            {
                mSrcCropRect[3].Y += step;
                EnsureValidRect(3);
            }

            SaveCropPosToXml();
            //LoadCropImg(0);
            DrawAllRectangles();

        }
        public void RightPos(int posIdx, int step)
        {
            mSrcCropRect[posIdx].X += step;
            EnsureValidRect(posIdx);

            if (posIdx == 2)
            {
                mSrcCropRect[3].X += step;
                EnsureValidRect(3);
            }

            SaveCropPosToXml();
            //LoadCropImg(0);
            DrawAllRectangles();

        }
        public void LeftPos(int posIdx, int step)
        {
            mSrcCropRect[posIdx].X -= step;
            EnsureValidRect(posIdx);

            if (posIdx == 2)
            {
                mSrcCropRect[3].X -= step;
                EnsureValidRect(3);
            }

            SaveCropPosToXml();
            //LoadCropImg(0);
            DrawAllRectangles();

        }
        public void AdjustDistancePos(int newDistance)
        {
            int step = newDistance - CropCgap;
            var half = step * 0.5;
            int dxC;
            int dxD;

            if (half > 0)
            {
                dxC = (int)Math.Floor(half);
                dxD = (int)Math.Ceiling(half);
            }
            else
            {
                dxC = (int)Math.Ceiling(half);
                dxD = (int)Math.Floor(half);
            }

            mSrcCropRect[2].X -= dxC;
            mSrcCropRect[3].X += dxD;

            // 결과 위치가 이미지를 넘어갈 때 거리 다시 처리
            if (mSrcCropRect[2].X > srcSize.Width - mSrcCropRect[2].Width)
            {
                mSrcCropRect[2].X = srcSize.Width - mSrcCropRect[2].Width;
                mSrcCropRect[3].X = srcSize.Width - newDistance - mSrcCropRect[3].Width;
            }
            else if (mSrcCropRect[2].X < 0)
            {
                mSrcCropRect[2].X = 0;
                mSrcCropRect[3].X = newDistance;
            }
            else if (mSrcCropRect[3].X > srcSize.Width - mSrcCropRect[3].Width)
            {
                mSrcCropRect[3].X = srcSize.Width - mSrcCropRect[3].Width;
                mSrcCropRect[2].X = srcSize.Width - newDistance - mSrcCropRect[2].Width;
            }
            else if (mSrcCropRect[3].X < 0)
            {
                mSrcCropRect[3].X = 0;
                mSrcCropRect[2].X = newDistance;
            }

            if (mSrcCropRect[2].X < 0 && mSrcCropRect[3].X > srcSize.Width - mSrcCropRect[3].Width
                || mSrcCropRect[3].X < 0 && mSrcCropRect[2].X > srcSize.Width - mSrcCropRect[2].Width)
            {
                EnsureValidRect(2);
                EnsureValidRect(3);
            }
            SaveCropPosToXml();
            //LoadCropImg(0);
            DrawAllRectangles();
        }
        public void NarrowPos(int step)
        {
            AdjustDistancePos(CropCgap - step);
        }
        public void WidenPos(int step)
        {
            AdjustDistancePos(CropCgap + step);
            //mSrcCropRect[2].X -= step;
            //mSrcCropRect[3].X += step;
            //EnsureValidRect(2);
            //EnsureValidRect(3);
            //SaveCropPosToXml();
            //LoadCropImg(0);
            //DrawAllRectangles();
        }


        public void SaveCropPosToXml()
        {
            CropABgap = mSrcCropRect[0].Bottom - mSrcCropRect[1].Top;
            mFAL.SetCropGaps(CropABgap, CropCgap);
            string filePath = "C:\\B7WideTest\\DoNotTouch\\";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string fileName = $"CropPos{CamID}.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(Rect[]));
            using (StreamWriter writer = new StreamWriter(filePath + fileName))
            {
                serializer.Serialize(writer, mSrcCropRect);
            }
        }

        public bool LoadCropPosFromXml(string camID)
        {
            CamID = camID;
            string filePath = $"C:\\B7WideTest\\DoNotTouch\\CropPos{CamID}.xml";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Rect[]));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    mSrcCropRect = (Rect[])serializer.Deserialize(reader);
                    int rightMost = 0;
                    for (int i = 0; i < mSrcCropRect.Length; i++)
                    {
                        if (rightMost < mSrcCropRect[i].Right)
                            rightMost = mSrcCropRect[i].Right;
                    }
                    if (rightMost >= M_HROI)
                    {
                        int toLeftby = rightMost - M_HROI;
                        for (int i = 0; i < mSrcCropRect.Length; i++)
                            mSrcCropRect[i].Left -= toLeftby;
                    }
                }
                CropABgap = mSrcCropRect[1].Top - mSrcCropRect[0].Bottom;
                mFAL.SetCropGaps(CropABgap, CropCgap);
                return true;
            }
            catch { return false; }
        }

        #endregion

        public void CopyFromLive(int i = 1)
        {
            MIL.MbufCopy(milImageDisp, milCommonImageGrab[i]);
            MIL.MimResize(milCommonImageGrab[i], milCommonImageResize[i], mModelScale, mModelScale, MIL.M_DEFAULT);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DispCommonImage(int i = 0)
        {
            MIL.MbufCopy(milCommonImageGrab[i], milImageDisp);
        }

        public void SaveGrabbedImage(int index, string FullbmpName)
        {
            Mat img = LoadCropMat(index);
            img.SaveImage(FullbmpName);

            byte[] data = null;
            img.GetArray(out data);
            StreamWriter wr = new StreamWriter("raw" + index.ToString() + ".csv");
            for (int i = 0; i < data.Length; i++)
                wr.WriteLine(data[i].ToString());
            wr.Close();

        }
        public void SaveCompressedImage(int index, string FullbmpName)
        {
            try
            {
                MIL.MbufExport(FullbmpName, MIL.M_BMP, milCommonImageResize[index]);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void GrabA(int i = 0)
        {
            if (IsLiveA == true)
            {
                HaltA();
                Thread.Sleep(1);
            }

            while (OnGrab)
            {
                Thread.Sleep(1);
            }
            OnGrab = true;

            //MIL.MbufClear(milImageDisp, 0);  //
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[i]);    //
            MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END); //   카메라 없을 경우 에러남. 에러 처리 필요. 

            //MIL.MbufCopy(milCommonImageGrab[i], milImageDisp);

            OnGrab = false;
            //            MIL.MbufCopy(milXstepImageGrab[0], milImageDisp);
        }
        public void GrabB(int i = 0, bool autoTest = false)
        {
            if (IsLiveA == true)
            {
                HaltA();
                Thread.Sleep(1);
            }

            while (OnGrab)
            {
                Thread.Sleep(1);
            }
            OnGrab = true;

            MIL.MdigGrab(milDigitizer, milCommonImageGrab[i]);    //
            MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END); //   카메라 없을 경우 에러남. 에러 처리 필요. 
            //MIL.MimResize(milCommonImageGrab[i], milCommonImageResize[i], mModelScale, mModelScale, MIL.M_DEFAULT);
            MIL.MbufCopy(milCommonImageGrab[i], milImageDisp);

            OnGrab = false;
            //            MIL.MbufCopy(milXstepImageGrab[0], milImageDisp);
        }
        public void GrabC(int i = 0)
        {
            if (IsLiveA == true)
            {
                HaltA();
                Thread.Sleep(1);
            }

            while (OnGrab)
            {
                Thread.Sleep(1);
            }
            OnGrab = true;

            //MIL.MbufClear(milImageDisp, 0);  //
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[i]);    //
            //MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END); //   카메라 없을 경우 에러남. 에러 처리 필요. 

            OnGrab = false;
            //            MIL.MbufCopy(milXstepImageGrab[0], milImageDisp);
        }
        public void GrabD(int i = 0)
        {
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[i]);    //
        }
        public void ResizeImgs(int ifrom, int iNum)
        {
            for (int i = ifrom; i < ifrom + iNum; i++)
            {
                MIL.MbufCopy(milCommonImageGrab[i], milImageDisp);
                MIL.MimResize(milCommonImageGrab[i], milCommonImageResize[i], mModelScale, mModelScale, MIL.M_DEFAULT);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void HaltA()
        {
            bLiveA = false;

            MIL.MdigHalt(milDigitizer);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        // 0 = Infinite, 1~ Fixed Grab
        public void Grab_Process(int nNumber)
        {
            if (IsLive == true)
                Halt_Process();

            if (nNumber == 0)
                bLive = true;

            userHookData.milGrabIndex = 0;
            userHookData.milGrabNumber = nNumber;

            MIL.MdigProcess(milDigitizer, milImageGrab, MAX_DEFAULT_GRAB_COUNT, MIL.M_START, MIL.M_ASYNCHRONOUS, userHookFuncDelegate, GCHandle.ToIntPtr(userHookDataHandle));
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void Halt_Process()
        {
            bLive = false;
            MIL.MdigProcess(milDigitizer, milImageGrab, MAX_DEFAULT_GRAB_COUNT, MIL.M_STOP, MIL.M_DEFAULT, userHookFuncDelegate, GCHandle.ToIntPtr(userHookDataHandle));
            MIL.MdigControl(milDigitizer, MIL.M_GRAB_ABORT, MIL.M_DEFAULT);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void GrabA_User(int index)
        {
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[index]);
            MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END);
            SupremeTimer.QueryPerformanceCounter(ref GrabT1[index]);
        }
        public void GrabB_User(int index)
        {
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[index]);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void GrabA_UserWait()
        {
            MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------c
        MIL_INT DigHookFunction(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        {
            GCHandle HookDataHandle = GCHandle.FromIntPtr(HookDataPtr);
            HookDataObject UserHookDataPtr = HookDataHandle.Target as HookDataObject;

            MIL_ID ModifiedBufferId = MIL.M_NULL;

            MIL.MdigGetHookInfo(HookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref ModifiedBufferId);

            if (ModifiedBufferId != MIL.M_NULL)
            {
                // Fire Event
                MIL.MthrControl(UserHookDataPtr.milEvent, MIL.M_EVENT_SET, MIL.M_SIGNALED);
            }

            if (UserHookDataPtr.milGrabNumber > 0 && userHookData.milGrabIndex >= userHookData.milGrabNumber)
            {
                MIL.MdigProcess(milDigitizer, milImageGrab, MAX_DEFAULT_GRAB_COUNT, MIL.M_STOP, MIL.M_DEFAULT, userHookFuncDelegate, GCHandle.ToIntPtr(userHookDataHandle));
            }

            return 0;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        static uint MyThread__Live(IntPtr ThreadParameters)
        {
            GCHandle threadParamHandle = GCHandle.FromIntPtr(ThreadParameters);
            HookDataObject TPar = threadParamHandle.Target as HookDataObject;

            while (TPar.milExit == 0)
            {
                // Wait for the Event
                MIL.MthrWait(TPar.milEvent, MIL.M_EVENT_WAIT);

                if (TPar.milExit == 1)
                    return 1;

                if (TPar.milGrabNumber == 0)    // Live
                {
                    MIL.MbufCopy(TPar.milImageGrab[TPar.milGrabIndex % MAX_GRAB_COUNT], TPar.milImageDisp);
                    //MIL.MbufCopyClip(TPar.milImageGrab[TPar.milGrabIndex % MAX_GRAB_COUNT], TPar.milImageDisp, -512, 0);

                }
                else                            // Record
                {
                    //MessageBox.Show("Here I am");
                    //MIL.MbufCopy(TPar.milImageGrab[TPar.milGrabIndex % MAX_GRAB_COUNT], TPar.milImageData[TPar.milGrabIndex]);
                    //MIL.MbufCopyClip(TPar.milImageGrab[TPar.milGrabIndex % MAX_GRAB_COUNT], TPar.milImageData[TPar.milGrabIndex], -512, 0);

                }
                TPar.milGrabIndex++;
            }

            return 1;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void ShowBalancingImage()
        {
            MIL.MbufGet2d(milCommonImageGrab[0], 0, 0, nSizeX, nSizeY, p_Value[0]);
            int edgeBin = p_Value[0][nSizeX / 4 - 1 + (nSizeY / 4) * 3 * nSizeX];
            for (int x = 0; x < nSizeX; x++)
            {
                for (int y = nSizeY / 2; y < nSizeY; y++)
                {
                    if (p_Value[0][x + y * nSizeX] == edgeBin)
                        p_Value[0][x + y * nSizeX] = 255;
                }
            }
            MIL.MbufPut2d(milCommonImageGrab[0], 0, 0, nSizeX, nSizeY, p_Value[0]);
            MIL.MbufCopy(milCommonImageGrab[0], milImageDisp);
            string FullbmpName = "C:\\B7WideTest\\Result\\RawData\\Image\\Balancing.bmp";

            MIL.MbufExport(FullbmpName, MIL.M_BMP, milCommonImageGrab[0]);
        }
        public void SaveImageBuf(string strFileName, int index = 1)
        {
            if (index >= 0)
                MIL.MbufExport(strFileName, MIL.M_BMP, milCommonImageGrab[index]);
            //MIL.MbufSave(strFileName, milCommonImageGrab[index]);
            else
                MIL.MbufExport(strFileName, MIL.M_BMP, milImageDisp);
            //MIL.MbufSave(strFileName, milImageDisp);

            //MIL_ID resMilID = MIL.M_NULL;

            //resMilID = MIL.MbufImport(strFileName, MIL.M_BMP, MIL.M_RESTORE, milSystem, ref milCommonImageGrab[0]);
            //MIL.MbufGet2d(milCommonImageGrab[0], 0, 0, nSizeX, nSizeY, p_Value[0]);

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void BufCopy2Disp_TRG(int nIndex)
        {
            //MIL.MbufCopy(OISXImageData[nIndex], milImageDisp);
            MIL.MbufCopy(milCommonImageGrab[nIndex], milImageDisp);
        }
        ////------------------------------------------------------------------------------------------------------------------------------------------------
        //public void BufCopy2Disp_ZMStep(int nIndex)
        //{
        //    //MIL.MbufCopy(ZoomstepImageData[nIndex], milImageDisp);
        //    //MIL.MbufCopy(ZoomstepImageData[nIndex], milCommonImageGrab[nIndex]);
        //}
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void BufCopy2Disp_XStep(int nIndex)
        {
            //MIL.MbufCopy(AFstepImageData[nIndex], milImageDisp);
            //MIL.MbufCopy(AFstepImageData[nIndex], milCommonImageGrab[nIndex]);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void BufCopy2Disp_OISX(int nIndex)
        {
            MIL.MbufCopy(milCommonImageGrab[nIndex], milImageDisp);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------         Pattern Matching Function         ---------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void ClearDisp()
        {
            MIL.MbufClear(milImageDisp, 64);  //  여기 정상
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //public void PrepareBlob()
        //{
        //    MIL.MbufInquire(milImageDisp, MIL.M_SIZE_X, ref mBlobSizeX);
        //    MIL.MbufInquire(milImageDisp, MIL.M_SIZE_Y, ref mBlobSizeY);
        //    MIL.MbufAlloc2d(milSystem, mBlobSizeX, mBlobSizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MilBinImage);
        //}
        //public void FinishBlob()
        //{
        //    MIL.MbufFree(MilBinImage);
        //}
        //public bool FindOpenArea(ref double[] sA, ref double[] cx, ref double[] cy, ref long Nfound, long thresh, int threshwidth)
        //{
        //    int k = 1;
        //    double numOverlap = 2 * threshwidth + 1;
        //    double[] _sA = new double[4];
        //    double[] _cx = new double[4];
        //    double[] _cy = new double[4];

        //    for (int j = 0; j < 6; j++)
        //    {
        //        _sA[0] = 0;
        //        _cx[0] = 0;
        //        _cy[0] = 0;
        //    }
        //    for (int stepThresh = -threshwidth; stepThresh <= threshwidth; stepThresh++)
        //    {
        //        // Binarize image.
        //        MIL.MimBinarize(milImageDisp, MilBinImage, MIL.M_FIXED + MIL.M_GREATER_OR_EQUAL, thresh + stepThresh, MIL.M_NULL);
        //        //MIL.MimBinarize(milImageDisp, MilBinImage, MIL.M_FIXED + MIL.M_GREATER_OR_EQUAL, 30, MIL.M_NULL);

        //        MIL.MblobCalculate(MilBinImage, MIL.M_NULL, MilBlobContext, MilBlobResult);
        //        // Exclude blobs whose area is too small.
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TotalBlobs);    //  원래 돌아가던 코드
        //        MIL.MblobGetNumber(MilBlobResult, ref TotalBlobs);    //   Old Function
        //        MIL.MblobSelect(MilBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS, MIN_BLOB_AREA, MIL.M_NULL);
        //        MIL.MblobSelect(MilBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_GREATER, MAX_BLOB_AREA, MIL.M_NULL);

        //        // Get the total number of selected blobs.
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TotalBlobs);       //  원래 돌아가던 코드
        //        MIL.MblobGetNumber(MilBlobResult, ref TotalBlobs);    //   Old Function
        //        if (TotalBlobs < 1) break;

        //        mBlobCogX = new double[4];                                // X coordinate of center of gravity.
        //        mBlobCogY = new double[4];                                // Y coordinate of center of gravity.
        //                                                                  //mBlobRadius = new double[TotalBlobs];                                // Y coordinate of center of gravity.
        //                                                                  //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, mBlobCogX);//  원래 돌아가던 코드
        //                                                                  //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, mBlobCogY);//  원래 돌아가던 코드
        //        MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, mBlobCogX);// 
        //        MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, mBlobCogY);// 

        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_X, mBlobCogX);  //  Old Function
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, mBlobCogY);  //  Old Function
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_AREA, sArea);  //  Old Function
        //        //  thresh + stepThresh 값이 커질 수록 반복성이 떨어진다. 값이 큰 경우의 중심좌표에 있어서 비중을 낮춰야 한다. 
        //        Array.Sort(mBlobCogX, mBlobCogY);   //  X 가 작은 값부터 정리
        //        Array.Sort(mBlobCogX, mBlobCogX);   //  X 가 작은 값부터 정리

        //        for (int q = 0; q < 4; q++)
        //        {
        //            _sA[q] += 1;
        //            _cx[q] += mBlobCogX[q];
        //            _cy[q] += mBlobCogY[q];
        //        }
        //        //if (mBlobCogX[0] < mBlobCogX[1])
        //        //{
        //        //    _sA[0] += 1;
        //        //    _cx[0] += mBlobCogX[0];
        //        //    _cy[0] += mBlobCogY[0];

        //        //    _sA[1] += 1;
        //        //    _cx[1] += mBlobCogX[1];
        //        //    _cy[1] += mBlobCogY[1];
        //        //}
        //        //else
        //        //{
        //        //    _sA[0] += 1;
        //        //    _cx[0] += mBlobCogX[1];
        //        //    _cy[0] += mBlobCogY[1];

        //        //    _sA[1] += 1;
        //        //    _cx[1] += mBlobCogX[0];
        //        //    _cy[1] += mBlobCogY[0];
        //        //}
        //    }
        //    for (int bCnt = 0; bCnt < 4; bCnt++)
        //    {
        //        if (_sA[bCnt] == 0) continue;
        //        cx[bCnt] = _cx[bCnt] / _sA[bCnt];
        //        cy[bCnt] = _cy[bCnt] / _sA[bCnt];

        //        sA[bCnt] = _sA[bCnt] / _sA[bCnt];   //  mm^2 unit
        //    }
        //    Nfound = TotalBlobs;
        //    return true;
        //}
        //public bool FindOpenArea(int index, ref double[] sA, ref double[] cx, ref double[] cy, ref long Nfound, long thresh, int threshwidth)
        //{
        //    MIL.MbufCopy(milCommonImageGrab[index], milImageDisp);
        //    int k = 1;
        //    double numOverlap = 2 * threshwidth + 1;
        //    double[] _sA = new double[4];
        //    double[] _cx = new double[4];
        //    double[] _cy = new double[4];

        //    //StreamWriter wr = File.AppendText("Find.txt");
        //    //string wrLine = "";
        //    for (int stepThresh = -threshwidth; stepThresh <= threshwidth; stepThresh++)
        //    {
        //        // Binarize image.
        //        MIL.MimBinarize(milImageDisp, MilBinImage, MIL.M_FIXED + MIL.M_GREATER_OR_EQUAL, thresh + stepThresh, MIL.M_NULL);
        //        MIL.MblobCalculate(MilBinImage, MIL.M_NULL, MilBlobContext, MilBlobResult);
        //        // Exclude blobs whose area is too small.
        //        MIL.MblobSelect(MilBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS, MIN_BLOB_AREA, MIL.M_NULL);
        //        MIL.MblobSelect(MilBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_GREATER, MAX_BLOB_AREA, MIL.M_NULL);

        //        // Get the total number of selected blobs.
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TotalBlobs);  //  원래 돌아가던 코드
        //        //MessageBox.Show("TotalBlobs = " + TotalBlobs.ToString());
        //        MIL.MblobGetNumber(MilBlobResult, ref TotalBlobs);    //   Old Function
        //        if (TotalBlobs < 1) break;

        //        mBlobCogX[0] = mBlobCogX[1] = mBlobCogX[2] = mBlobCogX[3] = 0;                                // X coordinate of center of gravity.
        //        mBlobCogY[0] = mBlobCogY[1] = mBlobCogY[2] = mBlobCogY[3] = 0;                                // Y coordinate of center of gravity.

        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, mBlobCogX);  //  원래 돌아가던 코드
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, mBlobCogY);  //  원래 돌아가던 코드
        //        MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, mBlobCogX);
        //        MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, mBlobCogY);

        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_X, mBlobCogX);  //  Old Function
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, mBlobCogY);  //  Old Function
        //        //MIL.MblobGetResult(MilBlobResult, MIL.M_AREA, sArea);  //  Old Function
        //        Array.Sort(mBlobCogX, mBlobCogY);   //  X 가 작은 값부터 정리
        //        Array.Sort(mBlobCogX, mBlobCogX);   //  X 가 작은 값부터 정리

        //        for (int q = 0; q < 4; q++)
        //        {
        //            _sA[q] += 1;
        //            _cx[q] += mBlobCogX[q];
        //            _cy[q] += mBlobCogY[q];
        //        }
        //        //if (mBlobCogX[0] < mBlobCogX[1])
        //        //{
        //        //    _sA[0] += 1;
        //        //    _cx[0] += mBlobCogX[0];
        //        //    _cy[0] += mBlobCogY[0];

        //        //    _sA[1] += 1;
        //        //    _cx[1] += mBlobCogX[1];
        //        //    _cy[1] += mBlobCogY[1];
        //        //}
        //        //else
        //        //{
        //        //    _sA[0] += 1;
        //        //    _cx[0] += mBlobCogX[1];
        //        //    _cy[0] += mBlobCogY[1];

        //        //    _sA[1] += 1;
        //        //    _cx[1] += mBlobCogX[0];
        //        //    _cy[1] += mBlobCogY[0];
        //        //}
        //    }
        //    for (int bCnt = 0; bCnt < 4; bCnt++)
        //    {
        //        if (_sA[bCnt] == 0) continue;
        //        cx[bCnt] = _cx[bCnt] / _sA[bCnt];
        //        cy[bCnt] = _cy[bCnt] / _sA[bCnt];

        //        sA[bCnt] = _sA[bCnt] / _sA[bCnt];   //  mm^2 unit
        //    }
        //    Nfound = TotalBlobs;
        //    return true;
        //}
        public void GetRefBrightness(double[] cx, double[] cy, ref double[] lBin)
        {
        }

        //public bool GetAvgBin(int index, ref double lBin, int iBuf=0)
        //{
        //    MIL.MbufGet2d(milCommonImageGrab[index], 0, 0, nSizeX, nSizeY, p_Value[iBuf]);
        //    MIL.MimResize(milCommonImageGrab[index], milQuaterImg[iBuf], 1.0/ mModelScale, 1.0/ mModelScale, MIL.M_DEFAULT);   //  To Find quick & approximate, 840 x 160

        //    q_Value[iBuf] = new byte[ (nSizeX / mModelScale) * (nSizeY / mModelScale) ];    // 1/4 축소이미지

        //    MIL.MbufGet2d(milQuaterImg[iBuf], 0, 0, nSizeX / mModelScale, nSizeY / mModelScale, q_Value[iBuf]);

        //    int quaterWidth = nSizeX / mModelScale;
        //    double sum = 0;
        //    for (int j = 10; j < nSizeY / mModelScale - 10; j++)
        //        for (int i = 10; i < nSizeX / mModelScale - 10; i++)
        //        {
        //            sum += q_Value[iBuf][i + j * quaterWidth];
        //        }
        //    lBin = sum / ((nSizeY / mModelScale - 20) * (nSizeX / mModelScale - 20));
        //    return true;
        //}

        public void LoadBMPtoBuf0(string filename)
        {
            MIL_ID resMilID = MIL.M_NULL;

            resMilID = MIL.MbufImport(filename, MIL.M_BMP, MIL.M_RESTORE, milSystem, ref milCommonImageGrab[0]);

            //MIL.MimResize(milCommonImageGrab[0], milCommonImageResize[0], mModelScale, mModelScale, MIL.M_DEFAULT);

            //            MIL.MbufImport(filename, MIL.M_BMP, MIL.M_LOAD, MIL.M_NULL, ref milCommonImageGrab[0]);
            //MIL.MbufCopy(resMilID, milImageDisp);
            //MIL.MbufGet2d(milCommonImageGrab[0], 0, 0, nSizeX, nSizeY, p_Value[0]);

            MIL.MbufCopy(milCommonImageGrab[0], milImageDisp);
        }
        public void LoadBMPtoBufN(string filename, int N)
        {
            MIL_ID resMilID = MIL.M_NULL;
            resMilID = MIL.MbufImport(filename, MIL.M_BMP, MIL.M_RESTORE, milSystem, ref milCommonImageGrab[N]);

            //MIL.MimResize(milCommonImageGrab[N], milCommonImageResize[N], mModelScale, mModelScale, MIL.M_DEFAULT);
            MIL.MbufCopy(resMilID, milImageDisp);
        }

        public void PrepareFineCOG()
        {
            mFAL.Prepare6DMotion(nSizeX, nSizeY);
        }

        public double vSin40 = Math.Sin(40 / 180.0 * Math.PI);
        public void SetSideviewTheta(double rad)
        {
            vSin40 = Math.Sin(rad);
            if (mFAL == null)
                return;

            mFAL.SetSideviewTheta(rad);

        }

        public void FindScaleNOpticalRotation(int index, string fileName)
        {
            //  mAzimuthPts[index] 및 mFAL.mFidMarkSide, mFAL.mFidMarkTop 으로부터 Scale 및 Rotation 을 계산하고 파일로 저장한다.

            FAutoLearn.FZMath.Point2D[] pTopNorm = new FAutoLearn.FZMath.Point2D[2];
            FAutoLearn.FZMath.Point2D[] pSideNorm = new FAutoLearn.FZMath.Point2D[3];
            FAutoLearn.FZMath.Point2D[] pTop = new FAutoLearn.FZMath.Point2D[2];
            FAutoLearn.FZMath.Point2D[] pSide = new FAutoLearn.FZMath.Point2D[3];

            pTopNorm[0] = new FAutoLearn.FZMath.Point2D();   //  North on Top, real Value in mm
            pTopNorm[1] = new FAutoLearn.FZMath.Point2D();   //  South on Top, real Value in mm

            pSideNorm[0] = new FAutoLearn.FZMath.Point2D();  //  North on Side, measured in mm
            pSideNorm[1] = new FAutoLearn.FZMath.Point2D();  //  South on Side, measured in mm
            pSideNorm[2] = new FAutoLearn.FZMath.Point2D();  //  East on Side, measured in mm

            pTop[0] = new FAutoLearn.FZMath.Point2D();  //  North on Top, measured in pixel
            pTop[1] = new FAutoLearn.FZMath.Point2D();  //  South on Top, measured in pixel

            pSide[0] = new FAutoLearn.FZMath.Point2D(); //  North on Side, measured in pixel
            pSide[1] = new FAutoLearn.FZMath.Point2D(); //  South on Side, measured in pixel
            pSide[2] = new FAutoLearn.FZMath.Point2D(); //  East on Side, measured in pixel


            double pixSize = 0.0055 / Global.LensMag;


            foreach (FAutoLearn.FAutoLearn.sFiducialMark lmark in mFAL.mFidMarkSide)
            {
                if (lmark.Azimuth == 0 && mAzimuthPts[index][0].X > 0)
                {
                    //  North 1
                    pSideNorm[0].X = lmark.fidInfo.X;
                    pSideNorm[0].Y = lmark.fidInfo.Y * vSin40;
                    pSide[0].X = mAzimuthPts[index][0].X * pixSize;
                    pSide[0].Y = nSizeY - mAzimuthPts[index][0].Y * pixSize;
                }
                else if (lmark.Azimuth == 1 && mAzimuthPts[index][1].X > 0)
                {
                    //  North 2
                    pSideNorm[0].X = lmark.fidInfo.X / pixSize;
                    pSideNorm[0].Y = lmark.fidInfo.Y / pixSize * vSin40;
                    pSide[0].X = mAzimuthPts[index][1].X * pixSize;
                    pSide[0].Y = nSizeY - mAzimuthPts[index][1].Y * pixSize;
                }
                else if (lmark.Azimuth == 4 && mAzimuthPts[index][4].X > 0)
                {
                    //  South 1
                    pSideNorm[1].X = -lmark.fidInfo.X;
                    pSideNorm[1].Y = lmark.fidInfo.Y * vSin40;
                    pSide[1].X = mAzimuthPts[index][4].X * pixSize;
                    pSide[1].Y = nSizeY - mAzimuthPts[index][4].Y * pixSize;
                }
                else if (lmark.Azimuth == 5 && mAzimuthPts[index][5].X > 0)
                {
                    //  South 2
                    pSideNorm[1].X = -lmark.fidInfo.X;
                    pSideNorm[1].Y = lmark.fidInfo.Y * vSin40;
                    pSide[1].X = mAzimuthPts[index][5].X * pixSize;
                    pSide[1].Y = nSizeY - mAzimuthPts[index][5].Y * pixSize;
                }
                else if (lmark.Azimuth == 6 && mAzimuthPts[index][6].X > 0)
                {
                    //  East 1
                    pSideNorm[2].X = -lmark.fidInfo.X;
                    pSideNorm[2].Y = lmark.fidInfo.Y * vSin40;
                    pSide[2].X = mAzimuthPts[index][6].X * pixSize;
                    pSide[2].Y = nSizeY - mAzimuthPts[index][6].Y * pixSize;
                }
                else if (lmark.Azimuth == 7 && mAzimuthPts[index][7].X > 0)
                {
                    //  East 2
                    pSideNorm[3].X = -lmark.fidInfo.X;
                    pSideNorm[3].Y = lmark.fidInfo.Y * vSin40;
                    pSide[3].X = mAzimuthPts[index][7].X * pixSize;
                    pSide[3].Y = nSizeY - mAzimuthPts[index][7].Y * pixSize;
                }
            }

            foreach (FAutoLearn.FAutoLearn.sFiducialMark lmark in mFAL.mFidMarkTop)
            {
                if (lmark.Azimuth == 0 && mAzimuthPts[index][8].X > 0)
                {
                    //  North 1 from Top
                    pTopNorm[0].X = lmark.fidInfo.X;
                    pTopNorm[0].Y = lmark.fidInfo.Y;
                    pTop[0].X = mAzimuthPts[index][8].X * pixSize;
                    pTop[0].Y = nSizeY - mAzimuthPts[index][8].Y * pixSize;
                }
                else if (lmark.Azimuth == 1 && mAzimuthPts[index][9].X > 0)
                {
                    //  North 2 from Top
                    pTopNorm[0].X = lmark.fidInfo.X;
                    pTopNorm[0].Y = lmark.fidInfo.Y;
                    pTop[0].X = mAzimuthPts[index][9].X * pixSize;
                    pTop[0].Y = nSizeY - mAzimuthPts[index][9].Y * pixSize;
                }
                else if (lmark.Azimuth == 2 && mAzimuthPts[index][10].X > 0)
                {
                    //  South 1 from Top
                    pTopNorm[1].X = -lmark.fidInfo.X;
                    pTopNorm[1].Y = lmark.fidInfo.Y;
                    pTop[1].X = mAzimuthPts[index][10].X * pixSize;
                    pTop[1].Y = nSizeY - mAzimuthPts[index][10].Y * pixSize;
                }
                else if (lmark.Azimuth == 3 && mAzimuthPts[index][11].X > 0)
                {
                    //  South 2 from Top
                    pTopNorm[1].X = -lmark.fidInfo.X;
                    pTopNorm[1].Y = lmark.fidInfo.Y;
                    pTop[1].X = mAzimuthPts[index][11].X * pixSize;
                    pTop[1].Y = nSizeY - mAzimuthPts[index][11].Y * pixSize;
                }
            }
            double aT = 0;
            double aS = 0;
            mFAL.mFZM.TopNSideViewRotationAnlge(pTopNorm, pSideNorm, pTop, pSide, ref aT, ref aS);

            mFAL.mFZM.mOpticsAngleTop = -aT;
            mFAL.mFZM.mOpticsAngleSide = -aS;

            double normXLength = Math.Abs(pSideNorm[0].X - pSideNorm[1].X);
            double normYLength = Math.Abs((pSideNorm[0].Y + pSideNorm[1].Y) / 2 - pSideNorm[2].Y);
            double scaleTop = 1;
            double scaleSide = 1;
            mFAL.mFZM.CalcScaleTopNSide(pTop, pSide, normXLength, ref scaleTop, ref scaleSide);

            //mFAL.mFZM.mScaleTop = scaleTop;
            //mFAL.mFZM.mScaleSide = scaleSide;

            StreamWriter wr = new StreamWriter(fileName);
            wr.WriteLine(mFAL.mFZM.mOpticsAngleTop.ToString());
            wr.WriteLine(mFAL.mFZM.mOpticsAngleSide.ToString());
            wr.WriteLine(mFAL.mFZM.mScaleTop.ToString());
            wr.WriteLine(mFAL.mFZM.mScaleSide.ToString());
            wr.Close();
        }

        public void GetScaleNOpticalR(ref double sTop, ref double sSide, ref double rTop, ref double rSide)
        {
            if (mFAL == null)
                return;

            sTop = mFAL.mFZM.mScaleTop;
            sSide = mFAL.mFZM.mScaleSide;
            rTop = mFAL.mFZM.mOpticsAngleTop;
            rSide = mFAL.mFZM.mOpticsAngleSide;
        }

        public void LoadScaleNOpticalRotation(string fileName, ref double sTop, ref double sSide, ref double rTop, ref double rSide)
        {
            //  파일로부터 Scale 및 Rotation 을 읽어들여서 관련 내부변수를 초기화해준다.
            if (mFAL == null)
                return;

            if (!File.Exists(fileName))
                return;

            StreamReader rd = new StreamReader(fileName);
            string lstr = rd.ReadToEnd();
            rd.Close();
            string[] lines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 4)
                return;

            double lOAT = double.Parse(lines[0]);
            double lOAS = double.Parse(lines[1]);
            double lST = double.Parse(lines[2]);
            double lSS = double.Parse(lines[3]);

            if (!double.IsNaN(lOAT))
                mFAL.mFZM.mOpticsAngleTop = lOAT;
            if (!double.IsNaN(lOAS))
                mFAL.mFZM.mOpticsAngleSide = lOAS;
            //if (!double.IsNaN(lST))
            //    mFAL.mFZM.mScaleTop         = lST ;
            //if (!double.IsNaN(lSS))
            //    mFAL.mFZM.mScaleSide        = lSS;

            //sTop = mFAL.mFZM.mScaleTop;
            //sTop = mFAL.mFZM.mScaleSide;
            rTop = mFAL.mFZM.mOpticsAngleTop;
            rSide = mFAL.mFZM.mOpticsAngleSide;
        }

        public Rect[] mInitialSearchROISide = new Rect[8];
        public Rect[] mInitialSearchROITop = new Rect[4];

        List<FAutoLearn.FAutoLearn.sFiducialMark> mFidMarkSideBk = new List<FAutoLearn.FAutoLearn.sFiducialMark>();
        List<FAutoLearn.FAutoLearn.sFiducialMark> mFidMarkTopBk = new List<FAutoLearn.FAutoLearn.sFiducialMark>();
        public void BackupFidMark()
        {
            for (int i = 0; i < mFAL.mFidMarkSide.Count; i++)
            {
                mInitialSearchROISide[i].X = mFAL.mFidMarkSide[i].searchRoi.X;
                mInitialSearchROISide[i].Y = mFAL.mFidMarkSide[i].searchRoi.Y;
                mInitialSearchROISide[i].Height = mFAL.mFidMarkSide[i].searchRoi.Height;
                mInitialSearchROISide[i].Width = mFAL.mFidMarkSide[i].searchRoi.Width;
            }
            for (int i = 0; i < mFAL.mFidMarkTop.Count; i++)
            {
                mInitialSearchROITop[i].X = mFAL.mFidMarkTop[i].searchRoi.X;
                mInitialSearchROITop[i].Y = mFAL.mFidMarkTop[i].searchRoi.Y;
                mInitialSearchROITop[i].Height = mFAL.mFidMarkTop[i].searchRoi.Height;
                mInitialSearchROITop[i].Width = mFAL.mFidMarkTop[i].searchRoi.Width;
            }
        }

        public int[] mBackgroundNoise = null;
        public int[] mBackgroundNoiseY = null;
        public void CalcBackgroundNoise(int indexFrom, int indexTo, int iBuf)
        {
            int[] bgNoise = new int[nSizeX];
            double[] bgNoiseY = new double[nSizeY];
            mBackgroundNoise = new int[nSizeX];
            mBackgroundNoiseY = new int[nSizeY];
            double den = 80; //  원래 노이즈의 50배

            for (int index = indexFrom; index < indexTo; index++)
            {
                MIL.MbufGet2d(milCommonImageGrab[index], 0, 0, nSizeX, nSizeY, p_Value[iBuf]);

                //  수직 80줄 합 계산 
                for (int j = 20; j < 100; j++)
                {
                    for (int i = 0; i < nSizeX; i++)
                        bgNoise[i] += p_Value[iBuf][i + j * nSizeX];
                }
                // 매 Line 별 수평 전체 합 계산 840배 x 50배 = 42000 배
                for (int j = 0; j < nSizeY; j++)
                {
                    for (int i = 0; i < nSizeX; i++)
                        bgNoiseY[j] += p_Value[iBuf][i + j * nSizeX];
                }
            }
            int min = 9999;
            for (int i = 0; i < nSizeX; i++)
            {
                mBackgroundNoise[i] = (int)(bgNoise[i] / den); //  80줄 x 50장 = 4000 배 , 4000/80 = 50배

                if (min > mBackgroundNoise[i])
                    min = mBackgroundNoise[i];
            }
            for (int i = 0; i < nSizeX; i++)
            {
                mBackgroundNoise[i] -= min;
            }
            //  Y 방향
            double[] ma40 = new double[nSizeX];
            for (int i = 0; i < nSizeY; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    int maIndex = i + j - 20;

                    if (maIndex < 0)
                        maIndex = 0;
                    else if (maIndex >= nSizeY)
                        maIndex = nSizeY - 1;
                    ma40[i] += bgNoiseY[maIndex];
                }
                ma40[i] /= 40;
            }
            min = 9999;
            den = nSizeX; //  840
            for (int i = 0; i < nSizeY; i++)
            {
                mBackgroundNoiseY[i] = (int)((bgNoiseY[i] - ma40[i]) / den);    //  42000 배 / 840 = 50배
                if (min > mBackgroundNoiseY[i])
                    min = mBackgroundNoiseY[i];
            }
            for (int i = 0; i < nSizeY; i++)
            {
                mBackgroundNoiseY[i] -= min;
            }
            mFAL.SetBackgroundNoise(mBackgroundNoise, mBackgroundNoiseY);
        }

        public FAutoLearn.FAutoLearn.sMarkResult[] m_sMR = new FAutoLearn.FAutoLearn.sMarkResult[12];
        public FAutoLearn.FAutoLearn.sMarkResult[] m_sMRinstant = new FAutoLearn.FAutoLearn.sMarkResult[12];
        Mat[] scaledImg = new Mat[30];
        public void ResizeGrab(int i)
        {
            MIL.MimResize(milCommonImageGrab[i], milCommonImageResize[i], mModelScale, mModelScale, MIL.M_AVERAGE);
        }
        //public bool FineCOG(int index, int iBuf, bool IsShowBox = false, bool need6D = true)
        public FAutoLearn.FAutoLearn.sMarkResult[] mOldSMR = new FAutoLearn.FAutoLearn.sMarkResult[12];
        public OpenCvSharp.Point[][] mDetectedMarkPos = new Point[30][];
        public bool FineCOG(bool IsFirst, int index, int iBuf, bool IsShowBox = false, bool need6D = true, bool needLEDavg = false, bool IsFile = false)
        {
            if (mFAL.mFidMarkSide[0] == null)
                return false;

            int lModelScale = mFAL.mFidMarkSide[0].MScale;
            FAutoLearn.FAutoLearn.sMarkResult[] sMR = new FAutoLearn.FAutoLearn.sMarkResult[12];
            FAutoLearn.FAutoLearn.sMarkResult[] sMR_T = new FAutoLearn.FAutoLearn.sMarkResult[12];
            FAutoLearn.FAutoLearn.sMarkResult[] sMR_B = new FAutoLearn.FAutoLearn.sMarkResult[12];

            if (IsFirst)
            {
                for (int i = 0; i < 30; i++)
                {
                    scaledImg[i] = new Mat();
                    mFAL.mSourceImg[i] = new Mat(Global.mMergeImgHeight, Global.mMergeImgWidth, MatType.CV_8UC1);
                    mFAL.q_Value[i] = new byte[(Global.mMergeImgWidth / lModelScale) * (Global.mMergeImgHeight / lModelScale)];
                }
            }

            for (int i = 0; i < 12; i++)
            {
                sMR[i] = new FAutoLearn.FAutoLearn.sMarkResult();
                sMR_T[i] = new sMarkResult();
                sMR_B[i] = new sMarkResult();

            }


            //ResizeGrab(index);

            long Nfound = 0;


            if (index == 0)
            {
                //  0 번 프레임의 경우 모든 버퍼에 0번 프레임을 넣어준다. MultiTask작업 시 각 Task 에서 각 버퍼를 독립적으로 활용한다.
                if (!IsFile)
                    CropImage(0, 0);

                mFAL.ResizeSourceImg(0, 0);
                for (int nbuf = 1; nbuf < 30; nbuf++)
                {
                    mFAL.mSourceImg[0].CopyTo(mFAL.mSourceImg[nbuf]);
                    mFAL.ResizeSourceImg(nbuf, nbuf); //  시간 많이 걸림. 단축할 필요 있음
                }
                //mFAL.mSourceImg[0].SaveImage("C:\\CSHTest\\Result\\RawData\\Image\\Crop2.bmp");
            }
            else
            {
                if (!IsFile)
                    CropImage(index, iBuf);
                mFAL.ResizeSourceImg(iBuf, iBuf);
            }

            OpenCvSharp.Point[] markPos = null;

            long starttime = 0;
            long endtime = 0;
            long timerFrequency = 0;

            if (needLEDavg)
            {
                Scalar sValue = Cv2.Mean(mFAL.mSourceImg[iBuf]);
                mAvgLED[index] = sValue.Val0;
            }

            SupremeTimer.QueryPerformanceFrequency(ref timerFrequency);
            SupremeTimer.QueryPerformanceCounter(ref starttime);


            mDetectedMarkPos[iBuf] = mFAL.FineCOG(IsFirst, index, ref sMR, ref sMR_T, ref sMR_B, ref Nfound, false, iBuf);

            mOldSMR = sMR;

            if (index == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    m_sMR[i] = new FAutoLearn.FAutoLearn.sMarkResult();
                    m_sMRinstant[i] = new FAutoLearn.FAutoLearn.sMarkResult();
                    m_sMR[i] = sMR[i];
                }
            }
            //if (IsShowBox)    //  Crop 영상에서 보여줘야 하므로 FZ4P 에서는 아래 기능이 의미 없다.
            //{
            //    for (int i = 0; i < markPos.Length; i++)
            //    {
            //        if (markPos[i].X == 0)
            //            continue;
            //        DrawDCBox(new System.Drawing.Point(markPos[i].X * lModelScale, markPos[i].Y * lModelScale), new System.Drawing.Point((markPos[i].X + sMR[i].mSize.Width) * lModelScale, (markPos[i].Y + +sMR[i].mSize.Height) * lModelScale), Brushes.Magenta);
            //    }

            //}
            for (int i = 0; i < 5; i++)
                m_sMRinstant[i] = sMR[i];


            OpenCvSharp.Point2d[] pts = new OpenCvSharp.Point2d[12];
            OpenCvSharp.Point2d[] ptsU = new OpenCvSharp.Point2d[12];
            OpenCvSharp.Point2d[] ptsL = new OpenCvSharp.Point2d[12];
            FAutoLearn.FZMath.Point2D[] ptsSide = new FAutoLearn.FZMath.Point2D[6];
            FAutoLearn.FZMath.Point2D[] ptsTop = new FAutoLearn.FZMath.Point2D[6];

            for (int j = 0; j < sMR.Length; j++)
            {
                if (sMR[j].Azimuth < 0) continue;
                pts[sMR[j].Azimuth] = sMR[j].pos;
                ptsU[sMR[j].Azimuth] = sMR_T[j].pos;
                ptsL[sMR[j].Azimuth] = sMR_B[j].pos;

            }


            mAzimuthPts[index] = pts;
            mAzimuthPtsUpper[index] = ptsU;
            mAzimuthPtsLower[index] = ptsL;

            for (int j = 0; j < 4; j++)
            {
                if (pts[j * 2 + 1].X > 0)
                {
                    ptsSide[j] = new FAutoLearn.FZMath.Point2D((pts[j * 2].X + pts[j * 2 + 1].X) / 2, (pts[j * 2].Y + pts[j * 2 + 1].Y) / 2);
                    //ptsSide[j].X = (pts[j * 2].X + pts[j * 2 + 1].X) / 2;
                    //ptsSide[j].Y = (pts[j * 2].Y + pts[j * 2 + 1].Y) / 2;
                }
                else
                {
                    //  0->0, 2->1, 4->2, 6->3
                    ptsSide[j] = new FAutoLearn.FZMath.Point2D(pts[j * 2].X, pts[j * 2].Y);
                    //ptsSide[j].X = pts[j * 2].X;
                    //ptsSide[j].Y = pts[j * 2].Y;
                }
            }

            for (int j = 4; j < 6; j++)
            {
                if (pts[j * 2 + 1].X > 0)
                {

                    ptsTop[j - 4] = new FAutoLearn.FZMath.Point2D((pts[j * 2].X + pts[j * 2 + 1].X) / 2, (pts[j * 2].Y + pts[j * 2 + 1].Y) / 2);
                    //ptsTop[j - 4].X = (pts[j * 2].X + pts[j * 2 + 1].X) / 2;
                    //ptsTop[j - 4].Y = (pts[j * 2].Y + pts[j * 2 + 1].Y) / 2;
                }
                else
                {
                    //  8->0, 10->1
                    ptsTop[j - 4] = new FAutoLearn.FZMath.Point2D(pts[j * 2].X, pts[j * 2].Y);
                    //ptsTop[j - 4].X = pts[j * 2].X;
                    //ptsTop[j - 4].Y = pts[j * 2].Y;
                }
            }
            FAutoLearn.FZMath.Point2D lTranslation = new FAutoLearn.FZMath.Point2D();

            // 241206 YLUT 사용안함으로 변경
            // mFAL.ApplyYLUT(ref ptsSide[0].Y, ref ptsSide[2].Y, ref ptsSide[3].Y);
            // 
            mAzimuthPts[index][0].Y = ptsSide[0].Y;
            mAzimuthPts[index][4].Y = ptsSide[2].Y;
            mAzimuthPts[index][6].Y = ptsSide[3].Y;

            //////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////
            //ptsSide[0].Y = ptsSide[0].Y * 0.985;
            //ptsSide[2].Y = ptsSide[2].Y * 0.985;
            //ptsSide[3].Y = ptsSide[3].Y + 0.003 * (ptsSide[3].X - 320);

            //mAzimuthPts[index][0].Y = ptsSide[0].Y;
            //mAzimuthPts[index][4].Y = ptsSide[2].Y;
            //mAzimuthPts[index][6].Y = ptsSide[3].Y;

            //////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////
            double umsale = 5.5 / Global.LensMag;

            if (Nfound >= 5 && need6D)
            {
                mMarkPosRes[0][index] = ptsSide[0];
                mMarkPosRes[1][index] = ptsSide[2];
                mMarkPosRes[2][index] = ptsSide[3];
                mMarkPosRes[3][index] = ptsTop[0];
                mMarkPosRes[4][index] = ptsTop[1];
                mFAL.mFZM.Extract6DMotion(index, ptsTop, ptsSide, ref lTranslation, ref mC_pTZ[index], ref mC_pZ[index], ref mC_pTX[index], ref mC_pTY[index]);

                mC_pX[index] = lTranslation.X;// - mFAL.mFZM.mZtoXbyView[1] * mC_pZ[index] + (mC_pY[index] + 4* mC_pZ[index])* mC_pX[index] /120000;   //  120000 -> 
                mC_pY[index] = lTranslation.Y;// - mFAL.mFZM.mZtoYbyView[1] * mC_pZ[index] - mFAL.mFZM.mXtoYbyView[1] * mC_pX[index] + mC_pTX[index] * 1.875134602 ; //  영상에서 위쪽으로 이동이 + 방향 되도록 조정함.

            }
            if (lTranslation.X == 0 && bSaveLostMarkFrame)
            {
                DateTime dtNow = DateTime.Now;
                string filename = "C:\\B7WideTest\\Result\\log\\Err" + index.ToString() + "_" + dtNow.ToString("ddHHmmss.fff") + ".bmp";
                mFAL.mSourceImg[iBuf].SaveImage(filename);
            }

            return true;
        }

        public FAutoLearn.FZMath.Point2D[][] mMarkPosRes = new FAutoLearn.FZMath.Point2D[5][];

        bool bSaveLostMarkFrame = false;
        public void SetSaveLostMarkFrame(bool which)
        {
            bSaveLostMarkFrame = which;
        }
        public void ClearSaveLostMarkFrame()
        {
            bSaveLostMarkFrame = false;
        }
        public void PointTo6DMotion(int index, System.Drawing.Point[] pts)
        {
            //  표준마크위치를 받아서 처리하므로 CropGap 을 적용할 필요가 없다.

            FAutoLearn.FZMath.Point2D[] ptsTop = new FAutoLearn.FZMath.Point2D[2];
            FAutoLearn.FZMath.Point2D[] ptsSide = new FAutoLearn.FZMath.Point2D[4];
            FAutoLearn.FZMath.Point2D lTranslation = new FAutoLearn.FZMath.Point2D();

            ptsTop[0] = new FAutoLearn.FZMath.Point2D(pts[3].X, pts[3].Y);
            ptsTop[1] = new FAutoLearn.FZMath.Point2D(pts[4].X, pts[4].Y);

            ptsSide[0] = new FAutoLearn.FZMath.Point2D(pts[0].X, pts[0].Y);
            ptsSide[2] = new FAutoLearn.FZMath.Point2D(pts[1].X, pts[1].Y);
            ptsSide[3] = new FAutoLearn.FZMath.Point2D(pts[2].X, pts[2].Y);
            if (pts[5].X != 0)
            {
                ptsSide[3].X = (pts[2].X + pts[3].X) / 2;
                ptsSide[3].Y = (pts[2].Y + pts[3].Y) / 2;
                ptsTop[0] = new FAutoLearn.FZMath.Point2D(pts[4].X, pts[4].Y);
                ptsTop[1] = new FAutoLearn.FZMath.Point2D(pts[5].X, pts[5].Y);
            }

            //241206 YLUT 적용 제거
            //mFAL.ApplyYLUT(ref ptsSide[0].Y, ref ptsSide[2].Y, ref ptsSide[3].Y);

            if (index < 0)
                mFAL.mFZM.Extract6DMotion(-1, ptsTop, ptsSide, ref lTranslation, ref mC_pTZ[0], ref mC_pZ[0], ref mC_pTX[0], ref mC_pTY[0]);
            else
            {
                mFAL.mFZM.Extract6DMotion(index, ptsTop, ptsSide, ref lTranslation, ref mC_pTZ[index], ref mC_pZ[index], ref mC_pTX[index], ref mC_pTY[index], false);
                mC_pX[index] = lTranslation.X;// - mFAL.mFZM.mZtoXbyView[1] * mC_pZ[index];
                mC_pY[index] = lTranslation.Y;// - mFAL.mFZM.mZtoYbyView[1] * mC_pZ[index]; //  영상에서 위쪽으로 이동이 + 방향 되도록 조정함.
            }
        }
        public void PointTo6DMotion(int index, FAutoLearn.FZMath.Point2D[] pts)
        {
            //  측정값을 받아서 처리하므로 CropGap 을 적용해야 한다.

            FAutoLearn.FZMath.Point2D[] ptsTop = new FAutoLearn.FZMath.Point2D[2];
            FAutoLearn.FZMath.Point2D[] ptsSide = new FAutoLearn.FZMath.Point2D[4];
            FAutoLearn.FZMath.Point2D lTranslation = new FAutoLearn.FZMath.Point2D();

            ptsTop[0] = new FAutoLearn.FZMath.Point2D(pts[3].X, pts[3].Y);
            ptsTop[1] = new FAutoLearn.FZMath.Point2D(pts[4].X, pts[4].Y);

            ptsSide[0] = new FAutoLearn.FZMath.Point2D(pts[0].X, pts[0].Y);
            ptsSide[2] = new FAutoLearn.FZMath.Point2D(pts[1].X, pts[1].Y);
            ptsSide[3] = new FAutoLearn.FZMath.Point2D(pts[2].X, pts[2].Y);

            //241206 YLUT 적용 제거
            // mFAL.ApplyYLUT(ref ptsSide[0].Y, ref ptsSide[2].Y, ref ptsSide[3].Y);   //  실측치 기준으로 CropGap 적용 없이 YLUT 가 저장 되어있어야 한다.

            if (index < 0)
            {
                mFAL.mFZM.Extract6DMotion(-1, ptsTop, ptsSide, ref lTranslation, ref mC_pTZ[0], ref mC_pZ[0], ref mC_pTX[0], ref mC_pTY[0]);
            }
            else
            {
                mFAL.mFZM.Extract6DMotion(index, ptsTop, ptsSide, ref lTranslation, ref mC_pTZ[index], ref mC_pZ[index], ref mC_pTX[index], ref mC_pTY[index]);
                mC_pX[index] = lTranslation.X;// - mFAL.mFZM.mZtoXbyView[1] * mC_pZ[index];
                mC_pY[index] = lTranslation.Y;// - mFAL.mFZM.mZtoYbyView[1] * mC_pZ[index]; //  영상에서 위쪽으로 이동이 + 방향 되도록 조정함.
            }

        }
        public bool FineMTF(bool IsFirst, int index, int iBuf, bool IsShowBox = false, bool need6D = true)
        {

            FAutoLearn.FAutoLearn.sMarkResult[] sMR = new FAutoLearn.FAutoLearn.sMarkResult[12];

            for (int i = 0; i < 12; i++)
                sMR[i] = new FAutoLearn.FAutoLearn.sMarkResult();

            long Nfound = 0;

            int lModelScale = mFAL.mFidMarkSide[0].MScale;

            MIL.MbufGet2d(milCommonImageGrab[index], 0, 0, nSizeX, nSizeY, p_Value[iBuf]);
            mFAL.mSourceImg[iBuf] = new Mat(nSizeY, nSizeX, MatType.CV_8UC1, p_Value[iBuf]);
            Mat scaledImg = new Mat();
            Cv2.Resize(mFAL.mSourceImg[iBuf], scaledImg, new OpenCvSharp.Size((int)(nSizeX / lModelScale), (int)(nSizeY / lModelScale)), 1.0 / lModelScale, 1.0 / lModelScale, InterpolationFlags.Area);
            scaledImg.GetArray(out mFAL.q_Value[iBuf]);

            //OpenCvSharp.Point[] markPos = mFAL.FineCOG(index, ref sMR, ref Nfound, false, iBuf);
            OpenCvSharp.Point[] markPos = mFAL.FineMTF(IsFirst, index, ref sMR, ref Nfound, false, iBuf);
            //if (index == 0)
            //{
            for (int i = 0; i < 12; i++)
            {
                m_sMR[i] = new FAutoLearn.FAutoLearn.sMarkResult();
                m_sMR[i] = sMR[i];
                mBufMTF[i][index] = sMR[i].mMTF;
            }
            //}

            if (IsShowBox)
            {
                for (int i = 0; i < markPos.Length; i++)
                    DrawDCBox(new System.Drawing.Point(markPos[i].X * lModelScale, markPos[i].Y * lModelScale), new System.Drawing.Point((markPos[i].X + sMR[i].mSize.Width) * lModelScale, (markPos[i].Y + +sMR[i].mSize.Height) * lModelScale), Brushes.Magenta);

            }

            return true;
        }
        public string CheckResultFolder()
        {
            DateTime dt = DateTime.Now;
            string resDirectory = "C:\\B7WideTest\\Data\\" + dt.Year + "\\" + dt.Month + "\\" + dt.Day + "\\";
            if (!Directory.Exists(resDirectory))
                Directory.CreateDirectory(resDirectory);
            return resDirectory;
        }


        public void SwapPoint(ref System.Drawing.Point pA, ref System.Drawing.Point pB)
        {
            try
            {
                System.Drawing.Point tmp = new System.Drawing.Point();
                tmp.X = pA.X;
                tmp.Y = pA.Y;
                pA.X = pB.X;
                pA.Y = pB.Y;
                pB.X = tmp.X;
                pB.Y = tmp.Y;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.ToString()); }

        }



        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////
        ////////////////////////////////////////////////
        ////////////////////////////////////////////////

        public int[] mYtoPXoffset = new int[2] { 1000, 1000 };
        public int mPitchWidth = 261;

        public void ResetYtoPXoffset()
        {
            mYtoPXoffset[0] = 1000;
            mYtoPXoffset[1] = 1000;
        }


        public bool Process_VisionGrab(int testItem, int GrabCount, ref int grabbedcount)
        {
            if (!Process_VisionGrabWait())
            {
                return false;
            }
            m_bProcess_Vision = true;
            int n = 0;

            long l_dEndTime = 0;
            SupremeTimer.QueryPerformanceCounter(ref l_dEndTime);

            if (IsLiveA == true)
            {
                HaltA();
            }
            switch (testItem)
            {
                case 5:
                case 0:
                    for (n = 0; n < GrabCount; n++)
                    {
                        GrabA_User(n);
                        SupremeTimer.QueryPerformanceCounter(ref GrabT1[n]);
                    }
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    for (n = 0; n < GrabCount; n++)
                    {
                        GrabA_User(n);
                        SupremeTimer.QueryPerformanceCounter(ref GrabT1[n]);

                        if (uTimer.Enabled == false)
                            break;
                    }
                    break;
            }

            FPS = n / ((GrabT1[n - 1] - GrabT1[0]) / (double)mTimerFrequency);
            grabbedcount = n;
            switch (testItem)
            {
                case 5:
                case 1:
                    dAFZM_FrameCount = n;
                    dAF_FrameCount = n;
                    dZoom_FrameCount = n;
                    break;
                case 2:
                case 3:
                case 4:
                    dAFZM_FrameCount = n;
                    break;
                default: break;
            }
            m_bProcess_Vision = false;
            return true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public bool Process_VisionGrabWait()
        {
            for (int i = 0; i < 350; i++)  // 7초
            {
                Thread.Sleep(10);
                if (m_bProcess_Vision == false)
                    break;
            }
            if (m_bProcess_Vision == true)
                return false;
            return true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void ForceCPXY(int n, ref double[] tx, ref double[] ty, ref double[] x, ref double[] y)
        {
            for (int j = 0; j < 4; j++)
            {
                //mC_pX[j, n] = x[j];
                //mC_pY[j, n] = y[j];
                //if ( j<2)
                //{
                //    mC_pTX[j, n] = tx[j];
                //    mC_pTY[j, n] = ty[j];
                //}
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public bool CalcStroke(int i, ref double[] stroke)  //  Unit of the results : minute
        {
            int k = 0;
            try
            {
                for (k = 0; k < 2; k++)
                {
                    //if (mC_pX[k, i] == 0)
                    //    stroke[k] = 0;
                    //else
                    //    stroke[k] = 11.0 * mMScaleS[0] * (mC_pX[k, i] - mC_pX[k, 0]); //  Left L, L2
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e + " at k=" + k.ToString() + " i=" + i.ToString());
            }
            return true;
        }


        public dLine Linefitting(dPoint[] data, int dataSize)
        {
            dLine rtnLine;
            double SUMx = 0; //sum of x values
            double SUMy = 0; //sum of y values
            double SUMxy = 0; //sum of x * y
            double SUMxx = 0; //sum of x^2 
            double slope = 0; //slope of regression line
            double y_intercept = 0; //y intercept of regression line 
            double AVGy = 0; //mean of y
            double AVGx = 0; //mean of x

            //calculate various sums 
            for (int i = 0; i < dataSize; i++)
            {
                SUMx = SUMx + data[i].x;
                SUMy = SUMy + data[i].y;
                SUMxy = SUMxy + data[i].x * data[i].y;
                SUMxx = SUMxx + data[i].x * data[i].x;
            }

            //calculate the means of x and y
            AVGy = SUMy / dataSize;
            AVGx = SUMx / dataSize;

            //slope or a1
            slope = (dataSize * SUMxy - SUMx * SUMy) / (dataSize * SUMxx - SUMx * SUMx);

            //y itercept or a0
            y_intercept = AVGy - slope * AVGx;

            rtnLine.dSlope = slope;
            rtnLine.dYintercept = y_intercept;

            return rtnLine;
        }

        void ConfigExternalTrigger()
        {
            //MIL_ID MilBuffer = MIL.MbufAlloc2d(milSystem, MIL.MdigInquire(milDigitizer, MIL.M_SIZE_X, MIL.M_NULL), MIL.MdigInquire(milDigitizer, MIL.M_SIZE_Y, MIL.M_NULL), 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_DISP, MIL.M_NULL); // 프레임 저장 버퍼

            //MIL_ID MilDisplay = MIL.MdispAlloc(milSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, MIL.M_NULL); // 프레임 표시 디스플레이

            //MIL.MdispSelect(MilDisplay, MilBuffer); // 디스플레이에서 표시할 버퍼

            // System control



            //MIL.MdigControl(milDigitizer, MIL.M_GRAB_TRIGGER_SOURCE, MIL.M_AUX_IO4); // 실제 Grab에서 사용 될 Trigger Source
            MIL.MdigControl(milDigitizer, MIL.M_GRAB_TRIGGER_SOURCE, MIL.M_TIMER1); // 실제 Grab에서 사용 될 Trigger Source
            MIL.MdigControl(milDigitizer, MIL.M_GRAB_TRIGGER_STATE, MIL.M_ENABLE);  // Grab Trigger 활성화
            MIL.MdigControl(milDigitizer, MIL.M_IO_MODE + MIL.M_AUX_IO4, MIL.M_INPUT);  // M_AUX_IO8 의 용도 IN 설정
            MIL.MdigControl(milDigitizer, MIL.M_IO_MODE + MIL.M_AUX_IO7, MIL.M_OUTPUT); // M_AUX_IO9 의 용도 OUT 설정
            MIL.MdigControl(milDigitizer, MIL.M_TIMER_STATE + MIL.M_TIMER1, MIL.M_ENABLE);  // Gab Trigger 에서 사용한 M_TIMER1 을 활성화
            //MIL.MdigControl(milDigitizer, MIL.M_TIMER_DURATION + MIL.M_TIMER1, 100000000); // nsec 단위로 M_TIMER1 의 활성화시간 설정
            //MIL.MdigControl(milDigitizer, MIL.M_TIMER_DELAY + MIL.M_TIMER1, 1000000);       // nsec 단위로 M_TIMER1 의 비활성화시간 설정
            MIL.MdigControl(milDigitizer, MIL.M_TIMER_TRIGGER_SOURCE + MIL.M_TIMER1, MIL.M_AUX_IO4); // M_TIMER1 의 작동 소스를 M_AUX_IO4 로 설정
            MIL.MdigControl(milDigitizer, MIL.M_IO_SOURCE + MIL.M_AUX_IO7, MIL.M_TIMER1); // M_TIMER1 의 OUTPUT을 M_AUX_IO9 으로 설정




            //	dud

            //MIL.MdigGrabContinuous(MilDigitizer, MilBuffer); // 연속 그랩 실시, MdigGrab() 적용 가능
            //MIL.MdigHalt(MilDigitizer); // 그랩 정지하여 프레임 획득
            //MIL.MdispFree(MilDisplay);
            //MIL.MbufFree(MilBuffer);
            //MIL.MdigFree(MilDigitizer);
            //MIL.MsysFree(MilSystem);
            //MIL.MappFree(MilApplication);

        }

        public class HookDataStruct
        {
            public MIL_ID MilDigitizer;
            public MIL_ID MilImageDisp;
            public int ProcessedImageCount;
        };
        //static int mTrgGrabCount = 0;
        private const int STRING_LENGTH_MAX = 20;
        private const int STRING_POS_X = 5;
        private const int STRING_POS_Y = 3;
        public int mTargetTriggerCount = 0;
        public int mRequestedTriggerCount = 0;
        public static long[] mGrabTiming = new long[11000];
        public static DateTime mTriggerDateTime = new DateTime();
        public double[] mGrabAbsTiming = new double[11000];
        public static long mLastGrabTiming = 0;
        public static int mTriggeredFrameCount = 0;

        public int GetTriggeredframeCount()
        {
            return mTriggeredFrameCount;
        }
        public void SetTriggeredframeCount(int cnt)
        {
            mTriggeredFrameCount = cnt;
        }

        //int mNumImageFromTrigger = 0;
        static MIL_INT ProcessingFunction(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        {
            //  최대 프레임 개수 넘어가면 자동으로 앞쪽 프레임은 삭제됨
            MIL_ID ModifiedBufferId = MIL.M_NULL;

            //// this is how to check if the user data is null, the IntPtr class
            //// contains a member, Zero, which exists solely for this purpose
            //if (!IntPtr.Zero.Equals(HookDataPtr))
            //{
            //    // get the handle to the DigHookUserData object back from the IntPtr
            //    // get a reference to the DigHookUserData object
            //    //GCHandle hUserData = GCHandle.FromIntPtr(HookDataPtr);
            //    //HookDataStruct UserData = hUserData.Target as HookDataStruct;
            //    //int fIndex = UserData.ProcessedImageCount;

            //    // Retrieve the MIL_ID of the grabbed buffer.
            //    MIL.MdigGetHookInfo(HookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref ModifiedBufferId);

            //    Task.Run(()=>MIL.MimResize(ModifiedBufferId, milCommonImageResize[mTriggeredFrameCount], mModelScale, mModelScale, MIL.M_DEFAULT));

            //    //  영상에 프레임번호 써주기
            //    //  MIL.MgraText(MIL.M_DEFAULT, ModifiedBufferId, STRING_POS_X, STRING_POS_Y, String.Format("{0}", mTriggeredFrameCount));

            //    SupremeTimer.QueryPerformanceCounter(ref mGrabTiming[mTriggeredFrameCount]);
            //    mLastGrabTiming = mGrabTiming[mTriggeredFrameCount];

            //    //UserData.ProcessedImageCount++;

            //    mTriggeredFrameCount++;
            //}

            MIL.MdigGetHookInfo(HookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref ModifiedBufferId);
            //int fCnt = mTriggeredFrameCount;

            //MIL_ID mbId = ModifiedBufferId;
            //Task.Run(() => MIL.MimResize(mbId, milCommonImageResize[fCnt % 10000], mModelScale, mModelScale, MIL.M_DEFAULT));

            SupremeTimer.QueryPerformanceCounter(ref mLastGrabTiming);
            mGrabTiming[mTriggeredFrameCount % 10000] = mLastGrabTiming;
            mTriggeredFrameCount++;

            return 0;
        }

        public DateTime GetLastTriggerTime()
        {
            return mTriggerDateTime;
        }

        public bool mExternalTriggerOrg = false;
        public bool mTriggerGrabFinish = false;
        //HookDataStruct UserHookData;
        //GCHandle hUserData;
        //MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr;
        //GCHandle hUserData;
        //MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr = null;
        public long mCurTime = 0;
        public long mBeforeTime = 0;
        public long mAfterTime = 0;

        public void ForceTriggerTime()
        {
            mTriggerDateTime = DateTime.Now;
        }

        public long ExternalTriggerOrgTmp(ref double fperSec, ref int frameCount)
        {
            mTriggerDateTime = DateTime.Now;

            mTriggerGrabFinish = true;

            int ProcessFrameCount = mTriggeredFrameCount;
            if (ProcessFrameCount > 0)
                //dAFZM_FrameCount = (int)ProcessFrameCount;
                dAFZM_FrameCount = mTriggeredFrameCount;//

            if (dAFZM_FrameCount > MAX_TRGGRAB_COUNT)
                dAFZM_FrameCount = MAX_TRGGRAB_COUNT;

            for (int i = 0; i < mTriggeredFrameCount; i++)
                GrabB(i);

            fperSec = 1000;
            //MessageBox.Show(ProcessFrameCount.ToString() + " frames grabbed at " + ProcessFrameRate.ToString("F1") + " frames/sec " + (1000.0 / ProcessFrameRate).ToString("f3") + " ms/frame).");
            //StreamWriter wr = new StreamWriter("GrabTiming.csv");
            SupremeTimer.QueryPerformanceCounter(ref mGrabTiming[0]);

            for (int i = 0; i < ProcessFrameCount; i++)
                mGrabAbsTiming[i] = mGrabTiming[0] + i * 0.001;

            mBeforeTime = mGrabTiming[0];
            //frameCount = (int)ProcessFrameCount;
            frameCount = mTriggeredFrameCount;
            mExternalTriggerOrg = false;
            return mCurTime;
        }

        public long ExternalTriggerOrg(ref double fperSec, ref int frameCount)
        {
            //mMatroxMsg = "Target Frame Count to Grab = " + mTargetTriggerCount.ToString() + "\r\n";

            mGrabTiming = new long[10000];


            mExternalTriggerOrg = true;

            HookDataStruct UserHookData = new HookDataStruct();
            UserHookData.MilDigitizer = milDigitizer;
            UserHookData.MilImageDisp = milImageDisp;
            UserHookData.ProcessedImageCount = 0;
            GCHandle hUserData = GCHandle.Alloc(UserHookData);
            MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);

            //////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////
            //   MdigGrabContinuous() 으로 Trigger image 확인목적 -> 확인 완료
            //MIL.MbufClear(milImageDisp, MIL.M_COLOR_BLACK);
            //MIL.MdispSelect(milDisplay, milImageDisp);
            //MIL.MdigGrabContinuous(milDigitizer, milImageDisp);
            //////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////
            // 연속저장 목적
            //  ProcessingFunctionPtr 내부에서 아무것도 안해도 저장은 됨.
            //mTrgGrabCount = 0;

            //  Clear mGrabTiming[]
            //for (int i = 0; i < mGrabTiming.Length; i++)
            //    mGrabTiming[i] = 0;
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            mLastGrabTiming = 0;

            //  200 프레임 이하로 측정하는 경우의 Timeout 시간
            mWaitLimitForNextTriggerSec = mWaitLimitForNextTrigger / 1000.0;
            //  200 프레임 초과로 측정하는 경우의 Timeout 시간은 
            //  200 ~ 800 frame 은 40msec
            //  800 ~ frame 은 12msec

            mTriggerDateTime = DateTime.Now;

            mTriggerGrabFinish = false;

            ////////////////////////////////////////////////////
            //SupremeTimer.QueryPerformanceCounter(ref mBeforeTime);
            ////////////////////////////////////////////////////
            while (mRequestedTriggerCount == 0)
            {
                if (mAbort)
                {
                    //mAbort = false;
                    SupremeTimer.QueryPerformanceCounter(ref mCurTime);
                    mTriggerGrabFinish = true;
                    mExternalTriggerOrg = false;
                    mRequestedTriggerCount = 0;
                    frameCount = 0;
                    return mCurTime;
                }
                Thread.Sleep(10);
            }

            mTriggeredFrameCount = 0;
            MIL_INT ProcessFrameCount = 0;
            double ProcessFrameRate = 0;

            mTrgBufLength = mRequestedTriggerCount;
            mTargetTriggerCount = mRequestedTriggerCount;
            //AckSignal(1, true);
            //Thread.Sleep(100);

            if (mRequestedTriggerCount == 3900)
                mTriggeredFrameCount = 0;

            //StandbyTrigger();

            MilGrabBufferListSize = MAX_TRGGRAB_COUNT;
            MIL.MdigProcess(milDigitizer, milCommonImageGrab, MAX_TRGGRAB_COUNT, MIL.M_START, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
            //MIL.MdigProcess(milDigitizer, milCommonImageGrab, MilGrabBufferListSize, MIL.M_START, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));

            StandbyTrigger();

            SupremeTimer.QueryPerformanceCounter(ref mCurTime);

            //double readyTime = (mCurTime - lstart) / (double)(lTimerFrequency);

            ////////////////////////////////////////////////////
            //double digSettingTime = (mCurTime - mBeforeTime) / (double)lTimerFrequency;
            //MessageBox.Show("digSettingTime = " + digSettingTime.ToString("F3") + "sec");
            ////////////////////////////////////////////////////

            ResetTrigger(mTrgBufLength);
            //  아래 함수를 호출하면 멈춰버린다. 오류도 안나고 그냥 멈춤. Top 은 됬는데 합수자체가 종료되지 않음.

            ////////////////////////////////////////////////////
            //SupremeTimer.QueryPerformanceCounter(ref mBeforeTime);
            ////////////////////////////////////////////////////
            ///
            //AckSignal(1, false);

            MIL.MdigProcess(milDigitizer, milCommonImageGrab, MilGrabBufferListSize, MIL.M_STOP, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));

            ////////////////////////////////////////////////////
            //SupremeTimer.QueryPerformanceCounter(ref mAfterTime);
            //double digFreeTime = (mAfterTime - mBeforeTime) / (double)lTimerFrequency;
            //MessageBox.Show("digFreeTime = " + digFreeTime.ToString("F3") + "sec");
            ////////////////////////////////////////////////////

            mTriggerGrabFinish = true;

            //MIL.MdigProcess(milDigitizer, milCommonImageGrab, MilGrabBufferListSize, MIL.M_START, MIL.M_DEFAULT, null, GCHandle.ToIntPtr(hUserData));
            //Thread.Sleep(10000);
            //MIL.MdigProcess(milDigitizer, milCommonImageGrab, MilGrabBufferListSize, MIL.M_STOP, MIL.M_DEFAULT, null, GCHandle.ToIntPtr(hUserData));

            // Free the GCHandle when no longer used
            HookDataStruct UserData = hUserData.Target as HookDataStruct;

            hUserData.Free();

            // Print statistics.
            MIL.MdigInquire(milDigitizer, MIL.M_PROCESS_FRAME_COUNT, ref ProcessFrameCount);
            MIL.MdigInquire(milDigitizer, MIL.M_PROCESS_FRAME_RATE, ref ProcessFrameRate);

            if (ProcessFrameCount > 0)
                //dAFZM_FrameCount = (int)ProcessFrameCount;
                dAFZM_FrameCount = mTriggeredFrameCount;//

            if (dAFZM_FrameCount > MAX_TRGGRAB_COUNT)
                dAFZM_FrameCount = MAX_TRGGRAB_COUNT;

            fperSec = ProcessFrameRate;
            //MessageBox.Show(ProcessFrameCount.ToString() + " frames grabbed at " + ProcessFrameRate.ToString("F1") + " frames/sec " + (1000.0 / ProcessFrameRate).ToString("f3") + " ms/frame).");
            StreamWriter wr = new StreamWriter("C:\\B7WideTest\\DoNotTouch\\Admin\\GrabTiming.csv");
            for (int i = 0; i < ProcessFrameCount; i++)
            {
                mGrabAbsTiming[i] = (mGrabTiming[i] - mGrabTiming[0]) / (double)(lTimerFrequency);
                wr.WriteLine(mGrabAbsTiming[i].ToString("F6"));
            }
            wr.Close();

            mBeforeTime = mGrabTiming[0];
            //frameCount = (int)ProcessFrameCount;
            frameCount = mTriggeredFrameCount;
            mExternalTriggerOrg = false;
            mRequestedTriggerCount = 0;
            return mCurTime;
        }
        //  언제 수정된거냐

        public bool mAbort = false;
        public bool mFinishVisionData = false;
        //public void ResetTrigger(int maxGrab)
        //{
        //    long curTime = 0;
        //    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

        //    //int whichone = 0;
        //    int maxTime = maxGrab + 10;

        //    while (true)
        //    {
        //        Thread.Sleep(1);

        //        if (mTriggeredFrameCount >= maxGrab)
        //        {
        //            //  SuperFast Mode 에서 영상처리하는 중에 Stop 해버리면 영상 처리에 문제가 생겼던 것 같아서 처리완료시까지 Stop 을 대기시키는 효과
        //            if (mFinishVisionData == true)
        //            {
        //                mMatroxMsg += "Final Frame Count Grabbed = " + mTriggeredFrameCount.ToString() + "\r\n";
        //                mAbort = false;
        //                break;
        //            }

        //            //////////////////////////////////////////////////////////////////////
        //            //   SuperFast Mode 를 쓰지 않는 경우 바로 중단 필요
        //            //mMatroxMsg += "Final Frame Count Grabbed = " + mTriggeredFrameCount.ToString() + "\r\n";
        //            //mAbort = false;
        //            //break;
        //        }
        //        if (mAbort)
        //        {
        //            if (mFinishVisionData == true)
        //            {
        //                mAbort = false;
        //                break;
        //            }
        //        }

        //        SupremeTimer.QueryPerformanceCounter(ref curTime);
        //        double waitingTime = (curTime - mLastGrabTiming) / (double)(lTimerFrequency);

        //        if (waitingTime > mWaitLimitForNextTriggerSec && mLastGrabTiming > 0 )
        //        {
        //            if (mFinishVisionData == true)
        //            {
        //                mMatroxMsg += "Timeout 1 : Final Frame Count Grabbed = " + mTriggeredFrameCount.ToString() + "  waiting " + waitingTime.ToString("F3") + "sec \r\n";
        //                mAbort = false;
        //                break;
        //            }
        //            else
        //            {
        //                mMatroxMsg += "Timeout 2 : Final Frame Count Grabbed = " + mTriggeredFrameCount.ToString() + "  waiting " + waitingTime.ToString("F3") + "sec \r\n";
        //                mAbort = false;
        //                break;

        //            }
        //        }
        //    }
        //}
        public static long mStartWaitingTriggerTiming = 0;
        public long lTimerFrequency = 0;
        public void ResetTrigger(int maxGrab)
        {
            long curTime = 0;
            SupremeTimer.QueryPerformanceCounter(ref mStartWaitingTriggerTiming);
            double limitTime = maxGrab / 1000.0 + mWaitLimitForNextTriggerSec + 16;
            double waitingTime = 0;
            //int whichone = 0;
            mMatroxMsg = "";
            int oldTriggeredFrameCount = 0;
            //mTriggeredFrameCount
            int overtimeCount = 0;
            int skipCount = 0;
            while (true)
            {
                Thread.Sleep(10);

                if (oldTriggeredFrameCount == mTriggeredFrameCount && mLastGrabTiming > 0)
                {
                    if (skipCount++ > 6)
                    {
                        mMatroxMsg += mTriggeredFrameCount.ToString() + "\r\n";
                        mAbort = false;
                        break;
                    }
                }

                if (mTriggeredFrameCount >= maxGrab)
                {
                    mMatroxMsg += "Final Frame Count Grabbed = " + mTriggeredFrameCount.ToString() + "\r\n";
                    mAbort = false;
                    break;
                }
                if (mAbort)
                {
                    if (mFinishVisionData == true)
                    {
                        mAbort = false;
                        mMatroxMsg += "Aborted. TriggeredFrame = " + mTriggeredFrameCount.ToString() + " TargetFrame = " + maxGrab.ToString() + "r\n";
                        break;
                    }
                }
                oldTriggeredFrameCount = mTriggeredFrameCount;
                SupremeTimer.QueryPerformanceCounter(ref curTime);

                waitingTime = (curTime - mStartWaitingTriggerTiming) / (double)(lTimerFrequency);
                if (waitingTime > limitTime || waitingTime > 20)
                {
                    if (bNoHostPC)
                    {
                        //  In case of Direct trigger from MCU Block, do not make "Timeout 4"
                        mMatroxMsg = "Timeout 4 \r\n";
                        break;
                    }
                    else
                    {
                        //  In case of Trigger controlled By Host PC. Make "Timeout 5" 
                        mMatroxMsg = "Timeout 5 \tLimitTime = " + limitTime.ToString("F3") + "\twaitingTime = " + waitingTime.ToString("F3") + " TriggeredFrame = " + mTriggeredFrameCount.ToString() + " TargetFrame = " + maxGrab.ToString() + "r\n";
                        mAbort = true;
                        break;
                    }
                }
            }
        }

        public void CommonToReplayBuf(string name, int count)
        {
            switch (name)
            {
                case "AF Scan":
                case "AF Settling":
                    for (int i = 0; i < count; i++)
                        MIL.MbufCopy(milCommonImageGrab[i], milAFRelay[i]);
                    AFRelayCnt = count;
                    break;
                case "OIS X Scan":
                    for (int i = 0; i < count; i++)
                        MIL.MbufCopy(milCommonImageGrab[i], milXRelay[i]);
                    XRelayCnt = count;
                    break;
                case "OIS Y Scan":
                    for (int i = 0; i < count; i++)
                        MIL.MbufCopy(milCommonImageGrab[i], milYRelay[i]);
                    YRelayCnt = count;
                    break;
                case "OIS Matrix Scan":
                    for (int i = 0; i < count; i++)
                        MIL.MbufCopy(milCommonImageGrab[i], milCommonImageGrab6000[i]);
                    MRelayCnt = count;
                    break;
                case "OIS X Linearity Comp":
                    for (int i = 0; i < count; i++)
                        MIL.MbufCopy(milCommonImageGrab[i], milXLinRelay[i]);
                    XLinRelayCnt = count;
                    break;
                case "OIS Y Linearity Comp":
                    for (int i = 0; i < count; i++)
                        MIL.MbufCopy(milCommonImageGrab[i], milYLinRelay[i]);
                    YLinRelayCnt = count;
                    break;
            }
        }
        public void ReplayBuftoCommon(string name, int index)
        {
            switch (name)
            {
                case "AF Scan":
                case "AF Settling":
                    MIL.MbufCopy(milAFRelay[index], milCommonImageGrab[index]);
                    break;
                case "OIS X Scan":
                    MIL.MbufCopy(milXRelay[index], milCommonImageGrab[index]);
                    break;
                case "OIS Y Scan":
                    MIL.MbufCopy(milYRelay[index], milCommonImageGrab[index]);
                    break;
                case "OIS Matrix Scan":
                    MIL.MbufCopy(milCommonImageGrab6000[index], milCommonImageGrab[index]);
                    break;
                case "OIS X Linearity Comp":
                    MIL.MbufCopy(milXLinRelay[index], milCommonImageGrab[index]);
                    break;
                case "OIS Y Linearity Comp":
                    MIL.MbufCopy(milYLinRelay[index], milCommonImageGrab[index]);
                    break;
                case "Vision":
                    if (mTrgBufLength > 2500)
                    {
                        for (int i = 0; i < mTrgBufLength; i++)
                        {
                            MIL.MbufCopy(milCommonImageGrab6000[i], milCommonImageGrab[index]);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public Mat GrabLoadCropImg(int index, bool bDrawReference)
        {
            MIL.MdigGrab(milDigitizer, milCommonImageGrab[index]);
            MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_FRAME_END);
            //MIL.MbufExport("D:\\TestImage.bmp", MIL.M_BMP, milCommonImageGrab[index]);
            MIL.MbufCopy(milCommonImageGrab[index], milImageDisp);
            Mat cropImg = LoadCropMat(index);

            //Cv2.ImWrite("D:\\CVImage.bmp", src);

            if (bDrawReference)
            {
                Mat lOverlayedImg = new Mat();
                Cv2.CvtColor(cropImg, lOverlayedImg, ColorConversionCodes.GRAY2RGB);

                for (int i = 0; i < 5; i++)
                {
                    int x = (int)mFAL.mMarkPosOnPanel[i].X;
                    int y = (int)mFAL.mMarkPosOnPanel[i].Y;
                    Cv2.Line(lOverlayedImg, x - 10, y, x + 10, y, Scalar.OrangeRed, 1, LineTypes.Link4);
                    Cv2.Line(lOverlayedImg, x, y - 10, x, y + 10, Scalar.OrangeRed, 1, LineTypes.Link4);
                }
                //return BitmapConverter.ToBitmap(lOverlayedImg);
                return lOverlayedImg;
            }
            else
                //return BitmapConverter.ToBitmap(cropImg);
                return cropImg;
        }
        public void ReplayBufToDisp(string name, int delay)
        {
            switch (name)
            {
                case "AF Scan":
                    for (int i = 0; i < AFRelayCnt; i++)
                    {
                        MIL.MbufCopy(milAFRelay[i], milImageDisp);
                        Thread.Sleep(delay);
                    }
                    break;
                case "OIS X Scan":
                    for (int i = 0; i < XRelayCnt; i++)
                    {
                        MIL.MbufCopy(milXRelay[i], milImageDisp);
                        Thread.Sleep(delay+80);
                    }
                    break;
                case "OIS Y Scan":
                    for (int i = 0; i < YRelayCnt; i++)
                    {
                        MIL.MbufCopy(milYRelay[i], milImageDisp);
                        Thread.Sleep(delay+80);
                    }
                    break;
                case "OIS Matrix Scan":
                    for (int i = 0; i < MRelayCnt; i++)
                    {
                        MIL.MbufCopy(milCommonImageGrab6000[i], milImageDisp);
                        Thread.Sleep(delay);
                    }
                    break;
                case "OIS X Linearity Comp":
                    for (int i = 0; i < XLinRelayCnt; i++)
                    {
                        MIL.MbufCopy(milXLinRelay[i], milImageDisp);
                        Thread.Sleep(delay);
                    }
                    break;
                case "OIS Y Linearity Comp":
                    for (int i = 0; i < YLinRelayCnt; i++)
                    {
                        MIL.MbufCopy(milYLinRelay[i], milImageDisp);
                        Thread.Sleep(delay);
                    }
                    break;
                case "Vision":
                    if (mTrgBufLength > 6000)
                    {
                        for (int i = 0; i < mTrgBufLength; i++)
                        {
                            MIL.MbufCopy(milCommonImageGrab[i], milImageDisp);
                            Thread.Sleep(1);
                        }
                    }
                    else if (mTrgBufLength > 2500)
                    {
                        for (int i = 0; i < mTrgBufLength; i++)
                        {
                            MIL.MbufCopy(milCommonImageGrab6000[i], milImageDisp);
                            Thread.Sleep(1);
                        }
                    }
                    break;
            }
        }
    }

}

