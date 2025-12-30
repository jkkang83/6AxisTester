using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Forms.VisualStyles;
using FZ4P.Helper;

namespace FZ4P
{
    public class Recipe
    {
        public CurrentPath Current { get; set; }
        public Condition Condition { get; set; }
        public AFPidSet AfPidSet { get; set; }
        public XPidSet XPidSet { get; set; }
        public YPidSet YPidSet { get; set; }
        public CodeScript CodeScript { get; set; }
        public Spec Spec { get; set; }
        public Model Model { get; set; }
        public Option Option { get; set; }
        public List<PassFail> PassFails { get; set; }
        public TotalYield yield { get; set; }
        public TestTime tt { get; set; }
        public VisionFile vsFile { get; set; }
        public RetryCount RetryCnt { get; set; }
        public Password pw { get; set; }

        public Recipe()
        {
            Current = new CurrentPath();
            if (File.Exists(STATIC.appPath.CurrentPath))
                Current = DataIO.DeserializeXMLFileToObject<CurrentPath>(STATIC.appPath.CurrentPath);

            if (!Directory.Exists(STATIC.appPath.RootDir)) Directory.CreateDirectory(STATIC.appPath.RootDir);
            if (!Directory.Exists(STATIC.appPath.DataDir)) Directory.CreateDirectory(STATIC.appPath.DataDir);
            if (!Directory.Exists(STATIC.appPath.RecipeDir)) Directory.CreateDirectory(STATIC.appPath.RecipeDir);
            if (!Directory.Exists(STATIC.appPath.SpecDir)) Directory.CreateDirectory(STATIC.appPath.SpecDir);
            if (!Directory.Exists(STATIC.appPath.PackageDir)) Directory.CreateDirectory(STATIC.appPath.PackageDir);
            string res = string.Empty;
            res = AppHelper.PKGRelease(STATIC.appPath.PackageDir, "*.rcp", STATIC.appPath.RecipeDir);
            if (res != string.Empty) Current.ConditionName = Path.GetFileName(res);
            res = AppHelper.PKGRelease(STATIC.appPath.PackageDir, "*.spc", STATIC.appPath.SpecDir);
            if (res != string.Empty) Current.SpecName = Path.GetFileName(res);
            res = AppHelper.PKGRelease(STATIC.appPath.PackageDir, "*.txt", STATIC.appPath.RootDir);

            Current.SerializeToXMLFile(STATIC.appPath.CurrentPath);

            Condition = new Condition();
            if (File.Exists(STATIC.appPath.RecipeDir + Current.ConditionName))
                Condition = DataIO.DeserializeXMLFileToObject<Condition>(STATIC.appPath.RecipeDir + Current.ConditionName);

            Spec = new Spec();
            Spec.InitSpecList();
            if (File.Exists(STATIC.appPath.SpecDir + Current.SpecName))
            {
                Spec compare = new Spec();
                compare = DataIO.DeserializeXMLFileToObject<Spec>(STATIC.appPath.SpecDir + Current.SpecName);
                for (int i = 0; i < compare.specList.Count; i++)
                {
                    int index = Spec.specList.FindIndex(x => x.DisplayName == compare.specList[i].DisplayName);
                    if (index != -1)
                    {
                        Spec.specList[index].MinSpec = compare.specList[i].MinSpec;
                        Spec.specList[index].MaxSpec = compare.specList[i].MaxSpec;
                        Spec.specList[index].OnOff = compare.specList[i].OnOff;
                        Spec.specList[index].FailCnt = compare.specList[i].FailCnt;
                        Spec.specList[index].InspectionType = compare.specList[i].InspectionType;
                    }
                }
            }

            AfPidSet = new AFPidSet();
            AfPidSet.Init(Current.AFPidPath, "PID\\");
            XPidSet = new XPidSet();
            XPidSet.Init(Current.XPidPath, "PID\\");
            YPidSet = new YPidSet();
            YPidSet.Init(Current.YPidPath, "PID\\");
            CodeScript = new CodeScript();
            CodeScript.Init(Current.CodeScriptPath, "PID\\");

            Model = new Model();

            Option = new Option();
            if(File.Exists(STATIC.appPath.OptionPath))
                Option = DataIO.DeserializeXMLFileToObject<Option>(STATIC.appPath.OptionPath);

            vsFile = new VisionFile();
            if (File.Exists(STATIC.appPath.VisionFileDir))
                vsFile = DataIO.DeserializeXMLFileToObject<VisionFile>(STATIC.appPath.VisionFileDir);
            else DataIO.SerializeToXMLFile(vsFile, STATIC.appPath.VisionFileDir);

            yield = new TotalYield();
            if (File.Exists(STATIC.appPath.YieldPath))
                yield = DataIO.DeserializeXMLFileToObject<TotalYield>(STATIC.appPath.YieldPath);


            PassFails = new List<PassFail>();
            for (int i = 0; i < 2; i++)
            {
                PassFails.Add(new PassFail());
                for (int j = 0; j < (int)SpecItem.Length; j++) PassFails[i].Results.Add(new ResultItems());
            }
            tt = new TestTime();
            if (File.Exists(STATIC.appPath.TestTimeDir)) tt = DataIO.DeserializeXMLFileToObject<TestTime>(STATIC.appPath.TestTimeDir);
           
            RetryCnt = new RetryCount();

            pw = new Password();
            if (File.Exists(STATIC.appPath.PasswordDir))
                pw = DataIO.DeserializeXMLFileToObject<Password>(STATIC.appPath.PasswordDir);
        }
    }
    public class BaseRecipe
    {
        public List<object[]> Param = new List<object[]>();
        public string CurrentName { get; set; }
        public string FilePath { get; set; }
        public string[] ReadArry { get; set; }
        public bool bChange = false;
        public string InitDir { get; set; }
        public string Ext { get; set; }
        public virtual void Init(string current, string subDir)
        {
            if (!Directory.Exists(STATIC.appPath.BaseDir)) Directory.CreateDirectory(STATIC.appPath.BaseDir);
            InitDir = STATIC.appPath.BaseDir + subDir;
            Ext = Path.GetExtension(current);
            if (!Directory.Exists(InitDir)) Directory.CreateDirectory(InitDir);
            FilePath = STATIC.appPath.BaseDir + subDir + current;

            CurrentName = current;
            if (!File.Exists(FilePath)) Save();

            Read();
        }
        public virtual void Save(string filePath = "")
        {
        }
        public virtual void Read(string filePath = "")
        {
            if (!Directory.Exists(STATIC.appPath.RootDir)) Directory.CreateDirectory(STATIC.appPath.RootDir);
        }
        public virtual void SetParam()
        {
        }
        public virtual void SetParam(string key, string comment, object val)
        {
            for(int i = 0; i < Param.Count; i++)
            {
                if (Param[i][0].ToString() == key && Param[i][1].ToString() == comment)
                {
                    Param[i][2] = val;
                }
                if (Param[i][0].ToString() == key && comment == "")
                {
                    Param[i][1] = val;
                }
            }
        }
    }

