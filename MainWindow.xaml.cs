using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VB6ImageCreator {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		int _trnspThresh;
		Color _colBack, _colTrnsp;

		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			SetBackgroudColorRectBrush();
			SetTransparentColorRectBrush();
			_trnspThresh = (int) Math.Round(_sldTrnspThresh.Value);
			_lblPercent.Content = _trnspThresh.ToString();

			_txtSource.Text = Properties.Settings.Default.LastSourceDir;
			_txtDest.Text = Properties.Settings.Default.LastDestDir;
		}

		private void SetBackgroudColorRectBrush() {
			if (_rectColBack == null) return;

			FillRect(_rectColBack, _txtColBck.Text);
			_colBack = ((SolidColorBrush) _rectColBack.Fill).Color;
		}

		private void SetTransparentColorRectBrush() {
			if (_rectColTransp == null) return;

			FillRect(_rectColTransp, _txtColTrnsp.Text);
			_colTrnsp = ((SolidColorBrush) _rectColTransp.Fill).Color;
		}

		private void FillRect(Rectangle rect, string colHex) {
			try {
				rect.Fill = (SolidColorBrush) (new BrushConverter().ConvertFromString(colHex));
			}
			catch { }
		}

		private void TxtColBck_TextChanged(object sender, TextChangedEventArgs e) {
			SetBackgroudColorRectBrush();
		}

		private void TxtColTrnsp_TextChanged(object sender, TextChangedEventArgs e) {
			SetTransparentColorRectBrush();
		}

		private void SldTrnspThresh_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (_lblPercent == null) return;
			_trnspThresh = ((int) Math.Round(e.NewValue));
			_lblPercent.Content = _trnspThresh.ToString();
		}

		private void BtnConvert_Click(object sender, RoutedEventArgs e) {
			Convert();
		}

		private void Convert() {
			if ((_txtSource.Text == string.Empty) || (_txtDest.Text == string.Empty)) {
				MessageBox.Show("Select a source and a target directory first.");
				return;
			}
			Properties.Settings.Default.Save(); // Remember source and target directories

			Cursor = Cursors.Wait;
			IsEnabled = false;

			try {
				ImageConverter.Convert(_trnspThresh, _colBack, _colTrnsp, _txtSource.Text, _txtDest.Text);
				MessageBox.Show($"Converted {ImageConverter.CountConverted} images.");
			}
			catch (Exception exc) {
				MessageBox.Show(exc.Message, "Oops", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally {
				Cursor = Cursors.Arrow;
				IsEnabled = true;
			}
		}

		private void BtnSelSource_Click(object sender, RoutedEventArgs e) {
			var lastSourceDir = Properties.Settings.Default.LastSourceDir;
			var srcDir = Win32.FolderBrowserDialog.SelectFolder("Select Source Directory", lastSourceDir, new WindowInteropHelper(this).Handle);

			if ((srcDir != null) && (srcDir != string.Empty)) {
				_txtSource.Text = srcDir;
				Properties.Settings.Default.LastSourceDir = srcDir;
			}
		}

		private void BtnSelTgt_Click(object sender, RoutedEventArgs e) {
			var lastDestDir = Properties.Settings.Default.LastDestDir;
			var destDir = Win32.FolderBrowserDialog.SelectFolder("Select Target Directory", lastDestDir, new WindowInteropHelper(this).Handle);

			if ((destDir != null) && (destDir != string.Empty)) {
				_txtDest.Text = destDir;
				Properties.Settings.Default.LastDestDir = destDir;
			}
		}

		private void BtnExit_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
