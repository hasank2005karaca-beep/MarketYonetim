using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormOdeme : Form
    {
        private readonly DataTable sepet;
        private readonly int musteriId;
        private decimal toplamBrut;
        private decimal satirIskontoToplam;
        private decimal dipIskontoTutar;
        private decimal netTutar;
        private decimal kdvToplam;

        private DataGridView dgvSepet;
        private TextBox txtDipIskonto;
        private Label lblMalBedeli;
        private Label lblIskonto;
        private Label lblNet;
        private Label lblKdv;
        private ListView lvKdvDagitim;
        private RadioButton rdoNakit;
        private RadioButton rdoKart;
        private RadioButton rdoKarisik;
        private RadioButton rdoVeresiye;
        private TextBox txtNakit;
        private TextBox txtKart;
        private Label lblParaUstu;
        private Button btnOnay;

        public FormOdeme(DataTable sepetData, int musteri)
        {
            sepet = sepetData.Copy();
            musteriId = musteri;
            InitializeComponent();
            Hesapla();
        }

        private void InitializeComponent()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10);
            Text = "💰 Ödeme";
            Size = new Size(900, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            layout.Controls.Add(OlusturSepetPanel(), 0, 0);
            layout.Controls.Add(OlusturOdemePanel(), 1, 0);

            Controls.Add(layout);
        }

        private Control OlusturSepetPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            dgvSepet = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 350,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            dgvSepet.DataSource = sepet;
            dgvSepet.Columns["nStokID"].Visible = false;
            dgvSepet.Columns["sBirimCinsi"].Visible = false;
            dgvSepet.Columns["nIskontoYuzde"].HeaderText = "İskonto%";
            dgvSepet.Columns["lBirimFiyat"].HeaderText = "Birim Fiyat";
            dgvSepet.Columns["lSatirToplam"].HeaderText = "Satır Toplam";
            dgvSepet.Columns["nKdvOrani"].HeaderText = "KDV%";
            dgvSepet.Columns["lKdvTutar"].Visible = false;

            foreach (DataGridViewColumn col in dgvSepet.Columns)
            {
                if (col.DataPropertyName == "lBirimFiyat" || col.DataPropertyName == "lSatirToplam")
                {
                    col.DefaultCellStyle.Format = "N2";
                }
            }

            var lblDip = new Label
            {
                Text = "Dip İskonto %",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 370)
            };

            txtDipIskonto = new TextBox
            {
                Text = "0",
                Width = 80,
                Location = new Point(10, 395)
            };
            txtDipIskonto.TextChanged += (_, __) => Hesapla();

            var lblDagitim = new Label
            {
                Text = "KDV Dağılımı",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 430)
            };

            lvKdvDagitim = new ListView
            {
                View = View.Details,
                Width = 480,
                Height = 160,
                Location = new Point(10, 455),
                FullRowSelect = true
            };
            lvKdvDagitim.Columns.Add("Oran", 80);
            lvKdvDagitim.Columns.Add("Matrah", 180);
            lvKdvDagitim.Columns.Add("KDV", 180);

            panel.Controls.Add(dgvSepet);
            panel.Controls.Add(lblDip);
            panel.Controls.Add(txtDipIskonto);
            panel.Controls.Add(lblDagitim);
            panel.Controls.Add(lvKdvDagitim);

            return panel;
        }

        private Control OlusturOdemePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            lblMalBedeli = new Label
            {
                Text = "Mal Bedeli: ₺0,00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 20)
            };

            lblIskonto = new Label
            {
                Text = "İskonto: ₺0,00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 50)
            };

            lblNet = new Label
            {
                Text = "Net Tutar: ₺0,00",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 83),
                AutoSize = true,
                Location = new Point(10, 80)
            };

            lblKdv = new Label
            {
                Text = "KDV: ₺0,00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 115)
            };

            var grpOdeme = new GroupBox
            {
                Text = "Ödeme Tipi",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Width = 300,
                Height = 180,
                Location = new Point(10, 150)
            };

            rdoNakit = new RadioButton { Text = "Nakit", Location = new Point(15, 30), Checked = true };
            rdoKart = new RadioButton { Text = "Kredi Kartı", Location = new Point(15, 60) };
            rdoKarisik = new RadioButton { Text = "Karışık", Location = new Point(15, 90) };
            rdoVeresiye = new RadioButton { Text = "Veresiye", Location = new Point(15, 120) };

            rdoNakit.CheckedChanged += (_, __) => OdemeTipiDegisti();
            rdoKart.CheckedChanged += (_, __) => OdemeTipiDegisti();
            rdoKarisik.CheckedChanged += (_, __) => OdemeTipiDegisti();
            rdoVeresiye.CheckedChanged += (_, __) => OdemeTipiDegisti();

            grpOdeme.Controls.Add(rdoNakit);
            grpOdeme.Controls.Add(rdoKart);
            grpOdeme.Controls.Add(rdoKarisik);
            grpOdeme.Controls.Add(rdoVeresiye);

            var lblNakit = new Label
            {
                Text = "Nakit",
                AutoSize = true,
                Location = new Point(10, 350)
            };

            txtNakit = new TextBox
            {
                Width = 150,
                Location = new Point(10, 375)
            };
            txtNakit.TextChanged += (_, __) => ParaUstuHesapla();

            var lblKart = new Label
            {
                Text = "Kredi Kartı",
                AutoSize = true,
                Location = new Point(10, 410)
            };

            txtKart = new TextBox
            {
                Width = 150,
                Location = new Point(10, 435)
            };
            txtKart.TextChanged += (_, __) => ParaUstuHesapla();

            lblParaUstu = new Label
            {
                Text = "Para Üstü: ₺0,00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 470)
            };

            btnOnay = new Button
            {
                Text = "Ödemeyi Onayla",
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 250,
                Height = 45,
                Location = new Point(10, 520)
            };
            btnOnay.Click += BtnOnay_Click;

            panel.Controls.Add(lblMalBedeli);
            panel.Controls.Add(lblIskonto);
            panel.Controls.Add(lblNet);
            panel.Controls.Add(lblKdv);
            panel.Controls.Add(grpOdeme);
            panel.Controls.Add(lblNakit);
            panel.Controls.Add(txtNakit);
            panel.Controls.Add(lblKart);
            panel.Controls.Add(txtKart);
            panel.Controls.Add(lblParaUstu);
            panel.Controls.Add(btnOnay);

            OdemeTipiDegisti();

            return panel;
        }

        private void Hesapla()
        {
            toplamBrut = 0m;
            satirIskontoToplam = 0m;

            foreach (DataRow row in sepet.Rows)
            {
                decimal miktar = Convert.ToDecimal(row["lMiktar"]);
                decimal birimFiyat = Convert.ToDecimal(row["lBirimFiyat"]);
                decimal iskonto = Convert.ToDecimal(row["nIskontoYuzde"]);

                decimal brut = Yardimcilar.YuvarlaKurus(miktar * birimFiyat);
                decimal satirIskonto = Yardimcilar.YuvarlaKurus(brut * (iskonto / 100m));
                decimal satirNet = Yardimcilar.YuvarlaKurus(brut - satirIskonto);

                row["lSatirToplam"] = satirNet;
                row["lKdvTutar"] = Yardimcilar.KdvTutarHesapla(satirNet, Convert.ToDecimal(row["nKdvOrani"]));

                toplamBrut += brut;
                satirIskontoToplam += satirIskonto;
            }

            decimal dipIskontoYuzde = ParseDecimal(txtDipIskonto.Text);
            decimal satirNetToplam = sepet.AsEnumerable().Sum(r => r.Field<decimal>("lSatirToplam"));
            dipIskontoTutar = dipIskontoYuzde > 0 ? Yardimcilar.YuvarlaKurus(satirNetToplam * (dipIskontoYuzde / 100m)) : 0m;
            netTutar = Yardimcilar.YuvarlaKurus(satirNetToplam - dipIskontoTutar);

            kdvToplam = 0m;
            var dagitim = new System.Collections.Generic.Dictionary<decimal, KdvDagitimSonuc>();
            foreach (DataRow row in sepet.Rows)
            {
                decimal satirNet = Convert.ToDecimal(row["lSatirToplam"]);
                decimal oran = Convert.ToDecimal(row["nKdvOrani"]);
                decimal pay = satirNetToplam > 0 ? satirNet / satirNetToplam : 0m;
                decimal dipPay = Yardimcilar.YuvarlaKurus(dipIskontoTutar * pay);
                decimal satirNetDip = Yardimcilar.YuvarlaKurus(satirNet - dipPay);
                decimal matrah = Yardimcilar.KdvMatrahHesapla(satirNetDip, oran);
                decimal kdv = Yardimcilar.KdvTutarHesapla(satirNetDip, oran);

                if (!dagitim.ContainsKey(oran))
                {
                    dagitim[oran] = new KdvDagitimSonuc();
                }

                dagitim[oran].Matrah += matrah;
                dagitim[oran].Kdv += kdv;
                kdvToplam += kdv;
            }

            lblMalBedeli.Text = $"Mal Bedeli: {Yardimcilar.ParaFormatla(toplamBrut)}";
            lblIskonto.Text = $"İskonto: {Yardimcilar.ParaFormatla(satirIskontoToplam + dipIskontoTutar)}";
            lblNet.Text = $"Net Tutar: {Yardimcilar.ParaFormatla(netTutar)}";
            lblKdv.Text = $"KDV: {Yardimcilar.ParaFormatla(kdvToplam)}";

            lvKdvDagitim.Items.Clear();
            foreach (var item in dagitim.OrderBy(x => x.Key))
            {
                var listItem = new ListViewItem($"%{item.Key}");
                listItem.SubItems.Add(Yardimcilar.ParaFormatla(item.Value.Matrah));
                listItem.SubItems.Add(Yardimcilar.ParaFormatla(item.Value.Kdv));
                lvKdvDagitim.Items.Add(listItem);
            }

            OdemeTipiDegisti();
        }

        private void OdemeTipiDegisti()
        {
            if (rdoNakit.Checked)
            {
                txtNakit.Enabled = true;
                txtKart.Enabled = false;
                txtNakit.Text = netTutar.ToString("N2");
                txtKart.Text = "0";
            }
            else if (rdoKart.Checked)
            {
                txtNakit.Enabled = false;
                txtKart.Enabled = true;
                txtNakit.Text = "0";
                txtKart.Text = netTutar.ToString("N2");
            }
            else if (rdoKarisik.Checked)
            {
                txtNakit.Enabled = true;
                txtKart.Enabled = true;
                txtNakit.Text = "0";
                txtKart.Text = "0";
            }
            else if (rdoVeresiye.Checked)
            {
                txtNakit.Enabled = false;
                txtKart.Enabled = false;
                txtNakit.Text = "0";
                txtKart.Text = "0";
            }

            ParaUstuHesapla();
        }

        private void ParaUstuHesapla()
        {
            decimal nakit = ParseDecimal(txtNakit.Text);
            decimal kart = ParseDecimal(txtKart.Text);
            decimal odenen = nakit + kart;
            decimal paraUstu = odenen - netTutar;

            lblParaUstu.Text = $"Para Üstü: {Yardimcilar.ParaFormatla(Math.Max(0, paraUstu))}";
        }

        private void BtnOnay_Click(object sender, EventArgs e)
        {
            string odemeTipi = rdoVeresiye.Checked ? "Veresiye"
                : rdoKarisik.Checked ? "Karışık"
                : rdoKart.Checked ? "Kredi Kartı"
                : "Nakit";

            if (rdoVeresiye.Checked && musteriId == 0)
            {
                MessageBox.Show("Veresiye için müşteri seçmelisiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal nakit = ParseDecimal(txtNakit.Text);
            decimal kart = ParseDecimal(txtKart.Text);

            if (!rdoVeresiye.Checked && nakit + kart < netTutar)
            {
                MessageBox.Show("Ödeme tutarı yetersiz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Ödeme onaylansın mı?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                decimal dipYuzde = ParseDecimal(txtDipIskonto.Text);
                int satisId = VeriKatmani.SatisKaydet(sepet, musteriId, odemeTipi, nakit, kart, dipYuzde);
                MessageBox.Show($"Satış kaydedildi. (ID: {satisId})", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satış kaydedilemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static decimal ParseDecimal(string text)
        {
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var value))
            {
                return value;
            }

            if (decimal.TryParse(text.Replace(".", ","), NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out value))
            {
                return value;
            }

            return 0m;
        }
    }
}