    public class TestTime
    {
        public double CurrentST { get; set; } = 0;

        public double St { get; set; } = 0;
        public int Count { get; set; } = 0;
    }
    public class Option
    {

        [Option("Save Raw Data")] public bool SaveRawData { get; set; }
        [Option("Screen Capture")] public bool ScreenCapture { get; set; }
  //      [Option("Fixed Center")] public bool FixedCenter { get; set; }
        [Option("Write Result to DriverIC")] public bool WriteResultToDriverIC { get; set; }
        [Option("Safety Sensor Enable")] public bool SafeSensor { get; set; }
        [Option("AF Dir Reverse")] public bool AFDirReverse { get; set; }
        [Option("X Dir Reverse")] public bool XDirReverse { get; set; }
        [Option("Y Dir Reverse")] public bool YDirReverse { get; set; }
        [Option("XY Pos Reverse")] public bool XYPosReverse { get; set; }
        [Option("Socket Sensor Use")] public bool SocketSensorUse { get; set; }
        [Option("Settling Graph Visible")] public bool settlingGraphVisible { get; set; }
        [Option("Continue testing On Fail")] public bool ContinueTestingOnFail { get; set; }
      //  [Option("Fail Retry")] public bool FailRetry { get; set; }
        [Option("DryRun Mode")] public bool DryRunMode { get; set; }

    }
    public class Condition
    {
        [Condition("ToDoList", "", "", "", "")] public List<string> ToDoList { get; set; } = new List<string>();
        //[Condition("PID", "OIS PID Ver.", "OIS Init", "", "_")] public int OISPIDVer { get; set; } = 11;
        //[Condition("PID", "AF PID Ver.", "AF Initial", "", "_")] public int AFPIDVer { get; set; } = 11;
        [Condition("Common", "Drv AF Step", "AF Scan", "", "code")] public int iDrvAFStep { get; set; } = 40;
        [Condition("Common", "Drv X Step", "OIS X Scan", "", "code")] public int iDrvXStep { get; set; } = 400;
        [Condition("Common", "Drv Y Step", "OIS Y Scan", "", "code")] public int iDrvYStep { get; set; } = 400;
        [Condition("Common", "Drv Step Interval AF", "AF Scan", "", "msec")] public int iDrvStepIntervalZ { get; set; } = 40;
        [Condition("Common", "Drv Step interval X", "OIS X Scan", "", "msec")] public int iDrvStepIntervalX { get; set; } = 40;
        [Condition("Common", "Drv step Interval Y", "OIS Y Scan", "", "msec")] public int iDrvStepIntervalY { get; set; } = 40;
     
        [Condition("AF", "Drv Code Min", "AF Scan", "", "code")] public int iAFDrvCodeMin { get; set; } = 8;
        [Condition("AF", "Drv Code Max", "AF Scan", "", "code")] public int iAFDrvCodeMax { get; set; } = 4088;
        [Condition("AF", "Cross Axis Offset X", "AF Scan", "", "code")] public int iAFCrossOffsetX { get; set; } = 2048;
        [Condition("AF", "Cross Axis Offset Y", "AF Scan", "", "code")] public int iAFCrossOffsetY { get; set; } = 2048;
        [Condition("AF", "Plot Range", "AF Scan", "", "code")] public int iAFPlotRange { get; set; } = 2048;
        [Condition("AF", "Code Range", "AF Scan", "", "code")] public int iAFCodeRange { get; set; } = 2048;
        [Condition("AF", "Stroke Range", "AF Scan", "", "um")] public int iAFStrokeRange { get; set; } = 500;
        [Condition("AF Settling", "Standby Code", "AF Settling", "", "code")] public int iAFStandbyCode { get; set; } = 8;
        [Condition("AF Settling", "Jump Step Code", "AF Settling", "", "code")] public int iAFJumpStepCode { get; set; } = 2048;
        [Condition("AF Settling", "Settling Criteria", "AF Settling", "", "%")] public double iAFSettlingCriteria { get; set; } = 0.05;


        [Condition("X", "Drv Code Min", "OIS X Scan", "", "code")] public int iXDrvCodeMin { get; set; } = 8;
        [Condition("X", "Drv Code Max", "OIS X Scan", "", "code")] public int iXDrvCodeMax { get; set; } = 4088;
        [Condition("X", "Cross Axis Offset", "OIS X Scan", "", "code")] public int iXCrossOffset { get; set; } = 2048;
        [Condition("X", "Cross Axis Offset AF", "OIS X Scan", "", "code")] public int iXCrossOffsetAf { get; set; } = 2048;
        [Condition("X", "Plot Range", "OIS X Scan", "", "code")] public int iXPlotRange { get; set; } =  2048;
        [Condition("X", "Code Range", "OIS X Scan", "", "code")] public int iXCodeRange { get; set; } = 2048;
        [Condition("X", "stroke Range", "OIS X Scan", "", "um")] public int iXStrokeRange { get; set; } = 500;

        [Condition("Y1", "Drv Code Min", "OIS Y Scan", "", "code")] public int iYDrvCodeMin { get; set; } = 8;
        [Condition("Y1", "Drv Code Max", "OIS Y Scan", "", "code")] public int iYDrvCodeMax { get; set; } = 4088;
        [Condition("Y2", "Drv Code Min", "OIS Y Scan", "", "code")] public int iY2DrvCodeMin { get; set; } = 8;
        [Condition("Y2", "Drv Code Max", "OIS Y Scan", "", "code")] public int iY2DrvCodeMax { get; set; } = 4088;

