using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utill
{
    public static class LogManager
    {   
        public static void Write(string str)
        {
            try
            {
                string logFolderPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\System Log\\";

                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }

                DateTime dateTime = DateTime.Now;
                string filePath = logFolderPath + dateTime.ToString("yyyyMMdd") + ".log";

                StreamWriter writer = File.AppendText(filePath);
                writer.WriteLine(dateTime.ToString("HHmmss fff") + "\t" + str);

                Console.WriteLine(dateTime.ToString("yyyy-MM-dd HH:mm:ss fff") + "\t" + str);
                writer.Close();
            }
            catch
            {

            }
        }
    }
}
