using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormUrunYonetimi : Form
    {
        private TextBox txtArama;
        private TextBox txtBarkod;
        private ComboBox cmbSinif1;
        private ComboBox cmbSinif2;
        private ComboBox cmbStokDurum;
        private NumericUpDown nudMinFiyat;
        private NumericUpDown nudMaxFiyat;
        private DataGridView dgvUrunler;
        private Label lblSayfa;
        private Label lblToplam;
        private Button btnOnceki;
        private Button btnSonraki;
        private Timer aramaTimer;
        private int sayfa = 1;
        private int sayfaBoyutu = 50;
        private int toplamKayit = 0;
        private DataTable mevcutUrunler;
        private string mevcutSortKolon = "Kodu";
        private bool sortArtan = true;

        private Label lblDetayKod;
        private Label lblDetayAd;
        private Label lblDetayBirim;
        private Label lblDetayKategori;
        private Label lblDetayStok;
        private ListBox lstBarkodlar;
        private DataGridView dgvFiyatlar;
        private int? seciliStokId;

        public FormUrunYonetimi()
        {
            InitializeComponent();
            FiltreleriYukle();
            UrunleriYukle();
        }

        private void InitializeComponent()
        {
            Text = "📦 Ürün Yönetimi";
            Size = new Size(1350, 780);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            Panel panelFiltre = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            txtArama = new TextBox { Width = 220, PlaceholderText = "Ürün adı veya kod" };
            txtArama.TextChanged += TxtArama_TextChanged;

            txtBarkod = new TextBox { Width = 160, PlaceholderText = "Barkod (tam eşleşme)" };
            txtBarkod.KeyDown += TxtBarkod_KeyDown;

            cmbSinif1 = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSinif1.SelectedIndexChanged += CmbSinif1_SelectedIndexChanged;

            cmbSinif2 = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };

            cmbStokDurum = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStokDurum.Items.AddRange(new object[] { "Hepsi", "Var", "Yok", "Az" });
            cmbStokDurum.SelectedIndex = 0;

            nudMinFiyat = new NumericUpDown { Width = 100, DecimalPlaces = 2, Maximum = 1000000, Increment = 1 };
            nudMaxFiyat = new NumericUpDown { Width = 100, DecimalPlaces = 2, Maximum = 1000000, Increment = 1 };

            Button btnAra = new Button { Text = "Ara", Width = 90, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnAra.Click += (s, e) => { sayfa = 1; UrunleriYukle(); };

            Button btnTemizle = new Button { Text = "Temizle", Width = 90, BackColor = Color.Gray, ForeColor = Color.White };
            btnTemizle.Click += (s, e) => FiltreleriTemizle();

            FlowLayoutPanel filtreSatir1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 };
            filtreSatir1.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                txtArama,
                new Label { Text = "Barkod:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                txtBarkod,
                new Label { Text = "Kategori1:", Width = 70, TextAlign = ContentAlignment.MiddleLeft },
                cmbSinif1,
                new Label { Text = "Kategori2:", Width = 70, TextAlign = ContentAlignment.MiddleLeft },
                cmbSinif2
            });

            FlowLayoutPanel filtreSatir2 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 0) };
            filtreSatir2.Controls.AddRange(new Control[]
            {
                new Label { Text = "Stok:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                cmbStokDurum,
                new Label { Text = "Min Fiyat:", Width = 70, TextAlign = ContentAlignment.MiddleLeft },
                nudMinFiyat,
                new Label { Text = "Max Fiyat:", Width = 70, TextAlign = ContentAlignment.MiddleLeft },
                nudMaxFiyat,
                btnAra,
                btnTemizle
            });

            panelFiltre.Controls.Add(filtreSatir2);
            panelFiltre.Controls.Add(filtreSatir1);

            dgvUrunler = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvUrunler.ColumnHeadersHeight = 32;
            dgvUrunler.RowTemplate.Height = 28;
            dgvUrunler.DataBindingComplete += (s, e) => dgvUrunler.ClearSelection();
            dgvUrunler.SelectionChanged += DgvUrunler_SelectionChanged;
            dgvUrunler.ColumnHeaderMouseClick += DgvUrunler_ColumnHeaderMouseClick;
            Yardimcilar.DoubleBufferedAktifEt(dgvUrunler);

            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "nStokID", Visible = false });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kodu", DataPropertyName = "sKodu", Width = 120 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "sAciklama", Width = 240 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barkod", DataPropertyName = "Barkod", Width = 140 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Birim", DataPropertyName = "sBirimCinsi", Width = 90 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fiyat", DataPropertyName = "Fiyat", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stok", DataPropertyName = "StokMiktari", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kategori1", DataPropertyName = "Kategori1", Width = 100 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kategori2", DataPropertyName = "Kategori2", Width = 100 });

            Panel panelDetay = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            lblDetayKod = new Label { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lblDetayAd = new Label { AutoSize = true };
            lblDetayBirim = new Label { AutoSize = true };
            lblDetayKategori = new Label { AutoSize = true };
            lblDetayStok = new Label { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            lstBarkodlar = new ListBox { Height = 100 };

            dgvFiyatlar = new DataGridView
            {
                Height = 140,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvFiyatlar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat Tipi", DataPropertyName = "sFiyatTipi", Width = 100 });
            dgvFiyatlar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat", DataPropertyName = "lFiyat", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            Button btnEkle = new Button { Text = "➕ Ekle", Width = 150 };
            btnEkle.Click += BtnEkle_Click;
            Button btnDuzenle = new Button { Text = "✏️ Düzenle", Width = 150 };
            btnDuzenle.Click += BtnDuzenle_Click;
            Button btnSil = new Button { Text = "🗑️ Sil", Width = 150 };
            btnSil.Click += BtnSil_Click;
            Button btnBarkod = new Button { Text = "🏷️ Barkod Yönet", Width = 150 };
            btnBarkod.Click += BtnBarkod_Click;
            Button btnFiyat = new Button { Text = "💲 Fiyat Yönet", Width = 150 };
            btnFiyat.Click += BtnFiyat_Click;
            Button btnYenile = new Button { Text = "🔄 Yenile", Width = 150 };
            btnYenile.Click += (s, e) => UrunleriYukle();

            FlowLayoutPanel panelButonlar = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Bottom, Height = 240 };
            panelButonlar.Controls.AddRange(new Control[] { btnEkle, btnDuzenle, btnSil, btnBarkod, btnFiyat, btnYenile });

            panelDetay.Controls.Add(new Label { Text = "Ürün Detayı", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top });
            panelDetay.Controls.Add(new Label { Text = "Kod:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(lblDetayKod);
            panelDetay.Controls.Add(new Label { Text = "Ad:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(lblDetayAd);
            panelDetay.Controls.Add(new Label { Text = "Birim:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(lblDetayBirim);
            panelDetay.Controls.Add(new Label { Text = "Kategori:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(lblDetayKategori);
            panelDetay.Controls.Add(new Label { Text = "Barkodlar:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(lstBarkodlar);
            panelDetay.Controls.Add(new Label { Text = "Fiyatlar:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(dgvFiyatlar);
            panelDetay.Controls.Add(new Label { Text = "Toplam Stok:", Dock = DockStyle.Top });
            panelDetay.Controls.Add(lblDetayStok);
            panelDetay.Controls.Add(panelButonlar);

            Panel panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            btnOnceki = new Button { Text = "◀ Önceki", Width = 100 };
            btnOnceki.Click += (s, e) => { if (sayfa > 1) { sayfa--; UrunleriYukle(); } };
            btnSonraki = new Button { Text = "Sonraki ▶", Width = 100 };
            btnSonraki.Click += (s, e) => { if (sayfa < ToplamSayfa()) { sayfa++; UrunleriYukle(); } };

            lblSayfa = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            lblToplam = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            FlowLayoutPanel panelSayfa = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 340 };
            panelSayfa.Controls.AddRange(new Control[] { btnOnceki, btnSonraki, lblSayfa });

            FlowLayoutPanel panelBilgi = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 300, FlowDirection = FlowDirection.RightToLeft };
            panelBilgi.Controls.Add(lblToplam);

            panelAlt.Controls.Add(panelSayfa);
            panelAlt.Controls.Add(panelBilgi);

            Controls.Add(dgvUrunler);
            Controls.Add(panelDetay);
            Controls.Add(panelAlt);
            Controls.Add(panelFiltre);

            aramaTimer = new Timer { Interval = 300 };
            aramaTimer.Tick += AramaTimer_Tick;
        }

        private void FiltreleriYukle()
        {
            try
            {
                cmbSinif1.Items.Clear();
                cmbSinif1.Items.Add("Hepsi");
                DataTable sinif1ler = VeriKatmani.Sinif1leriGetir();
                foreach (DataRow row in sinif1ler.Rows)
                {
                    cmbSinif1.Items.Add(row[0].ToString());
                }
                cmbSinif1.SelectedIndex = 0;

                cmbSinif2.Items.Clear();
                cmbSinif2.Items.Add("Hepsi");
                cmbSinif2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategoriler yüklenemedi. Detay: {ex.Message}", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FiltreleriTemizle()
        {
            txtArama.Text = string.Empty;
            txtBarkod.Text = string.Empty;
            cmbSinif1.SelectedIndex = 0;
            cmbSinif2.SelectedIndex = 0;
            cmbStokDurum.SelectedIndex = 0;
            nudMinFiyat.Value = 0;
            nudMaxFiyat.Value = 0;
            sayfa = 1;
            UrunleriYukle();
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            string metin = txtArama.Text.Trim();
            if (metin.Length == 0 || metin.Length >= 2)
            {
                sayfa = 1;
                UrunleriYukle();
            }
        }

        private void TxtBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sayfa = 1;
                UrunleriYukle();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void CmbSinif1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSinif1.SelectedItem == null)
            {
                return;
            }

            string secim = cmbSinif1.SelectedItem.ToString();
            cmbSinif2.Items.Clear();
            cmbSinif2.Items.Add("Hepsi");

            if (secim != "Hepsi")
            {
                try
                {
                    DataTable sinif2ler = VeriKatmani.Sinif2leriGetir(secim);
                    foreach (DataRow row in sinif2ler.Rows)
                    {
                        cmbSinif2.Items.Add(row[0].ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kategori2 yüklenemedi. Detay: {ex.Message}", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            cmbSinif2.SelectedIndex = 0;
        }

        private void UrunleriYukle()
        {
            try
            {
                string arama = txtArama.Text.Trim();
                string barkod = txtBarkod.Text.Trim();
                string sinif1 = cmbSinif1.SelectedItem?.ToString();
                string sinif2 = cmbSinif2.SelectedItem?.ToString();
                string stokDurum = cmbStokDurum.SelectedItem?.ToString()?.ToLowerInvariant();

                decimal? minFiyat = nudMinFiyat.Value > 0 ? nudMinFiyat.Value : (decimal?)null;
                decimal? maxFiyat = nudMaxFiyat.Value > 0 ? nudMaxFiyat.Value : (decimal?)null;

                toplamKayit = VeriKatmani.UrunSayisiGetir(arama, barkod, sinif1, sinif2, stokDurum, minFiyat, maxFiyat);
                mevcutUrunler = VeriKatmani.UrunleriGetir(arama, barkod, sinif1, sinif2, stokDurum, minFiyat, maxFiyat, sayfa, sayfaBoyutu);

                dgvUrunler.DataSource = mevcutUrunler;
                UygulaSirala();

                lblSayfa.Text = $"Sayfa: {sayfa}/{ToplamSayfa()}";
                lblToplam.Text = $"Toplam: {toplamKayit} kayıt";
                btnOnceki.Enabled = sayfa > 1;
                btnSonraki.Enabled = sayfa < ToplamSayfa();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata oluştu. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UygulaSirala()
        {
            if (mevcutUrunler == null)
            {
                return;
            }

            string kolon = mevcutSortKolon;
            string yon = sortArtan ? "ASC" : "DESC";
            DataView view = mevcutUrunler.DefaultView;

            switch (kolon)
            {
                case "Kodu":
                    view.Sort = $"sKodu {yon}";
                    break;
                case "UrunAdi":
                    view.Sort = $"sAciklama {yon}";
                    break;
                case "Fiyat":
                    view.Sort = $"Fiyat {yon}";
                    break;
                case "Stok":
                    view.Sort = $"StokMiktari {yon}";
                    break;
            }
        }

        private void DgvUrunler_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string kolonAdi = dgvUrunler.Columns[e.ColumnIndex].Name;
            if (!new[] { "Kodu", "UrunAdi", "Fiyat", "Stok" }.Contains(kolonAdi))
            {
                return;
            }

            if (mevcutSortKolon == kolonAdi)
            {
                sortArtan = !sortArtan;
            }
            else
            {
                mevcutSortKolon = kolonAdi;
                sortArtan = true;
            }

            UygulaSirala();
        }

        private void DgvUrunler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count == 0)
            {
                return;
            }

            if (dgvUrunler.SelectedRows[0].Cells["ID"].Value == null)
            {
                return;
            }

            seciliStokId = Convert.ToInt32(dgvUrunler.SelectedRows[0].Cells["ID"].Value);
            UrunDetayYukle(seciliStokId.Value);
        }

        private void UrunDetayYukle(int stokId)
        {
            try
            {
                DataSet ds = VeriKatmani.UrunDetayGetir(stokId);
                if (ds.Tables["Urun"].Rows.Count == 0)
                {
                    return;
                }

                DataRow urun = ds.Tables["Urun"].Rows[0];
                lblDetayKod.Text = urun["sKodu"].ToString();
                lblDetayAd.Text = urun["sAciklama"].ToString();
                lblDetayBirim.Text = urun["sBirimCinsi1"].ToString();
                lblDetayKategori.Text = $"{urun["sSinifKodu1"]} / {urun["sSinifKodu2"]}";

                lstBarkodlar.Items.Clear();
                foreach (DataRow row in ds.Tables["Barkodlar"].Rows)
                {
                    lstBarkodlar.Items.Add(row["sBarkod"].ToString());
                }

                dgvFiyatlar.DataSource = ds.Tables["Fiyatlar"];

                if (ds.Tables["Stok"].Rows.Count > 0)
                {
                    lblDetayStok.Text = Convert.ToDecimal(ds.Tables["Stok"].Rows[0]["StokMiktari"]).ToString("N2");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay yüklenirken hata oluştu. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ToplamSayfa()
        {
            if (toplamKayit == 0)
            {
                return 1;
            }
            return (int)Math.Ceiling(toplamKayit / (decimal)sayfaBoyutu);
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            using (FormUrunDuzenle form = new FormUrunDuzenle())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    sayfa = 1;
                    UrunleriYukle();
                }
            }
        }

        private void BtnDuzenle_Click(object sender, EventArgs e)
        {
            if (!seciliStokId.HasValue)
            {
                MessageBox.Show("Lütfen düzenlenecek ürünü seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormUrunDuzenle form = new FormUrunDuzenle(seciliStokId.Value))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    UrunleriYukle();
                }
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (!seciliStokId.HasValue)
            {
                MessageBox.Show("Lütfen silinecek ürünü seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult sonuc = MessageBox.Show("Seçili ürün silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (sonuc != DialogResult.Yes)
            {
                return;
            }

            try
            {
                VeriKatmani.UrunSil(seciliStokId.Value, true);
                MessageBox.Show("Ürün silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UrunleriYukle();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme işlemi sırasında hata oluştu. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBarkod_Click(object sender, EventArgs e)
        {
            if (!seciliStokId.HasValue)
            {
                MessageBox.Show("Lütfen barkod yönetimi için ürün seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormBarkodYonetimi form = new FormBarkodYonetimi(seciliStokId.Value))
            {
                form.ShowDialog(this);
                UrunDetayYukle(seciliStokId.Value);
            }
        }

        private void BtnFiyat_Click(object sender, EventArgs e)
        {
            if (!seciliStokId.HasValue)
            {
                MessageBox.Show("Lütfen fiyat yönetimi için ürün seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormFiyatDuzenle form = new FormFiyatDuzenle(seciliStokId.Value))
            {
                form.ShowDialog(this);
                UrunDetayYukle(seciliStokId.Value);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F2:
                    txtArama.Focus();
                    return true;
                case Keys.F5:
                    UrunleriYukle();
                    return true;
                case Keys.Insert:
                    BtnEkle_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Enter:
                    BtnDuzenle_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Delete:
                    BtnSil_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Escape:
                    Close();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
