using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormAyarlar : Form
    {
        private TextBox txtSunucu;
        private TextBox txtPort;
        private TextBox txtInstance;
        private TextBox txtVeritabani;
        private TextBox txtKullanici;
        private TextBox txtSifre;
        private TextBox txtDepo;
        private TextBox txtKasiyer;
        private TextBox txtKdv;
        private CheckBox chkWindowsAuth;
        private Label lblBaglantiDurum;

        public FormAyarlar()
        {
            InitializeComponent();
            AyarlariYukle();
        }

        private void InitializeComponent()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10);
            Text = "⚙️ Ayarlar";
            Size = new Size(620, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            Panel panelBaslik = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblBaslik = new Label
            {
                Text = "⚙️ Uygulama Ayarları",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };
            panelBaslik.Controls.Add(lblBaslik);

            GroupBox grpVeritabani = new GroupBox
            {
                Text = "SQL Server Bağlantısı",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 80),
                Size = new Size(560, 250)
            };

            int y = 30;
            Label lblSunucu = new Label { Text = "Sunucu Adı:", Location = new Point(20, y), AutoSize = true };
            txtSunucu = new TextBox { Location = new Point(160, y - 3), Size = new Size(360, 28) };
            y += 35;

            Label lblPort = new Label { Text = "Port:", Location = new Point(20, y), AutoSize = true };
            txtPort = new TextBox { Location = new Point(160, y - 3), Size = new Size(120, 28) };

            Label lblInstance = new Label { Text = "Instance:", Location = new Point(300, y), AutoSize = true };
            txtInstance = new TextBox { Location = new Point(380, y - 3), Size = new Size(140, 28) };
            y += 35;

            Label lblVeritabani = new Label { Text = "Veritabanı Adı:", Location = new Point(20, y), AutoSize = true };
            txtVeritabani = new TextBox { Location = new Point(160, y - 3), Size = new Size(360, 28) };
            y += 35;

            chkWindowsAuth = new CheckBox
            {
                Text = "Windows Authentication Kullan",
                Location = new Point(20, y),
                AutoSize = true
            };
            chkWindowsAuth.CheckedChanged += ChkWindowsAuth_CheckedChanged;
            y += 30;

            Label lblKullanici = new Label { Text = "Kullanıcı Adı:", Location = new Point(20, y), AutoSize = true };
            txtKullanici = new TextBox { Location = new Point(160, y - 3), Size = new Size(180, 28) };

            Label lblSifre = new Label { Text = "Şifre:", Location = new Point(350, y), AutoSize = true };
            txtSifre = new TextBox { Location = new Point(400, y - 3), Size = new Size(120, 28), PasswordChar = '*' };
            y += 35;

            grpVeritabani.Controls.AddRange(new Control[]
            {
                lblSunucu, txtSunucu,
                lblPort, txtPort,
                lblInstance, txtInstance,
                lblVeritabani, txtVeritabani,
                chkWindowsAuth,
                lblKullanici, txtKullanici,
                lblSifre, txtSifre
            });

            GroupBox grpMagaza = new GroupBox
            {
                Text = "Mağaza/Kasiyer",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 340),
                Size = new Size(560, 120)
            };

            int y2 = 30;
            Label lblDepo = new Label { Text = "Depo Kodu:", Location = new Point(20, y2), AutoSize = true };
            txtDepo = new TextBox { Location = new Point(160, y2 - 3), Size = new Size(120, 28) };

            Label lblKasiyer = new Label { Text = "Kasiyer Rumuzu:", Location = new Point(300, y2), AutoSize = true };
            txtKasiyer = new TextBox { Location = new Point(420, y2 - 3), Size = new Size(100, 28) };
            y2 += 35;

            Label lblKdv = new Label { Text = "Varsayılan KDV (%):", Location = new Point(20, y2), AutoSize = true };
            txtKdv = new TextBox { Location = new Point(160, y2 - 3), Size = new Size(120, 28) };

            grpMagaza.Controls.AddRange(new Control[]
            {
                lblDepo, txtDepo,
                lblKasiyer, txtKasiyer,
                lblKdv, txtKdv
            });

            lblBaglantiDurum = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 470),
                AutoSize = true
            };

            Button btnTest = new Button
            {
                Text = "🔌 Bağlantıyı Test Et",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 500),
                Size = new Size(170, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.Click += BtnTest_Click;

            Button btnKaydet = new Button
            {
                Text = "💾 Kaydet",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(380, 500),
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
                Location = new Point(490, 500),
                Size = new Size(90, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnIptal.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                panelBaslik,
                grpVeritabani,
                grpMagaza,
                lblBaglantiDurum,
                btnTest,
                btnKaydet,
                btnIptal
            });
        }

        private void AyarlariYukle()
        {
            Ayarlar.YukleAyarlar();
            txtSunucu.Text = Ayarlar.SunucuAdi;
            txtPort.Text = Ayarlar.Port;
            txtInstance.Text = Ayarlar.Instance;
            txtVeritabani.Text = Ayarlar.VeritabaniAdi;
            txtKullanici.Text = Ayarlar.KullaniciAdi;
            txtSifre.Text = Ayarlar.Sifre;
            chkWindowsAuth.Checked = Ayarlar.WindowsAuth;
            txtDepo.Text = Ayarlar.DepoKodu;
            txtKasiyer.Text = Ayarlar.KasiyerRumuzu;
            txtKdv.Text = Ayarlar.VarsayilanKdvOrani.ToString("0.##");
            ChkWindowsAuth_CheckedChanged(this, EventArgs.Empty);
        }

        private void ChkWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            bool kullaniciGirilebilir = !chkWindowsAuth.Checked;
            txtKullanici.Enabled = kullaniciGirilebilir;
            txtSifre.Enabled = kullaniciGirilebilir;
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            GeciciAyarlariUygula();

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
            GeciciAyarlariUygula();
            Ayarlar.KaydetAyarlar();

            MessageBox.Show("Ayarlar kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void GeciciAyarlariUygula()
        {
            Ayarlar.SunucuAdi = txtSunucu.Text.Trim();
            Ayarlar.Port = txtPort.Text.Trim();
            Ayarlar.Instance = txtInstance.Text.Trim();
            Ayarlar.VeritabaniAdi = txtVeritabani.Text.Trim();
            Ayarlar.KullaniciAdi = txtKullanici.Text.Trim();
            Ayarlar.Sifre = txtSifre.Text;
            Ayarlar.WindowsAuth = chkWindowsAuth.Checked;
            Ayarlar.DepoKodu = txtDepo.Text.Trim();
            Ayarlar.KasiyerRumuzu = txtKasiyer.Text.Trim();
            if (decimal.TryParse(txtKdv.Text.Trim(), out decimal kdv))
            {
                Ayarlar.VarsayilanKdvOrani = kdv;
            }
            Ayarlar.ConnectionStringOlustur();
        }
    }
}
