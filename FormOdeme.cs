using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormOdeme : Form
    {
        private DataTable sepet;
        private decimal toplamTutar;
        private int musteriId;
        private decimal nakit = 0;
        private decimal krediKarti = 0;

        private TextBox txtNakit;
        private TextBox txtKrediKarti;
        private Label lblToplam;
        private Label lblKalan;
        private Label lblParaUstu;
        private Button btnOdemeYap;
        private ComboBox cmbOdemeSekli;

        public FormOdeme(DataTable sepetData, decimal toplam, int musteri)
        {
            sepet = sepetData;
            toplamTutar = toplam;
            musteriId = musteri;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "💰 Ödeme Al";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 11);

            // Başlık Panel
            Panel panelBaslik = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            lblToplam = new Label
            {
                Text = $"TOPLAM: ₺{toplamTutar:N2}",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 22)
            };
            panelBaslik.Controls.Add(lblToplam);

            // Ödeme Şekli
            Label lblOdemeSekli = new Label
            {
                Text = "Ödeme Şekli:",
                Location = new Point(30, 100),
                AutoSize = true
            };

            cmbOdemeSekli = new ComboBox
            {
                Location = new Point(30, 130),
                Size = new Size(420, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12)
            };
            cmbOdemeSekli.Items.AddRange(new object[] { "Nakit", "Kredi Kartı", "Karışık (Nakit + Kart)" });
            cmbOdemeSekli.SelectedIndex = 0;
            cmbOdemeSekli.SelectedIndexChanged += CmbOdemeSekli_SelectedIndexChanged;

            // Nakit
            Label lblNakit = new Label
            {
                Text = "Nakit (₺):",
                Location = new Point(30, 180),
                AutoSize = true
            };

            txtNakit = new TextBox
            {
                Location = new Point(30, 210),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 16),
                Text = toplamTutar.ToString("N2"),
                TextAlign = HorizontalAlignment.Right
            };
            txtNakit.TextChanged += OdemeTutarDegisti;
            txtNakit.Enter += (s, e) => txtNakit.SelectAll();

            // Kredi Kartı
            Label lblKredi = new Label
            {
                Text = "Kredi Kartı (₺):",
                Location = new Point(250, 180),
                AutoSize = true
            };

            txtKrediKarti = new TextBox
            {
                Location = new Point(250, 210),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 16),
                Text = "0,00",
                TextAlign = HorizontalAlignment.Right,
                Enabled = false
            };
            txtKrediKarti.TextChanged += OdemeTutarDegisti;
            txtKrediKarti.Enter += (s, e) => txtKrediKarti.SelectAll();

            // Kalan ve Para Üstü
            lblKalan = new Label
            {
                Text = "Kalan: ₺0,00",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(30, 270),
                AutoSize = true
            };

            lblParaUstu = new Label
            {
                Text = "Para Üstü: ₺0,00",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 83),
                Location = new Point(30, 310),
                AutoSize = true
            };

            // Hızlı tuşlar
            FlowLayoutPanel panelHizli = new FlowLayoutPanel
            {
                Location = new Point(30, 360),
                Size = new Size(420, 45),
                FlowDirection = FlowDirection.LeftToRight
            };

            foreach (int tutar in new int[] { 10, 20, 50, 100, 200, 500 })
            {
                Button btn = new Button
                {
                    Text = $"₺{tutar}",
                    Size = new Size(65, 38),
                    BackColor = Color.FromArgb(240, 240, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10),
                    Tag = tutar
                };
                btn.Click += (s, e) => {
                    decimal mevcut = 0;
                    decimal.TryParse(txtNakit.Text.Replace(".", "").Replace(",", "."), 
                        System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out mevcut);
                    txtNakit.Text = (mevcut + (int)((Button)s).Tag).ToString("N2");
                };
                panelHizli.Controls.Add(btn);
            }

            // Ödeme Yap Butonu
            btnOdemeYap = new Button
            {
                Text = "✓ ÖDEMEYİ TAMAMLA",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(420, 50),
                Location = new Point(30, 415),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOdemeYap.Click += BtnOdemeYap_Click;

            this.Controls.AddRange(new Control[] {
                panelBaslik, lblOdemeSekli, cmbOdemeSekli,
                lblNakit, txtNakit, lblKredi, txtKrediKarti,
                lblKalan, lblParaUstu, panelHizli, btnOdemeYap
            });

            this.Load += (s, e) => { txtNakit.Focus(); txtNakit.SelectAll(); };
        }

        private void CmbOdemeSekli_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbOdemeSekli.SelectedIndex)
            {
                case 0: // Nakit
                    txtNakit.Enabled = true;
                    txtKrediKarti.Enabled = false;
                    txtNakit.Text = toplamTutar.ToString("N2");
                    txtKrediKarti.Text = "0,00";
                    break;
                case 1: // Kredi Kartı
                    txtNakit.Enabled = false;
                    txtKrediKarti.Enabled = true;
                    txtNakit.Text = "0,00";
                    txtKrediKarti.Text = toplamTutar.ToString("N2");
                    break;
                case 2: // Karışık
                    txtNakit.Enabled = true;
                    txtKrediKarti.Enabled = true;
                    txtNakit.Text = "0,00";
                    txtKrediKarti.Text = "0,00";
                    break;
            }
        }

        private void OdemeTutarDegisti(object sender, EventArgs e)
        {
            decimal.TryParse(txtNakit.Text.Replace(".", "").Replace(",", "."), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out nakit);
            decimal.TryParse(txtKrediKarti.Text.Replace(".", "").Replace(",", "."), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out krediKarti);

            decimal odenen = nakit + krediKarti;
            decimal kalan = toplamTutar - odenen;

            if (kalan > 0)
            {
                lblKalan.Text = $"Kalan: ₺{kalan:N2}";
                lblKalan.ForeColor = Color.Red;
                lblParaUstu.Text = "Para Üstü: ₺0,00";
            }
            else
            {
                lblKalan.Text = "Kalan: ₺0,00";
                lblKalan.ForeColor = Color.FromArgb(0, 122, 204);
                lblParaUstu.Text = $"Para Üstü: ₺{Math.Abs(kalan):N2}";
            }
        }

        private void BtnOdemeYap_Click(object sender, EventArgs e)
        {
            decimal odenen = nakit + krediKarti;
            if (odenen < toplamTutar)
            {
                MessageBox.Show("Ödeme tutarı yetersiz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Yeni fiş numarası al
                            string yeniFisNo = DateTime.Now.ToString("yyyyMMddHHmmss");
                            
                            // Satış kaydı oluştur (tbAlisVeris)
                            string sqlSatis = @"
                                INSERT INTO tbAlisVeris (nAlisverisID, sFisTipi, dteFaturaTarihi, nGirisCikis, 
                                    nMusteriID, lMalBedeli, lNetTutar, lPesinat, sKullaniciAdi, dteKayitTarihi, sMagaza)
                                VALUES (@fisNo, 'PS', @tarih, 2, @musteriId, @tutar, @tutar, @pesinat, @kullanici, GETDATE(), '001')";

                            using (SqlCommand cmd = new SqlCommand(sqlSatis, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@fisNo", yeniFisNo);
                                cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                                cmd.Parameters.AddWithValue("@musteriId", musteriId == 0 ? DBNull.Value : (object)musteriId);
                                cmd.Parameters.AddWithValue("@tutar", toplamTutar);
                                cmd.Parameters.AddWithValue("@pesinat", odenen);
                                cmd.Parameters.AddWithValue("@kullanici", Environment.UserName);
                                cmd.ExecuteNonQuery();
                            }

                            // Satış detayları (tbAlisverisSiparis)
                            int siraNo = 1;
                            foreach (DataRow row in sepet.Rows)
                            {
                                string sqlDetay = @"
                                    INSERT INTO tbAlisverisSiparis (nAlisverisID, nSiparisID, nStokID, lMiktar, lBirimFiyat, lTutar)
                                    VALUES (@fisNo, @siraNo, @stokId, @miktar, @birimFiyat, @tutar)";

                                using (SqlCommand cmd = new SqlCommand(sqlDetay, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@fisNo", yeniFisNo);
                                    cmd.Parameters.AddWithValue("@siraNo", siraNo++);
                                    cmd.Parameters.AddWithValue("@stokId", row["StokID"]);
                                    cmd.Parameters.AddWithValue("@miktar", row["Miktar"]);
                                    cmd.Parameters.AddWithValue("@birimFiyat", row["BirimFiyat"]);
                                    cmd.Parameters.AddWithValue("@tutar", row["Tutar"]);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Ödeme kaydı (tbOdeme)
                            if (nakit > 0)
                            {
                                KaydetOdeme(conn, trans, yeniFisNo, "N", nakit);
                            }
                            if (krediKarti > 0)
                            {
                                KaydetOdeme(conn, trans, yeniFisNo, "KK", krediKarti);
                            }

                            trans.Commit();

                            decimal paraUstu = odenen - toplamTutar;
                            string mesaj = $"✓ Satış başarıyla tamamlandı!\n\nFiş No: {yeniFisNo}";
                            if (paraUstu > 0 && nakit > 0)
                            {
                                mesaj += $"\n\n💵 Para Üstü: ₺{paraUstu:N2}";
                            }

                            MessageBox.Show(mesaj, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satış kaydedilemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KaydetOdeme(SqlConnection conn, SqlTransaction trans, string fisNo, string odemeTipi, decimal tutar)
        {
            string sql = @"INSERT INTO tbOdeme (nAlisverisID, sOdemeSekli, lTutar, dteKayitTarihi) 
                           VALUES (@fisNo, @odemeTipi, @tutar, GETDATE())";
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@fisNo", fisNo);
                cmd.Parameters.AddWithValue("@odemeTipi", odemeTipi);
                cmd.Parameters.AddWithValue("@tutar", tutar);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
