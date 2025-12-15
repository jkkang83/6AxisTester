using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace FZ4P
{
    public static class STATIC
    {
        public static FVision fVision = new FVision();
        public static F_Manage fManage = new F_Manage();
        public static F_Start fStart = new F_Start();
        public static HandlerConnection TcpConn = new HandlerConnection();
        public static int I2CFailcnt = 0;
        public static string SaveLogData = string.Empty;
       

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

        public static string BaseDir = "C:\\6AxisTester\\";
        public static string RecipeDir = BaseDir + "Recipe\\";
        public static string SpecDir = BaseDir + "Spec\\";
        public static string RootDir = BaseDir + "\\DoNotTouch\\";
        public static string DataDir = BaseDir + "\\Data\\";
        public static string UserScriptDir = BaseDir + "\\DriverIC\\FW\\";
        public static string OptionPath = RootDir + "OptionState.txt";
        public static string YieldPath = RootDir + "Yield.txt";
        public static string CurrentPath = RootDir + "CurrPath.txt";
        public static string PackageDir = BaseDir + "Package\\";
        public static string TestTimeDir = RootDir + "TestTime.txt";
        public static DateTime LogDate = new DateTime();
        public static string FailNumber = string.Empty;
        public static string ActID = string.Empty;
        
        public static string PKGRelease(string srcdir, string Ext, string destdir)
        {

            string[] Arr = Directory.GetFiles(srcdir, Ext);
            string destFile = string.Empty;
            for (int i = 0; i < Arr.Length; i++)
            {
                if (Arr[i].Contains("CurrentPath ") || Arr[i].Contains("MCInfo"))
                    continue;
                destFile = destdir + Arr[i].Substring(srcdir.Length);
                if (File.Exists(destFile))
                    File.Delete(destFile);
                File.Move(Arr[i], destFile);
            }
            return destFile;
        }
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
            DateTime dt = STATIC.LogDate;
            string dir = string.Format("{0}\\{1}\\{2}\\{3}\\", DataDir, dt.Year, dt.Month, dt.Day);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
        public static char GetEthernetIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Wi-Fi 제외 조건
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    continue;

                // 비활성화된 NIC 제외
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                // IPv4 검색
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {

                        string s = ip.Address.ToString();

                        return s[s.Length - 1];
                    }
                }
            }
            return '0';
        }



        public static Recipe Rcp = new Recipe();
        public static Process Process = new Process();
        public static DLN Dln = new DLN();
        public static AK73XX DrvIC = new AK73XX();

    }
    public static class DataIO
    {
        public static string SerializeToXML<T>(this T toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                using (var ms = new MemoryStream())
                {
                    using (var xw = XmlWriter.Create(ms, new XmlWriterSettings()
                    {
                        Encoding = new UTF8Encoding(false),
                        Indent = true,
                    }))
                    {
                        xmlSerializer.Serialize(xw, toSerialize, ns);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            { return string.Empty; }

        }
        public static bool SerializeToXMLFile<T>(this T toSerialize, string FileName) where T : class, new()
        {
            try
            {
                string dir = Path.GetDirectoryName(FileName);
                try { Directory.CreateDirectory(dir); }
                catch
                { return false; }
                string backFile = Path.ChangeExtension(FileName, ".bak");
                if (File.Exists(backFile))
                    File.Delete(backFile);
                try { File.WriteAllText(backFile, toSerialize.SerializeToXML<T>()); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return false;
                }
                FileInfo info = new FileInfo(backFile);
                if (info.Length == 0)
                { return false; }

                if (File.Exists(FileName))
                    File.Delete(FileName);
                File.Move(backFile, FileName);
                return true;
            }
            catch { return false; }
        }
        public static object Deserialize<T>(this string toDeserialize) where T : class, new()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StringReader txtReader = new StringReader(toDeserialize))
                {
                    return xmlSerializer.Deserialize(txtReader);
                }
            }
            catch
            { return default(T); }
        }
        public static T DeserializeXMLFileToObject<T>(string FileName) where T : class, new()
        {
            try
            {
                string xml = File.ReadAllText(FileName);
                return xml.Deserialize<T>() as T;
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetEnumArttribute<T>(Enum val) where T : Attribute
        {
            Type enumT = val.GetType();
            string enumName = Enum.GetName(enumT, val);
            if (enumName != null)
            {
                FieldInfo finfo = enumT.GetField(enumName);
                if (finfo != null)
                {
                    T attri = (T)Attribute.GetCustomAttribute(finfo, typeof(T));
                    return attri;
                }
            }

            return null;
        }
        public static T GetCustomAttribute<T>(PropertyDescriptor p) where T : Attribute
        {
            T attri = (T)p.Attributes[typeof(T)];
            return attri;

        }
    }
}