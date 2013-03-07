using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VB6ImageCreator
{
   static class ImageConverter
   {
      private static readonly double[] AlphaTable;

      static ImageConverter()
      {
         CountConverted = 0;

         // Calculate alpha table
         AlphaTable = new double[0x100];
         for (int i = 0; i < 0x100; ++i)
         {
            AlphaTable[i] = (double) i / 0xFF;
         }
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
            string imgPathDest = string.Format("{0}{1}\\{2}.bmp", dirDest, subDirDest, fileName);

            // Create destination directory if it does not exist
            string imgDirDest = Path.GetDirectoryName(imgPathDest);
            if (!Directory.Exists(imgDirDest)) Directory.CreateDirectory(imgDirDest);

            var img = Image.FromFile(imgPath);
            var bmpDest = Convert(img, trnspThresh, sysDrwColBack, sysDrwColTrnsp);
            bmpDest.Save(imgPathDest, System.Drawing.Imaging.ImageFormat.Bmp);
         });

         CountConverted = sourceImages.Count;
      }

      /// <summary>
      /// Converts one image according to the given conversion parameters.
      /// </summary>
      /// <param name="img">The source image.</param>
      /// <param name="trnspThresh">Transparency threshold in percent.</param>
      /// <param name="colBack">Background color.</param>
      /// <param name="colTrnsp">Transparent color.</param>
      /// <returns>The converted image.</returns>
      private static Bitmap Convert(
         Image img,
         int trnspThresh,
         System.Drawing.Color colBack, 
         System.Drawing.Color colTrnsp
      )
      {
         IntPtr ptr;
         double alpha;
         BitmapData bmpData = null;

         double dblTrnspThresh = (double) trnspThresh / 100;
         var bmp = new Bitmap(img);
         int nWidth = img.Width;
         int nHeight = img.Height;

         try
         {
            bmpData = bmp.LockBits(
               new Rectangle(0, 0, nWidth, nHeight),
               ImageLockMode.ReadWrite,
               PixelFormat.Format32bppArgb
            );

            ptr = bmpData.Scan0;
            byte[] line = new byte[Math.Abs(bmpData.Stride)];

            for (int j = 0; j < nHeight; ++j)
            {
               // Get managed copy of the current line
               Marshal.Copy(ptr, line, 0, line.Length);

               // Manipulate line
               for (int i = 0; i < line.Length; i += 4)
               {
                  // Get alpha value of current pixel.
                  // Add 0.5 to byte alpha value to 
                  // make rounding uneccessary below.
                  alpha = AlphaTable[line[i + 3]] + (0.5 / 0xFF); 

                  // Calculate new pixel color
                  if ((1.0 - alpha) >= dblTrnspThresh)
                  {
                     line[i] = colTrnsp.B;
                     line[i + 1] = colTrnsp.G;
                     line[i + 2] = colTrnsp.R;
                  }
                  else if (alpha < 1.0)
                  {
                     line[i] = (byte) ((alpha * line[i]) + ((1.0 - alpha) * colBack.B));
                     line[i + 1] = (byte) ((alpha * line[i + 1]) + ((1.0 - alpha) * colBack.G));
                     line[i + 2] = (byte) ((alpha * line[i + 2]) + ((1.0 - alpha) * colBack.R));
                  }

                  line[i + 3] = 0xFF;  // Alpha channel: always fully opaque
               }

               // Write line
               Marshal.Copy(line, 0, ptr, line.Length);

               // Go to next line
               ptr = new IntPtr((long) ptr + bmpData.Stride);
            }
         }
         finally
         {
            if (bmpData != null) bmp.UnlockBits(bmpData);
         }

         var bmp24bpp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format24bppRgb);
         return bmp24bpp;
      }

      private static System.Drawing.Color FromWindowsMediaColor(System.Windows.Media.Color color)
      {
         return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
      }
   }
}
