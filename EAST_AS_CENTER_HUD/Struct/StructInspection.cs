using EAST_AS_CENTER_HUD.Camera;
using HCore;
using HHUDTool;
using HTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAST_AS_CENTER_HUD.Struct
{
    public class StructInspection : StructInspectionPart
    {

        public StructCamera StructCamera { get; set; }

        public StructInspection(StructCarkind structCarkind, string name, int inspectionIndex) : base(structCarkind, name, inspectionIndex)
        {

        }

        public IHTool GetTool()
        {
            string toolType = Carkind.IniFileCarkind.GetString("ToolType", inspectionIndex.ToString(), "");
            IHTool tool = null;
            switch (toolType)
            {
                case "Cutoff":
                    tool = new HCutoffTool(ItemIndex, Carkind);
                    break;
                case "Distortion":
                    tool = new HDistortionTool(ItemIndex, Carkind);
                    break;
                case "Center":
                    tool = new HCenterTool(ItemIndex, Carkind);
                    break;
                case "FullContents":
                    tool = new HFullContentsTool(ItemIndex, Carkind);
                    break;
            }

            return tool;
        }

        public Type GetToolType()
        {
            return GetTool().GetType();
        }

        public string GetToolTypeName()
        {
            return GetTool().GetType().ToString();
        }

    }
}
