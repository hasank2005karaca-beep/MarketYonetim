using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace MarketYonetim
{
    public static class VeriKatmani
    {
        private const string DefaultFiyatTipi = "1";

        public static DataTable BarkodIleUrunBul(string barkod)
        {
            const string sql = @"
                SELECT TOP 1
                    s.nStokID,
                    s.sKodu,
                    s.sAciklama,
                    s.sBirimCinsi,
                    b.sBarkod,
                    ISNULL(f.lFiyat, 0) AS lFiyat,
                    ISNULL(s.nKdvOrani, @defaultKdv) AS nKdvOrani
                FROM tbStok s
                INNER JOIN tbStokBarkodu b ON s.nStokID = b.nStokID
                LEFT JOIN tbStokFiyati f ON s.nStokID = f.nStokID AND f.sFiyatTipi = @fiyatTipi
                WHERE b.sBarkod = @barkod";

            return GetDataTable(sql,
                new SqlParameter("@barkod", barkod),
                new SqlParameter("@fiyatTipi", DefaultFiyatTipi),
                new SqlParameter("@defaultKdv", Ayarlar.VarsayilanKdvOrani));
        }

        public static DataTable UrunAra(string arama, int sayfa = 0, int sayfaBoyutu = 50)
        {
            const string sql = @"
                SELECT
                    s.nStokID,
                    s.sKodu,
                    s.sAciklama,
                    s.sBirimCinsi,
                    ISNULL(f.lFiyat, 0) AS lFiyat,
                    ISNULL(s.nKdvOrani, @defaultKdv) AS nKdvOrani
                FROM tbStok s
                LEFT JOIN tbStokFiyati f ON s.nStokID = f.nStokID AND f.sFiyatTipi = @fiyatTipi
                WHERE s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%'
                ORDER BY s.sAciklama
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";

            int skip = sayfa * sayfaBoyutu;
            return GetDataTable(sql,
                new SqlParameter("@arama", arama),
                new SqlParameter("@skip", skip),
                new SqlParameter("@take", sayfaBoyutu),
                new SqlParameter("@fiyatTipi", DefaultFiyatTipi),
                new SqlParameter("@defaultKdv", Ayarlar.VarsayilanKdvOrani));
        }

        public static decimal StokMiktariGetir(int stokId)
        {
            const string sql = @"
                SELECT
                    ISNULL(SUM(ISNULL(lGirisMiktar1, 0)), 0) - ISNULL(SUM(ISNULL(lCikisMiktar1, 0)), 0)
                FROM tbStokFisiDetayi
                WHERE nStokID = @id";

            using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", stokId);
                conn.Open();
                object sonuc = cmd.ExecuteScalar();
                return sonuc == DBNull.Value ? 0m : Convert.ToDecimal(sonuc, CultureInfo.InvariantCulture);
            }
        }

        public static int SatisKaydet(DataTable sepet, int musteriId, string odemeTipi,
            decimal nakit, decimal krediKarti, decimal dipIskontoYuzde)
        {
            if (sepet == null || sepet.Rows.Count == 0)
            {
                throw new InvalidOperationException("Sepet boş.");
            }

            return TransactionCalistir((conn, tran) =>
            {
                int alisverisId = YeniIdUret(conn, tran, "tbAlisVeris", "nAlisverisID");
                int stokFisiId = YeniIdUret(conn, tran, "tbStokFisiMaster", "nStokFisiID");

                string fisTipi = odemeTipi == "Veresiye" ? "KR" : "PS";

                var detaylar = SepetDetaylariOlustur(sepet, dipIskontoYuzde);

                decimal brutToplam = detaylar.Sum(x => x.BrutTutar);
                decimal netToplam = detaylar.Sum(x => x.NetTutar);
                decimal malIskontoToplam = detaylar.Sum(x => x.SatirIskonto);
                decimal dipIskontoTutar = detaylar.Sum(x => x.DipIskontoPayi);

                var kdvDagitim = DagitimSlotlariOlustur(detaylar);

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO tbAlisVeris (
                        nAlisverisID, sFisTipi, nGirisCikis, nMusteriID,
                        dteFaturaTarihi, dteKayitTarihi,
                        lMalBedeli, lNetTutar,
                        lKdv1, lKdv2, lKdv3, lKdv4, lKdv5,
                        lKdvMatrahi1, lKdvMatrahi2, lKdvMatrahi3, lKdvMatrahi4, lKdvMatrahi5,
                        nKdvOrani1, nKdvOrani2, nKdvOrani3, nKdvOrani4, nKdvOrani5,
                        lMalIskontoTutari, lDipIskontoTutari,
                        sKasiyerRumuzu, sMagaza
                    ) VALUES (
                        @id, @fisTipi, 2, @musteriId,
                        @faturaTarihi, @kayitTarihi,
                        @malBedeli, @netTutar,
                        @kdv1, @kdv2, @kdv3, @kdv4, @kdv5,
                        @kdvMatrah1, @kdvMatrah2, @kdvMatrah3, @kdvMatrah4, @kdvMatrah5,
                        @kdvOrani1, @kdvOrani2, @kdvOrani3, @kdvOrani4, @kdvOrani5,
                        @malIskonto, @dipIskonto,
                        @kasiyer, @magaza
                    )", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@id", alisverisId);
                    cmd.Parameters.AddWithValue("@fisTipi", fisTipi);
                    cmd.Parameters.AddWithValue("@musteriId", musteriId == 0 ? 0 : musteriId);
                    cmd.Parameters.AddWithValue("@faturaTarihi", DateTime.Now);
                    cmd.Parameters.AddWithValue("@kayitTarihi", DateTime.Now);
                    cmd.Parameters.AddWithValue("@malBedeli", brutToplam);
                    cmd.Parameters.AddWithValue("@netTutar", netToplam);
                    cmd.Parameters.AddWithValue("@kdv1", kdvDagitim[0].Kdv);
                    cmd.Parameters.AddWithValue("@kdv2", kdvDagitim[1].Kdv);
                    cmd.Parameters.AddWithValue("@kdv3", kdvDagitim[2].Kdv);
                    cmd.Parameters.AddWithValue("@kdv4", kdvDagitim[3].Kdv);
                    cmd.Parameters.AddWithValue("@kdv5", kdvDagitim[4].Kdv);
                    cmd.Parameters.AddWithValue("@kdvMatrah1", kdvDagitim[0].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah2", kdvDagitim[1].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah3", kdvDagitim[2].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah4", kdvDagitim[3].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah5", kdvDagitim[4].Matrah);
                    cmd.Parameters.AddWithValue("@kdvOrani1", kdvDagitim[0].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani2", kdvDagitim[1].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani3", kdvDagitim[2].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani4", kdvDagitim[3].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani5", kdvDagitim[4].Oran);
                    cmd.Parameters.AddWithValue("@malIskonto", malIskontoToplam);
                    cmd.Parameters.AddWithValue("@dipIskonto", dipIskontoTutar);
                    cmd.Parameters.AddWithValue("@kasiyer", Ayarlar.KasiyerRumuzu);
                    cmd.Parameters.AddWithValue("@magaza", Ayarlar.DepoKodu);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO tbStokFisiMaster (
                        nStokFisiID, sFisTipi, nGirisCikis, sDepo, nFirmaID,
                        dteFisTarihi, lMalBedeli, lNetTutar,
                        lKdv1, lKdv2, lKdv3, lKdv4, lKdv5,
                        lKdvMatrahi1, lKdvMatrahi2, lKdvMatrahi3, lKdvMatrahi4, lKdvMatrahi5,
                        nKdvOrani1, nKdvOrani2, nKdvOrani3, nKdvOrani4, nKdvOrani5
                    ) VALUES (
                        @id, @fisTipi, 2, @depo, 1,
                        @tarih, @malBedeli, @netTutar,
                        @kdv1, @kdv2, @kdv3, @kdv4, @kdv5,
                        @kdvMatrah1, @kdvMatrah2, @kdvMatrah3, @kdvMatrah4, @kdvMatrah5,
                        @kdvOrani1, @kdvOrani2, @kdvOrani3, @kdvOrani4, @kdvOrani5
                    )", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@id", stokFisiId);
                    cmd.Parameters.AddWithValue("@fisTipi", fisTipi);
                    cmd.Parameters.AddWithValue("@depo", Ayarlar.DepoKodu);
                    cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                    cmd.Parameters.AddWithValue("@malBedeli", brutToplam);
                    cmd.Parameters.AddWithValue("@netTutar", netToplam);
                    cmd.Parameters.AddWithValue("@kdv1", kdvDagitim[0].Kdv);
                    cmd.Parameters.AddWithValue("@kdv2", kdvDagitim[1].Kdv);
                    cmd.Parameters.AddWithValue("@kdv3", kdvDagitim[2].Kdv);
                    cmd.Parameters.AddWithValue("@kdv4", kdvDagitim[3].Kdv);
                    cmd.Parameters.AddWithValue("@kdv5", kdvDagitim[4].Kdv);
                    cmd.Parameters.AddWithValue("@kdvMatrah1", kdvDagitim[0].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah2", kdvDagitim[1].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah3", kdvDagitim[2].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah4", kdvDagitim[3].Matrah);
                    cmd.Parameters.AddWithValue("@kdvMatrah5", kdvDagitim[4].Matrah);
                    cmd.Parameters.AddWithValue("@kdvOrani1", kdvDagitim[0].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani2", kdvDagitim[1].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani3", kdvDagitim[2].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani4", kdvDagitim[3].Oran);
                    cmd.Parameters.AddWithValue("@kdvOrani5", kdvDagitim[4].Oran);
                    cmd.ExecuteNonQuery();
                }

                decimal detayToplam = detaylar.Sum(x => x.NetTutar);
                decimal fark = Yardimcilar.YuvarlaKurus(netToplam - detayToplam);
                if (Math.Abs(fark) > 0.01m)
                {
                    var duzeltilecek = detaylar.OrderByDescending(x => x.NetTutar).First();
                    duzeltilecek.NetTutar += fark;
                }

                foreach (var detay in detaylar)
                {
                    int islemId = YeniIdUret(conn, tran, "tbStokFisiDetayi", "nIslemID");

                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO tbStokFisiDetayi (
                            nIslemID, nStokFisiID, nStokID, nGirisCikis,
                            lCikisMiktar1, lCikisFiyat, lBrutTutar, lCikisTutar,
                            nKdvOrani, lIskontoTutari, nIskontoYuzdesi,
                            nAlisverisID, sDepo, dteFisTarihi, sKasiyerRumuzu, sFisTipi
                        ) VALUES (
                            @islemId, @stokFisiId, @stokId, 2,
                            @miktar, @birimFiyat, @brutTutar, @cikisTutar,
                            @kdvOrani, @iskontoTutari, @iskontoYuzde,
                            @alisverisId, @depo, @tarih, @kasiyer, @fisTipi
                        )", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@islemId", islemId);
                        cmd.Parameters.AddWithValue("@stokFisiId", stokFisiId);
                        cmd.Parameters.AddWithValue("@stokId", detay.StokId);
                        cmd.Parameters.AddWithValue("@miktar", detay.Miktar);
                        cmd.Parameters.AddWithValue("@birimFiyat", detay.BirimFiyat);
                        cmd.Parameters.AddWithValue("@brutTutar", detay.BrutTutar);
                        cmd.Parameters.AddWithValue("@cikisTutar", detay.NetTutar);
                        cmd.Parameters.AddWithValue("@kdvOrani", detay.KdvOrani);
                        cmd.Parameters.AddWithValue("@iskontoTutari", detay.SatirIskonto);
                        cmd.Parameters.AddWithValue("@iskontoYuzde", detay.IskontoYuzde);
                        cmd.Parameters.AddWithValue("@alisverisId", alisverisId);
                        cmd.Parameters.AddWithValue("@depo", Ayarlar.DepoKodu);
                        cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                        cmd.Parameters.AddWithValue("@kasiyer", Ayarlar.KasiyerRumuzu);
                        cmd.Parameters.AddWithValue("@fisTipi", fisTipi);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (nakit > 0)
                {
                    KaydetOdeme(conn, tran, alisverisId, "N", nakit);
                    KaydetNakitKasa(conn, tran, alisverisId, nakit);
                }

                if (krediKarti > 0)
                {
                    KaydetOdeme(conn, tran, alisverisId, "KK", krediKarti);
                }

                return alisverisId;
            });
        }

        public static DataTable GunSatisOzeti()
        {
            const string sql = @"
                SELECT
                    ISNULL(SUM(lNetTutar), 0) AS ToplamTutar,
                    ISNULL(COUNT(*), 0) AS SatisAdedi
                FROM tbAlisVeris
                WHERE nGirisCikis = 2 AND dteFaturaTarihi >= @tarih";

            return GetDataTable(sql, new SqlParameter("@tarih", DateTime.Today));
        }

        private static List<SepetDetay> SepetDetaylariOlustur(DataTable sepet, decimal dipIskontoYuzde)
        {
            var detaylar = new List<SepetDetay>();
            decimal toplamNet = 0m;

            foreach (DataRow row in sepet.Rows)
            {
                decimal miktar = Convert.ToDecimal(row["lMiktar"]);
                decimal birimFiyat = Convert.ToDecimal(row["lBirimFiyat"]);
                decimal iskontoYuzde = Convert.ToDecimal(row["nIskontoYuzde"]);
                decimal kdvOrani = Convert.ToDecimal(row["nKdvOrani"]);

                decimal brut = Yardimcilar.YuvarlaKurus(miktar * birimFiyat);
                decimal satirIskonto = Yardimcilar.YuvarlaKurus(brut * (iskontoYuzde / 100m));
                decimal net = Yardimcilar.YuvarlaKurus(brut - satirIskonto);

                var detay = new SepetDetay
                {
                    StokId = Convert.ToInt32(row["nStokID"]),
                    Miktar = miktar,
                    BirimFiyat = birimFiyat,
                    IskontoYuzde = iskontoYuzde,
                    KdvOrani = kdvOrani,
                    BrutTutar = brut,
                    SatirIskonto = satirIskonto,
                    NetTutar = net
                };

                detaylar.Add(detay);
                toplamNet += net;
            }

            if (toplamNet > 0 && dipIskontoYuzde > 0)
            {
                decimal dipIskontoTutar = Yardimcilar.YuvarlaKurus(toplamNet * (dipIskontoYuzde / 100m));
                foreach (var detay in detaylar)
                {
                    decimal pay = detay.NetTutar / toplamNet;
                    detay.DipIskontoPayi = Yardimcilar.YuvarlaKurus(dipIskontoTutar * pay);
                    detay.NetTutar = Yardimcilar.YuvarlaKurus(detay.NetTutar - detay.DipIskontoPayi);
                }
            }

            return detaylar;
        }

        private static List<KdvSlot> DagitimSlotlariOlustur(List<SepetDetay> detaylar)
        {
            var dagitim = new Dictionary<decimal, KdvDagitimSonuc>();
            foreach (var detay in detaylar)
            {
                decimal matrah = Yardimcilar.KdvMatrahHesapla(detay.NetTutar, detay.KdvOrani);
                decimal kdv = Yardimcilar.KdvTutarHesapla(detay.NetTutar, detay.KdvOrani);

                if (!dagitim.ContainsKey(detay.KdvOrani))
                {
                    dagitim[detay.KdvOrani] = new KdvDagitimSonuc();
                }

                dagitim[detay.KdvOrani].Matrah += matrah;
                dagitim[detay.KdvOrani].Kdv += kdv;
            }

            var slotlar = dagitim
                .OrderBy(x => x.Key)
                .Take(5)
                .Select(x => new KdvSlot { Oran = x.Key, Matrah = x.Value.Matrah, Kdv = x.Value.Kdv })
                .ToList();

            while (slotlar.Count < 5)
            {
                slotlar.Add(new KdvSlot());
            }

            return slotlar;
        }

        private static void KaydetOdeme(SqlConnection conn, SqlTransaction tran, int alisverisId, string odemeTipi, decimal tutar)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tbOdeme (nAlisverisID, sOdemeSekli, lTutar, dteKayitTarihi)
                VALUES (@alisverisId, @odemeTipi, @tutar, @tarih)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@alisverisId", alisverisId);
                cmd.Parameters.AddWithValue("@odemeTipi", odemeTipi);
                cmd.Parameters.AddWithValue("@tutar", tutar);
                cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        private static void KaydetNakitKasa(SqlConnection conn, SqlTransaction tran, int alisverisId, decimal tutar)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tbNakitKasa (nHesapID, nFirmaID, dteIslemTarihi, sHangiUygulama, lTutar, nAlisverisID)
                VALUES (1, 1, @tarih, 'POS', @tutar, @alisverisId)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                cmd.Parameters.AddWithValue("@tutar", tutar);
                cmd.Parameters.AddWithValue("@alisverisId", alisverisId);
                cmd.ExecuteNonQuery();
            }
        }

        private static DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        private static int YeniIdUret(SqlConnection conn, SqlTransaction tran, string tablo, string alan)
        {
            string sql = $@"
                SELECT ISNULL(MAX({alan}), 0) + 1
                FROM {tablo} WITH (UPDLOCK, HOLDLOCK)";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                object sonuc = cmd.ExecuteScalar();
                return Convert.ToInt32(sonuc, CultureInfo.InvariantCulture);
            }
        }

        private static int TransactionCalistir(Func<SqlConnection, SqlTransaction, int> action)
        {
            using (SqlConnection conn = new SqlConnection(Ayarlar.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int sonuc = action(conn, tran);
                        tran.Commit();
                        return sonuc;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private class SepetDetay
        {
            public int StokId { get; set; }
            public decimal Miktar { get; set; }
            public decimal BirimFiyat { get; set; }
            public decimal IskontoYuzde { get; set; }
            public decimal KdvOrani { get; set; }
            public decimal BrutTutar { get; set; }
            public decimal SatirIskonto { get; set; }
            public decimal DipIskontoPayi { get; set; }
            public decimal NetTutar { get; set; }
        }

        private class KdvSlot
        {
            public decimal Oran { get; set; }
            public decimal Matrah { get; set; }
            public decimal Kdv { get; set; }
        }
    }
}
