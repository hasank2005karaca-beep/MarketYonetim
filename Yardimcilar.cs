using System;
using System.Collections.Generic;
using System.Globalization;

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
    }
}
