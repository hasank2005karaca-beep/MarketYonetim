using System;
using System.Windows.Forms;

namespace MarketYonetim
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!Ayarlar.AyarDosyasiVarMi())
            {
                FormAyarlar ayarlarForm = new FormAyarlar();
                if (ayarlarForm.ShowDialog() != DialogResult.OK)
                {
                    return; // Kullanıcı iptal ettiyse çık
                }
            }

            // Ayarları yükle
            Ayarlar.YukleAyarlar();

            // Veritabanı bağlantısını test et
            if (!Ayarlar.BaglantiTest())
            {
                MessageBox.Show(
                    "Veritabanına bağlanılamadı!\nLütfen bağlantı ayarlarını yapılandırın.",
                    "Bağlantı Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                FormAyarlar ayarlarForm = new FormAyarlar();
                if (ayarlarForm.ShowDialog() != DialogResult.OK)
                {
                    return; // Kullanıcı iptal ettiyse çık
                }

                if (!Ayarlar.BaglantiTest())
                {
                    MessageBox.Show(
                        "Veritabanına hala bağlanılamıyor. Program kapatılıyor.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            VeriKatmani.DiscoveryLogla();
            
            // Ana formu başlat
            Application.Run(new FormSatis());
        }
    }
}
