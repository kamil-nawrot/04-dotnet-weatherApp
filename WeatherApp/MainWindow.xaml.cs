using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Properties.Settings.Default.Reload();
            LoadWeatherData();
        }

        public async void LoadWeatherData()
        {
            while (true)
            {
                HomeCity.Text = Properties.Settings.Default.HomeCity;

                string homeCity = Properties.Settings.Default.HomeCity;
                string responseXML = await WeatherInfo.LoadInfoAsync(homeCity);
                WeatherInfo result;

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
                {
                    result = WeatherInfo.ParseWeather(stream);
                    if (Properties.Settings.Default.Temperature == "C") HomeCityTemp.Text = result.KelvinToCelsius().ToString("0.00") + "°C";
                    else if (Properties.Settings.Default.Temperature == "K") HomeCityTemp.Text = result.Temperature.ToString("0.00") + "K";
                    else if (Properties.Settings.Default.Temperature == "F") HomeCityTemp.Text = result.KelvinToFahrenheit().ToString("0.00") + "°F";
                    HomeCityPressure.Text = "PRESSURE: " + result.Pressure.ToString() + "hPa";
                    HomeCityHumidity.Text = "HUMIDITY: " + result.Humidity.ToString() + "%";
                    HomeCityWindSpeed.Text = "WIND SPEED: " + result.WindSpeed.ToString() + "km/h";
                    HomeCityDesc.Text = result.Description;
                }

                using (var db = new WeatherDbEntities())
                {
                    decimal tempTemperature;
                    if (Properties.Settings.Default.Temperature == "C") tempTemperature = (decimal)result.KelvinToCelsius();
                    else if (Properties.Settings.Default.Temperature == "K") tempTemperature = (decimal)result.Temperature;
                    else tempTemperature = (decimal)result.KelvinToFahrenheit();

                    HomeCityInfo newInfo = new HomeCityInfo
                    {
                        Id = db.HomeCityInfoes.Count() + 1,
                        City = homeCity,
                        Temperature = Decimal.Round(tempTemperature, 2),
                        TempUnits = Properties.Settings.Default.Temperature,
                        Description = result.Description,
                        Pressure = (int)result.Pressure,
                        Humidity = (int)result.Humidity,
                        WindSpeed = (decimal)result.WindSpeed,
                        Time = result.Time.ToString()
                    };

                    db.HomeCityInfoes.Add(newInfo);
                    db.SaveChanges();
                }

                await Task.Delay(300000);
            }
        }

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            HomeView.Visibility = Visibility.Visible;
            SearchView.Visibility = Visibility.Collapsed;
            SettingsView.Visibility = Visibility.Collapsed;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            HomeView.Visibility = Visibility.Collapsed;
            SearchView.Visibility = Visibility.Visible;
            SettingsView.Visibility = Visibility.Collapsed;
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            HomeView.Visibility = Visibility.Collapsed;
            SearchView.Visibility = Visibility.Collapsed;
            SettingsView.Visibility = Visibility.Visible;

            HomeCitySetting.Text = Properties.Settings.Default.HomeCity;
            ChangeHomeCityBtn.IsChecked = false;
            if (Properties.Settings.Default.Temperature == "C") CelsiusSetting.IsChecked = true;
            else if (Properties.Settings.Default.Temperature == "K") KelvinSetting.IsChecked = true;
            else if (Properties.Settings.Default.Temperature == "F") FahrenheitSetting.IsChecked = true;
        }

        public async void Search ()
        {
            searchInProgress = "True";
            string searchedCity = SearchedCity.Text;
            string responseXML = await WeatherInfo.LoadInfoAsync(searchedCity);
            WeatherInfo result;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
            {
                result = WeatherInfo.ParseWeather(stream);
                if (result != null)
                {
                    SearchedCityName.Text = searchedCity;
                    if (Properties.Settings.Default.Temperature == "C") SearchedCityTemp.Text = result.KelvinToCelsius().ToString("0.00") + "°C";
                    else if (Properties.Settings.Default.Temperature == "K") SearchedCityTemp.Text = result.Temperature.ToString("0.00") + "K";
                    else if (Properties.Settings.Default.Temperature == "F") SearchedCityTemp.Text = result.KelvinToFahrenheit().ToString("0.00") + "°F";
                    SearchedCityPressure.Text = "PRESSURE: " + result.Pressure.ToString() + "hPa";
                    SearchedCityHumidity.Text = "HUMIDITY: " + result.Humidity.ToString() + "%";
                    SearchedCityDesc.Text = result.Description;
                    SearchedCityWindSpeed.Text = "WIND SPEED: " + result.WindSpeed.ToString() + "km/h";
                }
            }

            await Task.Delay(1000);

            if (result != null)
            {
                SearchResult.Visibility = Visibility.Visible;
            }

            SearchedCity.Text = String.Empty;
            searchInProgress = "False";
        }

        private void StartSearching_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private async void ChangeHomeCityBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchedCity = HomeCitySetting.Text;
            string responseXML = await WeatherInfo.LoadInfoAsync(searchedCity);
            WeatherInfo result;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
            {
                result = WeatherInfo.ParseWeather(stream);
                if (result != null)
                {
                    Properties.Settings.Default.HomeCity = HomeCitySetting.Text;
                    Properties.Settings.Default.Save();
                    LoadWeatherData();
                }
            }
        }

        private async void HomeCitySetting_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            e.Handled = true;
            string searchedCity = HomeCitySetting.Text;
            string responseXML = await WeatherInfo.LoadInfoAsync(searchedCity);
            WeatherInfo result;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
            {
                result = WeatherInfo.ParseWeather(stream);
                if (result != null)
                {
                    ChangeHomeCityBtn.IsChecked = true;
                    Properties.Settings.Default.HomeCity = HomeCitySetting.Text;
                    Properties.Settings.Default.Save();
                    LoadWeatherData();
                }
            }
        }

        private void KelvinSetting_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Temperature = "K";
            Properties.Settings.Default.Save();
            LoadWeatherData();
        }

        private void CelsiusSetting_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Temperature = "C";
            Properties.Settings.Default.Save();
            LoadWeatherData();
        }

        private void FahrenheitSetting_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Temperature = "F";
            Properties.Settings.Default.Save();
            LoadWeatherData();
        }

        private void SearchedCity_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            e.Handled = true;
            Search();
        }

        private string _searchInProgress = "False";
        public string searchInProgress
        {
            get { return _searchInProgress; }
            set
            {
                _searchInProgress = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string searchInProgress = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(searchInProgress);
                handler(this, e);
            }
        }

        #endregion

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)HistoryBtn.Content == "SHOW HISTORY")
            {
                WeatherImage.Visibility = Visibility.Collapsed;
                HistoryList.Visibility = Visibility.Visible;
                AddNewInfoBtn.Visibility = Visibility.Visible;
                HistoryBtn.Content = "HIDE HISTORY";

                using (var db = new WeatherDbEntities())
                {
                    var query = (from info in db.HomeCityInfoes
                                 select new WeatherInfo
                                 {
                                     City = info.City,
                                     Temperature = (float)info.Temperature,
                                     TempUnits = info.TempUnits,
                                     Pressure = info.Pressure,
                                     Humidity = info.Humidity,
                                     WindSpeed = (float)info.WindSpeed,
                                     Description = info.Description,
                                     Time = info.Time
                                 });

                    var weatherInfoList = new List<WeatherInfo>(query);
                    HistoryList.ItemsSource = weatherInfoList;
                }
            }
            else
            {
                WeatherImage.Visibility = Visibility.Visible;
                HistoryList.Visibility = Visibility.Collapsed;
                AddNewInfoBtn.Visibility = Visibility.Collapsed;
                HistoryBtn.Content = "SHOW HISTORY";
            }
        }

        private void AddNewInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AddNewInfo.Visibility == Visibility.Collapsed)
            {
                HistoryList.Visibility = Visibility.Collapsed;
                AddNewInfo.Visibility = Visibility.Visible;
                HistoryBtn.IsEnabled = false;
            }
            else if (AddNewInfo.Visibility == Visibility.Visible)
            {
                using (var db = new WeatherDbEntities())
                {
                    HomeCityInfo newInfo = new HomeCityInfo
                    {
                        Id = db.HomeCityInfoes.Count() + 1,
                        City = AddNewCity.Text,
                        Temperature = decimal.Parse(AddNewTemp.Text),
                        TempUnits = "C",
                        Description = AddNewDesc.Text,
                        Pressure = int.Parse(AddNewPressure.Text),
                        Humidity = int.Parse(AddNewHumidity.Text),
                        WindSpeed = decimal.Parse(AddNewWindSpeed.Text),
                        Time = DateTime.Now.ToString()
                    };

                    db.HomeCityInfoes.Add(newInfo);
                    db.SaveChanges();
                }

                HistoryList.Visibility = Visibility.Visible;
                AddNewInfo.Visibility = Visibility.Collapsed;
                HistoryBtn.IsEnabled = true;
                HistoryBtn_Click(sender, e);
            }
        }
    }
}
