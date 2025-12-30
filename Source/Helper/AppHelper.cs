using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Helper
{
    public static class AppHelper
    {
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
            DateTime dt = STATIC.appPath.LogDate;
            string dir = string.Format("{0}\\{1}\\{2}\\{3}\\", STATIC.appPath.DataDir, dt.Year, dt.Month, dt.Day);
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
    }
}