        [Condition("Y", "Cross Axis Offset", "OIS Y Scan", "", "code")] public int iYCrossOffset { get; set; } = 2048;
        [Condition("Y", "Cross Axis Offset AF", "OIS Y Scan", "", "code")] public int iYCrossOffsetAf { get; set; } = 2048;
        [Condition("Y", "Plot Range", "OIS Y Scan", "", "code")] public int iYPlotRange { get; set; } = 2048;
        [Condition("Y", "Code Range", "OIS Y Scan", "", "code")] public int iYCodeRange { get; set; } = 2048;
        [Condition("Y", "Stroke Range", "OIS Y Scan", "", "um")] public int iYStrokeRange { get; set; } = 500;

        [Condition("AF OL Aging", "Frequency", "AF OpenLoopAging", "", "Hz")] public int AFOpenLoopFreq { get; set; } = 10;
        [Condition("AF OL Aging", "Count", "AF OpenLoopAging", "", "-")] public int AFOpenLoopCount { get; set; } = 10;


        [Condition("OIS XYZ Aging", "Frequency", "OIS XYZ Aging", "", "-")] public int CLAgingFreq { get; set; } = 10;
        [Condition("OIS XYZ Aging", "Count", "OIS XYZ Aging", "", "-")] public int CLAgingCount { get; set; } = 30;
    
        [Condition("OIS XYZ Aging", "AF Start", "OIS XYZ Aging", "", "-")] public int CLAgingAFMin { get; set; } = 200;
        [Condition("OIS XYZ Aging", "AF End", "OIS XYZ Aging", "", "-")] public int CLAgingAFMax { get; set; } = 3000;
        [Condition("OIS XYZ Aging", "OIS Start", "OIS XYZ Aging", "", "-")] public int CLAgingOISMin { get; set; } = 100;
        [Condition("OIS XYZ Aging", "OIS End", "OIS XYZ Aging", "", "-")] public int CLAgingOISMax { get; set; } = 4000;


        [Condition("AF Aging", "Count", "AF Aging", "", "-")] public int AFSCanAgingCount { get; set; } = 3;
        [Condition("AF Aging", "Step", "AF Aging", "", "-")] public int AFScanAgingStep { get; set; } = 256;
        [Condition("AF Aging", "delay", "AF Aging", "", "-")] public int AFScanAgingDelay { get; set; } = 15;
      


        //[Condition("AF Pre Driving", "delay", "AF PreDriving", "", "-")] public int AFPreDrvDelay { get; set; } = 30;
        //[Condition("AF Pre Driving", "Count", "AF PreDriving", "", "-")] public int AFPReDrvCount { get; set; } = 3;


        //[Condition("AF EPA", "Target Stroke", "AF EPA", "", "code")] public int AFEPATarget { get; set; } = 700;
        //[Condition("AF EPA", "POSVT", "AF EPA", "", "code")] public int AFPOSVT { get; set; } = 256;
        //[Condition("AF EPA", "NEGVT", "AF EPA", "", "code")] public int AFNEGVT { get; set; } = 256;

        //[Condition("OIS EPA", "X POSVT", "OIS EPA", "", "code")] public int XPOSVT { get; set; } = 264;
        //[Condition("OIS EPA", "X NEGVT", "OIS EPA", "", "code")] public int XNEGVT { get; set; } = 264;
        //[Condition("OIS EPA", "Y POSVT", "OIS EPA", "", "code")] public int YPOSVT { get; set; } = 264;
        //[Condition("OIS EPA", "Y NEGVT", "OIS EPA", "", "code")] public int YNEGVT { get; set; } = 264;

        //[Condition("AF Linearity Comp", "Start", "AF Linearity Comp", "", "code")] public int AfLinCompStart { get; set; } = 8;
        //[Condition("AF Linearity Comp", "End", "AF Linearity Comp", "", "code")] public int AfLinCompEnd { get; set; } = 4088;
        //[Condition("AF Linearity Comp", "Step", "AF Linearity Comp", "", "code")] public int AFLinCompStep { get; set; } = 120;
        //[Condition("AF Linearity Comp", "Move Delay", "AF Linearity Comp", "", "msec")] public int AFLinCompMoveDelay { get; set; } = 50;

        [Condition("OIS Linearity Comp", "Steps", "OIS LinearityCompensation", "", "code")] public int OISLincompStep { get; set; } = 32;
        [Condition("OIS Linearity Comp", "Code Margin", "OIS LinearityCompensation", "", "code")] public int OISLincompCodeMargin { get; set; } = 100;
        [Condition("OIS Linearity Comp", "X EPA POS", "OIS LinearityCompensation", "", "code")] public int OISLincompXEPAPos { get; set; } = 0;
        [Condition("OIS Linearity Comp", "X EPA NEG", "OIS LinearityCompensation", "", "code")] public int OISLincompXEPANeg { get; set; } = 0;
        [Condition("OIS Linearity Comp", "Y EPA POS", "OIS LinearityCompensation", "", "code")] public int OISLincompYEPAPos { get; set; } = 8;
        [Condition("OIS Linearity Comp", "Y EPA NEG", "OIS LinearityCompensation", "", "code")] public int OISLincompYEPANeg { get; set; } = 0;
      


  
        [Condition("OIS PM", "OIS Step", "OIS Phase Margin", "", "%")] public int iOISFRAstep { get; set; } = 15;
        [Condition("OIS PM", "X Chirp from", "OIS Phase Margin", "", "Hz")] public int iXChirpFrom { get; set; } = 250;
        [Condition("OIS PM", "X Chirp to", "OIS Phase Margin", "", "Hz")] public int iXChirpTo { get; set; } = 20;
        [Condition("OIS PM", "X Drv Amp", "OIS Phase Margin", "", "mV")] public int iXAmplitude { get; set; } = 60;
        [Condition("OIS PM", "X Min Phase", "OIS Phase Margin", "", "_")] public int PMXMinPhase { get; set; } = 0;
        [Condition("OIS PM", "X Gain Th", "", "OIS Phase Margin", "_")] public int PMXGainTH { get; set; } = 0;
        [Condition("OIS PM", "Y Chirp from", "OIS Phase Margin", "", "Hz")] public int iYChirpFrom { get; set; } = 250;
        [Condition("OIS PM", "Y Chirp to", "OIS Phase Margin", "", "Hz")] public int iYChirpTo { get; set; } = 100;
        [Condition("OIS PM", "Y Drv Amp", "OIS Phase Margin", "", "mV")] public int iYAmplitude { get; set; } = 75;
        [Condition("OIS PM", "Y Min Phase", "OIS Phase Margin", "", "_")] public int PMYMinPhase { get; set; } = 0;
        [Condition("OIS PM", "Y Gain Th", "OIS Phase Margin", "", "_")] public int PMYGainTH { get; set; } = 0;


