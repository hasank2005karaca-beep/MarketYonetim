using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormSatis : Form
    {
        private readonly DataTable sepet = new DataTable();
        private readonly Timer aramaTimer = new Timer();
        private readonly Timer saatTimer = new Timer();
        private int musteriId;

        private TextBox txtBarkod;
        private TextBox txtArama;
        private ListBox lstAramaSonuc;
        private DataGridView dgvSepet;
        private Label lblToplam;
        private Label lblKdv;
        private Label lblGenelToplam;
        private Label lblGunlukOzet;
        private Label lblSaat;
        private Label lblSonUrun;
        private Label lblMusteri;
        private Button btnOdeme;
        private Button btnSepetTemizle;
        private Panel panelMenu;

        public FormSatis()
        {
            InitializeComponent();
            SepetHazirla();
            KeyPreview = true;
        }

        private void InitializeComponent()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10);
            Text = "🛒 Satış";
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));

            layout.Controls.Add(OlusturUstPanel(), 0, 0);
            layout.Controls.Add(OlusturOrtaPanel(), 0, 1);
            layout.Controls.Add(OlusturAltPanel(), 0, 2);

            var anaPanel = new Panel { Dock = DockStyle.Fill };
            // S7-FIX: Sol menü paneli
            panelMenu = OlusturMenuPanel();
            anaPanel.Controls.Add(layout);
            anaPanel.Controls.Add(panelMenu);
            Controls.Add(anaPanel);

            aramaTimer.Interval = 300;
            aramaTimer.Tick += AramaTimer_Tick;

            saatTimer.Interval = 1000;
            saatTimer.Tick += SaatTimer_Tick;
            saatTimer.Start();

            Load += FormSatis_Load;
            KeyDown += FormSatis_KeyDown;
        }

        private Control OlusturUstPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            var lblBaslik = new Label
            {
                Text = "🛒 Satış",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 25)
            };

            lblGunlukOzet = new Label
            {
                Text = "Bugün: ₺0,00 (0 satış)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(220, 32)
            };

            lblSaat = new Label
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(0, 35)
            };
            lblSaat.Left = panel.Width - lblSaat.Width - 20;
            lblSaat.Top = 35;
            lblSaat.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            panel.Controls.Add(lblBaslik);
            panel.Controls.Add(lblGunlukOzet);
            panel.Controls.Add(lblSaat);
            panel.Resize += (_, __) =>
            {
                lblSaat.Left = panel.Width - lblSaat.Width - 20;
            };

            return panel;
        }

        private Panel OlusturMenuPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = Color.FromArgb(30, 30, 46)
            };

            int y = 20;
            panel.Controls.Add(OlusturMenuButon("📦 Ürünler (F3)", y, (_, __) => AcForm(new FormUrunYonetimi()))); y += 45;
            panel.Controls.Add(OlusturMenuButon("📋 Stok (F4)", y, (_, __) => AcForm(new FormStokYonetimi()))); y += 45;
            panel.Controls.Add(OlusturMenuButon("👥 Müşteriler (F6)", y, (_, __) => AcForm(new FormMusteriYonetimi()))); y += 45;
            panel.Controls.Add(OlusturMenuButon("📊 Raporlar (F7)", y, (_, __) => AcForm(new FormRaporlar()))); y += 45;
            panel.Controls.Add(OlusturMenuButon("⚙️ Toplu İşlemler (F8)", y, (_, __) => AcForm(new FormTopluIslemler()))); y += 45;
            panel.Controls.Add(OlusturMenuButon("💲 Fiyatlar (F10)", y, (_, __) => AcForm(new FormFiyatYonetimi()))); y += 45;
            panel.Controls.Add(OlusturMenuButon("🔧 Ayarlar (F11)", y, (_, __) =>
            {
                using (var form = new FormAyarlar())
                {
                    form.ShowDialog();
                }
                Ayarlar.YukleAyarlar();
                GunSatisOzetiniGuncelle();
            }));

            return panel;
        }

        private Button OlusturMenuButon(string text, int y, EventHandler click)
        {
            var btn = new Button
            {
                Text = text,
                Width = 160,
                Height = 36,
                Location = new Point(10, y),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 64),
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += click;
            return btn;
        }

        private void AcForm(Form form)
        {
            using (form)
            {
                form.ShowDialog();
            }
            // S7-FIX: Form dönüşünde satış özetini güncelle
            GunSatisOzetiniGuncelle();
        }

        private Control OlusturOrtaPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

            panel.Controls.Add(OlusturSepetPanel(), 0, 0);
            panel.Controls.Add(OlusturSagPanel(), 1, 0);

            return panel;
        }

        private Control OlusturSepetPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            dgvSepet = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            // S7-FIX: Grid flicker azaltma
            Yardimcilar.DoubleBufferedAktifEt(dgvSepet);

            dgvSepet.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 122, 204);
            dgvSepet.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSepet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvSepet.ColumnHeadersHeight = 36;
            dgvSepet.EnableHeadersVisualStyles = false;
            dgvSepet.CellEndEdit += DgvSepet_CellEndEdit;
            dgvSepet.UserDeletingRow += (_, __) => ToplamlariHesapla();

            panel.Controls.Add(dgvSepet);

            return panel;
        }

        private Control OlusturSagPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var lblBarkod = new Label
            {
                Text = "Barkod",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            txtBarkod = new TextBox
            {
                Font = new Font("Segoe UI", 14),
                Width = 250,
                Location = new Point(0, 25)
            };
            txtBarkod.KeyDown += TxtBarkod_KeyDown;

            var lblArama = new Label
            {
                Text = "Arama",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 70)
            };

            txtArama = new TextBox
            {
                Font = new Font("Segoe UI", 12),
                Width = 250,
                Location = new Point(0, 95)
            };
            txtArama.TextChanged += TxtArama_TextChanged;

            lstAramaSonuc = new ListBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 250,
                Height = 200,
                Location = new Point(0, 130),
                Visible = false
            };
            lstAramaSonuc.DoubleClick += LstAramaSonuc_DoubleClick;

            lblSonUrun = new Label
            {
                Text = "Son ürün: -",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = false,
                Width = 250,
                Height = 60,
                Location = new Point(0, 350)
            };

            lblMusteri = new Label
            {
                Text = "Müşteri: Peşin",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 420)
            };

            var btnMusteri = new Button
            {
                Text = "Müşteri Seç (F5)",
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 250,
                Height = 40,
                Location = new Point(0, 450)
            };
            btnMusteri.Click += BtnMusteri_Click;

            panel.Controls.Add(lblBarkod);
            panel.Controls.Add(txtBarkod);
            panel.Controls.Add(lblArama);
            panel.Controls.Add(txtArama);
            panel.Controls.Add(lstAramaSonuc);
            panel.Controls.Add(lblSonUrun);
            panel.Controls.Add(lblMusteri);
            panel.Controls.Add(btnMusteri);

            return panel;
        }

        private Control OlusturAltPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            lblToplam = new Label
            {
                Text = "Toplam: ₺0,00",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            lblKdv = new Label
            {
                Text = "KDV: ₺0,00",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 50)
            };

            lblGenelToplam = new Label
            {
                Text = "Genel Toplam: ₺0,00",
                ForeColor = Color.FromArgb(0, 200, 83),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 80)
            };

            btnSepetTemizle = new Button
            {
                Text = "Sepeti Temizle (F9)",
                BackColor = Color.FromArgb(255, 23, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 200,
                Height = 45,
                Location = new Point(600, 35)
            };
            btnSepetTemizle.Click += BtnSepetTemizle_Click;

            btnOdeme = new Button
            {
                Text = "💰 Ödeme (F12)",
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 220,
                Height = 55,
                Location = new Point(820, 30)
            };
            btnOdeme.Click += BtnOdeme_Click;

            panel.Controls.Add(lblToplam);
            panel.Controls.Add(lblKdv);
            panel.Controls.Add(lblGenelToplam);
            panel.Controls.Add(btnSepetTemizle);
            panel.Controls.Add(btnOdeme);

            return panel;
        }

        private void SepetHazirla()
        {
            sepet.Columns.Add("nStokID", typeof(int));
            sepet.Columns.Add("sBarkod", typeof(string));
            sepet.Columns.Add("sAciklama", typeof(string));
            sepet.Columns.Add("sBirimCinsi", typeof(string));
            sepet.Columns.Add("lMiktar", typeof(decimal));
            sepet.Columns.Add("lBirimFiyat", typeof(decimal));
            sepet.Columns.Add("nIskontoYuzde", typeof(decimal));
            sepet.Columns.Add("lSatirToplam", typeof(decimal));
            sepet.Columns.Add("nKdvOrani", typeof(decimal));
            sepet.Columns.Add("lKdvTutar", typeof(decimal));

            dgvSepet.DataSource = sepet;
            dgvSepet.Columns.Clear();

            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Sıra",
                Width = 50,
                ReadOnly = true
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "sBarkod",
                HeaderText = "Barkod",
                Width = 120,
                ReadOnly = true
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "sAciklama",
                HeaderText = "Ürün Adı",
                Width = 240,
                ReadOnly = true
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "lMiktar",
                HeaderText = "Miktar",
                Width = 80
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "lBirimFiyat",
                HeaderText = "Birim Fiyat",
                Width = 110
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "nIskontoYuzde",
                HeaderText = "İskonto%",
                Width = 90
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "lSatirToplam",
                HeaderText = "Satır Toplamı",
                Width = 120,
                ReadOnly = true
            });
            dgvSepet.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "nKdvOrani",
                HeaderText = "KDV%",
                Width = 70,
                ReadOnly = true
            });

            dgvSepet.CellFormatting += DgvSepet_CellFormatting;
        }

        private void FormSatis_Load(object sender, EventArgs e)
        {
            txtBarkod.Focus();
            GunlukOzetYukle();
        }

        private void SaatTimer_Tick(object sender, EventArgs e)
        {
            lblSaat.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        private void GunlukOzetYukle()
        {
            try
            {
                DataTable ozet = VeriKatmani.GunSatisOzeti();
                if (ozet.Rows.Count > 0)
                {
                    decimal toplam = Convert.ToDecimal(ozet.Rows[0]["ToplamTutar"]);
                    int adet = Convert.ToInt32(ozet.Rows[0]["SatisAdedi"]);
                    lblGunlukOzet.Text = $"Bugün: {Yardimcilar.ParaFormatla(toplam)} ({adet} satış)";
                }
            }
            catch
            {
                lblGunlukOzet.Text = "Bugün: ₺0,00 (0 satış)";
            }
        }

        private void GunSatisOzetiniGuncelle()
        {
            // S7-FIX: Menü dönüşlerinde güncel satış özetini yükle
            GunlukOzetYukle();
        }

        private void TxtBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string barkod = txtBarkod.Text.Trim();
                if (!string.IsNullOrWhiteSpace(barkod))
                {
                    BarkodIleEkle(barkod);
                }
                txtBarkod.Clear();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            string arama = txtArama.Text.Trim();
            if (arama.Length < 2)
            {
                lstAramaSonuc.DataSource = null;
                lstAramaSonuc.Visible = false;
                return;
            }

            try
            {
                DataTable sonuc = VeriKatmani.UrunAra(arama);
                lstAramaSonuc.DataSource = sonuc;
                lstAramaSonuc.DisplayMember = "sAciklama";
                lstAramaSonuc.ValueMember = "nStokID";
                lstAramaSonuc.Visible = sonuc.Rows.Count > 0;
            }
            catch
            {
                lstAramaSonuc.DataSource = null;
                lstAramaSonuc.Visible = false;
            }
        }

        private void LstAramaSonuc_DoubleClick(object sender, EventArgs e)
        {
            if (lstAramaSonuc.SelectedItem is DataRowView rowView)
            {
                UrunEkle(rowView.Row);
                txtArama.Clear();
                lstAramaSonuc.Visible = false;
            }
        }

        private void BarkodIleEkle(string barkod)
        {
            try
            {
                DataTable sonuc = VeriKatmani.BarkodIleUrunBul(barkod);
                if (sonuc.Rows.Count == 0)
                {
                    MessageBox.Show("Ürün bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UrunEkle(sonuc.Rows[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün eklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UrunEkle(DataRow urun)
        {
            int stokId = Convert.ToInt32(urun["nStokID"]);
            string barkod = urun["sBarkod"].ToString().Trim();
            string aciklama = urun["sAciklama"].ToString().Trim();
            string birim = urun["sBirimCinsi"].ToString().Trim();
            decimal fiyat = Convert.ToDecimal(urun["lFiyat"]);
            decimal kdv = Convert.ToDecimal(urun["nKdvOrani"]);

            if (fiyat <= 0)
            {
                MessageBox.Show("Ürün fiyatı tanımlı değil.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRow mevcut = sepet.AsEnumerable().FirstOrDefault(row => row.Field<int>("nStokID") == stokId);
            if (mevcut == null)
            {
                DataRow yeni = sepet.NewRow();
                yeni["nStokID"] = stokId;
                yeni["sBarkod"] = barkod;
                yeni["sAciklama"] = aciklama;
                yeni["sBirimCinsi"] = birim;
                yeni["lMiktar"] = 1m;
                yeni["lBirimFiyat"] = fiyat;
                yeni["nIskontoYuzde"] = 0m;
                yeni["nKdvOrani"] = kdv;
                yeni["lSatirToplam"] = 0m;
                yeni["lKdvTutar"] = 0m;
                sepet.Rows.Add(yeni);
                mevcut = yeni;
            }
            else
            {
                mevcut["lMiktar"] = Convert.ToDecimal(mevcut["lMiktar"]) + 1m;
            }

            SatirHesapla(mevcut);
            lblSonUrun.Text = $"Son ürün: {aciklama}";
            ToplamlariHesapla();
        }

        private void SatirHesapla(DataRow row)
        {
            decimal miktar = Convert.ToDecimal(row["lMiktar"]);
            decimal birimFiyat = Convert.ToDecimal(row["lBirimFiyat"]);
            decimal iskonto = Convert.ToDecimal(row["nIskontoYuzde"]);
            decimal brut = Yardimcilar.YuvarlaKurus(miktar * birimFiyat);
            decimal iskontoTutar = Yardimcilar.YuvarlaKurus(brut * (iskonto / 100m));
            decimal net = Yardimcilar.YuvarlaKurus(brut - iskontoTutar);
            decimal kdvOrani = Convert.ToDecimal(row["nKdvOrani"]);
            decimal kdvTutar = Yardimcilar.KdvTutarHesapla(net, kdvOrani);

            row["lSatirToplam"] = net;
            row["lKdvTutar"] = kdvTutar;
        }

        private void ToplamlariHesapla()
        {
            decimal brutToplam = 0m;
            decimal kdvToplam = 0m;

            foreach (DataRow row in sepet.Rows)
            {
                brutToplam += Convert.ToDecimal(row["lSatirToplam"]);
                kdvToplam += Convert.ToDecimal(row["lKdvTutar"]);
            }

            lblToplam.Text = $"Toplam: {Yardimcilar.ParaFormatla(brutToplam)}";
            lblKdv.Text = $"KDV: {Yardimcilar.ParaFormatla(kdvToplam)}";
            lblGenelToplam.Text = $"Genel Toplam: {Yardimcilar.ParaFormatla(brutToplam)}";
        }

        private void DgvSepet_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == 0)
            {
                e.Value = (e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
            }

            if (dgvSepet.Columns[e.ColumnIndex].DataPropertyName == "lBirimFiyat" ||
                dgvSepet.Columns[e.ColumnIndex].DataPropertyName == "lSatirToplam")
            {
                if (e.Value != null)
                {
                    e.Value = Yardimcilar.ParaFormatla(Convert.ToDecimal(e.Value));
                    e.FormattingApplied = true;
                }
            }

            if (dgvSepet.Columns[e.ColumnIndex].DataPropertyName == "nKdvOrani")
            {
                if (e.Value != null)
                {
                    e.Value = Yardimcilar.KdvOraniYuzdeGoster(Convert.ToDecimal(e.Value));
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvSepet_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataRow row = ((DataRowView)dgvSepet.Rows[e.RowIndex].DataBoundItem).Row;
            SatirHesapla(row);
            ToplamlariHesapla();
        }

        private void BtnOdeme_Click(object sender, EventArgs e)
        {
            if (sepet.Rows.Count == 0)
            {
                MessageBox.Show("Sepet boş.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new FormOdeme(sepet, musteriId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    sepet.Clear();
                    musteriId = 0;
                    lblMusteri.Text = "Müşteri: Peşin";
                    ToplamlariHesapla();
                    GunlukOzetYukle();
                }
            }
        }

        private void BtnSepetTemizle_Click(object sender, EventArgs e)
        {
            if (sepet.Rows.Count == 0)
            {
                return;
            }

            if (MessageBox.Show("Sepet temizlensin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                sepet.Clear();
                ToplamlariHesapla();
            }
        }

        private void BtnMusteri_Click(object sender, EventArgs e)
        {
            using (var form = new FormMusteriSec())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    musteriId = form.SecilenMusteriId;
                    lblMusteri.Text = $"Müşteri: {form.SecilenMusteriAdi}";
                }
            }
        }

        private void FormSatis_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    txtBarkod.Focus();
                    e.Handled = true;
                    break;
                case Keys.F2:
                    txtArama.Focus();
                    e.Handled = true;
                    break;
                case Keys.F5:
                    BtnMusteri_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.F3:
                    AcForm(new FormUrunYonetimi());
                    e.Handled = true;
                    break;
                case Keys.F4:
                    AcForm(new FormStokYonetimi());
                    e.Handled = true;
                    break;
                case Keys.F6:
                    AcForm(new FormMusteriYonetimi());
                    e.Handled = true;
                    break;
                case Keys.F7:
                    AcForm(new FormRaporlar());
                    e.Handled = true;
                    break;
                case Keys.F8:
                    AcForm(new FormTopluIslemler());
                    e.Handled = true;
                    break;
                case Keys.F10:
                    AcForm(new FormFiyatYonetimi());
                    e.Handled = true;
                    break;
                case Keys.F11:
                    using (var form = new FormAyarlar())
                    {
                        form.ShowDialog();
                    }
                    Ayarlar.YukleAyarlar();
                    GunSatisOzetiniGuncelle();
                    e.Handled = true;
                    break;
                case Keys.F9:
                    BtnSepetTemizle_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.F12:
                    BtnOdeme_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    if (dgvSepet.SelectedRows.Count > 0)
                    {
                        dgvSepet.Rows.RemoveAt(dgvSepet.SelectedRows[0].Index);
                        ToplamlariHesapla();
                    }
                    e.Handled = true;
                    break;
            }
        }
    }
}
