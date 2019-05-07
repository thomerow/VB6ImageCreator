using System;
using System.Threading.Tasks;
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

		int m_trnspThresh;
		Color m_colBack, m_colTrnsp;

		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			SetBackgroudColorRectBrush();
			SetTransparentColorRectBrush();
			m_trnspThresh = (int) Math.Round(m_sldTrnspThresh.Value);
			m_lblPercent.Content = m_trnspThresh.ToString();

			m_txtSource.Text = Properties.Settings.Default.LastSourceDir;
			m_txtDest.Text = Properties.Settings.Default.LastDestDir;
		}

		private void SetBackgroudColorRectBrush() {
			if (m_rectColBack == null) return;

			FillRect(m_rectColBack, m_txtColBck.Text);
			m_colBack = ((SolidColorBrush) m_rectColBack.Fill).Color;
		}

		private void SetTransparentColorRectBrush() {
			if (m_rectColTransp == null) return;

			FillRect(m_rectColTransp, m_txtColTrnsp.Text);
			m_colTrnsp = ((SolidColorBrush) m_rectColTransp.Fill).Color;
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
			if (m_lblPercent == null) return;
			m_trnspThresh = ((int) Math.Round(e.NewValue));
			m_lblPercent.Content = m_trnspThresh.ToString();
		}

		private async void BtnConvert_Click(object sender, RoutedEventArgs e) {
			await Convert();
		}

		private async Task Convert() {
			if ((m_txtSource.Text == string.Empty) || (m_txtDest.Text == string.Empty)) {
				MessageBox.Show("Select a source and a target directory first.");
				return;
			}
			Properties.Settings.Default.Save(); // Remember source and target directories

			Cursor = Cursors.Wait;
			IsEnabled = false;

			try {
				await ImageConverter.Convert(m_trnspThresh, m_colBack, m_colTrnsp, m_txtSource.Text, m_txtDest.Text);
				MessageBox.Show(this, $"Converted {ImageConverter.CountConverted} images.", "Successs", MessageBoxButton.OK, MessageBoxImage.Information);
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
			string lastSourceDir = Properties.Settings.Default.LastSourceDir;
			string dirSrc = Win32.FolderBrowserDialog.SelectFolder("Select Source Directory", lastSourceDir, new WindowInteropHelper(this).Handle);

			if ((dirSrc != null) && (dirSrc != string.Empty)) {
				m_txtSource.Text = dirSrc;
				Properties.Settings.Default.LastSourceDir = dirSrc;
			}
		}

		private void BtnSelTgt_Click(object sender, RoutedEventArgs e) {
			string lastDestDir = Properties.Settings.Default.LastDestDir;
			string dirDest = Win32.FolderBrowserDialog.SelectFolder("Select Target Directory", lastDestDir, new WindowInteropHelper(this).Handle);

			if ((dirDest != null) && (dirDest != string.Empty)) {
				m_txtDest.Text = dirDest;
				Properties.Settings.Default.LastDestDir = dirDest;
			}
		}

		private void BtnExit_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
