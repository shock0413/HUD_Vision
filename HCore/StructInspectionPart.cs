
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCore
{
    public class StructInspectionPart
    {
        public StructCarkindPart Carkind { get; private set; }
        public string Name { get; set; }
        public int inspectionIndex { get; private set; }
        public int ItemIndex { get; private set; }

        public StructInspectionPart(StructCarkindPart structCarkind, string name, int inspectionIndex)
        {
            this.Carkind = structCarkind;
            this.Name = name;
            this.inspectionIndex = inspectionIndex;
            this.ItemIndex = Carkind.IniFileCarkind.GetInt32("Item", inspectionIndex.ToString(), -1);
        }

        public string GetMasterImagePath()
        {
            string path = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Tool\\" + Carkind.IniFileCarkind.GetString("ToolType", inspectionIndex.ToString(), "") + "\\" + ItemIndex + ".jpeg";

            return path;
        }

        public void SaveCameraParams(int gain, int exposure)
        {
            Carkind.IniFileCarkind.WriteValue(Name, "Camera Gain", gain);
            Carkind.IniFileCarkind.WriteValue(Name, "Camera Exposure", exposure);
        }

        public override string ToString()
        {
            return Name;
        }


    }
}
