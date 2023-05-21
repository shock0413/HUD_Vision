using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HOVLib
{
    public class ImageConverter
    {
        public static HMat ConvertGray(HMat mat)
        {
            Mat convertImage = new Mat();
            try
            {
                Cv2.CvtColor(mat.Mat, convertImage, ColorConversion.BgrToGray);
            }
            catch
            {
                try
                {
                    Cv2.CvtColor(mat.Mat, convertImage, ColorConversion.BgraToGray);
                }
                catch
                {

                }
            }

            return ToHMat(convertImage);
        }

        public static HMat ConvertBinary(HMat input, int minBright, int maxBright)
        {
            if (minBright < 0)
            {
                minBright = 0;
            }

            if (maxBright > 255)
            {
                maxBright = 255;
            }

            if (minBright > maxBright)
            {
                minBright = maxBright;
            }

            int width = input.Mat.Width;
            int height = input.Mat.Height;

            Mat output = new Mat(height, width, MatType.CV_8UC1);
            // Cv2.Threshold(input.Mat, output, minBright, maxBright, ThresholdType.Binary);

            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    byte* ptr1 = (byte*)input.Mat.Ptr(i).ToPointer();
                    byte* ptr2 = (byte*)output.Ptr(i).ToPointer();

                    for (int j = 0; j < width; j++)
                    {
                        if (ptr1[j] > minBright)
                        {
                            ptr2[j] = 255;
                        }
                        else
                        {
                            ptr2[j] = 0;
                        }
                    }
                }
            }

            return new HMat(output);
        }

        public static BitmapImage BitmapSourceToImage(BitmapSource source)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        public static BitmapSource MatToBitmapSource(HMat mat)
        {
            return BitmapSourceConverter.ToBitmapSource(mat.Mat);
        }

        internal static HMat ToHMat(Mat mat)
        {
            return new HMat(mat);
        }

        public static HMat ToHMat(BitmapSource source)
        {
            return new HMat(BitmapSourceConverter.ToMat(source));
        }
    }
}
