using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormUrunYonetimi : Form
    {
        private DataGridView dgvUrunler;
        private TextBox txtArama;

        public FormUrunYonetimi()
        {
            InitializeComponent();
            UrunleriYukle();
        }

        private void InitializeComponent()
        {
            this.Text = "📦 Ürün Yönetimi";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            Panel panelUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblBaslik = new Label
            {
                Text = "📦 Ürün Listesi (8570 ürün)",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            txtArama = new TextBox
            {
                Location = new Point(400, 20),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 12),
                PlaceholderText = "🔍 Ürün ara (kod, ad, barkod)..."
            };
            txtArama.TextChanged += TxtArama_TextChanged;

            Button btnYenile = new Button
            {
                Text = "🔄 Yenile",
                Location = new Point(770, 18),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnYenile.Click += (s, e) => UrunleriYukle();

            panelUst.Controls.AddRange(new Control[] { lblBaslik, txtArama, btnYenile });

            dgvUrunler = new DataGridView
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
            dgvUrunler.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvUrunler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvUrunler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUrunler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUrunler.ColumnHeadersHeight = 40;
            dgvUrunler.EnableHeadersVisualStyles = false;
            dgvUrunler.RowTemplate.Height = 32;

            Panel panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Button btnKapat = new Button
            {
                Text = "Kapat",
                Location = new Point(1050, 8),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKapat.Click += (s, e) => this.Close();

            panelAlt.Controls.Add(btnKapat);

            this.Controls.Add(dgvUrunler);
            this.Controls.Add(panelAlt);
            this.Controls.Add(panelUst);
        }

        private void UrunleriYukle(string filtre = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    string sql = @"
                        SELECT TOP 500
                            s.nStokID as 'ID',
                            s.sKodu as 'Kod',
                            ISNULL(b.sBarkod, '') as 'Barkod',
                            s.sAciklama as 'Ürün Adı',
                            s.sBirimCinsi1 as 'Birim',
                            ISNULL(fa.lFiyat, 0) as 'Alış ₺',
                            ISNULL(fs.lFiyat, 0) as 'Satış ₺',
                            CASE s.sKdvTipi 
                                WHEN '01' THEN '%0'
                                WHEN '02' THEN '%1'
                                WHEN '03' THEN '%8'
                                WHEN '04' THEN '%18'
                                WHEN '05' THEN '%18'
                                ELSE '%18'
                            END as 'KDV'
                        FROM tbStok s
                        LEFT JOIN tbStokBarkodu b ON s.nStokID = b.nStokId
                        LEFT JOIN tbStokFiyati fa ON s.nStokID = fa.nStokID AND fa.sFiyatTipi = 'A'
                        LEFT JOIN tbStokFiyati fs ON s.nStokID = fs.nStokID AND fs.sFiyatTipi = '1'
                        WHERE 1=1";

                    if (!string.IsNullOrEmpty(filtre))
                    {
                        sql += @" AND (s.sKodu LIKE @filtre OR s.sAciklama LIKE @filtre 
                                 OR s.sKisaAdi LIKE @filtre OR b.sBarkod LIKE @filtre)";
                    }

                    sql += " ORDER BY s.sAciklama";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(filtre))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@filtre", $"%{filtre}%");
                        }

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUrunler.DataSource = dt;

                        if (dgvUrunler.Columns.Contains("Alış ₺"))
                            dgvUrunler.Columns["Alış ₺"].DefaultCellStyle.Format = "₺#,##0.00";
                        if (dgvUrunler.Columns.Contains("Satış ₺"))
                            dgvUrunler.Columns["Satış ₺"].DefaultCellStyle.Format = "₺#,##0.00";
                        if (dgvUrunler.Columns.Contains("ID"))
                            dgvUrunler.Columns["ID"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            UrunleriYukle(txtArama.Text.Trim());
        }
    }
}
