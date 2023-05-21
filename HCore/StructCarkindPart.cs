using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utill;

namespace HCore
{
    public class StructCarkindPart
    {

        public IniManager IniFileCarkind { get; private set; }
        public IniManager IniFileConfig { get; private set; }
        public string Name { get { return name; } set{ name = value; } }
        private string name;

        public string DefaultName { get { return defaultName; } }
        public string defaultName = null;

        public bool isNewCarkind = false;

        public StructCarkindPart(string name)
        {
            Init(name);
        }

        private void Init(string name)
        {
            this.name = name;
            if (File.Exists(IniManager.GetCarkindIniPath(name)))
            {
                isNewCarkind = false;
                defaultName = name;
            }
            else
            {
                isNewCarkind = true;
            }
            InitIniFile(name);
        }

        public void SaveData()
        {
            //DefaultName이 Null일 경우 새로 만든 경우 임.
            if (defaultName == null)
            {
                isNewCarkind = true;
                defaultName = name;
            }
            else
            {
                if (defaultName != name)
                {
                    File.Move(IniManager.GetCarkindIniPath(defaultName), IniManager.GetCarkindIniPath(name));
                    Init(name);
                }
            }
        }

        public void LoadData()
        {
            
        }

        public void InitIniFile(string name)
        {
            IniFileConfig = IniManager.GetConfingFile();
            IniFileCarkind = IniManager.GetCarkindIni(name);
        }

        public override string ToString()
        {
            return name;
        }

        public void Delete()
        {
            File.Delete(IniManager.GetCarkindIniPath(defaultName));
        }
    }
}
