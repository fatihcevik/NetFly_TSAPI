# TSAPI Service - .NET 9 WebSocket ve REST API

Bu proje, Avaya TSAPI SDK ile entegre olan .NET 9 tabanlı bir WebSocket ve REST API servisidir. Gerçek zamanlı agent durumu izleme, çağrı yönetimi ve event handling sağlar.

## 🏗️ Mimari

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   React Client  │◄──►│  .NET 9 Service  │◄──►│   Avaya AES     │
│   (WebSocket)   │    │  (TSAPI SDK)     │    │   TSAPI Server  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## 🔧 Özellikler

### TSAPI Entegrasyonu
- **Native DLL Import**: `csta32.dll` ve `attprv32.dll` kullanımı
- **Agent İşlemleri**: Login, Logout, State değiştirme
- **Call İşlemleri**: Arama, Cevaplama, Transfer, Hold/Retrieve
- **Device Monitoring**: Gerçek zamanlı cihaz izleme
- **Event Handling**: Tüm TSAPI eventlerinin yakalanması

### WebSocket Hub
- **Gerçek Zamanlı İletişim**: SignalR ile client-server iletişimi
- **Event Broadcasting**: Tüm clientlara event yayını
- **Group Management**: Agent bazlı gruplandırma
- **Connection Management**: Otomatik bağlantı yönetimi

### REST API
- **Agent Controller**: Agent CRUD işlemleri
- **Call Controller**: Çağrı yönetimi işlemleri
- **Event Controller**: Event sorgulama işlemleri
- **TSAPI Controller**: Sistem yönetimi işlemleri

### Background Services
- **TSAPI Background Service**: Sürekli TSAPI bağlantısı yönetimi
- **Event Processing**: Asenkron event işleme
- **Auto Reconnection**: Otomatik yeniden bağlanma

## 📋 Gereksinimler

### Sistem Gereksinimleri
- **Windows Server 2019+** (TSAPI SDK için)
- **.NET 9 Runtime**
- **Avaya TSAPI SDK** (csta32.dll, attprv32.dll)
- **Avaya AES Server** erişimi

### Avaya Gereksinimleri
- **TSAPI License** (Avaya'dan)
- **AES Server** yapılandırması
- **TSAPI Security Database** izinleri
- **Agent Device** tanımları

## 🚀 Kurulum

### 1. TSAPI SDK Kurulumu
```bash
# Avaya TSAPI SDK'yı Windows sunucuya kurun
# DLL dosyalarını TSAPIService/lib/ klasörüne kopyalayın
mkdir TSAPIService/lib
copy "C:\Program Files\Avaya\TSAPI\csta32.dll" TSAPIService/lib/
copy "C:\Program Files\Avaya\TSAPI\attprv32.dll" TSAPIService/lib/
```

### 2. Proje Derleme
```bash
cd TSAPIService
dotnet restore
dotnet build --configuration Release
```

### 3. Yapılandırma
```json
// appsettings.json
{
  "TSAPI": {
    "ServerName": "your-aes-server.company.com",
    "LoginId": "tsapi_user",
    "Password": "tsapi_password",
    "ApplicationName": "TSAPIService",
    "MonitorDevices": [
      "AGENT_001",
      "AGENT_002",
      "AGENT_003"
    ]
  }
}
```

### 4. Windows Service Kurulumu
```bash
# Service olarak kurulum
sc create "TSAPI Service" binPath="C:\path\to\TSAPIService.exe"
sc config "TSAPI Service" start=auto
sc start "TSAPI Service"
```

## 🔌 API Kullanımı

### REST API Endpoints

#### Agent İşlemleri
```bash
# Tüm agentları getir
GET /api/agent

# Belirli agent bilgisi
GET /api/agent/{agentId}

# Agent giriş
POST /api/agent/{agentId}/login
{
  "password": "agent_password"
}

# Agent çıkış
POST /api/agent/{agentId}/logout

# Agent durumu değiştir
PUT /api/agent/{agentId}/state
{
  "state": "Available"
}

# İstatistikler
GET /api/agent/stats
```

#### Call İşlemleri
```bash
# Arama yap
POST /api/call/make
{
  "agentId": "AGENT_001",
  "destination": "1234567890"
}

# Çağrı cevapla
POST /api/call/{callId}/answer

# Çağrı kapat
POST /api/call/{callId}/hangup

# Çağrı beklet
POST /api/call/{callId}/hold

# Çağrı aktarım
POST /api/call/{callId}/transfer
{
  "destination": "AGENT_002"
}
```

### WebSocket Hub Kullanımı

#### JavaScript Client
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://your-server/tsapihub")
    .build();

// Bağlantı kur
await connection.start();

// Agent eventlerini dinle
connection.on("AgentStateChanged", (agentId, newState) => {
    console.log(`Agent ${agentId} durumu: ${newState}`);
});

// Agent giriş yap
const result = await connection.invoke("LoginAgent", "AGENT_001", "password");

// Agent durumu değiştir
await connection.invoke("SetAgentState", "AGENT_001", "Available");
```

#### C# Client
```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://your-server/tsapihub")
    .Build();

// Event handler
connection.On<string, string>("AgentStateChanged", (agentId, newState) =>
{
    Console.WriteLine($"Agent {agentId} durumu: {newState}");
});

// Bağlan
await connection.StartAsync();

// Agent işlemleri
var result = await connection.InvokeAsync<bool>("LoginAgent", "AGENT_001", "password");
```

## 📊 Event Türleri

### Agent Events
- **AgentLoggedOn**: Agent giriş yaptı
- **AgentLoggedOff**: Agent çıkış yaptı
- **AgentStateChanged**: Agent durumu değişti
- **AgentWorkMode**: Agent çalışma modu değişti

### Call Events
- **CallDelivered**: Çağrı geldi
- **CallEstablished**: Çağrı kuruldu
- **CallCleared**: Çağrı sonlandı
- **CallHeld**: Çağrı beklemeye alındı
- **CallTransferred**: Çağrı aktarıldı

### System Events
- **SystemEvent**: Sistem olayları
- **ConnectionLost**: Bağlantı kesildi
- **ConnectionRestored**: Bağlantı yeniden kuruldu

## 🔧 Geliştirme

### Debug Modunda Çalıştırma
```bash
cd TSAPIService
dotnet run --environment Development
```

### Test Etme
```bash
# Health check
curl https://localhost:5001/api/tsapi/health

# Agent listesi
curl https://localhost:5001/api/agent

# WebSocket test (browser console)
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/tsapihub")
    .build();
```

## 🛠️ Sorun Giderme

### Yaygın Hatalar

#### TSAPI Bağlantı Hatası
```
Error: acsOpenStream failed with code -1
```
**Çözüm**: AES server adresini ve kimlik bilgilerini kontrol edin

#### DLL Bulunamadı
```
Error: Unable to load DLL 'csta32.dll'
```
**Çözüm**: DLL dosyalarının lib/ klasöründe olduğundan emin olun

#### İzin Hatası
```
Error: TSERVER_DEVICE_NOT_SUPPORTED
```
**Çözüm**: TSAPI Security Database'de device izinlerini kontrol edin

### Log Dosyaları
```bash
# Log dosyalarını kontrol edin
tail -f logs/tsapi-service-20241213.txt
```

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Avaya TSAPI SDK ayrı lisans gerektirir.

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📞 Destek

- **GitHub Issues**: Bug raporları ve özellik istekleri
- **Email**: support@yourcompany.com
- **Documentation**: [Wiki sayfası](https://github.com/yourrepo/wiki)