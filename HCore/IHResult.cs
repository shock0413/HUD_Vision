using HCore.HDrawPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCore
{
    public interface IHResult
    {
        HResult.RESULT GetResult();
        DrawManager GetDrawManager();
    }
}
