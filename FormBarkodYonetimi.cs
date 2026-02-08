using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormBarkodYonetimi : Form
    {
        private readonly int stokId;
        private ListBox lstBarkodlar;
        private TextBox txtBarkod;

        public FormBarkodYonetimi(int stokId)
        {
            this.stokId = stokId;
            InitializeComponent();
            BarkodlariYukle();
        }

        private void InitializeComponent()
        {
            Text = "🏷️ Barkod Yönetimi";
            Size = new Size(400, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            lstBarkodlar = new ListBox { Dock = DockStyle.Top, Height = 220 };
            txtBarkod = new TextBox { Width = 160 };
            Button btnEkle = new Button { Text = "Ekle", Width = 80 };
            btnEkle.Click += BtnEkle_Click;
            Button btnSil = new Button { Text = "Sil", Width = 80 };
            btnSil.Click += BtnSil_Click;

            FlowLayoutPanel panelAlt = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, FlowDirection = FlowDirection.LeftToRight };
            panelAlt.Controls.AddRange(new Control[] { txtBarkod, btnEkle, btnSil });

            Controls.Add(lstBarkodlar);
            Controls.Add(panelAlt);
        }

        private void BarkodlariYukle()
        {
            try
            {
                DataTable dt = VeriKatmani.BarkodlariGetir(stokId);
                lstBarkodlar.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    lstBarkodlar.Items.Add(row["sBarkod"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Barkodlar yüklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEkle_Click(object sender, EventArgs e)
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

            try
            {
                VeriKatmani.BarkodEkle(stokId, barkod);
                txtBarkod.Clear();
                BarkodlariYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Barkod eklenemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (lstBarkodlar.SelectedItem == null)
            {
                return;
            }

            string barkod = lstBarkodlar.SelectedItem.ToString();
            try
            {
                VeriKatmani.BarkodSil(stokId, barkod);
                BarkodlariYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Barkod silinemedi. Detay: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
