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
        public Recipe()
        {
            Current = new CurrentPath();

            Condition = new Condition();
            Condition.Init(Current.ConditionName, "Recipe\\");

            Spec = new Spec();
            Spec.Init(Current.SpecName, "Spec\\");

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

            if (!Directory.Exists(STATIC.RootDir)) Directory.CreateDirectory(STATIC.RootDir);
            if (!Directory.Exists(STATIC.DataDir)) Directory.CreateDirectory(STATIC.DataDir);
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
    public class Condition : BaseRecipe
    {
        public int iDrvAFStep;
        public int iDrvXStep;
        public int iDrvYStep;
        public int iDrvStepIntervalZ;
        public int iDrvStepIntervalX;
        public int iDrvStepIntervalY;

        public int iAFDrvCodeMin;
        public int iAFDrvCodeMax;
        public int iAFCrossOffsetCntl;
        public int iAFCrossOffsetX;
        public int iAFCrossOffsetY;
        public int iAFPlotRange;
        public int iAFCodeRange;
        public int iAFStrokeRange;
        public int iAFStandbyCode;
        public int iAFJumpStepCode;
        public double iAFSettlingCriteria;
        public int iXDrvCodeMin;
        public int iXDrvCodeMax;
        public int iXCrossOffsetCntl;
        public int iXCrossOffset;
        public int iXCrossOffsetCntlAf;
        public int iXCrossOffsetAf;
        public int iXPlotRange;  
        public int iXCodeRange;
        public int iXStrokeRange;

        public int iYDrvCodeMin;
        public int iYDrvCodeMax;
        public int iY2DrvCodeMin;
        public int iY2DrvCodeMax;
        public int iYCrossOffsetCntl;
        public int iYCrossOffset;
        public int iYCrossOffsetCntlAf;
        public int iYCrossOffsetAf;
        public int iYPlotRange;   
        public int iYCodeRange;
        public int iYStrokeRange;

        public int iMCrossOffsetCntlAf;
        public int iMCrossOffsetAf;

        public int HallCrossOffsetCntlAf;
        public int HallCrossOffsetAf;
        public int HallCalCntl;
        public int HallCalMode;

        public int LinSamplingSize;
        public int LinStart;
        public int LinEnd;
        public int LinTargetDelay;

        public int iXEPACutBottom;
        public int iXEPACutTop;
        public int iY1EPACutBottom;
        public int iY1EPACutTop;
        public int iY2EPACutBottom;
        public int iY2EPACutTop;

        public int iLinearityCntl;
        public int iXEPAExBottom;
        public int iXEPAExTop;
        public int iY1EPAExBottom;
        public int iY1EPAExTop;
        public int iY2EPAExBottom;
        public int iY2EPAExTop;

        public int iFRAloop;
        public int iFRAstep;
        public int iFRAdelay;

        public int iAFChirpFrom;
        public int iAFChirpTo;
        public double iAFAmplitude;

        public int iXChirpFrom;
        public int iXChirpTo;
        public double iXAmplitude;
    
        public int iYChirpFrom;
        public int iYChirpTo;
        public double iYAmplitude;

        public int iGainLoop;
        public int iGainStep;
        public int iGainDelay;

        public int iXGainFrom;
        public int iXGainTo;
        public int iYGainFrom;
        public int iYGainTo;

        public double iFRAXgainTH2;
        public double iFRAYgainTH2;
        public double iXAmplitudeGain;
        public double iYAmplitudeGain;
        public double iLoppgainXAmp;
        public double iLoppgainYAmp;

        public int SIN_THD;
        public int SIN_CNT_ERR;
        public int SIN_FREQ;
        public int SIN_AMP;
        public int SIN_CYCL;
        public int SIN_AXIS;

        public int RNG_THD;
        public int RNG_STVT;
        public int RNG_METM;
        public int RNG_WSEC;
        public int RNG_AXIS;

        public int iOLMaxCode;
        public int iOLMidCode;
        public int iOLMinCode;
        public int iOLAgingLoop;
        public int iOLAgingDelay;

        public int iI2Cclock;
        public int iGrabTimeLimit;
        public int iMaxWaitAfterLastTrigger;
        public int iTriggeredGrabImageCount;
        public int iRawGain;
        public double iGamma;
        public int iExposure;
        public int iEdgeBand;
        public double LedCurrentL;
        public double LedCurrentR;

        public ObservableCollection<string> ToDoList = new ObservableCollection<string>();

        public Condition()
        {
            Param.Add(new object[] { "Common", "Drv AF Step", "40", "code", true });
            Param.Add(new object[] { "Common", "Drv X Step", "400", "code", true });
            Param.Add(new object[] { "Common", "Drv Y Step", "400", "code", true });
            Param.Add(new object[] { "Common", "Drv Step Interval AF", "40", "msec", true });
            Param.Add(new object[] { "Common", "Drv Step Interval X", "40", "msec", true });
            Param.Add(new object[] { "Common", "Drv Step Interval Y", "40", "msec", true });

            Param.Add(new object[] { "AF", "Drv Code Min", "8", "code", true });
            Param.Add(new object[] { "AF", "Drv Code Max", "4088", "code", true });
            Param.Add(new object[] { "AF", "Cross Axis Cntl", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "AF", "Cross Axis Offset X", "2048", "code", true });
            Param.Add(new object[] { "AF", "Cross Axis Offset Y", "2048", "code", true });
            Param.Add(new object[] { "AF", "Plot Range", "2048", "code", true });
            Param.Add(new object[] { "AF", "Code Range", "2048", "code", true });
            Param.Add(new object[] { "AF", "Stroke Range", "500", "um", true });
            Param.Add(new object[] { "AF", "Standby Code", "8", "code", true });
            Param.Add(new object[] { "AF", "Jump Step Code", "2048", "code", true });
            Param.Add(new object[] { "AF", "Settling Criteria", "0.05", "%", true });

            Param.Add(new object[] { "X", "Drv Code Min", "8", "code", true });
            Param.Add(new object[] { "X", "Drv Code Max", "4088", "code", true });
            Param.Add(new object[] { "X", "Cross Axis Cntl", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "X", "Cross Axis Offset", "2048", "code", true });
            Param.Add(new object[] { "X", "Cross Axis Cntl Af", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "X", "Cross Axis Offset Af", "2048", "code", true });
            Param.Add(new object[] { "X", "Plot Range", "2048", "code", true });
            Param.Add(new object[] { "X", "Code Range", "2048", "code", true });
            Param.Add(new object[] { "X", "Stroke Range", "500", "um", true });

            Param.Add(new object[] { "Y1", "Drv Code Min", "8", "code", true });
            Param.Add(new object[] { "Y1", "Drv Code Max", "4088", "code", true });
            Param.Add(new object[] { "Y2", "Drv Code Min", "8", "code", true });
            Param.Add(new object[] { "Y2", "Drv Code Max", "4088", "code", true });
            Param.Add(new object[] { "Y", "Cross Axis Cntl", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "Y", "Cross Axis Offset", "2048", "code", true });
            Param.Add(new object[] { "Y", "Cross Axis Cntl Af", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "Y", "Cross Axis Offset Af", "2048", "code", true });
            Param.Add(new object[] { "Y", "Plot Range", "2048", "code", true });
            Param.Add(new object[] { "Y", "Code Range", "2048", "code", true });
            Param.Add(new object[] { "Y", "Stroke Range", "500", "um", true });

            Param.Add(new object[] { "Matrix", "Cross Axis Cntl Af", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "Matrix", "Cross Axis Offset Af", "2048", "code", true });

            Param.Add(new object[] { "Hall Cal", "Cross Axis Cntl Af", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "Hall Cal", "Cross Axis Offset Af", "2048", "code", true });
            Param.Add(new object[] { "Hall Cal", "Hall Cal Cntl", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "Hall Cal", "Hall Cal Mode", "0", "doe", true });

            Param.Add(new object[] { "Linearity Comp", "Move Step", "64", "code", true });
            Param.Add(new object[] { "Linearity Comp", "Start Code", "0", "code", true });
            Param.Add(new object[] { "Linearity Comp", "End Code", "4095", "code", true });
            Param.Add(new object[] { "Linearity Comp", "Move Delay", "20", "ms", true });

            Param.Add(new object[] { "EPA", "X EPA Cut Bottom", "300", "code", true });
            Param.Add(new object[] { "EPA", "X EPA Cut Top", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y1 EPA Cut Bottom", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y1 EPA Cut Top", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y2 EPA Cut Bottom", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y2 EPA Cut Top", "300", "code", true });

            Param.Add(new object[] { "EPA", "Linearity Cntl", "1", "On(1)/Off(0)", true });
            Param.Add(new object[] { "EPA", "X Ex EPA Bottom", "300", "code", true });
            Param.Add(new object[] { "EPA", "X Ex EPA Top", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y1 Ex EPA Bottom", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y1 Ex EPA Top", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y2 Ex EPA Bottom", "300", "code", true });
            Param.Add(new object[] { "EPA", "Y2 Ex EPA Top", "300", "code", true });

            Param.Add(new object[] { "Phase Margin", "Loop", "1", "#", true });
            Param.Add(new object[] { "Phase Margin", "Step", "5", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "Delay", "10", "msec", true });
            Param.Add(new object[] { "Phase Margin", "AF Chirp from", "250", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "AF Chirp to", "100", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "AF Drv Amplitude", "75", "mV", true });
            Param.Add(new object[] { "Phase Margin", "X Chirp from", "250", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "X Chirp to", "100", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "X Drv Amplitude", "75", "mV", true });
            Param.Add(new object[] { "Phase Margin", "Y Chirp from", "250", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "Y Chirp to", "100", "Hz", true });
            Param.Add(new object[] { "Phase Margin", "Y Drv Amplitude", "75", "mV", true });

            Param.Add(new object[] { "Gain Margin", "Loop", "1", "#", true });
            Param.Add(new object[] { "Gain Margin", "Step", "5", "Hz", true });
            Param.Add(new object[] { "Gain Margin", "Delay", "10", "msec", true });
            Param.Add(new object[] { "Gain Margin", "X Chirp from", "400", "Hz", true });
            Param.Add(new object[] { "Gain Margin", "X Chirp to", "100", "Hz", true });
            Param.Add(new object[] { "Gain Margin", "X Drv Amplitude", "60", "mV", true });
            Param.Add(new object[] { "Gain Margin", "Y Chirp from", "400", "Hz", true });
            Param.Add(new object[] { "Gain Margin", "Y Chirp to", "100", "Hz", true });
            Param.Add(new object[] { "Gain Margin", "Y Drv Amplitude", "60", "mV", true });

            Param.Add(new object[] { "LG @ 10Hz", "X Amp", "60", "mV", true });
            Param.Add(new object[] { "LG @ 10Hz", "Y Amp", "60", "mV", true });

            Param.Add(new object[] { "Sine Wave", "SIN THD", "150", "code", true });
            Param.Add(new object[] { "Sine Wave", "SIN CNT ERR", "1", "cnt", true });
            Param.Add(new object[] { "Sine Wave", "SIN FREQ", "5", "Hz", true });
            Param.Add(new object[] { "Sine Wave", "SIN AMP", "58", "mV", true });
            Param.Add(new object[] { "Sine Wave", "SIN CYCL", "18", "#", true });
            Param.Add(new object[] { "Sine Wave", "SIN AXIS", "0", "0:X 1:Y 2:Both", true });

            Param.Add(new object[] { "Ringing", "RNG THD", "20", "code", true });
            Param.Add(new object[] { "Ringing", "RNG STVT", "90", "%", true });
            Param.Add(new object[] { "Ringing", "RNG METM", "100", "msec", true });
            Param.Add(new object[] { "Ringing", "RNG WSEC", "50", "msec", true });
            Param.Add(new object[] { "Ringing", "RNG AXIS", "0", "0:X 1:Y 2:Both", true });

            Param.Add(new object[] { "Aging OpenLoop", "MaxCode", "1023", "Code", true });
            Param.Add(new object[] { "Aging OpenLoop", "MidCode", "512", "Code", true });
            Param.Add(new object[] { "Aging OpenLoop", "MinCode", "0", "Code", true });
            Param.Add(new object[] { "Aging OpenLoop", "Loop Count", "1", "cnt", true });
            Param.Add(new object[] { "Aging OpenLoop", "Delay", "60", "msec", true });

            Param.Add(new object[] { "I2C", "I2C Clock", "400", "KHz", true });
            Param.Add(new object[] { "Others", "Grab Time Limit", "10", "sec", true });
            Param.Add(new object[] { "Others", "Max Wait after Last Trigger", "1000", "msec", true });
            Param.Add(new object[] { "Others", "Triggered Grab Image Count", "10000", "#", true });
            Param.Add(new object[] { "Others", "Raw Gain", "35", "30~512", true });
            Param.Add(new object[] { "Others", "Gamma", "0.85", "0.1~3.99", true });
            Param.Add(new object[] { "Others", "ExposureTime", "74", "usec", true });
            Param.Add(new object[] { "Others", "Edge Band","7","5,7,9,11", true });
            Param.Add(new object[] { "Others", "LEDCurrentL", "2.7", "V", true });
            Param.Add(new object[] { "Others", "LEDCurrentR", "2.7", "V", true });
        }
        public override void Save(string filePath = "")
        {
            if (filePath == null) return;
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);

            string data = "";
            for (int i = 0; i < ToDoList.Count; i++)
            {
                data += string.Format("{0}\t", ToDoList[i]);
            }
            if (data != "") data = data.Remove(data.Length - 1);
            sw.WriteLine(data);

            for (int i = 0; i < Param.Count; i++)
            {
                data = string.Format("{0}\t{1}", Param[i][1], Param[i][2]);
                sw.WriteLine(data);
            }
            sw.Close();

            bChange = false;

            Read();
        }
        public override void Read(string filePath = "")
        {
            if (filePath == null) return;
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');
            ToDoList.Clear();

            string[] actionArry = ReadArry[0].Split('\t');
            for (int i = 0; i < actionArry.Length; i++)
            {
                if (actionArry[i] != "") ToDoList.Add(actionArry[i]);
            }

            int Paramindex = 0;

            for (int i = 1; i < ReadArry.Length; i++)
            {
                string[] arr = ReadArry[i].Split('\t');
                for (int j = Paramindex; j < Param.Count; j++)
                {
                    arr[0] = arr[0].Trim();
                    if (arr[0] == Param[j][1].ToString().Trim())
                    {
                        Param[j][2] = arr[1];
                        Paramindex = j + 1;
                        break;
                    }
                    bChange = true;
                }

            }

            if(Param.Count != ReadArry.Length - 2) bChange = true;
            
            sr.Close();
            SetParam();
        }
        public override void SetParam()
        {
            int index = 0;
            iDrvAFStep = Convert.ToInt32(Param[index++][2]);
            iDrvXStep = Convert.ToInt32(Param[index++][2]);
            iDrvYStep = Convert.ToInt32(Param[index++][2]);
            iDrvStepIntervalZ = Convert.ToInt32(Param[index++][2]);
            iDrvStepIntervalX = Convert.ToInt32(Param[index++][2]);
            iDrvStepIntervalY = Convert.ToInt32(Param[index++][2]);

            iAFDrvCodeMin = Convert.ToInt32(Param[index++][2]);
            iAFDrvCodeMax = Convert.ToInt32(Param[index++][2]);
            iAFCrossOffsetCntl = Convert.ToInt32(Param[index++][2]);
            iAFCrossOffsetX = Convert.ToInt32(Param[index++][2]);
            iAFCrossOffsetY = Convert.ToInt32(Param[index++][2]);
            iAFPlotRange = Convert.ToInt32(Param[index++][2]);
            iAFCodeRange = Convert.ToInt32(Param[index++][2]);
            iAFStrokeRange = Convert.ToInt32(Param[index++][2]);
            iAFStandbyCode = Convert.ToInt32(Param[index++][2]);
            iAFJumpStepCode = Convert.ToInt32(Param[index++][2]);
            iAFSettlingCriteria = Convert.ToDouble(Param[index++][2]);

            iXDrvCodeMin = Convert.ToInt32(Param[index++][2]);
            iXDrvCodeMax = Convert.ToInt32(Param[index++][2]);
            iXCrossOffsetCntl = Convert.ToInt32(Param[index++][2]);
            iXCrossOffset = Convert.ToInt32(Param[index++][2]);
            iXCrossOffsetCntlAf = Convert.ToInt32(Param[index++][2]);
            iXCrossOffsetAf = Convert.ToInt32(Param[index++][2]);
            iXPlotRange = Convert.ToInt32(Param[index++][2]);
            iXCodeRange = Convert.ToInt32(Param[index++][2]);
            iXStrokeRange = Convert.ToInt32(Param[index++][2]);

            iYDrvCodeMin = Convert.ToInt32(Param[index++][2]);
            iYDrvCodeMax = Convert.ToInt32(Param[index++][2]);
            iY2DrvCodeMin = Convert.ToInt32(Param[index++][2]);
            iY2DrvCodeMax = Convert.ToInt32(Param[index++][2]);
            iYCrossOffsetCntl = Convert.ToInt32(Param[index++][2]);
            iYCrossOffset = Convert.ToInt32(Param[index++][2]);
            iYCrossOffsetCntlAf = Convert.ToInt32(Param[index++][2]);
            iYCrossOffsetAf = Convert.ToInt32(Param[index++][2]);
            iYPlotRange = Convert.ToInt32(Param[index++][2]);
            iYCodeRange = Convert.ToInt32(Param[index++][2]);
            iYStrokeRange = Convert.ToInt32(Param[index++][2]);
            iMCrossOffsetCntlAf = Convert.ToInt32(Param[index++][2]);
            iMCrossOffsetAf = Convert.ToInt32(Param[index++][2]);
            HallCrossOffsetCntlAf = Convert.ToInt32(Param[index++][2]);
            HallCrossOffsetAf = Convert.ToInt32(Param[index++][2]);
            HallCalCntl = Convert.ToInt32(Param[index++][2]);
            HallCalMode = Convert.ToInt32(Param[index++][2]);

            LinSamplingSize = Convert.ToInt32(Param[index++][2]);
            LinStart = Convert.ToInt32(Param[index++][2]);
            LinEnd = Convert.ToInt32(Param[index++][2]);
            LinTargetDelay = Convert.ToInt32(Param[index++][2]);

            iXEPACutBottom = Convert.ToInt32(Param[index++][2]);
            iXEPACutTop = Convert.ToInt32(Param[index++][2]);
            iY1EPACutBottom = Convert.ToInt32(Param[index++][2]);
            iY1EPACutTop = Convert.ToInt32(Param[index++][2]);
            iY2EPACutBottom = Convert.ToInt32(Param[index++][2]);
            iY2EPACutTop = Convert.ToInt32(Param[index++][2]);

            iLinearityCntl = Convert.ToInt32(Param[index++][2]);
            iXEPAExBottom = Convert.ToInt32(Param[index++][2]);
            iXEPAExTop = Convert.ToInt32(Param[index++][2]);
            iY1EPAExBottom = Convert.ToInt32(Param[index++][2]);
            iY1EPAExTop = Convert.ToInt32(Param[index++][2]);
            iY2EPAExBottom = Convert.ToInt32(Param[index++][2]);
            iY2EPAExTop = Convert.ToInt32(Param[index++][2]);

            iFRAloop = Convert.ToInt32(Param[index++][2]);
            iFRAstep = Convert.ToInt32(Param[index++][2]);
            iFRAdelay = Convert.ToInt32(Param[index++][2]);

            iAFChirpFrom = Convert.ToInt32(Param[index++][2]);
            iAFChirpTo = Convert.ToInt32(Param[index++][2]);
            iAFAmplitude = Convert.ToDouble(Param[index++][2]);

            iXChirpFrom = Convert.ToInt32(Param[index++][2]);
            iXChirpTo = Convert.ToInt32(Param[index++][2]);
            iXAmplitude = Convert.ToDouble(Param[index++][2]);
            iYChirpFrom = Convert.ToInt32(Param[index++][2]);
            iYChirpTo = Convert.ToInt32(Param[index++][2]);
            iYAmplitude = Convert.ToDouble(Param[index++][2]);

            iGainLoop = Convert.ToInt32(Param[index++][2]);
            iGainStep = Convert.ToInt32(Param[index++][2]);
            iGainDelay = Convert.ToInt32(Param[index++][2]);
            iXGainFrom = Convert.ToInt32(Param[index++][2]);
            iXGainTo = Convert.ToInt32(Param[index++][2]);
            iXAmplitudeGain = Convert.ToDouble(Param[index++][2]);
            iYGainFrom = Convert.ToInt32(Param[index++][2]);
            iYGainTo = Convert.ToInt32(Param[index++][2]);
            iYAmplitudeGain = Convert.ToDouble(Param[index++][2]);

            iLoppgainXAmp = Convert.ToDouble(Param[index++][2]);
            iLoppgainYAmp = Convert.ToDouble(Param[index++][2]);

            SIN_THD = Convert.ToInt32(Param[index++][2]);
            SIN_CNT_ERR = Convert.ToInt32(Param[index++][2]);
            SIN_FREQ = Convert.ToInt32(Param[index++][2]);
            SIN_AMP = Convert.ToInt32(Param[index++][2]);
            SIN_CYCL = Convert.ToInt32(Param[index++][2]);
            SIN_AXIS = Convert.ToInt32(Param[index++][2]);

            RNG_THD = Convert.ToInt32(Param[index++][2]);
            RNG_STVT = Convert.ToInt32(Param[index++][2]);
            RNG_METM = Convert.ToInt32(Param[index++][2]);
            RNG_WSEC = Convert.ToInt32(Param[index++][2]);
            RNG_AXIS = Convert.ToInt32(Param[index++][2]);

            iOLMaxCode = Convert.ToInt32(Param[index++][2]);
            iOLMidCode = Convert.ToInt32(Param[index++][2]);
            iOLMinCode = Convert.ToInt32(Param[index++][2]);
            iOLAgingLoop = Convert.ToInt32(Param[index++][2]);
            iOLAgingDelay = Convert.ToInt32(Param[index++][2]);

            iI2Cclock = Convert.ToInt32(Param[index++][2]);
            iGrabTimeLimit = Convert.ToInt32(Param[index++][2]);
            iMaxWaitAfterLastTrigger = Convert.ToInt32(Param[index++][2]);
            iTriggeredGrabImageCount = Convert.ToInt32(Param[index++][2]);
            iRawGain = Convert.ToInt32(Param[index++][2]);
            iGamma = Convert.ToDouble(Param[index++][2]);
            iExposure = Convert.ToInt32(Param[index++][2]);
            iEdgeBand = Convert.ToInt32(Param[index++][2]);
            LedCurrentL = Convert.ToDouble(Param[index++][2]);
            LedCurrentR = Convert.ToDouble(Param[index++][2]);

            if (bChange) Save();
        }
    }
    public enum SpecItem
    {
        YEILD,
        OISX_Ratedstroke,
        OISX_Forwardstroke,
        OISX_Backwardstroke,
        OISX_Sensitivity,
        OISX_Linearity,
        OISX_Hysteresis,
        OISX_CenteringCurrent,
        OISX_MaxCurrent,
        OISX_CrosstalkY,
        OISX_CrosstalkZ,
        OISX_CrosstalkR,
        OISX_Rolling,

        OISY_Ratedstroke,
        OISY_Forwardstroke,
        OISY_Backwardstroke,
        OISY_Sensitivity,
        OISY_Linearity,
        OISY_Hysteresis,
        OISY_CenteringCurrent,
        OISY_MaxCurrent,
        OISY_CrosstalkX,
        OISY_CrosstalkZ,
        OISY_CrosstalkR,
        OISY_Rolling,

        AF_Ratedstroke,
        AF_Forwardstroke,
        AF_Backwardstroke,
        AF_Sensitivity,
        AF_Linearity,
        AF_Hysteresis,
        AF_HoldingCurrent,
        AF_MaxCurrent,
        AF_CrosstalkX,
        AF_CrosstalkY,
        AF_CrosstalkR,
        AF_Rolling,
        AF_SettillingTime,

        FRAX_PMFreq,
        FRAX_PhaseMargin,
        FRAX_Gain10Hz,
        FRAX_GainMargin,
        SineWaveX_Result,
        SineWaveX_Count,
        RingingX_Result,
        RingingX_Time,

        FRAAF_PMFreq,
        FRAAF_PhaseMargin,
        FRAAF_Gain10Hz,
        FRAAF_GainMargin,
        SineWaveAF_Result,
        SineWaveAF_Count,
        RingingAF_Result,
        RingingAF_Time,

        FRAY1_PMFreq,
        FRAY1_PhaseMargin,
        FRAY1_Gain10Hz,
        FRAY1_GainMargin,
        SineWaveY1_Result,
        SineWaveY1_Count,
        RingingY1_Result,
        RingingY1_Time,

        FRAY2_PMFreq,
        FRAY2_PhaseMargin,
        FRAY2_Gain10Hz,
        FRAY2_GainMargin,
        SineWaveY2_Result,
        SineWaveY2_Count,
        RingingY2_Result,
        RingingY2_Time,

        x_HallDecenter,
        y_HallDecenter,
        x_ServoDecenter,
        y_ServoDecenter,

    };
    public class Spec : BaseRecipe
    {
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
            public List<double> Output = new List<double>();
        }
        public List<PassFail> PassFails = new List<PassFail>();
        public int LastSampleNum;
        public int TotlaTested;
        public int TotlaPassed;
        public int TotlaFailed;
        public Spec()
        {
            Param.Add(new object[] { "", "Yeild", "0", "0", "0", "0", "0", "0", "0", "0", false });
            Param.Add(new object[] { "X", "Rated stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "Forward stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "Backward stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "Sensitivity", "-1", "1", "0", "0", "0", "0", "0", "um / code", true });
            Param.Add(new object[] { "X", "Linearity", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "Hysteresis", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "Centering Current", "0", "120", "0", "0", "0", "0", "0", "mA", true });
            Param.Add(new object[] { "X", "Max Current", "0", "120", "0", "0", "0", "0", "0", "mA", true });
            Param.Add(new object[] { "X", "CrosstalkY", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "CrosstalkZ", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "CrosstalkR", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "X", "Rolling", "-1", "1", "0", "0", "0", "0", "0", "min", true });

            Param.Add(new object[] { "Y", "Rated stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "Forward stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "Backward stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "Sensitivity", "-1", "1", "0", "0", "0", "0", "0", "um / code", true });
            Param.Add(new object[] { "Y", "Linearity", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "Hysteresis", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "Centering Current", "0", "120", "0", "0", "0", "0", "0", "mA", true });
            Param.Add(new object[] { "Y", "Max Current", "0", "120", "0", "0", "0", "0", "0", "mA", true });
            Param.Add(new object[] { "Y", "CrosstalkX", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "CrosstalkZ", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "CrosstalkR", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "Y", "Rolling", "-1", "1", "0", "0", "0", "0", "0", "min", true });

            Param.Add(new object[] { "AF", "Rated stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "Forward stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "Backward stroke", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "Sensitivity", "-1", "1", "0", "0", "0", "0", "0", "um / code", true });
            Param.Add(new object[] { "AF", "Linearity", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "Hysteresis", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "Holding Current", "0", "120", "0", "0", "0", "0", "0", "mA", true });
            Param.Add(new object[] { "AF", "Max Current", "0", "120", "0", "0", "0", "0", "0", "mA", true });
            Param.Add(new object[] { "AF", "CrosstalkX", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "CrosstalkY", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "CrosstalkR", "-1", "1", "0", "0", "0", "0", "0", "um", true });
            Param.Add(new object[] { "AF", "Rolling", "-1", "1", "0", "0", "0", "0", "0", "min", true });
            Param.Add(new object[] { "AF", "Settling Time", "-1", "1", "0", "0", "0", "0", "0", "ms", true });

            Param.Add(new object[] { "FRA AF", "PM Frequency", "-1", "1", "0", "0", "0", "0", "0", "Hz", true });
            Param.Add(new object[] { "FRA AF", "Phase margin", "-1", "1", "0", "0", "0", "0", "0", "deg", true });
            Param.Add(new object[] { "FRA AF", "Gain @ 10Hz", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA AF", "Gain Margin", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA AF", "Sinewave Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA AF", "Sinewave Count", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA AF", "Ringing Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA AF", "Ringing Time", "-1", "1", "0", "0", "0", "0", "0", "#", true });

            Param.Add(new object[] { "FRA X", "PM Frequency", "-1", "1", "0", "0", "0", "0", "0", "Hz", true });
            Param.Add(new object[] { "FRA X", "Phase margin", "-1", "1", "0", "0", "0", "0", "0", "deg", true });
            Param.Add(new object[] { "FRA X", "Gain @ 10Hz", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA X", "Gain Margin", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA X", "Sinewave Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA X", "Sinewave Count", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA X", "Ringing Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA X", "Ringing Time", "-1", "1", "0", "0", "0", "0", "0", "#", true });

            Param.Add(new object[] { "FRA Y1", "PM Frequency", "-1", "1", "0", "0", "0", "0", "0", "Hz", true });
            Param.Add(new object[] { "FRA Y1", "Phase margin", "-1", "1", "0", "0", "0", "0", "0", "deg", true });
            Param.Add(new object[] { "FRA Y1", "Gain @ 10Hz", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA Y1", "Gain Margin", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA Y1", "Sinewave Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA Y1", "Sinewave Count", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA Y1", "Ringing Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA Y1", "Ringing Time", "-1", "1", "0", "0", "0", "0", "0", "#", true });

            Param.Add(new object[] { "FRA Y2", "PM Frequency", "-1", "1", "0", "0", "0", "0", "0", "Hz", true });
            Param.Add(new object[] { "FRA Y2", "Phase margin", "-1", "1", "0", "0", "0", "0", "0", "deg", true });
            Param.Add(new object[] { "FRA Y2", "Gain @ 10Hz", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA Y2", "Gain Margin", "-1", "1", "0", "0", "0", "0", "0", "db", true });
            Param.Add(new object[] { "FRA Y2", "Sinewave Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA Y2", "Sinewave Count", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA Y2", "Ringing Result", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "FRA Y2", "Ringing Time", "-1", "1", "0", "0", "0", "0", "0", "#", true });

            Param.Add(new object[] { "Hall Decenter", "X Decenter", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "Hall Decenter", "Y Decenter", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "Servo Decenter", "X Decenter", "-1", "1", "0", "0", "0", "0", "0", "#", true });
            Param.Add(new object[] { "Servo Decenter", "Y Decenter", "-1", "1", "0", "0", "0", "0", "0", "#", true });

            for (int i = 0; i < 2; i++)
            {
                PassFails.Add(new PassFail());
                for (int j = 0; j < Param.Count; j++) PassFails[i].Results.Add(new ResultItems());
                for (int j = 0; j < 100; j++) PassFails[i].Output.Add(new double());
            }
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);

            for (int i = 0; i < Param.Count; i++)
            {
                string data;
                if (i == 0)
                {
                    data = string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                        Param[i][1],
                        Param[i][2] = LastSampleNum,
                        Param[i][3] = TotlaTested,
                        Param[i][4] = TotlaPassed,
                        Param[i][5] = TotlaFailed);
                }
                else
                {
                    data = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", Param[i][1], Param[i][2], Param[i][3], Param[i][8], Param[i][10]);
                }
                sw.WriteLine(data);
            }
            sw.Close();

            bChange = false;

            Read();
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

            string[] arry = ReadArry[0].Split('\t');

            LastSampleNum = Convert.ToInt32(arry[1]);
            TotlaTested = Convert.ToInt32(arry[2]);
            TotlaPassed = Convert.ToInt32(arry[3]);
            TotlaFailed = Convert.ToInt32(arry[4]);
            Param[0][10] = false;

            int Paramindex = 1;

            for (int i = 1; i < ReadArry.Length; i++)
            {
                string[] arr = ReadArry[i].Split('\t');
                for (int j = Paramindex; j < Param.Count; j++)
                {
                    arr[0] = arr[0].Trim();
                    if (arr[0] == Param[j][1].ToString().Trim())
                    {
                        Param[Paramindex][2] = arr[1];
                        Param[Paramindex][3] = arr[2];
                        Param[Paramindex][8] = arr[3];
                        Param[Paramindex][10] = arr[4];
                        Paramindex = j + 1;
                        break;
                    }
                    bChange = true;
                }
            }

            sr.Close();

            if (bChange) Save();
        }
        public void InitResult(int ch)
        {
            PassFails[ch].TotalFail = ""; 
            PassFails[ch].FirstFail = "";
            PassFails[ch].FirstFailIndex = 0;
            for (int i = 0; i < Param.Count; i++)
            {
                PassFails[ch].Results[i].Val = 0;
                PassFails[ch].Results[i].msg = ""; PassFails[ch].Results[i].bPass = true;
            }
        }
        public void SetResult(int ch, int start, int end)
        {
            for (int i = start; i < end + 1; i++)
            {
                if (!Convert.ToBoolean(Param[i][10])) continue;

                double lmin, lmax;
                lmin = Convert.ToDouble(Param[i][2]);
                lmax = Convert.ToDouble(Param[i][3]);

                if (PassFails[ch].Results[i].Val < lmin || PassFails[ch].Results[i].Val > lmax || double.IsNaN(PassFails[ch].Results[i].Val))
                {
                    PassFails[ch].Results[i].msg = Param[i][0] + "_" + Param[i][1];
                    PassFails[ch].Results[i].bPass = false;
                    PassFails[ch].TotalFail += string.Format("{0}'", i + 1);
                }
                else
                {
                    PassFails[ch].Results[i].msg = ""; PassFails[ch].Results[i].bPass = true;
                }
            }
            for (int i = start; i < end + 1; i++)
            {
                if (!PassFails[ch].Results[i].bPass)
                {
                    if (PassFails[ch].FirstFailIndex == 0)
                    {
                        PassFails[ch].FirstFailIndex = (i + 1);
                        PassFails[ch].FirstFail = PassFails[ch].Results[i].msg;
                    }

                    int failCnt = Convert.ToInt32(Param[i][8]); failCnt++;
                    Param[i][8] = failCnt;
                }
            }
        }
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
    public class CurrentPath : BaseRecipe
    {
        public List<string> List = new List<string>();
        public string ConditionName;
        public string SpecName;
        public string AFPidPath;
        public string XPidPath;
        public string YPidPath;
        public string CodeScriptPath;

        public CurrentPath()
        {
            FilePath = STATIC.RootDir + "CurrentPath.txt";

            List.Add(ConditionName = "Default.rcp");
            List.Add(SpecName = "Default.spc");
            List.Add(AFPidPath = "DefaultAF.txt");
            List.Add(XPidPath = "DefaultX.txt");
            List.Add(YPidPath = "DefaultY.txt");
            List.Add(CodeScriptPath = "DefaultCodeScript.txt");

            Read();
        }
        public override void Read(string Path = "")
        {
            base.Read();

            if (!File.Exists(FilePath))
            {
                STATIC.SetTextLine(FilePath, List);
            }
            else
            {
                List<string> ReadList = STATIC.GetTextAll(FilePath);
                if(List.Count != ReadList.Count)
                {
                    STATIC.SetTextLine(FilePath, List);
                }
                else
                {
                    List = ReadList;
                }
                int index = 0;
                ConditionName = List[index++];
                SpecName = List[index++];
                AFPidPath = List[index++];
                XPidPath = List[index++];
                YPidPath = List[index++];
                CodeScriptPath = List[index++];
            }
        }
        public override void Save(string Path = "")
        {
            List.Clear();
            List.Add(ConditionName);
            List.Add(SpecName);
            List.Add(AFPidPath);
            List.Add(XPidPath);
            List.Add(YPidPath);
            List.Add(CodeScriptPath);
            STATIC.SetTextLine(FilePath, List);
        }
    }
    public class Model : BaseRecipe
    {
        public string Maker;
        public string RevisionNo;
        public string TesterNo;
        public string ProductLine;
        public string Supplier;
        public string MCNumber;
        public string DriverIC;
        public string ModelName;
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
        public List<string> ICList = new List<string>();
        public List<string> ModelList = new List<string>();
        public List<string> SupplierList = new List<string>();


        public bool IsLotChanged = false;
        public event EventHandler Changed = null;

        public Model()
        {
            FilePath = STATIC.RootDir + "Model.txt";

            MakerList.Add("M (SEMCO NPD)");
            MakerList.Add("S (SEMV)");

            SupplierList.Add("Optrontech");
            SupplierList.Add("Crystal Optics");

            ICList.Add("AK7314");
            ICList.Add("AK7323D");

            ModelList.Add("SO1C31");

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
            List.Add(DriverIC);
            List.Add(ModelName);
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
            DriverIC = List[index++];
            ModelName = List[index++];
            LotID = List[index++];
            OperatorName = List[index++];
        }
        public void LotChanged()
        {
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
    public class Option : BaseRecipe
    {
        public bool PasswordOn;
        public bool SaveRawData;
        public bool ScreenCapture;
        public bool FixedCenter;
        public bool SampleCount;
        public bool WriteResultToDriverIC;
        public bool SafeSensor;
        public bool AFDirReverse;
        public bool XDirReverse;
        public bool YDirReverse;

        public Option()
        {
            FilePath = STATIC.RootDir + "OptionState.txt";
            Param.Add(new object[] { "Password", false });
            Param.Add(new object[] { "Save Raw Data", false });
            Param.Add(new object[] { "Save Screen Capture", false });
            Param.Add(new object[] { "Fixed Center for F / B Stroke", false });
            Param.Add(new object[] { "Sample Count Enable", false });
            Param.Add(new object[] { "Write Result To DriverIC", false });
            Param.Add(new object[] { "Safe Sensor Enable", false });
            Param.Add(new object[] { "AF Direction Reversal", false });
            Param.Add(new object[] { "X Direction Reversal", false });
            Param.Add(new object[] { "Y Direction Reversal", false });

            if (!File.Exists(FilePath)) Save();

            Read();
        }
        public override void Read(string filePath = "")
        {
            StreamReader sr = new StreamReader(FilePath);

            string[] readArry = sr.ReadToEnd().Split('\r');

            int index = 0;
            if (readArry.Length > index) PasswordOn = SetParam(readArry[index], index++);
            if (readArry.Length > index) SaveRawData = SetParam(readArry[index], index++);
            if (readArry.Length > index) ScreenCapture = SetParam(readArry[index], index++);
            if (readArry.Length > index) FixedCenter = SetParam(readArry[index], index++);
            if (readArry.Length > index) SampleCount = SetParam(readArry[index], index++);
            if (readArry.Length > index) WriteResultToDriverIC = SetParam(readArry[index], index++);
            if (readArry.Length > index) SafeSensor = SetParam(readArry[index], index++);
            if (readArry.Length > index) AFDirReverse = SetParam(readArry[index], index++);
            if (readArry.Length > index) XDirReverse = SetParam(readArry[index], index++);
            if (readArry.Length > index) YDirReverse = SetParam(readArry[index], index++);

            sr.Close();
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);

            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}", Param[i][0], Param[i][1]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();
        }
        public bool SetParam(string Src, int index)
        {
            string[] arry = Src.Split('\t');
            for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();

            if (arry[0] == Param[index][0].ToString())
            {
                bool ret = Convert.ToBoolean(arry[1]);
                Param[index][1] = ret;
                return ret;
            }
            return false;
        }
    }
}
