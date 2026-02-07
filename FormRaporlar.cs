using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormRaporlar : Form
    {
        private TabControl tabControl;
        private DataGridView dgvSatislar;
        private DataGridView dgvUrunler;
        private DateTimePicker dtpBaslangic;
        private DateTimePicker dtpBitis;
        private Label lblToplamCiro;

        public FormRaporlar()
        {
            InitializeComponent();
            RaporlariYukle();
        }

        private void InitializeComponent()
        {
            this.Text = "📊 Raporlar";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Panel panelUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblBaslik = new Label
            {
                Text = "📊 Satış Raporları",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 22),
                AutoSize = true
            };

            Label lblTarih = new Label
            {
                Text = "Tarih:",
                ForeColor = Color.White,
                Location = new Point(300, 30),
                AutoSize = true
            };

            dtpBaslangic = new DateTimePicker
            {
                Location = new Point(350, 27),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-30)
            };

            Label lblTire = new Label
            {
                Text = "-",
                ForeColor = Color.White,
                Location = new Point(490, 30),
                AutoSize = true
            };

            dtpBitis = new DateTimePicker
            {
                Location = new Point(510, 27),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            Button btnFiltrele = new Button
            {
                Text = "🔍 Filtrele",
                Location = new Point(660, 24),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFiltrele.Click += (s, e) => RaporlariYukle();

            lblToplamCiro = new Label
            {
                Text = "Toplam: ₺0",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 83),
                Location = new Point(800, 27),
                AutoSize = true
            };

            panelUst.Controls.AddRange(new Control[] { 
                lblBaslik, lblTarih, dtpBaslangic, lblTire, dtpBitis, btnFiltrele, lblToplamCiro 
            });

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11)
            };

            TabPage tabSatislar = new TabPage("💰 Satışlar");
            dgvSatislar = CreateDataGridView();
            tabSatislar.Controls.Add(dgvSatislar);

            TabPage tabUrunler = new TabPage("🏆 En Çok Satanlar");
            dgvUrunler = CreateDataGridView();
            tabUrunler.Controls.Add(dgvUrunler);

            tabControl.TabPages.AddRange(new TabPage[] { tabSatislar, tabUrunler });

            Panel panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Button btnKapat = new Button
            {
                Text = "Kapat",
                Location = new Point(950, 8),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKapat.Click += (s, e) => this.Close();

            panelAlt.Controls.Add(btnKapat);

            this.Controls.Add(tabControl);
            this.Controls.Add(panelAlt);
            this.Controls.Add(panelUst);
        }

        private DataGridView CreateDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersHeight = 40;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowTemplate.Height = 30;
            return dgv;
        }

        private void RaporlariYukle()
        {
            SatislariYukle();
            EnCokSatanlariYukle();
        }

        private void SatislariYukle()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    string sql = @"
                        SELECT 
                            nAlisverisID as 'Fiş No',
                            CONVERT(varchar(16), dteFaturaTarihi, 104) + ' ' + CONVERT(varchar(5), dteFaturaTarihi, 108) as 'Tarih',
                            ISNULL(m.sAdi + ' ' + ISNULL(m.sSoyadi,''), 'Peşin') as 'Müşteri',
                            a.lNetTutar as 'Tutar',
                            a.sKullaniciAdi as 'Kasiyer'
                        FROM tbAlisVeris a
                        LEFT JOIN tbMusteri m ON a.nMusteriID = m.nMusteriID
                        WHERE a.dteFaturaTarihi BETWEEN @baslangic AND @bitis
                            AND a.sFisTipi = 'PS'
                        ORDER BY a.dteFaturaTarihi DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@baslangic", dtpBaslangic.Value.Date);
                        adapter.SelectCommand.Parameters.AddWithValue("@bitis", dtpBitis.Value.Date.AddDays(1));

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvSatislar.DataSource = dt;

                        decimal toplamCiro = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Tutar"] != DBNull.Value)
                                toplamCiro += Convert.ToDecimal(row["Tutar"]);
                        }
                        lblToplamCiro.Text = $"Toplam: ₺{toplamCiro:N2}";

                        if (dgvSatislar.Columns.Contains("Tutar"))
                            dgvSatislar.Columns["Tutar"].DefaultCellStyle.Format = "₺#,##0.00";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnCokSatanlariYukle()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    string sql = @"
                        SELECT TOP 50
                            s.sKodu as 'Kod',
                            s.sAciklama as 'Ürün Adı',
                            SUM(d.lMiktar) as 'Satılan',
                            SUM(d.lTutar) as 'Toplam ₺'
                        FROM tbAlisverisSiparis d
                        INNER JOIN tbAlisVeris a ON d.nAlisverisID = a.nAlisverisID
                        INNER JOIN tbStok s ON d.nStokID = s.nStokID
                        WHERE a.dteFaturaTarihi BETWEEN @baslangic AND @bitis
                        GROUP BY s.sKodu, s.sAciklama
                        ORDER BY SUM(d.lMiktar) DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@baslangic", dtpBaslangic.Value.Date);
                        adapter.SelectCommand.Parameters.AddWithValue("@bitis", dtpBitis.Value.Date.AddDays(1));

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUrunler.DataSource = dt;

                        if (dgvUrunler.Columns.Contains("Toplam ₺"))
                            dgvUrunler.Columns["Toplam ₺"].DefaultCellStyle.Format = "₺#,##0.00";
                    }
                }
            }
            catch { }
        }
    }
}
