using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormVeresiyeOdemeAl : Form
    {
        private readonly int musteriId;
        private readonly string musteriAdi;
        private NumericUpDown nudTutar;
        private ComboBox cmbOdeme;
        private DateTimePicker dtTarih;
        private TextBox txtAciklama;

        public FormVeresiyeOdemeAl(int musteriId, string musteriAdi)
        {
            this.musteriId = musteriId;
            this.musteriAdi = musteriAdi ?? string.Empty;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // S7-FIX: DPI ölçekleme
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "💳 Veresiye Tahsilat";
            Size = new Size(420, 320);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);

            Label lblMusteri = new Label
            {
                Text = $"Müşteri: {musteriAdi}",
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblTarih = new Label
            {
                Text = "Tarih",
                Location = new Point(20, 60),
                AutoSize = true
            };

            dtTarih = new DateTimePicker
            {
                Location = new Point(140, 55),
                Width = 200,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            Label lblOdeme = new Label
            {
                Text = "Ödeme Şekli",
                Location = new Point(20, 100),
                AutoSize = true
            };

            cmbOdeme = new ComboBox
            {
                Location = new Point(140, 95),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOdeme.Items.Add(new ComboBoxItem("Nakit", "N"));
            cmbOdeme.Items.Add(new ComboBoxItem("Kredi Kartı", "KK"));
            cmbOdeme.SelectedIndex = 0;

            Label lblTutar = new Label
            {
                Text = "Tutar",
                Location = new Point(20, 140),
                AutoSize = true
            };

            nudTutar = new NumericUpDown
            {
                Location = new Point(140, 135),
                Width = 200,
                DecimalPlaces = 2,
                Maximum = 1000000,
                Minimum = 0
            };

            Label lblAciklama = new Label
            {
                Text = "Açıklama",
                Location = new Point(20, 180),
                AutoSize = true
            };

            txtAciklama = new TextBox
            {
                Location = new Point(140, 175),
                Width = 200
            };

            Button btnKaydet = new Button
            {
                Text = "Kaydet",
                Location = new Point(140, 220),
                Width = 90,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKaydet.Click += BtnKaydet_Click;

            Button btnIptal = new Button
            {
                Text = "İptal",
                Location = new Point(250, 220),
                Width = 90,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnIptal.Click += (s, e) => Close();

            Controls.Add(lblMusteri);
            Controls.Add(lblTarih);
            Controls.Add(dtTarih);
            Controls.Add(lblOdeme);
            Controls.Add(cmbOdeme);
            Controls.Add(lblTutar);
            Controls.Add(nudTutar);
            Controls.Add(lblAciklama);
            Controls.Add(txtAciklama);
            Controls.Add(btnKaydet);
            Controls.Add(btnIptal);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (nudTutar.Value <= 0)
            {
                MessageBox.Show("Tutar 0'dan büyük olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string odemeKodu = (cmbOdeme.SelectedItem as ComboBoxItem)?.Value ?? "N";

            try
            {
                VeriKatmani.VeresiyeTahsilatEkle(
                    musteriId,
                    dtTarih.Value,
                    odemeKodu,
                    nudTutar.Value,
                    txtAciklama.Text.Trim()
                );

                MessageBox.Show("Tahsilat kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // S7-FIX: DB hatalarını kullanıcıya göster
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ComboBoxItem
        {
            public string Text { get; }
            public string Value { get; }

            public ComboBoxItem(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
