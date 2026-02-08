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
        public static string VeritabaniAdi { get; set; } = "BUS2020";
        public static string KullaniciAdi { get; set; } = "";
        public static string Sifre { get; set; } = "";
        public static bool WindowsAuth { get; set; } = true;
        public static string VarsayilanFiyatTipi { get; set; } = "1";
        public static int KritikStokEsigi { get; set; } = 5;

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
                                case "Veritabani": VeritabaniAdi = deger; break;
                                case "Kullanici": KullaniciAdi = deger; break;
                                case "Sifre": Sifre = deger; break;
                                case "WindowsAuth": WindowsAuth = deger == "1"; break;
                                case "VarsayilanFiyatTipi": VarsayilanFiyatTipi = deger; break;
                                case "KritikStokEsigi":
                                    if (int.TryParse(deger, out int esik))
                                    {
                                        KritikStokEsigi = esik;
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
                    $"Veritabani={VeritabaniAdi}",
                    $"Kullanici={KullaniciAdi}",
                    $"Sifre={Sifre}",
                    $"WindowsAuth={(WindowsAuth ? "1" : "0")}",
                    $"VarsayilanFiyatTipi={VarsayilanFiyatTipi}",
                    $"KritikStokEsigi={KritikStokEsigi}"
                };
                File.WriteAllLines(ayarDosyasi, satirlar);
            }
            catch { }
        }

        public static void ConnectionStringOlustur()
        {
            if (WindowsAuth)
            {
                _connectionString = $"Server={SunucuAdi};Database={VeritabaniAdi};Integrated Security=True;TrustServerCertificate=True;";
            }
            else
            {
                _connectionString = $"Server={SunucuAdi};Database={VeritabaniAdi};User Id={KullaniciAdi};Password={Sifre};TrustServerCertificate=True;";
            }
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
        private TextBox txtVeritabani;
        private TextBox txtKullanici;
        private TextBox txtSifre;
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
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

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
                Size = new Size(440, 250)
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
                lblSunucu, txtSunucu, lblVeritabani, txtVeritabani,
                chkWindowsAuth, lblKullanici, txtKullanici, lblSifre, txtSifre
            });

            // Bağlantı Durumu
            lblBaglantiDurum = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 340),
                AutoSize = true
            };

            // Butonlar
            Button btnTest = new Button
            {
                Text = "🔌 Bağlantıyı Test Et",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 370),
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
                Location = new Point(260, 370),
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
                Location = new Point(370, 370),
                Size = new Size(90, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnIptal.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                panelBaslik, grpVeritabani, lblBaglantiDurum, btnTest, btnKaydet, btnIptal
            });
        }

        private void AyarlariYukle()
        {
            Ayarlar.YukleAyarlar();
            txtSunucu.Text = Ayarlar.SunucuAdi;
            txtVeritabani.Text = Ayarlar.VeritabaniAdi;
            txtKullanici.Text = Ayarlar.KullaniciAdi;
            txtSifre.Text = Ayarlar.Sifre;
            chkWindowsAuth.Checked = Ayarlar.WindowsAuth;
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
            Ayarlar.VeritabaniAdi = txtVeritabani.Text.Trim();
            Ayarlar.KullaniciAdi = txtKullanici.Text.Trim();
            Ayarlar.Sifre = txtSifre.Text;
            Ayarlar.WindowsAuth = chkWindowsAuth.Checked;
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
            Ayarlar.VeritabaniAdi = txtVeritabani.Text.Trim();
            Ayarlar.KullaniciAdi = txtKullanici.Text.Trim();
            Ayarlar.Sifre = txtSifre.Text;
            Ayarlar.WindowsAuth = chkWindowsAuth.Checked;
            Ayarlar.ConnectionStringOlustur();
            Ayarlar.KaydetAyarlar();

            MessageBox.Show("Ayarlar kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
