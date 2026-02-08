using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormTopluIslemler : Form
    {
        private TabControl tabControl;

        private TextBox txtAramaFiyat;
        private ComboBox cmbKat1Fiyat;
        private ComboBox cmbKat2Fiyat;
        private ComboBox cmbStokDurumFiyat;
        private ComboBox cmbFiyatTipiFiyat;
        private NumericUpDown nudYuzde;
        private ComboBox cmbYuvarlama;
        private Label lblOnizleme;
        private Button btnOnizleme;
        private Button btnTopluFiyatUygula;

        private TextBox txtAramaKategori;
        private ComboBox cmbKat1Kategori;
        private ComboBox cmbKat2Kategori;
        private ComboBox cmbStokDurumKategori;
        private DataGridView dgvKategoriUrunler;
        private Button btnKategoriYukle;
        private Button btnKategoriUygula;
        private TextBox txtYeniKat1;
        private TextBox txtYeniKat2;
        private int kategoriSayfa = 1;
        private Label lblKategoriSayfa;

        private DataGridView dgvBarkodsuz;
        private TextBox txtBarkod;
        private Button btnBarkodEkle;
        private Button btnBarkodOnceki;
        private Button btnBarkodSonraki;
        private Label lblBarkodSayfa;
        private int barkodSayfa = 1;

        private TextBox txtAramaExport;
        private ComboBox cmbKat1Export;
        private ComboBox cmbKat2Export;
        private ComboBox cmbStokDurumExport;
        private ComboBox cmbFiyatTipiExport;
        private Button btnExport;

        public FormTopluIslemler()
        {
            InitializeComponent();
            FiltreleriYukle();
        }

        private void InitializeComponent()
        {
            Text = "📦 Toplu İşlemler";
            Size = new Size(1350, 820);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;

            tabControl = new TabControl { Dock = DockStyle.Fill };
            TabPage tabFiyat = new TabPage("Toplu Fiyat");
            TabPage tabKategori = new TabPage("Toplu Kategori");
            TabPage tabBarkod = new TabPage("Barkod");
            TabPage tabExport = new TabPage("Export");

            tabControl.TabPages.AddRange(new[] { tabFiyat, tabKategori, tabBarkod, tabExport });

            InitializeTopluFiyatTab(tabFiyat);
            InitializeTopluKategoriTab(tabKategori);
            InitializeBarkodTab(tabBarkod);
            InitializeExportTab(tabExport);

            Controls.Add(tabControl);
        }

        private void InitializeTopluFiyatTab(TabPage tab)
        {
            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(10), BackColor = Color.FromArgb(248, 248, 248) };
            txtAramaFiyat = new TextBox { Width = 220, PlaceholderText = "Ürün adı/kod" };
            cmbKat1Fiyat = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKat2Fiyat = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKat1Fiyat.SelectedIndexChanged += (s, e) => Kat2leriYukle(cmbKat1Fiyat, cmbKat2Fiyat);
            cmbStokDurumFiyat = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStokDurumFiyat.Items.AddRange(new object[] { "Hepsi", "Var", "Yok" });
            cmbStokDurumFiyat.SelectedIndex = 0;
            cmbFiyatTipiFiyat = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            btnOnizleme = new Button { Text = "Önizleme", Width = 110 };
            btnOnizleme.Click += (s, e) => OnizlemeYukle();
            lblOnizleme = new Label { AutoSize = true, Text = "Etkilenecek: 0", Padding = new Padding(0, 6, 0, 0) };

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                txtAramaFiyat,
                new Label { Text = "Kat1:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbKat1Fiyat,
                new Label { Text = "Kat2:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbKat2Fiyat,
                new Label { Text = "Stok:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbStokDurumFiyat,
                new Label { Text = "Fiyat Tipi:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbFiyatTipiFiyat,
                btnOnizleme,
                lblOnizleme
            });
            filtrePanel.Controls.Add(filtreLayout);

            Panel islemPanel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(10) };
            nudYuzde = new NumericUpDown { Width = 90, Minimum = -99, Maximum = 999, DecimalPlaces = 2, Increment = 0.5m };
            cmbYuvarlama = new ComboBox { Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbYuvarlama.Items.AddRange(new object[] { "yok", "0.05", "0.10", "0.50", "1.00" });
            cmbYuvarlama.SelectedIndex = 0;
            btnTopluFiyatUygula = new Button { Text = "Uygula", Width = 100, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnTopluFiyatUygula.Click += BtnTopluFiyatUygula_Click;

            FlowLayoutPanel islemLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            islemLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Yüzde (%):", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                nudYuzde,
                new Label { Text = "Yuvarlama:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbYuvarlama,
                btnTopluFiyatUygula
            });
            islemPanel.Controls.Add(islemLayout);

            tab.Controls.Add(islemPanel);
            tab.Controls.Add(filtrePanel);
        }

        private void InitializeTopluKategoriTab(TabPage tab)
        {
            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(10), BackColor = Color.FromArgb(248, 248, 248) };
            txtAramaKategori = new TextBox { Width = 220, PlaceholderText = "Ürün adı/kod" };
            cmbKat1Kategori = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKat2Kategori = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKat1Kategori.SelectedIndexChanged += (s, e) => Kat2leriYukle(cmbKat1Kategori, cmbKat2Kategori);
            cmbStokDurumKategori = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStokDurumKategori.Items.AddRange(new object[] { "Hepsi", "Var", "Yok" });
            cmbStokDurumKategori.SelectedIndex = 0;
            btnKategoriYukle = new Button { Text = "Yükle", Width = 90 };
            btnKategoriYukle.Click += (s, e) => { kategoriSayfa = 1; KategoriUrunleriYukle(); };

            btnKategoriUygula = new Button { Text = "Uygula", Width = 90, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnKategoriUygula.Click += BtnKategoriUygula_Click;

            lblKategoriSayfa = new Label { AutoSize = true, Text = "Sayfa: 1", Padding = new Padding(0, 6, 0, 0) };

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                txtAramaKategori,
                new Label { Text = "Kat1:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbKat1Kategori,
                new Label { Text = "Kat2:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbKat2Kategori,
                new Label { Text = "Stok:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbStokDurumKategori,
                btnKategoriYukle,
                lblKategoriSayfa,
                btnKategoriUygula
            });
            filtrePanel.Controls.Add(filtreLayout);

            dgvKategoriUrunler = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvKategoriUrunler.ColumnHeadersHeight = 32;
            dgvKategoriUrunler.RowTemplate.Height = 28;
            Yardimcilar.DoubleBufferedAktifEt(dgvKategoriUrunler);
            dgvKategoriUrunler.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Sec", Width = 40 });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "nStokID", Visible = false });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kodu", DataPropertyName = "sKodu", Width = 120 });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "sAciklama", Width = 260 });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barkod", DataPropertyName = "sBarkod", Width = 140 });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stok", DataPropertyName = "StokMiktari", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kat1", HeaderText = "Kat1", DataPropertyName = "Kategori1", Width = 90 });
            dgvKategoriUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kat2", HeaderText = "Kat2", DataPropertyName = "Kategori2", Width = 90 });

            Panel kategoriPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(10) };
            txtYeniKat1 = new TextBox { Width = 140 };
            txtYeniKat2 = new TextBox { Width = 140 };
            Button btnOnceki = new Button { Text = "◀", Width = 40 };
            btnOnceki.Click += (s, e) => { if (kategoriSayfa > 1) { kategoriSayfa--; KategoriUrunleriYukle(); } };
            Button btnSonraki = new Button { Text = "▶", Width = 40 };
            btnSonraki.Click += (s, e) => { kategoriSayfa++; KategoriUrunleriYukle(); };

            FlowLayoutPanel kategoriLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            kategoriLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Yeni Kat1:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                txtYeniKat1,
                new Label { Text = "Yeni Kat2:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                txtYeniKat2,
                btnOnceki,
                btnSonraki
            });
            kategoriPanel.Controls.Add(kategoriLayout);

            tab.Controls.Add(dgvKategoriUrunler);
            tab.Controls.Add(kategoriPanel);
            tab.Controls.Add(filtrePanel);
        }

        private void InitializeBarkodTab(TabPage tab)
        {
            Panel ustPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10), BackColor = Color.FromArgb(248, 248, 248) };
            txtBarkod = new TextBox { Width = 160, PlaceholderText = "Yeni barkod" };
            btnBarkodEkle = new Button { Text = "Barkod Ekle", Width = 120, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnBarkodEkle.Click += BtnBarkodEkle_Click;
            btnBarkodOnceki = new Button { Text = "◀", Width = 40 };
            btnBarkodOnceki.Click += (s, e) => { if (barkodSayfa > 1) { barkodSayfa--; BarkodsuzUrunleriYukle(); } };
            btnBarkodSonraki = new Button { Text = "▶", Width = 40 };
            btnBarkodSonraki.Click += (s, e) => { barkodSayfa++; BarkodsuzUrunleriYukle(); };
            lblBarkodSayfa = new Label { AutoSize = true, Text = "Sayfa: 1", Padding = new Padding(0, 6, 0, 0) };

            FlowLayoutPanel ustLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            ustLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Barkod:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                txtBarkod,
                btnBarkodEkle,
                btnBarkodOnceki,
                btnBarkodSonraki,
                lblBarkodSayfa
            });
            ustPanel.Controls.Add(ustLayout);

            dgvBarkodsuz = new DataGridView
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
            dgvBarkodsuz.ColumnHeadersHeight = 32;
            dgvBarkodsuz.RowTemplate.Height = 28;
            Yardimcilar.DoubleBufferedAktifEt(dgvBarkodsuz);
            dgvBarkodsuz.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "nStokID", Visible = false });
            dgvBarkodsuz.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kodu", DataPropertyName = "sKodu", Width = 120 });
            dgvBarkodsuz.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "sAciklama", Width = 260 });
            dgvBarkodsuz.Columns.Add(new DataGridViewTextBoxColumn { Name = "Birim", DataPropertyName = "sBirimCinsi", Width = 90 });
            dgvBarkodsuz.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kat1", HeaderText = "Kat1", DataPropertyName = "Kategori1", Width = 100 });
            dgvBarkodsuz.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kat2", HeaderText = "Kat2", DataPropertyName = "Kategori2", Width = 100 });

            tab.Controls.Add(dgvBarkodsuz);
            tab.Controls.Add(ustPanel);

            BarkodsuzUrunleriYukle();
        }

        private void InitializeExportTab(TabPage tab)
        {
            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(10), BackColor = Color.FromArgb(248, 248, 248) };
            txtAramaExport = new TextBox { Width = 220, PlaceholderText = "Ürün adı/kod" };
            cmbKat1Export = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKat2Export = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKat1Export.SelectedIndexChanged += (s, e) => Kat2leriYukle(cmbKat1Export, cmbKat2Export);
            cmbStokDurumExport = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStokDurumExport.Items.AddRange(new object[] { "Hepsi", "Var", "Yok" });
            cmbStokDurumExport.SelectedIndex = 0;
            cmbFiyatTipiExport = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            btnExport = new Button { Text = "CSV Dışa Aktar", Width = 140, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnExport.Click += BtnExport_Click;

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                txtAramaExport,
                new Label { Text = "Kat1:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbKat1Export,
                new Label { Text = "Kat2:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbKat2Export,
                new Label { Text = "Stok:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbStokDurumExport,
                new Label { Text = "Fiyat Tipi:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbFiyatTipiExport,
                btnExport
            });
            filtrePanel.Controls.Add(filtreLayout);

            Label bilgi = new Label
            {
                Text = "Export kolonları: nStokID,sKodu,sAciklama,sBirimCinsi,sBarkod,lFiyat,StokMiktari,sSinifKodu1,sSinifKodu2",
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                ForeColor = Color.DimGray
            };

            tab.Controls.Add(bilgi);
            tab.Controls.Add(filtrePanel);
        }

        private void FiltreleriYukle()
        {
            DataTable fiyatTipleri = VeriKatmani.FiyatTipleriniGetir();
            cmbFiyatTipiFiyat.DataSource = fiyatTipleri.Copy();
            cmbFiyatTipiFiyat.DisplayMember = "sFiyatTipi";
            cmbFiyatTipiFiyat.ValueMember = "sFiyatTipi";

            cmbFiyatTipiExport.DataSource = fiyatTipleri.Copy();
            cmbFiyatTipiExport.DisplayMember = "sFiyatTipi";
            cmbFiyatTipiExport.ValueMember = "sFiyatTipi";

            DataTable kat1 = VeriKatmani.Sinif1leriGetir();
            kat1.Rows.InsertAt(kat1.NewRow(), 0);
            cmbKat1Fiyat.DataSource = kat1.Copy();
            cmbKat1Fiyat.DisplayMember = "sSinifKodu1";
            cmbKat1Kategori.DataSource = kat1.Copy();
            cmbKat1Kategori.DisplayMember = "sSinifKodu1";
            cmbKat1Export.DataSource = kat1.Copy();
            cmbKat1Export.DisplayMember = "sSinifKodu1";
            cmbKat1Fiyat.SelectedIndex = 0;
            cmbKat1Kategori.SelectedIndex = 0;
            cmbKat1Export.SelectedIndex = 0;

            Kat2leriYukle(cmbKat1Fiyat, cmbKat2Fiyat);
            Kat2leriYukle(cmbKat1Kategori, cmbKat2Kategori);
            Kat2leriYukle(cmbKat1Export, cmbKat2Export);

            if (!string.IsNullOrWhiteSpace(Ayarlar.VarsayilanFiyatTipi))
            {
                cmbFiyatTipiFiyat.SelectedValue = Ayarlar.VarsayilanFiyatTipi;
                cmbFiyatTipiExport.SelectedValue = Ayarlar.VarsayilanFiyatTipi;
            }
        }

        private void Kat2leriYukle(ComboBox cmbKat1, ComboBox cmbKat2)
        {
            string kat1 = cmbKat1.SelectedValue?.ToString();
            DataTable kat2 = string.IsNullOrWhiteSpace(kat1)
                ? new DataTable()
                : VeriKatmani.Sinif2leriGetir(kat1);
            if (kat2.Columns.Count == 0)
            {
                kat2.Columns.Add("sSinifKodu2");
            }
            kat2.Rows.InsertAt(kat2.NewRow(), 0);
            cmbKat2.DataSource = kat2;
            cmbKat2.DisplayMember = "sSinifKodu2";
            cmbKat2.SelectedIndex = 0;
        }

        private void OnizlemeYukle()
        {
            string arama = txtAramaFiyat.Text;
            string kat1 = cmbKat1Fiyat.SelectedValue?.ToString();
            string kat2 = cmbKat2Fiyat.SelectedValue?.ToString();
            string stokDurum = cmbStokDurumFiyat.SelectedItem?.ToString();
            string fiyatTipi = cmbFiyatTipiFiyat.SelectedValue?.ToString();

            DataTable dt = VeriKatmani.UrunIdleriGetirFiltreli(arama, kat1, kat2, null, null, stokDurum, fiyatTipi, 1, 200);
            lblOnizleme.Text = $"Etkilenecek (ilk sayfa): {dt.Rows.Count}";
        }

        private void BtnTopluFiyatUygula_Click(object sender, EventArgs e)
        {
            string arama = txtAramaFiyat.Text;
            string kat1 = cmbKat1Fiyat.SelectedValue?.ToString();
            string kat2 = cmbKat2Fiyat.SelectedValue?.ToString();
            string stokDurum = cmbStokDurumFiyat.SelectedItem?.ToString();
            string fiyatTipi = cmbFiyatTipiFiyat.SelectedValue?.ToString();
            decimal yuzde = nudYuzde.Value;
            string yuvarlama = cmbYuvarlama.SelectedItem?.ToString();

            BeklemedeCalistir(() =>
            {
                int toplam = 0;
                int sayfa = 1;
                while (true)
                {
                    DataTable dt = VeriKatmani.UrunIdleriGetirFiltreli(arama, kat1, kat2, null, null, stokDurum, fiyatTipi, sayfa, 200);
                    if (dt.Rows.Count == 0)
                    {
                        break;
                    }

                    List<int> ids = dt.AsEnumerable().Select(r => Convert.ToInt32(r["nStokID"])).ToList();
                    toplam += VeriKatmani.TopluFiyatGuncelleYuzde(ids, fiyatTipi, yuzde, yuvarlama);
                    sayfa++;
                }

                MessageBox.Show($"{toplam} ürün güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void KategoriUrunleriYukle()
        {
            BeklemedeCalistir(() =>
            {
                string arama = txtAramaKategori.Text;
                string kat1 = cmbKat1Kategori.SelectedValue?.ToString();
                string kat2 = cmbKat2Kategori.SelectedValue?.ToString();
                string stokDurum = cmbStokDurumKategori.SelectedItem?.ToString();

                DataTable dt = VeriKatmani.UrunIdleriGetirFiltreli(arama, kat1, kat2, null, null, stokDurum, kategoriSayfa, 200);
                dgvKategoriUrunler.DataSource = dt;
                lblKategoriSayfa.Text = $"Sayfa: {kategoriSayfa}";
            });
        }

        private void BtnKategoriUygula_Click(object sender, EventArgs e)
        {
            List<int> seciliIds = new List<int>();
            foreach (DataGridViewRow row in dgvKategoriUrunler.Rows)
            {
                bool secili = row.Cells["Sec"].Value is bool b && b;
                if (secili)
                {
                    seciliIds.Add(Convert.ToInt32(row.Cells["ID"].Value));
                }
            }

            if (seciliIds.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir ürün seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string yeniKat1 = txtYeniKat1.Text.Trim();
            string yeniKat2 = txtYeniKat2.Text.Trim();

            BeklemedeCalistir(() =>
            {
                int adet = VeriKatmani.TopluKategoriAta(seciliIds, yeniKat1, yeniKat2);
                MessageBox.Show($"{adet} ürün güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void BarkodsuzUrunleriYukle()
        {
            BeklemedeCalistir(() =>
            {
                DataTable dt = VeriKatmani.BarkodsuzUrunleriGetir(barkodSayfa, 100);
                dgvBarkodsuz.DataSource = dt;
                lblBarkodSayfa.Text = $"Sayfa: {barkodSayfa}";
            });
        }

        private void BtnBarkodEkle_Click(object sender, EventArgs e)
        {
            if (dgvBarkodsuz.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen ürün seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string barkod = txtBarkod.Text.Trim();
            if (string.IsNullOrWhiteSpace(barkod))
            {
                MessageBox.Show("Barkod boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int stokId = Convert.ToInt32(dgvBarkodsuz.SelectedRows[0].Cells["ID"].Value);
            BeklemedeCalistir(() =>
            {
                VeriKatmani.BarkodEkle(stokId, barkod);
            });

            txtBarkod.Clear();
            BarkodsuzUrunleriYukle();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV (*.csv)|*.csv";
                dialog.FileName = $"urunler_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string arama = txtAramaExport.Text;
                string kat1 = cmbKat1Export.SelectedValue?.ToString();
                string kat2 = cmbKat2Export.SelectedValue?.ToString();
                string stokDurum = cmbStokDurumExport.SelectedItem?.ToString();
                string fiyatTipi = cmbFiyatTipiExport.SelectedValue?.ToString();

                BeklemedeCalistir(() =>
                {
                    DataTable dt = VeriKatmani.UrunleriExportIcinGetir(arama, kat1, kat2, fiyatTipi, stokDurum);
                    string csv = CsvOlustur(dt);
                    File.WriteAllText(dialog.FileName, csv, Encoding.UTF8);
                });

                MessageBox.Show("CSV dosyası oluşturuldu.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string CsvOlustur(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string[] kolonlar = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            sb.AppendLine(string.Join(",", kolonlar.Select(CsvDegeri)));

            foreach (DataRow row in dt.Rows)
            {
                string[] degerler = kolonlar.Select(k => CsvDegeri(row[k])).ToArray();
                sb.AppendLine(string.Join(",", degerler));
            }

            return sb.ToString();
        }

        private string CsvDegeri(object deger)
        {
            string metin = deger == null || deger == DBNull.Value ? string.Empty : deger.ToString();
            if (metin.Contains("\""))
            {
                metin = metin.Replace("\"", "\"\"");
            }
            if (metin.Contains(",") || metin.Contains("\"") || metin.Contains("\n"))
            {
                metin = $"\"{metin}\"";
            }
            return metin;
        }

        private void BeklemedeCalistir(Action islem)
        {
            try
            {
                UseWaitCursor = true;
                Enabled = false;
                islem();
            }
            finally
            {
                Enabled = true;
                UseWaitCursor = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
