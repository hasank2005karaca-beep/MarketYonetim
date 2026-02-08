using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace MarketYonetim
{
    // Ayarlar - Statik sınıf
    public static class Ayarlar
    {
        private static string _connectionString;
        private static string ayarDosyasi = Path.Combine(Application.StartupPath, "ayarlar.ini");

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

        public static string SunucuAdi { get; set; } = "Hasan";
        public static string InstanceAdi { get; set; } = "";
        public static string Port { get; set; } = "";
        public static string VeritabaniAdi { get; set; } = "BUS2020";
        public static string KullaniciAdi { get; set; } = "";
        public static string Sifre { get; set; } = "";
        public static bool WindowsAuth { get; set; } = true;
        public static string DepoKodu { get; set; } = "001";
        public static string KasiyerRumuzu { get; set; } = "KSY";
        public static decimal VarsayilanKdvOrani { get; set; } = 20m;

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
                                case "Sunucu": SunucuAdi = deger; break;
                                case "Instance": InstanceAdi = deger; break;
                                case "Port": Port = deger; break;
                                case "Veritabani": VeritabaniAdi = deger; break;
                                case "Kullanici": KullaniciAdi = deger; break;
                                case "Sifre": Sifre = deger; break;
                                case "WindowsAuth": WindowsAuth = deger == "1"; break;
                                case "DepoKodu": DepoKodu = deger; break;
                                case "KasiyerRumuzu": KasiyerRumuzu = deger; break;
                                case "VarsayilanKdvOrani":
                                    if (decimal.TryParse(deger, out var kdv))
                                    {
                                        VarsayilanKdvOrani = kdv;
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
                    $"Sunucu={SunucuAdi}",
                    $"Instance={InstanceAdi}",
                    $"Port={Port}",
                    $"Veritabani={VeritabaniAdi}",
                    $"Kullanici={KullaniciAdi}",
                    $"Sifre={Sifre}",
                    $"WindowsAuth={(WindowsAuth ? "1" : "0")}",
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
            string dataSource = SunucuAdi;
            if (!string.IsNullOrWhiteSpace(InstanceAdi))
            {
                dataSource += $"\\{InstanceAdi}";
            }
            if (!string.IsNullOrWhiteSpace(Port))
            {
                dataSource += $",{Port}";
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = VeritabaniAdi,
                IntegratedSecurity = WindowsAuth,
                TrustServerCertificate = true,
                MaxPoolSize = 10
            };

            if (!WindowsAuth)
            {
                builder.UserID = KullaniciAdi;
                builder.Password = Sifre;
            }

            _connectionString = builder.ToString();
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

    // Ayarlar Formu
    public partial class FormAyarlar : Form
    {
        private TextBox txtSunucu;
        private TextBox txtInstance;
        private TextBox txtPort;
        private TextBox txtVeritabani;
        private TextBox txtKullanici;
        private TextBox txtSifre;
        private TextBox txtDepoKodu;
        private TextBox txtKasiyerRumuzu;
        private TextBox txtVarsayilanKdv;
        private CheckBox chkWindowsAuth;
        private Label lblBaglantiDurum;

        public FormAyarlar()
        {
            InitializeComponent();
            AyarlariYukle();
        }

        private void InitializeComponent()
        {
            this.Text = "⚙️ Ayarlar";
            this.Size = new Size(520, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new Font("Segoe UI", 10);

            // Başlık Panel
            Panel panelBaslik = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblBaslik = new Label
            {
                Text = "⚙️ Veritabanı Ayarları",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };
            panelBaslik.Controls.Add(lblBaslik);

            // Grup Kutusu
            GroupBox grpVeritabani = new GroupBox
            {
                Text = "SQL Server Bağlantısı",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 80),
                Size = new Size(460, 320)
            };

            int y = 30;

            Label lblSunucu = new Label { Text = "Sunucu Adı:", Location = new Point(20, y), AutoSize = true };
            txtSunucu = new TextBox
            {
                Location = new Point(150, y - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            y += 40;

            Label lblInstance = new Label { Text = "Instance:", Location = new Point(20, y), AutoSize = true };
            txtInstance = new TextBox
            {
                Location = new Point(150, y - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            y += 40;

            Label lblPort = new Label { Text = "Port:", Location = new Point(20, y), AutoSize = true };
            txtPort = new TextBox
            {
                Location = new Point(150, y - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            y += 40;

            Label lblVeritabani = new Label { Text = "Veritabanı Adı:", Location = new Point(20, y), AutoSize = true };
            txtVeritabani = new TextBox
            {
                Location = new Point(150, y - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            y += 40;

            chkWindowsAuth = new CheckBox
            {
                Text = "Windows Authentication Kullan",
                Location = new Point(20, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            chkWindowsAuth.CheckedChanged += ChkWindowsAuth_CheckedChanged;
            y += 35;

            Label lblKullanici = new Label { Text = "Kullanıcı Adı:", Location = new Point(20, y), AutoSize = true };
            txtKullanici = new TextBox
            {
                Location = new Point(150, y - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            y += 40;

            Label lblSifre = new Label { Text = "Şifre:", Location = new Point(20, y), AutoSize = true };
            txtSifre = new TextBox
            {
                Location = new Point(150, y - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11),
                PasswordChar = '*'
            };

            grpVeritabani.Controls.AddRange(new Control[] {
                lblSunucu, txtSunucu, lblInstance, txtInstance, lblPort, txtPort,
                lblVeritabani, txtVeritabani,
                chkWindowsAuth, lblKullanici, txtKullanici, lblSifre, txtSifre
            });

            GroupBox grpMagaza = new GroupBox
            {
                Text = "Mağaza Ayarları",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 410),
                Size = new Size(460, 150)
            };

            int mY = 30;
            Label lblDepo = new Label { Text = "Depo Kodu:", Location = new Point(20, mY), AutoSize = true };
            txtDepoKodu = new TextBox
            {
                Location = new Point(150, mY - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            mY += 40;

            Label lblKasiyer = new Label { Text = "Kasiyer Rumuzu:", Location = new Point(20, mY), AutoSize = true };
            txtKasiyerRumuzu = new TextBox
            {
                Location = new Point(150, mY - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };
            mY += 40;

            Label lblKdv = new Label { Text = "Varsayılan KDV:", Location = new Point(20, mY), AutoSize = true };
            txtVarsayilanKdv = new TextBox
            {
                Location = new Point(150, mY - 3),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 11)
            };

            grpMagaza.Controls.AddRange(new Control[] {
                lblDepo, txtDepoKodu, lblKasiyer, txtKasiyerRumuzu, lblKdv, txtVarsayilanKdv
            });

            // Bağlantı Durumu
            lblBaglantiDurum = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 570),
                AutoSize = true
            };

            // Butonlar
            Button btnTest = new Button
            {
                Text = "🔌 Bağlantıyı Test Et",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 595),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.Click += BtnTest_Click;

            Button btnKaydet = new Button
            {
                Text = "💾 Kaydet",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(280, 595),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKaydet.Click += BtnKaydet_Click;

            Button btnIptal = new Button
            {
                Text = "İptal",
                Font = new Font("Segoe UI", 10),
                Location = new Point(390, 595),
                Size = new Size(90, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnIptal.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                panelBaslik, grpVeritabani, grpMagaza, lblBaglantiDurum, btnTest, btnKaydet, btnIptal
            });
        }

        private void AyarlariYukle()
        {
            Ayarlar.YukleAyarlar();
            txtSunucu.Text = Ayarlar.SunucuAdi;
            txtInstance.Text = Ayarlar.InstanceAdi;
            txtPort.Text = Ayarlar.Port;
            txtVeritabani.Text = Ayarlar.VeritabaniAdi;
            txtKullanici.Text = Ayarlar.KullaniciAdi;
            txtSifre.Text = Ayarlar.Sifre;
            chkWindowsAuth.Checked = Ayarlar.WindowsAuth;
            txtDepoKodu.Text = Ayarlar.DepoKodu;
            txtKasiyerRumuzu.Text = Ayarlar.KasiyerRumuzu;
            txtVarsayilanKdv.Text = Ayarlar.VarsayilanKdvOrani.ToString("N2");
            ChkWindowsAuth_CheckedChanged(null, null);
        }

        private void ChkWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            txtKullanici.Enabled = !chkWindowsAuth.Checked;
            txtSifre.Enabled = !chkWindowsAuth.Checked;
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            // Geçici olarak ayarları uygula
            Ayarlar.SunucuAdi = txtSunucu.Text.Trim();
            Ayarlar.InstanceAdi = txtInstance.Text.Trim();
            Ayarlar.Port = txtPort.Text.Trim();
            Ayarlar.VeritabaniAdi = txtVeritabani.Text.Trim();
            Ayarlar.KullaniciAdi = txtKullanici.Text.Trim();
            Ayarlar.Sifre = txtSifre.Text;
            Ayarlar.WindowsAuth = chkWindowsAuth.Checked;
            Ayarlar.DepoKodu = txtDepoKodu.Text.Trim();
            Ayarlar.KasiyerRumuzu = txtKasiyerRumuzu.Text.Trim();
            if (decimal.TryParse(txtVarsayilanKdv.Text, out var kdv))
            {
                Ayarlar.VarsayilanKdvOrani = kdv;
            }
            Ayarlar.ConnectionStringOlustur();

            lblBaglantiDurum.Text = "Test ediliyor...";
            lblBaglantiDurum.ForeColor = Color.Orange;
            Application.DoEvents();

            if (Ayarlar.BaglantiTest())
            {
                lblBaglantiDurum.Text = "✓ Bağlantı başarılı!";
                lblBaglantiDurum.ForeColor = Color.Green;
            }
            else
            {
                lblBaglantiDurum.Text = "✗ Bağlantı başarısız!";
                lblBaglantiDurum.ForeColor = Color.Red;
            }
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            Ayarlar.SunucuAdi = txtSunucu.Text.Trim();
            Ayarlar.InstanceAdi = txtInstance.Text.Trim();
            Ayarlar.Port = txtPort.Text.Trim();
            Ayarlar.VeritabaniAdi = txtVeritabani.Text.Trim();
            Ayarlar.KullaniciAdi = txtKullanici.Text.Trim();
            Ayarlar.Sifre = txtSifre.Text;
            Ayarlar.WindowsAuth = chkWindowsAuth.Checked;
            Ayarlar.DepoKodu = txtDepoKodu.Text.Trim();
            Ayarlar.KasiyerRumuzu = txtKasiyerRumuzu.Text.Trim();
            if (decimal.TryParse(txtVarsayilanKdv.Text, out var kayitKdv))
            {
                Ayarlar.VarsayilanKdvOrani = kayitKdv;
            }
            Ayarlar.ConnectionStringOlustur();
            Ayarlar.KaydetAyarlar();

            MessageBox.Show("Ayarlar kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
