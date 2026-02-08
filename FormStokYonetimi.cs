using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormStokYonetimi : Form
    {
        private ComboBox cmbDepo;
        private Label lblDurum;
        private Button btnGenelYenile;

        private TabControl tabControl;
        private TabPage tabStokDurum;
        private TabPage tabGirisCikis;
        private TabPage tabHareketler;
        private TabPage tabSayim;

        private TextBox txtStokArama;
        private TextBox txtStokBarkod;
        private ComboBox cmbStokDurum;
        private NumericUpDown nudKritikEsik;
        private DataGridView dgvStokDurum;
        private Button btnStokYenile;
        private Button btnStokHareket;
        private Timer aramaTimer;
        private int stokSayfa = 1;

        private TextBox txtGirisCikisBarkod;
        private Label lblGirisCikisUrun;
        private NumericUpDown nudGirisCikisMiktar;
        private RadioButton rbGiris;
        private RadioButton rbCikis;
        private TextBox txtGirisCikisAciklama;
        private Button btnGirisCikisKaydet;
        private int? girisCikisStokId;

        private TextBox txtHareketStokId;
        private ComboBox cmbHareketDepo;
        private DateTimePicker dtBaslangic;
        private DateTimePicker dtBitis;
        private DataGridView dgvHareketler;
        private Button btnHareketYenile;
        private int hareketSayfa = 1;

        private TextBox txtSayimArama;
        private ComboBox cmbSayimDepo;
        private DataGridView dgvSayim;
        private Button btnSayimYenile;
        private Button btnSayimFarkHesapla;
        private Button btnSayimDuzelt;
        private Button btnSayimTemizle;
        private int sayimSayfa = 1;
        private DataTable sayimTablo;

        private int? seciliStokId;

        public FormStokYonetimi()
        {
            InitializeComponent();
            DepolariYukle();
            StokDurumunuYukle();
        }

        private void InitializeComponent()
        {
            Text = "📋 Stok Yönetimi";
            Size = new Size(1350, 800);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;

            Panel ustPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            cmbDepo = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepo.SelectedIndexChanged += (s, e) => GenelYenile();

            btnGenelYenile = new Button { Text = "Yenile (F5)", Width = 120, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnGenelYenile.Click += (s, e) => GenelYenile();

            lblDurum = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "Hazır" };

            FlowLayoutPanel ustLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            ustLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Depo:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 0, 0) },
                cmbDepo,
                btnGenelYenile,
                lblDurum
            });

            ustPanel.Controls.Add(ustLayout);

            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabStokDurum = new TabPage("Stok Durumu");
            tabGirisCikis = new TabPage("Stok Girişi/Çıkışı");
            tabHareketler = new TabPage("Hareketler");
            tabSayim = new TabPage("Sayım");

            tabControl.TabPages.AddRange(new[] { tabStokDurum, tabGirisCikis, tabHareketler, tabSayim });

            InitializeStokDurumTab();
            InitializeGirisCikisTab();
            InitializeHareketlerTab();
            InitializeSayimTab();

            Controls.Add(tabControl);
            Controls.Add(ustPanel);
        }

        private void InitializeStokDurumTab()
        {
            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(10), BackColor = Color.FromArgb(250, 250, 250) };

            txtStokArama = new TextBox { Width = 220, PlaceholderText = "Ürün adı/kod (min 2)" };
            txtStokArama.TextChanged += TxtStokArama_TextChanged;

            txtStokBarkod = new TextBox { Width = 160, PlaceholderText = "Barkod (tam eşleşme)" };
            txtStokBarkod.KeyDown += TxtStokBarkod_KeyDown;

            cmbStokDurum = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStokDurum.Items.AddRange(new object[] { "Hepsi", "Var", "Yok", "Az" });
            cmbStokDurum.SelectedIndex = 0;

            nudKritikEsik = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 1000, Value = 5 };

            btnStokYenile = new Button { Text = "Yenile", Width = 90 };
            btnStokYenile.Click += (s, e) => { stokSayfa = 1; StokDurumunuYukle(); };

            btnStokHareket = new Button { Text = "Seçili -> Hareketler", Width = 160 };
            btnStokHareket.Click += BtnStokHareket_Click;

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                txtStokArama,
                new Label { Text = "Barkod:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                txtStokBarkod,
                new Label { Text = "Durum:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                cmbStokDurum,
                new Label { Text = "Kritik:", Width = 50, TextAlign = ContentAlignment.MiddleLeft },
                nudKritikEsik,
                btnStokYenile,
                btnStokHareket
            });

            filtrePanel.Controls.Add(filtreLayout);

            dgvStokDurum = new DataGridView
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
            dgvStokDurum.ColumnHeadersHeight = 32;
            dgvStokDurum.RowTemplate.Height = 28;
            dgvStokDurum.DataBindingComplete += (s, e) => dgvStokDurum.ClearSelection();
            dgvStokDurum.SelectionChanged += DgvStokDurum_SelectionChanged;
            dgvStokDurum.CellFormatting += DgvStokDurum_CellFormatting;
            Yardimcilar.DoubleBufferedAktifEt(dgvStokDurum);

            dgvStokDurum.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "nStokID", Visible = false });
            dgvStokDurum.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kodu", DataPropertyName = "sKodu", Width = 120 });
            dgvStokDurum.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "sAciklama", Width = 260 });
            dgvStokDurum.Columns.Add(new DataGridViewTextBoxColumn { Name = "Birim", DataPropertyName = "sBirimCinsi", Width = 90 });
            dgvStokDurum.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barkod", DataPropertyName = "sBarkod", Width = 140 });
            dgvStokDurum.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stok", DataPropertyName = "StokMiktari", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            tabStokDurum.Controls.Add(dgvStokDurum);
            tabStokDurum.Controls.Add(filtrePanel);

            aramaTimer = new Timer { Interval = 300 };
            aramaTimer.Tick += (s, e) =>
            {
                aramaTimer.Stop();
                if (txtStokArama.Text.Length >= 2 || string.IsNullOrWhiteSpace(txtStokArama.Text))
                {
                    stokSayfa = 1;
                    StokDurumunuYukle();
                }
            };
        }

        private void InitializeGirisCikisTab()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            txtGirisCikisBarkod = new TextBox { Width = 200 };
            txtGirisCikisBarkod.KeyDown += TxtGirisCikisBarkod_KeyDown;
            lblGirisCikisUrun = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "Ürün seçilmedi" };
            nudGirisCikisMiktar = new NumericUpDown { Width = 120, DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, Increment = 1, Value = 1 };

            rbGiris = new RadioButton { Text = "Giriş", Checked = true, AutoSize = true };
            rbCikis = new RadioButton { Text = "Çıkış", AutoSize = true };

            txtGirisCikisAciklama = new TextBox { Width = 320 };
            btnGirisCikisKaydet = new Button { Text = "Kaydet", Width = 120, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnGirisCikisKaydet.Click += BtnGirisCikisKaydet_Click;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 7,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label { Text = "Barkod:", AutoSize = true }, 0, 0);
            layout.Controls.Add(txtGirisCikisBarkod, 1, 0);
            layout.Controls.Add(new Label { Text = "Ürün:", AutoSize = true }, 0, 1);
            layout.Controls.Add(lblGirisCikisUrun, 1, 1);
            layout.Controls.Add(new Label { Text = "Miktar:", AutoSize = true }, 0, 2);
            layout.Controls.Add(nudGirisCikisMiktar, 1, 2);

            FlowLayoutPanel tipPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            tipPanel.Controls.AddRange(new Control[] { rbGiris, rbCikis });
            layout.Controls.Add(new Label { Text = "İşlem:", AutoSize = true }, 0, 3);
            layout.Controls.Add(tipPanel, 1, 3);

            layout.Controls.Add(new Label { Text = "Açıklama:", AutoSize = true }, 0, 4);
            layout.Controls.Add(txtGirisCikisAciklama, 1, 4);
            layout.Controls.Add(btnGirisCikisKaydet, 1, 5);

            panel.Controls.Add(layout);
            tabGirisCikis.Controls.Add(panel);
        }

        private void InitializeHareketlerTab()
        {
            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(10), BackColor = Color.FromArgb(250, 250, 250) };

            txtHareketStokId = new TextBox { Width = 80 };
            cmbHareketDepo = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbHareketDepo.Items.Add("Tümü");
            cmbHareketDepo.SelectedIndex = 0;

            dtBaslangic = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            dtBitis = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            btnHareketYenile = new Button { Text = "Yenile", Width = 90 };
            btnHareketYenile.Click += (s, e) => { hareketSayfa = 1; HareketleriYukle(); };

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Stok ID:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                txtHareketStokId,
                new Label { Text = "Depo:", Width = 50, TextAlign = ContentAlignment.MiddleLeft },
                cmbHareketDepo,
                new Label { Text = "Başlangıç:", Width = 70, TextAlign = ContentAlignment.MiddleLeft },
                dtBaslangic,
                new Label { Text = "Bitiş:", Width = 40, TextAlign = ContentAlignment.MiddleLeft },
                dtBitis,
                btnHareketYenile
            });

            filtrePanel.Controls.Add(filtreLayout);

            dgvHareketler = new DataGridView
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
            dgvHareketler.ColumnHeadersHeight = 32;
            dgvHareketler.RowTemplate.Height = 28;
            dgvHareketler.DataBindingComplete += (s, e) => dgvHareketler.ClearSelection();
            Yardimcilar.DoubleBufferedAktifEt(dgvHareketler);

            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tarih", DataPropertyName = "Tarih", Width = 120 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "GirisCikis", HeaderText = "G/Ç", DataPropertyName = "GirisCikis", Width = 60 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "GirisMiktar", HeaderText = "Giriş", DataPropertyName = "GirisMiktar1", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "CikisMiktar", HeaderText = "Çıkış", DataPropertyName = "CikisMiktar1", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "BirimFiyat", HeaderText = "Fiyat", DataPropertyName = "BirimFiyat", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tutar", DataPropertyName = "Tutar", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "FisTipi", HeaderText = "Fiş Tipi", DataPropertyName = "FisTipi", Width = 90 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Aciklama", DataPropertyName = "Aciklama", Width = 200 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "FisNo", HeaderText = "Fiş No", DataPropertyName = "FisNo", Width = 90 });

            tabHareketler.Controls.Add(dgvHareketler);
            tabHareketler.Controls.Add(filtrePanel);
        }

        private void InitializeSayimTab()
        {
            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(10), BackColor = Color.FromArgb(250, 250, 250) };

            txtSayimArama = new TextBox { Width = 220, PlaceholderText = "Ürün adı/kod (min 2)" };
            txtSayimArama.TextChanged += TxtSayimArama_TextChanged;

            cmbSayimDepo = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

            btnSayimYenile = new Button { Text = "Listele", Width = 90 };
            btnSayimYenile.Click += (s, e) => { sayimSayfa = 1; SayimListesiniYukle(); };

            btnSayimFarkHesapla = new Button { Text = "Farkları Hesapla", Width = 130 };
            btnSayimFarkHesapla.Click += (s, e) => SayimFarklariniGuncelle();

            btnSayimDuzelt = new Button { Text = "Düzeltme Fişi Yaz", Width = 150, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnSayimDuzelt.Click += BtnSayimDuzelt_Click;

            btnSayimTemizle = new Button { Text = "Temizle", Width = 90 };
            btnSayimTemizle.Click += (s, e) => SayimTemizle();

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", Width = 60, TextAlign = ContentAlignment.MiddleLeft },
                txtSayimArama,
                new Label { Text = "Depo:", Width = 50, TextAlign = ContentAlignment.MiddleLeft },
                cmbSayimDepo,
                btnSayimYenile,
                btnSayimFarkHesapla,
                btnSayimDuzelt,
                btnSayimTemizle
            });

            filtrePanel.Controls.Add(filtreLayout);

            dgvSayim = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvSayim.ColumnHeadersHeight = 32;
            dgvSayim.RowTemplate.Height = 28;
            dgvSayim.DataBindingComplete += (s, e) => dgvSayim.ClearSelection();
            dgvSayim.CellEndEdit += (s, e) => SayimFarklariniGuncelle();
            Yardimcilar.DoubleBufferedAktifEt(dgvSayim);

            dgvSayim.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "nStokID", Visible = false });
            dgvSayim.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kodu", DataPropertyName = "sKodu", Width = 120 });
            dgvSayim.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "sAciklama", Width = 240 });
            dgvSayim.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mevcut", HeaderText = "Mevcut", DataPropertyName = "StokMiktari", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvSayim.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sayim", HeaderText = "Sayım", DataPropertyName = "SayimMiktari", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvSayim.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fark", DataPropertyName = "Fark", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            tabSayim.Controls.Add(dgvSayim);
            tabSayim.Controls.Add(filtrePanel);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                if (tabControl.SelectedTab == tabStokDurum)
                {
                    txtStokArama.Focus();
                    return true;
                }
            }

            if (keyData == Keys.F5)
            {
                GenelYenile();
                return true;
            }

            if (keyData == (Keys.Control | Keys.D1))
            {
                tabControl.SelectedIndex = 0;
                return true;
            }
            if (keyData == (Keys.Control | Keys.D2))
            {
                tabControl.SelectedIndex = 1;
                return true;
            }
            if (keyData == (Keys.Control | Keys.D3))
            {
                tabControl.SelectedIndex = 2;
                return true;
            }
            if (keyData == (Keys.Control | Keys.D4))
            {
                tabControl.SelectedIndex = 3;
                return true;
            }

            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DepolariYukle()
        {
            try
            {
                DataTable depolar = VeriKatmani.DepolariGetir();
                cmbDepo.DisplayMember = "sDepo";
                cmbDepo.ValueMember = "sDepo";
                cmbDepo.DataSource = depolar;

                cmbHareketDepo.Items.Clear();
                cmbHareketDepo.Items.Add("Tümü");
                foreach (DataRow row in depolar.Rows)
                {
                    cmbHareketDepo.Items.Add(row["sDepo"].ToString());
                }
                cmbHareketDepo.SelectedIndex = 0;

                cmbSayimDepo.Items.Clear();
                cmbSayimDepo.Items.Add("Tümü");
                foreach (DataRow row in depolar.Rows)
                {
                    cmbSayimDepo.Items.Add(row["sDepo"].ToString());
                }
                cmbSayimDepo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Depolar yüklenemedi. Detay: {ex.Message}", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TxtStokArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void TxtStokBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                stokSayfa = 1;
                StokDurumunuYukle();
            }
        }

        private void DgvStokDurum_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStokDurum.CurrentRow != null)
            {
                seciliStokId = Convert.ToInt32(dgvStokDurum.CurrentRow.Cells["ID"].Value);
            }
        }

        private void DgvStokDurum_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvStokDurum.Columns[e.ColumnIndex].Name == "Stok" && e.Value != null)
            {
                decimal stok = Convert.ToDecimal(e.Value);
                if (stok > 0 && stok < nudKritikEsik.Value)
                {
                    dgvStokDurum.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 242, 204);
                }
            }
        }

        private void BtnStokHareket_Click(object sender, EventArgs e)
        {
            if (!seciliStokId.HasValue)
            {
                MessageBox.Show("Önce stok seçmelisin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtHareketStokId.Text = seciliStokId.Value.ToString();
            tabControl.SelectedTab = tabHareketler;
            hareketSayfa = 1;
            HareketleriYukle();
        }

        private void StokDurumunuYukle()
        {
            try
            {
                string depo = cmbDepo.SelectedValue?.ToString();
                string stokDurum = cmbStokDurum.SelectedItem?.ToString() ?? "Hepsi";
                DataTable dt = VeriKatmani.StokDurumuGetir(
                    txtStokArama.Text.Trim(),
                    txtStokBarkod.Text.Trim(),
                    depo,
                    stokDurum.ToLowerInvariant(),
                    Convert.ToInt32(nudKritikEsik.Value),
                    stokSayfa);

                dgvStokDurum.DataSource = dt;
                lblDurum.Text = $"{dt.Rows.Count} kayıt listelendi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stok durumu yüklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtGirisCikisBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            try
            {
                DataTable sonuc = VeriKatmani.BarkodIleUrunBul(txtGirisCikisBarkod.Text.Trim());
                if (sonuc.Rows.Count == 0)
                {
                    lblGirisCikisUrun.Text = "Ürün bulunamadı.";
                    girisCikisStokId = null;
                    return;
                }

                DataRow row = sonuc.Rows[0];
                girisCikisStokId = Convert.ToInt32(row["nStokID"]);
                lblGirisCikisUrun.Text = $"{row["sKodu"]} - {row["sAciklama"]}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün bulunamadı. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGirisCikisKaydet_Click(object sender, EventArgs e)
        {
            if (!girisCikisStokId.HasValue)
            {
                MessageBox.Show("Ürün bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudGirisCikisMiktar.Value <= 0)
            {
                MessageBox.Show("Miktar 0 olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string depo = cmbDepo.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(depo))
            {
                MessageBox.Show("Depo seçmelisin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int fisId;
                if (rbGiris.Checked)
                {
                    fisId = VeriKatmani.StokGirisiYap(girisCikisStokId.Value, nudGirisCikisMiktar.Value, depo, txtGirisCikisAciklama.Text.Trim());
                }
                else
                {
                    fisId = VeriKatmani.StokCikisiYap(girisCikisStokId.Value, nudGirisCikisMiktar.Value, depo, txtGirisCikisAciklama.Text.Trim());
                }

                MessageBox.Show($"Fiş oluşturuldu: {fisId}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StokDurumunuYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fiş oluşturulamadı. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HareketleriYukle()
        {
            if (!int.TryParse(txtHareketStokId.Text.Trim(), out int stokId))
            {
                MessageBox.Show("Stok ID geçersiz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string depo = cmbHareketDepo.SelectedItem?.ToString();
                if (depo == "Tümü")
                {
                    depo = null;
                }

                DateTime? baslangic = dtBaslangic.Checked ? dtBaslangic.Value.Date : (DateTime?)null;
                DateTime? bitis = dtBitis.Checked ? dtBitis.Value.Date : (DateTime?)null;

                DataTable dt = VeriKatmani.StokHareketleriGetir(stokId, depo, baslangic, bitis, hareketSayfa);
                dgvHareketler.DataSource = dt;
                lblDurum.Text = $"{dt.Rows.Count} hareket listelendi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hareketler yüklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSayimArama_TextChanged(object sender, EventArgs e)
        {
            if (txtSayimArama.Text.Length < 2 && !string.IsNullOrWhiteSpace(txtSayimArama.Text))
            {
                return;
            }

            sayimSayfa = 1;
            SayimListesiniYukle();
        }

        private void SayimListesiniYukle()
        {
            try
            {
                string depo = cmbSayimDepo.SelectedItem?.ToString();
                if (depo == "Tümü")
                {
                    depo = null;
                }

                sayimTablo = VeriKatmani.SayimIcinUrunleriGetir(txtSayimArama.Text.Trim(), depo, sayimSayfa);

                if (!sayimTablo.Columns.Contains("SayimMiktari"))
                {
                    sayimTablo.Columns.Add("SayimMiktari", typeof(decimal));
                }
                if (!sayimTablo.Columns.Contains("Fark"))
                {
                    sayimTablo.Columns.Add("Fark", typeof(decimal));
                }

                foreach (DataRow row in sayimTablo.Rows)
                {
                    if (row["SayimMiktari"] == DBNull.Value)
                    {
                        row["SayimMiktari"] = 0m;
                    }
                }

                dgvSayim.DataSource = sayimTablo;
                SayimFarklariniGuncelle();
                lblDurum.Text = $"{sayimTablo.Rows.Count} ürün listelendi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayım listesi yüklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SayimFarklariniGuncelle()
        {
            if (sayimTablo == null)
            {
                return;
            }

            foreach (DataRow row in sayimTablo.Rows)
            {
                decimal mevcut = row["StokMiktari"] == DBNull.Value ? 0m : Convert.ToDecimal(row["StokMiktari"]);
                decimal sayim = row["SayimMiktari"] == DBNull.Value ? 0m : Convert.ToDecimal(row["SayimMiktari"]);
                row["Fark"] = sayim - mevcut;
            }
        }

        private void BtnSayimDuzelt_Click(object sender, EventArgs e)
        {
            if (sayimTablo == null || sayimTablo.Rows.Count == 0)
            {
                MessageBox.Show("Önce sayım listesi oluşturmalısın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<(int stokId, decimal farkMiktar, string satirAciklama)> farklar = new List<(int stokId, decimal farkMiktar, string satirAciklama)>();
            decimal toplamPozitif = 0m;
            decimal toplamNegatif = 0m;

            foreach (DataRow row in sayimTablo.Rows)
            {
                decimal fark = row["Fark"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Fark"]);
                if (Math.Abs(fark) > 0)
                {
                    int stokId = Convert.ToInt32(row["nStokID"]);
                    string aciklama = row["sAciklama"].ToString();
                    farklar.Add((stokId, fark, aciklama));
                    if (fark > 0)
                    {
                        toplamPozitif += fark;
                    }
                    else
                    {
                        toplamNegatif += Math.Abs(fark);
                    }
                }
            }

            if (farklar.Count == 0)
            {
                MessageBox.Show("Fark bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult onay = MessageBox.Show(
                $"{farklar.Count} ürün için düzeltme fişi yazılacak.\nPozitif: {toplamPozitif:N2} / Negatif: {toplamNegatif:N2}\nDevam edilsin mi?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (onay != DialogResult.Yes)
            {
                return;
            }

            string depo = cmbSayimDepo.SelectedItem?.ToString();
            if (depo == "Tümü")
            {
                depo = cmbDepo.SelectedValue?.ToString();
            }

            if (string.IsNullOrWhiteSpace(depo))
            {
                MessageBox.Show("Depo seçmelisin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var sonuc = VeriKatmani.SayimDuzeltmeFisYaz(depo, "Sayım düzeltme", farklar);
                MessageBox.Show($"Düzeltme fişi yazıldı. Giriş: {sonuc.girisFisId?.ToString() ?? "-"}, Çıkış: {sonuc.cikisFisId?.ToString() ?? "-"}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Düzeltme fişi yazılamadı. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SayimTemizle()
        {
            sayimTablo?.Clear();
            dgvSayim.DataSource = sayimTablo;
        }

        private void GenelYenile()
        {
            if (tabControl.SelectedTab == tabStokDurum)
            {
                StokDurumunuYukle();
            }
            else if (tabControl.SelectedTab == tabHareketler)
            {
                HareketleriYukle();
            }
            else if (tabControl.SelectedTab == tabSayim)
            {
                SayimListesiniYukle();
            }
        }
    }
}
