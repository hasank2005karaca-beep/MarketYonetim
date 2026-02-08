using System;
using System.Windows.Forms;

namespace MarketYonetim
{
    public static class DiscoveryKilit
    {
        public static bool Calistir()
        {
            if (!Ayarlar.AyarDosyasiVarMi())
            {
                MessageBox.Show(
                    "Ayar dosyası bulunamadı. Lütfen bağlantı ayarlarını yapılandırın.",
                    "Ayar Eksik",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            if (!Ayarlar.BaglantiTest())
            {
                MessageBox.Show(
                    "Veritabanı bağlantısı doğrulanamadı. Lütfen bağlantı ayarlarını kontrol edin.",
                    "Bağlantı Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            VeriKatmani.DiscoveryLogla();

            var kontrol = VeriKatmani.KritikSemaKontroluYap();
            if (!kontrol.Ok)
            {
                MessageBox.Show(
                    $"Nebim şeması beklenenle uyuşmuyor.\nDiscovery logunu paylaşın: discovery_log.txt\n\nDetaylar:\n{kontrol.Mesaj}",
                    "Discovery Kilidi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