        [Condition("AF PM", "AF Step", "AF Phase Margin", "", "%")] public int iAFFRAstep { get; set; } = 5;
        [Condition("AF PM", "AF Chirp from", "AF Phase Margin", "", "Hz")] public int iAFChirpFrom { get; set; } = 250;
        [Condition("AF PM", "AF Chirp to", "AF Phase Margin", "", "Hz")] public int iAFChirpTo { get; set; } = 100;
        [Condition("AF PM", "AF Drv Amp", "AF Phase Margin", "", "mV")] public double iAFAmplitude { get; set; } = 75;
        [Condition("AF PM", "AF Gain Th", "AF Phase Margin", "", "_")] public int PMAFGainTH { get; set; } = 0;
       



        //[Condition("High PM", "Step", "", "", "%")] public int iHighFRAstep { get; set; } = 5;    
        //[Condition("High PM", "X Chirp from", "", "", "Hz")] public int iHighXChirpFrom { get; set; } = 250;
        //[Condition("High PM", "X Chirp to", "", "", "Hz")] public int iHighXChirpTo { get; set; } = 100;
        //[Condition("High PM", "X Drv Amp", "", "", "mV")] public int iHighXAmplitude { get; set; } = 75;
        //[Condition("High PM", "Y Chirp from", "", "", "Hz")] public int iHighYChirpFrom { get; set; } = 250;
        //[Condition("High PM", "Y Chirp to", "", "", "Hz")] public int iHighYChirpTo { get; set; } = 100;
        //[Condition("High PM", "Y Drv Amp", "", "", "mV")] public int iHighYAmplitude { get; set; } = 75;

        [Condition("AF GM", "Chirp From", "AF Gain Margin", "", "Hz")] public int AFGMStartFreq { get; set; } = 2000;
        [Condition("AF GM", "Chirp To", "AF Gain Margin", "", "Hz")] public int AFGMEndFreq { get; set; } = 300;
        [Condition("AF GM", "Step", "AF Gain Margin", "", "Hz")] public int AFGMStep { get; set; } = 300;
        [Condition("AF GM", "Amp", "AF Gain Margin", "", "mV")] public int AFGMamp { get; set; } = 40;

        //[Condition("GM", "Loop", "", "", "#")] public int iGainLoop { get; set; } = 1;
        //[Condition("GM", "Step", "", "", "Hz")] public int iGainStep { get; set; } = 5;
        //[Condition("GM", "X Chirp from", "", "", "Hz")] public int iXGainFrom { get; set; } = 400;
        //[Condition("GM", "X Chirp to", "", "", "Hz")] public int iXGainTo { get; set; } = 100;
        //[Condition("GM", "X Drv Amplitude", "", "", "mV")] public double iXAmplitudeGain { get; set; } = 60;
        //[Condition("GM", "Y Chirp from", "", "", "Hz")] public int iYGainFrom { get; set; } = 250;
        //[Condition("GM", "Y Chirp to", "", "", "Hz")] public int iYGainTo { get; set; } = 100;
        //[Condition("GM", "Y Drv Amplitude", "", "", "mV")] public double iYAmplitudeGain { get; set; } = 60;

        [Condition("through Peak Hz", "Amp", "through Peak 25", "", "mV")] public int throughPeakAmp { get; set; } = 60;
        [Condition("through Peak Hz", "Freq", "through Peak 25", "", "Hz")] public int throughPeakFreq { get; set; } = 25;


        [Condition("LG @ 10Hz", "X Amp", "OIS Loopgain", "", "mV")] public double iLoppgainXAmp { get; set; } = 60;
        [Condition("LG @ 10Hz", "Y Amp", "OIS Loopgain", "", "mV")] public double iLoppgainYAmp { get; set; } = 60;


        [Condition("Auto Test", "THD", "Auto Test", "", "code")] public int AutoTest_THD { get; set; } = 120;
        [Condition("Auto Test", "AMP", "Auto Test", "", "mV")] public int AutoTest_AMP { get; set; } = 80;
        [Condition("Auto Test", "Error THD", "Auto Test", "", "code")] public int AutoTest_ErrTHD { get; set; } = 20;
        [Condition("Auto Test", "Init POS", "Auto Test", "", "code")] public int AutoTest_InitPos { get; set; } = 90;




        [Condition("AF Tilt", "Ref Code", "AF Scan", "", "code")] public int TiltRefCode { get; set; } = 1000;
        [Condition("AF Tilt", "Min Range", "AF Scan", "", "code")] public int TiltMinCode { get; set; } = 200;
        [Condition("AF Tilt", "Max Range", "AF Scan", "", "code")] public int TiltMaxCode { get; set; } = 3900;

        [Condition("AF Linearity", "Min Range", "AF Scan", "", "code")] public int AFLinMinRange { get; set; } = 200;
        [Condition("AF Linearity", "Max Range", "AF Scan", "", "code")] public int AFLinMaxRange { get; set; } = 3900;
        [Condition("AF Linearity", "Min Step", "AF Scan", "", "_")] public int AFLinMinStep { get; set; } = 0;
        [Condition("AF Linearity", "Max Step", "AF Scan", "", "_")] public int AFLinMaxStep { get; set; } = 0;
        [Condition("AF Linearity", "Min Stroke", "AF Scan", "", "um")] public double AFLinMinStroke { get; set; } = -310;
        [Condition("AF Linearity", "Max Stroke", "AF Scan", "", "um")] public double AFLinMaxStroke { get; set; } = 310;
        [Condition("AF Linearity", "Mode", "AF Scan", "", "0:CodeRange / 1:Step / 2:um")] public int AFLinMode { get; set; } = 0;

