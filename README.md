# 🏦 Kredi Al - Taksitli Ödeme Sistemi

Modern, güvenli ve ölçeklenebilir bir taksitli ödeme altyapısı. E-ticaret siteleri için banka entegrasyonu ve kredi onay süreçlerini yöneten kapsamlı bir API sistemi.

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Teknoloji Stack](#-teknoloji-stack)
- [Mimari](#-mimari)
- [Kurulum](#-kurulum)
- [API Kullanımı](#-api-kullanımı)
- [Transaction Akışı](#-transaction-akışı)
- [Swagger Dokümantasyonu](#-swagger-dokümantasyonu)
- [Geliştirme](#-geliştirme)

## ✨ Özellikler

### 🔐 Güvenlik
- JWT tabanlı kimlik doğrulama
- Marketplace credential validation
- Secure API key management
- Transaction expiration kontrolü

### 💳 Ödeme Yönetimi
- Çoklu banka entegrasyonu
- Dinamik faiz oranı hesaplama
- Taksit seçenekleri (12, 24, 36 ay)
- Komisyon ödeme sistemi

### 📊 Findeks Entegrasyonu
- Kredi skoru kontrolü
- Kefalet tutarı hesaplama
- Otomatik onay süreci

### 🔄 Transaction Yönetimi
- Durum bazlı akış kontrolü
- Otomatik süre dolumu
- Detaylı hata yönetimi
- Transaction history

## 🛠 Teknoloji Stack

### Backend
- **.NET 9.0** - Modern C# framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core** - ORM
- **In-Memory Database** - Demo için hızlı test

### Güvenlik & Auth
- **JWT (JSON Web Tokens)** - Kimlik doğrulama
- **BCrypt** - Şifre hashleme

### Dokümantasyon
- **Swagger/OpenAPI** - Interaktif API dokümantasyonu
- **Swashbuckle** - Swagger entegrasyonu

### Mimari Pattern
- **Clean Architecture** - Katmanlı mimari
- **Repository Pattern** - Veri erişim soyutlaması
- **Dependency Injection** - Loose coupling

## 🏗 Mimari

Proje Clean Architecture prensiplerine göre 4 katmandan oluşur:

```
KrediAl/
├── src/
│   ├── KrediAl.Domain/          # Entity'ler ve domain logic
│   │   ├── Entities/
│   │   └── Enums/
│   │
│   ├── KrediAl.Application/     # DTO'lar ve interface'ler
│   │   ├── DTOs/
│   │   └── Interfaces/
│   │
│   ├── KrediAl.Infrastructure/  # Veri erişim ve servisler
│   │   ├── Data/
│   │   └── Services/
│   │
│   └── KrediAl.API/             # API endpoints ve controllers
│       ├── Controllers/
│       └── wwwroot/
```

### Transaction Durumları

```
Created → OrderConfirmed → UserAuthenticated → FindeksApproved 
    → CommissionPaid → BankRedirected → Completed
```

Her durum geçişi validation ile korunur ve sadece izin verilen geçişler yapılabilir.

## 🚀 Kurulum

### Gereksinimler

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

### Adımlar

1. **Projeyi klonlayın**
```bash
git clone https://github.com/[username]/KrediAl.git
cd KrediAl
```

2. **Bağımlılıkları yükleyin**
```bash
dotnet restore
```

3. **Projeyi derleyin**
```bash
dotnet build
```

4. **API'yi başlatın**
```bash
cd src/KrediAl.API
dotnet run --urls "http://localhost:5800"
```

5. **Swagger'ı açın**
```
http://localhost:5800
```

## 📡 API Kullanımı

### 1️⃣ Session Oluşturma (Marketplace)

```bash
curl -X POST http://localhost:5800/api/marketplace/create-session \
  -H "Content-Type: application/json" \
  -d '{
    "mpUser": "demo_mp",
    "mpPassword": "demo123",
    "order": {
      "successUrl": "https://marketplace.com/success",
      "rejectUrl": "https://marketplace.com/reject",
      "orderId": "ORD-12345",
      "totalAmount": 15000,
      "items": [{
        "category": "Elektronik",
        "unitPrice": 15000,
        "tax": 20
      }]
    }
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "transactionId": "guid",
    "redirectUrl": "http://localhost:5800/transaction.html?id=guid"
  }
}
```

### 2️⃣ Kullanıcı Girişi

```bash
curl -X POST http://localhost:5800/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "userId": "guid",
    "email": "test@example.com"
  }
}
```

### 3️⃣ Banka Tekliflerini Görüntüleme

```bash
curl -X GET http://localhost:5800/api/transaction/{id}/bank-offers \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "bankName": "Garanti BBVA",
      "bankCode": "GARANTI",
      "loanAmount": 15000,
      "installmentCount": 12,
      "interestRate": 1.89,
      "monthlyPayment": 1350.50,
      "totalPayment": 16206.00
    }
  ]
}
```

## 🔄 Transaction Akışı

### Tam Akış Diyagramı

```
┌─────────────────┐
│   Marketplace   │
│  CreateSession  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  ConfirmOrder   │
│   (Müşteri)     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  User Login     │
│  + LinkUser     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ RequestFindeks  │
│   (Approval)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ GetBankOffers   │
│  (9 teklif)     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ PayCommission   │
│   (Ödeme)       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│SelectBankOffer  │
│  (Banka seç)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ BankRedirect    │
│  (Completed)    │
└─────────────────┘
```

### Durum Geçişleri

| Mevcut Durum | İzin Verilen Geçişler |
|--------------|----------------------|
| `Created` | `OrderConfirmed` |
| `OrderConfirmed` | `UserAuthenticated` |
| `UserAuthenticated` | `FindeksApprovalPending`, `FindeksApproved` |
| `FindeksApprovalPending` | `FindeksApproved` |
| `FindeksApproved` | `CommissionPaid` |
| `CommissionPaid` | `BankRedirected` |
| `BankRedirected` | `Completed` |

## 📚 Swagger Dokümantasyonu

API, kapsamlı Swagger dokümantasyonu ile gelir:

- **Interaktif test arayüzü**
- **Detaylı endpoint açıklamaları**
- **Request/Response örnekleri**
- **Authentication desteği**

Swagger'a erişim:
```
http://localhost:5800
```

### Endpoint Kategorileri

- 🏪 **Marketplace** - Session yönetimi
- 👤 **Auth** - Kimlik doğrulama
- 💳 **Transaction** - İşlem yönetimi
- 🏦 **Bank** - Banka teklifleri
- 🔍 **Test** - Debug endpoints

## 🧪 Test

### Demo Credentials

**Marketplace:**
- Username: `demo_mp`
- Password: `demo123`

**Test User:**
- Email: `test@example.com`
- Password: `Test123!`

### Test Bankaları

Sistem 3 test bankası ile gelir:

1. **Garanti BBVA**
   - Kredi aralığı: 1.000₺ - 100.000₺
   - Taksit: 3-36 ay

2. **Yapı Kredi**
   - Kredi aralığı: 1.000₺ - 150.000₺
   - Taksit: 3-48 ay

3. **İş Bankası**
   - Kredi aralığı: 2.000₺ - 200.000₺
   - Taksit: 6-60 ay

### HTML Test Sayfası

Tarayıcıda test için:
```
http://localhost:5800/transaction.html
```

## 🔧 Geliştirme

### Yeni Banka Ekleme

1. `Program.cs` içinde seed data'ya ekleyin:
```csharp
new Bank {
    Name = "Yeni Banka",
    Code = "YENIBANKA",
    IsActive = true,
    MinLoanAmount = 1000,
    MaxLoanAmount = 100000,
    MinInstallment = 3,
    MaxInstallment = 36
}
```

2. `BankService.cs` içinde faiz oranı ekleyin:
```csharp
private decimal CalculateInterestRate(string bankCode, int installment)
{
    return bankCode switch {
        "YENIBANKA" => installment switch {
            12 => 1.75m,
            24 => 1.85m,
            36 => 1.95m,
            _ => 2.0m
        },
        // ...
    };
}
```

### Yeni Transaction Durumu Ekleme

1. `TransactionStatus` enum'ına ekleyin
2. `TransactionService` içinde validation logic ekleyin
3. İlgili controller method'larını güncelleyin

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📧 İletişim

Sorularınız için issue açabilirsiniz.

---

**Not:** Bu proje demo amaçlıdır. Production kullanımı için:
- Gerçek database kullanın (SQL Server, PostgreSQL)
- API key'leri environment variable'lara taşıyın
- Rate limiting ekleyin
- Logging ve monitoring ekleyin
- Unit ve integration testler yazın
