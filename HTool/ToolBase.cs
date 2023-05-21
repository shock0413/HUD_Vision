using HCore;
using HOVLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Utill;

namespace HTool
{
    public class ToolBase
    {
        public string ToolTypeName { get; internal set; }
        public IniFile iniFile;
        public int ItemIndex { get; private set; }

        public string ToolPath { get { return IniManager.GetToolPath(ToolTypeName); } }

        public ToolBase(int itemIndex, string toolTypeName)
        {
            this.ItemIndex = itemIndex;
            this.ToolTypeName = toolTypeName;
            if(!Directory.Exists(ToolPath))
            {
                Directory.CreateDirectory(ToolPath);
            }
            iniFile = new IniFile(ToolPath + ItemIndex + ".ini");
        }

        private string GetMastImagePath()
        {
            return ToolPath + ItemIndex + ".jpeg";
        }

        public HMat ConvertGray(HMat mat)
        {
            return ImageConverter.ConvertGray(mat);
        }

        public HMat ConvertBinary(HMat input, int minBright, int maxBright)
        {
            return ImageConverter.ConvertBinary(input, minBright, maxBright);
        }

        public BitmapImage BitmapSourceToImage(BitmapSource source)
        {
            return ImageConverter.BitmapSourceToImage(source);
        }

        public IHResult Result { get; set; }
    }
}
