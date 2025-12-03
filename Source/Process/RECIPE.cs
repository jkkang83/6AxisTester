using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Forms.VisualStyles;

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
        public Recipe()
        {
            Current = new CurrentPath();
            if (File.Exists(STATIC.CurrentPath))
                Current = DataIO.DeserializeXMLFileToObject<CurrentPath>(STATIC.CurrentPath);

            if (!Directory.Exists(STATIC.RootDir)) Directory.CreateDirectory(STATIC.RootDir);
            if (!Directory.Exists(STATIC.DataDir)) Directory.CreateDirectory(STATIC.DataDir);
            if (!Directory.Exists(STATIC.RecipeDir)) Directory.CreateDirectory(STATIC.RecipeDir);
            if (!Directory.Exists(STATIC.SpecDir)) Directory.CreateDirectory(STATIC.SpecDir);
            if (!Directory.Exists(STATIC.PackageDir)) Directory.CreateDirectory(STATIC.PackageDir);
            string res = string.Empty;
            res = STATIC.PKGRelease(STATIC.PackageDir, "*.rcp", STATIC.RecipeDir);
            if (res != string.Empty) Current.ConditionName = Path.GetFileName(res);
            res = STATIC.PKGRelease(STATIC.PackageDir, "*.spc", STATIC.SpecDir);
            if (res != string.Empty) Current.SpecName = Path.GetFileName(res);
            res = STATIC.PKGRelease(STATIC.PackageDir, "*.txt", STATIC.RootDir);

            Current.SerializeToXMLFile(STATIC.CurrentPath);

            Condition = new Condition();
            if (File.Exists(STATIC.RecipeDir + Current.ConditionName))
                Condition = DataIO.DeserializeXMLFileToObject<Condition>(STATIC.RecipeDir + Current.ConditionName);

            Spec = new Spec();
            Spec.InitSpecList();
            if (File.Exists(STATIC.SpecDir + Current.SpecName))
            {
                Spec compare = new Spec();
                compare = DataIO.DeserializeXMLFileToObject<Spec>(STATIC.SpecDir + Current.SpecName);
                for (int i = 0; i < compare.specList.Count; i++)
                {
                    int index = Spec.specList.FindIndex(x => x.DisplayName == compare.specList[i].DisplayName && x.Category == compare.specList[i].Category);
                    if (index != -1)
                    {
                        Spec.specList[index].MinSpec = compare.specList[i].MinSpec;
                        Spec.specList[index].MaxSpec = compare.specList[i].MaxSpec;
                        Spec.specList[index].OnOff = compare.specList[i].OnOff;
                        Spec.specList[index].FailCnt = compare.specList[i].FailCnt;
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
            if(File.Exists(STATIC.OptionPath))
                Option = DataIO.DeserializeXMLFileToObject<Option>(STATIC.OptionPath);

            yield = new TotalYield();
            if (File.Exists(STATIC.YieldPath))
                yield = DataIO.DeserializeXMLFileToObject<TotalYield>(STATIC.YieldPath);


            PassFails = new List<PassFail>();
            for (int i = 0; i < 2; i++)
            {
                PassFails.Add(new PassFail());
                for (int j = 0; j < (int)SpecItem.Length; j++) PassFails[i].Results.Add(new ResultItems());
            }
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
            if (!Directory.Exists(STATIC.BaseDir)) Directory.CreateDirectory(STATIC.BaseDir);
            InitDir = STATIC.BaseDir + subDir;
            Ext = Path.GetExtension(current);
            if (!Directory.Exists(InitDir)) Directory.CreateDirectory(InitDir);
            FilePath = STATIC.BaseDir + subDir + current;

            CurrentName = current;
            if (!File.Exists(FilePath)) Save();

            Read();
        }
        public virtual void Save(string filePath = "")
        {
        }
        public virtual void Read(string filePath = "")
        {
            if (!Directory.Exists(STATIC.RootDir)) Directory.CreateDirectory(STATIC.RootDir);
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
        [Option("Fail Retry")] public bool FailRetry { get; set; }
        [Option("DryRun Mode")] public bool DryRunMode { get; set; }

    }
    public class Condition
    {
        [Condition("ToDoList", "", "", "", "")] public List<string> ToDoList { get; set; } = new List<string>();
        [Condition("PID", "OIS PID Ver.", "OIS Init", "", "_")] public int OISPIDVer { get; set; } = 11;
        [Condition("PID", "AF PID Ver.", "AF Initial", "", "_")] public int AFPIDVer { get; set; } = 11;
        [Condition("Common", "Drv AF Step", "AF Driving", "", "code")] public int iDrvAFStep { get; set; } = 40;
        [Condition("Common", "Drv X Step", "OIS X Driving", "", "code")] public int iDrvXStep { get; set; } = 400;
        [Condition("Common", "Drv Y Step", "OIS Y Driving", "", "code")] public int iDrvYStep { get; set; } = 400;
        [Condition("Common", "Drv Step Interval AF", "AF Driving", "", "msec")] public int iDrvStepIntervalZ { get; set; } = 40;
        [Condition("Common", "Drv Step interval X", "OIS X Driving", "", "msec")] public int iDrvStepIntervalX { get; set; } = 40;
        [Condition("Common", "Drv step Interval Y", "OIS Y Driving", "", "msec")] public int iDrvStepIntervalY { get; set; } = 40;
     
        [Condition("AF", "Drv Code Min", "AF Driving", "", "code")] public int iAFDrvCodeMin { get; set; } = 8;
        [Condition("AF", "Drv Code Max", "AF Driving", "", "code")] public int iAFDrvCodeMax { get; set; } = 4088;
        [Condition("AF", "Cross Axis Offset X", "AF Driving", "", "code")] public int iAFCrossOffsetX { get; set; } = 2048;
        [Condition("AF", "Cross Axis Offset Y", "AF Driving", "", "code")] public int iAFCrossOffsetY { get; set; } = 2048;
        [Condition("AF", "Plot Range", "AF Driving", "", "code")] public int iAFPlotRange { get; set; } = 2048;
        [Condition("AF", "Code Range", "AF Driving", "", "code")] public int iAFCodeRange { get; set; } = 2048;
        [Condition("AF", "Stroke Range", "AF Driving", "", "um")] public int iAFStrokeRange { get; set; } = 500;
        [Condition("AF Settling", "Standby Code", "AF Settling", "", "code")] public int iAFStandbyCode { get; set; } = 8;
        [Condition("AF Settling", "Jump Step Code", "AF Settling", "", "code")] public int iAFJumpStepCode { get; set; } = 2048;
        [Condition("AF Settling", "Settling Criteria", "AF Settling", "", "%")] public double iAFSettlingCriteria { get; set; } = 0.05;


        [Condition("X", "Drv Code Min", "OIS X Driving", "", "code")] public int iXDrvCodeMin { get; set; } = 8;
        [Condition("X", "Drv Code Max", "OIS X Driving", "", "code")] public int iXDrvCodeMax { get; set; } = 4088;
        [Condition("X", "Cross Axis Offset", "OIS X Driving", "", "code")] public int iXCrossOffset { get; set; } = 2048;
        [Condition("X", "Cross Axis Offset AF", "OIS X Driving", "", "code")] public int iXCrossOffsetAf { get; set; } = 2048;
        [Condition("X", "Plot Range", "OIS X Driving", "", "code")] public int iXPlotRange { get; set; } =  2048;
        [Condition("X", "Code Range", "OIS X Driving", "", "code")] public int iXCodeRange { get; set; } = 2048;
        [Condition("X", "stroke Range", "OIS X Driving", "", "um")] public int iXStrokeRange { get; set; } = 500;

        [Condition("Y1", "Drv Code Min", "OIS Y Driving", "", "code")] public int iYDrvCodeMin { get; set; } = 8;
        [Condition("Y1", "Drv Code Max", "OIS Y Driving", "", "code")] public int iYDrvCodeMax { get; set; } = 4088;
        [Condition("Y2", "Drv Code Min", "OIS Y Driving", "", "code")] public int iY2DrvCodeMin { get; set; } = 8;
        [Condition("Y2", "Drv Code Max", "OIS Y Driving", "", "code")] public int iY2DrvCodeMax { get; set; } = 4088;

        [Condition("Y", "Cross Axis Offset", "OIS Y Driving", "", "code")] public int iYCrossOffset { get; set; } = 2048;
        [Condition("Y", "Cross Axis Offset AF", "OIS Y Driving", "", "code")] public int iYCrossOffsetAf { get; set; } = 2048;
        [Condition("Y", "Plot Range", "OIS Y Driving", "", "code")] public int iYPlotRange { get; set; } = 2048;
        [Condition("Y", "Code Range", "OIS Y Driving", "", "code")] public int iYCodeRange { get; set; } = 2048;
        [Condition("Y", "Stroke Range", "OIS Y Driving", "", "um")] public int iYStrokeRange { get; set; } = 500;

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




        [Condition("AF Tilt", "Ref Code", "AF Driving", "", "code")] public int TiltRefCode { get; set; } = 1000;
        [Condition("AF Tilt", "Min Range", "AF Driving", "", "code")] public int TiltMinCode { get; set; } = 200;
        [Condition("AF Tilt", "Max Range", "AF Driving", "", "code")] public int TiltMaxCode { get; set; } = 3900;

        [Condition("AF Linearity", "Min Range", "AF Driving", "", "code")] public int AFLinMinRange { get; set; } = 200;
        [Condition("AF Linearity", "Max Range", "AF Driving", "", "code")] public int AFLinMaxRange { get; set; } = 3900;
        [Condition("AF Linearity", "Min Step", "AF Driving", "", "_")] public int AFLinMinStep { get; set; } = 0;
        [Condition("AF Linearity", "Max Step", "AF Driving", "", "_")] public int AFLinMaxStep { get; set; } = 0;
        [Condition("AF Linearity", "Min Stroke", "AF Driving", "", "um")] public double AFLinMinStroke { get; set; } = -310;
        [Condition("AF Linearity", "Max Stroke", "AF Driving", "", "um")] public double AFLinMaxStroke { get; set; } = 310;
        [Condition("AF Linearity", "Mode", "AF Driving", "", "0:CodeRange / 1:Step / 2:um")] public int AFLinMode { get; set; } = 0;

        [Condition("AF Hysteresis", "Min Range", "AF Driving", "", "code")] public int AFHysMinRange { get; set; } = 200;
        [Condition("AF Hysteresis", "Max Range", "AF Driving", "", "code")] public int AFHysMaxRange { get; set; } = 3900;
        [Condition("AF Hysteresis", "Min Step", "AF Driving", "", "_")] public int AFHysMinStep { get; set; } = 0;
        [Condition("AF Hysteresis", "Max Step", "AF Driving", "", "_")] public int AFhysMaxStep { get; set; } = 0;
        [Condition("AF Hysteresis", "Min Stroke", "AF Driving", "", "um")] public double AFHysMinStroke { get; set; } = -310;
        [Condition("AF Hysteresis", "Max Stroke", "AF Driving", "", "um")] public double AFHysMaxStroke { get; set; } = 310;
        [Condition("AF Hysteresis", "Mode", "AF Driving", "", "0:CodeRange / 1:Step / 2:um")] public int AFHysMode { get; set; } = 0;

        [Condition("AF Current", "Min Range", "AF Driving", "", "code")] public int AFCurrMinRange { get; set; } = 200;
        [Condition("AF Current", "Max Range", "AF Driving", "", "code")] public int AFCurrMaxRange { get; set; } = 3900;
        [Condition("AF Current", "Min Step", "AF Driving", "", "_")] public int AFCurrMinStep { get; set; } = 0;
        [Condition("AF Current", "Max Step", "AF Driving", "", "_")] public int AFCurrMaxStep { get; set; } = 0;
        [Condition("AF Current", "Min Stroke", "AF Driving", "", "um")] public double AFCurrMinStroke { get; set; } = -310;
        [Condition("AF Current", "Max Stroke", "AF Driving", "", "um")] public double AFCurrMaxStroke { get; set; } = 310;
        [Condition("AF Current", "Mode", "AF Driving", "", "0:CodeRange / 1:Step / 2:um")] public int AFCurrMode { get; set; } = 0;

        [Condition("X Linearity", "Min Range", "OIS X Driving", "", "code")] public int XLinMinRange { get; set; } = 648;
        [Condition("X Linearity", "Max Range", "OIS X Driving", "", "code")] public int XLinMaxRange { get; set; } = 3448;
        [Condition("X Linearity", "Min Step", "OIS X Driving", "", "_")] public int XLinMinStep { get; set; } = 0;
        [Condition("X Linearity", "Max Step", "OIS X Driving", "", "_")] public int XLinMaxStep { get; set; } = 0;
        [Condition("X Linearity", "Min Stroke", "OIS X Driving", "", "um")] public double XLinMinStroke { get; set; } = -310;
        [Condition("X Linearity", "Max Stroke", "OIS X Driving", "", "um")] public double XLinMaxStroke { get; set; } = 310;
        [Condition("X Linearity", "Mode", "OIS X Driving", "", "0:CodeRange / 1:Step / 2:um")] public int XLinMode { get; set; } = 0;

        [Condition("X Hysteresis", "Min Range", "OIS X Driving", "", "code")] public int XHysMinRange { get; set; } = 648;
        [Condition("X Hysteresis", "Max Range", "OIS X Driving", "", "code")] public int XHysMaxRange { get; set; } = 3448;
        [Condition("X Hysteresis", "Min Step", "OIS X Driving", "", "_")] public int XHysMinStep { get; set; } = 0;
        [Condition("X Hysteresis", "Max Step", "OIS X Driving", "", "_")] public int XHysMaxStep { get; set; } = 0;
        [Condition("X Hysteresis", "Min Stroke", "OIS X Driving", "", "um")] public double XHysMinStroke { get; set; } = -310;
        [Condition("X Hysteresis", "Max Stroke", "OIS X Driving", "", "um")] public double XHysMaxStroke { get; set; } = 310;
        [Condition("X Hysteresis", "Mode", "OIS X Driving", "", "0:CodeRange / 1:Step / 2:um")] public int XHysMode { get; set; } = 0;

        [Condition("X Current", "Min Range", "OIS X Driving", "", "code")] public int XCurrMinRange { get; set; } = 200;
        [Condition("X Current", "Max Range", "OIS X Driving", "", "code")] public int XCurrMaxRange { get; set; } = 3900;
        [Condition("X Current", "Min Step", "OIS X Driving", "", "_")] public int XCurrMinStep { get; set; } = 0;
        [Condition("X Current", "Max Step", "OIS X Driving", "", "_")] public int XCurrMaxStep { get; set; } = 0;
        [Condition("X Current", "Min Stroke", "OIS X Driving", "", "um")] public double XCurrMinStroke { get; set; } = -310;
        [Condition("X Current", "Max Stroke", "OIS X Driving", "", "um")] public double XCurrMaxStroke { get; set; } = 310;
        [Condition("X Current", "Mode", "OIS X Driving", "", "0:CodeRange / 1:Step / 2:um")] public int XCurrMode { get; set; } = 0;

        [Condition("Y Linearity", "Min Range", "OIS Y Driving", "", "code")] public int YLinMinRange { get; set; } = 648;
        [Condition("Y Linearity", "Max Range", "OIS Y Driving", "", "code")] public int YLinMaxRange { get; set; } = 3448;
        [Condition("Y Linearity", "Min Step", "OIS Y Driving", "", "_")] public int YLinMinStep { get; set; } = 0;
        [Condition("Y Linearity", "Max Step", "OIS Y Driving", "", "_")] public int YLinMaxStep { get; set; } = 0;
        [Condition("Y Linearity", "Min Stroke", "OIS Y Driving", "", "um")] public double YLinMinStroke { get; set; } = -310;
        [Condition("Y Linearity", "Max Stroke", "OIS Y Driving", "", "um")] public double YLinMaxStroke { get; set; } = 310;
        [Condition("Y Linearity", "Mode", "OIS Y Driving", "", "0:CodeRange / 1:Step / 2:um")] public int YLinMode { get; set; } = 0;

        [Condition("Y Hysteresis", "Min Range", "OIS Y Driving", "", "code")] public int YHysMinRange { get; set; } = 648;
        [Condition("Y Hysteresis", "Max Range", "OIS Y Driving", "", "code")] public int YHysMaxRange { get; set; } = 3448;
        [Condition("Y Hysteresis", "Min Step", "OIS Y Driving", "", "_")] public int YHysMinStep { get; set; } = 0;
        [Condition("Y Hysteresis", "Max Step", "OIS Y Driving", "", "_")] public int YHysMaxStep { get; set; } = 0;
        [Condition("Y Hysteresis", "Min Stroke", "OIS Y Driving", "", "_")] public double YHysMinStroke { get; set; } = -310;
        [Condition("Y Hysteresis", "Max Stroke", "OIS Y Driving", "", "_")] public double YHysMaxStroke { get; set; } = 310;
        [Condition("Y Hysteresis", "Mode", "OIS Y Driving", "", "0:CodeRange / 1:Step / 2:um")] public int YHysMode { get; set; } = 0;

        [Condition("Y Current", "Min Range", "OIS Y Driving", "", "code")] public int YCurrMinRange { get; set; } = 200;
        [Condition("Y Current", "Max Range", "OIS Y Driving", "", "code")] public int YCurrMaxRange { get; set; } = 3900;
        [Condition("Y Current", "Min Step", "OIS Y Driving", "", "_")] public int YCurrMinStep { get; set; } = 0;
        [Condition("Y Current", "Max Step", "OIS Y Driving", "", "_")] public int YCurrMaxStep { get; set; } = 0;
        [Condition("Y Current", "Min Stroke", "OIS Y Driving", "", "um")] public double YCurrMinStroke { get; set; } = -310;
        [Condition("Y Current", "Max Stroke", "OIS Y Driving", "", "um")] public double YCurrMaxStroke { get; set; } = 310;
        [Condition("Y Current", "Mode", "OIS Y Driving", "", "0:CodeRange / 1:Step / 2:um")] public int YCurrMode { get; set; } = 0;

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

        [Condition("Others", "Raw Gain", "", "", "30 ~ 512")] public int iRawGain { get; set; } = 35;
        [Condition("Others", "Gamma", "", "", "0.1 ~ 3.99")] public double iGamma { get; set; } = 0.85;
        [Condition("Others", "Exposure Time", "", "", "usec")] public int iExposure { get; set; } = 74;
        [Condition("Others", "Edge Band", "", "", "5,7,9,11")] public int iEdgeBand { get; set; } = 7;
        [Condition("Others", "LED Current L", "", "", "V")] public double LedCurrentL { get; set; } = 2.7;
        [Condition("Others", "LED Current R", "", "", "V")] public double LedCurrentR { get; set; } = 2.7;

    }
    public enum SpecItem
    {
        //  [Spec("AF", "Hall Calibration Stroke", "um")] OISX_Ratedstroke,


        [Spec("AF HallCalibration", "Stroke", "um")] AF_NonEPAStroke,
        [Spec("XY HallCalibration", "Result", "um")] XYHallCalibration,

        [Spec("X", "Displacement Range", "um")] OISX_Ratedstroke,
        [Spec("X", "Displacement Max", "um")] OISX_Forwardstroke,
        [Spec("X", "Displacement Min", "um")] OISX_Backwardstroke,
        [Spec("X", "Sensitivity", "um / code")] OISX_Sensitivity,
        [Spec("X", "Linearity", "um")] OISX_Linearity,
        [Spec("X", "Hysteresis", "um")] OISX_Hysteresis,
        [Spec("X", "Centering Current", "mA")] OISX_CenteringCurrent,
        [Spec("X", "Max Current", "mA")] OISX_MaxCurrent,
        [Spec("X", "Min Current", "mA")] OISX_MinCurrent,
        [Spec("X", "Crosstalk Y", "um")] OISX_CrosstalkY,
        [Spec("X", "Crosstalk Y dB", "dB")] OISX_CrosstalkY_dB,
        [Spec("X", "Crosstalk Y P2P", "um")] OISX_CrosstalkY_P2P,
        [Spec("X", "Crosstalk Y P2P dB", "dB")] OISX_CrosstalkYP2P_dB,
        //[Spec("X", "Crosstalk Z", "um")] OISX_CrosstalkZ,
        //[Spec("X", "Crosstalk R", "um")] OISX_CrosstalkR,
        [Spec("X", "Rolling", "deg")] OISX_Rolling,

        [Spec("Y", "Displacement Range", "um")] OISY_Ratedstroke,
        [Spec("Y", "Displacement Max", "um")] OISY_Forwardstroke,
        [Spec("Y", "Displacement Min", "um")] OISY_Backwardstroke,
        [Spec("Y", "Sensitivity", "um / code")] OISY_Sensitivity,
        [Spec("Y", "Linearity", "um")] OISY_Linearity,
        [Spec("Y", "Hysteresis", "um")] OISY_Hysteresis,
        [Spec("Y", "Centering Current", "mA")] OISY_CenteringCurrent,
        [Spec("Y", "Max Current", "mA")] OISY_MaxCurrent,
        [Spec("Y", "Min Current", "mA")] OISY_MinCurrent,
        [Spec("Y", "Crosstalk X", "um")] OISY_CrosstalkX,
        [Spec("Y", "Crosstalk X dB", "dB")] OISY_CrosstalkX_dB,
        [Spec("Y", "Crosstalk X P2P", "um")] OISY_CrosstalkX_P2P,
        [Spec("Y", "Crosstalk X P2P dB", "dB")] OISY_CrosstalkXP2P_dB,
        //[Spec("Y", "Crosstalk Z", "um")] OISY_CrosstalkZ,
        //[Spec("Y", "Crosstalk R", "um")] OISY_CrosstalkR,
        [Spec("Y", "Rolling", "deg")] OISY_Rolling,

        [Spec("AF", "Displacement Range", "um")] AF_Ratedstroke,
        [Spec("AF", "Displacement Max", "um")] AF_Forwardstroke,
        [Spec("AF", "Displacement Min", "um")] AF_Backwardstroke,
        [Spec("AF", "Sensitivity", "um / code")] AF_Sensitivity,
        [Spec("AF", "Linearity", "um")] AF_Linearity,
        [Spec("AF", "Hysteresis", "um")] AF_Hysteresis,
        [Spec("AF", "Holding Currnet", "mA")] AF_HoldingCurrent,
        [Spec("AF", "Max Current", "mA")] AF_MaxCurrent,
        [Spec("AF", "Min Current", "mA")] AF_MinCurrent,
        [Spec("AF", "Crosstalk X", "um")] AF_CrosstalkX,
        [Spec("AF", "Crosstalk Y", "um")] AF_CrosstalkY,
        [Spec("AF", "Crosstalk R", "um")] AF_CrosstalkR,
        [Spec("AF", "Rolling", "deg")] AF_Rolling,
        [Spec("AF", "Tilt", "min")] AF_Tilt,
        [Spec("AF", "Stabilize Time", "ms")] AF_SettillingTime,
       

        [Spec("FRA AF", "PM Frequency", "Hz")] FRAAF_PMFreq,
        [Spec("FRA AF", "Phase Margin", "deg")] FRAAF_PhaseMargin,
        [Spec("FRA AF", "-4dB Phase Margin", "deg")] FRAAF_4dB_PhaseMargin,
        [Spec("FRA AF", "Gain @ 10Hz", "db")] FRAAF_Gain10Hz,
        [Spec("FRA AF", "Gain Margin", "db")] FRAAF_GainMargin,
        //[Spec("FRA AF", "Sinewave Result", "#")] SineWaveAF_Result,
        //[Spec("FRA AF", "Sinewave Count", "#")] SineWaveAF_Count,
        //[Spec("FRA AF", "Ringing Result", "#")] RingingAF_Result,
        //[Spec("FRA AF", "Ringing Time", "#")] RingingAF_Time,

        [Spec("FRA X", "PM Frequency", "Hz")] FRAX_PMFreq,
        [Spec("FRA X", "Phase Margin", "deg")] FRAX_PhaseMargin,
        [Spec("FRA X", "PM Frequency High", "Hz")] FRAX_PMFreq_High,
        [Spec("FRA X", "Phase Margin High", "deg")] FRAX_PhaseMargin_High,
        [Spec("FRA X", "Loop Gain", "db")] FRAX_Gain10Hz,
        [Spec("FRA X", "Gain Margin", "db")] FRAX_GainMargin,
        //[Spec("FRA X", "Sinewave Result", "#")] SineWaveX_Result,
        //[Spec("FRA X", "Sinewave Count", "#")] SineWaveX_Count,
        //[Spec("FRA X", "Ringing Result", "#")] RingingX_Result,
        //[Spec("FRA X", "Ringing Time", "#")] RingingX_Time,

        [Spec("FRA Y1", "PM Frequency", "Hz")] FRAY1_PMFreq,
        [Spec("FRA Y1", "Phase Margin", "deg")] FRAY1_PhaseMargin,
        [Spec("FRA Y1", "PM Frequency High", "Hz")] FRAY1_PMFreq_High,
        [Spec("FRA Y1", "Phase Margin High", "deg")] FRAY1_PhaseMargin_High,
        [Spec("FRA Y1", "Loop Gain", "db")] FRAY1_Gain10Hz,
        [Spec("FRA Y1", "Gain Margin", "db")] FRAY1_GainMargin,

        //[Spec("FRA Y1", "Sinewave Result", "#")] SineWaveY1_Result,
        //[Spec("FRA Y1", "Sinewave Count", "#")] SineWaveY1_Count,
        //[Spec("FRA Y1", "Ringing Result", "#")] RingingY1_Result,
        //[Spec("FRA Y1", "Ringing Time", "#")] RingingY1_Time,

        [Spec("Throgh Peak 25", "X Gain", "db")] ThroughPeak_X_Gain,
        [Spec("Throgh Peak 25", "Y Gain", "db")] ThroughPeak_Y_Gain,

        //[Spec("FRA Y2", "PM Frequency", "Hz")] FRAY2_PMFreq,
        //[Spec("FRA Y2", "Phase Margin", "deg")] FRAY2_PhaseMargin,
        //[Spec("FRA Y2", "PM Frequency High", "Hz")] FRAY2_PMFreq_High,
        //[Spec("FRA Y2", "Phase Margin High", "deg")] FRAY2_PhaseMargin_High,
        //[Spec("FRA Y2", "Gain @ 10Hz", "db")] FRAY2_Gain10Hz,
        //[Spec("FRA Y2", "Gain Margin", "db")] FRAY2_GainMargin,
        //[Spec("FRA Y2", "Sinewave Result", "#")] SineWaveY2_Result,
        //[Spec("FRA Y2", "Sinewave Count", "#")] SineWaveY2_Count,
        //[Spec("FRA Y2", "Ringing Result", "#")] RingingY2_Result,
        //[Spec("FRA Y2", "Ringing Time", "#")] RingingY2_Time,

        [Spec("Centering Error", "X Decenter", "um")] x_HallDecenter,
        [Spec("Centering Error", "Y Decenter", "um")] y_HallDecenter,

        [Spec("Servo Decenter", "X Decenter", "um")] x_ServoDecenter,
        [Spec("Servo Decenter", "Y Decenter", "um")] y_ServoDecenter,

        //[Spec("OIS Shift", "X Shift", "um")] x_Shift,
        //[Spec("OIS Shift", "Y Shift", "um")] y_Shift,
        //[Spec("OIS Shift", "X Limit", "code")] x_Limit,
        //[Spec("OIS Shift", "Y Limit", "code")] y_Limit,

     
        [Spec("OIS Open Loop", "X Result", "_")] OLTestXResult,
        [Spec("OIS Open Loop", "Y Result", "_")] OLTestYResult,

        //PassFailItemAdd
        [Spec("OIS Sensitivity Test", "Result", "bool")] OISSensitivityTestRes,
        [Spec("AF PID Verify", "Result", "bool")] AFPIDVerifyRes,
        [Spec("OIS PID Verify", "Result", "bool")] OISPIDVerifyRes,
        [Spec("IME Test", "Result", "bool")] OISIMERes,
       // [Spec("IME Test", "Y Result", "bool")] OISYIMERes,
        [Spec("Temperature Test", "Result", "bool")] TempRes,
      
        [Spec("AutoTest", "Result", "bool")] AutoTestRes,

        Length,

    };
    public enum NonSpecItem
    {
        Store_Fail = -999,
        AF_HallCalibration,
        OIS_HallCalibration,
        AF_EPA,
        AF_LinearityComp,
        X_LinearityComp,
        Y_LinearityComp,
     //   Temperature_Test,
    //    AutoTest,
        NVM_Verify_NG,
       // PID_Verify_NG,
       // IME_Test_NG,
        OIS_Openloop_Test,
        DriftTestNG,
    }
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
                specList[i].Category = DataIO.GetEnumArttribute<SpecAttribute>(s)?.Category;
                specList[i].Unit = DataIO.GetEnumArttribute<SpecAttribute>(s)?.Unit;
                specList[i].DisplayName = DataIO.GetEnumArttribute<SpecAttribute>(s)?.DisplayName;
            }
        }

    }

    public class SpecArray
    {
        public double MinSpec { get; set; } = -1;
        public double MaxSpec { get; set; } = 1;
        public bool OnOff { get; set; } = true;
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }
        public int FailCnt { get; set; }
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
        public double Val = 0;
        public bool bPass = true;
        public string msg = "";
    }
    public class PassFail
    {
        public int FirstFailIndex;
        public string FirstFail;
        public string TotalFail;
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
            FilePath = STATIC.RootDir + "Model.txt";

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
                List.Add("Normal");
               
                STATIC.SetTextLine(FilePath, List);
                SetParam();
            }
            else
            {
                List = STATIC.GetTextAll(FilePath);
                SetParam();
            }
        }
        public override void Save(string filePath = "")
        {
            List.Clear();

            List.Add(TesterNo);
            List.Add(MCType);


            STATIC.SetTextLine(FilePath, List);
        }

        public override void SetParam()
        {
            base.SetParam();
            int index = 0;
       
            TesterNo = List[index++];
            MCType = List[index++];

        }
        public void LotChanged()
        {
            Changed?.Invoke(null, EventArgs.Empty);
        }
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
        public SpecAttribute(string des, string des2, string des3)
        {
            Category = des;
            DisplayName = des2;
            Unit = des3;
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
