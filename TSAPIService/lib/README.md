# TSAPI Native DLL Files

Bu klasöre Avaya TSAPI SDK'dan gelen native DLL dosyalarını koymanız gerekiyor:

## Gerekli Dosyalar

1. **csta32.dll** - CSTA (Computer Supported Telecommunications Applications) kütüphanesi
2. **attprv32.dll** - AT&T Private Data kütüphanesi

## Dosyaları Nereden Alabilirsiniz

### Avaya TSAPI SDK Kurulumu
1. Avaya TSAPI SDK'yı Windows sunucunuza kurun
2. Genellikle şu konumda bulunur:
   - `C:\Program Files\Avaya\TSAPI\`
   - `C:\Program Files (x86)\Avaya\TSAPI\`

### Dosyaları Kopyalama
```bash
# Windows Command Prompt
copy "C:\Program Files\Avaya\TSAPI\csta32.dll" lib\
copy "C:\Program Files\Avaya\TSAPI\attprv32.dll" lib\

# PowerShell
Copy-Item "C:\Program Files\Avaya\TSAPI\csta32.dll" -Destination lib\
Copy-Item "C:\Program Files\Avaya\TSAPI\attprv32.dll" -Destination lib\
```

## Önemli Notlar

⚠️ **Bu DLL dosyaları Avaya'nın telif hakkı altındadır ve TSAPI lisansı gerektirir.**

- Bu dosyalar repository'ye commit edilmemelidir
- Her deployment ortamında ayrı ayrı kopyalanmalıdır
- Sadece lisanslı Avaya müşterileri kullanabilir

## Alternatif Konumlar

Eğer TSAPI SDK farklı bir konuma kurulduysa, şu konumları kontrol edin:
- `%PROGRAMFILES%\Avaya\TSAPI\`
- `%PROGRAMFILES(X86)%\Avaya\TSAPI\`
- `C:\Avaya\TSAPI\`
- `C:\TSAPI\`

## Doğrulama

DLL dosyalarının doğru olduğunu kontrol etmek için:

```bash
# File properties kontrol edin
dir lib\*.dll

# Version bilgisini kontrol edin (PowerShell)
Get-ItemProperty lib\csta32.dll | Select-Object VersionInfo
```

## Sorun Giderme

### DLL Bulunamadı Hatası
```
System.DllNotFoundException: Unable to load DLL 'csta32.dll'
```

**Çözümler:**
1. DLL dosyalarının lib\ klasöründe olduğunu kontrol edin
2. DLL dosyalarının executable ile aynı klasörde olduğunu kontrol edin
3. Visual C++ Redistributable'ın kurulu olduğunu kontrol edin
4. 32-bit vs 64-bit uyumluluğunu kontrol edin

### Erişim İzni Hatası
```
System.UnauthorizedAccessException: Access to the path is denied
```

**Çözümler:**
1. Uygulamayı Administrator olarak çalıştırın
2. DLL dosyalarının izinlerini kontrol edin
3. Antivirus yazılımının engellemediğini kontrol edin

Bu dosyalar olmadan TSAPI entegrasyonu çalışmayacaktır!