using HCore;
using HTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHUDTool
{
    public class HudBase :ToolBase
    {
        public HudBase(int itemIndex, StructCarkindPart structCarkindPart, string toolTypeName) : base(itemIndex, toolTypeName)
        {
            this.StructCarkindPart = structCarkindPart;
        }

        public StructCarkindPart StructCarkindPart { get; internal set; }

        internal double GetHudWidth()
        {
            return StructCarkindPart.IniFileCarkind.GetDouble("HUD Spec", "Width", 176);
        }

        internal double GetHudHeight()
        {
            return StructCarkindPart.IniFileCarkind.GetDouble("HUD Spec", "Height", 65);
        }

        internal double GetHudMMPerPixel()
        {
            return StructCarkindPart.IniFileCarkind.GetDouble("HUD Spec", "mmPerPiexel", 0.225);
        }

        internal int GetHudDotHorizentalCount()
        {
            return StructCarkindPart.IniFileCarkind.GetInt32("HUD Spec", "Dot Horizental Count", 15);
        }

        internal int GetHudDotVerticalCount()
        {
            return StructCarkindPart.IniFileCarkind.GetInt32("HUD Spec", "Dot Vertical Count", 7);
        }

        internal double GetHudDotHorizentalInterval()
        {
            return StructCarkindPart.IniFileCarkind.GetDouble("HUD Spec", "Dot Horizental Interval", 11.84);
        }

        internal double GetHudDotVerticalInterval()
        {
            return StructCarkindPart.IniFileCarkind.GetDouble("HUD Spec", "Dot Vertical Interval", 8.88);
        }

        internal void SetHudWidth(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "Width", value);
        }

        internal void SetHudHeight(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "Height", value);
        }

        internal void SetHudMMPerPixel(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "mmPerPiexel", value);
        }

        internal void SetHudDotHorizentalCount(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "Dot Horizental Count", value);
        }

        internal void SetHudDotVerticalCount(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "Dot Vertical Count", value);
        }

        internal void SetHudDotHorizentalInterval(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "Dot Horizental Interval", value);
        }

        internal void SetHudDotVerticalInterval(double value)
        {
            StructCarkindPart.IniFileCarkind.WriteValue("HUD Spec", "Dot Vertical Interval", value);
        }
    }
}