        [Condition("AF Hysteresis", "Min Range", "AF Scan", "", "code")] public int AFHysMinRange { get; set; } = 200;
        [Condition("AF Hysteresis", "Max Range", "AF Scan", "", "code")] public int AFHysMaxRange { get; set; } = 3900;
        [Condition("AF Hysteresis", "Min Step", "AF Scan", "", "_")] public int AFHysMinStep { get; set; } = 0;
        [Condition("AF Hysteresis", "Max Step", "AF Scan", "", "_")] public int AFhysMaxStep { get; set; } = 0;
        [Condition("AF Hysteresis", "Min Stroke", "AF Scan", "", "um")] public double AFHysMinStroke { get; set; } = -310;
        [Condition("AF Hysteresis", "Max Stroke", "AF Scan", "", "um")] public double AFHysMaxStroke { get; set; } = 310;
        [Condition("AF Hysteresis", "Mode", "AF Scan", "", "0:CodeRange / 1:Step / 2:um")] public int AFHysMode { get; set; } = 0;

        [Condition("AF Current", "Min Range", "AF Scan", "", "code")] public int AFCurrMinRange { get; set; } = 200;
        [Condition("AF Current", "Max Range", "AF Scan", "", "code")] public int AFCurrMaxRange { get; set; } = 3900;
        [Condition("AF Current", "Min Step", "AF Scan", "", "_")] public int AFCurrMinStep { get; set; } = 0;
        [Condition("AF Current", "Max Step", "AF Scan", "", "_")] public int AFCurrMaxStep { get; set; } = 0;
        [Condition("AF Current", "Min Stroke", "AF Scan", "", "um")] public double AFCurrMinStroke { get; set; } = -310;
        [Condition("AF Current", "Max Stroke", "AF Scan", "", "um")] public double AFCurrMaxStroke { get; set; } = 310;
        [Condition("AF Current", "Mode", "AF Scan", "", "0:CodeRange / 1:Step / 2:um")] public int AFCurrMode { get; set; } = 0;

        [Condition("X Linearity", "Min Range", "OIS X Scan", "", "code")] public int XLinMinRange { get; set; } = 648;
        [Condition("X Linearity", "Max Range", "OIS X Scan", "", "code")] public int XLinMaxRange { get; set; } = 3448;
        [Condition("X Linearity", "Min Step", "OIS X Scan", "", "_")] public int XLinMinStep { get; set; } = 0;
        [Condition("X Linearity", "Max Step", "OIS X Scan", "", "_")] public int XLinMaxStep { get; set; } = 0;
        [Condition("X Linearity", "Min Stroke", "OIS X Scan", "", "um")] public double XLinMinStroke { get; set; } = -310;
        [Condition("X Linearity", "Max Stroke", "OIS X Scan", "", "um")] public double XLinMaxStroke { get; set; } = 310;
        [Condition("X Linearity", "Mode", "OIS X Scan", "", "0:CodeRange / 1:Step / 2:um")] public int XLinMode { get; set; } = 0;

        [Condition("X Hysteresis", "Min Range", "OIS X Scan", "", "code")] public int XHysMinRange { get; set; } = 648;
        [Condition("X Hysteresis", "Max Range", "OIS X Scan", "", "code")] public int XHysMaxRange { get; set; } = 3448;
        [Condition("X Hysteresis", "Min Step", "OIS X Scan", "", "_")] public int XHysMinStep { get; set; } = 0;
        [Condition("X Hysteresis", "Max Step", "OIS X Scan", "", "_")] public int XHysMaxStep { get; set; } = 0;
        [Condition("X Hysteresis", "Min Stroke", "OIS X Scan", "", "um")] public double XHysMinStroke { get; set; } = -310;
        [Condition("X Hysteresis", "Max Stroke", "OIS X Scan", "", "um")] public double XHysMaxStroke { get; set; } = 310;
        [Condition("X Hysteresis", "Mode", "OIS X Scan", "", "0:CodeRange / 1:Step / 2:um")] public int XHysMode { get; set; } = 0;

        [Condition("X Current", "Min Range", "OIS X Scan", "", "code")] public int XCurrMinRange { get; set; } = 200;
        [Condition("X Current", "Max Range", "OIS X Scan", "", "code")] public int XCurrMaxRange { get; set; } = 3900;
        [Condition("X Current", "Min Step", "OIS X Scan", "", "_")] public int XCurrMinStep { get; set; } = 0;
        [Condition("X Current", "Max Step", "OIS X Scan", "", "_")] public int XCurrMaxStep { get; set; } = 0;
        [Condition("X Current", "Min Stroke", "OIS X Scan", "", "um")] public double XCurrMinStroke { get; set; } = -310;
        [Condition("X Current", "Max Stroke", "OIS X Scan", "", "um")] public double XCurrMaxStroke { get; set; } = 310;
        [Condition("X Current", "Mode", "OIS X Scan", "", "0:CodeRange / 1:Step / 2:um")] public int XCurrMode { get; set; } = 0;

        [Condition("Y Linearity", "Min Range", "OIS Y Scan", "", "code")] public int YLinMinRange { get; set; } = 648;
        [Condition("Y Linearity", "Max Range", "OIS Y Scan", "", "code")] public int YLinMaxRange { get; set; } = 3448;
        [Condition("Y Linearity", "Min Step", "OIS Y Scan", "", "_")] public int YLinMinStep { get; set; } = 0;
        [Condition("Y Linearity", "Max Step", "OIS Y Scan", "", "_")] public int YLinMaxStep { get; set; } = 0;
        [Condition("Y Linearity", "Min Stroke", "OIS Y Scan", "", "um")] public double YLinMinStroke { get; set; } = -310;
        [Condition("Y Linearity", "Max Stroke", "OIS Y Scan", "", "um")] public double YLinMaxStroke { get; set; } = 310;
        [Condition("Y Linearity", "Mode", "OIS Y Scan", "", "0:CodeRange / 1:Step / 2:um")] public int YLinMode { get; set; } = 0;

        [Condition("Y Hysteresis", "Min Range", "OIS Y Scan", "", "code")] public int YHysMinRange { get; set; } = 648;
        [Condition("Y Hysteresis", "Max Range", "OIS Y Scan", "", "code")] public int YHysMaxRange { get; set; } = 3448;
        [Condition("Y Hysteresis", "Min Step", "OIS Y Scan", "", "_")] public int YHysMinStep { get; set; } = 0;
        [Condition("Y Hysteresis", "Max Step", "OIS Y Scan", "", "_")] public int YHysMaxStep { get; set; } = 0;
        [Condition("Y Hysteresis", "Min Stroke", "OIS Y Scan", "", "_")] public double YHysMinStroke { get; set; } = -310;
        [Condition("Y Hysteresis", "Max Stroke", "OIS Y Scan", "", "_")] public double YHysMaxStroke { get; set; } = 310;
        [Condition("Y Hysteresis", "Mode", "OIS Y Scan", "", "0:CodeRange / 1:Step / 2:um")] public int YHysMode { get; set; } = 0;

