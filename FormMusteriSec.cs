using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public partial class FormMusteriSec : Form
    {
        private DataGridView dgvMusteriler;
        private TextBox txtArama;
        private Timer aramaTimer;

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
                Text = "👤 Müşteri Seç",
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
                PlaceholderText = "🔍 Ad/Soyad/VergiNo/Telefon"
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
            dgvMusteriler.KeyDown += DgvMusteriler_KeyDown;

            aramaTimer = new Timer { Interval = 300 };
            aramaTimer.Tick += AramaTimer_Tick;

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
                string arama = string.IsNullOrWhiteSpace(filtre) || filtre.Length < 2 ? null : filtre;
                DataTable dt = VeriKatmani.MusterileriGetir(arama, 0, 200);
                dt.Columns.Add("Müşteri Adı", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    string adi = row["sAdi"]?.ToString() ?? string.Empty;
                    string soyadi = row["sSoyadi"]?.ToString() ?? string.Empty;
                    row["Müşteri Adı"] = string.Format("{0} {1}", adi, soyadi).Trim();
                }

                if (dt.Columns.Contains("nMusteriID")) dt.Columns["nMusteriID"].ColumnName = "ID";
                if (dt.Columns.Contains("sTelefon1")) dt.Columns["sTelefon1"].ColumnName = "Telefon";
                if (dt.Columns.Contains("sEmail")) dt.Columns["sEmail"].ColumnName = "E-posta";
                if (dt.Columns.Contains("sVergiNo")) dt.Columns["sVergiNo"].ColumnName = "Vergi No";

                dgvMusteriler.DataSource = dt;

                if (dgvMusteriler.Columns.Contains("ID"))
                {
                    dgvMusteriler.Columns["ID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            MusterileriYukle(txtArama.Text.Trim());
        }

        private void DgvMusteriler_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) BtnSec_Click(null, null);
        }

        private void DgvMusteriler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnSec_Click(null, null);
                e.Handled = true;
            }
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
