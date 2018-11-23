using DotNext.Native;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Mandelbrot {
	public partial class MainWindow : Window {
		WriteableBitmap _bmp;
		Complex _from = new Complex(-1.5, -1), _to = new Complex(1, 1);
		DispatcherTimer _timer;
		Stopwatch _sw = new Stopwatch();

		public MainWindow() {
			InitializeComponent();

			_timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(10) };
			_timer.Tick += delegate {
				_text.Text = _sw.ElapsedMilliseconds.ToString();
			};

			var res = Application.GetResourceStream(new Uri("nice.xml", UriKind.Relative));
			_rainbow = ColorGradientPersist.Read(res.Stream).GenerateColors(512);

			_image.Loaded += async delegate {
				await CreateBitmapAndRunAsync(_from, _to);
			};

		}

		async Task CreateBitmapAndRunAsync(Complex from, Complex to) {
			Title = "Mandelbrot: Working...";
			_sw.Restart();
			_timer.Start();

			int width = _image.ActualWidth == 0 ? 600 : (int)_image.ActualWidth;
			int height = _image.ActualHeight == 0 ? 600 : (int)_image.ActualHeight;
			_bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
			_image.Source = _bmp;
			await RunMandelbrotAsync(from, to);

			_sw.Stop();
			_timer.Start();
			Title += $" done. Elapsed: {_sw.ElapsedMilliseconds} msec";
		}

		async Task RunMandelbrotAsync(Complex from, Complex to) {
			_from = from; _to = to;
			int width = _bmp.PixelWidth, height = _bmp.PixelHeight;
			var deltax = (to.Real - from.Real) / _bmp.Width;
			var deltay = (to.Imaginary - from.Imaginary) / _bmp.Height;
			var pixels = new int[width];

			var options = new ParallelOptions {
				MaxDegreeOfParallelism = (bool)_cpuSets.IsChecked ? Environment.ProcessorCount - 2 : -1
			};

			for (int y = 0; y < height; y++) {
				await Task.Run(() => {
					Parallel.For(0, width, options, x => {
						pixels[x] = MandelbrotColor(from + new Complex(x * deltax, y * deltay));
					});
				});
				_bmp.WritePixels(new Int32Rect(0, y, width, 1), pixels, _bmp.BackBufferStride, 0);
			}
		}

		readonly Color[] _rainbow;

		int MandelbrotColor(Complex c) {
			int color = _rainbow.Length;

			var z = Complex.Zero;
			while (z.Real * z.Real + z.Imaginary * z.Imaginary <= 4 && color > 0) {
				z = z * z + c;
				color--;
			}
			return color == 0 ? Colors.Black.ToInt() : _rainbow[color].ToInt();
		}

		bool _isSelecting;
		Point _start;

		private void OnMouseDown(object sender, MouseButtonEventArgs e) {
			var pt = e.GetPosition(_image);
			_start = pt;
			_selection.Visibility = Visibility.Visible;
			_image.CaptureMouse();
			_rect.Rect = new Rect(pt.X, pt.Y, 0, 0);
			_isSelecting = true;
		}

		private void OnMouseMove(object sender, MouseEventArgs e) {
			if (_isSelecting) {
				var pt = e.GetPosition(_image);
				_rect.Rect = new Rect(Math.Min(_start.X, pt.X), Math.Min(_start.Y, pt.Y), Math.Abs(pt.X - _start.X), Math.Abs(pt.Y - _start.Y));
			}
		}

		private async void OnMouseUp(object sender, MouseButtonEventArgs e) {
			if (_isSelecting) {
				_isSelecting = false;
				_selection.Visibility = Visibility.Hidden;
				var pt = e.GetPosition(_image);
				var rc = _rect.Rect;
				double newWidth = rc.Width * (_to.Real - _from.Real) / _image.ActualWidth;
				double newHeight = rc.Height * (_to.Imaginary - _from.Imaginary) / _image.ActualHeight;
				double deltax = rc.X * (_to.Real - _from.Real) / _image.ActualWidth;
				double deltay = rc.Y * (_to.Imaginary - _from.Imaginary) / _image.ActualHeight;
				_from = _from + new Complex(deltax, deltay);
				_to = _from + new Complex(newWidth, newHeight);
				_image.ReleaseMouseCapture();
				await CreateBitmapAndRunAsync(_from, _to);
			}

		}

		private void OnBackgroundMode(object sender, RoutedEventArgs e) {
			if ((bool)_backgroundMode.IsChecked)
				ProcessExtensions.SetBackgroundMode();
			else
				ProcessExtensions.EndBackgroundMode();
		}

		private void OnSetCpuSets(object sender, RoutedEventArgs e) {
			if ((bool)_cpuSets.IsChecked) {
				// set default CPU set to exclude first two processors
				var cpus = 3UL;
				var cpuSets = ((1UL << Environment.ProcessorCount) - 1) & ~cpus;
				Process.GetCurrentProcess().SetDefaultCpuSets(cpuSets);

				Thread.CurrentThread.SetSelectedCpuSets(cpus);
			}
			else {
				Process.GetCurrentProcess().SetDefaultCpuSets(0);
				Thread.CurrentThread.SetSelectedCpuSets(0);
			}
		}

		private void OnPriorityClass(object sender, RoutedEventArgs e) {
			Process.GetCurrentProcess().PriorityClass = (bool)_priorityClass.IsChecked ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
		}

		private void OnThreadPriority(object sender, RoutedEventArgs e) {
			Thread.CurrentThread.SetPriority((bool)_threadPriority.IsChecked ? NativeThreadPriority.TimeCritical : NativeThreadPriority.Normal);
		}

		private async void OnReset(object sender, RoutedEventArgs e) {
			await CreateBitmapAndRunAsync(new Complex(-1.5, -1), new Complex(1, 1));
		}
	}
}
