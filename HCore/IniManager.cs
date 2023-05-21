using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utill;

namespace HCore
{
    public class IniManager : IniFile
    {
        public IniManager(string path) : base(path)
        {
        }

        public static IniManager GetConfingFile()
        {
            return new IniManager(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Config.ini");
        }

        public static IniManager GetCarkindIni(string carkindName)
        {
            return new IniManager(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Carkind\\" + carkindName + ".ini");
        }

        public static string GetCarkindIniPath(string carkindName)
        {
            return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Carkind\\" + carkindName + ".ini";
        }

        public static string GetToolPath(string ToolTypeName)
        {
            return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Tool\\" + ToolTypeName + "\\";
        }
    }
}
