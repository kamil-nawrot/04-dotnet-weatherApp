# WeatherApp

Aplikacja desktopowa stworzona z wykorzystaniem .NET Framework i Windows Presentation Foundation.

## Dane pogodowe
Aplikacja przedstawia użytkownikowi dane pogodowe pobierane z zewnętrznego API **[OpenWeatherMap](https://openweathermap.org/current)**. Otrzymywane z API dokumenty XML są parsowane z wykorzystaniem **XmlTextReader**. Pewne dane pobieranę są cyklicznie co 5 minut, istnieje także możliwość pobierania informacji dotyczących konkretnego miasta na życzenie użytkownika.

## Interfejs użytkownika
Całość interfejsu została wykonana w oparciu o **[Material Design in XAML Toolkit](http://materialdesigninxaml.net)** oraz dobre praktyki projektowania **Material Design**. 

## Baza danych
Dane pobierane cyklicznie są zapisywane w bazie danych zagnieżdżonej wewnątrz projektu. Jest to baza **SQLite**, której połączenie z programem zrealizowano z wykorzystaniem **Entity Framework**.
