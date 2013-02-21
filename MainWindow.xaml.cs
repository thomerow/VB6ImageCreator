using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VB6ImageCreator
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      private void SliderRed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         // Green rectangle
         var lgb = _rectGreen.Fill as LinearGradientBrush;
         if (lgb == null) return;

         var stop0 = lgb.GradientStops[0];
         var stop1 = lgb.GradientStops[1];

         stop0.Color = new Color { A = 0xFF, R = (byte) e.NewValue, G = stop0.Color.G, B = stop0.Color.B };
         stop1.Color = new Color { A = 0xFF, R = (byte) e.NewValue, G = stop1.Color.G, B = stop1.Color.B };

         // Blue rectangle
         lgb = _rectBlue.Fill as LinearGradientBrush;
         if (lgb == null) return;

         stop0 = lgb.GradientStops[0];
         stop1 = lgb.GradientStops[1];

         stop0.Color = new Color { A = 0xFF, R = (byte) e.NewValue, G = stop0.Color.G, B = stop0.Color.B };
         stop1.Color = new Color { A = 0xFF, R = (byte) e.NewValue, G = stop1.Color.G, B = stop1.Color.B };

         _txtRed.Text = ((byte) e.NewValue).ToString();

         UpdateSelectedColorRect();
      }

      private void SliderGreen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         // Red rectangle
         var lgb = _rectRed.Fill as LinearGradientBrush;
         if (lgb == null) return;

         var stop0 = lgb.GradientStops[0];
         var stop1 = lgb.GradientStops[1];

         stop0.Color = new Color { A = 0xFF, R = stop0.Color.R, G = (byte) e.NewValue, B = stop0.Color.B };
         stop1.Color = new Color { A = 0xFF, R = stop1.Color.R, G = (byte) e.NewValue, B = stop1.Color.B };

         // Blue rectangle
         lgb = _rectBlue.Fill as LinearGradientBrush;
         if (lgb == null) return;

         stop0 = lgb.GradientStops[0];
         stop1 = lgb.GradientStops[1];

         stop0.Color = new Color { A = 0xFF, R = stop0.Color.R, G = (byte) e.NewValue, B = stop0.Color.B };
         stop1.Color = new Color { A = 0xFF, R = stop1.Color.R, G = (byte) e.NewValue, B = stop1.Color.B };

         _txtGreen.Text = ((byte) e.NewValue).ToString();

         UpdateSelectedColorRect();
      }

      private void SliderBlue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         // Red rectangle
         var lgb = _rectRed.Fill as LinearGradientBrush;
         if (lgb == null) return;

         var stop0 = lgb.GradientStops[0];
         var stop1 = lgb.GradientStops[1];

         stop0.Color = new Color { A = 0xFF, R = stop0.Color.R, G = stop0.Color.G, B = (byte) e.NewValue };
         stop1.Color = new Color { A = 0xFF, R = stop1.Color.R, G = stop1.Color.G, B = (byte) e.NewValue };

         // Green rectangle
         lgb = _rectGreen.Fill as LinearGradientBrush;
         if (lgb == null) return;

         stop0 = lgb.GradientStops[0];
         stop1 = lgb.GradientStops[1];

         stop0.Color = new Color { A = 0xFF, R = stop0.Color.R, G = stop0.Color.G, B = (byte) e.NewValue };
         stop1.Color = new Color { A = 0xFF, R = stop1.Color.R, G = stop1.Color.G, B = (byte) e.NewValue };

         _txtBlue.Text = ((byte) e.NewValue).ToString();

         UpdateSelectedColorRect();
      }

      private void UpdateSelectedColorRect()
      {
         _rectColor.Fill 
            = new SolidColorBrush(new Color { A = 0xFF, R = (byte) _sliderRed.Value, G = (byte) _sliderGreen.Value, B = (byte) _sliderBlue.Value });
      }
   }
}
