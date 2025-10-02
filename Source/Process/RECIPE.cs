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


    }
    public class Condition
    {
        [Condition("ToDoList", "", "", "", "")] public List<string> ToDoList { get; set; } = new List<string>();
        [Condition("Common", "Drv AF Step", "", "", "code")] public int iDrvAFStep { get; set; } = 40;
        [Condition("Common", "Drv X Step", "", "", "code")] public int iDrvXStep { get; set; } = 400;
        [Condition("Common", "Drv Y Step", "", "", "code")] public int iDrvYStep { get; set; } = 400;
        [Condition("Common", "Drv Step Interval AF", "", "", "msec")] public int iDrvStepIntervalZ { get; set; } = 40;
        [Condition("Common", "Drv Step interval X", "", "", "msec")] public int iDrvStepIntervalX { get; set; } = 40;
        [Condition("Common", "Drv step Interval Y", "", "", "msec")] public int iDrvStepIntervalY { get; set; } = 40;
     
        [Condition("AF", "Drv Code Min", "", "", "code")] public int iAFDrvCodeMin { get; set; } = 8;
        [Condition("AF", "Drv Code Max", "", "", "code")] public int iAFDrvCodeMax { get; set; } = 4088;
        [Condition("AF", "Cross Axis Offset X", "", "", "code")] public int iAFCrossOffsetX { get; set; } = 2048;
        [Condition("AF", "Cross Axis Offset Y", "", "", "code")] public int iAFCrossOffsetY { get; set; } = 2048;
        [Condition("AF", "Plot Range", "", "", "code")] public int iAFPlotRange { get; set; } = 2048;
        [Condition("AF", "Code Range", "", "", "code")] public int iAFCodeRange { get; set; } = 2048;
        [Condition("AF", "Stroke Range", "", "", "um")] public int iAFStrokeRange { get; set; } = 500;
        [Condition("AF", "Standby Code", "", "", "code")] public int iAFStandbyCode { get; set; } = 8;
        [Condition("AF", "Jump Step Code", "", "", "code")] public int iAFJumpStepCode { get; set; } = 2048;
        [Condition("AF", "Settling Criteria", "", "", "%")] public double iAFSettlingCriteria { get; set; } = 0.05;


        [Condition("X", "Drv Code Min", "", "", "code")] public int iXDrvCodeMin { get; set; } = 8;
        [Condition("X", "Drv Code Max", "", "", "code")] public int iXDrvCodeMax { get; set; } = 4088;
        [Condition("X", "Cross Axis Offset", "", "", "code")] public int iXCrossOffset { get; set; } = 2048;
        [Condition("X", "Cross Axis Offset AF", "", "", "code")] public int iXCrossOffsetAf { get; set; } = 2048;
        [Condition("X", "Plot Range", "", "", "code")] public int iXPlotRange { get; set; } =  2048;
        [Condition("X", "Code Range", "", "", "code")] public int iXCodeRange { get; set; } = 2048;
        [Condition("X", "stroke Range", "", "", "um")] public int iXStrokeRange { get; set; } = 500;

        [Condition("Y1", "Drv Code Min", "", "", "code")] public int iYDrvCodeMin { get; set; } = 8;
        [Condition("Y1", "Drv Code Max", "", "", "code")] public int iYDrvCodeMax { get; set; } = 4088;
        [Condition("Y2", "Drv Code Min", "", "", "code")] public int iY2DrvCodeMin { get; set; } = 8;
        [Condition("Y2", "Drv Code Max", "", "", "code")] public int iY2DrvCodeMax { get; set; } = 4088;

        [Condition("Y", "Cross Axis Offset", "", "", "code")] public int iYCrossOffset { get; set; } = 2048;
        [Condition("Y", "Cross Axis Offset AF", "", "", "code")] public int iYCrossOffsetAf { get; set; } = 2048;
        [Condition("Y", "Plot Range", "", "", "code")] public int iYPlotRange { get; set; } = 2048;
        [Condition("Y", "Code Range", "", "", "code")] public int iYCodeRange { get; set; } = 2048;
        [Condition("Y", "Stroke Range", "", "", "um")] public int iYStrokeRange { get; set; } = 500;

        [Condition("AF OL Aging", "Frequency", "", "", "Hz")] public int AFOpenLoopFreq { get; set; } = 10;
        [Condition("AF OL Aging", "Count", "", "", "-")] public int AFOpenLoopCount { get; set; } = 10;

        [Condition("CL Aging", "AF Min", "", "", "-")] public int CLAgingAFMin { get; set; } = 1000;
        [Condition("CL Aging", "AF Max", "", "", "-")] public int CLAgingAFMax { get; set; } = 3000;
        [Condition("CL Aging", "OIS Min", "", "", "-")] public int CLAgingOISMin { get; set; } = 100;
        [Condition("CL Aging", "OIS Max", "", "", "-")] public int CLAgingOISMax { get; set; } = 4000;
        [Condition("CL Aging", "Frequency", "", "", "-")] public int CLAgingFreq { get; set; } = 10;
        [Condition("CL Aging", "Count", "", "", "-")] public int CLAgingCount { get; set; } = 10;
        [Condition("CL Aging", "Mode", "", "", "0:M-m / 1:Rand")] public int CLAgingMode { get; set; } = 0;


        [Condition("AF Scan Aging", "AF Min", "", "", "-")] public int AFScanAgingMin { get; set; } = 0;
        [Condition("AF Scan Aging", "AF Max", "", "", "-")] public int AFScanAgingMax { get; set; } = 4095;    
        [Condition("AF Scan Aging", "delay", "", "", "-")] public int AFScanAgingDelay { get; set; } = 30;
        [Condition("AF Scan Aging", "Count", "", "", "-")] public int AFSCanAgingCount { get; set; } = 3;
        [Condition("AF Scan Aging", "Step", "", "", "-")] public int AFScanAgingStep { get; set; } = 256;


        [Condition("AF Pre Driving", "delay", "", "", "-")] public int AFPreDrvDelay { get; set; } = 30;
        [Condition("AF Pre Driving", "Count", "", "", "-")] public int AFPReDrvCount { get; set; } = 3;


        [Condition("AF EPA", "Target Stroke", "", "", "code")] public int AFEPATarget { get; set; } = 700;
        [Condition("AF EPA", "POSVT", "", "", "code")] public int AFPOSVT { get; set; } = 256;
        [Condition("AF EPA", "NEGVT", "", "", "code")] public int AFNEGVT { get; set; } = 256;

        [Condition("OIS EPA", "X POSVT", "", "", "code")] public int XPOSVT { get; set; } = 264;
        [Condition("OIS EPA", "X NEGVT", "", "", "code")] public int XNEGVT { get; set; } = 264;
        [Condition("OIS EPA", "Y POSVT", "", "", "code")] public int YPOSVT { get; set; } = 264;
        [Condition("OIS EPA", "Y NEGVT", "", "", "code")] public int YNEGVT { get; set; } = 264;

        [Condition("AF Linearity Comp", "Start", "", "", "code")] public int AfLinCompStart { get; set; } = 8;
        [Condition("AF Linearity Comp", "End", "", "", "code")] public int AfLinCompEnd { get; set; } = 4088;
        [Condition("AF Linearity Comp", "Step", "", "", "code")] public int AFLinCompStep { get; set; } = 120;
        [Condition("AF Linearity Comp", "Move Delay", "", "", "msec")] public int AFLinCompMoveDelay { get; set; } = 50;

        [Condition("X Linearity Comp", "Start", "", "", "code")] public int XLinCompStart { get; set; } = 8;
        [Condition("X Linearity Comp", "End", "", "", "code")] public int XLinCompEnd { get; set; } = 4088;
        [Condition("X Linearity Comp", "Step", "", "", "code")] public int XLinCompStep { get; set; } = 120;
        [Condition("X Linearity Comp", "Move Delay", "", "", "msec")] public int XLinCompMoveDelay { get; set; } = 50;

        [Condition("Y Linearity Comp", "Start", "", "", "code")] public int YLinCompStart { get; set; } = 8;
        [Condition("Y Linearity Comp", "End", "", "", "code")] public int YLinCompEnd { get; set; } = 4088;
        [Condition("Y Linearity Comp", "Step", "", "", "code")] public int YLinCompStep { get; set; } = 120;
        [Condition("Y Linearity Comp", "Move Delay", "", "", "msec")] public int YLinCompMoveDelay { get; set; } = 50;


        [Condition("PM", "Loop", "", "", "#")] public int iFRAloop { get; set; } = 1;
        [Condition("PM", "Step", "", "", "%")] public int iFRAstep { get; set; } = 5;
        [Condition("PM", "AF Chirp from", "", "", "Hz")] public int iAFChirpFrom { get; set; } = 250;
        [Condition("PM", "AF Chirp to", "", "", "Hz")] public int iAFChirpTo { get; set; } = 100;
        [Condition("PM", "AF Drv Amp", "", "", "mV")] public double iAFAmplitude { get; set; } = 75;
        [Condition("PM", "X Chirp from", "", "", "Hz")] public int iXChirpFrom { get; set; } = 250;
        [Condition("PM", "X Chirp to", "", "", "Hz")] public int iXChirpTo { get; set; } = 100;
        [Condition("PM", "X Drv Amp", "", "", "mV")] public int iXAmplitude { get; set; } = 75;
        [Condition("PM", "Y Chirp from", "", "", "Hz")] public int iYChirpFrom { get; set; } = 250;
        [Condition("PM", "Y Chirp to", "", "", "Hz")] public int iYChirpTo { get; set; } = 100;
        [Condition("PM", "Y Drv Amp", "", "", "mV")] public int iYAmplitude { get; set; } = 75;

       
        [Condition("High PM", "Step", "", "", "%")] public int iHighFRAstep { get; set; } = 5;    
        [Condition("High PM", "X Chirp from", "", "", "Hz")] public int iHighXChirpFrom { get; set; } = 250;
        [Condition("High PM", "X Chirp to", "", "", "Hz")] public int iHighXChirpTo { get; set; } = 100;
        [Condition("High PM", "X Drv Amp", "", "", "mV")] public int iHighXAmplitude { get; set; } = 75;
        [Condition("High PM", "Y Chirp from", "", "", "Hz")] public int iHighYChirpFrom { get; set; } = 250;
        [Condition("High PM", "Y Chirp to", "", "", "Hz")] public int iHighYChirpTo { get; set; } = 100;
        [Condition("High PM", "Y Drv Amp", "", "", "mV")] public int iHighYAmplitude { get; set; } = 75;



        [Condition("GM", "Loop", "", "", "#")] public int iGainLoop { get; set; } = 1;
        [Condition("GM", "Step", "", "", "Hz")] public int iGainStep { get; set; } = 5;
        [Condition("GM", "X Chirp from", "", "", "Hz")] public int iXGainFrom { get; set; } = 400;
        [Condition("GM", "X Chirp to", "", "", "Hz")] public int iXGainTo { get; set; } = 100;
        [Condition("GM", "X Drv Amplitude", "", "", "mV")] public double iXAmplitudeGain { get; set; } = 60;
        [Condition("GM", "Y Chirp from", "", "", "Hz")] public int iYGainFrom { get; set; } = 250;
        [Condition("GM", "Y Chirp to", "", "", "Hz")] public int iYGainTo { get; set; } = 100;
        [Condition("GM", "Y Drv Amplitude", "", "", "mV")] public double iYAmplitudeGain { get; set; } = 60;

        [Condition("LG @ 10Hz", "X Amp", "", "", "mV")] public double iLoppgainXAmp { get; set; } = 60;
        [Condition("LG @ 10Hz", "Y Amp", "", "", "mV")] public double iLoppgainYAmp { get; set; } = 60;


        [Condition("Sine Wave", "SIN THD", "", "", "code")] public int SIN_THD { get; set; } = 150;
        [Condition("Sine Wave", "SIN CNT ERR", "", "", "cnt")] public int SIN_CNT_ERR { get; set; } = 1;
        [Condition("Sine Wave", "SIN FREQ", "", "", "Hz")] public int SIN_FREQ { get; set; } = 5;
        [Condition("Sine Wave", "SIN AMP", "", "", "mV")] public int SIN_AMP { get; set; } = 58;
        [Condition("Sine Wave", "SIN CYCL", "", "", "#")] public int SIN_CYCL { get; set; } = 18;
        [Condition("Sine Wave", "SIN AXIS", "", "", "0:X 1:Y 2:Both")] public int SIN_AXIS { get; set; } = 0;

        [Condition("Ringing", "RNG THD", "", "", "code")] public int RNG_THD { get; set; } = 20;
        [Condition("Ringing", "RNG STVT", "", "", "%")] public int RNG_STVT { get; set; } = 90;
        [Condition("Ringing", "RNG METM", "", "", "msec")] public int RNG_METM { get; set; } = 100;
        [Condition("Ringing", "RNG WSEC", "", "", "msec")] public int RNG_WSEC { get; set; } = 50;
        [Condition("Ringing", "RNG AXIS", "", "", "0:X 1:Y 2:Both")] public int RNG_AXIS { get; set; } = 0;

        [Condition("Tilt", "Ref Code", "", "", "code")] public int TiltRefCode { get; set; } = 1000;
        [Condition("Tilt", "Min Range", "", "", "code")] public int TiltMinCode { get; set; } = 200;
        [Condition("Tilt", "Max Range", "", "", "code")] public int TiltMaxCode { get; set; } = 3900;

        [Condition("AF Linearity", "Min Range", "", "", "code")] public int AFLinMinRange { get; set; } = 200;
        [Condition("AF Linearity", "Max Range", "", "", "code")] public int AFLinMaxRange { get; set; } = 3900;
        [Condition("AF Linearity", "Min Step", "", "", "_")] public int AFLinMinStep { get; set; } = 0;
        [Condition("AF Linearity", "Max Step", "", "", "_")] public int AFLinMaxStep { get; set; } = 0;
        [Condition("AF Linearity", "Min Stroke", "", "", "um")] public double AFLinMinStroke { get; set; } = -310;
        [Condition("AF Linearity", "Max Stroke", "", "", "um")] public double AFLinMaxStroke { get; set; } = 310;
        [Condition("AF Linearity", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int AFLinMode { get; set; } = 0;

        [Condition("AF Hysteresis", "Min Range", "", "", "code")] public int AFHysMinRange { get; set; } = 200;
        [Condition("AF Hysteresis", "Max Range", "", "", "code")] public int AFHysMaxRange { get; set; } = 3900;
        [Condition("AF Hysteresis", "Min Step", "", "", "_")] public int AFHysMinStep { get; set; } = 0;
        [Condition("AF Hysteresis", "Max Step", "", "", "_")] public int AFhysMaxStep { get; set; } = 0;
        [Condition("AF Hysteresis", "Min Stroke", "", "", "um")] public double AFHysMinStroke { get; set; } = -310;
        [Condition("AF Hysteresis", "Max Stroke", "", "", "um")] public double AFHysMaxStroke { get; set; } = 310;
        [Condition("AF Hysteresis", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int AFHysMode { get; set; } = 0;

        [Condition("AF Current", "Min Range", "", "", "code")] public int AFCurrMinRange { get; set; } = 200;
        [Condition("AF Current", "Max Range", "", "", "code")] public int AFCurrMaxRange { get; set; } = 3900;
        [Condition("AF Current", "Min Step", "", "", "_")] public int AFCurrMinStep { get; set; } = 0;
        [Condition("AF Current", "Max Step", "", "", "_")] public int AFCurrMaxStep { get; set; } = 0;
        [Condition("AF Current", "Min Stroke", "", "", "um")] public double AFCurrMinStroke { get; set; } = -310;
        [Condition("AF Current", "Max Stroke", "", "", "um")] public double AFCurrMaxStroke { get; set; } = 310;
        [Condition("AF Current", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int AFCurrMode { get; set; } = 0;

        [Condition("X Linearity", "Min Range", "", "", "code")] public int XLinMinRange { get; set; } = 648;
        [Condition("X Linearity", "Max Range", "", "", "code")] public int XLinMaxRange { get; set; } = 3448;
        [Condition("X Linearity", "Min Step", "", "", "_")] public int XLinMinStep { get; set; } = 0;
        [Condition("X Linearity", "Max Step", "", "", "_")] public int XLinMaxStep { get; set; } = 0;
        [Condition("X Linearity", "Min Stroke", "", "", "um")] public double XLinMinStroke { get; set; } = -310;
        [Condition("X Linearity", "Max Stroke", "", "", "um")] public double XLinMaxStroke { get; set; } = 310;
        [Condition("X Linearity", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int XLinMode { get; set; } = 0;

        [Condition("X Hysteresis", "Min Range", "", "", "code")] public int XHysMinRange { get; set; } = 648;
        [Condition("X Hysteresis", "Max Range", "", "", "code")] public int XHysMaxRange { get; set; } = 3448;
        [Condition("X Hysteresis", "Min Step", "", "", "_")] public int XHysMinStep { get; set; } = 0;
        [Condition("X Hysteresis", "Max Step", "", "", "_")] public int XHysMaxStep { get; set; } = 0;
        [Condition("X Hysteresis", "Min Stroke", "", "", "um")] public double XHysMinStroke { get; set; } = -310;
        [Condition("X Hysteresis", "Max Stroke", "", "", "um")] public double XHysMaxStroke { get; set; } = 310;
        [Condition("X Hysteresis", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int XHysMode { get; set; } = 0;

        [Condition("X Current", "Min Range", "", "", "code")] public int XCurrMinRange { get; set; } = 200;
        [Condition("X Current", "Max Range", "", "", "code")] public int XCurrMaxRange { get; set; } = 3900;
        [Condition("X Current", "Min Step", "", "", "_")] public int XCurrMinStep { get; set; } = 0;
        [Condition("X Current", "Max Step", "", "", "_")] public int XCurrMaxStep { get; set; } = 0;
        [Condition("X Current", "Min Stroke", "", "", "um")] public double XCurrMinStroke { get; set; } = -310;
        [Condition("X Current", "Max Stroke", "", "", "um")] public double XCurrMaxStroke { get; set; } = 310;
        [Condition("X Current", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int XCurrMode { get; set; } = 0;

        [Condition("Y Linearity", "Min Range", "", "", "code")] public int YLinMinRange { get; set; } = 648;
        [Condition("Y Linearity", "Max Range", "", "", "code")] public int YLinMaxRange { get; set; } = 3448;
        [Condition("Y Linearity", "Min Step", "", "", "_")] public int YLinMinStep { get; set; } = 0;
        [Condition("Y Linearity", "Max Step", "", "", "_")] public int YLinMaxStep { get; set; } = 0;
        [Condition("Y Linearity", "Min Stroke", "", "", "um")] public double YLinMinStroke { get; set; } = -310;
        [Condition("Y Linearity", "Max Stroke", "", "", "um")] public double YLinMaxStroke { get; set; } = 310;
        [Condition("Y Linearity", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int YLinMode { get; set; } = 0;

        [Condition("Y Hysteresis", "Min Range", "", "", "code")] public int YHysMinRange { get; set; } = 648;
        [Condition("Y Hysteresis", "Max Range", "", "", "code")] public int YHysMaxRange { get; set; } = 3448;
        [Condition("Y Hysteresis", "Min Step", "", "", "_")] public int YHysMinStep { get; set; } = 0;
        [Condition("Y Hysteresis", "Max Step", "", "", "_")] public int YHysMaxStep { get; set; } = 0;
        [Condition("Y Hysteresis", "Min Stroke", "", "", "_")] public double YHysMinStroke { get; set; } = -310;
        [Condition("Y Hysteresis", "Max Stroke", "", "", "_")] public double YHysMaxStroke { get; set; } = 310;
        [Condition("Y Hysteresis", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int YHysMode { get; set; } = 0;

        [Condition("Y Current", "Min Range", "", "", "code")] public int YCurrMinRange { get; set; } = 200;
        [Condition("Y Current", "Max Range", "", "", "code")] public int YCurrMaxRange { get; set; } = 3900;
        [Condition("Y Current", "Min Step", "", "", "_")] public int YCurrMinStep { get; set; } = 0;
        [Condition("Y Current", "Max Step", "", "", "_")] public int YCurrMaxStep { get; set; } = 0;
        [Condition("Y Current", "Min Stroke", "", "", "um")] public double YCurrMinStroke { get; set; } = -310;
        [Condition("Y Current", "Max Stroke", "", "", "um")] public double YCurrMaxStroke { get; set; } = 310;
        [Condition("Y Current", "Mode", "", "", "0:CodeRange / 1:Step / 2:um")] public int YCurrMode { get; set; } = 0;


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

        [Spec("X", "Rated Stroke", "um")] OISX_Ratedstroke,
        [Spec("X", "Forward Stroke", "um")] OISX_Forwardstroke,
        [Spec("X", "Backward Stroke", "um")] OISX_Backwardstroke,
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
        [Spec("X", "Rolling", "min")] OISX_Rolling,

        [Spec("Y", "Rated Stroke", "um")] OISY_Ratedstroke,
        [Spec("Y", "Forward Stroke", "um")] OISY_Forwardstroke,
        [Spec("Y", "Backward Stroke", "um")] OISY_Backwardstroke,
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
        [Spec("Y", "Rolling", "min")] OISY_Rolling,

        [Spec("AF", "Rated Stroke", "um")] AF_Ratedstroke,
        [Spec("AF", "Forward Stroke", "um")] AF_Forwardstroke,
        [Spec("AF", "Backward Stroke", "um")] AF_Backwardstroke,
        [Spec("AF", "Sensitivity", "um / code")] AF_Sensitivity,
        [Spec("AF", "Linearity", "um")] AF_Linearity,
        [Spec("AF", "Hysteresis", "um")] AF_Hysteresis,
        [Spec("AF", "Holding Currnet", "mA")] AF_HoldingCurrent,
        [Spec("AF", "Max Current", "mA")] AF_MaxCurrent,
        [Spec("AF", "Min Current", "mA")] AF_MinCurrent,
        [Spec("AF", "Crosstalk X", "um")] AF_CrosstalkX,
        [Spec("AF", "Crosstalk Y", "um")] AF_CrosstalkY,
        [Spec("AF", "Crosstalk R", "um")] AF_CrosstalkR,
        [Spec("AF", "Rolling", "min")] AF_Rolling,
        [Spec("AF", "Tilt", "min")] AF_Tilt,
        [Spec("AF", "Settling Time", "ms")] AF_SettillingTime,
       

        [Spec("FRA AF", "PM Frequency", "Hz")] FRAAF_PMFreq,
        [Spec("FRA AF", "Phase Margin", "deg")] FRAAF_PhaseMargin,
        [Spec("FRA AF", "Gain @ 10Hz", "db")] FRAAF_Gain10Hz,
        [Spec("FRA AF", "Gain Margin", "db")] FRAAF_GainMargin,
        [Spec("FRA AF", "Sinewave Result", "#")] SineWaveAF_Result,
        [Spec("FRA AF", "Sinewave Count", "#")] SineWaveAF_Count,
        [Spec("FRA AF", "Ringing Result", "#")] RingingAF_Result,
        [Spec("FRA AF", "Ringing Time", "#")] RingingAF_Time,

        [Spec("FRA X", "PM Frequency", "Hz")] FRAX_PMFreq,
        [Spec("FRA X", "Phase Margin", "deg")] FRAX_PhaseMargin,
        [Spec("FRA X", "PM Frequency High", "Hz")] FRAX_PMFreq_High,
        [Spec("FRA X", "Phase Margin High", "deg")] FRAX_PhaseMargin_High,
        [Spec("FRA X", "Gain @ 10Hz", "db")] FRAX_Gain10Hz,
        [Spec("FRA X", "Gain Margin", "db")] FRAX_GainMargin,
        [Spec("FRA X", "Sinewave Result", "#")] SineWaveX_Result,
        [Spec("FRA X", "Sinewave Count", "#")] SineWaveX_Count,
        [Spec("FRA X", "Ringing Result", "#")] RingingX_Result,
        [Spec("FRA X", "Ringing Time", "#")] RingingX_Time,

        [Spec("FRA Y1", "PM Frequency", "Hz")] FRAY1_PMFreq,
        [Spec("FRA Y1", "Phase Margin", "deg")] FRAY1_PhaseMargin,
        [Spec("FRA Y1", "PM Frequency High", "Hz")] FRAY1_PMFreq_High,
        [Spec("FRA Y1", "Phase Margin High", "deg")] FRAY1_PhaseMargin_High,
        [Spec("FRA Y1", "Gain @ 10Hz", "db")] FRAY1_Gain10Hz,
        [Spec("FRA Y1", "Gain Margin", "db")] FRAY1_GainMargin,
        [Spec("FRA Y1", "Sinewave Result", "#")] SineWaveY1_Result,
        [Spec("FRA Y1", "Sinewave Count", "#")] SineWaveY1_Count,
        [Spec("FRA Y1", "Ringing Result", "#")] RingingY1_Result,
        [Spec("FRA Y1", "Ringing Time", "#")] RingingY1_Time,

        [Spec("FRA Y2", "PM Frequency", "Hz")] FRAY2_PMFreq,
        [Spec("FRA Y2", "Phase Margin", "deg")] FRAY2_PhaseMargin,
        [Spec("FRA Y2", "PM Frequency High", "Hz")] FRAY2_PMFreq_High,
        [Spec("FRA Y2", "Phase Margin High", "deg")] FRAY2_PhaseMargin_High,
        [Spec("FRA Y2", "Gain @ 10Hz", "db")] FRAY2_Gain10Hz,
        [Spec("FRA Y2", "Gain Margin", "db")] FRAY2_GainMargin,
        [Spec("FRA Y2", "Sinewave Result", "#")] SineWaveY2_Result,
        [Spec("FRA Y2", "Sinewave Count", "#")] SineWaveY2_Count,
        [Spec("FRA Y2", "Ringing Result", "#")] RingingY2_Result,
        [Spec("FRA Y2", "Ringing Time", "#")] RingingY2_Time,

        [Spec("Hall Decenter", "X Decenter", "um")] x_HallDecenter,
        [Spec("Hall Decenter", "Y Decenter", "um")] y_HallDecenter,

        [Spec("Servo Decenter", "X Decenter", "um")] x_ServoDecenter,
        [Spec("Servo Decenter", "Y Decenter", "um")] y_ServoDecenter,

        [Spec("OIS Shift", "X Shift", "um")] x_Shift,
        [Spec("OIS Shift", "Y Shift", "um")] y_Shift,
        [Spec("OIS Shift", "X Limit", "code")] x_Limit,
        [Spec("OIS Shift", "Y Limit", "code")] y_Limit,

        Length,

    };
    public enum NonSpecItem
    {
        Store_Fail = -999,
        AF_Init,
        OIS_Init,
        AF_EPA,
        AF_LinearityComp,
        X_LinearityComp,
        Y_LinearityComp,
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
        public string Maker;
        public string RevisionNo;
        public string TesterNo;
        public string ProductLine;
        public string Supplier;
        public string MCNumber;       
        public string ModelName;
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
      
        public List<string> ModelList = new List<string>();
        public List<string> SupplierList = new List<string>();
        public List<string> MCTypeList = new List<string>();


        public bool IsLotChanged = false;
        public event EventHandler Changed = null;

        public Model()
        {
            FilePath = STATIC.RootDir + "Model.txt";

            MakerList.Add("M (SEMCO NPD)");
            MakerList.Add("S (SEMV)");

            SupplierList.Add("Optrontech");
            SupplierList.Add("Crystal Optics");

            ModelList.Add("SO1C31");

            MCTypeList.Add("Normal");
            MCTypeList.Add("Master");
            MCTypeList.Add("Slave");

            Read();
        }
        public override void Read(string filePath = "")
        {
            base.Read();
            if (!File.Exists(FilePath))
            {
                List.Add("M (SEMCO NPD)");
                List.Add("0");
                List.Add("0");
                List.Add("0");
                List.Add("Optrontech");
                List.Add("Continuous");
                List.Add("AK73XX");
                List.Add("SO1C31");
                List.Add("Test");
                List.Add("Operator");
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
            List.Add(Maker);
            List.Add(RevisionNo);
            List.Add(TesterNo);
            List.Add(ProductLine);
            List.Add(Supplier);
            List.Add(MCNumber);
            List.Add(ModelName);
            List.Add(MCType);
            List.Add(LotID);
            List.Add(OperatorName);
            STATIC.SetTextLine(FilePath, List);
        }

        public override void SetParam()
        {
            base.SetParam();
            int index = 0;
            Maker = List[index++];
            RevisionNo = List[index++];
            TesterNo = List[index++];
            ProductLine = List[index++];
            Supplier = List[index++];
            MCNumber = List[index++];
            ModelName = List[index++];
            MCType = List[index++];
            LotID = List[index++];
            OperatorName = List[index++];
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
