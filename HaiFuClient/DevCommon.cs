using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace HaiFuClient
{
    public static class DevCommon
    {
        public static string Version => "1.0.0.0";
        public static string VersionTime => "201804181300";
        public static FrmMain Main { get; set; }
        public static void MsgBox(this Control ctl, string msg)
        {
            MessageBox.Show(ctl, msg);
        }
        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }
        public static string ToHFString(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd hh:mm:ss");
        }
        public static void Init()
        {

        }
    }
}
