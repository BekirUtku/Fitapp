# FitApp — Spor Takip Uygulaması

Local-first, offline uyumlu, kural tabanlı gelişim önerileri sunan mobil spor takip uygulaması.

- **Mobil:** Flutter + Riverpod + Hive + Dio
- **Backend:** ASP.NET Core (LTS) + EF Core + PostgreSQL, Clean Architecture
- **Auth:** JWT + Refresh Token
- **AI:** Kural tabanlı (LLM yok) — deterministik istatistik + şablon cümle

Tam teknik plan için `docs/` altındaki planlama dokümanına bakın. Bu depo **Faz 0 (iskele)** aşamasındadır.

---

## Depo Yapısı

```
Spor Uygulaması/
├── backend/            ASP.NET Core çözümü (Clean Architecture)
│   ├── FitApp.sln
│   └── src/
│       ├── FitApp.Domain/          Entity'ler, enum'lar, iş kuralları
│       ├── FitApp.Application/      CQRS, DTO, engine'ler, arayüzler
│       ├── FitApp.Infrastructure/   EF Core, PostgreSQL, JWT, Serilog
│       └── FitApp.Api/              Controller, middleware, Swagger
└── mobile/
    └── fitapp/         Flutter uygulaması (feature-first + design system)
```

---

## Ön Gereksinimler

| Araç | Sürüm (öneri) |
|------|----------------|
| .NET SDK | 8.0 (LTS) |
| PostgreSQL | 15+ |
| Flutter | Stable (3.x) |
| Dart | Flutter ile gelir |

---

## Backend'i Çalıştırma

```bash
cd backend

# Bağımlılıkları geri yükle
dotnet restore

# Veritabanı bağlantısını ayarla (appsettings.Development.json → ConnectionStrings:Default)

# EF Core araçları (bir kez)
dotnet tool install --global dotnet-ef

# İlk migration'ı oluştur ve uygula
dotnet ef migrations add InitialCreate \
  --project src/FitApp.Infrastructure \
  --startup-project src/FitApp.Api
dotnet ef database update \
  --project src/FitApp.Infrastructure \
  --startup-project src/FitApp.Api

# API'yi başlat (Swagger: https://localhost:xxxx/swagger)
dotnet run --project src/FitApp.Api
```

Seed ile varsayılan bir **Admin** hesabı, kas grupları ve ekipmanlar oluşturulur (bkz. `Persistence/Seed`).

---

## Mobil Uygulamayı Çalıştırma

```bash
cd mobile/fitapp
flutter pub get
flutter run
```

`lib/core/config/env.dart` içinde backend base URL'i ayarlayın.

---

## Mimari İlkeler

- **UUID (GUID) Primary Key** — offline oluşturulan kayıtların çakışmasız senkronizasyonu için.
- **Local-first** — Hive gerçeğin kaynağı; backend senkron hedefi (outbox + `sync/push` + `sync/pull`).
- **Tombstone silme**, **last-write-wins** çakışma çözümü.
- **Clean Architecture + SOLID + Repository + DI + CQRS (gerektiğinde) + FluentValidation + AutoMapper.**
- **Design System** merkezî; her ekran ortak bileşenlere bağlanır.

## Yol Haritası (özet)

Faz 0 İskele · Faz 1 Auth+Katalog+Admin · Faz 2 Programlar · Faz 3 Focus Mode · Faz 4 Sync · Faz 5 İlerleme · Faz 6 Beslenme · Faz 7 Kural Motorları · Faz 8 Gamification · Faz 9 Export · Faz 10 Cila.