        [Condition("Y Current", "Min Range", "OIS Y Scan", "", "code")] public int YCurrMinRange { get; set; } = 200;
        [Condition("Y Current", "Max Range", "OIS Y Scan", "", "code")] public int YCurrMaxRange { get; set; } = 3900;
        [Condition("Y Current", "Min Step", "OIS Y Scan", "", "_")] public int YCurrMinStep { get; set; } = 0;
        [Condition("Y Current", "Max Step", "OIS Y Scan", "", "_")] public int YCurrMaxStep { get; set; } = 0;
        [Condition("Y Current", "Min Stroke", "OIS Y Scan", "", "um")] public double YCurrMinStroke { get; set; } = -310;
        [Condition("Y Current", "Max Stroke", "OIS Y Scan", "", "um")] public double YCurrMaxStroke { get; set; } = 310;
        [Condition("Y Current", "Mode", "OIS Y Scan", "", "0:CodeRange / 1:Step / 2:um")] public int YCurrMode { get; set; } = 0;

        [Condition("OIS XYZ Temperature", "Pos", "OIS XYZ Temperature", "", "_")] public double TemperPos { get; set; } = 256;
        [Condition("OIS XYZ Temperature", "Time", "OIS XYZ Temperature", "", "_")] public double TemperTime { get; set; } = 2000;
        [Condition("OIS XYZ Temperature", "AF Min Spec", "OIS XYZ Temperature", "", "_")] public double AFTempMinSpec { get; set; } = -20;
        [Condition("OIS XYZ Temperature", "AF Thd Spec", "OIS XYZ Temperature", "", "_")] public double AFTempMaxSpec { get; set; } = 40;
        [Condition("OIS XYZ Temperature", "AF Val Spec", "OIS XYZ Temperature", "", "_")] public double AFTempValSpec { get; set; } = 20;
        [Condition("OIS XYZ Temperature", "OIS Min Spec", "OIS XYZ Temperature", "", "_")] public double OISTempMinSpec { get; set; } = -20;
        [Condition("OIS XYZ Temperature", "OIS Thd Spec", "OIS XYZ Temperature", "", "_")] public double OISTempMaxSpec { get; set; } = 40;
        [Condition("OIS XYZ Temperature", "OIS Val Spec", "OIS XYZ Temperature", "", "_")] public double OISTempValSpec { get; set; } = 20;

        [Condition("X/Y Servo Decenter", "AF Position", "X/Y Servo Decenter", "", "code")] public int ServoDecenterAFPos { get; set; } = 1252;
        [Condition("X/Y Servo Decenter", "Delay", "X/Y Servo Decenter", "", "ms")] public int ServoDecenterDelay { get; set; } = 100;

        [Condition("OIS IC Mount Error", "Neg Thd", "OIS IC Mount Error", "", "_")] public int IMEMinThd { get; set; } = -200;
        [Condition("OIS IC Mount Error", "Pos Thd", "OIS IC Mount Error", "", "_")] public int IMEMaxThd { get; set; } = 200;
        [Condition("OIS IC Mount Error", "OIS X/Y Stroke", "OIS IC Mount Error", "", "_")] public int IMEOISStroke { get; set; } = 800;

        [Condition("OIS X/Y OpenLoop", "Step Num", "OIS X/Y OpenLoop", "", "_")] public int OISOLStepNum { get; set; } = 20;
        [Condition("OIS X/Y OpenLoop", "Move Delay", "OIS X/Y OpenLoop", "", "_")] public int OISOLMoveDelay { get; set; } = 30;
        [Condition("OIS X/Y OpenLoop", "tp1", "OIS X/Y OpenLoop", "", "_")] public int OISOLtp1 { get; set; } = 100;
        [Condition("OIS X/Y OpenLoop", "tp2", "OIS X/Y OpenLoop", "", "_")] public int OISOLtp2 { get; set; } = 400;
        [Condition("OIS X/Y OpenLoop", "Spec", "OIS X/Y OpenLoop", "", "_")] public int OISOLSpec { get; set; } = 20000;

        [Condition("X/Y Drift Test", "Standard Code", "X/Y Drift Test", "", "code")] public int DriftStdCode { get; set; } = 2048;
        [Condition("X/Y Drift Test", "Start Code", "X/Y Drift Test", "", "code")] public int DriftStartCode { get; set; } = 0;
        [Condition("X/Y Drift Test", "End Code", "X/Y Drift Test", "", "code")] public int DriftEndCode { get; set; } = 4095;
        [Condition("X/Y Drift Test", "Step Value", "X/Y Drift Test", "", "code")] public int DriftStepValue { get; set; } = 512;
        [Condition("X/Y Drift Test", "Step Delay", "X/Y Drift Test", "", "ms")] public int DriftStepDelay { get; set; } = 50;
        [Condition("X/Y Drift Test", "Spec", "X/Y Drift Test", "", "code")] public int DriftTestSpec { get; set; } = 650;

        [Condition("OIS Sensitivity Test", "Delay Time", "OIS Sensitivity Test", "", "ms")] public int OISSensDelayTime { get; set; } = 200;
        [Condition("I2C", "I2C Clock", "", "", "KHz")] public int iI2Cclock { get; set; } = 400;

        //[Condition("Others", "Raw Gain", "", "", "30 ~ 512")] public int iRawGain { get; set; } = 35;
        //[Condition("Others", "Gamma", "", "", "0.1 ~ 3.99")] public double iGamma { get; set; } = 0.85;
        //[Condition("Others", "Exposure Time", "", "", "usec")] public int iExposure { get; set; } = 74;
        //[Condition("Others", "Edge Band", "", "", "5,7,9,11")] public int iEdgeBand { get; set; } = 7;
        //[Condition("Others", "LED Current L", "", "", "V")] public double LedCurrentL { get; set; } = 2.7;
        //[Condition("Others", "LED Current R", "", "", "V")] public double LedCurrentR { get; set; } = 2.7;

    }
    public class RetryCount
    {
        public List<Retry> RetryOption = new List<Retry>();

    }
    public class Retry
    {
        public string InspName { get; set; }
        public int Count { get; set; }
    }

