using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormSatis : Form
    {
        private DataTable sepet;
        private decimal toplamTutar = 0;
        private int musteriId = 0;

        // UI Bileşenleri
        private TextBox txtBarkod;
        private TextBox txtArama;
        private DataGridView dgvSepet;
        private Label lblToplam;
        private Label lblMusteriAdi;
        private Label lblUrunSayisi;
        private Button btnOdemeAl;
        private Button btnSepetTemizle;
        private Button btnUrunSil;
        private Button btnMusteriSec;
        private Button btnUrunler;
        private Button btnRaporlar;
        private Button btnAyarlar;
        private NumericUpDown nudMiktar;
        private ListBox lstAramaSonuc;

        public FormSatis()
        {
            InitializeComponent();
            SepetOlustur();
            this.KeyPreview = true;
        }

        private void InitializeComponent()
        {
            this.Text = "KARAKAŞ MARKET - Satış Ekranı";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 10);
            this.WindowState = FormWindowState.Maximized;

            // ÜST PANEL
            Panel panelUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblBaslik = new Label
            {
                Text = "🛒 KARAKAŞ MARKET",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            Label lblTarih = new Label
            {
                Text = DateTime.Now.ToString("dd MMMM yyyy dddd - HH:mm"),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(350, 25)
            };

            btnUrunler = CreateMenuButton("📦 Ürünler", 650);
            btnUrunler.Click += BtnUrunler_Click;

            btnRaporlar = CreateMenuButton("📊 Raporlar", 780);
            btnRaporlar.Click += BtnRaporlar_Click;

            btnAyarlar = CreateMenuButton("⚙️ Ayarlar", 910);
            btnAyarlar.Click += BtnAyarlar_Click;

            panelUst.Controls.AddRange(new Control[] { lblBaslik, lblTarih, btnUrunler, btnRaporlar, btnAyarlar });

            // SOL PANEL
            Panel panelSol = new Panel
            {
                Dock = DockStyle.Left,
                Width = 380,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            Label lblBarkod = new Label
            {
                Text = "📷 Barkod Okut / Gir:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            txtBarkod = new TextBox
            {
                Font = new Font("Segoe UI", 16),
                Location = new Point(15, 45),
                Size = new Size(340, 40)
            };
            txtBarkod.KeyDown += TxtBarkod_KeyDown;

            Label lblMiktar = new Label
            {
                Text = "Miktar:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(15, 95),
                AutoSize = true
            };

            nudMiktar = new NumericUpDown
            {
                Font = new Font("Segoe UI", 14),
                Location = new Point(15, 125),
                Size = new Size(120, 35),
                Minimum = 0.001m,
                Maximum = 9999,
                Value = 1,
                DecimalPlaces = 3
            };

            Label lblArama = new Label
            {
                Text = "🔍 Ürün Ara:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(15, 175),
                AutoSize = true
            };

            txtArama = new TextBox
            {
                Font = new Font("Segoe UI", 12),
                Location = new Point(15, 205),
                Size = new Size(340, 30)
            };
            txtArama.TextChanged += TxtArama_TextChanged;

            lstAramaSonuc = new ListBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 240),
                Size = new Size(340, 220),
                Visible = false
            };
            lstAramaSonuc.DoubleClick += LstAramaSonuc_DoubleClick;

            GroupBox grpMusteri = new GroupBox
            {
                Text = "👤 Müşteri",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 470),
                Size = new Size(340, 120)
            };

            lblMusteriAdi = new Label
            {
                Text = "Peşin Satış",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(10, 30),
                Size = new Size(320, 30)
            };

            btnMusteriSec = new Button
            {
                Text = "Müşteri Seç (F5)",
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 70),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMusteriSec.Click += BtnMusteriSec_Click;

            Button btnMusteriTemizle = new Button
            {
                Text = "Temizle",
                Font = new Font("Segoe UI", 10),
                Location = new Point(170, 70),
                Size = new Size(90, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMusteriTemizle.Click += (s, e) => { musteriId = 0; lblMusteriAdi.Text = "Peşin Satış"; };

            grpMusteri.Controls.AddRange(new Control[] { lblMusteriAdi, btnMusteriSec, btnMusteriTemizle });

            panelSol.Controls.AddRange(new Control[] { 
                lblBarkod, txtBarkod, lblMiktar, nudMiktar, 
                lblArama, txtArama, lstAramaSonuc, grpMusteri 
            });

            // SAĞ PANEL
            Panel panelSag = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            Label lblSepet = new Label
            {
                Text = "🛒 SEPET",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(15, 10),
                AutoSize = true
            };

            lblUrunSayisi = new Label
            {
                Text = "(0 ürün)",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Location = new Point(120, 15),
                AutoSize = true
            };

            dgvSepet = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(900, 450),
                Font = new Font("Segoe UI", 11),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dgvSepet.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvSepet.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 122, 204);
            dgvSepet.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSepet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvSepet.ColumnHeadersHeight = 40;
            dgvSepet.RowTemplate.Height = 35;
            dgvSepet.EnableHeadersVisualStyles = false;

            panelSag.Controls.AddRange(new Control[] { lblSepet, lblUrunSayisi, dgvSepet });

            // ALT PANEL
            Panel panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            lblToplam = new Label
            {
                Text = "TOPLAM: ₺0,00",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 83),
                Location = new Point(30, 35),
                AutoSize = true
            };

            btnUrunSil = new Button
            {
                Text = "🗑️ Sil (DEL)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(140, 55),
                Location = new Point(550, 38),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUrunSil.Click += BtnUrunSil_Click;

            btnSepetTemizle = new Button
            {
                Text = "🔄 Temizle",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(140, 55),
                Location = new Point(710, 38),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSepetTemizle.Click += BtnSepetTemizle_Click;

            btnOdemeAl = new Button
            {
                Text = "💰 ÖDEME AL (F12)",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(280, 70),
                Location = new Point(900, 30),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOdemeAl.Click += BtnOdemeAl_Click;

            panelAlt.Controls.AddRange(new Control[] { lblToplam, btnUrunSil, btnSepetTemizle, btnOdemeAl });

            this.Controls.Add(panelSag);
            this.Controls.Add(panelSol);
            this.Controls.Add(panelAlt);
            this.Controls.Add(panelUst);

            this.KeyDown += FormSatis_KeyDown;
            this.Load += (s, e) => txtBarkod.Focus();
        }

        private Button CreateMenuButton(string text, int x)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11),
                Location = new Point(x, 17),
                Size = new Size(120, 38),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private void SepetOlustur()
        {
            sepet = new DataTable();
            sepet.Columns.Add("StokID", typeof(int));
            sepet.Columns.Add("Barkod", typeof(string));
            sepet.Columns.Add("UrunAdi", typeof(string));
            sepet.Columns.Add("Miktar", typeof(decimal));
            sepet.Columns.Add("BirimFiyat", typeof(decimal));
            sepet.Columns.Add("Tutar", typeof(decimal));
            sepet.Columns.Add("KdvOrani", typeof(decimal));

            dgvSepet.AutoGenerateColumns = true;
            dgvSepet.DataSource = sepet;

            if (dgvSepet.Columns.Count == 0) return;

            dgvSepet.Columns["StokID"].Visible = false;
            dgvSepet.Columns["KdvOrani"].Visible = false;
            dgvSepet.Columns["Barkod"].HeaderText = "Barkod";
            dgvSepet.Columns["Barkod"].Width = 130;
            dgvSepet.Columns["UrunAdi"].HeaderText = "Ürün Adı";
            dgvSepet.Columns["Miktar"].HeaderText = "Miktar";
            dgvSepet.Columns["Miktar"].Width = 80;
            dgvSepet.Columns["BirimFiyat"].HeaderText = "Birim Fiyat";
            dgvSepet.Columns["BirimFiyat"].Width = 100;
            dgvSepet.Columns["Tutar"].HeaderText = "Tutar";
            dgvSepet.Columns["Tutar"].Width = 100;
            dgvSepet.Columns["BirimFiyat"].DefaultCellStyle.Format = "₺#,##0.00";
            dgvSepet.Columns["Tutar"].DefaultCellStyle.Format = "₺#,##0.00";
            dgvSepet.Columns["Miktar"].DefaultCellStyle.Format = "#,##0.###";
        }

        private void TxtBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string barkod = txtBarkod.Text.Trim();
                if (!string.IsNullOrEmpty(barkod))
                {
                    UrunEkle(barkod);
                    txtBarkod.Clear();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void UrunEkle(string barkod)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        SELECT TOP 1 s.nStokID, ISNULL(b.sBarkod, s.sKodu) as Barkod, s.sAciklama, 
                               ISNULL(f.lFiyat, 0) as SatisFiyat, 
                               ISNULL(k.nKdvOrani, 18) as KdvOrani
                        FROM tbStok s
                        LEFT JOIN tbStokBarkodu b ON s.nStokID = b.nStokId
                        LEFT JOIN tbStokFiyati f ON s.nStokID = f.nStokID AND f.sFiyatTipi = '1'
                        LEFT JOIN tbKdv k ON s.sKdvTipi = k.sKdvTipi
                        WHERE b.sBarkod = @barkod OR s.sKodu = @barkod
                        ORDER BY f.lFiyat DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@barkod", barkod);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int stokId = reader.GetInt32(0);
                                string urunBarkod = reader.IsDBNull(1) ? barkod : reader.GetString(1).Trim();
                                string urunAdi = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                                decimal fiyat = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                                decimal kdv = reader.IsDBNull(4) ? 18 : reader.GetDecimal(4);
                                decimal miktar = nudMiktar.Value;

                                if (fiyat == 0)
                                {
                                    MessageBox.Show($"'{urunAdi}' ürününün satış fiyatı tanımlı değil!", "Uyarı", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                DataRow[] mevcutUrun = sepet.Select($"StokID = {stokId}");
                                if (mevcutUrun.Length > 0)
                                {
                                    decimal yeniMiktar = (decimal)mevcutUrun[0]["Miktar"] + miktar;
                                    mevcutUrun[0]["Miktar"] = yeniMiktar;
                                    mevcutUrun[0]["Tutar"] = yeniMiktar * fiyat;
                                }
                                else
                                {
                                    sepet.Rows.Add(stokId, urunBarkod, urunAdi, miktar, fiyat, miktar * fiyat, kdv);
                                }

                                ToplamHesapla();
                                nudMiktar.Value = 1;
                            }
                            else
                            {
                                MessageBox.Show($"'{barkod}' barkodlu ürün bulunamadı!", "Uyarı", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            string arama = txtArama.Text.Trim();

            if (arama.Length < 2)
            {
                lstAramaSonuc.Visible = false;
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT TOP 20 s.nStokID, s.sKodu, s.sAciklama, ISNULL(f.lFiyat, 0) as Fiyat
                        FROM tbStok s
                        LEFT JOIN tbStokFiyati f ON s.nStokID = f.nStokID AND f.sFiyatTipi = '1'
                        WHERE s.sAciklama LIKE @arama OR s.sKodu LIKE @arama OR s.sKisaAdi LIKE @arama
                        ORDER BY s.sAciklama";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@arama", $"%{arama}%");
                        lstAramaSonuc.Items.Clear();
                        lstAramaSonuc.Tag = new System.Collections.Generic.Dictionary<int, int>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int idx = 0;
                            while (reader.Read())
                            {
                                int stokId = reader.GetInt32(0);
                                string ad = reader.GetString(2).Trim();
                                decimal fiyat = reader.GetDecimal(3);
                                lstAramaSonuc.Items.Add($"{ad} - ₺{fiyat:N2}");
                                ((System.Collections.Generic.Dictionary<int, int>)lstAramaSonuc.Tag)[idx++] = stokId;
                            }
                        }

                        lstAramaSonuc.Visible = lstAramaSonuc.Items.Count > 0;
                    }
                }
            }
            catch { }
        }

        private void LstAramaSonuc_DoubleClick(object sender, EventArgs e)
        {
            if (lstAramaSonuc.SelectedIndex >= 0)
            {
                var dict = lstAramaSonuc.Tag as System.Collections.Generic.Dictionary<int, int>;
                if (dict != null && dict.ContainsKey(lstAramaSonuc.SelectedIndex))
                {
                    UrunEkleById(dict[lstAramaSonuc.SelectedIndex]);
                    lstAramaSonuc.Visible = false;
                    txtArama.Clear();
                }
            }
        }

        private void UrunEkleById(int stokId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT s.nStokID, ISNULL(b.sBarkod, s.sKodu) as Barkod, s.sAciklama,
                               ISNULL(f.lFiyat, 0) as SatisFiyat, ISNULL(k.nKdvOrani, 18) as KdvOrani
                        FROM tbStok s
                        LEFT JOIN tbStokBarkodu b ON s.nStokID = b.nStokId
                        LEFT JOIN tbStokFiyati f ON s.nStokID = f.nStokID AND f.sFiyatTipi = '1'
                        LEFT JOIN tbKdv k ON s.sKdvTipi = k.sKdvTipi
                        WHERE s.nStokID = @stokId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@stokId", stokId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string barkod = reader.GetString(1).Trim();
                                string urunAdi = reader.GetString(2).Trim();
                                decimal fiyat = reader.GetDecimal(3);
                                decimal kdv = reader.GetDecimal(4);
                                decimal miktar = nudMiktar.Value;

                                if (fiyat == 0)
                                {
                                    MessageBox.Show($"'{urunAdi}' ürününün satış fiyatı tanımlı değil!", "Uyarı", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                DataRow[] mevcutUrun = sepet.Select($"StokID = {stokId}");
                                if (mevcutUrun.Length > 0)
                                {
                                    decimal yeniMiktar = (decimal)mevcutUrun[0]["Miktar"] + miktar;
                                    mevcutUrun[0]["Miktar"] = yeniMiktar;
                                    mevcutUrun[0]["Tutar"] = yeniMiktar * fiyat;
                                }
                                else
                                {
                                    sepet.Rows.Add(stokId, barkod, urunAdi, miktar, fiyat, miktar * fiyat, kdv);
                                }

                                ToplamHesapla();
                                nudMiktar.Value = 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToplamHesapla()
        {
            toplamTutar = 0;
            foreach (DataRow row in sepet.Rows)
            {
                toplamTutar += (decimal)row["Tutar"];
            }
            lblToplam.Text = $"TOPLAM: ₺{toplamTutar:N2}";
            lblUrunSayisi.Text = $"({sepet.Rows.Count} ürün)";
        }

        private void BtnUrunSil_Click(object sender, EventArgs e)
        {
            if (dgvSepet.SelectedRows.Count > 0)
            {
                int index = dgvSepet.SelectedRows[0].Index;
                sepet.Rows.RemoveAt(index);
                ToplamHesapla();
            }
        }

        private void BtnSepetTemizle_Click(object sender, EventArgs e)
        {
            if (sepet.Rows.Count == 0) return;
            
            if (MessageBox.Show("Sepeti temizlemek istediğinize emin misiniz?", "Onay", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                sepet.Clear();
                ToplamHesapla();
            }
        }

        private void BtnOdemeAl_Click(object sender, EventArgs e)
        {
            if (sepet.Rows.Count == 0)
            {
                MessageBox.Show("Sepet boş!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FormOdeme formOdeme = new FormOdeme(sepet, toplamTutar, musteriId);
            if (formOdeme.ShowDialog() == DialogResult.OK)
            {
                sepet.Clear();
                ToplamHesapla();
                musteriId = 0;
                lblMusteriAdi.Text = "Peşin Satış";
                txtBarkod.Focus();
            }
        }

        private void BtnMusteriSec_Click(object sender, EventArgs e)
        {
            FormMusteriSec formMusteri = new FormMusteriSec();
            if (formMusteri.ShowDialog() == DialogResult.OK)
            {
                musteriId = formMusteri.SecilenMusteriId;
                lblMusteriAdi.Text = formMusteri.SecilenMusteriAdi;
            }
        }

        private void BtnUrunler_Click(object sender, EventArgs e)
        {
            FormUrunYonetimi form = new FormUrunYonetimi();
            form.ShowDialog();
        }

        private void BtnRaporlar_Click(object sender, EventArgs e)
        {
            FormRaporlar form = new FormRaporlar();
            form.ShowDialog();
        }

        private void BtnAyarlar_Click(object sender, EventArgs e)
        {
            FormAyarlar form = new FormAyarlar();
            form.ShowDialog();
        }

        private void FormSatis_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F12:
                    BtnOdemeAl_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    BtnUrunSil_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    txtBarkod.Focus();
                    txtBarkod.SelectAll();
                    e.Handled = true;
                    break;
                case Keys.F5:
                    BtnMusteriSec_Click(null, null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
