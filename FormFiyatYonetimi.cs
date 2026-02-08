using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarketYonetim
{
    public class FormFiyatYonetimi : Form
    {
        private TextBox txtArama;
        private ComboBox cmbFiyatTipi;
        private DataGridView dgvUrunler;
        private DataGridView dgvFiyatlar;
        private ComboBox cmbTekilFiyatTipi;
        private NumericUpDown nudYeniFiyat;
        private Button btnKaydet;
        private Button btnVarsayilan;
        private Label lblSayfa;
        private Button btnOnceki;
        private Button btnSonraki;
        private Timer aramaTimer;
        private int sayfa = 1;
        private int? seciliStokId;

        public FormFiyatYonetimi()
        {
            InitializeComponent();
            FiyatTipleriniYukle();
            UrunleriYukle();
        }

        private void InitializeComponent()
        {
            // S7-FIX: DPI ölçekleme
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "💲 Fiyat Yönetimi";
            Size = new Size(1350, 820);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;

            Panel filtrePanel = new Panel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(10), BackColor = Color.FromArgb(248, 248, 248) };
            txtArama = new TextBox { Width = 240, PlaceholderText = "Ürün adı/kod (min 2)" };
            txtArama.TextChanged += TxtArama_TextChanged;
            cmbFiyatTipi = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFiyatTipi.SelectedIndexChanged += (s, e) => { sayfa = 1; UrunleriYukle(); };

            btnOnceki = new Button { Text = "◀", Width = 40 };
            btnOnceki.Click += (s, e) =>
            {
                try
                {
                    if (sayfa > 1)
                    {
                        sayfa--;
                        UrunleriYukle();
                    }
                }
                catch (Exception ex)
                {
                    // S7-FIX: DB hatalarını kullanıcıya göster
                    MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            btnSonraki = new Button { Text = "▶", Width = 40 };
            btnSonraki.Click += (s, e) =>
            {
                try
                {
                    sayfa++;
                    UrunleriYukle();
                }
                catch (Exception ex)
                {
                    // S7-FIX: DB hatalarını kullanıcıya göster
                    MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            lblSayfa = new Label { AutoSize = true, Text = "Sayfa: 1", Padding = new Padding(0, 6, 0, 0) };

            FlowLayoutPanel filtreLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            filtreLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Arama:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                txtArama,
                new Label { Text = "Fiyat Tipi:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                cmbFiyatTipi,
                btnOnceki,
                btnSonraki,
                lblSayfa
            });
            filtrePanel.Controls.Add(filtreLayout);

            dgvUrunler = new DataGridView
            {
                Dock = DockStyle.Left,
                Width = 860,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvUrunler.ColumnHeadersHeight = 32;
            dgvUrunler.RowTemplate.Height = 28;
            dgvUrunler.SelectionChanged += DgvUrunler_SelectionChanged;
            dgvUrunler.DataBindingComplete += (s, e) => dgvUrunler.ClearSelection();
            Yardimcilar.DoubleBufferedAktifEt(dgvUrunler);

            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "nStokID", Visible = false });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kodu", DataPropertyName = "sKodu", Width = 110 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "sAciklama", Width = 240 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barkod", DataPropertyName = "sBarkod", Width = 130 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fiyat", DataPropertyName = "lFiyat", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stok", DataPropertyName = "StokMiktari", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kat1", HeaderText = "Kat1", DataPropertyName = "Kategori1", Width = 90 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kat2", HeaderText = "Kat2", DataPropertyName = "Kategori2", Width = 90 });

            Panel sagPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            dgvFiyatlar = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 260,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvFiyatlar.ColumnHeadersHeight = 28;
            dgvFiyatlar.RowTemplate.Height = 26;
            dgvFiyatlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tip", DataPropertyName = "sFiyatTipi", HeaderText = "Fiyat Tipi", Width = 120 });
            dgvFiyatlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "Deger", DataPropertyName = "lFiyat", HeaderText = "Fiyat", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            Yardimcilar.DoubleBufferedAktifEt(dgvFiyatlar);

            GroupBox tekilPanel = new GroupBox { Text = "Tekil Fiyat Güncelle", Dock = DockStyle.Top, Height = 160, Padding = new Padding(10) };
            cmbTekilFiyatTipi = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            nudYeniFiyat = new NumericUpDown { Width = 120, DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, Increment = 0.05m };
            btnKaydet = new Button { Text = "Kaydet", Width = 90, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnKaydet.Click += BtnKaydet_Click;

            btnVarsayilan = new Button { Text = $"Varsayılan ({Ayarlar.VarsayilanFiyatTipi})", Width = 180 };
            btnVarsayilan.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(Ayarlar.VarsayilanFiyatTipi))
                {
                    cmbTekilFiyatTipi.SelectedItem = Ayarlar.VarsayilanFiyatTipi;
                }
                BtnKaydet_Click(s, e);
            };

            FlowLayoutPanel tekilLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            tekilLayout.Controls.AddRange(new Control[]
            {
                new Label { Text = "Fiyat Tipi:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
                cmbTekilFiyatTipi,
                new Label { Text = "Yeni Fiyat:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) },
                nudYeniFiyat,
                btnKaydet,
                btnVarsayilan
            });
            tekilPanel.Controls.Add(tekilLayout);

            sagPanel.Controls.Add(tekilPanel);
            sagPanel.Controls.Add(dgvFiyatlar);

            Controls.Add(sagPanel);
            Controls.Add(dgvUrunler);
            Controls.Add(filtrePanel);

            aramaTimer = new Timer { Interval = 300 };
            aramaTimer.Tick += (s, e) =>
            {
                aramaTimer.Stop();
                if (txtArama.Text.Length >= 2 || string.IsNullOrWhiteSpace(txtArama.Text))
                {
                    try
                    {
                        sayfa = 1;
                        UrunleriYukle();
                    }
                    catch (Exception ex)
                    {
                        // S7-FIX: DB hatalarını kullanıcıya göster
                        MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
        }

        private void FiyatTipleriniYukle()
        {
            DataTable dt = VeriKatmani.FiyatTipleriniGetir();
            cmbFiyatTipi.DataSource = dt;
            cmbFiyatTipi.DisplayMember = "sFiyatTipi";
            cmbFiyatTipi.ValueMember = "sFiyatTipi";

            cmbTekilFiyatTipi.DataSource = dt.Copy();
            cmbTekilFiyatTipi.DisplayMember = "sFiyatTipi";
            cmbTekilFiyatTipi.ValueMember = "sFiyatTipi";

            if (!string.IsNullOrWhiteSpace(Ayarlar.VarsayilanFiyatTipi))
            {
                cmbFiyatTipi.SelectedValue = Ayarlar.VarsayilanFiyatTipi;
                cmbTekilFiyatTipi.SelectedValue = Ayarlar.VarsayilanFiyatTipi;
            }
        }

        private void UrunleriYukle()
        {
            BeklemedeCalistir(() =>
            {
                string arama = txtArama.Text;
                string fiyatTipi = cmbFiyatTipi.SelectedValue?.ToString();
                DataTable dt = VeriKatmani.UrunleriFiyatYonetimiIcinGetir(arama, fiyatTipi, sayfa, 50);
                dgvUrunler.DataSource = dt;
                lblSayfa.Text = $"Sayfa: {sayfa}";
            });
        }

        private void DgvUrunler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count == 0)
            {
                seciliStokId = null;
                dgvFiyatlar.DataSource = null;
                return;
            }

            seciliStokId = Convert.ToInt32(dgvUrunler.SelectedRows[0].Cells["ID"].Value);
            FiyatlariYukle();
        }

        private void FiyatlariYukle()
        {
            if (!seciliStokId.HasValue)
            {
                return;
            }

            DataTable dt = VeriKatmani.UrunFiyatlariniGetir(seciliStokId.Value);
            dgvFiyatlar.DataSource = dt;
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            try
            {
                if (!seciliStokId.HasValue)
                {
                    MessageBox.Show("Lütfen ürün seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string fiyatTipi = cmbTekilFiyatTipi.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(fiyatTipi))
                {
                    MessageBox.Show("Fiyat tipi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal yeniFiyat = nudYeniFiyat.Value;
                if (yeniFiyat < 0)
                {
                    MessageBox.Show("Yeni fiyat 0'dan küçük olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                BeklemedeCalistir(() =>
                {
                    VeriKatmani.FiyatGuncelle(seciliStokId.Value, fiyatTipi, yeniFiyat);
                });

                FiyatlariYukle();
                UrunleriYukle();
            }
            catch (Exception ex)
            {
                // S7-FIX: DB hatalarını kullanıcıya göster
                MessageBox.Show($"İşlem başarısız: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void BeklemedeCalistir(Action islem)
        {
            try
            {
                UseWaitCursor = true;
                Enabled = false;
                islem();
            }
            finally
            {
                Enabled = true;
                UseWaitCursor = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F2:
                    txtArama.Focus();
                    return true;
                case Keys.F5:
                    sayfa = 1;
                    UrunleriYukle();
                    return true;
                case Keys.Enter:
                    BtnKaydet_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Escape:
                    Close();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
