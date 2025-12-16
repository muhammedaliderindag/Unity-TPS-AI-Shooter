# 🎮 Unity TPS AI Shooter

**Unity TPS AI Shooter**, Unity 3D oyun motoru kullanılarak geliştirilmiş, temel mekaniklere odaklanan bir Üçüncü Şahıs Nişancı (Third-Person Shooter) oyun projesidir.

Bu proje, oyun geliştirmeye yeni başlayanlar veya TPS mekanikleri (karakter kontrolü, silah sistemleri ve özellikle düşman yapay zekası) üzerine çalışanlar için temiz bir temel ve öğrenme kaynağı oluşturmayı amaçlamaktadır.

## ✨ Özellikler

Proje şu temel oyun mekaniklerini içermektedir:

* 🕹️ **TPS Karakter Kontrolü:** Standart üçüncü şahıs kamera açısı, karakter hareketi ve animasyon entegrasyonları.
* 🔫 **Silah ve Çatışma Sistemi:** Nişan alma, ateş etme ve mermi fiziği/etkileşimi temelleri.
* 🤖 **Düşman Yapay Zekası (AI):** Oyuncuyu algılayan, takip eden ve temel saldırı davranışları sergileyen yapay zeka (Muhtemelen NavMesh tabanlı).
* 🛠️ **Genişletilebilir Altyapı:** Kendi seviyelerinizi, düşman türlerinizi veya silahlarınızı eklemek için uygun modüler yapı.

## 🛠️ Teknolojiler ve Araçlar

* **Oyun Motoru:** Unity 3D
* **Programlama Dili:** C#
* **AI (Yapay Zeka):** Unity NavMesh ve script tabanlı davranışlar.

## 🚀 Kurulum ve Çalıştırma

Bu projeyi kendi bilgisayarınızda açmak ve incelemek için aşağıdaki adımları izleyin:

### Gereksinimler

* [Unity Hub](https://unity.com/download) ve uyumlu bir Unity Editör sürümü.

### Adım Adım Kurulum

1.  **Projeyi Klonlayın:**
    Terminalinizi veya komut istemcinizi açın ve aşağıdaki komutu çalıştırın (veya sağ üstteki "Code" butonundan ZIP olarak indirin):
    ```bash
    git clone [https://github.com/muhammedaliderindag/Unity-TPS-AI-Shooter.git](https://github.com/muhammedaliderindag/Unity-TPS-AI-Shooter.git)
    ```

2.  **Unity Hub'ı Açın:**
    Unity Hub'ı başlatın ve "Projects" sekmesine gidin.

3.  **Projeyi Ekleyin:**
    Sağ üstteki "Add" (veya "Open") butonuna tıklayın ve klonladığınız/indirdiğiniz `Unity-TPS-AI-Shooter` klasörünü seçin.

4.  **Projeyi Başlatın:**
    Unity, proje dosyalarını içe aktaracaktır (bu işlem ilk seferde birkaç dakika sürebilir). Proje açıldığında, `Assets/Scenes` klasörü altındaki ana sahneyi (genellikle "SampleScene" veya "MainScene") bulun ve çift tıklayarak açın.

5.  **Oyunu Oynayın:**
    Editörün üst kısmındaki ▶️ **Play** butonuna basarak oyunu test etmeye başlayın.

## 🎮 Kontroller

*Genel TPS kontrolleri varsayılmıştır, projeye göre farklılık gösterebilir.*

* **W, A, S, D:** Karakter Hareketi
* **Mouse:** Kamera Açısı / Nişan Alma
* **Sol Mouse Tık:** Ateş Etme
* **Sağ Mouse Tık:** Nişan Alma (Aim)

**Geliştirici:** [Muhammed Ali Derindağ](https://github.com/muhammedaliderindag)
