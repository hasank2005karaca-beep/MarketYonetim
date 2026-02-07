using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormMusteriSec : Form
    {
        private DataGridView dgvMusteriler;
        private TextBox txtArama;
        
        public int SecilenMusteriId { get; private set; }
        public string SecilenMusteriAdi { get; private set; }

        public FormMusteriSec()
        {
            InitializeComponent();
            MusterileriYukle();
        }

        private void InitializeComponent()
        {
            this.Text = "👤 Müşteri Seç";
            this.Size = new Size(800, 600);
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
                Text = "👤 Müşteri Seç (70 müşteri)",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            txtArama = new TextBox
            {
                Location = new Point(350, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12),
                PlaceholderText = "🔍 Müşteri ara..."
            };
            txtArama.TextChanged += TxtArama_TextChanged;

            panelUst.Controls.AddRange(new Control[] { lblBaslik, txtArama });

            dgvMusteriler = new DataGridView
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
            dgvMusteriler.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvMusteriler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvMusteriler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMusteriler.ColumnHeadersHeight = 40;
            dgvMusteriler.EnableHeadersVisualStyles = false;
            dgvMusteriler.CellDoubleClick += DgvMusteriler_CellDoubleClick;

            Panel panelAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Button btnSec = new Button
            {
                Text = "✓ Seç",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(540, 10),
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSec.Click += BtnSec_Click;

            Button btnIptal = new Button
            {
                Text = "İptal",
                Size = new Size(100, 40),
                Location = new Point(670, 10),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnIptal.Click += (s, e) => this.Close();

            panelAlt.Controls.AddRange(new Control[] { btnSec, btnIptal });

            this.Controls.Add(dgvMusteriler);
            this.Controls.Add(panelAlt);
            this.Controls.Add(panelUst);
        }

        private void MusterileriYukle(string filtre = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
                {
                    string sql = @"
                        SELECT 
                            nMusteriID as 'ID',
                            sAdi + ' ' + ISNULL(sSoyadi, '') as 'Müşteri Adı',
                            ISNULL(sTelefon1, '') as 'Telefon',
                            ISNULL(sIl, '') as 'İl',
                            ISNULL(sAdres, '') as 'Adres'
                        FROM tbMusteri WHERE 1=1";

                    if (!string.IsNullOrEmpty(filtre))
                    {
                        sql += " AND (sAdi LIKE @filtre OR sSoyadi LIKE @filtre OR sTelefon1 LIKE @filtre)";
                    }
                    sql += " ORDER BY sAdi";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(filtre))
                            adapter.SelectCommand.Parameters.AddWithValue("@filtre", $"%{filtre}%");

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvMusteriler.DataSource = dt;
                        
                        if (dgvMusteriler.Columns.Contains("ID"))
                            dgvMusteriler.Columns["ID"].Visible = false;
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
            MusterileriYukle(txtArama.Text.Trim());
        }

        private void DgvMusteriler_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) BtnSec_Click(null, null);
        }

        private void BtnSec_Click(object sender, EventArgs e)
        {
            if (dgvMusteriler.SelectedRows.Count > 0)
            {
                SecilenMusteriId = Convert.ToInt32(dgvMusteriler.SelectedRows[0].Cells["ID"].Value);
                SecilenMusteriAdi = dgvMusteriler.SelectedRows[0].Cells["Müşteri Adı"].Value.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
