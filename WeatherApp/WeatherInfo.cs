using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace WeatherApp
{
    public class WeatherInfo
    {
        static string ApiKey = "0a07712c459a768a3181478df78c4608";
        static string ApiBaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        public string City { get; set; }
        public float Temperature { get; set; }
        public string TempUnits { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }
        public float WindSpeed { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }

        public static async Task<string> LoadInfoAsync(string CityName)
        {
            string ApiQuery = ApiBaseUrl + "?q=" + CityName + "&apikey=" + ApiKey + "&mode=xml";
            Task<string> result;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(ApiQuery))
            using (HttpContent content = response.Content)
            {
                result = content.ReadAsStringAsync();
            }
            return await result;
        }

        public static WeatherInfo ParseWeather(System.IO.Stream stream)
        {

            XmlTextReader reader = new XmlTextReader(stream);
            WeatherInfo result = new WeatherInfo()
            {
                City = string.Empty,
                Temperature = float.NaN,
                TempUnits = Properties.Settings.Default.Temperature,
                Pressure = float.NaN,
                Humidity = float.NaN,
                WindSpeed = float.NaN,
                Description = string.Empty,
                Time = DateTime.Now.ToString()
            };

            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "city":
                                result.City = reader.GetAttribute("name");
                                break;
                            case "temperature":
                                result.Temperature = float.Parse(reader.GetAttribute("value"),
                                                                  System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            case "pressure":
                                result.Pressure = float.Parse(reader.GetAttribute("value"));
                                break;
                            case "humidity":
                                result.Humidity = float.Parse(reader.GetAttribute("value"));
                                break;
                            case "weather":
                                result.Description = reader.GetAttribute("value");
                                break;
                            case "wind":
                                reader.ReadToDescendant("speed");
                                result.WindSpeed = float.Parse(reader.GetAttribute("value"),
                                                               System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            case "message":
                                result = null;
                                throw new XmlException();
                        }
                    }
                }
            }
            catch (XmlException error)
            {
                MessageBox.Show(error.Message);
                return null;
            }
            return result;
        }

        public float KelvinToCelsius()
        {
            return Temperature - (float)273.15;
        }

        public float KelvinToFahrenheit()
        {
            float FTemp = (Temperature - (float)273.15) * (float)1.8 + (float)32;
            return FTemp;
        }
    }
}
