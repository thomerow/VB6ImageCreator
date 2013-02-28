using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VB6ImageCreator
{
   static class ImageConverter
   {
      static ImageConverter()
      {
         CountConverted = 0;
      }

      public static int CountConverted { get; private set; }

      internal static void ConvertAsync(
         int trnspThresh, 
         System.Windows.Media.Color colBack, 
         System.Windows.Media.Color colTrnsp, 
         string dirSource, 
         string dirDest
      )
      {
         ThreadPool.QueueUserWorkItem(p => Convert(trnspThresh, colBack, colTrnsp, dirSource, dirDest));
      }

      /// <summary>
      /// Converts all png images in a source directory to 24 bit bmp images 
      /// and saves them to a destination directory.
      /// </summary>
      /// <param name="trnspThresh">Transparency threshold in percent. Pixels 
      /// more transparent than this value are considered fully tranparent.</param>
      /// <param name="colBack">Color to use as a background color when blending 
      /// semi transparent pixels.</param>
      /// <param name="colTrnsp">Color of fully transparent pixels.</param>
      /// <param name="dirSource">Source directory.</param>
      /// <param name="dirDest">Destination directory.</param>
      internal static void Convert(
         int trnspThresh, 
         System.Windows.Media.Color colBack, 
         System.Windows.Media.Color colTrnsp, 
         string dirSource, 
         string dirDest
      )
      {
         CountConverted = 0;

         // Convert WPF colors to System.Drawing.Color
         var sysDrwColBack = FromWindowsMediaColor(colBack);
         var sysDrwColTrnsp = FromWindowsMediaColor(colTrnsp);

         // Remove rightmost slash from destination directory
         if (dirDest.EndsWith("\\")) dirDest = dirDest.Substring(0, dirDest.Length - 1);

         // Get list of all file paths inside the source directory
         var sourceImages = Directory.EnumerateFiles(dirSource, "*.png", SearchOption.AllDirectories).ToList();

         Parallel.ForEach(sourceImages, imgPath =>
         {
            string imgDir = Path.GetDirectoryName(imgPath);
            string subDirDest = imgDir.Substring(dirSource.Length);
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string imgPathDest = dirDest + subDirDest + "\\" + fileName + ".bmp";

            // Create destination directory if it does not exist
            string imgDirDest = Path.GetDirectoryName(imgPathDest);
            if (!Directory.Exists(imgDirDest)) Directory.CreateDirectory(imgDirDest);

            var img = Image.FromFile(imgPath);
            var imgDest = Convert(img, trnspThresh, sysDrwColBack, sysDrwColTrnsp);
            imgDest.Save(imgPathDest, System.Drawing.Imaging.ImageFormat.Bmp);
         });

         CountConverted = sourceImages.Count;
      }

      private static Image Convert(
         Image img,
         int trnspThresh,
         System.Drawing.Color colBack, 
         System.Drawing.Color colTrnsp
      )
      {
         var bmpSrc = new Bitmap(img);
         int nWidth = img.Width;
         int nHeight = img.Height;

         for (int j = 0; j < nHeight; ++j)
         {
            for (int i = 0; i < nWidth; ++i)
            {
               var c = bmpSrc.GetPixel(i, j);
               double alpha = (double) c.A / 0xFF;

               // Calculate new pixel color
               if ((1.0 - alpha) >= ((double) trnspThresh / 100)) c = colTrnsp;
               else
               {
                  c = System.Drawing.Color.FromArgb(
                     0xFF,
                     BlendChannel(alpha, c.R, colBack.R),
                     BlendChannel(alpha, c.G, colBack.G),
                     BlendChannel(alpha, c.B, colBack.B)
                  );
               }

               // Replace pixel color
               bmpSrc.SetPixel(i, j, c);
            }
         }

         return bmpSrc;
      }

      /// <summary>
      /// Blends a foreground color and a background color according to a given
      /// alpha value.
      /// </summary>
      /// <param name="alpha">Alpha value (0.0 - 1.0)</param>
      /// <param name="chnFG">Foreground color channel value (0 - 255)</param>
      /// <param name="chnBG">Background channel value (0 - 255)</param>
      /// <returns>Combined color channel value.</returns>
      private static byte BlendChannel(double alpha, byte chnFG, byte chnBG)
      {
         return (byte) Math.Round((alpha * chnFG) + ((1.0 - alpha) * chnBG));
      }

      private static System.Drawing.Color FromWindowsMediaColor(System.Windows.Media.Color color)
      {
         return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
      }
   }
}
