using FZ4P.AppCore;
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
    public enum STATE
    {
        Manage,
        Main,
        Vision,
    }

    public static class STATIC
    {
        public static bool Enabled { get; private set; }
        public static void Enable() => Enabled = true;
        public static void Disable() => Enabled = false;

        private static FVision fVision;
        private static F_Manage fManage;
        private static F_Start fStart;
        private static HandlerConnection tcpConn;
        public static FVision FVision
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");
                if (fVision is null)
                    fVision = new FVision();
                return fVision;
            }
        }
        public static F_Manage FManage
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");
                if (fManage is null)
                    fManage = new F_Manage();
                return fManage;
            }
        }
        public static F_Start FStart
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");
                if(fStart is null)
                    fStart = new F_Start();
                return fStart;
            }
        }
        public static HandlerConnection TcpConn
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");

                if (tcpConn is null)
                    tcpConn = new HandlerConnection();
                return tcpConn;
            }
        }

        public static event EventHandler StateChange = null;

        private static Recipe rcp;
        private static Process process;
        private static DLN dln;
        private static AK73XX drvIC;
        public static Recipe Rcp
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");

                if (rcp is null)
                    rcp = new Recipe();
                return rcp;
            }
        }
        public static Process Process
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");

                if (process is null)
                    process = new Process();
                return process;
            }
        }
        public static DLN Dln
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");

                if (dln is null)
                    dln = new DLN();
                return dln;
            }
        }
        public static AK73XX DrvIC
        {
            get
            {
                if (!Enabled)
                    throw new InvalidOperationException("STATIC is disabled");

                if (drvIC is null)
                    drvIC = new AK73XX();
                return drvIC;
            }
        }


        public static int I2CFailcnt = 0;
        public static string SaveLogData = string.Empty;

        private static int state = 0;
        public static int State
        {
            get { return state; }
            set { if (state != value) state = value; StateChange?.Invoke(null, EventArgs.Empty); }
        }

        public static AppPath appPath = new AppPath();
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