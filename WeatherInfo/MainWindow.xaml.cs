using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Web.Http;

namespace WeatherInfo {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		Geopoint _currentPoint;
		const string ApiKey = "&APPID=af02ab34dd14e5b4a2eb26767f8015f1&units=metric";

		public MainWindow() {
			InitializeComponent();

			Loaded += async delegate {
				_latitude.Text = "getting info...";
				var position = await new Geolocator().GetGeopositionAsync();
				_currentPoint = position.Coordinate.Point;
				_latitude.Text = _currentPoint.Position.Latitude.ToString("G4");
				_longitude.Text = _currentPoint.Position.Longitude.ToString("G4");
				_getWeather.IsEnabled = true;
			};
		}

		private async void OnGetWeather(object sender, RoutedEventArgs e) {
			string apicall = $"http://api.openweathermap.org/data/2.5/weather?lat={_currentPoint.Position.Latitude}&lon={_currentPoint.Position.Longitude}";
			try {
				var hc = new HttpClient();
				var result = await hc.GetStringAsync(new Uri(apicall + ApiKey));
				var json = JsonObject.Parse(result);
				var main = json["main"];
				var general = json["weather"].GetArray()[0];
				var weather = general.GetObject()["main"].GetString();
				var desc = general.GetObject()["description"].GetString();
				var temp = main.GetObject()["temp"].GetNumber();
				_temp.Text = $"{temp} Celsius";
				_desc.Text = $"{weather} ({desc})";
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message, "Error");
			}
		}
	}
}
