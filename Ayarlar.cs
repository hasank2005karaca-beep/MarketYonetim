using System;
using System.IO;
using System.Data.SqlClient;

namespace MarketYonetim
{
    // Ayarlar - Statik sınıf
    public static class Ayarlar
    {
        private static string _connectionString;
        private static readonly string ayarDosyasi = Path.Combine(AppContext.BaseDirectory, "ayarlar.ini");

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    YukleAyarlar();
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
                KaydetAyarlar();
            }
        }

        public static string SunucuAdi { get; set; } = "";
        public static string Port { get; set; } = "";
        public static string Instance { get; set; } = "";
        public static string VeritabaniAdi { get; set; } = "BUS2020";
        public static string KullaniciAdi { get; set; } = "";
        public static string Sifre { get; set; } = "";
        public static bool WindowsAuth { get; set; } = false;
        public static string DepoKodu { get; set; } = "001";
        public static string KasiyerRumuzu { get; set; } = "KASIYER1";
        public static decimal VarsayilanKdvOrani { get; set; } = 20m;

        public static bool AyarDosyasiVarMi()
        {
            return File.Exists(ayarDosyasi);
        }

        public static void YukleAyarlar()
        {
            try
            {
                if (File.Exists(ayarDosyasi))
                {
                    string[] satirlar = File.ReadAllLines(ayarDosyasi);
                    foreach (string satir in satirlar)
                    {
                        string[] parcalar = satir.Split('=');
                        if (parcalar.Length == 2)
                        {
                            string anahtar = parcalar[0].Trim();
                            string deger = parcalar[1].Trim();

                            switch (anahtar)
                            {
                                case "SunucuAdi": SunucuAdi = deger; break;
                                case "Port": Port = deger; break;
                                case "Instance": Instance = deger; break;
                                case "VeritabaniAdi": VeritabaniAdi = deger; break;
                                case "KullaniciAdi": KullaniciAdi = deger; break;
                                case "Sifre": Sifre = deger; break;
                                case "WindowsAuth": WindowsAuth = deger.Equals("True", StringComparison.OrdinalIgnoreCase); break;
                                case "DepoKodu": DepoKodu = deger; break;
                                case "KasiyerRumuzu": KasiyerRumuzu = deger; break;
                                case "VarsayilanKdvOrani":
                                    if (decimal.TryParse(deger, out decimal kdvOrani))
                                    {
                                        VarsayilanKdvOrani = kdvOrani;
                                    }
                                    break;
                            }
                        }
                    }
                }

                ConnectionStringOlustur();
            }
            catch
            {
                ConnectionStringOlustur();
            }
        }

        public static void KaydetAyarlar()
        {
            try
            {
                string[] satirlar = new string[]
                {
                    $"SunucuAdi={SunucuAdi}",
                    $"Port={Port}",
                    $"Instance={Instance}",
                    $"VeritabaniAdi={VeritabaniAdi}",
                    $"KullaniciAdi={KullaniciAdi}",
                    $"Sifre={Sifre}",
                    $"WindowsAuth={WindowsAuth}",
                    $"DepoKodu={DepoKodu}",
                    $"KasiyerRumuzu={KasiyerRumuzu}",
                    $"VarsayilanKdvOrani={VarsayilanKdvOrani}"
                };
                File.WriteAllLines(ayarDosyasi, satirlar);
            }
            catch { }
        }

        public static void ConnectionStringOlustur()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = SunucuVerisiniOlustur(),
                InitialCatalog = VeritabaniAdi,
                TrustServerCertificate = true,
                IntegratedSecurity = WindowsAuth,
                MaxPoolSize = 10
            };

            if (!WindowsAuth)
            {
                builder.UserID = KullaniciAdi;
                builder.Password = Sifre;
            }

            _connectionString = builder.ConnectionString;
        }

        public static string SunucuVerisiniOlustur()
        {
            string sunucu = SunucuAdi?.Trim() ?? string.Empty;
            string instance = Instance?.Trim() ?? string.Empty;
            string port = Port?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(instance))
            {
                sunucu = $"{sunucu}\\{instance}";
            }

            if (!string.IsNullOrWhiteSpace(port))
            {
                sunucu = $"{sunucu},{port}";
            }

            return sunucu;
        }

        public static bool BaglantiTest()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
