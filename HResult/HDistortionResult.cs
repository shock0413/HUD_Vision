using HCore;
using HCore.HDrawPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HCore.HResult;

namespace HResult
{
    public class HDistortionResult : IHResult
    {
        public RESULT Result { get { return result; } set { result = value; } }
        private RESULT result = RESULT.NG;
        private DrawManager drawManager;
        public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; } }
        private List<DrawPoint> drawPoints = new List<DrawPoint>();
        public double? MoveValue { get { return moveValue; } set { moveValue = value; } }
        private double? moveValue = null;

        public DrawManager GetDrawManager()
        {
            return DrawManager;
        }

        public RESULT GetResult()
        {
            return result;
        }

        public string GetResultValue()
        {
            throw new NotImplementedException();
        }
    }
}
