using HOVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace HTool
{
    public class HFocusUtill
    {
        public static float GetBlurValue(BitmapSource bitmapSource)
        {
            HBlurCheck blurCheck = new HBlurCheck();
            return blurCheck.GetBlurValue(bitmapSource);
        }
    }
}
