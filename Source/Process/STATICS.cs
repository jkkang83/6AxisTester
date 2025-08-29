using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FZ4P
{
    public static class STATIC
    {
        public static FVision fVision = new FVision();
        public static F_Manage fManage = new F_Manage();
        public static F_Start fStart = new F_Start();


        public enum STATE
        {
            Manage,
            Main,
            Vision,
        }
        private static int state = 0;
        public static int State
        {
            get { return state; }
            set { if (state != value) state = value; StateChange?.Invoke(null, EventArgs.Empty); }
        }

        public static event EventHandler StateChange = null;

        public static string BaseDir = "C:\\B7WideTest\\";
        public static string RootDir = BaseDir + "\\DoNotTouch\\";
        public static string DataDir = BaseDir + "\\Data\\";
        public static string UserScriptDir = BaseDir + "\\DriverIC\\FW\\";
        public static void SetTextLine(string path, List<string> list)
        {
            try
            {
                string FilePath = path;
                //if (!File.Exists(FilePath)) return;
                StreamWriter sw = new StreamWriter(FilePath);
                for (int i = 0; i < list.Count; i++)
                { sw.WriteLine(list[i]); }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public static List<string> GetTextAll(string path)
        {
            List<string> result = new List<string>();
            string FilePath = path;
            if (!File.Exists(FilePath)) return null;
            StreamReader sr = new StreamReader(FilePath);
            while (sr.Peek() >= 0)
            {
                result.Add(sr.ReadLine());
            }
            sr.Close();
            return result;
        }
        public static byte[] BinFileRead(string fileName)
        {
            byte[] reselt;
            if (fileName != "")
            {
                if (!File.Exists(fileName))
                {
                    return null;
                }
                BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
                int count = (int)binReader.BaseStream.Length;
                reselt = binReader.ReadBytes(count);
                binReader.Close();
            }
            else
            {
                return null;
            }
            return reselt;
        }
        public static string OpenFile(string InitDir, string ext, bool save = false)
        {
            FileDialog op;
            if (save) op = new SaveFileDialog();
            else op = new OpenFileDialog();

            op.InitialDirectory = InitDir;
            if (ext != "") ext = ext.Remove(0, 1);
            op.Filter = "*." + ext + "|*." + ext;
            if (op.ShowDialog() == DialogResult.OK)
                return op.FileName;
            else return null;
        }
        public static string CreateDateDir()
        {
            DateTime dt = DateTime.Now;
            string dir = string.Format("{0}\\{1}\\{2}\\{3}\\", DataDir, dt.Year, dt.Month, dt.Day);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
        public class DeviceCompactInfo
        {
            public string Name
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public string Manufacturer
            {
                get;
                set;
            }

            public string SystemName
            {
                get;
                set;
            }

            public string DeviceID
            {
                get;
                set;
            }
        }

        public static Recipe Rcp = new Recipe();
        public static Process Process = new Process();
        public static DLN Dln = new DLN();
        public static AK73XX DrvIC = new AK73XX();

    }
}