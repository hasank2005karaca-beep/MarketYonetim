using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace MarketYonetim
{
    public static class Yardimcilar
    {
        public static void DoubleBufferedAktifEt(DataGridView grid)
        {
            PropertyInfo prop = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(grid, true, null);
        }

        public static bool DecimalCoz(string metin, out decimal sonuc)
        {
            return decimal.TryParse(metin, NumberStyles.Any, CultureInfo.CurrentCulture, out sonuc);
        }
    }
}
