# 📦 MagMini ERP & WMS Ecosystem

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/Desktop-WPF%20MVVM-blue?logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![ASP.NET Core](https://img.shields.io/badge/Backend-ASP.NET%20Core%20Web%20API-green?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![.NET MAUI](https://img.shields.io/badge/Mobile-MAUI%20Android%20%2F%20iOS-purple?logo=dotnet)](https://dotnet.microsoft.com/apps/maui)
[![EF Core](https://img.shields.io/badge/ORM-EF%20Core%20%2B%20MS%20SQL-orange?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/ef/core/)

Nowoczesny, wieloplatformowy system klasy ERP / WMS (zarzadzanie sprzedaza, magazynem i zamowieniami) inspirowany systemami takimi jak WAPRO MAG czy Subiekt GT.

Projekt zostal zrealizowany w architekturze Clean Architecture z pelna separacja logiki biznesowej, wspierajac aplikacje desktopowa WPF, bezpieczne REST Web API (JWT) oraz mobilny kolektor magazynowy .NET MAUI.

---

## 🏛️ Architektura Systemu

Rozwiazanie bazuje na zasadach **SOLID**, **DRY**, **DDD** oraz **Clean Architecture**:


```
                                  ┌──────────────────────────────┐
                                  │   MagMini.UI (Desktop WPF)   │
                                  └──────────────┬───────────────┘
                                                 │
┌───────────────────────────┐                    │
│ MagMini.Api (ASP.NET Core)│────────────────────┤ 
└─────────────┬─────────────┘                    │
              │                                  ▼
              │                    ┌───────────────────────────┐
              │                    │    MagMini.Application    │  
              │                    └─────────────┬─────────────┘
              │                                  │
              │                                  ▼
              │                    ┌───────────────────────────┐
              │                    │     MagMini.Domain        │  
              │                    └─────────────┬─────────────┘
              │                                  │
              └──────────────────────────────────┼────────────────────────┐
                                                 ▼                        ▼
                                   ┌───────────────────────────┐   ┌────────────┐
                                   │   MagMini.Infrastructure  │──►│ Baza MS SQL│
                                   └───────────────────────────┘   └────────────┘
                                                 ▲
                                                 │ (Komunikacja REST + JWT)
                                   ┌─────────────┴─────────────┐
                                   │  MagMini.Mobile (.NET MAUI│
                                   │   Android / iOS / WinUI)  │
                                   └───────────────────────────┘
```

- MagMini.Domain (Encje, Reguly handlowe, Enumy)
- MagMini.Application (DTO, Serwisy, Walidacje, Interfejsy)
- MagMini.Infrastructure (EF Core, Baza MS SQL, Migracje, GUS BIR1 SOAP, MF Biala Lista)
- MagMini.UI (Desktop WPF MVVM, Generic Host, Ribbon, Asynchroniczny SplashScreen)
- MagMini.Api (ASP.NET Core Minimal APIs, JWT Bearer, Swagger UI)
- MagMini.Mobile (.NET MAUI, Android WMS, Skaner kodow kreskowych, Offline JWT SecureStorage)

---

## 🌟 Kluczowe Moduly i Funkcjonalnosci

### 1. 🖥️ Aplikacja Desktopowa (MagMini.UI - WPF)
- Asynchroniczny SplashScreen z migracjami bazy w tle
- Shell Layout: Menu wstazkowe (Ribbon) + Dolny pasek stanu sesji i bazy
- Kartoteka Towarowa: Server-side pagination (200 szt.), filtry wielokolumnowe
- Kartoteka Kontrahentow: Walidacja sumy kontrolnej NIP + Hybrydowe pobieranie danych z rejestrow GUS REGON i Bialej Listy MF
- Modul Zamowien (ZK): Automatyczna numeracja, kalkulator podatkowy (Netto/VAT/Brutto), automatyczne zdejmowanie stanow magazynowych

### 2. 🌐 Bezpieczne Web API (MagMini.Api - ASP.NET Core)
- REST Minimal APIs dla wszystkich kartotek
- Autoryzacja JWT Bearer Token (HMAC-SHA256)
- Swagger UI z kłódką autoryzacyjną
- Automatyczny audyt uzytkownika w bazie danych

### 3. 📱 Mobilny Kolektor Magazynowy (MagMini.Mobile - .NET MAUI)
- Kafelkowy Dashboard dla magazyniera
- Automatyczny AuthHeaderHandler (wstrzykiwanie JWT z SecureStorage)
- Skaner kodow kreskowych (weryfikator cen i stanow)
- Kompletacja i wydawanie zamowien ZK z telefonu

---

## 🚀 Jak uruchomic projekt

1. Desktop WPF:
   dotnet run --project src/MagMini.UI (Login: admin / Haslo: admin123)

2. Web API i Swagger:
   dotnet run --project src/MagMini.Api (Swagger: https://localhost:7156/swagger)

3. Mobile MAUI:
   Uruchom projekt MagMini.Mobile na emulatorze Androida z poziomu Visual Studio (F5).

---

## 📄 Licencja
Projekt udostepniany na licencji MIT.