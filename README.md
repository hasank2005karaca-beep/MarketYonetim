# KARAKAŞ MARKET - POS Yazılımı

## 📋 Özellikler

### Satış İşlemleri
- ✅ Barkod ile hızlı ürün ekleme
- ✅ Ürün arama (isim, kod, barkod)
- ✅ Sepet yönetimi
- ✅ Nakit, kredi kartı, karışık ödeme
- ✅ Veresiye satış (müşteri seçimli)
- ✅ Para üstü hesaplama
- ✅ Hızlı tuşlar (F12: Ödeme, DEL: Sil, F5: Müşteri Seç)

### Ürün Yönetimi
- ✅ Ürün ekleme/düzenleme/silme
- ✅ Barkod tanımlama
- ✅ Alış/satış fiyatı belirleme
- ✅ KDV oranı seçimi
- ✅ Kar marjı hesaplama
- ✅ Stok girişi/çıkışı

### Müşteri Yönetimi
- ✅ Müşteri ekleme/düzenleme
- ✅ Müşteri arama
- ✅ Veresiye takibi

### Raporlar
- ✅ Günlük satış raporu
- ✅ En çok satan ürünler
- ✅ Stok durumu
- ✅ Veresiye listesi
- ✅ Excel'e aktarma (CSV)

---

## 🛠️ Kurulum

### Gereksinimler
- Windows 10/11
- .NET 6.0 Runtime veya .NET Framework 4.8
- SQL Server (Express veya üstü)
- Nebim veritabanı (market yedek.BCK)

### Adım 1: Veritabanını Geri Yükle

1. SQL Server Management Studio'yu aç
2. Databases > Restore Database
3. Device > ... > Add > `market yedek.BCK` dosyasını seç
4. Veritabanı adı olarak `MARKET` yaz
5. OK'a tıkla

### Adım 2: Projeyi Derle

#### Visual Studio ile:
1. `MarketYonetim.csproj` dosyasını aç
2. Build > Build Solution (Ctrl+Shift+B)
3. `bin\Debug\` klasöründeki EXE'yi çalıştır

#### Komut satırı ile (.NET 6):
```bash
dotnet build
dotnet run
```

#### .NET Framework 4.8 için:
- `MarketYonetim_Framework48.csproj` dosyasını kullan

### Adım 3: Bağlantı Ayarları

İlk çalıştırmada veritabanı bağlantısı yapılandırma ekranı açılır:

- **Sunucu Adı:** SQL Server sunucu adı (örn: `.\SQLEXPRESS`, `localhost`, `192.168.1.100`)
- **Veritabanı Adı:** `MARKET` (veya yüklediğin isim)
- **Windows Authentication:** Yerel kullanıcı ile bağlanmak için işaretle
- **Kullanıcı/Şifre:** SQL Authentication için

---

## ⌨️ Kısayol Tuşları

| Tuş | İşlem |
|-----|-------|
| Enter | Barkod ile ürün ekle |
| F12 | Ödeme al |
| DEL | Seçili ürünü sil |
| F5 | Müşteri seç |
| ESC | Barkod alanına dön |

---

## 📁 Dosya Yapısı

```
MarketYonetim/
├── Program.cs              # Ana giriş noktası
├── FormSatis.cs           # Ana satış ekranı
├── FormOdeme.cs           # Ödeme alma formu
├── FormUrunYonetimi.cs    # Ürün listesi ve yönetimi
├── FormUrunDetay.cs       # Ürün ekleme/düzenleme
├── FormMusteriSec.cs      # Müşteri seçimi ve ekleme
├── FormStokGirisi.cs      # Stok giriş/çıkış
├── FormRaporlar.cs        # Raporlar
├── Ayarlar.cs             # Veritabanı ayarları
├── MarketYonetim.csproj   # .NET 6 proje dosyası
└── MarketYonetim_Framework48.csproj  # .NET Framework 4.8
```

---

## 🔧 Veritabanı Tabloları (Nebim Uyumlu)

Yazılım şu tablolarla çalışır:
- `tbStok` - Ürünler
- `tbStokBarkodu` - Barkodlar
- `tbStokFiyati` - Fiyatlar
- `tbStokFisiMaster/Detayi` - Stok hareketleri
- `tbMusteri` - Müşteriler
- `tbAlisVeris` - Satışlar
- `tbAlisverisSiparis` - Satış detayları
- `tbOdeme` - Ödemeler
- `tbKdv` - KDV oranları

---

## ❓ Sık Sorulan Sorular

**S: Barkod okuyucu çalışmıyor?**
C: Barkod okuyucu klavye gibi davranmalı. Ayarlardan "Keyboard Mode" aktif olmalı.

**S: Veritabanına bağlanamıyorum?**
C: SQL Server Browser servisi çalışıyor olmalı. Firewall'da 1433 portu açık olmalı.

**S: Ürün bulunamadı hatası?**
C: Ürün kodu veya barkod veritabanında kayıtlı değil. Önce ürünü ekleyin.

---

## 📞 Destek

Hasan Kuru tarafından geliştirilmiştir.

---

## 📜 Lisans

Bu yazılım özel kullanım içindir.
