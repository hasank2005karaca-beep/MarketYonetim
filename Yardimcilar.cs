using System;
using System.Collections.Generic;
using System.Data;
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

            decimal bolen = 1 + (kdvOrani / 100m);
            return YuvarlaKurus(kdvDahilTutar / bolen);
        }

        public static decimal KdvTutarHesapla(decimal kdvDahilTutar, decimal kdvOrani)
        {
            decimal matrah = KdvMatrahHesapla(kdvDahilTutar, kdvOrani);
            return YuvarlaKurus(kdvDahilTutar - matrah);
        }

        public static Dictionary<decimal, KdvDagitimSonuc> KdvDagit(DataTable sepet)
        {
            var sonuc = new Dictionary<decimal, KdvDagitimSonuc>();
            if (sepet == null)
            {
                return sonuc;
            }

            foreach (DataRow row in sepet.Rows)
            {
                if (!decimal.TryParse(row["nKdvOrani"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var oran))
                {
                    oran = Ayarlar.VarsayilanKdvOrani;
                }

                decimal satirNet = Convert.ToDecimal(row["lSatirToplam"]);
                decimal matrah = KdvMatrahHesapla(satirNet, oran);
                decimal kdv = KdvTutarHesapla(satirNet, oran);

                if (!sonuc.ContainsKey(oran))
                {
                    sonuc[oran] = new KdvDagitimSonuc();
                }

                sonuc[oran].Matrah += matrah;
                sonuc[oran].Kdv += kdv;
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

    public class KdvDagitimSonuc
    {
        public decimal Matrah { get; set; }
        public decimal Kdv { get; set; }
    }
}
