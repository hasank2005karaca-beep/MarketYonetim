using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormRaporlar : Form
    {
        private Panel panelFiltre;
        private Panel panelAlt;
        private DataGridView dgvRapor;
        private DateTimePicker dtpBaslangic;
        private DateTimePicker dtpBitis;
        private ComboBox cmbRaporTipi;
        private ComboBox cmbOdeme;
        private TextBox txtKasiyer;
        private NumericUpDown nudTopN;
        private ComboBox cmbSiralama;
        private TextBox txtUrunArama;
        private Button btnUrunSec;
        private Label lblSeciliUrun;
        private Label lblOzet;
        private Button btnRaporGetir;
        private Button btnCsv;
        private Button btnYenile;
        private int? seciliStokId;

        public FormRaporlar()
        {
            InitializeComponent();
            dtpBaslangic.Value = DateTime.Today;
            dtpBitis.Value = DateTime.Now;
            cmbRaporTipi.SelectedIndex = 0;
            GuncelleFiltreGorunurlugu();
            _ = RaporuGetirAsync();
        }

        private void InitializeComponent()
        {
            // S7-FIX: DPI ölçekleme
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "📊 Raporlar";
            Size = new Size(1200, 720);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            KeyPreview = true;

            panelFiltre = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.FromArgb(245, 246, 250)
            };

            Label lblBaslangic = new Label
            {
                Text = "Başlangıç",
                Location = new Point(16, 16),
                AutoSize = true
            };
            dtpBaslangic = new DateTimePicker
            {
                Location = new Point(16, 40),
                Width = 140,
                Format = DateTimePickerFormat.Short
            };

            Label lblBitis = new Label
            {
                Text = "Bitiş",
                Location = new Point(170, 16),
                AutoSize = true
            };
            dtpBitis = new DateTimePicker
            {
                Location = new Point(170, 40),
                Width = 140,
                Format = DateTimePickerFormat.Short
            };

            Label lblRaporTipi = new Label
            {
                Text = "Rapor Tipi",
                Location = new Point(324, 16),
                AutoSize = true
            };
            cmbRaporTipi = new ComboBox
            {
                Location = new Point(324, 40),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRaporTipi.Items.AddRange(new object[]
            {
                "Satış Özeti",
                "KDV Raporu",
                "En Çok Satanlar",
                "Ürün Satış Performansı",
                "Veresiye Raporu",
                "Kasa Raporu"
            });
            cmbRaporTipi.SelectedIndexChanged += (s, e) => GuncelleFiltreGorunurlugu();

            Label lblOdeme = new Label
            {
                Text = "Ödeme",
                Location = new Point(560, 16),
                AutoSize = true
            };
            cmbOdeme = new ComboBox
            {
                Location = new Point(560, 40),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOdeme.Items.AddRange(new object[] { "Hepsi", "Nakit", "Kredi Kartı" });
            cmbOdeme.SelectedIndex = 0;

            Label lblKasiyer = new Label
            {
                Text = "Kasiyer",
                Location = new Point(716, 16),
                AutoSize = true
            };
            txtKasiyer = new TextBox
            {
                Location = new Point(716, 40),
                Width = 120
            };

            Label lblTopN = new Label
            {
                Text = "Top N",
                Location = new Point(852, 16),
                AutoSize = true
            };
            nudTopN = new NumericUpDown
            {
                Location = new Point(852, 40),
                Width = 70,
                Minimum = 1,
                Maximum = 500,
                Value = 20
            };

            Label lblSiralama = new Label
            {
                Text = "Sıralama",
                Location = new Point(936, 16),
                AutoSize = true
            };
            cmbSiralama = new ComboBox
            {
                Location = new Point(936, 40),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSiralama.Items.AddRange(new object[] { "Adet", "Tutar" });
            cmbSiralama.SelectedIndex = 0;

            Label lblUrun = new Label
            {
                Text = "Ürün",
                Location = new Point(16, 78),
                AutoSize = true
            };
            txtUrunArama = new TextBox
            {
                Location = new Point(16, 102),
                Width = 240
            };
            btnUrunSec = new Button
            {
                Text = "Seç",
                Location = new Point(268, 100),
                Width = 60,
                Height = 28
            };
            btnUrunSec.Click += BtnUrunSec_Click;

            lblSeciliUrun = new Label
            {
                Text = "Seçili ürün: -",
                Location = new Point(340, 104),
                AutoSize = true
            };

            panelFiltre.Controls.AddRange(new Control[]
            {
                lblBaslangic, dtpBaslangic,
                lblBitis, dtpBitis,
                lblRaporTipi, cmbRaporTipi,
                lblOdeme, cmbOdeme,
                lblKasiyer, txtKasiyer,
                lblTopN, nudTopN,
                lblSiralama, cmbSiralama,
                lblUrun, txtUrunArama, btnUrunSec, lblSeciliUrun
            });

            dgvRapor = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White
            };
            dgvRapor.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvRapor.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRapor.ColumnHeadersHeight = 36;
            dgvRapor.EnableHeadersVisualStyles = false;
            dgvRapor.RowTemplate.Height = 28;
            Yardimcilar.DoubleBufferedAktifEt(dgvRapor);

            panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            btnRaporGetir = new Button
            {
                Text = "Raporu Getir",
                Width = 130,
                Height = 36,
                Location = new Point(16, 12),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRaporGetir.Click += async (s, e) =>
            {
                try
                {
                    await RaporuGetirAsync();
                }
                catch (Exception ex)
                {
                    // S7-FIX: DB hatalarını kullanıcıya göster
                    MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCsv = new Button
            {
                Text = "CSV Dışa Aktar",
                Width = 140,
                Height = 36,
                Location = new Point(156, 12),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCsv.Click += BtnCsv_Click;

            btnYenile = new Button
            {
                Text = "Yenile (F5)",
                Width = 110,
                Height = 36,
                Location = new Point(306, 12),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnYenile.Click += async (s, e) =>
            {
                try
                {
                    await RaporuGetirAsync();
                }
                catch (Exception ex)
                {
                    // S7-FIX: DB hatalarını kullanıcıya göster
                    MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnKapat = new Button
            {
                Text = "Kapat",
                Width = 90,
                Height = 36,
                Location = new Point(1060, 12),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKapat.Click += (s, e) => Close();

            lblOzet = new Label
            {
                Text = "Kayıt: 0",
                AutoSize = true,
                Location = new Point(430, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            panelAlt.Controls.AddRange(new Control[]
            {
                btnRaporGetir, btnCsv, btnYenile, lblOzet, btnKapat
            });

            Controls.Add(dgvRapor);
            Controls.Add(panelAlt);
            Controls.Add(panelFiltre);
        }

        private void BtnUrunSec_Click(object sender, EventArgs e)
        {
            try
            {
                using (UrunSecimForm form = new UrunSecimForm(txtUrunArama.Text))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        seciliStokId = form.SeciliStokId;
                        lblSeciliUrun.Text = $"Seçili ürün: {form.SeciliUrun}";
                    }
                }
            }
            catch (Exception ex)
            {
                // S7-FIX: DB hatalarını kullanıcıya göster
                MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuncelleFiltreGorunurlugu()
        {
            string raporTipi = cmbRaporTipi.SelectedItem?.ToString() ?? string.Empty;
            bool satisOzeti = raporTipi == "Satış Özeti";
            bool enCokSatan = raporTipi == "En Çok Satanlar";
            bool urunPerformans = raporTipi == "Ürün Satış Performansı";

            cmbOdeme.Enabled = satisOzeti;
            txtKasiyer.Enabled = satisOzeti;
            nudTopN.Enabled = enCokSatan;
            cmbSiralama.Enabled = enCokSatan;
            txtUrunArama.Enabled = urunPerformans;
            btnUrunSec.Enabled = urunPerformans;
            lblSeciliUrun.Enabled = urunPerformans;
        }

        private async Task RaporuGetirAsync()
        {
            if (cmbRaporTipi.SelectedItem == null)
            {
                return;
            }

            SetLoading(true);
            try
            {
                DataTable dt = await Task.Run(() => RaporVerisiGetir());
                dgvRapor.DataSource = dt;
                GuncelleOzet(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Rapor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private DataTable RaporVerisiGetir()
        {
            DateTime baslangic = dtpBaslangic.Value.Date;
            DateTime bitis = dtpBitis.Value;
            string raporTipi = cmbRaporTipi.SelectedItem?.ToString() ?? string.Empty;

            switch (raporTipi)
            {
                case "Satış Özeti":
                    return VeriKatmani.SatisRaporuGetir(baslangic, bitis, OdemeSekliGetir(), txtKasiyer.Text.Trim());
                case "KDV Raporu":
                    return VeriKatmani.KdvRaporuGetir(baslangic, bitis);
                case "En Çok Satanlar":
                    return VeriKatmani.EnCokSatanlarGetir(baslangic, bitis, (int)nudTopN.Value, cmbSiralama.SelectedIndex == 1 ? "tutar" : "adet");
                case "Ürün Satış Performansı":
                    if (!seciliStokId.HasValue)
                    {
                        throw new InvalidOperationException("Lütfen ürün seçin.");
                    }
                    return VeriKatmani.UrunSatisPerformansGetir(seciliStokId.Value, baslangic, bitis);
                case "Veresiye Raporu":
                    return VeriKatmani.VeresiyeRaporuGetir(baslangic, bitis);
                case "Kasa Raporu":
                    return VeriKatmani.KasaRaporuGetir(baslangic, bitis);
                default:
                    return new DataTable();
            }
        }

        private string OdemeSekliGetir()
        {
            string odeme = cmbOdeme.SelectedItem?.ToString() ?? "Hepsi";
            if (odeme == "Nakit")
            {
                return "N";
            }
            if (odeme == "Kredi Kartı")
            {
                return "KK";
            }
            return "hepsi";
        }

        private void BtnCsv_Click(object sender, EventArgs e)
        {
            if (!(dgvRapor.DataSource is DataTable dt) || dt.Rows.Count == 0)
            {
                MessageBox.Show("Önce rapor alın.", "CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV Dosyası|*.csv";
                dialog.FileName = $"rapor_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Yardimcilar.DataTableToCsv(dt, dialog.FileName);
                    MessageBox.Show("CSV oluşturuldu.", "CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void GuncelleOzet(DataTable dt)
        {
            int kayit = dt?.Rows.Count ?? 0;
            string toplamMetin = "";
            if (dt != null)
            {
                decimal toplam = 0m;
                string[] adaylar = { "ToplamNet", "ToplamTutar", "ToplamBrut", "Toplam", "NetBakiye" };
                foreach (string kolon in adaylar)
                {
                    if (dt.Columns.Contains(kolon))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row[kolon] != DBNull.Value)
                            {
                                toplam += Convert.ToDecimal(row[kolon]);
                            }
                        }
                        toplamMetin = $" | Toplam: {Yardimcilar.ParaFormatla(toplam)}";
                        break;
                    }
                }
            }
            lblOzet.Text = $"Kayıt: {kayit}{toplamMetin}";
        }

        private void SetLoading(bool loading)
        {
            UseWaitCursor = loading;
            panelFiltre.Enabled = !loading;
            panelAlt.Enabled = !loading;
            dgvRapor.Enabled = !loading;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                _ = RaporuGetirAsync();
                return true;
            }
            if (keyData == (Keys.Control | Keys.E))
            {
                BtnCsv_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private class UrunSecimForm : Form
        {
            private readonly TextBox txtArama;
            private readonly DataGridView dgvUrun;
            private readonly Button btnSec;
            public int? SeciliStokId { get; private set; }
            public string SeciliUrun { get; private set; }

            public UrunSecimForm(string arama)
            {
                Text = "Ürün Seç";
                Size = new Size(720, 480);
                StartPosition = FormStartPosition.CenterParent;

                Label lblArama = new Label { Text = "Arama", Location = new Point(16, 16), AutoSize = true };
                txtArama = new TextBox { Location = new Point(16, 40), Width = 320, Text = arama ?? string.Empty };
                Button btnAra = new Button { Text = "Ara", Location = new Point(348, 38), Width = 80 };
                btnAra.Click += (s, e) =>
                {
                    try
                    {
                        ListeyiDoldur();
                    }
                    catch (Exception ex)
                    {
                        // S7-FIX: DB hatalarını kullanıcıya göster
                        MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dgvUrun = new DataGridView
                {
                    Location = new Point(16, 80),
                    Size = new Size(670, 300),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };
                dgvUrun.DoubleClick += (s, e) => Sec();
                Yardimcilar.DoubleBufferedAktifEt(dgvUrun);

                btnSec = new Button
                {
                    Text = "Seç",
                    Location = new Point(596, 392),
                    Width = 90,
                    Height = 32
                };
                btnSec.Click += (s, e) => Sec();

                Controls.AddRange(new Control[] { lblArama, txtArama, btnAra, dgvUrun, btnSec });

                ListeyiDoldur();
            }

            private void ListeyiDoldur()
            {
                try
                {
                    string arama = txtArama.Text.Trim();
                    DataTable dt = VeriKatmani.UrunAra(arama);
                    dgvUrun.DataSource = dt;
                }
                catch (Exception ex)
                {
                    // S7-FIX: DB hatalarını kullanıcıya göster
                    MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void Sec()
            {
                if (dgvUrun.CurrentRow == null)
                {
                    return;
                }

                if (dgvUrun.CurrentRow.DataBoundItem is DataRowView row)
                {
                    SeciliStokId = Convert.ToInt32(row["nStokID"]);
                    SeciliUrun = $"{row["sKodu"]} - {row["sAciklama"]}";
                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
