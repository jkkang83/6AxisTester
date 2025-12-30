using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.AppCore
{
    public class AppPath
    {
        public string BaseDir { get; }
        public string RecipeDir { get; }
        public string SpecDir { get; }

        public string RootDir { get; }
        public string DataDir { get; }
        public string UserScriptDir { get; }
        public string OptionPath { get; }
        public string YieldPath { get; }
        public string CurrentPath { get; }
        public string PackageDir { get; }
        public string TestTimeDir { get; }
        public string VisionFileDir { get; }
        public string RetryCountDir { get; }
        public string PasswordDir { get; }
        public string FailNumber { get; set; }
        public string ActID { get; set; }
        public DateTime LogDate { get; set; }

        //public string BaseDir = "C:\\6AxisTester\\";
        //public string RecipeDir = BaseDir + "Recipe\\";
        //public string SpecDir = BaseDir + "Spec\\";
        //public string RootDir = BaseDir + "\\DoNotTouch\\";
        //public string DataDir = BaseDir + "\\Data\\";
        //public string UserScriptDir = BaseDir + "\\DriverIC\\FW\\";
        //public string OptionPath = RootDir + "OptionState.txt";
        //public string YieldPath = RootDir + "Yield.txt";
        //public string CurrentPath = RootDir + "CurrPath.txt";
        //public string PackageDir = BaseDir + "Package\\";
        //public string TestTimeDir = RootDir + "TestTime.txt";
        //public string VisionFileDir = RootDir + "VisionFile.txt";
        //public string RetryCountDir = RootDir + "RetryCount.txt";
        //public string PasswordDir = RootDir + "PW.txt";
        //public DateTime LogDate = new DateTime();
        //public string FailNumber = string.Empty;
        //public string ActID = string.Empty;

        public AppPath()
        {
            //BaseDir = AppDomain.CurrentDomain.BaseDirectory;
            BaseDir = "C:\\6AxisTester\\";
            RecipeDir = BaseDir + "Recipe\\";
            SpecDir = BaseDir + "Spec\\";
            RootDir = BaseDir + "\\DoNotTouch\\";
            DataDir = BaseDir + "\\Data\\";
            UserScriptDir = BaseDir + "\\DriverIC\\FW\\";
            OptionPath = RootDir + "OptionState.txt";
            YieldPath = RootDir + "Yield.txt";
            CurrentPath = RootDir + "CurrPath.txt";
            PackageDir = BaseDir + "Package\\";
            TestTimeDir = RootDir + "TestTime.txt";
            VisionFileDir = RootDir + "VisionFile.txt";
            RetryCountDir = RootDir + "RetryCount.txt";
            PasswordDir = RootDir + "PW.txt";
            LogDate = new DateTime();
            FailNumber = string.Empty;
            ActID = string.Empty;
        }
    }
}
