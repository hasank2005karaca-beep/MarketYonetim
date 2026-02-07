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
            
            // Ayarları yükle
            Ayarlar.YukleAyarlar();
            
            // Veritabanı bağlantısını test et
            if (!Ayarlar.BaglantiTest())
            {
                // Bağlantı yoksa ayarlar formunu göster
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
                
                // Tekrar test et
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
            
            // Ana formu başlat
            Application.Run(new FormSatis());
        }
    }
}
