using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormFiyatDuzenle : Form
    {
        private readonly int stokId;
        private DataGridView dgvFiyatlar;
        private ComboBox cmbFiyatTipi;
        private NumericUpDown nudFiyat;

        public FormFiyatDuzenle(int stokId)
        {
            this.stokId = stokId;
            InitializeComponent();
            FiyatlariYukle();
        }

        private void InitializeComponent()
        {
            Text = "💲 Fiyat Yönetimi";
            Size = new Size(420, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            dgvFiyatlar = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 220,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false
            };
            dgvFiyatlar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat Tipi", DataPropertyName = "sFiyatTipi", Width = 120 });
            dgvFiyatlar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat", DataPropertyName = "lFiyat", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            cmbFiyatTipi = new ComboBox { Width = 120 };
            nudFiyat = new NumericUpDown { Width = 120, DecimalPlaces = 2, Maximum = 1000000, Increment = 1 };
            Button btnGuncelle = new Button { Text = "Güncelle", Width = 100 };
            btnGuncelle.Click += BtnGuncelle_Click;

            FlowLayoutPanel panelAlt = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60 };
            panelAlt.Controls.AddRange(new Control[]
            {
                new Label { Text = "Tip:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft },
                cmbFiyatTipi,
                new Label { Text = "Fiyat:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft },
                nudFiyat,
                btnGuncelle
            });

            Controls.Add(dgvFiyatlar);
            Controls.Add(panelAlt);
        }

        private void FiyatlariYukle()
        {
            try
            {
                DataTable dt = VeriKatmani.FiyatlariGetir(stokId);
                dgvFiyatlar.DataSource = dt;
                cmbFiyatTipi.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cmbFiyatTipi.Items.Add(row["sFiyatTipi"].ToString());
                }

                if (!cmbFiyatTipi.Items.Contains(Ayarlar.VarsayilanFiyatTipi))
                {
                    cmbFiyatTipi.Items.Add(Ayarlar.VarsayilanFiyatTipi);
                }

                if (cmbFiyatTipi.Items.Count > 0)
                {
                    cmbFiyatTipi.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fiyatlar yüklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (cmbFiyatTipi.SelectedItem == null)
            {
                MessageBox.Show("Fiyat tipi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fiyatTipi = cmbFiyatTipi.SelectedItem.ToString();
            try
            {
                VeriKatmani.FiyatGuncelle(stokId, fiyatTipi, nudFiyat.Value);
                FiyatlariYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fiyat güncellenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
