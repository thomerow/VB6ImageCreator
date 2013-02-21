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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Interop;

namespace VB6ImageCreator
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      int _trnspThresh = 50;
      Color _colBack, _colTrnsp;

      public MainWindow()
      {
         InitializeComponent();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         SetBackgroudColorRectBrush();
         SetTransparentColorRectBrush();
         _lblPercent.Content = ((int)Math.Round(_sldTrnspThresh.Value)).ToString();
      }

      private void SetBackgroudColorRectBrush()
      {
         if (_rectColBack == null) return;

         FillRect(_rectColBack, _txtColBck.Text);
         _colBack = ((SolidColorBrush) _rectColBack.Fill).Color;
      }

      private void SetTransparentColorRectBrush()
      {
         if (_rectColTransp == null) return;

         FillRect(_rectColTransp, _txtColTrnsp.Text);
         _colTrnsp = ((SolidColorBrush) _rectColTransp.Fill).Color;
      }

      private void FillRect(Rectangle rect, string colHex)
      {
         try
         {
            rect.Fill = (SolidColorBrush) (new BrushConverter().ConvertFromString(colHex));
         }
         catch { }
      }

      private void TxtColBck_TextChanged(object sender, TextChangedEventArgs e)
      {
         SetBackgroudColorRectBrush();
      }

      private void TxtColTrnsp_TextChanged(object sender, TextChangedEventArgs e)
      {
         SetTransparentColorRectBrush();
      }

      private void SldTrnspThresh_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         if (_lblPercent == null) return;
         _trnspThresh = ((int) Math.Round(e.NewValue));
         _lblPercent.Content = _trnspThresh.ToString();
      }

      private void BtnConvert_Click(object sender, RoutedEventArgs e)
      {
         Convert();
      }

      private void Convert()
      {
         if ((_txtSource.Text == string.Empty) || (_txtDest.Text == string.Empty))
         {
            MessageBox.Show("Select a source and a target directory first.");
            return;
         }

         Cursor = Cursors.Wait;
         IsEnabled = false;
         ImageConverter.Convert(_trnspThresh, _colBack, _colTrnsp, _txtSource.Text, _txtDest.Text);
         Cursor = Cursors.Arrow;
         IsEnabled = true;

         MessageBox.Show("Converted " + ImageConverter.CountConverted.ToString() + " images.");
      }

      private void BtnSelSource_Click(object sender, RoutedEventArgs e)
      {
         _txtSource.Text = FolderBrowserDialog.SelectFolder("Select Source Directory", "", new WindowInteropHelper(this).Handle);
      }

      private void BtnSelTgt_Click(object sender, RoutedEventArgs e)
      {
         _txtDest.Text = FolderBrowserDialog.SelectFolder("Select Target Directory", "", new WindowInteropHelper(this).Handle);
      }
   }
}