    public enum InspType
    {
        Normal, 
        OKNG,
        OnlyMin,
        OnlyMax,
        MintoMax,
    }

  
    public enum SpecItem
    {

        [Spec("AF> HallCalibration", "um", InspType.Normal, "AF HallCalibration")] AF_NonEPAStroke,
        [Spec("XY> HallCalibration", "", InspType.OKNG, "OIS HallCalibration")] XYHallCalibration,
        [Spec("XY> OIS IC Mount Error", "any", InspType.Normal, "OIS IC Mount Error")] OISIMERes,
        [Spec("XY> OIS XYZ Temperature", "any", InspType.Normal, "OIS XYZ Temperature")] TempRes,
        [Spec("XY> OIS XYZ aging", "OK/NG", InspType.Normal, "OIS XYZ Aging")] XYZAging,
        [Spec("XY> LinearCompensation", "any", InspType.Normal, "OIS LinearityCompensation")] XYLinearComp,
        [Spec("XY> X Decenter", "um", InspType.Normal, "X/Y Servo Decenter")] x_ServoDecenter,
        [Spec("XY> Y Decenter", "um", InspType.Normal, "X/Y Servo Decenter")] y_ServoDecenter,
        [Spec("XY> OIS X OpenLoop", "any", InspType.Normal, "OIS X/Y OpenLoop")] OLTestXResult,
        [Spec("XY> OIS Y OpenLoop", "any", InspType.Normal, "OIS X/Y OpenLoop")] OLTestYResult,
        [Spec("XY> OIS AutoTest", "any", InspType.Normal, "Auto Test")] AutoTestRes,

        [Spec("USER> OIS Sensitivity test", "No.", InspType.Normal, "OIS Sensitivity Test")] OISSensitivityTestRes,
        [Spec("USER> AF Aging", "any", InspType.OnlyMax, "AF Aging")] AFScanAging,

        [Spec("AF> Displacement Range", "um", InspType.Normal, "AF Scan")] AF_Ratedstroke,
        [Spec("AF> Displacement Min", "um", InspType.OnlyMax, "AF Scan")] AF_Backwardstroke,
        [Spec("AF> Displacement Max", "um", InspType.OnlyMin, "AF Scan")] AF_Forwardstroke,
        [Spec("AF> Hysteresis", "um", InspType.OnlyMax, "AF Scan")] AF_Hysteresis,
        [Spec("AF> Linearity(R)", "um", InspType.OnlyMax, "AF Scan")] AF_Linearity,
        [Spec("AF> Current", "mA", InspType.MintoMax, "AF Scan")] AF_Current,
        [Spec("AF> Tilt", "min", InspType.Normal, "AF Scan")] AF_Tilt,
        [Spec("AF> Hall Shift Verify", "OK/NG", InspType.OKNG, "X/Y Drift Test")] HallShiftVerify,
        [Spec("AF> Stabilize Time", "ms", InspType.OnlyMax, "AF Settling")] AF_SettillingTime,
        

        [Spec("X> Displacement Range", "um", InspType.Normal, "OIS X Scan")] OISX_Ratedstroke,
        [Spec("X> Displacement Min", "um", InspType.OnlyMax, "OIS X Scan")] OISX_Backwardstroke,
        [Spec("X> Displacement Max", "um", InspType.OnlyMin, "OIS X Scan")] OISX_Forwardstroke,
        [Spec("X> Hysteresis", "um", InspType.OnlyMax, "OIS X Scan")] OISX_Hysteresis,      
        [Spec("X> Linearity(R)", "um", InspType.OnlyMax, "OIS X Scan")] OISX_Linearity,
        [Spec("X> Current", "mA", InspType.MintoMax, "OIS X Scan")] OISX_Current,
        [Spec("X> Hall Decenter(Centering Error)", "um", InspType.Normal, "OIS X Scan")] x_HallDecenter,

        [Spec("Y> Displacement Range", "um", InspType.Normal, "OIS Y Scan")] OISY_Ratedstroke,
        [Spec("Y> Displacement Min", "um", InspType.OnlyMax, "OIS Y Scan")] OISY_Backwardstroke,
        [Spec("Y> Displacement Max", "um", InspType.OnlyMin, "OIS Y Scan")] OISY_Forwardstroke,
        [Spec("Y> Hysteresis", "um", InspType.OnlyMax, "OIS Y Scan")] OISY_Hysteresis,
        [Spec("Y> Linearity(R)", "um", InspType.OnlyMax, "OIS Y Scan")] OISY_Linearity,
        [Spec("Y> Current", "mA", InspType.MintoMax, "OIS Y Scan")] OISY_Current,
        [Spec("Y> Hall Decenter(Centering Error)", "um", InspType.Normal, "OIS Y Scan")] y_HallDecenter,

        [Spec("USER> X Through Peak 25", "dB", InspType.OnlyMax, "through Peak 25")] ThroughPeak_X_Gain,
        [Spec("USER> Y Through Peak 25", "dB", InspType.OnlyMax, "through Peak 25")] ThroughPeak_Y_Gain,

        [Spec("USER> OIS X Phase Margin", "deg", InspType.Normal, "OIS Phase Margin")] FRAX_PhaseMargin,
        [Spec("USER> OIS Y Phase Margin", "deg", InspType.Normal, "OIS Phase Margin")] FRAY1_PhaseMargin,

        [Spec("USER> OIS X Loop Gain", "dB", InspType.Normal, "OIS Loopgain")] FRAX_Gain10Hz,
        [Spec("USER> OIS Y Loop Gain", "dB", InspType.Normal, "OIS Loopgain")] FRAY1_Gain10Hz,

        [Spec("USER> AF Gain Margin", "dB", InspType.Normal, "AF Gain Margin")] FRAAF_GainMargin,
        [Spec("USER> AF Phase Margin", "deg", InspType.Normal, "AF Phase Margin")] FRAAF_PhaseMargin,
        [Spec("USER> AF -4dB Phase Margin", "deg", InspType.Normal, "AF Phase Margin")] FRAAF_4dB_PhaseMargin,

        [Spec("USER> AF PID Verify", "any", InspType.Normal, "AF PID Verify")] AFPIDVerifyRes,
        [Spec("USER> OIS PID Verify", "any", InspType.Normal, "OIS PID Verify")] OISPIDVerifyRes,

