using HCore;
using HResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HHUDTool
{
    public interface IHTool
    {
        void LoadParams();
        void LoadParams(IHToolParams toolParams);
        IHResult Run(BitmapSource bitmapImage);
    }
}
