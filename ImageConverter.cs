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

namespace VB6ImageCreator
{
   static class ImageConverter
   {
      static int _trnspThresh;
      static System.Windows.Media.Color _colBack, _colTrnsp;

      internal static void Convert(int trnspThresh, System.Windows.Media.Color colBack, System.Windows.Media.Color colTrnsp, string dirSource, string dirDest)
      {
         _trnspThresh = trnspThresh;
         _colTrnsp = colTrnsp;
         _colBack = colBack;

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
      }

      private static Image Convert(Image img)
      {
         var bmpSrc = new Bitmap(img);
         int nWidth = img.Width;
         int nHeight = img.Height;

         for (int j = 0; j < nHeight; ++j)
         {
            for (int i = 0; i < nWidth; ++i)
            {
               System.Drawing.Color c = bmpSrc.GetPixel(i, j);

               // ToDo: implement

               // bmpSrc.SetPixel(i, j, System.Drawing.Color.FromArgb(0xFF, 0xFF, 0, 0xFF));
            }
         }

         return bmpSrc;
      }
   }
}
