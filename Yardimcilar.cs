using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MarketYonetim
{
    public static class Yardimcilar
    {
        public static decimal YuvarlaKurus(decimal tutar)
        {
            return Math.Round(tutar, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal KdvMatrahHesapla(decimal kdvDahilTutar, decimal kdvOrani)
        {
            if (kdvOrani <= 0)
            {
                return YuvarlaKurus(kdvDahilTutar);
            }

            decimal oran = kdvOrani / 100m;
            decimal matrah = kdvDahilTutar / (1 + oran);
            return YuvarlaKurus(matrah);
        }

        public static decimal KdvTutarHesapla(decimal kdvDahilTutar, decimal kdvOrani)
        {
            decimal matrah = KdvMatrahHesapla(kdvDahilTutar, kdvOrani);
            return YuvarlaKurus(kdvDahilTutar - matrah);
        }

        public static Dictionary<decimal, (decimal Matrah, decimal Kdv)> KdvDagit(IEnumerable<(decimal Tutar, decimal KdvOrani)> satirlar)
        {
            Dictionary<decimal, (decimal Matrah, decimal Kdv)> sonuc = new Dictionary<decimal, (decimal Matrah, decimal Kdv)>();

            foreach (var satir in satirlar)
            {
                decimal matrah = KdvMatrahHesapla(satir.Tutar, satir.KdvOrani);
                decimal kdv = YuvarlaKurus(satir.Tutar - matrah);

                if (!sonuc.ContainsKey(satir.KdvOrani))
                {
                    sonuc[satir.KdvOrani] = (0m, 0m);
                }

                var mevcut = sonuc[satir.KdvOrani];
                sonuc[satir.KdvOrani] = (YuvarlaKurus(mevcut.Matrah + matrah), YuvarlaKurus(mevcut.Kdv + kdv));
            }

            return sonuc;
        }

        public static string ParaFormatla(decimal tutar)
        {
            return string.Format(CultureInfo.GetCultureInfo("tr-TR"), "{0:N2} ₺", tutar);
        }

        public static string TarihFormatla(DateTime tarih)
        {
            return tarih.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"));
        }

        public static void DoubleBufferedAktifEt(Control kontrol)
        {
            if (kontrol == null)
            {
                return;
            }

            PropertyInfo prop = kontrol.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(kontrol, true, null);
        }

        public static void DataTableToCsv(DataTable dt, string filePath, string delimiter = ";")
        {
            if (dt == null)
            {
                throw new ArgumentNullException(nameof(dt));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));
            }

            CultureInfo culture = CultureInfo.GetCultureInfo("tr-TR");
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(delimiter);
                }
                sb.Append(CsvHucre(dt.Columns[i].ColumnName, delimiter));
            }
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(delimiter);
                    }
                    object value = row[i];
                    string metin = FormatDeger(value, culture);
                    sb.Append(CsvHucre(metin, delimiter));
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string FormatDeger(object value, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (value is DateTime tarih)
            {
                return tarih.ToString("dd.MM.yyyy HH:mm", culture);
            }

            if (value is decimal || value is double || value is float)
            {
                return Convert.ToDecimal(value).ToString("N2", culture);
            }

            return Convert.ToString(value, culture) ?? string.Empty;
        }

        private static string CsvHucre(string deger, string delimiter)
        {
            if (string.IsNullOrEmpty(deger))
            {
                return string.Empty;
            }

            bool quote = deger.Contains(delimiter) || deger.Contains("\n") || deger.Contains("\r") || deger.Contains("\"");
            if (quote)
            {
                deger = deger.Replace("\"", "\"\"");
                return $"\"{deger}\"";
            }

            return deger;
        }
    }
}
