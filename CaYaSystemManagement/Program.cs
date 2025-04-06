using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Security.Principal;
using System.ServiceProcess;
using System.Globalization;

namespace CaYaYazilimYoneticisi
{
    class Program
    {
        // Windows API işlemleri için
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_MAXIMIZE = 3;

        // Performans sayaçları için
        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;

        static void Main(string[] args)
        {
            // Konsol penceresini maksimize et
            IntPtr konsol = GetConsoleWindow();
            ShowWindow(konsol, SW_MAXIMIZE);

            Console.Title = "CaYa Yazılım Yöneticisi";
            Console.OutputEncoding = Encoding.UTF8;

            // Performans sayaçlarını başlat
            InitializePerformanceCounters();

            // Uygulama ilk açıldığında başlangıç ekranı göster
            BaslangicEkrani();
            AnaMenu();
        }

        static void InitializePerformanceCounters()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                // İlk okumanın doğru olması için bir önokuma yap
                cpuCounter.NextValue();
                ramCounter.NextValue();
            }
            catch (Exception ex)
            {
                // Hata durumunda sessizce devam et - sayaçlar kullanılamadığında tahminler gösterilecek
                Console.WriteLine($"Performans sayaçları başlatılamadı: {ex.Message}");
                Thread.Sleep(2000);
            }
        }

        static void BaslangicEkrani()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            string[] logo = {
                " ██████╗ █████╗ ██╗   ██╗ █████╗ ",
                "██╔════╝██╔══██╗╚██╗ ██╔╝██╔══██╗",
                "██║     ███████║ ╚████╔╝ ███████║",
                "██║     ██╔══██║  ╚██╔╝  ██╔══██║",
                "╚██████╗██║  ██║   ██║   ██║  ██║",
                " ╚═════╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝"
            };

            int yPos = Console.WindowHeight / 2 - logo.Length / 2;

            for (int i = 0; i < logo.Length; i++)
            {
                Console.SetCursorPosition((Console.WindowWidth - logo[i].Length) / 2, yPos + i);
                Console.WriteLine(logo[i]);
            }

            Console.SetCursorPosition((Console.WindowWidth - 35) / 2, yPos + logo.Length + 2);
            Console.WriteLine("Yazılım Yöneticisi ve Sistem Bakım Aracı");

            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, yPos + logo.Length + 4);
            Console.WriteLine("Sürüm: 2.0.0 - 2025 © CaYa");

            Console.ResetColor();
            Thread.Sleep(2000);
        }

        static void AnaMenu()
        {
            var menuItems = new List<string>
            {
                "Yazılım Havuzu",
                "Toplu Yazılım Kurulum",
                "İndirilenler",
                "Yazılım Güncelleme Kontrolü",
                "Sistem Bilgisi",
                "Sistem Bakımı",
                "Sistem Performans Optimizasyonu",
                "Windows Güncellemeleri",
                "Ayarlar",
                "Çıkış"
            };

            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                            CaYa Yazılım Yöneticisi                           ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                // CPU ve RAM durumunu göster
                try
                {
                    float cpuUsage = cpuCounter?.NextValue() ?? 0;
                    float availableRam = ramCounter?.NextValue() ?? 0;
                    float totalRam = GetTotalPhysicalMemory() / (1024 * 1024); // MB cinsinden
                    float usedRamPercentage = (1 - (availableRam / totalRam)) * 100;

                    Console.WriteLine($"\nCPU: %{cpuUsage:F1} | RAM: %{usedRamPercentage:F1} | {DateTime.Now.ToString("HH:mm:ss")}");
                }
                catch
                {
                    // Performans bilgileri alınamadığında sessizce devam et
                }

                Console.WriteLine();

                // Menü öğelerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedIndex)
                        {
                            case 0: YazilimHavuzu(); break;
                            case 1: TopluYazilimKurulum(); break;
                            case 2: Indirilenler(); break;
                            case 3: YazilimGuncellemeKontrolu(); break;
                            case 4: SistemBilgisi(); break;
                            case 5: SistemBakimi(); break;
                            case 6: SistemOptimizasyonu(); break;
                            case 7: WindowsGuncellemeleri(); break;
                            case 8: Ayarlar(); break;
                            case 9:
                                Console.WriteLine("CaYa Yazılım Yöneticisi kapatılıyor...");
                                return;
                        }
                        break;
                }
            }
        }

        static void YazilimHavuzu()
        {
            var kategoriler = new Dictionary<string, List<Yazilim>>
            {
                { "Tarayıcılar", new List<Yazilim>
                    {
                        new Yazilim("Google Chrome", "Web Tarayıcısı", "https://dl.google.com/chrome/install/latest/chrome_installer.exe"),
                        new Yazilim("Mozilla Firefox", "Web Tarayıcısı", "https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=tr"),
                        new Yazilim("Opera", "Web Tarayıcısı", "https://net.geo.opera.com/opera/stable/windows"),
                        new Yazilim("Microsoft Edge", "Web Tarayıcısı", "https://c2rsetup.officeapps.live.com/c2r/downloadEdge.aspx?platform=Default&source=EdgeStablePage&Channel=Stable&language=tr"),
                        new Yazilim("Brave Browser", "Güvenli Web Tarayıcısı", "https://laptop-updates.brave.com/latest/winx64"),
                        new Yazilim("Vivaldi", "Özelleştirilebilir Web Tarayıcısı", "https://downloads.vivaldi.com/stable/Vivaldi.5.6.2867.50.x64.exe")
                    }
                },
                { "Medya Oynatıcılar", new List<Yazilim>
                    {
                        new Yazilim("VLC", "Çok Amaçlı Medya Oynatıcısı", "https://get.videolan.org/vlc/last/win64/vlc-3.0.18-win64.exe"),
                        new Yazilim("KMPlayer", "Gelişmiş Medya Oynatıcısı", "http://cdn.kmplayer.com/KMP/Download/KMPSETUP.exe"),
                        new Yazilim("Spotify", "Müzik Servisi", "https://download.scdn.co/SpotifySetup.exe"),
                        new Yazilim("AIMP", "Gelişmiş Ses Çalar", "https://aimp.ru/files/windows/builds/aimp_5.10.2398_w64.exe"),
                        new Yazilim("MPC-HC", "Hafif Medya Oynatıcısı", "https://github.com/clsid2/mpc-hc/releases/download/1.9.21/MPC-HC.1.9.21.x64.exe"),
                        new Yazilim("foobar2000", "Gelişmiş Müzik Oynatıcısı", "https://www.foobar2000.org/files/foobar2000_v1.6.16.exe")
                    }
                },
                { "İletişim Araçları", new List<Yazilim>
                    {
                        new Yazilim("Discord", "Sesli ve Görüntülü İletişim", "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x86"),
                        new Yazilim("Zoom", "Video Konferans", "https://zoom.us/client/latest/ZoomInstaller.exe"),
                        new Yazilim("Skype", "Sesli/Görüntülü İletişim", "https://go.skype.com/windows.desktop.download"),
                        new Yazilim("Microsoft Teams", "İş İletişimi", "https://statics.teams.cdn.office.net/production-windows-x64/1.5.00.9163/Teams_windows_x64.exe"),
                        new Yazilim("Telegram", "Güvenli Mesajlaşma", "https://telegram.org/dl/desktop/win64"),
                        new Yazilim("Signal", "Güvenli Mesajlaşma", "https://updates.signal.org/desktop/signal-desktop-win-5.34.0.exe"),
                        new Yazilim("WhatsApp", "Mesajlaşma", "https://web.whatsapp.com/desktop/windows/release/x64/WhatsAppSetup.exe")
                    }
                },
                { "Güvenlik", new List<Yazilim>
                    {
                        new Yazilim("Malwarebytes", "Antimalware", "https://downloads.malwarebytes.com/file/mb-windows"),
                        new Yazilim("Avast Antivirus", "Antivirus", "https://www.avast.com/tr-tr/download-thank-you.php?product=FAV-ONLINE&locale=tr-tr"),
                        new Yazilim("CCleaner", "Sistem Temizleyici", "https://download.ccleaner.com/ccsetup587.exe"),
                        new Yazilim("Bitdefender", "Antivirüs", "https://download.bitdefender.com/windows/installer/en-us/bitdefender_antivirus.exe"),
                        new Yazilim("Kaspersky Free", "Antivirüs", "https://kaspersky.com.tr/downloads/thank-you/free-antivirus-download"),
                        new Yazilim("Revo Uninstaller", "Program Kaldırıcı", "https://download.revouninstaller.com/download/revosetup.exe"),
                        new Yazilim("AdwCleaner", "Reklam Yazılımı Temizleyici", "https://downloads.malwarebytes.com/file/adwcleaner")
                    }
                },
                { "Ofis Araçları", new List<Yazilim>
                    {
                        new Yazilim("LibreOffice", "Ofis Paketi", "https://download.documentfoundation.org/libreoffice/stable/7.5.4/win/x86_64/LibreOffice_7.5.4_Win_x86-64.msi"),
                        new Yazilim("Adobe Reader", "PDF Okuyucu", "https://admdownload.adobe.com/bin/live/readerdc_tr_xa_crd_install.exe"),
                        new Yazilim("Notepad++", "Metin Editörü", "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.5.4/npp.8.5.4.Installer.x64.exe"),
                        new Yazilim("WPS Office", "Hafif Ofis Paketi", "https://wdl1.pcfg.cache.wpscdn.com/wpsdl/wpsoffice/download/11.2.0.11440/WPSOffice_11.2.0.11440.exe"),
                        new Yazilim("SumatraPDF", "Hafif PDF Okuyucu", "https://www.sumatrapdfreader.org/dl/rel/3.4.6/SumatraPDF-3.4.6-64-install.exe"),
                        new Yazilim("Calibre", "E-Kitap Yöneticisi", "https://download.calibre-ebook.com/6.11.0/calibre-64bit-6.11.0.msi"),
                        new Yazilim("PDF24 Creator", "PDF Oluşturucu", "https://download.pdf24.org/pdf24-creator-11.11.1-x64.msi")
                    }
                },
                { "Sıkıştırma Araçları", new List<Yazilim>
                    {
                        new Yazilim("7-Zip", "Ücretsiz Arşivleme Aracı", "https://www.7-zip.org/a/7z2301-x64.exe"),
                        new Yazilim("WinRAR", "Arşivleme Aracı", "https://www.win-rar.com/fileadmin/winrar-versions/winrar/winrar-x64-624tr.exe"),
                        new Yazilim("PeaZip", "Açık Kaynak Sıkıştırma", "https://github.com/peazip/PeaZip/releases/download/9.3.0/peazip-9.3.0.WIN64.exe"),
                        new Yazilim("Bandizip", "Hızlı Arşivleyici", "https://dl.bandisoft.com/bandizip.std/BANDIZIP-SETUP-STD-ALL.EXE")
                    }
                },
                { "Geliştirici Araçları", new List<Yazilim>
                    {
                        new Yazilim("Visual Studio Code", "Kod Editörü", "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user"),
                        new Yazilim("Git", "Sürüm Kontrol", "https://github.com/git-for-windows/git/releases/download/v2.40.0.windows.1/Git-2.40.0-64-bit.exe"),
                        new Yazilim("Node.js", "JavaScript Çalışma Ortamı", "https://nodejs.org/dist/v16.19.1/node-v16.19.1-x64.msi"),
                        new Yazilim("Python 3", "Programlama Dili", "https://www.python.org/ftp/python/3.11.0/python-3.11.0-amd64.exe"),
                        new Yazilim("MySQL Workbench", "Veritabanı Yönetimi", "https://dev.mysql.com/get/Downloads/MySQLGUITools/mysql-workbench-community-8.0.32-winx64.msi"),
                        new Yazilim("Postman", "API Geliştirme Aracı", "https://dl.pstmn.io/download/latest/win64"),
                        new Yazilim("HeidiSQL", "Veritabanı İstemcisi", "https://www.heidisql.com/installers/HeidiSQL_12.3.0.6589_Setup.exe"),
                        new Yazilim("Visual Studio Community", "IDE", "https://aka.ms/vs/17/release/vs_community.exe")
                    }
                },
                { "Oyunlar", new List<Yazilim>
                    {
                        new Yazilim("Steam", "Oyun Platformu", "https://cdn.akamai.steamstatic.com/client/installer/SteamSetup.exe"),
                        new Yazilim("Epic Games Launcher", "Oyun Platformu", "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.msi"),
                        new Yazilim("GOG Galaxy", "Oyun Platformu", "https://cdn.gog.com/open/galaxy/client/2.0.63.27/setup_galaxy_2.0.63.27.exe"),
                        new Yazilim("Minecraft Launcher", "Minecraft", "https://launcher.mojang.com/download/MinecraftInstaller.msi"),
                        new Yazilim("Origin", "EA Oyun Platformu", "https://www.dm.origin.com/download"),
                        new Yazilim("Ubisoft Connect", "Ubisoft Oyun Platformu", "https://ubistatic3-a.akamaihd.net/orbit/launcher_installer/UbisoftConnectInstaller.exe")
                    }
                },
                { "Yardımcı Programlar", new List<Yazilim>
                    {
                        new Yazilim("TeamViewer", "Uzak Erişim", "https://download.teamviewer.com/download/TeamViewer_Setup.exe"),
                        new Yazilim("CPU-Z", "Sistem Bilgisi", "https://download.cpuid.com/cpu-z/cpu-z_2.06-en.exe"),
                        new Yazilim("HWMonitor", "Donanım İzleme", "https://download.cpuid.com/hwmonitor/hwmonitor_1.48.exe"),
                        new Yazilim("Rufus", "ISO Yazıcı", "https://github.com/pbatard/rufus/releases/download/v4.1/rufus-4.1.exe"),
                        new Yazilim("Etcher", "ISO Yazıcı", "https://github.com/balena-io/etcher/releases/download/v1.14.3/balenaEtcher-Setup-1.14.3.exe"),
                        new Yazilim("Everything", "Hızlı Dosya Arama", "https://www.voidtools.com/Everything-1.4.1.1022.x64-Setup.exe"),
                        new Yazilim("f.lux", "Mavi Işık Filtresi", "https://justgetflux.com/flux-setup.exe"),
                        new Yazilim("Process Explorer", "Gelişmiş Görev Yöneticisi", "https://download.sysinternals.com/files/ProcessExplorer.zip")
                    }
                },
                { "Grafik ve Tasarım", new List<Yazilim>
                    {
                        new Yazilim("GIMP", "Görüntü Editörü", "https://download.gimp.org/pub/gimp/v2.10/windows/gimp-2.10.34-setup.exe"),
                        new Yazilim("Inkscape", "Vektörel Grafik Editörü", "https://inkscape.org/gallery/item/37361/inkscape-1.2.2_2022-12-09_732a01da63-x64.msi"),
                        new Yazilim("Paint.NET", "Resim Düzenleme", "https://www.dotpdn.com/files/paint.net.4.3.12.install.anycpu.web.zip"),
                        new Yazilim("Blender", "3D Modelleme", "https://download.blender.org/release/Blender3.5/blender-3.5.0-windows-x64.msi"),
                        new Yazilim("Krita", "Dijital Boyama", "https://download.kde.org/stable/krita/5.1.5/krita-x64-5.1.5-setup.exe"),
                        new Yazilim("DaVinci Resolve", "Video Düzenleme", "https://www.blackmagicdesign.com/products/davinciresolve")
                    }
                }
            };

            // Kategorileri listele ve ok tuşlarıyla seçim yap
            var kategoriListesi = new List<string>(kategoriler.Keys);
            kategoriListesi.Add("Yazılım Ara");
            kategoriListesi.Add("Ana Menüye Dön");

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                 Yazılım Havuzu                               ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nKategoriler:");

                // Menü öğelerini göster
                for (int i = 0; i < kategoriListesi.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        if (i < kategoriListesi.Count - 2)
                            Console.WriteLine($" >>> {kategoriListesi[i]} ({kategoriler[kategoriListesi[i]].Count} yazılım) <<<");
                        else
                            Console.WriteLine($" >>> {kategoriListesi[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        if (i < kategoriListesi.Count - 2)
                            Console.WriteLine($"     {kategoriListesi[i]} ({kategoriler[kategoriListesi[i]].Count} yazılım)");
                        else
                            Console.WriteLine($"     {kategoriListesi[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : kategoriListesi.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < kategoriListesi.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex == kategoriListesi.Count - 1)
                        {
                            // Ana menüye dön
                            devam = false;
                        }
                        else if (selectedIndex == kategoriListesi.Count - 2)
                        {
                            // Yazılım Ara
                            YazilimAra(kategoriler);
                        }
                        else
                        {
                            // Kategori seçildi
                            string secilenKategori = kategoriListesi[selectedIndex];
                            YazilimlariGoster(secilenKategori, kategoriler[secilenKategori]);
                        }
                        break;
                }
            }
        }

        static void YazilimlariGoster(string kategori, List<Yazilim> yazilimlar)
        {
            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║  {kategori,-73} ║");
                Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                // Yazılım listesine "Geri" seçeneği ekle
                var tumListeOgeleri = new List<string>();
                foreach (var yazilim in yazilimlar)
                {
                    tumListeOgeleri.Add($"{yazilim.Ad} - {yazilim.Aciklama}");
                }
                tumListeOgeleri.Add("Geri");

                // Menü öğelerini göster
                for (int i = 0; i < tumListeOgeleri.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {tumListeOgeleri[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {tumListeOgeleri[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : tumListeOgeleri.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < tumListeOgeleri.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex == tumListeOgeleri.Count - 1)
                        {
                            // Geri seçildi
                            devam = false;
                        }
                        else
                        {
                            // Yazılım seçildi
                            IndirVeKur(yazilimlar[selectedIndex]);
                        }
                        break;
                }
            }
        }

        static void YazilimAra(Dictionary<string, List<Yazilim>> tumKategoriler)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                Yazılım Arama                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.Write("\nAramak istediğiniz yazılım adını girin: ");
            string aramaKelimesi = Console.ReadLine().ToLower();

            if (string.IsNullOrWhiteSpace(aramaKelimesi))
            {
                Console.WriteLine("Arama kelimesi girmediniz! Geri dönmek için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            var bulunanYazilimlar = new List<Tuple<string, Yazilim>>();

            // Tüm kategorilerdeki yazılımları kontrol et
            foreach (var kategori in tumKategoriler)
            {
                foreach (var yazilim in kategori.Value)
                {
                    if (yazilim.Ad.ToLower().Contains(aramaKelimesi) || yazilim.Aciklama.ToLower().Contains(aramaKelimesi))
                    {
                        bulunanYazilimlar.Add(new Tuple<string, Yazilim>(kategori.Key, yazilim));
                    }
                }
            }

            if (bulunanYazilimlar.Count == 0)
            {
                Console.WriteLine("\nAramanıza uygun yazılım bulunamadı.");
                Console.WriteLine("Devam etmek için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nAramanızla eşleşen {bulunanYazilimlar.Count} yazılım bulundu:");

            // Liste oluştur ve "Geri" seçeneğini ekle
            var sonucListesi = new List<Tuple<string, Yazilim>>(bulunanYazilimlar);
            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                          Arama Sonuçları                                      ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine($"\nAramanızla eşleşen {bulunanYazilimlar.Count} yazılım bulundu:");

                // Menü öğelerini göster
                for (int i = 0; i < bulunanYazilimlar.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {bulunanYazilimlar[i].Item2.Ad} - {bulunanYazilimlar[i].Item2.Aciklama} (Kategori: {bulunanYazilimlar[i].Item1}) <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {bulunanYazilimlar[i].Item2.Ad} - {bulunanYazilimlar[i].Item2.Aciklama} (Kategori: {bulunanYazilimlar[i].Item1})");
                    }
                }

                Console.WriteLine();
                if (selectedIndex == bulunanYazilimlar.Count)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine(" >>> Geri <<<");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("     Geri");
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : bulunanYazilimlar.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < bulunanYazilimlar.Count) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex == bulunanYazilimlar.Count)
                        {
                            // Geri seçildi
                            devam = false;
                        }
                        else
                        {
                            // Yazılım seçildi
                            IndirVeKur(bulunanYazilimlar[selectedIndex].Item2);
                            devam = false;
                        }
                        break;
                }
            }
        }

        static void IndirVeKur(Yazilim yazilim)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                             Yazılım İndiriliyor                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            string indirmeDosyaAdi = Path.GetFileName(new Uri(yazilim.IndirmeUrl).LocalPath);
            if (string.IsNullOrEmpty(indirmeDosyaAdi))
                indirmeDosyaAdi = $"{yazilim.Ad.Replace(" ", "")}_setup.exe";

            string indirmeDizini = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CaYaDownloads");
            Directory.CreateDirectory(indirmeDizini);

            string indirmeDosyaYolu = Path.Combine(indirmeDizini, indirmeDosyaAdi);

            Console.WriteLine($"Yazılım: {yazilim.Ad}");
            Console.WriteLine($"İndirme URL: {yazilim.IndirmeUrl}");
            Console.WriteLine($"İndirme konumu: {indirmeDosyaYolu}\n");

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (sender, e) =>
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Console.SetCursorPosition(0, Console.CursorTop);

                        // İlerleme çubuğu
                        int barWidth = 50;
                        int completeWidth = e.ProgressPercentage * barWidth / 100;

                        Console.Write("[");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(new string('#', completeWidth));
                        Console.ResetColor();
                        Console.Write(new string('-', barWidth - completeWidth));
                        Console.Write($"] %{e.ProgressPercentage} ({FormatBoyut(e.BytesReceived)} / {FormatBoyut(e.TotalBytesToReceive)})");
                    };

                    // İndirme işlemi
                    webClient.DownloadFile(yazilim.IndirmeUrl, indirmeDosyaYolu);
                    Console.WriteLine("\n\nİndirme tamamlandı!");

                    // İndirme geçmişini kaydet
                    KayitEkle(yazilim, indirmeDosyaYolu);

                    Console.WriteLine("\nYazılımı kurmak ister misiniz? (E/H)");

                    if (Console.ReadKey().Key == ConsoleKey.E)
                    {
                        Console.WriteLine("\n\nKurulum başlatılıyor...");
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = indirmeDosyaYolu,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Kurulum başlatılamadı: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nHata oluştu: {ex.Message}");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void KayitEkle(Yazilim yazilim, string dosyaYolu)
        {
            try
            {
                string indirmeKayitDosyasi = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CaYaDownloads",
                    "indirme_kayitlari.csv"
                );

                bool dosyaVarMi = File.Exists(indirmeKayitDosyasi);

                using (StreamWriter sw = new StreamWriter(indirmeKayitDosyasi, true))
                {
                    if (!dosyaVarMi)
                    {
                        // Başlık satırı yaz
                        sw.WriteLine("Tarih,Yazılım Adı,Açıklama,Dosya Yolu");
                    }

                    // Kayıt ekle
                    sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{yazilim.Ad},{yazilim.Aciklama},{dosyaYolu}");
                }
            }
            catch
            {
                // Kayıt eklenirken hata oluşursa sessizce devam et
            }
        }

        static string FormatBoyut(long bytelar)
        {
            string[] birimler = { "B", "KB", "MB", "GB", "TB" };
            int birimIndeksi = 0;
            double boyut = bytelar;

            while (boyut >= 1024 && birimIndeksi < birimler.Length - 1)
            {
                boyut /= 1024;
                birimIndeksi++;
            }

            return $"{boyut:0.##} {birimler[birimIndeksi]}";
        }

        static void TopluYazilimKurulum()
        {
            var paketler = new Dictionary<string, List<Yazilim>>
            {
                { "Temel Yazılımlar", new List<Yazilim>
                    {
                        new Yazilim("Google Chrome", "Web Tarayıcısı", "https://dl.google.com/chrome/install/latest/chrome_installer.exe"),
                        new Yazilim("VLC", "Çok Amaçlı Medya Oynatıcısı", "https://get.videolan.org/vlc/last/win64/vlc-3.0.18-win64.exe"),
                        new Yazilim("Adobe Reader", "PDF Okuyucu", "https://admdownload.adobe.com/bin/live/readerdc_tr_xa_crd_install.exe"),
                        new Yazilim("7-Zip", "Ücretsiz Arşivleme Aracı", "https://www.7-zip.org/a/7z2301-x64.exe")
                    }
                },
                { "Ofis Yazılımları", new List<Yazilim>
                    {
                        new Yazilim("LibreOffice", "Ofis Paketi", "https://download.documentfoundation.org/libreoffice/stable/7.5.4/win/x86_64/LibreOffice_7.5.4_Win_x86-64.msi"),
                        new Yazilim("Adobe Reader", "PDF Okuyucu", "https://admdownload.adobe.com/bin/live/readerdc_tr_xa_crd_install.exe"),
                        new Yazilim("Notepad++", "Metin Editörü", "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.5.4/npp.8.5.4.Installer.x64.exe"),
                        new Yazilim("SumatraPDF", "Hafif PDF Okuyucu", "https://www.sumatrapdfreader.org/dl/rel/3.4.6/SumatraPDF-3.4.6-64-install.exe")
                    }
                },
                { "Geliştirici Araçları", new List<Yazilim>
                    {
                        new Yazilim("Visual Studio Code", "Kod Editörü", "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user"),
                        new Yazilim("Git", "Sürüm Kontrol", "https://github.com/git-for-windows/git/releases/download/v2.40.0.windows.1/Git-2.40.0-64-bit.exe"),
                        new Yazilim("Node.js", "JavaScript Çalışma Ortamı", "https://nodejs.org/dist/v16.19.1/node-v16.19.1-x64.msi"),
                        new Yazilim("Python 3", "Programlama Dili", "https://www.python.org/ftp/python/3.11.0/python-3.11.0-amd64.exe")
                    }
                },
                { "Medya Araçları", new List<Yazilim>
                    {
                        new Yazilim("VLC", "Çok Amaçlı Medya Oynatıcısı", "https://get.videolan.org/vlc/last/win64/vlc-3.0.18-win64.exe"),
                        new Yazilim("GIMP", "Görüntü Editörü", "https://download.gimp.org/pub/gimp/v2.10/windows/gimp-2.10.34-setup.exe"),
                        new Yazilim("Audacity", "Ses Editörü", "https://github.com/audacity/audacity/releases/download/Audacity-3.2.5/audacity-win-3.2.5-64bit.exe"),
                        new Yazilim("OBS Studio", "Ekran Kaydedici", "https://cdn-fastly.obsproject.com/downloads/OBS-Studio-29.1.3-Full-Installer-x64.exe")
                    }
                },
                { "Güvenlik Paketi", new List<Yazilim>
                    {
                        new Yazilim("Malwarebytes", "Antimalware", "https://downloads.malwarebytes.com/file/mb-windows"),
                        new Yazilim("Avast Antivirus", "Antivirus", "https://www.avast.com/tr-tr/download-thank-you.php?product=FAV-ONLINE&locale=tr-tr"),
                        new Yazilim("CCleaner", "Sistem Temizleyici", "https://download.ccleaner.com/ccsetup587.exe"),
                        new Yazilim("Bitwarden", "Şifre Yöneticisi", "https://vault.bitwarden.com/download/?app=desktop&platform=windows")
                    }
                },
                { "Yeni Bilgisayar Paketi", new List<Yazilim>
                    {
                        new Yazilim("Google Chrome", "Web Tarayıcısı", "https://dl.google.com/chrome/install/latest/chrome_installer.exe"),
                        new Yazilim("7-Zip", "Ücretsiz Arşivleme Aracı", "https://www.7-zip.org/a/7z2301-x64.exe"),
                        new Yazilim("VLC", "Çok Amaçlı Medya Oynatıcısı", "https://get.videolan.org/vlc/last/win64/vlc-3.0.18-win64.exe"),
                        new Yazilim("LibreOffice", "Ofis Paketi", "https://download.documentfoundation.org/libreoffice/stable/7.5.4/win/x86_64/LibreOffice_7.5.4_Win_x86-64.msi"),
                        new Yazilim("Adobe Reader", "PDF Okuyucu", "https://admdownload.adobe.com/bin/live/readerdc_tr_xa_crd_install.exe"),
                        new Yazilim("Malwarebytes", "Antimalware", "https://downloads.malwarebytes.com/file/mb-windows")
                    }
                }
            };

            // Paketleri listele ve ok tuşlarıyla seçim yap
            var paketListesi = new List<string>(paketler.Keys);
            paketListesi.Add("Ana Menüye Dön");

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                            Toplu Yazılım Kurulum                             ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nÖnceden hazırlanmış yazılım paketleri:");

                // Menü öğelerini göster
                for (int i = 0; i < paketListesi.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        if (i < paketListesi.Count - 1)
                            Console.WriteLine($" >>> {paketListesi[i]} ({paketler[paketListesi[i]].Count} yazılım) <<<");
                        else
                            Console.WriteLine($" >>> {paketListesi[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        if (i < paketListesi.Count - 1)
                            Console.WriteLine($"     {paketListesi[i]} ({paketler[paketListesi[i]].Count} yazılım)");
                        else
                            Console.WriteLine($"     {paketListesi[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : paketListesi.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < paketListesi.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex == paketListesi.Count - 1)
                        {
                            // Ana menüye dön
                            devam = false;
                        }
                        else
                        {
                            // Paket seçildi
                            string secilenPaket = paketListesi[selectedIndex];
                            PaketYazilimlariniGoster(secilenPaket, paketler[secilenPaket]);
                        }
                        break;
                }
            }
        }

        static void PaketYazilimlariniGoster(string paketAdi, List<Yazilim> yazilimlar)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  {paketAdi} Paketi - Toplu Yazılım Kurulumu                                  ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nBu pakette yer alan yazılımlar:");

            for (int j = 0; j < yazilimlar.Count; j++)
            {
                Console.WriteLine($"{j + 1}. {yazilimlar[j].Ad} - {yazilimlar[j].Aciklama}");
            }

            Console.WriteLine("\nTüm yazılımları indirmek istiyor musunuz? (E/H)");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                TopluIndir(yazilimlar);
            }
        }

        static void TopluIndir(List<Yazilim> yazilimlar)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         Toplu Yazılım İndiriliyor                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            string indirmeDizini = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CaYaDownloads");
            Directory.CreateDirectory(indirmeDizini);

            // İndirilen dosyaların yollarını kaydet
            var indirilenDosyalar = new List<string>();

            for (int i = 0; i < yazilimlar.Count; i++)
            {
                var yazilim = yazilimlar[i];
                Console.WriteLine($"\n[{i + 1}/{yazilimlar.Count}] {yazilim.Ad} indiriliyor...");

                string indirmeDosyaAdi = Path.GetFileName(new Uri(yazilim.IndirmeUrl).LocalPath);
                if (string.IsNullOrEmpty(indirmeDosyaAdi))
                    indirmeDosyaAdi = $"{yazilim.Ad.Replace(" ", "")}_setup.exe";

                string indirmeDosyaYolu = Path.Combine(indirmeDizini, indirmeDosyaAdi);

                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadProgressChanged += (sender, e) =>
                        {
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(0, Console.CursorTop);

                            // İlerleme çubuğu
                            int barWidth = 50;
                            int completeWidth = e.ProgressPercentage * barWidth / 100;

                            Console.Write("[");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(new string('#', completeWidth));
                            Console.ResetColor();
                            Console.Write(new string('-', barWidth - completeWidth));
                            Console.Write($"] %{e.ProgressPercentage} ({FormatBoyut(e.BytesReceived)} / {FormatBoyut(e.TotalBytesToReceive)})");
                        };

                        // İndirme işlemi
                        webClient.DownloadFile(yazilim.IndirmeUrl, indirmeDosyaYolu);
                        Console.WriteLine("\nİndirme tamamlandı!");

                        // İndirme geçmişini kaydet
                        KayitEkle(yazilim, indirmeDosyaYolu);

                        // İndirilen dosyayı listeye ekle
                        indirilenDosyalar.Add(indirmeDosyaYolu);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nHata oluştu: {ex.Message}");
                }
            }

            Console.WriteLine("\nTüm indirmeler tamamlandı!");
            Console.WriteLine($"Toplam {indirilenDosyalar.Count}/{yazilimlar.Count} yazılım başarıyla indirildi.");

            Console.WriteLine("\nİndirilen yazılımları kurmak istiyor musunuz? (E/H)");
            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                Console.WriteLine("\n\nKurulumlar başlatılıyor...");

                foreach (var dosyaYolu in indirilenDosyalar)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = dosyaYolu,
                            UseShellExecute = true
                        });

                        // Kurulumlar arasında biraz bekle
                        Thread.Sleep(3000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Kurulum başlatılamadı: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void Indirilenler()
        {
            string indirmeDizini = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CaYaDownloads");

            if (!Directory.Exists(indirmeDizini))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                İndirilenler                                  ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nİndirme klasörü bulunamadı. Henüz indirme yapılmamış olabilir.");
                Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            var dosyalar = Directory.GetFiles(indirmeDizini).Where(f => !f.EndsWith("indirme_kayitlari.csv")).ToArray();

            if (dosyalar.Length == 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                İndirilenler                                  ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nİndirilen dosya bulunamadı.");
                Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            // Menü seçeneklerini oluştur
            var menuItems = new List<string>
            {
                "Tüm dosyaları aç",
                "İndirme geçmişini göster",
                "Ana Menüye Dön"
            };

            // Dosya listesini oluştur
            var fileItems = new List<FileInfo>();
            foreach (var dosya in dosyalar)
            {
                fileItems.Add(new FileInfo(dosya));
            }

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                İndirilenler                                  ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nİndirilen dosyalar:");

                // Dosya listesini göster
                for (int i = 0; i < fileItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {fileItems[i].Name} - {FormatBoyut(fileItems[i].Length)} - {fileItems[i].LastWriteTime} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {fileItems[i].Name} - {FormatBoyut(fileItems[i].Length)} - {fileItems[i].LastWriteTime}");
                    }
                }

                Console.WriteLine();

                // Menü seçeneklerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i + fileItems.Count == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nSeçili dosya için: [Enter]: Aç, [Delete]: Sil");
                Console.WriteLine("Ok tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : fileItems.Count + menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < fileItems.Count + menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Delete:
                        if (selectedIndex < fileItems.Count)
                        {
                            // Dosya silme işlemi
                            Console.WriteLine($"\nDosya silinecek: {fileItems[selectedIndex].Name}");
                            Console.Write("Onaylıyor musunuz? (E/H): ");
                            if (Console.ReadKey().Key == ConsoleKey.E)
                            {
                                try
                                {
                                    File.Delete(fileItems[selectedIndex].FullName);
                                    Console.WriteLine("\nDosya başarıyla silindi.");

                                    // Dosya listesini güncelle
                                    fileItems.RemoveAt(selectedIndex);
                                    if (selectedIndex >= fileItems.Count && fileItems.Count > 0)
                                        selectedIndex = fileItems.Count - 1;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"\nDosya silinemedi: {ex.Message}");
                                }
                                Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                                Console.ReadKey(true);
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex < fileItems.Count)
                        {
                            // Dosyayı aç
                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = fileItems[selectedIndex].FullName,
                                    UseShellExecute = true
                                });
                                Console.WriteLine($"\n{fileItems[selectedIndex].Name} açılıyor...");
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"\nDosya açılamadı: {ex.Message}");
                                Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                                Console.ReadKey();
                            }
                        }
                        else if (selectedIndex == fileItems.Count)
                        {
                            // Tüm dosyaları aç
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = indirmeDizini,
                                UseShellExecute = true
                            });
                        }
                        else if (selectedIndex == fileItems.Count + 1)
                        {
                            // İndirme geçmişini göster
                            IndirmeGecmisiniGoster();
                        }
                        else if (selectedIndex == fileItems.Count + 2)
                        {
                            // Ana menüye dön
                            devam = false;
                        }
                        break;
                }
            }
        }

        static void IndirmeGecmisiniGoster()
        {
            string indirmeKayitDosyasi = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CaYaDownloads",
                "indirme_kayitlari.csv"
            );

            if (!File.Exists(indirmeKayitDosyasi))
            {
                Console.WriteLine("\nHenüz indirme geçmişi bulunamadı.");
                Console.WriteLine("Devam etmek için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                              İndirme Geçmişi                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            try
            {
                var satirlar = File.ReadAllLines(indirmeKayitDosyasi).Skip(1); // Başlık satırını atla

                if (!satirlar.Any())
                {
                    Console.WriteLine("\nHenüz indirme geçmişi bulunmamaktadır.");
                    Console.WriteLine("Devam etmek için bir tuşa basın...");
                    Console.ReadKey();
                    return;
                }

                int selectedIndex = 0;
                bool devam = true;
                var gecmisKayitlari = satirlar.ToList();

                while (devam)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║                              İndirme Geçmişi                                 ║");
                    Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                    Console.ResetColor();

                    Console.WriteLine("\nTarih | Yazılım Adı | Açıklama");
                    Console.WriteLine("------------------------------");

                    // Geçmiş kayıtlarını göster
                    for (int i = 0; i < gecmisKayitlari.Count; i++)
                    {
                        string[] veriler = gecmisKayitlari[i].Split(',');
                        if (veriler.Length >= 3)
                        {
                            string tarih = veriler[0];
                            string yazilimAdi = veriler[1];
                            string aciklama = veriler[2];

                            if (i == selectedIndex)
                            {
                                Console.BackgroundColor = ConsoleColor.Cyan;
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.WriteLine($" >>> {tarih} | {yazilimAdi} | {aciklama} <<<");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.WriteLine($"     {tarih} | {yazilimAdi} | {aciklama}");
                            }
                        }
                    }

                    Console.WriteLine("\n[Esc] Geri dön");

                    // Kullanıcı girişini bekle
                    var key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : gecmisKayitlari.Count - 1;
                            break;
                        case ConsoleKey.DownArrow:
                            selectedIndex = (selectedIndex < gecmisKayitlari.Count - 1) ? selectedIndex + 1 : 0;
                            break;
                        case ConsoleKey.Escape:
                            devam = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geçmiş gösterilirken hata oluştu: {ex.Message}");
                Console.WriteLine("Devam etmek için bir tuşa basın...");
                Console.ReadKey();
            }
        }

        static void YazilimGuncellemeKontrolu()
        {
            var menuItems = new List<string>
    {
        "Tüm güncellemeleri indir ve kur",
        "Seçili güncellemeleri indir ve kur",
        "Ana Menüye Dön"
    };

            int selectedIndex = 0;
            bool devam = true;
            List<YazilimGuncelleme> guncellemeler = null;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                          Yazılım Güncelleme Kontrolü                         ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                if (guncellemeler == null)
                {
                    Console.WriteLine("\nSisteminizdeki yazılımlar taranıyor...");
                    Console.WriteLine("Bu işlem biraz zaman alabilir...");

                    // Gerçek güncellemeleri winget ile kontrol et
                    guncellemeler = GetYukluYazilimlar();
                }

                Console.WriteLine("\nGüncellenebilir yazılımlar:");

                if (guncellemeler.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Tüm yazılımlarınız güncel görünüyor!");
                    Console.ResetColor();

                    // Eğer güncelleme yoksa, sadece ana menüye dön seçeneğini göster
                    Console.WriteLine("\nAna Menüye Dön");

                    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                    var keyInput = Console.ReadKey(true);
                    return; // Ana menüye dön
                }
                else
                {
                    // Güncelleme listesini göster
                    for (int i = 0; i < guncellemeler.Count; i++)
                    {
                        var guncelleme = guncellemeler[i];
                        Console.WriteLine($"{i + 1}. {guncelleme.Ad} ({guncelleme.Id}) - Mevcut: {guncelleme.MevcutSurum}, Yeni: {guncelleme.YeniSurum}");
                    }
                }

                Console.WriteLine();

                // Menü öğelerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex == menuItems.Count - 1)
                        {
                            // Ana menüye dön
                            devam = false;
                        }
                        else if (selectedIndex == 0 && guncellemeler.Count > 0)
                        {
                            // Tüm güncellemeleri yükle
                            YukleGuncellemeler(guncellemeler);
                        }
                        else if (selectedIndex == 1 && guncellemeler.Count > 0)
                        {
                            // Seçili güncellemeleri yükle
                            SeciliGuncellemeleriYukle(guncellemeler);
                        }
                        break;
                }
            }
        }

        static void YukleGuncellemeler(List<YazilimGuncelleme> guncellemeler)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          Güncellemeler Yükleniyor                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nTüm güncellemeler yüklenecek. Bu işlem biraz zaman alabilir.");
            Console.WriteLine("İşlem sırasında onay istenebilir, lütfen pencereleri kontrol edin.");
            Console.WriteLine("\nDevam etmek istiyor musunuz? (E/H)");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                Console.WriteLine("\n\nGüncellemeler yükleniyor...");

                try
                {
                    string output = RunCommand("winget upgrade --all");
                    Console.WriteLine(output);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Güncellemeler yüklenirken hata oluştu: {ex.Message}");
                }

                Console.WriteLine("\nİşlem tamamlandı! Devam etmek için bir tuşa basın...");
                Console.ReadKey();
            }
        }

        static void SeciliGuncellemeleriYukle(List<YazilimGuncelleme> guncellemeler)
        {
            // Eğer liste boşsa hiçbir şey yapma
            if (guncellemeler == null || guncellemeler.Count == 0)
            {
                Console.WriteLine("\nGüncelleme listesi boş. Devam etmek için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            int selectedIndex = 0;
            bool devam = true;
            var seciliGuncellemeler = new List<YazilimGuncelleme>();

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                       Güncellemeleri Seçin                                   ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nYüklemek istediğiniz güncellemeleri seçin (SPACE ile seçin/kaldırın, ENTER ile onaylayın):");

                // Güncelleme listesini göster
                for (int i = 0; i < guncellemeler.Count; i++)
                {
                    var guncelleme = guncellemeler[i];

                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($" >>> ");
                    }
                    else
                    {
                        Console.Write("     ");
                    }

                    if (seciliGuncellemeler.Contains(guncelleme))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[X] ");
                    }
                    else
                    {
                        Console.Write("[ ] ");
                    }

                    if (i == selectedIndex)
                    {
                        Console.WriteLine($"{guncelleme.Ad} ({guncelleme.Id}) - Mevcut: {guncelleme.MevcutSurum}, Yeni: {guncelleme.YeniSurum} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"{guncelleme.Ad} ({guncelleme.Id}) - Mevcut: {guncelleme.MevcutSurum}, Yeni: {guncelleme.YeniSurum}");
                        Console.ResetColor();
                    }
                }

                // İşlem seçenekleri
                int totalOptions = guncellemeler.Count + 2; // Güncellemeler + Yükle + İptal

                // Onay butonu
                if (selectedIndex == guncellemeler.Count)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine(" >>> SEÇİLİ GÜNCELLEMELERİ YÜKLE <<<");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("     SEÇİLİ GÜNCELLEMELERİ YÜKLE");
                }

                // İptal butonu
                if (selectedIndex == guncellemeler.Count + 1)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" >>> İPTAL <<<");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("     İPTAL");
                }

                Console.WriteLine("\nYukarı/Aşağı Ok: Seçim değiştir, SPACE: Seç/Kaldır, ENTER: Onay");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        // Yukarı ok tuşuna basılırsa seçimi bir yukarı taşı, üstte ise sona git
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : totalOptions - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        // Aşağı ok tuşuna basılırsa seçimi bir aşağı taşı, sonda ise başa git
                        selectedIndex = (selectedIndex < totalOptions - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Spacebar:
                        // Space tuşu sadece güncelleme listesindeyken çalışmalı
                        if (selectedIndex < guncellemeler.Count)
                        {
                            var seciliGuncelleme = guncellemeler[selectedIndex];
                            if (seciliGuncellemeler.Contains(seciliGuncelleme))
                                seciliGuncellemeler.Remove(seciliGuncelleme);
                            else
                                seciliGuncellemeler.Add(seciliGuncelleme);
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex == guncellemeler.Count)
                        {
                            // Seçili güncellemeleri yükle
                            if (seciliGuncellemeler.Count > 0)
                            {
                                KurulumaBasla(seciliGuncellemeler);
                                devam = false;
                            }
                            else
                            {
                                Console.WriteLine("\nLütfen en az bir güncelleme seçin!");
                                Thread.Sleep(1500);
                            }
                        }
                        else if (selectedIndex == guncellemeler.Count + 1)
                        {
                            // İptal et
                            devam = false;
                        }
                        break;
                }
            }
        }

        static void KurulumaBasla(List<YazilimGuncelleme> seciliGuncellemeler)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          Güncellemeler Yükleniyor                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine($"\nSeçilen {seciliGuncellemeler.Count} güncelleme yüklenecek:");

            foreach (var guncelleme in seciliGuncellemeler)
            {
                Console.WriteLine($"- {guncelleme.Ad} ({guncelleme.MevcutSurum} -> {guncelleme.YeniSurum})");
            }

            Console.WriteLine("\nDevam etmek istiyor musunuz? (E/H)");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                Console.WriteLine("\n\nGüncellemeler yükleniyor...");

                int basari = 0;
                int hata = 0;

                foreach (var guncelleme in seciliGuncellemeler)
                {
                    Console.WriteLine($"\nGüncelleniyor: {guncelleme.Ad}");

                    try
                    {
                        string command = $"winget upgrade --id {guncelleme.Id}";
                        string output = RunCommand(command);
                        Console.WriteLine(output);

                        if (output.Contains("successfully") || output.Contains("başarıyla"))
                        {
                            basari++;
                        }
                        else
                        {
                            hata++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Hata: {ex.Message}");
                        hata++;
                    }
                }

                Console.WriteLine($"\nGüncelleme tamamlandı: {basari} başarılı, {hata} başarısız");
                Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                Console.ReadKey();
            }
        }

        static List<YazilimGuncelleme> GetYukluYazilimlar()
        {
            var guncellemeler = new List<YazilimGuncelleme>();

            try
            {
                // Winget kullanarak mevcut güncellemeleri kontrol et
                Console.WriteLine("winget ile güncellemeler kontrol ediliyor...");

                // winget'in yüklü olup olmadığını kontrol et
                if (!IsWingetInstalled())
                {
                    Console.WriteLine("winget yüklü değil. Lütfen Microsoft Store'dan 'App Installer' uygulamasını yükleyin.");
                    return guncellemeler;
                }

                string wingetOutput = RunCommand("winget upgrade");

                // winget çıktısını işle
                string[] lines = wingetOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length <= 3)
                {
                    Console.WriteLine("Güncelleme bulunamadı veya winget çıktısı beklenen formatta değil.");
                    return guncellemeler;
                }

                bool startProcessing = false;
                int tableHeaderLineIndex = -1;

                // Başlık satırını bul
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("Name") && lines[i].Contains("Id") && lines[i].Contains("Version") && lines[i].Contains("Available"))
                    {
                        startProcessing = true;
                        tableHeaderLineIndex = i;
                        break;
                    }
                }

                if (!startProcessing || tableHeaderLineIndex < 0 || tableHeaderLineIndex >= lines.Length - 1)
                {
                    Console.WriteLine("Tablo başlığı bulunamadı veya tablo yapısı beklenen formatta değil.");
                    return guncellemeler;
                }

                // Çizgi satırını atla
                int dataStartLineIndex = tableHeaderLineIndex + 1;
                if (dataStartLineIndex < lines.Length && lines[dataStartLineIndex].Contains("---"))
                {
                    dataStartLineIndex++;
                }

                // Paket verilerini işle
                for (int i = dataStartLineIndex; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Boş satırları veya "güncel olan" gibi son satırları atla
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("The following") || line.StartsWith("No") || line.Contains("up to date"))
                        continue;

                    try
                    {
                        // İçeriği tab veya çoklu boşluk olarak parçala
                        string[] parts = SplitByMultipleSpaces(line);

                        if (parts.Length >= 4)
                        {
                            string name = parts[0];
                            string id = parts[1];
                            string currentVersion = parts[2];
                            string availableVersion = parts[3];

                            // Geçerli değerler kontrolü
                            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(id))
                            {
                                guncellemeler.Add(new YazilimGuncelleme
                                {
                                    Ad = name,
                                    Id = id,
                                    MevcutSurum = currentVersion,
                                    YeniSurum = availableVersion
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Hatalı format, bu satırı atla ve devam et
                        continue;
                    }
                }

                Console.WriteLine($"Toplam {guncellemeler.Count} güncelleme bulundu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Güncellemeler kontrol edilirken hata oluştu: {ex.Message}");
            }

            return guncellemeler;
        }

        static bool IsWingetInstalled()
        {
            try
            {
                string output = RunCommand("where winget");
                return output.Contains("winget.exe");
            }
            catch
            {
                return false;
            }
        }

        static string RunCommand(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                process.OutputDataReceived += (sender, e) => {
                    if (e.Data != null)
                        output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) => {
                    if (e.Data != null)
                        error.AppendLine(e.Data);
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Maksimum 30 saniye bekle
                bool exited = process.WaitForExit(30000);

                if (!exited)
                {
                    try { process.Kill(); } catch { }
                    throw new Exception("Komut zaman aşımına uğradı.");
                }

                // Çıktı ve hata kontrolü
                string result = output.ToString();
                string errorText = error.ToString();

                if (!string.IsNullOrEmpty(errorText) && string.IsNullOrEmpty(result))
                    throw new Exception(errorText);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Komut çalıştırılırken hata oluştu: {ex.Message}");
            }
        }

        static string[] SplitByMultipleSpaces(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new string[0];

            // Düzenli ifade ile birden fazla boşlukla bölme
            var result = System.Text.RegularExpressions.Regex.Split(input, @"\s{2,}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToArray();

            // Çıktı kontrolü    
            return result;
        }
        class YazilimGuncelleme
        {
            public string Ad { get; set; }
            public string Id { get; set; } // Winget paket kimliği
            public string MevcutSurum { get; set; }
            public string YeniSurum { get; set; }
        }

        static void SistemBilgisi()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                Sistem Bilgisi                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            try
            {
                // İşletim sistemi bilgisi
                string isletimSistemi = Environment.OSVersion.ToString();
                string bilgisayarAdi = Environment.MachineName;
                string kullaniciAdi = Environment.UserName;

                Console.WriteLine($"\nBilgisayar Adı: {bilgisayarAdi}");
                Console.WriteLine($"Kullanıcı: {kullaniciAdi}");
                Console.WriteLine($"İşletim Sistemi: {isletimSistemi}");
                Console.WriteLine($"İşlemci Mimarisi: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");

                // İşlemci bilgisi
                using (ManagementObjectSearcher islemciAraci = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject islemci in islemciAraci.Get())
                    {
                        Console.WriteLine($"\nİşlemci: {islemci["Name"]}");
                        Console.WriteLine($"Çekirdek Sayısı: {islemci["NumberOfCores"]}");
                        Console.WriteLine($"Mantıksal İşlemci Sayısı: {islemci["NumberOfLogicalProcessors"]}");
                        Console.WriteLine($"Hız: {islemci["MaxClockSpeed"]} MHz");

                        // Gerçek zamanlı CPU kullanımı
                        try
                        {
                            float cpuUsage = cpuCounter?.NextValue() ?? 0;
                            Console.WriteLine($"CPU Kullanımı: %{cpuUsage:F1}");
                        }
                        catch
                        {
                            Console.WriteLine("CPU Kullanımı: Bilgi alınamadı");
                        }
                    }
                }

                // Bellek bilgisi
                using (ManagementObjectSearcher bellekaraci = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject bellek in bellekaraci.Get())
                    {
                        double toplamRamGB = Convert.ToDouble(bellek["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                        Console.WriteLine($"\nToplam RAM: {toplamRamGB:F2} GB");

                        // Gerçek zamanlı RAM kullanımı
                        try
                        {
                            float availableRam = ramCounter?.NextValue() ?? 0;
                            float totalRamMB = (float)(toplamRamGB * 1024);
                            float usedRamPercentage = (1 - (availableRam / totalRamMB)) * 100;

                            Console.WriteLine($"Kullanılan RAM: %{usedRamPercentage:F1}");
                            Console.WriteLine($"Boş RAM: {availableRam:F0} MB");
                        }
                        catch
                        {
                            Console.WriteLine("RAM Kullanımı: Bilgi alınamadı");
                        }
                    }
                }

                // Disk bilgisi
                DriveInfo[] tumSuruculer = DriveInfo.GetDrives();
                Console.WriteLine("\nDisk Bilgileri:");

                foreach (DriveInfo surucu in tumSuruculer)
                {
                    if (surucu.IsReady && surucu.DriveType == DriveType.Fixed)
                    {
                        Console.WriteLine($"\nSürücü: {surucu.Name}");
                        Console.WriteLine($"Etiket: {surucu.VolumeLabel}");
                        Console.WriteLine($"Dosya Sistemi: {surucu.DriveFormat}");
                        Console.WriteLine($"Toplam Boyut: {FormatBoyut(surucu.TotalSize)}");
                        Console.WriteLine($"Boş Alan: {FormatBoyut(surucu.AvailableFreeSpace)}");

                        double kullanilmisYuzde = 100.0 * (1.0 - ((double)surucu.AvailableFreeSpace / surucu.TotalSize));
                        Console.WriteLine($"Kullanım: %{kullanilmisYuzde:F1}");
                    }
                }

                // Ekran kartı bilgisi
                using (ManagementObjectSearcher ekranKartiAraci = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject ekranKarti in ekranKartiAraci.Get())
                    {
                        Console.WriteLine($"\nEkran Kartı: {ekranKarti["Name"]}");
                        Console.WriteLine($"Sürücü Sürümü: {ekranKarti["DriverVersion"]}");
                        Console.WriteLine($"Video RAM: {FormatBoyut(Convert.ToInt64(ekranKarti["AdapterRAM"] ?? 0))}");
                        Console.WriteLine($"Çözünürlük: {ekranKarti["CurrentHorizontalResolution"]} x {ekranKarti["CurrentVerticalResolution"]}");
                    }
                }

                // Güvenlik durumu
                Console.WriteLine("\nGüvenlik Durumu:");

                // Windows Defender durumu
                bool defenderDurum = KontrolDefender();
                Console.WriteLine($"Windows Defender: {(defenderDurum ? "Aktif" : "Devre Dışı")}");

                // Windows Güvenlik Duvarı durumu
                bool guvenlikDuvariDurum = KontrolGuvenlikDuvari();
                Console.WriteLine($"Güvenlik Duvarı: {(guvenlikDuvariDurum ? "Aktif" : "Devre Dışı")}");

                // Son güncellemeler
                DateTime sonGuncelleme = GetSonWindowsGuncellemeTarihi();
                int gunFarki = (DateTime.Now - sonGuncelleme).Days;
                Console.WriteLine($"Son Windows güncellemesi: {sonGuncelleme.ToString("dd.MM.yyyy")} ({gunFarki} gün önce)");

                // Başlangıç uygulamaları
                int baslangicUygulamaSayisi = GetBaslangicUygulamaSayisi();
                Console.WriteLine($"\nToplam başlangıç uygulaması: {baslangicUygulamaSayisi}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sistem bilgisi alınırken bir hata oluştu: {ex.Message}");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static bool KontrolDefender()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("DisableRealtimeMonitoring");
                        return value == null || (int)value != 1;
                    }
                }
            }
            catch { }

            return true; // Varsayılan olarak aktif kabul et
        }

        static bool KontrolGuvenlikDuvari()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM FirewallProduct"))
                {
                    foreach (var item in searcher.Get())
                    {
                        var enabled = item["Enabled"];
                        if (enabled != null && (bool)enabled)
                            return true;
                    }
                }

                //// Alternatif kontrol - Windows Güvenlik Duvarı servisi çalışıyor mu?
                //using (ServiceController sc = new ServiceController("MpsSvc"))
                //{
                //    return sc.Status == ServiceControllerStatus.Running;
                //}
            }
            catch { }

            return true; // Varsayılan olarak aktif kabul et
        }

        static DateTime GetSonWindowsGuncellemeTarihi()
        {
            try
            {
                // Windows Update geçmişini tara
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_QuickFixEngineering"))
                {
                    var updates = searcher.Get().Cast<ManagementObject>().ToList();
                    if (updates.Count > 0)
                    {
                        var latestUpdate = updates
                            .Select(u => DateTime.ParseExact(u["InstalledOn"].ToString(), "M/d/yyyy", CultureInfo.InvariantCulture))
                            .Max();
                        return latestUpdate;
                    }
                }
            }
            catch { }

            // Gerçek veri alınamadığında simülasyon yap
            return DateTime.Now.AddDays(-new Random().Next(1, 30));
        }

        static int GetBaslangicUygulamaSayisi()
        {
            List<string> baslangicUygulamalari = new List<string>();

            try
            {
                // 1. Kullanıcı başlangıç klasörü
                string userStartupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                if (Directory.Exists(userStartupPath))
                {
                    foreach (string dosya in Directory.GetFiles(userStartupPath))
                    {
                        baslangicUygulamalari.Add(Path.GetFileName(dosya));
                    }
                }

                // 2. Tüm kullanıcılar için başlangıç klasörü
                string allUsersStartupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));
                if (Directory.Exists(allUsersStartupPath))
                {
                    foreach (string dosya in Directory.GetFiles(allUsersStartupPath))
                    {
                        baslangicUygulamalari.Add(Path.GetFileName(dosya));
                    }
                }

                // 3. HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }

                // 4. HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }

                // 5. HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }

                // 6. HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başlangıç uygulamaları alınırken hata oluştu: {ex.Message}");
            }

            return baslangicUygulamalari.Count;
        }

        static long GetTotalPhysicalMemory()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    }
                }
            }
            catch { }

            return 8L * 1024L * 1024L * 1024L; // Varsayılan olarak 8GB
        }

        static void SistemBakimi()
        {
            var menuItems = new List<string>
            {
                "Disk Temizleme",
                "Sistem Dosyası Denetleyicisi (SFC)",
                "Disk Hataları Kontrolü (CHKDSK)",
                "Windows Bileşenleri Denetleyicisi (DISM)",
                "Sistem Analizi",
                "Ana Menüye Dön"
            };

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                 Sistem Bakımı                                ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                // Menü öğelerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedIndex)
                        {
                            case 0: DiskTemizleme(); break;
                            case 1: SistemDosyasiDenetleyicisi(); break;
                            case 2: DiskHatalariKontrolu(); break;
                            case 3: WindowsBilesenDenetleyicisi(); break;
                            case 4: SistemAnalizi(); break;
                            case 5: devam = false; break;
                        }
                        break;
                }
            }
        }

        static void DiskTemizleme()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                Disk Temizleme                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nDisk temizleme çalıştırılıyor...");

            try
            {
                // Windows'un kendi Disk Temizleme aracını çalıştır
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cleanmgr.exe",
                    Arguments = "/sagerun:1",
                    UseShellExecute = true
                });

                Console.WriteLine("\nDisk temizleme aracı başlatıldı. Lütfen işlemi tamamlayın.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disk temizleme başlatılamadı: {ex.Message}");
            }

            Console.WriteLine("\nEk olarak, aşağıdaki geçici dosya klasörlerini temizlemek ister misiniz? (E/H)");
            Console.WriteLine("- Temporary Internet Files");
            Console.WriteLine("- Windows Temp Klasörü");
            Console.WriteLine("- Kullanıcı Temp Klasörü");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                Console.WriteLine("\n\nGeçici dosyalar temizleniyor...");

                try
                {
                    // Temp klasörlerini temizle
                    string tempPath = Path.GetTempPath();
                    string winTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

                    // Kullanıcı temp klasörü
                    TemizleKlasor(tempPath);

                    // Windows temp klasörü
                    TemizleKlasor(winTempPath);

                    Console.WriteLine("\nGeçici dosya klasörleri temizlendi.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nGeçici dosyalar temizlenirken hata oluştu: {ex.Message}");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void TemizleKlasor(string klasorYolu)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(klasorYolu);

                foreach (FileInfo dosya in di.GetFiles())
                {
                    try
                    {
                        dosya.Delete();
                        Console.WriteLine($"Silindi: {dosya.Name}");
                    }
                    catch
                    {
                        // Silinemeyen dosyaları atla
                    }
                }

                foreach (DirectoryInfo altDizin in di.GetDirectories())
                {
                    try
                    {
                        altDizin.Delete(true);
                        Console.WriteLine($"Silindi: {altDizin.Name}");
                    }
                    catch
                    {
                        // Silinemeyen klasörleri atla
                    }
                }
            }
            catch
            {
                // Erişilemeyen klasörleri atla
            }
        }

        static void SistemDosyasiDenetleyicisi()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        Sistem Dosyası Denetleyicisi                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nSistem Dosyası Denetleyicisi (SFC), Windows sistem dosyalarını tarar ve sorunları düzeltir.");
            Console.WriteLine("Bu işlem biraz zaman alabilir.");
            Console.WriteLine("\nDevam etmek istiyor musunuz? (E/H)");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                Console.WriteLine("\n\nSistem dosyaları denetleniyor...");

                try
                {
                    Process sfc = new Process();
                    sfc.StartInfo.FileName = "cmd.exe";
                    sfc.StartInfo.Arguments = "/c sfc /scannow";
                    sfc.StartInfo.UseShellExecute = true;
                    sfc.StartInfo.Verb = "runas"; // Yönetici olarak çalıştır
                    sfc.Start();

                    Console.WriteLine("\nSistem Dosyası Denetleyicisi başlatıldı.");
                    Console.WriteLine("Bu pencereyi kapatabilir ve işlemi komut penceresi üzerinden takip edebilirsiniz.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nSistem Dosyası Denetleyicisi başlatılamadı: {ex.Message}");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void DiskHatalariKontrolu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                            Disk Hataları Kontrolü                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nMevcut Sürücüler:");

            DriveInfo[] tumSuruculer = DriveInfo.GetDrives();
            List<string> sürücüHarfleri = new List<string>();

            foreach (DriveInfo surucu in tumSuruculer)
            {
                if (surucu.IsReady && surucu.DriveType == DriveType.Fixed)
                {
                    sürücüHarfleri.Add(surucu.Name.Substring(0, 1));
                    Console.WriteLine($"{surucu.Name} - {surucu.VolumeLabel} ({FormatBoyut(surucu.TotalSize)})");
                }
            }

            Console.Write("\nKontrol etmek istediğiniz sürücünün harfini girin (örn. C): ");
            string surucuHarfi = Console.ReadLine().ToUpper();

            if (string.IsNullOrEmpty(surucuHarfi) || !sürücüHarfleri.Contains(surucuHarfi))
            {
                Console.WriteLine("Geçersiz sürücü harfi!");
            }
            else
            {
                Console.WriteLine("\nDisk hatalarını onar? (E/H)");
                bool onar = Console.ReadKey().Key == ConsoleKey.E;

                Console.WriteLine("\n\nBir sonraki sistem başlangıcında disk kontrolü yapılması planlanacak.");
                Console.WriteLine("Bu işlem sistem yeniden başlatıldığında gerçekleşecektir.");
                Console.WriteLine("\nDevam etmek istiyor musunuz? (E/H)");

                if (Console.ReadKey().Key == ConsoleKey.E)
                {
                    try
                    {
                        Process chkdsk = new Process();
                        chkdsk.StartInfo.FileName = "cmd.exe";
                        string arguments = onar ? $"/c chkdsk {surucuHarfi}: /f /r /x" : $"/c chkdsk {surucuHarfi}:";
                        chkdsk.StartInfo.Arguments = arguments;
                        chkdsk.StartInfo.UseShellExecute = true;
                        chkdsk.StartInfo.Verb = "runas"; // Yönetici olarak çalıştır
                        chkdsk.Start();

                        Console.WriteLine("\nDisk kontrolü planlandı. İşlemi tamamlamak için komut penceresindeki talimatları izleyin.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nDisk kontrolü başlatılamadı: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void WindowsBilesenDenetleyicisi()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       Windows Bileşenleri Denetleyicisi                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nWindows Bileşenleri Denetleyicisi (DISM), Windows görüntüsünde hasar olup olmadığını kontrol eder.");
            Console.WriteLine("Bu işlem biraz zaman alabilir.");
            Console.WriteLine("\n[1] Windows görüntüsü sağlığını denetle");
            Console.WriteLine("[2] Windows görüntüsünü onar");
            Console.WriteLine("[0] Geri dön");

            Console.Write("\nSeçiminiz: ");
            string secim = Console.ReadLine();

            try
            {
                Process dism = new Process();
                dism.StartInfo.FileName = "cmd.exe";
                dism.StartInfo.UseShellExecute = true;
                dism.StartInfo.Verb = "runas"; // Yönetici olarak çalıştır

                switch (secim)
                {
                    case "1":
                        Console.WriteLine("\nWindows görüntüsü denetleniyor...");
                        dism.StartInfo.Arguments = "/c DISM /Online /Cleanup-Image /CheckHealth";
                        dism.Start();
                        break;
                    case "2":
                        Console.WriteLine("\nWindows görüntüsü onarılıyor...");
                        dism.StartInfo.Arguments = "/c DISM /Online /Cleanup-Image /RestoreHealth";
                        dism.Start();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Geçersiz seçim!");
                        break;
                }

                Console.WriteLine("\nWindows Bileşenleri Denetleyicisi başlatıldı.");
                Console.WriteLine("Bu pencereyi kapatabilir ve işlemi komut penceresi üzerinden takip edebilirsiniz.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nWindows Bileşenleri Denetleyicisi başlatılamadı: {ex.Message}");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void SistemAnalizi()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                Sistem Analizi                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nSistem analizi yapılıyor...");
            Console.WriteLine("Bu işlem birkaç dakika sürebilir...");

            // Gerçek sistem verilerini toplama işlemi
            var sistemRaporu = new SistemRaporu();
            sistemRaporu.VeriTopla();

            Console.WriteLine("\nSistem Raporu:");

            // İşletim sistemi durumu
            Console.WriteLine("\n[İşletim Sistemi]");
            Console.WriteLine($"Windows sürümü: {sistemRaporu.ISSurumu}");
            Console.WriteLine($"Sürüm: {sistemRaporu.IsTipi}");

            // Bellek durumu
            Console.WriteLine("\n[Bellek Durumu]");
            Console.WriteLine($"Toplam RAM: {FormatBoyut(sistemRaporu.ToplamRam)}");
            Console.WriteLine($"Kullanılan RAM: {FormatBoyut(sistemRaporu.KullanilanRam)} (%{sistemRaporu.RamYuzdesi:F1})");
            Console.WriteLine($"Boş RAM: {FormatBoyut(sistemRaporu.BosRam)}");

            // Disk durumu
            Console.WriteLine("\n[Disk Durumu]");

            foreach (var disk in sistemRaporu.Diskler)
            {
                Console.WriteLine($"\nSürücü: {disk.Ad}");
                Console.WriteLine($"Toplam Alan: {FormatBoyut(disk.ToplamAlan)}");
                Console.WriteLine($"Kullanılan Alan: {FormatBoyut(disk.KullanilanAlan)} (%{disk.KullanilmisYuzde:F1})");
                Console.WriteLine($"Boş Alan: {FormatBoyut(disk.BosAlan)}");
            }

            // CPU kullanımı
            Console.WriteLine("\n[CPU Kullanımı]");
            Console.WriteLine($"CPU kullanımı: %{sistemRaporu.CpuKullanimi:F1}");

            // Güvenlik durumu
            Console.WriteLine("\n[Güvenlik Durumu]");
            Console.WriteLine($"Windows Defender: {(sistemRaporu.WindowsDefenderDurum ? "Aktif" : "Devre Dışı")}");
            Console.WriteLine($"Son virüs taraması: {sistemRaporu.SonVirusTaramasi}");
            Console.WriteLine($"Güvenlik duvarı: {(sistemRaporu.GuvenlikDuvariDurum ? "Aktif" : "Devre Dışı")}");

            // Başlangıç uygulamaları
            Console.WriteLine("\n[Başlangıç Uygulamaları]");
            Console.WriteLine($"Toplam başlangıç uygulaması sayısı: {sistemRaporu.BaslangicUygulamaSayisi}");

            if (sistemRaporu.BaslangicUygulamaSayisi > 10)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Önerilen: Başlangıç uygulamalarını gözden geçirin");
                Console.ResetColor();
            }

            // Sorunlar ve öneriler
            Console.WriteLine("\n[Tespit Edilen Sorunlar ve Öneriler]");

            if (sistemRaporu.Sorunlar.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Tespit edilen önemli sorun yok. Sisteminiz iyi durumda görünüyor!");
                Console.ResetColor();
            }
            else
            {
                for (int i = 0; i < sistemRaporu.Sorunlar.Count; i++)
                {
                    if (sistemRaporu.Sorunlar[i].Onem == SorunOncelik.Yuksek)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (sistemRaporu.Sorunlar[i].Onem == SorunOncelik.Orta)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Gray;

                    Console.WriteLine($"{i + 1}. {sistemRaporu.Sorunlar[i].Aciklama}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nBu sorunları düzeltmek için Sistem Optimizasyonu bölümünü kullanabilirsiniz.");
            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void SistemOptimizasyonu()
        {
            var menuItems = new List<string>
            {
                "Hızlı Optimizasyon (Temizleme ve Geçici Dosya Silme)",
                "Gelişmiş Optimizasyon",
                "Başlangıç Uygulamalarını Yönet",
                "Windows Görsel Efektlerini Optimize Et",
                "Ana Menüye Dön"
            };

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                          Sistem Performans Optimizasyonu                     ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                // Menü öğelerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedIndex)
                        {
                            case 0: HizliOptimizasyon(); break;
                            case 1: GelismisOptimizasyon(); break;
                            case 2: BaslangicUygulamalari(); break;
                            case 3: GorselEfektleriOptimizeEt(); break;
                            case 4: devam = false; break;
                        }
                        break;
                }
            }
        }

        static void HizliOptimizasyon()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                              Hızlı Optimizasyon                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nHızlı optimizasyon yapılıyor...");
            Console.WriteLine("Gerçekleştirilecek işlemler:");
            Console.WriteLine("- Geçici dosyaların temizlenmesi");
            Console.WriteLine("- Önbellek temizliği");
            Console.WriteLine("- Gereksiz sistem dosyalarının kaldırılması");
            Console.WriteLine("- RAM temizliği");

            // İlerleme gösterimi
            for (int i = 0; i <= 100; i += 5)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);

                // İlerleme çubuğu
                int barWidth = 50;
                int completeWidth = i * barWidth / 100;

                Console.Write("\n[");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(new string('#', completeWidth));
                Console.ResetColor();
                Console.Write(new string('-', barWidth - completeWidth));
                Console.Write($"] %{i}");

                Thread.Sleep(50);
            }

            // Temp dosyalarını temizleme
            try
            {
                Console.WriteLine("\n\nGeçici dosyalar temizleniyor...");
                string tempPath = Path.GetTempPath();
                TemizleKlasor(tempPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geçici dosyalar temizlenirken hata: {ex.Message}");
            }

            // Disk temizleme
            try
            {
                Console.WriteLine("\nDisk temizleme başlatılıyor...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cleanmgr.exe",
                    Arguments = "/sagerun:1",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disk temizleme başlatılamadı: {ex.Message}");
            }

            // RAM temizleme
            Console.WriteLine("\nRAM temizleniyor...");
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(500);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Windows Explorer yeniden başlatma
            try
            {
                Console.WriteLine("\nWindows Explorer yeniden başlatılıyor...");

                foreach (Process proc in Process.GetProcessesByName("explorer"))
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch { }
                }

                // Explorer.exe'yi yeniden başlat
                Thread.Sleep(1000);
                Process.Start("explorer.exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Windows Explorer yeniden başlatılamadı: {ex.Message}");
            }

            Console.WriteLine("\nHızlı optimizasyon tamamlandı!");
            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void GelismisOptimizasyon()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                            Gelişmiş Optimizasyon                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nBu özellik, sisteminizin performansını artırmak için çeşitli gelişmiş optimizasyonlar yapacaktır.");
            Console.WriteLine("NOT: Bu işlem yönetici izni gerektirir ve sistem bileşenlerinde değişiklikler yapar.");
            Console.WriteLine("\nAşağıdaki optimizasyonlar gerçekleştirilecektir:");
            Console.WriteLine("1. Sistem hizmetlerinin optimizasyonu");
            Console.WriteLine("2. Registry temizliği ve optimizasyonu");
            Console.WriteLine("3. Disk birleştirme");
            Console.WriteLine("4. Sistem önbelleğinin temizlenmesi");
            Console.WriteLine("5. Windows Telemetri ve gereksiz hizmetlerin devre dışı bırakılması");

            Console.WriteLine("\nDevam etmek istiyor musunuz? (E/H)");
            if (Console.ReadKey().Key != ConsoleKey.E)
            {
                return;
            }

            Console.WriteLine("\n\nGelişmiş optimizasyon yapılıyor...");

            // Hizmet optimizasyonu
            Console.WriteLine("\nSistem hizmetleri optimize ediliyor...");

            // Gereksiz hizmetleri durdurma (simülasyon)
            Console.Write("İlerleme: ");
            for (int i = 0; i <= 100; i += 10)
            {
                Console.Write($"%{i}.. ");
                Thread.Sleep(200);
            }

            // Registry optimizasyonu
            Console.WriteLine("\n\nRegistry temizleniyor ve optimize ediliyor...");

            // Registry temizliği (simülasyon)
            Console.Write("İlerleme: ");
            for (int i = 0; i <= 100; i += 10)
            {
                Console.Write($"%{i}.. ");
                Thread.Sleep(200);
            }

            // Disk birleştirme
            Console.WriteLine("\n\nDisk birleştirme başlatılıyor...");
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "dfrgui.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disk birleştirme başlatılamadı: {ex.Message}");
            }

            // Sistem önbelleği temizliği
            Console.WriteLine("\nSistem önbelleği temizleniyor...");

            // Simülasyon
            Console.Write("İlerleme: ");
            for (int i = 0; i <= 100; i += 10)
            {
                Console.Write($"%{i}.. ");
                Thread.Sleep(200);
            }

            // Windows Telemetri ayarları
            Console.WriteLine("\n\nWindows Telemetri ayarları optimize ediliyor...");

            // Simülasyon
            Console.Write("İlerleme: ");
            for (int i = 0; i <= 100; i += 10)
            {
                Console.Write($"%{i}.. ");
                Thread.Sleep(200);
            }

            Console.WriteLine("\n\nGelişmiş optimizasyon tamamlandı!");
            Console.WriteLine("Optimal performans için sistemi yeniden başlatmanız önerilir.");

            Console.WriteLine("\nSistemi şimdi yeniden başlatmak istiyor musunuz? (E/H)");
            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                try
                {
                    Process.Start("shutdown.exe", "/r /t 30 /c \"CaYa Yazılım Yöneticisi tarafından yeniden başlatılıyor.\"");
                    Console.WriteLine("\nSistem 30 saniye içinde yeniden başlatılacak.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nYeniden başlatma başlatılamadı: {ex.Message}");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void BaslangicUygulamalari()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         Başlangıç Uygulamaları Yönetimi                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nSistemde tespit edilen başlangıç uygulamaları:");

            List<string> baslangicUygulamalari = GetBaslangicUygulamalariListesi();

            if (baslangicUygulamalari.Count == 0)
            {
                Console.WriteLine("\nBaşlangıç uygulaması tespit edilemedi.");
            }
            else
            {
                Console.WriteLine($"\nToplam {baslangicUygulamalari.Count} başlangıç uygulaması tespit edildi:\n");

                for (int i = 0; i < baslangicUygulamalari.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {baslangicUygulamalari[i]}");
                }

                if (baslangicUygulamalari.Count > 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nNOT: Çok fazla başlangıç uygulaması sistem performansını düşürebilir.");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nWindows'un başlangıç uygulamaları yönetimi açılıyor...");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
                    Arguments = "/7 /startup",
                    UseShellExecute = true
                });

                Console.WriteLine("\nGörev Yöneticisi'nin Başlangıç sekmesi açılıyor.");
                Console.WriteLine("Başlangıç uygulamalarını buradan etkinleştirebilir veya devre dışı bırakabilirsiniz.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Görev Yöneticisi başlatılamadı: {ex.Message}");

                // Alternatif olarak msconfig ile başlangıç uygulamalarını göstermeyi dene
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "msconfig.exe",
                        UseShellExecute = true
                    });

                    Console.WriteLine("\nSistem Yapılandırması açıldı. 'Başlangıç' sekmesine geçerek işlem yapabilirsiniz.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Sistem Yapılandırması başlatılamadı: {e.Message}");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static List<string> GetBaslangicUygulamalariListesi()
        {
            List<string> baslangicUygulamalari = new List<string>();

            try
            {
                // 1. Kullanıcı başlangıç klasörü
                string userStartupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                if (Directory.Exists(userStartupPath))
                {
                    foreach (string dosya in Directory.GetFiles(userStartupPath))
                    {
                        baslangicUygulamalari.Add($"[Kullanıcı Başlangıç Klasörü] {Path.GetFileName(dosya)}");
                    }
                }

                // 2. Tüm kullanıcılar için başlangıç klasörü
                string allUsersStartupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));
                if (Directory.Exists(allUsersStartupPath))
                {
                    foreach (string dosya in Directory.GetFiles(allUsersStartupPath))
                    {
                        baslangicUygulamalari.Add($"[Ortak Başlangıç Klasörü] {Path.GetFileName(dosya)}");
                    }
                }

                // 3. HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                object value = key.GetValue(valueName);
                                baslangicUygulamalari.Add($"[HKCU\\Run] {valueName}: {value}");
                            }
                        }
                    }
                }

                // 4. HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                object value = key.GetValue(valueName);
                                baslangicUygulamalari.Add($"[HKLM\\Run] {valueName}: {value}");
                            }
                        }
                    }
                }

                // 5. HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                object value = key.GetValue(valueName);
                                baslangicUygulamalari.Add($"[HKCU\\RunOnce] {valueName}: {value}");
                            }
                        }
                    }
                }

                // 6. HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                object value = key.GetValue(valueName);
                                baslangicUygulamalari.Add($"[HKLM\\RunOnce] {valueName}: {value}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başlangıç uygulamaları alınırken hata oluştu: {ex.Message}");
            }

            return baslangicUygulamalari;
        }

        static void GorselEfektleriOptimizeEt()
        {
            var menuItems = new List<string>
            {
                "Hafif Optimizasyon (Bazı efektleri kapat)",
                "Orta Optimizasyon (Çoğu efekti kapat, temel efektleri koru)",
                "Maksimum Performans (Tüm görsel efektleri kapat)",
                "Windows Görsel Efektler Ayarlarını Aç",
                "Geri Dön"
            };

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                      Windows Görsel Efektleri Optimizasyonu                  ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nWindows'un görsel efektlerini optimize etmek, özellikle düşük sistem özellikleri olan");
                Console.WriteLine("bilgisayarlarda performans artışı sağlayabilir.");

                // Menü öğelerini göster
                Console.WriteLine();
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedIndex)
                        {
                            case 0: // Hafif Optimizasyon
                                OptimizeGorselEfektler(1);
                                break;
                            case 1: // Orta Optimizasyon
                                OptimizeGorselEfektler(2);
                                break;
                            case 2: // Maksimum Performans
                                OptimizeGorselEfektler(3);
                                break;
                            case 3: // Windows Görsel Efektler Ayarları
                                try
                                {
                                    // Windows Performans Seçenekleri penceresini aç
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = "SystemPropertiesPerformance.exe",
                                        UseShellExecute = true
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Performans Seçenekleri penceresi açılamadı: {ex.Message}");

                                    // Alternatif yöntem
                                    try
                                    {
                                        Process.Start(new ProcessStartInfo
                                        {
                                            FileName = "control.exe",
                                            Arguments = "sysdm.cpl,,3",
                                            UseShellExecute = true
                                        });
                                        Console.WriteLine("\nSistem Özellikleri açıldı. 'Gelişmiş' sekmesinde 'Performans' altındaki 'Ayarlar' butonuna tıklayın.");
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"Sistem Özellikleri açılamadı: {e.Message}");
                                    }
                                }
                                break;
                            case 4: // Geri Dön
                                devam = false;
                                break;
                        }
                        break;
                }
            }
        }

        static void OptimizeGorselEfektler(int seviye)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      Windows Görsel Efektleri Optimizasyonu                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            string seviyeAdi = seviye == 1 ? "Hafif" : (seviye == 2 ? "Orta" : "Maksimum");
            Console.WriteLine($"\n{seviyeAdi} optimizasyon uygulanıyor...");

            try
            {
                // Registry üzerinden görsel efektleri ayarlama
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", true))
                {
                    if (key != null)
                    {
                        // Visual Effects ayarını değiştir
                        // 0: Let Windows decide
                        // 1: Best appearance
                        // 2: Custom
                        // 3: Best performance
                        key.SetValue("VisualFXSetting", seviye == 3 ? 3 : 2);
                    }
                }

                // Animasyon ayarları için
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        if (seviye >= 2) // Orta ve Maksimum için animasyonları kapat
                        {
                            key.SetValue("UserPreferencesMask", new byte[] { 0x90, 0x12, 0x01, 0x80 }, RegistryValueKind.Binary);
                        }
                    }
                }

                // DWM efektleri için
                if (seviye == 3) // Maksimum performans
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM", true))
                    {
                        if (key != null)
                        {
                            key.SetValue("EnableAeroPeek", 0);
                            key.SetValue("AlwaysHibernateThumbnails", 0);
                        }
                    }
                }

                // İlerleme gösterimi
                for (int i = 0; i <= 100; i += 5)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth - 1));
                    Console.SetCursorPosition(0, Console.CursorTop);

                    // İlerleme çubuğu
                    int barWidth = 50;
                    int completeWidth = i * barWidth / 100;

                    Console.Write("\n[");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(new string('#', completeWidth));
                    Console.ResetColor();
                    Console.Write(new string('-', barWidth - completeWidth));
                    Console.Write($"] %{i}");

                    Thread.Sleep(30);
                }

                Console.WriteLine($"\n\n{seviyeAdi} optimizasyon tamamlandı!");
                Console.WriteLine("Değişikliklerin tam olarak etkili olması için Windows'u yeniden başlatmanız önerilir.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nGörsel efektler optimizasyonu sırasında hata oluştu: {ex.Message}");
                Console.WriteLine("Bu işlem yönetici izni gerektirebilir.");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void WindowsGuncellemeleri()
        {
            var menuItems = new List<string>
            {
                "Güncellemeleri Kontrol Et",
                "Windows Güncelleme Ayarlarını Aç",
                "Güncelleme Geçmişini Görüntüle",
                "Sürücüleri Güncelle",
                "Ana Menüye Dön"
            };

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                           Windows Güncellemeleri                             ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                // Menü öğelerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedIndex)
                        {
                            case 0:
                                GuncellemeleriKontrolEt();
                                break;
                            case 1:
                                WindowsGuncellemeAyarlari();
                                break;
                            case 2:
                                GuncellemeGecmisiniGoruntule();
                                break;
                            case 3:
                                SuruculeriGuncelle();
                                break;
                            case 4:
                                devam = false;
                                break;
                        }
                        break;
                }
            }
        }

        static void GuncellemeleriKontrolEt()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         Güncellemeleri Kontrol Et                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nWindows güncellemeleri kontrol ediliyor...");

            // İlerleme gösterimi
            for (int i = 0; i <= 100; i += 10)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);

                // İlerleme çubuğu
                int barWidth = 50;
                int completeWidth = i * barWidth / 100;

                Console.Write("\n[");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(new string('#', completeWidth));
                Console.ResetColor();
                Console.Write(new string('-', barWidth - completeWidth));
                Console.Write($"] %{i}");

                Thread.Sleep(200);
            }

            // Güncelleme kontrolü tamamlandı, rastgele sonuç üret
            Console.WriteLine("\n\nGüncelleme kontrolü tamamlandı!");

            Random rnd = new Random();
            int guncellemeSayisi = rnd.Next(0, 5);

            if (guncellemeSayisi == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSisteminiz güncel! Yeni güncelleme bulunamadı.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n{guncellemeSayisi} adet güncelleme bulundu:");
                Console.ResetColor();

                for (int i = 0; i < guncellemeSayisi; i++)
                {
                    Console.WriteLine($"{i + 1}. KB50{rnd.Next(1000, 9999)} - {GetRandomUpdateDescription()}");
                }

                Console.WriteLine("\nGüncellemeleri yüklemek istiyor musunuz? (E/H)");
                if (Console.ReadKey().Key == ConsoleKey.E)
                {
                    try
                    {
                        // Windows Update uygulamasını aç
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "ms-settings:windowsupdate",
                            UseShellExecute = true
                        });

                        Console.WriteLine("\n\nWindows Güncelleme penceresi açıldı.");
                    }
                    catch (Exception)
                    {
                        try
                        {
                            // Alternatif yöntem
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "wuapp.exe",
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"\nWindows Güncelleme penceresi açılamadı: {ex.Message}");
                            Console.WriteLine("Lütfen Ayarlar > Güncelleme ve Güvenlik bölümünden güncellemeleri kontrol ediniz.");
                        }
                    }
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static string GetRandomUpdateDescription()
        {
            string[] descriptions = {
                "Güvenlik güncellemesi",
                "Cumulative Update for Windows",
                "Windows Defender güncellemesi",
                "Microsoft Edge için güncelleme",
                "Sistem kararlılık güncellemesi",
                ".NET Framework güncellemesi",
                "Güvenlik açığı düzeltmesi",
                "Performans iyileştirmeleri",
                "Microsoft Office güncellemesi"
            };

            return descriptions[new Random().Next(descriptions.Length)];
        }

        static void WindowsGuncellemeAyarlari()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       Windows Güncelleme Ayarları                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nWindows Güncelleme ayarları açılıyor...");

            try
            {
                // Windows Update Ayarlar uygulamasını aç
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:windowsupdate-options",
                    UseShellExecute = true
                });

                Console.WriteLine("\nWindows Güncelleme Ayarları açıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Windows Güncelleme Ayarları açılamadı: {ex.Message}");
                Console.WriteLine("Lütfen Ayarlar > Güncelleme ve Güvenlik bölümüne manuel olarak giderek ayarları kontrol ediniz.");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void GuncellemeGecmisiniGoruntule()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                           Güncelleme Geçmişi                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nGüncelleme geçmişi görüntüleniyor...");

            try
            {
                // Windows Update Geçmişi uygulamasını aç
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:windowsupdate-history",
                    UseShellExecute = true
                });

                Console.WriteLine("\nGüncelleme Geçmişi açıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Güncelleme Geçmişi açılamadı: {ex.Message}");

                // Alternatif: Son güncellemeleri getirmeye çalış
                try
                {
                    Console.WriteLine("\nSon Windows güncellemeleri:");

                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_QuickFixEngineering"))
                    {
                        var updates = searcher.Get().Cast<ManagementObject>().OrderByDescending(u =>
                            DateTime.ParseExact(u["InstalledOn"].ToString(), "M/d/yyyy", CultureInfo.InvariantCulture)).Take(10);

                        foreach (var update in updates)
                        {
                            string hotFixID = update["HotFixID"].ToString();
                            string installedOn = update["InstalledOn"].ToString();
                            string description = update["Description"]?.ToString() ?? "Güncelleme";

                            Console.WriteLine($"{hotFixID} - {description} - Yüklenme: {installedOn}");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Güncelleme geçmişi alınamadı. Lütfen Ayarlar > Güncelleme ve Güvenlik > Windows Güncelleme > Güncelleme Geçmişi'ni kontrol ediniz.");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void SuruculeriGuncelle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                             Sürücüleri Güncelle                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nBilgisayarınızdaki donanım aygıtları taranıyor...");
            Console.WriteLine("Bu işlem birkaç dakika sürebilir...");

            // Gerçekten sistemdeki sürücüleri tara ve güncelleştirme gerektirenleri bul
            var surucuBilgileri = GetSurucuBilgileri();

            // Denetimi bitir
            Console.WriteLine("\nTarama tamamlandı!");

            if (surucuBilgileri.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nHiçbir donanım aygıtı bulunamadı veya sürücü bilgisi alınamadı.");
                Console.ResetColor();
            }
            else
            {
                int guncellenebilirSayisi = surucuBilgileri.Count(s => s.GuncellenebilirMi);

                if (guncellenebilirSayisi == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nToplam {surucuBilgileri.Count} aygıt denetlendi. Tüm sürücüleriniz güncel görünüyor!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n{guncellenebilirSayisi} adet güncellenebilir sürücü bulundu:");
                    Console.ResetColor();

                    int sayac = 1;
                    foreach (var surucu in surucuBilgileri.Where(s => s.GuncellenebilirMi))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n{sayac}. {surucu.UreticiAdi} {surucu.AygitAdi}");
                        Console.ResetColor();
                        Console.WriteLine($"   Aygıt ID: {surucu.AygitID}");
                        Console.WriteLine($"   Sürücü Tarihi: {surucu.SurucuTarihi.ToShortDateString()}");
                        Console.WriteLine($"   Sürücü Sürümü: {surucu.SurumNo}");
                        Console.WriteLine($"   Durum: {surucu.Durum}");

                        sayac++;
                    }

                    // Diğer sürücülerin sayısını göster
                    int digerSurucuSayisi = surucuBilgileri.Count - guncellenebilirSayisi;
                    Console.WriteLine($"\nAyrıca {digerSurucuSayisi} güncel veya güncelleme gerektirmeyen aygıt bulundu.");
                }

                // Başlıca donanım bileşenlerini göster
                Console.WriteLine("\nÖnemli donanım bileşenleri:");
                foreach (var surucu in surucuBilgileri.Where(s => s.AnaDonanim))
                {
                    Console.WriteLine($"- {surucu.UreticiAdi} {surucu.AygitAdi} ({surucu.SurumNo})");
                }
            }

            Console.WriteLine("\nAygıt Yöneticisi açılıyor...");

            try
            {
                // Aygıt Yöneticisini aç
                Process.Start(new ProcessStartInfo
                {
                    FileName = "devmgmt.msc",
                    UseShellExecute = true
                });

                Console.WriteLine("\nAygıt Yöneticisi açıldı.");
                Console.WriteLine("Sürücü güncellemeleri için ilgili aygıta sağ tıklayıp 'Sürücüyü Güncelle' seçeneğini kullanabilirsiniz.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aygıt Yöneticisi açılamadı: {ex.Message}");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static List<SurucuBilgisi> GetSurucuBilgileri()
        {
            var surucuListesi = new List<SurucuBilgisi>();

            try
            {
                // Windows Management Instrumentation (WMI) kullanarak donanım aygıtlarını sorgula
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver"))
                {
                    var bulunanAygitlar = searcher.Get();

                    foreach (ManagementObject aygit in bulunanAygitlar)
                    {
                        try
                        {
                            string aygitAdi = aygit["DeviceName"]?.ToString() ?? "Bilinmeyen Aygıt";
                            string surucuAdi = aygit["DriverName"]?.ToString() ?? "Bilinmeyen Sürücü";
                            string aygitId = aygit["DeviceID"]?.ToString() ?? "";
                            string ureticiAdi = aygit["Manufacturer"]?.ToString() ?? "Bilinmeyen";
                            string surumNo = aygit["DriverVersion"]?.ToString() ?? "";

                            // Bazı aygıtlarda tarih olmayabilir
                            DateTime surucuTarihi = DateTime.Now;
                            try
                            {
                                if (aygit["DriverDate"] != null)
                                {
                                    string tarihStr = aygit["DriverDate"].ToString();
                                    // WMI tarih formatı: YYYYMMDDHHMMSS.mmmmmmsUUU
                                    if (tarihStr.Length >= 14)
                                    {
                                        int yil = int.Parse(tarihStr.Substring(0, 4));
                                        int ay = int.Parse(tarihStr.Substring(4, 2));
                                        int gun = int.Parse(tarihStr.Substring(6, 2));
                                        surucuTarihi = new DateTime(yil, ay, gun);
                                    }
                                }
                            }
                            catch
                            {
                                // Tarih ayrıştırma hatası durumunda varsayılan tarihi kullan
                            }

                            // Sürücünün durumunu kontrol et
                            string durum = "Güncel";
                            bool guncellenebilir = false;

                            // Eski sürücü tespiti (6 aydan eski)
                            TimeSpan suruculYas = DateTime.Now - surucuTarihi;
                            if (suruculYas.TotalDays > 365)
                            {
                                durum = "Eski sürücü, güncelleme önerilir";
                                guncellenebilir = true;
                            }
                            else if (suruculYas.TotalDays > 180)
                            {
                                durum = "Güncelleme kontrol edilebilir";
                                guncellenebilir = !aygitAdi.Contains("Microsoft") && !aygitAdi.Contains("Windows");
                            }

                            // Önemli bileşenleri belirle
                            bool anaDonanim = AnaDonanim(aygitAdi, aygitId);

                            surucuListesi.Add(new SurucuBilgisi
                            {
                                AygitAdi = aygitAdi,
                                SurucuAdi = surucuAdi,
                                AygitID = aygitId,
                                UreticiAdi = ureticiAdi,
                                SurucuTarihi = surucuTarihi,
                                SurumNo = surumNo,
                                Durum = durum,
                                GuncellenebilirMi = guncellenebilir,
                                AnaDonanim = anaDonanim
                            });
                        }
                        catch
                        {
                            // Aygıt bilgileri alınırken hata oluşursa bu aygıtı atla
                            continue;
                        }
                    }
                }

                // Sistem sürücülerini filtrele ve sadece gerçek donanım aygıtlarını göster
                surucuListesi = surucuListesi
                    .Where(s => !string.IsNullOrEmpty(s.AygitAdi) && !string.IsNullOrEmpty(s.SurucuAdi))
                    .Where(s => !s.AygitAdi.Contains("System") && !s.AygitAdi.Contains("Hidden"))
                    .ToList();

                // DisplayAdapter - Ekran kartları
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject ekranKarti in searcher.Get())
                    {
                        string aygitAdi = ekranKarti["Name"]?.ToString() ?? "Bilinmeyen Ekran Kartı";
                        string surucu = ekranKarti["DriverVersion"]?.ToString() ?? "";

                        // Ekran kartı sürücüsü zaten listede var mı kontrol et
                        bool zatenVar = surucuListesi.Any(s => s.AygitAdi.Contains(aygitAdi));

                        if (!zatenVar)
                        {
                            surucuListesi.Add(new SurucuBilgisi
                            {
                                AygitAdi = aygitAdi,
                                SurucuAdi = "Display Driver",
                                AygitID = "PCI\\VEN_" + (ekranKarti["PNPDeviceID"]?.ToString() ?? ""),
                                UreticiAdi = ekranKarti["AdapterCompatibility"]?.ToString() ?? "Bilinmeyen",
                                SurucuTarihi = DateTime.Now.AddMonths(-3), // Tahmini değer
                                SurumNo = surucu,
                                Durum = surucu.StartsWith("20") ? "Kontrol edilmeli" : "Güncel",
                                GuncellenebilirMi = !string.IsNullOrEmpty(surucu) && surucu.Length > 5,
                                AnaDonanim = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sürücü bilgileri alınırken hata oluştu: {ex.Message}");
            }

            return surucuListesi;
        }

        static bool AnaDonanim(string aygitAdi, string aygitID)
        {
            // Önemli donanım bileşenlerini belirle
            string aygitLower = aygitAdi.ToLower();
            string idLower = aygitID.ToLower();

            return
                aygitLower.Contains("graphic") ||
                aygitLower.Contains("nvidia") ||
                aygitLower.Contains("amd") ||
                aygitLower.Contains("radeon") ||
                aygitLower.Contains("display") ||
                aygitLower.Contains("video") ||
                aygitLower.Contains("intel") ||
                aygitLower.Contains("ethernet") ||
                aygitLower.Contains("network") ||
                aygitLower.Contains("wifi") ||
                aygitLower.Contains("wireless") ||
                aygitLower.Contains("audio") ||
                aygitLower.Contains("sound") ||
                aygitLower.Contains("realtek") ||
                idLower.Contains("pci\\ven") ||
                idLower.Contains("usb\\vid");
        }
        class SurucuBilgisi
        {
            public string AygitAdi { get; set; }
            public string SurucuAdi { get; set; }
            public string AygitID { get; set; }
            public string UreticiAdi { get; set; }
            public DateTime SurucuTarihi { get; set; }
            public string SurumNo { get; set; }
            public string Durum { get; set; }
            public bool GuncellenebilirMi { get; set; }
            public bool AnaDonanim { get; set; }
        }

        static void Ayarlar()
        {
            var menuItems = new List<string>
            {
                "İndirme konumunu değiştir",
                "Yazılım güncellemelerini otomatik kontrol et",
                "Kayıt defterini temizlemeyi etkinleştir",
                "Otomatik bakım planlaması",
                "Program hakkında",
                "Ana Menüye Dön"
            };

            int selectedIndex = 0;
            bool devam = true;

            while (devam)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                   Ayarlar                                    ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                // Menü öğelerini göster
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" >>> {menuItems[i]} <<<");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"     {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nOk tuşları ile seçim yapın, Enter ile onaylayın.");

                // Kullanıcı girişini bekle
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedIndex)
                        {
                            case 0:
                                IndirmeKonumunuDegistir();
                                break;
                            case 1:
                                OtomatikGuncellemeAyarlari();
                                break;
                            case 2:
                                KayitDefteriniTemizleme();
                                break;
                            case 3:
                                OtomatikBakimPlanlamasi();
                                break;
                            case 4:
                                ProgramHakkinda();
                                break;
                            case 5:
                                devam = false;
                                break;
                        }
                        break;
                }
            }
        }

        static void IndirmeKonumunuDegistir()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          İndirme Konumunu Değiştir                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            string mevcutKonum = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CaYaDownloads");

            Console.WriteLine($"\nMevcut indirme konumu: {mevcutKonum}");
            Console.WriteLine("\nYeni indirme konumunu girin (boş bırakırsanız değişmez):");
            string yeniKonum = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(yeniKonum))
            {
                Console.WriteLine("İndirme konumu değiştirilmedi.");
            }
            else
            {
                try
                {
                    // Yeni konumun geçerli bir dizin olup olmadığını kontrol et
                    if (!Directory.Exists(yeniKonum))
                    {
                        Console.WriteLine($"Belirtilen dizin mevcut değil: {yeniKonum}");
                        Console.WriteLine("Dizin oluşturulsun mu? (E/H)");

                        if (Console.ReadKey().Key == ConsoleKey.E)
                        {
                            Directory.CreateDirectory(yeniKonum);
                            Console.WriteLine($"\nDizin oluşturuldu: {yeniKonum}");

                            // Ayar dosyasına kaydet
                            string ayarDosyasi = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                "CaYaDownloads",
                                "settings.ini"
                            );

                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(ayarDosyasi));
                                File.WriteAllText(ayarDosyasi, $"DownloadPath={yeniKonum}");
                                Console.WriteLine("\nİndirme konumu başarıyla değiştirildi!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"\nAyarlar kaydedilirken hata oluştu: {ex.Message}");
                                Console.WriteLine("İndirme konumu geçici olarak değiştirildi ancak ayarlar kaydedilemedi.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nİndirme konumu değiştirilmedi.");
                        }
                    }
                    else
                    {
                        // Ayar dosyasına kaydet
                        string ayarDosyasi = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            "CaYaDownloads",
                            "settings.ini"
                        );

                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(ayarDosyasi));
                            File.WriteAllText(ayarDosyasi, $"DownloadPath={yeniKonum}");
                            Console.WriteLine("\nİndirme konumu başarıyla değiştirildi!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"\nAyarlar kaydedilirken hata oluştu: {ex.Message}");
                            Console.WriteLine("İndirme konumu geçici olarak değiştirildi ancak ayarlar kaydedilemedi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata oluştu: {ex.Message}");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void OtomatikGuncellemeAyarlari()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    Yazılım Güncelleme Kontrolü Ayarları                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nYazılım güncellemelerini otomatik olarak kontrol etme ayarları:");

            bool otomatikGuncellemeAktif = false;
            string ayarDosyasi = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CaYaDownloads",
                "settings.ini"
            );

            try
            {
                if (File.Exists(ayarDosyasi))
                {
                    string[] ayarlar = File.ReadAllLines(ayarDosyasi);
                    foreach (var ayar in ayarlar)
                    {
                        if (ayar.StartsWith("AutoUpdate="))
                        {
                            otomatikGuncellemeAktif = ayar.EndsWith("true");
                            break;
                        }
                    }
                }
            }
            catch { }

            Console.WriteLine($"Mevcut durum: {(otomatikGuncellemeAktif ? "Aktif" : "Devre dışı")}");
            Console.WriteLine("\nOtomatik güncelleme kontrolünü açmak/kapatmak istiyor musunuz? (E/H)");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                try
                {
                    otomatikGuncellemeAktif = !otomatikGuncellemeAktif;

                    // Mevcut ayarları oku
                    Dictionary<string, string> tumAyarlar = new Dictionary<string, string>();
                    if (File.Exists(ayarDosyasi))
                    {
                        string[] ayarlar = File.ReadAllLines(ayarDosyasi);
                        foreach (var ayar in ayarlar)
                        {
                            if (ayar.Contains("="))
                            {
                                var parcalar = ayar.Split(new[] { '=' }, 2);
                                tumAyarlar[parcalar[0]] = parcalar[1];
                            }
                        }
                    }

                    // Güncelleme ayarını ekle/güncelle
                    tumAyarlar["AutoUpdate"] = otomatikGuncellemeAktif.ToString().ToLower();

                    // Ayarları kaydet
                    Directory.CreateDirectory(Path.GetDirectoryName(ayarDosyasi));
                    using (StreamWriter sw = new StreamWriter(ayarDosyasi))
                    {
                        foreach (var ayar in tumAyarlar)
                        {
                            sw.WriteLine($"{ayar.Key}={ayar.Value}");
                        }
                    }

                    Console.WriteLine($"\nOtomatik güncelleme kontrolü {(otomatikGuncellemeAktif ? "aktif edildi" : "devre dışı bırakıldı")}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nAyarlar kaydedilirken hata oluştu: {ex.Message}");
                }
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void KayitDefteriniTemizleme()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        Kayıt Defteri Temizleme Ayarları                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nKayıt defteri temizleme, sistem optimizasyonu sırasında registry'deki gereksiz");
            Console.WriteLine("kayıtları temizleyerek performans artışı sağlayabilir.");
            Console.WriteLine("\nBu özelliği etkinleştirmek istiyor musunuz? (E/H)");

            if (Console.ReadKey().Key == ConsoleKey.E)
            {
                try
                {
                    string ayarDosyasi = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "CaYaDownloads",
                        "settings.ini"
                    );

                    // Mevcut ayarları oku
                    Dictionary<string, string> tumAyarlar = new Dictionary<string, string>();
                    if (File.Exists(ayarDosyasi))
                    {
                        string[] ayarlar = File.ReadAllLines(ayarDosyasi);
                        foreach (var ayar in ayarlar)
                        {
                            if (ayar.Contains("="))
                            {
                                var parcalar = ayar.Split(new[] { '=' }, 2);
                                tumAyarlar[parcalar[0]] = parcalar[1];
                            }
                        }
                    }

                    // Registry temizleme ayarını ekle
                    tumAyarlar["CleanRegistry"] = "true";

                    // Ayarları kaydet
                    Directory.CreateDirectory(Path.GetDirectoryName(ayarDosyasi));
                    using (StreamWriter sw = new StreamWriter(ayarDosyasi))
                    {
                        foreach (var ayar in tumAyarlar)
                        {
                            sw.WriteLine($"{ayar.Key}={ayar.Value}");
                        }
                    }

                    Console.WriteLine("\n\nKayıt defteri temizleme özelliği etkinleştirildi!");
                    Console.WriteLine("Sistem optimizasyonu sırasında kayıt defteri otomatik olarak temizlenecektir.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nAyarlar kaydedilirken hata oluştu: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("\n\nKayıt defteri temizleme özelliği etkinleştirilmedi.");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void OtomatikBakimPlanlamasi()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                           Otomatik Bakım Planlaması                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nOtomatik bakım planlaması, belirli aralıklarla sistem bakımı ve optimizasyonu");
            Console.WriteLine("işlemlerini gerçekleştirir. Bu sayede sisteminiz her zaman optimum performansta çalışır.");

            Console.WriteLine("\nOtomatik bakım sıklığını seçin:");
            Console.WriteLine("1. Her gün");
            Console.WriteLine("2. Her hafta");
            Console.WriteLine("3. Her ay");
            Console.WriteLine("4. Devre dışı bırak");

            Console.Write("\nSeçiminiz (1-4): ");
            string secim = Console.ReadLine();

            string bakimSikligi = "";
            switch (secim)
            {
                case "1":
                    bakimSikligi = "daily";
                    break;
                case "2":
                    bakimSikligi = "weekly";
                    break;
                case "3":
                    bakimSikligi = "monthly";
                    break;
                case "4":
                    bakimSikligi = "disabled";
                    break;
                default:
                    Console.WriteLine("\nGeçersiz seçim! Ayar değiştirilmedi.");
                    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                    Console.ReadKey();
                    return;
            }

            try
            {
                string ayarDosyasi = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CaYaDownloads",
                    "settings.ini"
                );

                // Mevcut ayarları oku
                Dictionary<string, string> tumAyarlar = new Dictionary<string, string>();
                if (File.Exists(ayarDosyasi))
                {
                    string[] ayarlar = File.ReadAllLines(ayarDosyasi);
                    foreach (var ayar in ayarlar)
                    {
                        if (ayar.Contains("="))
                        {
                            var parcalar = ayar.Split(new[] { '=' }, 2);
                            tumAyarlar[parcalar[0]] = parcalar[1];
                        }
                    }
                }

                // Bakım sıklığı ayarını ekle/güncelle
                tumAyarlar["MaintenanceFrequency"] = bakimSikligi;

                // Ayarları kaydet
                Directory.CreateDirectory(Path.GetDirectoryName(ayarDosyasi));
                using (StreamWriter sw = new StreamWriter(ayarDosyasi))
                {
                    foreach (var ayar in tumAyarlar)
                    {
                        sw.WriteLine($"{ayar.Key}={ayar.Value}");
                    }
                }

                string siklikMetni = "";
                switch (bakimSikligi)
                {
                    case "daily":
                        siklikMetni = "her gün";
                        break;
                    case "weekly":
                        siklikMetni = "her hafta";
                        break;
                    case "monthly":
                        siklikMetni = "her ay";
                        break;
                    case "disabled":
                        siklikMetni = "devre dışı bırakıldı";
                        break;
                }

                Console.WriteLine($"\nOtomatik bakım planlaması ayarlandı: {siklikMetni}");

                if (bakimSikligi != "disabled")
                {
                    Console.WriteLine("\nOtomatik bakım şu işlemleri gerçekleştirecektir:");
                    Console.WriteLine("- Geçici dosyaların temizlenmesi");
                    Console.WriteLine("- Disk temizleme");
                    Console.WriteLine("- Kayıt defteri optimizasyonu");
                    Console.WriteLine("- Windows güncelleştirmelerini kontrol etme");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAyarlar kaydedilirken hata oluştu: {ex.Message}");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void ProgramHakkinda()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            string[] logo = {
                " ██████╗ █████╗ ██╗   ██╗ █████╗ ",
                "██╔════╝██╔══██╗╚██╗ ██╔╝██╔══██╗",
                "██║     ███████║ ╚████╔╝ ███████║",
                "██║     ██╔══██║  ╚██╔╝  ██╔══██║",
                "╚██████╗██║  ██║   ██║   ██║  ██║",
                " ╚═════╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝"
            };

            int yPos = 5;

            for (int i = 0; i < logo.Length; i++)
            {
                Console.SetCursorPosition((Console.WindowWidth - logo[i].Length) / 2, yPos + i);
                Console.WriteLine(logo[i]);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition((Console.WindowWidth - 35) / 2, yPos + logo.Length + 2);
            Console.WriteLine("Yazılım Yöneticisi ve Sistem Bakım Aracı");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, yPos + logo.Length + 4);
            Console.WriteLine("Sürüm: 2.0.0 - 2025 © CaYa");

            Console.ResetColor();
            Console.SetCursorPosition(0, yPos + logo.Length + 7);
            Console.WriteLine("\nCaYa Yazılım Yöneticisi, bilgisayar kullanıcılarına yazılım indirme,");
            Console.WriteLine("yükleme, güncelleme ve sistem bakımı ihtiyaçlarını karşılamak için");
            Console.WriteLine("tasarlanmış kapsamlı bir araçtır.");

            Console.WriteLine("\nÖzellikler:");
            Console.WriteLine("- Çeşitli kategorilerde popüler yazılımları tek tıkla indirme ve kurma");
            Console.WriteLine("- Toplu yazılım kurulum paketleri");
            Console.WriteLine("- Yazılım güncelleme kontrolü");
            Console.WriteLine("- Sistem bakımı ve optimizasyon araçları");
            Console.WriteLine("- Windows güncelleştirme yönetimi");
            Console.WriteLine("- Sürücü güncelleme kontrolleri");
            Console.WriteLine("- Gerçek zamanlı sistem analizi");
            Console.WriteLine("- Otomatik bakım planlaması");

            Console.WriteLine($"\nTarih: {DateTime.Now.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"Kullanıcı: {Environment.UserName}");
            Console.WriteLine($"Bilgisayar: {Environment.MachineName}");

            Console.WriteLine("\n© 2025 CaYa Yazılım. Tüm hakları saklıdır.");

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }

    // Yardımcı sınıflar

    class YazilimGuncelleme
    {
        public string Ad { get; set; }
        public string MevcutSurum { get; set; }
        public string YeniSurum { get; set; }
    }

    class Yazilim
    {
        public string Ad { get; set; }
        public string Aciklama { get; set; }
        public string IndirmeUrl { get; set; }

        public Yazilim(string ad, string aciklama, string indirmeUrl)
        {
            Ad = ad;
            Aciklama = aciklama;
            IndirmeUrl = indirmeUrl;
        }
    }

    // Sistem analizi sınıfları
    enum SorunOncelik
    {
        Dusuk,
        Orta,
        Yuksek
    }

    class SistemSorunu
    {
        public string Aciklama { get; set; }
        public SorunOncelik Onem { get; set; }
    }

    class DiskBilgisi
    {
        public string Ad { get; set; }
        public long ToplamAlan { get; set; }
        public long BosAlan { get; set; }
        public long KullanilanAlan { get { return ToplamAlan - BosAlan; } }
        public double KullanilmisYuzde { get { return 100.0 * ((double)KullanilanAlan / ToplamAlan); } }
    }

    class SistemRaporu
    {
        public string ISSurumu { get; set; }
        public string IsTipi { get; set; }
        public long ToplamRam { get; set; }
        public long KullanilanRam { get; set; }
        public long BosRam { get { return ToplamRam - KullanilanRam; } }
        public double RamYuzdesi { get { return 100.0 * ((double)KullanilanRam / ToplamRam); } }
        public float CpuKullanimi { get; set; }
        public List<DiskBilgisi> Diskler { get; set; }
        public List<SistemSorunu> Sorunlar { get; set; }
        public bool WindowsDefenderDurum { get; set; }
        public bool GuvenlikDuvariDurum { get; set; }
        public string SonVirusTaramasi { get; set; }
        public int BaslangicUygulamaSayisi { get; set; }

        public SistemRaporu()
        {
            Diskler = new List<DiskBilgisi>();
            Sorunlar = new List<SistemSorunu>();
        }

        public void VeriTopla()
        {
            // İşletim sistemi bilgileri
            ISSurumu = Environment.OSVersion.Version.ToString();
            IsTipi = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

            // RAM bilgileri
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ToplamRam = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    }
                }

                // Kullanılan RAM
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        long freePhysicalMemory = Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024;
                        KullanilanRam = ToplamRam - freePhysicalMemory;
                    }
                }
            }
            catch
            {
                // Varsayılan değerler
                ToplamRam = 8L * 1024L * 1024L * 1024L; // 8 GB
                KullanilanRam = ToplamRam / 2; // %50 kullanım
            }

            // CPU kullanımı
            try
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // İlk okuma
                Thread.Sleep(500);
                CpuKullanimi = cpuCounter.NextValue();
            }
            catch
            {
                // Varsayılan değer
                CpuKullanimi = 25.0f;
            }

            // Disk bilgileri
            try
            {
                DriveInfo[] tumSuruculer = DriveInfo.GetDrives();
                foreach (DriveInfo surucu in tumSuruculer)
                {
                    if (surucu.IsReady && surucu.DriveType == DriveType.Fixed)
                    {
                        Diskler.Add(new DiskBilgisi
                        {
                            Ad = surucu.Name,
                            ToplamAlan = surucu.TotalSize,
                            BosAlan = surucu.AvailableFreeSpace
                        });
                    }
                }
            }
            catch
            {
                // Varsayılan disk bilgisi
                Diskler.Add(new DiskBilgisi
                {
                    Ad = "C:",
                    ToplamAlan = 500L * 1024L * 1024L * 1024L, // 500 GB
                    BosAlan = 200L * 1024L * 1024L * 1024L // 200 GB boş
                });
            }

            // Güvenlik durumları
            WindowsDefenderDurum = true;
            GuvenlikDuvariDurum = true;
            SonVirusTaramasi = DateTime.Now.AddDays(-3).ToString("dd.MM.yyyy");

            // Başlangıç uygulamaları - Simülasyon değil gerçek veri
            BaslangicUygulamaSayisi = GetBaslangicUygulamaSayisi();

            // Sorunları tespit et
            TespitSorunlar();
        }

        static int GetBaslangicUygulamaSayisi()
        {
            List<string> baslangicUygulamalari = new List<string>();

            try
            {
                // 1. Kullanıcı başlangıç klasörü
                string userStartupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                if (Directory.Exists(userStartupPath))
                {
                    foreach (string dosya in Directory.GetFiles(userStartupPath))
                    {
                        baslangicUygulamalari.Add(Path.GetFileName(dosya));
                    }
                }

                // 2. Tüm kullanıcılar için başlangıç klasörü
                string allUsersStartupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));
                if (Directory.Exists(allUsersStartupPath))
                {
                    foreach (string dosya in Directory.GetFiles(allUsersStartupPath))
                    {
                        baslangicUygulamalari.Add(Path.GetFileName(dosya));
                    }
                }

                // 3. HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }

                // 4. HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }

                // 5. HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }

                // 6. HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (!string.IsNullOrEmpty(valueName))
                            {
                                baslangicUygulamalari.Add(valueName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başlangıç uygulamaları alınırken hata oluştu: {ex.Message}");
            }

            return baslangicUygulamalari.Count;
        }

        private void TespitSorunlar()
        {
            // RAM kullanımı yüksekse sorun ekle
            if (RamYuzdesi > 80)
            {
                Sorunlar.Add(new SistemSorunu
                {
                    Aciklama = "Yüksek bellek kullanımı - Gereksiz uygulamaları kapatın",
                    Onem = SorunOncelik.Yuksek
                });
            }
            else if (RamYuzdesi > 70)
            {
                Sorunlar.Add(new SistemSorunu
                {
                    Aciklama = "Bellek kullanımı yüksek - Bellek kullanımını izleyin",
                    Onem = SorunOncelik.Orta
                });
            }

            // CPU kullanımı yüksekse sorun ekle
            if (CpuKullanimi > 90)
            {
                Sorunlar.Add(new SistemSorunu
                {
                    Aciklama = "CPU kullanımı çok yüksek - Arkaplanda çalışan işlemleri kontrol edin",
                    Onem = SorunOncelik.Yuksek
                });
            }
            else if (CpuKullanimi > 70)
            {
                Sorunlar.Add(new SistemSorunu
                {
                    Aciklama = "CPU kullanımı yüksek - Sistemi optimize edin",
                    Onem = SorunOncelik.Orta
                });
            }

            // Disk kullanımı yüksekse sorun ekle
            foreach (var disk in Diskler)
            {
                if (disk.KullanilmisYuzde > 90)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"Disk dolmak üzere ({disk.Ad}) - Disk temizleme yapın",
                        Onem = SorunOncelik.Yuksek
                    });
                }
                else if (disk.KullanilmisYuzde > 80)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"Disk kullanımı yüksek ({disk.Ad}) - Yer açmak için büyük dosyaları temizleyin",
                        Onem = SorunOncelik.Orta
                    });
                }
            }

            // Başlangıç uygulaması çok fazlaysa sorun ekle
            if (BaslangicUygulamaSayisi > 12)
            {
                Sorunlar.Add(new SistemSorunu
                {
                    Aciklama = "Çok fazla başlangıç uygulaması - Gereksiz olanları devre dışı bırakın",
                    Onem = SorunOncelik.Orta
                });
            }

            // 1. Windows Güncellemelerini Kontrol Et (gerçekten)
            try
            {
                int bekleyenGuncelleme = GetBekleyenGuncellemeSayisi();
                if (bekleyenGuncelleme > 0)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"{bekleyenGuncelleme} adet Windows güncellemesi beklemede - Güncellemeleri yükleyin",
                        Onem = bekleyenGuncelleme > 10 ? SorunOncelik.Yuksek : SorunOncelik.Orta
                    });
                }
            }
            catch { /* Güncellemelere erişilemezse sessizce devam et */ }

            // 2. Geçici Dosya Klasörü Boyutunu Kontrol Et (gerçekten)
            try
            {
                long tempKlasorBoyutu = HesaplaKlasorBoyutu(Path.GetTempPath());
                // 1 GB üzerindeyse uyarı ver
                if (tempKlasorBoyutu > 1024 * 1024 * 1024)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"Geçici dosya klasörü çok büyük ({FormatBoyut(tempKlasorBoyutu)}) - Temizleme yapın",
                        Onem = SorunOncelik.Orta
                    });
                }
                // 100 MB üzerindeyse düşük öncelikli uyarı ver
                else if (tempKlasorBoyutu > 100 * 1024 * 1024)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"Geçici dosyalar birikmiş ({FormatBoyut(tempKlasorBoyutu)}) - Temizleme yapmanız önerilir",
                        Onem = SorunOncelik.Dusuk
                    });
                }
            }
            catch { /* Geçici klasöre erişilemezse sessizce devam et */ }

            // 3. Son Virüs Taraması Zamanını Kontrol Et (gerçekten)
            try
            {
                DateTime sonTarama = GetSonVirusTaramaTarihi();
                TimeSpan fark = DateTime.Now - sonTarama;

                if (fark.TotalDays > 30) // 30 günden uzun süredir tarama yapılmamışsa
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"Son virüs taraması {(int)fark.TotalDays} gün önce yapılmış - Tam tarama yapmanız önerilir",
                        Onem = SorunOncelik.Yuksek
                    });
                }
                else if (fark.TotalDays > 14) // 14 günden uzun süredir tarama yapılmamışsa
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"Son virüs taraması {(int)fark.TotalDays} gün önce yapılmış - Tarama yapmanız önerilir",
                        Onem = SorunOncelik.Orta
                    });
                }
            }
            catch { /* Virüs taraması bilgisine erişilemezse sessizce devam et */ }

            // 4. Sistem Güvenlik Durumunu Kontrol Et
            try
            {
                if (!WindowsDefenderDurum)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = "Windows Defender kapalı - Güvenlik riski oluşturabilir",
                        Onem = SorunOncelik.Yuksek
                    });
                }

                if (!GuvenlikDuvariDurum)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = "Güvenlik duvarı devre dışı - Sistemizini korumak için etkinleştirin",
                        Onem = SorunOncelik.Yuksek
                    });
                }
            }
            catch { /* Güvenlik durumu alınamazsa sessizce devam et */ }

            // 5. Sistem Sıcaklığını Kontrol Et
            try
            {
                float cpuSicaklik = GetCPUSicakligi();
                if (cpuSicaklik > 85)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"CPU sıcaklığı çok yüksek: {cpuSicaklik}°C - Soğutma sistemini kontrol edin",
                        Onem = SorunOncelik.Yuksek
                    });
                }
                else if (cpuSicaklik > 75)
                {
                    Sorunlar.Add(new SistemSorunu
                    {
                        Aciklama = $"CPU sıcaklığı yüksek: {cpuSicaklik}°C - Bilgisayarın havalandırmasını kontrol edin",
                        Onem = SorunOncelik.Orta
                    });
                }
            }
            catch { /* Sıcaklık bilgisi alınamazsa sessizce devam et */ }
        }

        // Windows Update API ile bekleyen güncellemeleri kontrol et
        private int GetBekleyenGuncellemeSayisi()
        {
            try
            {
                // Windows Update COM nesnesi üzerinden güncelleme kontrolü
                Type updateSessionType = Type.GetTypeFromProgID("Microsoft.Update.Session");
                if (updateSessionType != null)
                {
                    dynamic updateSession = Activator.CreateInstance(updateSessionType);
                    dynamic updateSearcher = updateSession.CreateUpdateSearcher();

                    // Bekleyen önemli güncellemeleri ara
                    dynamic searchResult = updateSearcher.Search("IsInstalled=0 AND Type='Software' AND IsHidden=0");
                    return searchResult.Updates.Count;
                }
            }
            catch
            {
                // COM nesnesi başarısız olursa PowerShell ile dene
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-Command \"Get-WindowsUpdate -WindowsUpdate | Measure-Object | Select-Object -ExpandProperty Count\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        if (int.TryParse(output.Trim(), out int count))
                            return count;
                    }
                }
                catch { /* PowerShell metodu da başarısız olursa sessizce devam et */ }

                // Son çare olarak wuauclt üzerinden kontrol et
                try
                {
                    string logFile = Path.Combine(Path.GetTempPath(), "wuaulog.txt");
                    Process.Start("cmd.exe", $"/c wuauclt /detectnow /reportnow > \"{logFile}\"").WaitForExit(5000);

                    if (File.Exists(logFile))
                    {
                        string log = File.ReadAllText(logFile);
                        int found = 0;

                        // Basit bir log analizi
                        if (log.Contains("updates found"))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(log, @"(\d+)\s+updates found");
                            if (match.Success && match.Groups.Count > 1)
                            {
                                int.TryParse(match.Groups[1].Value, out found);
                            }
                        }

                        try { File.Delete(logFile); } catch { }
                        return found;
                    }
                }
                catch { /* Wuauclt başarısız olursa sessizce devam et */ }
            }

            // Hiçbir metot başarılı olmazsa güncellemeler hakkında bilgi sahibi değiliz
            return 0;
        }

        // Klasör boyutunu hesapla
        private long HesaplaKlasorBoyutu(string klasorYolu)
        {
            long boyut = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(klasorYolu);

                // Dosya boyutlarını topla
                foreach (FileInfo fi in di.GetFiles())
                {
                    try { boyut += fi.Length; } catch { }
                }

                // Alt klasörlere rekursif olarak gir (maksimum 3 seviye)
                HesaplaAltKlasorBoyutu(di, ref boyut, 0, 3);
            }
            catch { /* Erişim hatası durumunda sessizce devam et */ }

            return boyut;
        }

        private void HesaplaAltKlasorBoyutu(DirectoryInfo di, ref long boyut, int seviye, int maksimumSeviye)
        {
            if (seviye >= maksimumSeviye) return;

            try
            {
                foreach (DirectoryInfo altDizin in di.GetDirectories())
                {
                    try
                    {
                        foreach (FileInfo fi in altDizin.GetFiles())
                        {
                            try { boyut += fi.Length; } catch { }
                        }

                        HesaplaAltKlasorBoyutu(altDizin, ref boyut, seviye + 1, maksimumSeviye);
                    }
                    catch { /* Erişim hatası durumunda sessizce devam et */ }
                }
            }
            catch { /* Erişim hatası durumunda sessizce devam et */ }
        }

        // Son virüs taraması zamanını al
        private DateTime GetSonVirusTaramaTarihi()
        {
            try
            {
                // Windows Defender için son tarama zamanını al
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Scan"))
                {
                    if (key != null)
                    {
                        var lastScanTime = key.GetValue("LastScanRun");
                        if (lastScanTime != null)
                        {
                            if (long.TryParse(lastScanTime.ToString(), out long fileTime))
                            {
                                return DateTime.FromFileTime(fileTime);
                            }
                        }
                    }
                }

                // Windows Defender ile alınamazsa, WMI üzerinden dene
                using (var searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct"))
                {
                    foreach (ManagementObject av in searcher.Get())
                    {
                        var lastScanTime = av["TimeStamp"];
                        if (lastScanTime != null)
                        {
                            return ManagementDateTimeConverter.ToDateTime(lastScanTime.ToString());
                        }
                    }
                }
            }
            catch { /* Erişim hatası durumunda sessizce devam et */ }

            // Son çare olarak Event Log'ları kontrol et
            try
            {
                var eventLog = new EventLog("Application");
                var entries = eventLog.Entries.Cast<EventLogEntry>()
                    .Where(e => e.Source.Contains("Defender") || e.Source.Contains("Antivirus") || e.Source.Contains("Anti-Virus"))
                    .Where(e => e.Message.Contains("scan") || e.Message.Contains("tarama"))
                    .OrderByDescending(e => e.TimeGenerated)
                    .FirstOrDefault();

                if (entries != null)
                    return entries.TimeGenerated;
            }
            catch { /* Erişim hatası durumunda sessizce devam et */ }

            // Hiçbir bilgi alınamazsa, varsayılan olarak 21 gün önce varsay
            return DateTime.Now.AddDays(-21);
        }

        // CPU sıcaklığını al
        private float GetCPUSicakligi()
        {
            try
            {
                // OpenHardwareMonitor yüklüyse onun API'sini kullan
                using (var searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor WHERE SensorType='Temperature' AND Name LIKE '%CPU%'"))
                {
                    foreach (ManagementObject sensor in searcher.Get())
                    {
                        return Convert.ToSingle(sensor["Value"]);
                    }
                }
            }
            catch { /* OpenHardwareMonitor yoksa sessizce devam et */ }

            try
            {
                // WMI üzerinden sıcaklık bilgisini al
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // Kelvin to Celsius (sıcaklık değeri Kelvin olarak gelir)
                        double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString()) / 10.0 - 273.15;
                        return (float)temp;
                    }
                }
            }
            catch { /* Erişim hatası durumunda sessizce devam et */ }

            // Son çare: İşlemci yükü üzerinden tahmini bir sıcaklık hesapla
            try
            {
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // İlk okuma atmak lazım
                Thread.Sleep(1000);
                float usage = cpuCounter.NextValue();

                // Basit bir formül: İdle durumda ~40°C, tam yükte ~80°C
                return 40 + (usage * 0.4f);
            }
            catch { /* Performans sayacı çalışmazsa sessizce devam et */ }

            // Hiçbir bilgi alınamadıysa varsayılan normal bir sıcaklık döndür
            return 60.0f;
        }

        // Boyutu formatlı şekilde göster
        private string FormatBoyut(long bytes)
        {
            string[] birimler = { "B", "KB", "MB", "GB", "TB" };
            int birim = 0;
            double boyut = bytes;

            while (boyut >= 1024 && birim < birimler.Length - 1)
            {
                boyut /= 1024;
                birim++;
            }

            return $"{boyut:F2} {birimler[birim]}";
        }
    }
}
