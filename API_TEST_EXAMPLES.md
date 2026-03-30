# Kredi Al API Test Örnekleri

API şu anda çalışıyor: **http://localhost:5000**

## 1. Session Oluşturma (Pazaryerinden)

```bash
curl -X POST http://localhost:5000/api/marketplace/create-session \
  -H "Content-Type: application/json" \
  -d '{
    "mp_user": "demo_mp",
    "mp_password": "demo123",
    "order": {
      "success_url": "https://marketplace.com/success",
      "reject_url": "https://marketplace.com/reject",
      "order_id": "ORD-12345",
      "total_amount": 15000,
      "items": [
        {
          "category": "cep telefonu",
          "unit_price": 15000,
          "tax": 20
        }
      ]
    }
  }'
```

**Beklenen Response:**
```json
{
  "data": {
    "order_id": "ORD-12345",
    "redirect_url": "http://localhost:5000/transaction/{transaction_id}"
  }
}
```

## 2. Kullanıcı Kaydı

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "ahmet@example.com",
    "password": "Test123!",
    "firstName": "Ahmet",
    "lastName": "Yılmaz",
    "phoneNumber": "05551234567",
    "nationalId": "12345678901"
  }'
```

**Beklenen Response:**
```json
{
  "userId": "guid",
  "email": "ahmet@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "firstName": "Ahmet",
  "lastName": "Yılmaz"
}
```

## 3. Kullanıcı Girişi

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "ahmet@example.com",
    "password": "Test123!"
  }'
```

## 4. Transaction Detayı Görüntüleme

```bash
curl -X GET http://localhost:5000/api/transaction/{transaction_id}
```

## 5. Siparişi Onaylama

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/confirm-order
```

## 6. Kullanıcıyı Transaction'a Bağlama (Auth Gerekli)

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/link-user \
  -H "Authorization: Bearer {token}"
```

## 7. İşleme Devam Etme (Auth Gerekli)

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/continue \
  -H "Authorization: Bearer {token}"
```

## 8. Findeks Onayı İsteme (Auth Gerekli)

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/request-findeks \
  -H "Authorization: Bearer {token}"
```

## 9. Banka Tekliflerini Görüntüleme (Auth Gerekli)

```bash
curl -X GET http://localhost:5000/api/transaction/{transaction_id}/bank-offers \
  -H "Authorization: Bearer {token}"
```

**Beklenen Response:**
```json
[
  {
    "id": "guid",
    "bankName": "Garanti BBVA",
    "bankCode": "GARANTI",
    "loanAmount": 15000,
    "installmentCount": 12,
    "interestRate": 1.89,
    "monthlyPayment": 1350.50,
    "totalPayment": 16206.00,
    "expiresAt": "2026-03-31T12:00:00Z"
  },
  {
    "id": "guid",
    "bankName": "Yapı Kredi",
    "bankCode": "YAPIKREDI",
    "loanAmount": 15000,
    "installmentCount": 12,
    "interestRate": 1.79,
    "monthlyPayment": 1340.25,
    "totalPayment": 16083.00,
    "expiresAt": "2026-03-31T12:00:00Z"
  }
]
```

## 10. Komisyon Ödemesi (Auth Gerekli)

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/pay-commission \
  -H "Authorization: Bearer {token}"
```

## 11. Banka Teklifi Seçme (Auth Gerekli)

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/select-offer/{offer_id} \
  -H "Authorization: Bearer {token}"
```

**Beklenen Response:**
```json
{
  "message": "Bank offer selected",
  "redirectUrl": "https://api.garanti.example.com/credit-application?transactionId={transaction_id}"
}
```

## 12. İşlemi İptal Etme (Auth Gerekli)

```bash
curl -X POST http://localhost:5000/api/transaction/{transaction_id}/cancel \
  -H "Authorization: Bearer {token}"
```

## 13. Kredi Tamamlandı Bildirimi (Bankadan)

```bash
curl -X POST http://localhost:5000/api/marketplace/credit-completed \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": "{transaction_id}",
    "bankReference": "BANK-REF-12345"
  }'
```

---

## Tam İş Akışı Örneği

### Adım 1: Session Oluştur
```bash
RESPONSE=$(curl -s -X POST http://localhost:5000/api/marketplace/create-session \
  -H "Content-Type: application/json" \
  -d '{
    "mp_user": "demo_mp",
    "mp_password": "demo123",
    "order": {
      "success_url": "https://marketplace.com/success",
      "reject_url": "https://marketplace.com/reject",
      "order_id": "ORD-12345",
      "total_amount": 15000,
      "items": [{"category": "cep telefonu", "unit_price": 15000, "tax": 20}]
    }
  }')

TRANSACTION_ID=$(echo $RESPONSE | grep -o '"redirect_url":"[^"]*' | grep -o '[^/]*$')
echo "Transaction ID: $TRANSACTION_ID"
```

### Adım 2: Kullanıcı Kaydı
```bash
AUTH_RESPONSE=$(curl -s -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "phoneNumber": "05551234567",
    "nationalId": "12345678901"
  }')

TOKEN=$(echo $AUTH_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)
echo "Token: $TOKEN"
```

### Adım 3: Siparişi Onayla
```bash
curl -X POST http://localhost:5000/api/transaction/$TRANSACTION_ID/confirm-order
```

### Adım 4: Kullanıcıyı Bağla
```bash
curl -X POST http://localhost:5000/api/transaction/$TRANSACTION_ID/link-user \
  -H "Authorization: Bearer $TOKEN"
```

### Adım 5: Findeks Onayı
```bash
curl -X POST http://localhost:5000/api/transaction/$TRANSACTION_ID/request-findeks \
  -H "Authorization: Bearer $TOKEN"
```

### Adım 6: İşleme Devam Et
```bash
curl -X POST http://localhost:5000/api/transaction/$TRANSACTION_ID/continue \
  -H "Authorization: Bearer $TOKEN"
```

### Adım 7: Banka Tekliflerini Al
```bash
curl -X GET http://localhost:5000/api/transaction/$TRANSACTION_ID/bank-offers \
  -H "Authorization: Bearer $TOKEN"
```

### Adım 8: Komisyon Öde
```bash
curl -X POST http://localhost:5000/api/transaction/$TRANSACTION_ID/pay-commission \
  -H "Authorization: Bearer $TOKEN"
```

### Adım 9: Banka Seç (offer_id'yi önceki adımdan alın)
```bash
curl -X POST http://localhost:5000/api/transaction/$TRANSACTION_ID/select-offer/{offer_id} \
  -H "Authorization: Bearer $TOKEN"
```

---

## Notlar

- Tüm tarih/saat değerleri UTC formatındadır
- Token'lar 24 saat geçerlidir
- Transaction'lar 3 gün sonra otomatik expire olur
- Komisyon oranı %3'tür
- Banka teklifleri 24 saat geçerlidir
