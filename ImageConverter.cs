using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace VB6ImageCreator
{
   static class ImageConverter
   {
      static int _trnspThresh;
      static System.Windows.Media.Color _colBack, _colTrnsp;

      static ImageConverter()
      {
         CountConverted = 0;
      }

      public static int CountConverted { get; private set; }

      internal static void ConvertAsync(int trnspThresh, System.Windows.Media.Color colBack, System.Windows.Media.Color colTrnsp, string dirSource, string dirDest)
      {
         ThreadPool.QueueUserWorkItem(p => Convert(trnspThresh, colBack, colTrnsp, dirSource, dirDest));
      }

      internal static void Convert(int trnspThresh, System.Windows.Media.Color colBack, System.Windows.Media.Color colTrnsp, string dirSource, string dirDest)
      {
         _trnspThresh = trnspThresh;
         _colTrnsp = colTrnsp;
         _colBack = colBack;

         CountConverted = 0;

         if (dirDest.EndsWith("\\")) dirDest = dirDest.Substring(0, dirDest.Length - 1);

         // Get list of all file paths inside the source directory
         var sourceImages = Directory.EnumerateFiles(dirSource, "*.png", SearchOption.AllDirectories).ToList();

         foreach (var imgPath in sourceImages)
         {
            string imgDir = Path.GetDirectoryName(imgPath);
            string subDirDest = imgDir.Substring(dirSource.Length);
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string imgPathDest = dirDest + subDirDest + "\\" + fileName + ".bmp";

            // Create destination directory if it does not exist
            string imgDirDest = Path.GetDirectoryName(imgPathDest);
            if (!Directory.Exists(imgDirDest)) Directory.CreateDirectory(imgDirDest);

            var img = Image.FromFile(imgPath);
            var imgDest = Convert(img);
            imgDest.Save(imgPathDest, System.Drawing.Imaging.ImageFormat.Bmp);
         }

         CountConverted = sourceImages.Count;
      }

      private static Image Convert(Image img)
      {
         var bmpSrc = new Bitmap(img);
         int nWidth = img.Width;
         int nHeight = img.Height;

         var colTrnsp = FromWindowsMediaColor(_colTrnsp);
         var colBack = FromWindowsMediaColor(_colBack);

         for (int j = 0; j < nHeight; ++j)
         {
            for (int i = 0; i < nWidth; ++i)
            {
               System.Drawing.Color c = bmpSrc.GetPixel(i, j);
               double alpha = (double) c.A / 0xFF;
               if ((1.0 - alpha) >= ((double) _trnspThresh / 100)) c = colTrnsp;
               else
               {
                  c = System.Drawing.Color.FromArgb(
                     0xFF,
                     BlendChannels(alpha, c.R, colBack.R),
                     BlendChannels(alpha, c.G, colBack.G),
                     BlendChannels(alpha, c.B, colBack.B)
                  );

               }

               bmpSrc.SetPixel(i, j, c);
            }
         }

         return bmpSrc;
      }

      private static byte BlendChannels(double alpha, byte chnFore, byte chnBack)
      {
         return (byte) Math.Round((alpha * chnFore) + ((1.0 - alpha) * chnBack));
      }

      private static System.Drawing.Color FromWindowsMediaColor(System.Windows.Media.Color color)
      {
         return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
      }
   }
}