        Length,

    };
   
    public class Spec
    {
        public List<SpecArray> specList { get; set; } = new List<SpecArray>();
        public void InitSpecList()
        {
            specList.Clear();
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                SpecItem s = (SpecItem)i;
                specList.Add(new SpecArray());
              
                specList[i].Unit = DataIO.GetEnumArttribute<SpecAttribute>(s)?.Unit;
                specList[i].DisplayName = DataIO.GetEnumArttribute<SpecAttribute>(s)?.DisplayName;
                specList[i].InspectionType = (InspType)DataIO.GetEnumArttribute<SpecAttribute>(s)?.InspType;
                specList[i].Category = DataIO.GetEnumArttribute<SpecAttribute>(s)?.Category;
            }
        }

    }

    public class SpecArray
    {
        public double MinSpec { get; set; } = 0;
        public double MaxSpec { get; set; } = 0;
        public bool OnOff { get; set; } = true;
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }
        public int FailCnt { get; set; }

        public InspType InspectionType { get; set; }
    }

    public class TotalYield
    {
        public int LastSampleNum { get; set; }
        public int TotlaTested { get; set; }
        public int TotlaPassed { get; set; }
        public int TotlaFailed { get; set; }

    }
    public class ResultItems
    {
        public double Val = double.MaxValue;
        public bool bPass = true;
        public string msg = "";
    }
    public class PassFail
    {
        public int FirstFailIndex;
        public string FirstFail;    
        public string TotalTime;
        public List<ResultItems> Results = new List<ResultItems>();
    }

    public class AFPidSet : BaseRecipe
    {
        public AFPidSet()
        {
            Param.Add(new object[] { "11", "2D" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Addr\tData");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}", Param[i][0], Param[i][1]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (Arryindex >= ReadArry.Length) break;
                if (ReadArry[Arryindex] == "\n") break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Addr") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }
                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class XPidSet : BaseRecipe
    {
        public XPidSet()
        {
            Param.Add(new object[] { "10", "1E" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Addr\tData");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}", Param[i][0], Param[i][1]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (Arryindex >= ReadArry.Length) break;
                if (ReadArry[Arryindex] == "\n") break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Addr") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }

                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class YPidSet : BaseRecipe
    {
        public YPidSet()
        {
            Param.Add(new object[] { "10", "14", "14" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Addr\tY1Data\tY2Data");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}\t{1}", Param[i][0], Param[i][1], Param[i][2]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (Arryindex >= ReadArry.Length) break;
                if (ReadArry[Arryindex] == "\n") break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Addr") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }
                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class CodeScript : BaseRecipe
    {
        public CodeScript()
        {
            Param.Add(new object[] { "0", "0", "0", "0" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Index\ttarget_X\ttarget_Y1\ttarget_Y2");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}\t{2}\t{3}", Param[i][0], Param[i][1], Param[i][2], Param[i][3]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (ReadArry.Length <= Arryindex)
                    break;
                if (ReadArry[Arryindex] == "\n")
                    break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Index") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }
                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class CurrentPath
    {

        public string ConditionName { get; set; } = "";
        public string SpecName { get; set; } = "";
        public string AFPidPath { get; set; } = "DefaultAF.txt";
        public string XPidPath { get; set; } = "DefaultX.txt";
        public string YPidPath { get; set; } = "DefaultY.txt";
        public string CodeScriptPath { get; set; } = "DefaultCodeScript.txt";


    }
    public class Model : BaseRecipe
    {
        public string MCNum;
        public string TesterNo;
        
        public string MCType;
        private string lotID;
        public string LotID
        {
            get { return lotID; }
            set
            {
                if (value != lotID)
                { lotID = value; IsLotChanged = true; }
                else IsLotChanged = false;
            }
        }
        public string OperatorName;

        public List<string> List = new List<string>();

        public List<string> MakerList = new List<string>();
      
      
        public List<string> SupplierList = new List<string>();
        public List<string> MCTypeList = new List<string>();


        public bool IsLotChanged = false;
        public event EventHandler Changed = null;

        public Model()
        {
            FilePath = STATIC.appPath.RootDir + "Model.txt";

            MCTypeList.Add("Normal");
            MCTypeList.Add("Master");
            MCTypeList.Add("Slave");
            MCTypeList.Add("Handler");

            Read();
        }
        public override void Read(string filePath = "")
        {
            base.Read();
            if (!File.Exists(FilePath))
            {
                List.Add("0");
                List.Add("0");
                List.Add("Normal");

                AppHelper.SetTextLine(FilePath, List);
                SetParam();
            }
            else
            {
                List = AppHelper.GetTextAll(FilePath);
                SetParam();
            }
        }
        public override void Save(string filePath = "")
        {
            List.Clear();
            List.Add(MCNum);
            List.Add(TesterNo);
            List.Add(MCType);


            AppHelper.SetTextLine(FilePath, List);
        }

        public override void SetParam()
        {
            base.SetParam();
            int index = 0;
            MCNum = List[index++];
            TesterNo = List[index++];
            MCType = List[index++];

        }
        public void LotChanged()
        {
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
    public class VisionFile
    {
        public int RawGain { get; set; } = 40;
        public double Gamma { get; set; } = 0.85;
        public int Exposure { get; set; } = 73;
        public int EdgeBand { get; set; } = 9;
        public double LEDCurrentL { get; set; } = 2.8;
        public double LEDCurrentR { get; set; } = 2.3;
    }
   
    public class Password
    {
        public string PW { get; set; } = "0";
    }

  

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public OptionAttribute(string des)
        {
            DisplayName = des;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ConditionAttribute : Attribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string ToDo1 { get; set; }
        public string ToDo2 { get; set; }
        public string Unit { get; set; }
        public ConditionAttribute(string des, string des2, string des3, string des4, string des5)
        {
            Category = des;
            DisplayName = des2;
            ToDo1 = des3;
            ToDo2 = des4;
            Unit = des5;

        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SpecAttribute : Attribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }
        public InspType InspType { get; set; }
        public SpecAttribute(string des, string des2, InspType type, string des3)
        {
            Category = des3;
            DisplayName = des;
            Unit = des2;
            InspType = type;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class CommonAttribute : Attribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public CommonAttribute(string des, string des2)
        {
            Category = des;
            DisplayName = des2;
        }
    }

}
