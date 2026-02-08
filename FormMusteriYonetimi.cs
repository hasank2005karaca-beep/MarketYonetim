using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormMusteriYonetimi : Form
    {
        private DataGridView dgvMusteriler;
        private TextBox txtArama;
        private Label lblSayfa;
        private Button btnOnceki;
        private Button btnSonraki;
        private Timer aramaTimer;
        private int sayfa;
        private const int SayfaBoyutu = 50;
        private int seciliMusteriId;
        private bool cariDestekli;

        private TextBox txtAdi;
        private TextBox txtSoyadi;
        private TextBox txtTel;
        private TextBox txtIsTel;
        private TextBox txtEmail;
        private TextBox txtAdres;
        private TextBox txtIl;
        private TextBox txtSemt;
        private TextBox txtVergiNo;
        private TextBox txtVergiDairesi;

        public FormMusteriYonetimi()
        {
            cariDestekli = VeriKatmani.CariIslemDestekleniyor();
            InitializeComponent();
            YukleListe();
        }

        private void InitializeComponent()
        {
            Text = "👤 Müşteri Yönetimi";
            Size = new Size(1200, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            KeyPreview = true;
            KeyDown += FormMusteriYonetimi_KeyDown;

            Panel panelUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblBaslik = new Label
            {
                Text = "👤 Müşteri Yönetimi",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            txtArama = new TextBox
            {
                Location = new Point(320, 15),
                Width = 320,
                PlaceholderText = "Ad/Soyad/VergiNo/Telefon"
            };
            txtArama.TextChanged += TxtArama_TextChanged;

            panelUst.Controls.Add(lblBaslik);
            panelUst.Controls.Add(txtArama);

            Panel panelSol = new Panel
            {
                Dock = DockStyle.Left,
                Width = 620,
                Padding = new Padding(10)
            };

            dgvMusteriler = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvMusteriler.SelectionChanged += DgvMusteriler_SelectionChanged;
            dgvMusteriler.CellDoubleClick += DgvMusteriler_CellDoubleClick;

            panelSol.Controls.Add(dgvMusteriler);

            Panel panelSag = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            GroupBox grpDetay = new GroupBox
            {
                Text = "Müşteri Detayları",
                Dock = DockStyle.Top,
                Height = 360
            };

            int labelX = 20;
            int inputX = 160;
            int y = 30;
            int gap = 30;

            grpDetay.Controls.Add(CreateLabel("Ad", labelX, y));
            txtAdi = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtAdi);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("Soyad", labelX, y));
            txtSoyadi = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtSoyadi);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("Telefon", labelX, y));
            txtTel = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtTel);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("İş Telefonu", labelX, y));
            txtIsTel = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtIsTel);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("E-posta", labelX, y));
            txtEmail = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtEmail);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("Adres", labelX, y));
            txtAdres = CreateTextBox(inputX, y - 3);
            txtAdres.Width = 320;
            grpDetay.Controls.Add(txtAdres);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("İl", labelX, y));
            txtIl = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtIl);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("Semt", labelX, y));
            txtSemt = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtSemt);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("Vergi No", labelX, y));
            txtVergiNo = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtVergiNo);

            y += gap;
            grpDetay.Controls.Add(CreateLabel("Vergi Dairesi", labelX, y));
            txtVergiDairesi = CreateTextBox(inputX, y - 3);
            grpDetay.Controls.Add(txtVergiDairesi);

            Panel panelButonlar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90
            };

            Button btnEkle = CreateActionButton("➕ Ekle", 10, 15, BtnEkle_Click);
            Button btnDuzenle = CreateActionButton("✏️ Düzenle", 120, 15, BtnDuzenle_Click);
            Button btnSil = CreateActionButton("🗑️ Sil", 250, 15, BtnSil_Click);
            Button btnTahsilat = CreateActionButton("💳 Veresiye Tahsilat Al", 360, 15, BtnTahsilat_Click);
            Button btnEkstre = CreateActionButton("📄 Ekstre", 10, 50, BtnEkstre_Click);
            Button btnYenile = CreateActionButton("🔄 Yenile", 120, 50, BtnYenile_Click);

            if (!cariDestekli)
            {
                btnTahsilat.Enabled = false;
                btnTahsilat.Text = "💳 Cari Modül Yok";
            }

            if (!cariDestekli)
            {
                Label lblCariUyari = new Label
                {
                    Text = "Bu kurulumda cari modül yok. Tahsilat kapatıldı.",
                    AutoSize = true,
                    ForeColor = Color.DarkRed,
                    Location = new Point(250, 55)
                };
                panelButonlar.Controls.Add(lblCariUyari);
            }

            panelButonlar.Controls.AddRange(new Control[] { btnEkle, btnDuzenle, btnSil, btnTahsilat, btnEkstre, btnYenile });

            panelSag.Controls.Add(panelButonlar);
            panelSag.Controls.Add(grpDetay);

            Panel panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            btnOnceki = new Button
            {
                Text = "◀ Önceki",
                Location = new Point(10, 10),
                Width = 110
            };
            btnOnceki.Click += BtnOnceki_Click;

            btnSonraki = new Button
            {
                Text = "Sonraki ▶",
                Location = new Point(130, 10),
                Width = 110
            };
            btnSonraki.Click += BtnSonraki_Click;

            lblSayfa = new Label
            {
                AutoSize = true,
                Location = new Point(260, 14)
            };

            panelAlt.Controls.Add(btnOnceki);
            panelAlt.Controls.Add(btnSonraki);
            panelAlt.Controls.Add(lblSayfa);

            aramaTimer = new Timer { Interval = 300 };
            aramaTimer.Tick += AramaTimer_Tick;

            Controls.Add(panelSag);
            Controls.Add(panelSol);
            Controls.Add(panelAlt);
            Controls.Add(panelUst);
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private static TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = 220
            };
        }

        private static Button CreateActionButton(string text, int x, int y, EventHandler click)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 220,
                Height = 28,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            btn.Click += click;
            return btn;
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            sayfa = 0;
            YukleListe();
        }

        private void YukleListe()
        {
            try
            {
                string arama = txtArama.Text.Trim();
                string filtre = string.IsNullOrWhiteSpace(arama) || arama.Length < 2 ? null : arama;
                DataTable dt = VeriKatmani.MusterileriGetir(filtre, sayfa, SayfaBoyutu);
                if (!dt.Columns.Contains("Veresiye Bakiye"))
                {
                    dt.Columns.Add("Veresiye Bakiye", typeof(decimal));
                }

                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["nMusteriID"]);
                    decimal bakiye = 0m;
                    try
                    {
                        bakiye = VeriKatmani.MusteriVeresiyeBakiyeGetir(id);
                    }
                    catch
                    {
                        bakiye = 0m;
                    }
                    row["Veresiye Bakiye"] = bakiye;
                }

                if (dt.Columns.Contains("nMusteriID")) dt.Columns["nMusteriID"].ColumnName = "ID";
                if (dt.Columns.Contains("sAdi")) dt.Columns["sAdi"].ColumnName = "Ad";
                if (dt.Columns.Contains("sSoyadi")) dt.Columns["sSoyadi"].ColumnName = "Soyad";
                if (dt.Columns.Contains("sTelefon1")) dt.Columns["sTelefon1"].ColumnName = "Telefon";
                if (dt.Columns.Contains("sEmail")) dt.Columns["sEmail"].ColumnName = "E-posta";
                if (dt.Columns.Contains("sVergiNo")) dt.Columns["sVergiNo"].ColumnName = "Vergi No";

                dgvMusteriler.DataSource = dt;
                if (dgvMusteriler.Columns.Contains("ID"))
                {
                    dgvMusteriler.Columns["ID"].Visible = false;
                }

                int toplam = VeriKatmani.MusteriSayisiGetir(filtre);
                int toplamSayfa = Math.Max(1, (int)Math.Ceiling(toplam / (double)SayfaBoyutu));
                lblSayfa.Text = $"Sayfa {sayfa + 1}/{toplamSayfa} (Toplam {toplam})";
                btnOnceki.Enabled = sayfa > 0;
                btnSonraki.Enabled = sayfa + 1 < toplamSayfa;

                if (dgvMusteriler.Rows.Count > 0)
                {
                    dgvMusteriler.Rows[0].Selected = true;
                }
                else
                {
                    TemizleForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvMusteriler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMusteriler.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow row = dgvMusteriler.SelectedRows[0];
            if (row.Cells["ID"].Value == null)
            {
                return;
            }

            seciliMusteriId = Convert.ToInt32(row.Cells["ID"].Value);
            MusteriDetayYukle();
        }

        private void DgvMusteriler_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnDuzenle_Click(null, null);
            }
        }

        private void MusteriDetayYukle()
        {
            try
            {
                DataRow detay = VeriKatmani.MusteriDetayGetir(seciliMusteriId);
                if (detay == null)
                {
                    TemizleForm();
                    return;
                }

                txtAdi.Text = detay["sAdi"]?.ToString() ?? string.Empty;
                txtSoyadi.Text = detay["sSoyadi"]?.ToString() ?? string.Empty;
                txtTel.Text = detay["sTelefon1"]?.ToString() ?? string.Empty;
                txtIsTel.Text = detay.Table.Columns.Contains("sTelefon2") ? detay["sTelefon2"]?.ToString() ?? string.Empty : string.Empty;
                txtEmail.Text = detay.Table.Columns.Contains("sEmail") ? detay["sEmail"]?.ToString() ?? string.Empty : string.Empty;
                txtAdres.Text = detay.Table.Columns.Contains("sAdres") ? detay["sAdres"]?.ToString() ?? string.Empty : string.Empty;
                txtIl.Text = detay.Table.Columns.Contains("sIl") ? detay["sIl"]?.ToString() ?? string.Empty : string.Empty;
                txtSemt.Text = detay.Table.Columns.Contains("sSemt") ? detay["sSemt"]?.ToString() ?? string.Empty : string.Empty;
                txtVergiNo.Text = detay.Table.Columns.Contains("sVergiNo") ? detay["sVergiNo"]?.ToString() ?? string.Empty : string.Empty;
                txtVergiDairesi.Text = detay.Table.Columns.Contains("sVergiDairesi") ? detay["sVergiDairesi"]?.ToString() ?? string.Empty : string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TemizleForm()
        {
            txtAdi.Text = string.Empty;
            txtSoyadi.Text = string.Empty;
            txtTel.Text = string.Empty;
            txtIsTel.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtAdres.Text = string.Empty;
            txtIl.Text = string.Empty;
            txtSemt.Text = string.Empty;
            txtVergiNo.Text = string.Empty;
            txtVergiDairesi.Text = string.Empty;
            seciliMusteriId = 0;
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAdi.Text))
            {
                MessageBox.Show("Ad alanı zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                VeriKatmani.MusteriEkle(
                    txtAdi.Text.Trim(),
                    txtSoyadi.Text.Trim(),
                    txtTel.Text.Trim(),
                    txtIsTel.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtAdres.Text.Trim(),
                    txtIl.Text.Trim(),
                    txtSemt.Text.Trim(),
                    txtVergiNo.Text.Trim(),
                    txtVergiDairesi.Text.Trim()
                );

                MessageBox.Show("Müşteri eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleListe();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDuzenle_Click(object sender, EventArgs e)
        {
            if (seciliMusteriId <= 0)
            {
                MessageBox.Show("Düzenlenecek müşteri seçilmedi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                VeriKatmani.MusteriGuncelle(
                    seciliMusteriId,
                    txtAdi.Text.Trim(),
                    txtSoyadi.Text.Trim(),
                    txtTel.Text.Trim(),
                    txtIsTel.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtAdres.Text.Trim(),
                    txtIl.Text.Trim(),
                    txtSemt.Text.Trim(),
                    txtVergiNo.Text.Trim(),
                    txtVergiDairesi.Text.Trim()
                );

                MessageBox.Show("Müşteri güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleListe();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (seciliMusteriId <= 0)
            {
                MessageBox.Show("Silinecek müşteri seçilmedi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Seçili müşteri silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                VeriKatmani.MusteriSil(seciliMusteriId);
                MessageBox.Show("Müşteri silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleListe();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTahsilat_Click(object sender, EventArgs e)
        {
            if (seciliMusteriId <= 0)
            {
                MessageBox.Show("Tahsilat için müşteri seçilmedi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string musteriAdi = string.Format("{0} {1}", txtAdi.Text.Trim(), txtSoyadi.Text.Trim()).Trim();
            using (FormVeresiyeOdemeAl form = new FormVeresiyeOdemeAl(seciliMusteriId, musteriAdi))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    YukleListe();
                }
            }
        }

        private void BtnEkstre_Click(object sender, EventArgs e)
        {
            if (seciliMusteriId <= 0)
            {
                MessageBox.Show("Ekstre için müşteri seçilmedi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Form dialog = new Form
            {
                Text = "📄 Ekstre Tarih Aralığı",
                Size = new Size(360, 220),
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Segoe UI", 10)
            };

            DateTimePicker dtBas = new DateTimePicker
            {
                Location = new Point(140, 20),
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };
            DateTimePicker dtBit = new DateTimePicker
            {
                Location = new Point(140, 60),
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            dialog.Controls.Add(new Label { Text = "Başlangıç", Location = new Point(20, 25), AutoSize = true });
            dialog.Controls.Add(new Label { Text = "Bitiş", Location = new Point(20, 65), AutoSize = true });
            dialog.Controls.Add(dtBas);
            dialog.Controls.Add(dtBit);

            Button btnTamam = new Button
            {
                Text = "Göster",
                Location = new Point(140, 110),
                Width = 80
            };
            Button btnIptal = new Button
            {
                Text = "İptal",
                Location = new Point(230, 110),
                Width = 80
            };
            btnTamam.Click += (s, args) =>
            {
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            };
            btnIptal.Click += (s, args) => dialog.Close();
            dialog.Controls.Add(btnTamam);
            dialog.Controls.Add(btnIptal);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                EkstreGoster(dtBas.Value.Date, dtBit.Value.Date);
            }
        }

        private void EkstreGoster(DateTime bas, DateTime bit)
        {
            try
            {
                DataTable ekstre = VeriKatmani.MusteriEkstreGetir(seciliMusteriId, bas, bit);
                Form frm = new Form
                {
                    Text = "📄 Müşteri Ekstresi",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent
                };

                DataGridView dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    DataSource = ekstre
                };

                frm.Controls.Add(dgv);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnYenile_Click(object sender, EventArgs e)
        {
            YukleListe();
        }

        private void BtnOnceki_Click(object sender, EventArgs e)
        {
            if (sayfa > 0)
            {
                sayfa--;
                YukleListe();
            }
        }

        private void BtnSonraki_Click(object sender, EventArgs e)
        {
            sayfa++;
            YukleListe();
        }

        private void FormMusteriYonetimi_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    txtArama.Focus();
                    e.Handled = true;
                    break;
                case Keys.Insert:
                    BtnEkle_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    BtnDuzenle_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    BtnSil_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.F5:
                    BtnYenile_Click(null, null);
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    Close();
                    e.Handled = true;
                    break;
            }
        }
    }
}
