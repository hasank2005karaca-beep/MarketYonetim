using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormUrunDuzenle : Form
    {
        private readonly int? stokId;
        private TextBox txtKodu;
        private TextBox txtAciklama;
        private TextBox txtKisaAdi;
        private TextBox txtBirim;
        private ComboBox cmbSinif1;
        private ComboBox cmbSinif2;
        private TextBox txtBarkod;
        private ListBox lstBarkodlar;
        private NumericUpDown nudFiyat;

        public FormUrunDuzenle(int? stokId = null)
        {
            this.stokId = stokId;
            InitializeComponent();
            KategorileriYukle();
            if (stokId.HasValue)
            {
                UrunYukle(stokId.Value);
            }
        }

        private void InitializeComponent()
        {
            // S7-FIX: DPI ölçekleme
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = stokId.HasValue ? "✏️ Ürün Düzenle" : "➕ Ürün Ekle";
            Size = new Size(520, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label lblKodu = new Label { Text = "Kodu*", AutoSize = true };
            txtKodu = new TextBox { Width = 250 };

            Label lblAciklama = new Label { Text = "Ürün Adı*", AutoSize = true };
            txtAciklama = new TextBox { Width = 350 };

            Label lblKisa = new Label { Text = "Kısa Ad", AutoSize = true };
            txtKisaAdi = new TextBox { Width = 250 };

            Label lblBirim = new Label { Text = "Birim*", AutoSize = true };
            txtBirim = new TextBox { Width = 120, Text = "AD" };

            Label lblSinif1 = new Label { Text = "Kategori1", AutoSize = true };
            cmbSinif1 = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSinif1.SelectedIndexChanged += CmbSinif1_SelectedIndexChanged;

            Label lblSinif2 = new Label { Text = "Kategori2", AutoSize = true };
            cmbSinif2 = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblBarkod = new Label { Text = "Barkod", AutoSize = true };
            txtBarkod = new TextBox { Width = 160 };
            Button btnBarkodEkle = new Button { Text = "Ekle", Width = 60 };
            btnBarkodEkle.Click += BtnBarkodEkle_Click;
            Button btnBarkodSil = new Button { Text = "Sil", Width = 60 };
            btnBarkodSil.Click += BtnBarkodSil_Click;
            lstBarkodlar = new ListBox { Height = 100, Width = 250 };

            Label lblFiyat = new Label { Text = $"Fiyat ({Ayarlar.VarsayilanFiyatTipi})*", AutoSize = true };
            nudFiyat = new NumericUpDown { Width = 140, DecimalPlaces = 2, Maximum = 1000000, Increment = 1 };

            Button btnKaydet = new Button { Text = "Kaydet", Width = 120, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnKaydet.Click += BtnKaydet_Click;
            Button btnIptal = new Button { Text = "İptal", Width = 120 };
            btnIptal.Click += (s, e) => DialogResult = DialogResult.Cancel;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(15),
                AutoScroll = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.Controls.Add(lblKodu, 0, 0);
            layout.Controls.Add(txtKodu, 1, 0);
            layout.Controls.Add(lblAciklama, 0, 1);
            layout.Controls.Add(txtAciklama, 1, 1);
            layout.Controls.Add(lblKisa, 0, 2);
            layout.Controls.Add(txtKisaAdi, 1, 2);
            layout.Controls.Add(lblBirim, 0, 3);
            layout.Controls.Add(txtBirim, 1, 3);
            layout.Controls.Add(lblSinif1, 0, 4);
            layout.Controls.Add(cmbSinif1, 1, 4);
            layout.Controls.Add(lblSinif2, 0, 5);
            layout.Controls.Add(cmbSinif2, 1, 5);
            layout.Controls.Add(lblBarkod, 0, 6);

            FlowLayoutPanel barkodPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            barkodPanel.Controls.AddRange(new Control[] { txtBarkod, btnBarkodEkle, btnBarkodSil });
            layout.Controls.Add(barkodPanel, 1, 6);
            layout.Controls.Add(lstBarkodlar, 1, 7);
            layout.Controls.Add(lblFiyat, 0, 8);
            layout.Controls.Add(nudFiyat, 1, 8);

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 50 };
            buttonPanel.Controls.AddRange(new Control[] { btnKaydet, btnIptal });

            Controls.Add(layout);
            Controls.Add(buttonPanel);
        }

        private void KategorileriYukle()
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

        private void UrunYukle(int id)
        {
            try
            {
                DataSet ds = VeriKatmani.UrunDetayGetir(id);
                if (ds.Tables["Urun"].Rows.Count == 0)
                {
                    return;
                }

                DataRow urun = ds.Tables["Urun"].Rows[0];
                txtKodu.Text = urun["sKodu"].ToString();
                txtAciklama.Text = urun["sAciklama"].ToString();
                txtKisaAdi.Text = urun["sKisaAdi"].ToString();
                txtBirim.Text = urun["sBirimCinsi1"].ToString();

                string sinif1 = urun["sSinifKodu1"].ToString();
                string sinif2 = urun["sSinifKodu2"].ToString();

                if (!string.IsNullOrWhiteSpace(sinif1))
                {
                    cmbSinif1.SelectedItem = sinif1;
                }

                if (!string.IsNullOrWhiteSpace(sinif2))
                {
                    cmbSinif2.SelectedItem = sinif2;
                }

                lstBarkodlar.Items.Clear();
                foreach (DataRow row in ds.Tables["Barkodlar"].Rows)
                {
                    lstBarkodlar.Items.Add(row["sBarkod"].ToString());
                }

                DataRow fiyatRow = ds.Tables["Fiyatlar"].AsEnumerable()
                    .FirstOrDefault(r => r["sFiyatTipi"].ToString() == Ayarlar.VarsayilanFiyatTipi);

                if (fiyatRow != null)
                {
                    nudFiyat.Value = Convert.ToDecimal(fiyatRow["lFiyat"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün yüklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBarkodEkle_Click(object sender, EventArgs e)
        {
            try
            {
                string barkod = txtBarkod.Text.Trim();
                if (string.IsNullOrWhiteSpace(barkod))
                {
                    MessageBox.Show("Barkod boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (lstBarkodlar.Items.Contains(barkod))
                {
                    MessageBox.Show("Bu barkod zaten listede.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (VeriKatmani.BarkodVarMi(barkod, stokId))
                {
                    MessageBox.Show("Bu barkod başka bir üründe kullanılıyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                lstBarkodlar.Items.Add(barkod);
                txtBarkod.Clear();
            }
            catch (Exception ex)
            {
                // S7-FIX: DB hatalarını kullanıcıya göster
                MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBarkodSil_Click(object sender, EventArgs e)
        {
            if (lstBarkodlar.SelectedItem == null)
            {
                return;
            }

            lstBarkodlar.Items.Remove(lstBarkodlar.SelectedItem);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            try
            {
                string kodu = txtKodu.Text.Trim();
                string aciklama = txtAciklama.Text.Trim();
                string kisaAdi = txtKisaAdi.Text.Trim();
                string birim = txtBirim.Text.Trim();
                string sinif1 = cmbSinif1.SelectedItem?.ToString();
                string sinif2 = cmbSinif2.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(kodu) || string.IsNullOrWhiteSpace(aciklama) || string.IsNullOrWhiteSpace(birim))
                {
                    MessageBox.Show("Kodu, ürün adı ve birim zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (nudFiyat.Value <= 0)
                {
                    MessageBox.Show("En az bir fiyat girilmelidir.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (VeriKatmani.StokKoduVarMi(kodu, stokId))
                {
                    MessageBox.Show("Bu ürün kodu başka bir üründe kullanılıyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<string> barkodlar = lstBarkodlar.Items.Cast<string>().ToList();
                Dictionary<string, decimal> fiyatlar = new Dictionary<string, decimal>
                {
                    { Ayarlar.VarsayilanFiyatTipi, nudFiyat.Value }
                };

                if (stokId.HasValue)
                {
                    VeriKatmani.UrunGuncelle(stokId.Value, kodu, aciklama, kisaAdi, birim, sinif1, sinif2, barkodlar, fiyatlar);
                }
                else
                {
                    VeriKatmani.UrunEkle(kodu, aciklama, kisaAdi, birim, sinif1, sinif2, barkodlar, fiyatlar);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                // S7-FIX: DB hatalarını kullanıcıya göster
                MessageBox.Show($"Kayıt sırasında hata oluştu. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
