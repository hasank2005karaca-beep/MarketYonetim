using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MarketYonetim
{
    /// <summary>
    /// MarketYonetim — Merkezi Veri Katmanı
    ///
    /// DISCOVERY SONUÇLARI (DB erişimi yoksa '?' olarak bırakıldı):
    /// | Tablo | PK Sütunu | IDENTITY mi? | NOT NULL zorunlu alanlar | Satış fiş tipi | Örnek kayıt notları |
    /// |-------|-----------|-------------|------------------------|----------------|---------------------|
    /// | tbAlisVeris | nAlisverisID | ? | ? | ? | ? |
    /// | tbStokFisiMaster | nStokFisiID | ? | ? | ? | ? |
    /// | tbStokFisiDetayi | nIslemID | ? | ? | ? | ? |
    /// | tbOdeme | ? | ? | ? | ? | ? |
    /// | tbStok | nStokID | ? | ? | - | ? |
    /// | tbMusteri | nMusteriID | ? | ? | - | ? |
    /// </summary>
    public static class VeriKatmani
    {
        private const string DiscoveryTabloYapisiSql = @"
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
    'tbStok','tbStokBarkodu','tbStokFiyati','tbStokSinifi','tbStokTipi',
    'tbStokFisiMaster','tbStokFisiDetayi',
    'tbAlisVeris','tbAlisverisSiparis','tbOdeme',
    'tbMusteri','tbMusteriKarti',
    'tbNakitKasa','tbDepo','tbCariIslem',
    'tbFiyatTipi','tbOdemeSekli','tbAVSiraNo','tbAVReyonFisi'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;";

        private const string DiscoveryIdentitySql = @"
SELECT t.name AS TableName, c.name AS ColumnName, c.is_identity
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name IN (
    'tbStok','tbStokBarkodu','tbStokFiyati','tbStokFisiMaster','tbStokFisiDetayi',
    'tbAlisVeris','tbAlisverisSiparis','tbOdeme','tbMusteri','tbNakitKasa','tbCariIslem'
)
AND c.is_identity = 1;";

        private const string DiscoveryNumaratorSql = @"
SELECT * FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%SiraNo%' OR TABLE_NAME LIKE '%Numarat%' OR TABLE_NAME LIKE '%Counter%';";

        private const string DiscoveryOrnekSql = @"
SELECT TOP 3 * FROM tbAlisVeris ORDER BY nAlisverisID DESC;
SELECT TOP 3 * FROM tbStokFisiMaster ORDER BY nStokFisiID DESC;
SELECT TOP 3 * FROM tbStokFisiDetayi ORDER BY nIslemID DESC;
SELECT TOP 3 * FROM tbOdeme ORDER BY 1 DESC;";

        private const string DiscoveryFisTipiSql = @"
SELECT DISTINCT sFisTipi, nGirisCikis, COUNT(*) AS Adet
FROM tbStokFisiMaster GROUP BY sFisTipi, nGirisCikis ORDER BY Adet DESC;

SELECT DISTINCT sFisTipi, COUNT(*) AS Adet
FROM tbAlisVeris GROUP BY sFisTipi ORDER BY Adet DESC;";

        private const string DiscoveryDepoSql = @"
SELECT sDepo, nFirmaID FROM tbDepo;";

        private const string DiscoveryIndexSql = @"
SELECT i.name AS IndexName, t.name AS TableName, c.name AS ColumnName
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('tbStok','tbStokBarkodu','tbStokFiyati','tbStokFisiDetayi','tbAlisVeris')
ORDER BY t.name, i.name;";

        public static SqlConnection BaglantiOlustur()
        {
            return new SqlConnection(Ayarlar.ConnectionString);
        }

        public static DataTable SorguCalistirDataTable(string sql, params SqlParameter[] parametreler)
        {
            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parametreler != null && parametreler.Length > 0)
                {
                    cmd.Parameters.AddRange(parametreler);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public static object SorguCalistirScalar(string sql, params SqlParameter[] parametreler)
        {
            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parametreler != null && parametreler.Length > 0)
                {
                    cmd.Parameters.AddRange(parametreler);
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static int SorguCalistirNonQuery(string sql, params SqlParameter[] parametreler)
        {
            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parametreler != null && parametreler.Length > 0)
                {
                    cmd.Parameters.AddRange(parametreler);
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static T TransactionCalistir<T>(Func<SqlConnection, SqlTransaction, T> islem)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        T sonuc = islem(conn, tran);
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

        public static void TransactionCalistir(Action<SqlConnection, SqlTransaction> islem)
        {
            TransactionCalistir((conn, tran) =>
            {
                islem(conn, tran);
                return 0;
            });
        }

        public static int YeniIdUret(SqlConnection conn, SqlTransaction tran, string tablo, string pkKolon)
        {
            string sql = $"SELECT ISNULL(MAX({pkKolon}), 0) + 1 FROM {tablo} WITH (UPDLOCK, HOLDLOCK)";
            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                object sonuc = cmd.ExecuteScalar();
                return Convert.ToInt32(sonuc);
            }
        }

        public static DataTable DiscoveryTabloYapisi()
        {
            return SorguCalistirDataTable(DiscoveryTabloYapisiSql);
        }

        public static DataTable DiscoveryIdentityKontrol()
        {
            return SorguCalistirDataTable(DiscoveryIdentitySql);
        }

        public static DataTable DiscoveryNumaratorKontrol()
        {
            return SorguCalistirDataTable(DiscoveryNumaratorSql);
        }

        public static DataSet DiscoveryOrnekKayitlar()
        {
            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(DiscoveryOrnekSql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public static DataSet DiscoveryFisTipleri()
        {
            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(DiscoveryFisTipiSql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public static DataTable DiscoveryDepolar()
        {
            return SorguCalistirDataTable(DiscoveryDepoSql);
        }

        public static DataTable DiscoveryIndexler()
        {
            return SorguCalistirDataTable(DiscoveryIndexSql);
        }

        public static void DiscoveryLogla()
        {
            try
            {
                DataTable tabloYapisi = DiscoveryTabloYapisi();
                DataTable identity = DiscoveryIdentityKontrol();
                DataTable numarator = DiscoveryNumaratorKontrol();
                DataSet ornekler = DiscoveryOrnekKayitlar();
                DataSet fisTipleri = DiscoveryFisTipleri();
                DataTable depolar = DiscoveryDepolar();
                DataTable indexler = DiscoveryIndexler();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== DISCOVERY TABLO YAPISI ===");
                sb.AppendLine($"Satır: {tabloYapisi.Rows.Count}");
                sb.AppendLine("=== DISCOVERY IDENTITY ===");
                sb.AppendLine($"Satır: {identity.Rows.Count}");
                sb.AppendLine("=== DISCOVERY NUMARATOR ===");
                sb.AppendLine($"Satır: {numarator.Rows.Count}");
                sb.AppendLine("=== DISCOVERY ÖRNEK KAYITLAR ===");
                sb.AppendLine($"Set: {ornekler.Tables.Count}");
                sb.AppendLine("=== DISCOVERY FİŞ TİPLERİ ===");
                sb.AppendLine($"Set: {fisTipleri.Tables.Count}");
                sb.AppendLine("=== DISCOVERY DEPOLAR ===");
                sb.AppendLine($"Satır: {depolar.Rows.Count}");
                sb.AppendLine("=== DISCOVERY INDEXLER ===");
                sb.AppendLine($"Satır: {indexler.Rows.Count}");

                Console.WriteLine(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discovery çalıştırılamadı: {ex.Message}");
            }
        }

        private static HashSet<string> TabloKolonlariniGetir(SqlConnection conn, SqlTransaction tran, string tablo)
        {
            HashSet<string> kolonlar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string sql = @"SELECT c.name
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = @tablo";
            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@tablo", tablo);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        kolonlar.Add(reader.GetString(0));
                    }
                }
            }
            return kolonlar;
        }

        private static bool IdentityKolonVarMi(SqlConnection conn, SqlTransaction tran, string tablo, string kolon)
        {
            const string sql = @"SELECT c.is_identity
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = @tablo AND c.name = @kolon";
            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@tablo", tablo);
                cmd.Parameters.AddWithValue("@kolon", kolon);
                object sonuc = cmd.ExecuteScalar();
                if (sonuc == null || sonuc == DBNull.Value)
                {
                    return false;
                }
                return Convert.ToBoolean(sonuc);
            }
        }

        private static int DinamikInsert(SqlConnection conn, SqlTransaction tran, string tablo, Dictionary<string, object> kolonlar, bool identityGeriDonus)
        {
            HashSet<string> mevcutKolonlar = TabloKolonlariniGetir(conn, tran, tablo);
            List<string> kullanilanKolonlar = kolonlar.Keys.Where(k => mevcutKolonlar.Contains(k)).ToList();

            if (kullanilanKolonlar.Count == 0)
            {
                throw new InvalidOperationException($"{tablo} tablosunda kullanılabilir kolon bulunamadı.");
            }

            string kolonListesi = string.Join(", ", kullanilanKolonlar);
            string parametreListesi = string.Join(", ", kullanilanKolonlar.Select(k => "@" + k));
            StringBuilder sql = new StringBuilder();
            sql.Append($"INSERT INTO {tablo} ({kolonListesi}) VALUES ({parametreListesi});");
            if (identityGeriDonus)
            {
                sql.Append("SELECT CAST(SCOPE_IDENTITY() AS int);");
            }

            using (SqlCommand cmd = new SqlCommand(sql.ToString(), conn, tran))
            {
                foreach (string kolon in kullanilanKolonlar)
                {
                    cmd.Parameters.AddWithValue("@" + kolon, kolonlar[kolon] ?? DBNull.Value);
                }

                if (identityGeriDonus)
                {
                    object sonuc = cmd.ExecuteScalar();
                    return sonuc == null || sonuc == DBNull.Value ? 0 : Convert.ToInt32(sonuc);
                }

                cmd.ExecuteNonQuery();
                return 0;
            }
        }

        public static DataTable DepolariGetir()
        {
            try
            {
                return SorguCalistirDataTable("SELECT sDepo, nFirmaID FROM tbDepo ORDER BY sDepo");
            }
            catch
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("sDepo", typeof(string));
                dt.Columns.Add("nFirmaID", typeof(int));
                dt.Rows.Add(Ayarlar.DepoKodu ?? string.Empty, 1);
                return dt;
            }
        }

        public static DataTable StokDurumuGetir(string arama, string barkod, string depo, string stokDurum, int kritikEsik, int sayfa, int sayfaBoyutu = 50)
        {
            int offset = Math.Max(0, sayfa - 1) * sayfaBoyutu;
            string sql = @"
WITH StokListe AS (
    SELECT s.nStokID,
           s.sKodu,
           s.sAciklama,
           s.sBirimCinsi1 AS sBirimCinsi,
           b.sBarkod,
           ISNULL(stk.StokMiktari, 0) AS StokMiktari
    FROM tbStok s
    OUTER APPLY (
        SELECT TOP 1 sBarkod FROM tbStokBarkodu b WHERE b.nStokID = s.nStokID ORDER BY b.sBarkod
    ) b
    OUTER APPLY (
        SELECT SUM(ISNULL(lGirisMiktar1, 0)) - SUM(ISNULL(lCikisMiktar1, 0)) AS StokMiktari
        FROM tbStokFisiDetayi d
        WHERE d.nStokID = s.nStokID
          AND (@depo IS NULL OR @depo = '' OR d.sDepo = @depo)
    ) stk
    WHERE (@arama IS NULL OR @arama = '' OR s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%')
      AND (@barkod IS NULL OR @barkod = '' OR b.sBarkod = @barkod)
)
SELECT *
FROM StokListe
WHERE (@stokDurum = 'hepsi'
    OR (@stokDurum = 'var' AND StokMiktari > 0)
    OR (@stokDurum = 'yok' AND StokMiktari <= 0)
    OR (@stokDurum = 'az' AND StokMiktari > 0 AND StokMiktari < @kritikEsik))
ORDER BY nStokID
OFFSET @offset ROWS FETCH NEXT @sayfaBoyutu ROWS ONLY;";

            return SorguCalistirDataTable(
                sql,
                Parametre("@arama", string.IsNullOrWhiteSpace(arama) ? null : arama),
                Parametre("@barkod", string.IsNullOrWhiteSpace(barkod) ? null : barkod),
                Parametre("@depo", string.IsNullOrWhiteSpace(depo) ? null : depo),
                Parametre("@stokDurum", (stokDurum ?? "hepsi").ToLowerInvariant()),
                Parametre("@kritikEsik", kritikEsik),
                Parametre("@offset", offset),
                Parametre("@sayfaBoyutu", sayfaBoyutu)
            );
        }

        public static DataTable StokHareketleriGetir(int stokId, string depo, DateTime? baslangic, DateTime? bitis, int sayfa, int sayfaBoyutu = 100)
        {
            int offset = Math.Max(0, sayfa - 1) * sayfaBoyutu;
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    HashSet<string> detayKolonlar = TabloKolonlariniGetir(conn, null, "tbStokFisiDetayi");
                    HashSet<string> masterKolonlar = TabloKolonlariniGetir(conn, null, "tbStokFisiMaster");

                    string tarihKolon = null;
                    if (detayKolonlar.Contains("dteFisTarihi"))
                    {
                        tarihKolon = "d.dteFisTarihi";
                    }
                    else if (detayKolonlar.Contains("dteIslemTarihi"))
                    {
                        tarihKolon = "d.dteIslemTarihi";
                    }
                    else if (masterKolonlar.Contains("dteFisTarihi"))
                    {
                        tarihKolon = "m.dteFisTarihi";
                    }

                    string tarihSecim = tarihKolon ?? "NULL";
                    string girisCikis = detayKolonlar.Contains("nGirisCikis") ? "d.nGirisCikis" : "NULL";
                    string girisMiktar = detayKolonlar.Contains("lGirisMiktar1") ? "d.lGirisMiktar1" : "NULL";
                    string cikisMiktar = detayKolonlar.Contains("lCikisMiktar1") ? "d.lCikisMiktar1" : "NULL";
                    string birimFiyat = detayKolonlar.Contains("lCikisFiyat") ? "d.lCikisFiyat" :
                        (detayKolonlar.Contains("lBrutFiyat") ? "d.lBrutFiyat" : "NULL");
                    string tutar = detayKolonlar.Contains("lCikisTutar") ? "d.lCikisTutar" :
                        (detayKolonlar.Contains("lBrutTutar") ? "d.lBrutTutar" : "NULL");
                    string aciklama = detayKolonlar.Contains("sAciklama") ? "d.sAciklama" : "NULL";
                    string fisTipi = detayKolonlar.Contains("sFisTipi") ? "d.sFisTipi" : "NULL";
                    string fisNo = masterKolonlar.Contains("lFisNo") ? "m.lFisNo" : "NULL";

                    List<string> filtreler = new List<string> { "d.nStokID = @stokId" };
                    if (detayKolonlar.Contains("sDepo") && !string.IsNullOrWhiteSpace(depo))
                    {
                        filtreler.Add("d.sDepo = @depo");
                        cmd.Parameters.AddWithValue("@depo", depo);
                    }

                    if (!string.IsNullOrWhiteSpace(tarihKolon))
                    {
                        if (baslangic.HasValue)
                        {
                            filtreler.Add($"{tarihKolon} >= @baslangic");
                            cmd.Parameters.AddWithValue("@baslangic", baslangic.Value.Date);
                        }
                        if (bitis.HasValue)
                        {
                            filtreler.Add($"{tarihKolon} <= @bitis");
                            cmd.Parameters.AddWithValue("@bitis", bitis.Value.Date.AddDays(1).AddSeconds(-1));
                        }
                    }

                    string orderBy = !string.IsNullOrWhiteSpace(tarihKolon) ? $"{tarihKolon} DESC" :
                        (detayKolonlar.Contains("nIslemID") ? "d.nIslemID DESC" : "d.nStokID DESC");

                    string sql = $@"
SELECT {tarihSecim} AS Tarih,
       {girisCikis} AS GirisCikis,
       {girisMiktar} AS GirisMiktar1,
       {cikisMiktar} AS CikisMiktar1,
       {birimFiyat} AS BirimFiyat,
       {tutar} AS Tutar,
       {aciklama} AS Aciklama,
       {fisTipi} AS FisTipi,
       {fisNo} AS FisNo
FROM tbStokFisiDetayi d
LEFT JOIN tbStokFisiMaster m ON d.nStokFisiID = m.nStokFisiID
WHERE {string.Join(" AND ", filtreler)}
ORDER BY {orderBy}
OFFSET @offset ROWS FETCH NEXT @sayfaBoyutu ROWS ONLY;";

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    cmd.Parameters.AddWithValue("@offset", offset);
                    cmd.Parameters.AddWithValue("@sayfaBoyutu", sayfaBoyutu);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static int StokFisiOlustur(int girisCikis, string fisTipi, string depo, int? firmaId, string aciklama, List<(int stokId, decimal miktar, decimal? birimFiyat, string satirAciklama)> kalemler)
        {
            if (kalemler == null || kalemler.Count == 0)
            {
                throw new ArgumentException("Fiş kalemi boş olamaz.");
            }

            return TransactionCalistir((conn, tran) =>
            {
                HashSet<string> masterKolonlar = TabloKolonlariniGetir(conn, tran, "tbStokFisiMaster");
                HashSet<string> detayKolonlar = TabloKolonlariniGetir(conn, tran, "tbStokFisiDetayi");

                bool masterIdentity = IdentityKolonVarMi(conn, tran, "tbStokFisiMaster", "nStokFisiID");
                int stokFisiId = masterIdentity ? 0 : YeniIdUret(conn, tran, "tbStokFisiMaster", "nStokFisiID");

                decimal toplamMiktar = kalemler.Sum(k => k.miktar);
                decimal toplamTutar = kalemler.Sum(k => (k.birimFiyat ?? 0m) * k.miktar);

                Dictionary<string, object> masterKolonDegerleri = new Dictionary<string, object>
                {
                    ["nStokFisiID"] = stokFisiId,
                    ["nGirisCikis"] = girisCikis,
                    ["sFisTipi"] = fisTipi,
                    ["sDepo"] = depo,
                    ["nFirmaID"] = firmaId ?? 1,
                    ["dteFisTarihi"] = DateTime.Now,
                    ["lToplamMiktar"] = toplamMiktar,
                    ["lMalBedeli"] = toplamTutar,
                    ["lNetTutar"] = toplamTutar,
                    ["lToplamTutar"] = toplamTutar,
                    ["sAciklama"] = aciklama
                };

                if (masterIdentity && masterKolonlar.Contains("nStokFisiID"))
                {
                    stokFisiId = DinamikInsert(conn, tran, "tbStokFisiMaster", masterKolonDegerleri, true);
                }
                else
                {
                    DinamikInsert(conn, tran, "tbStokFisiMaster", masterKolonDegerleri, false);
                }

                bool detayIdentity = IdentityKolonVarMi(conn, tran, "tbStokFisiDetayi", "nIslemID");

                foreach (var kalem in kalemler)
                {
                    int islemId = detayIdentity ? 0 : YeniIdUret(conn, tran, "tbStokFisiDetayi", "nIslemID");
                    Dictionary<string, object> detayKolonDegerleri = new Dictionary<string, object>
                    {
                        ["nIslemID"] = islemId,
                        ["nStokFisiID"] = stokFisiId,
                        ["nStokID"] = kalem.stokId,
                        ["nGirisCikis"] = girisCikis,
                        ["sDepo"] = depo,
                        ["sFisTipi"] = fisTipi,
                        ["dteFisTarihi"] = DateTime.Now,
                        ["dteIslemTarihi"] = DateTime.Now,
                        ["sAciklama"] = kalem.satirAciklama
                    };

                    if (girisCikis == 1)
                    {
                        detayKolonDegerleri["lGirisMiktar1"] = kalem.miktar;
                    }
                    else
                    {
                        detayKolonDegerleri["lCikisMiktar1"] = kalem.miktar;
                    }

                    if (kalem.birimFiyat.HasValue)
                    {
                        decimal birimFiyatDegeri = kalem.birimFiyat.Value;
                        decimal tutarDegeri = birimFiyatDegeri * kalem.miktar;
                        detayKolonDegerleri["lCikisFiyat"] = birimFiyatDegeri;
                        detayKolonDegerleri["lBrutFiyat"] = birimFiyatDegeri;
                        detayKolonDegerleri["lBirimFiyat"] = birimFiyatDegeri;
                        detayKolonDegerleri["lCikisTutar"] = tutarDegeri;
                        detayKolonDegerleri["lBrutTutar"] = tutarDegeri;
                        detayKolonDegerleri["lNetTutar"] = tutarDegeri;
                    }

                    if (detayIdentity && detayKolonlar.Contains("nIslemID"))
                    {
                        DinamikInsert(conn, tran, "tbStokFisiDetayi", detayKolonDegerleri, true);
                    }
                    else
                    {
                        DinamikInsert(conn, tran, "tbStokFisiDetayi", detayKolonDegerleri, false);
                    }
                }

                return stokFisiId;
            });
        }

        public static int StokGirisiYap(int stokId, decimal miktar, string depo, string aciklama)
        {
            return StokFisiOlustur(1, "SG", depo, null, aciklama, new List<(int stokId, decimal miktar, decimal? birimFiyat, string satirAciklama)>
            {
                (stokId, miktar, null, aciklama)
            });
        }

        public static int StokCikisiYap(int stokId, decimal miktar, string depo, string aciklama)
        {
            return StokFisiOlustur(2, "SC", depo, null, aciklama, new List<(int stokId, decimal miktar, decimal? birimFiyat, string satirAciklama)>
            {
                (stokId, miktar, null, aciklama)
            });
        }

        public static DataTable SayimIcinUrunleriGetir(string arama, string depo, int sayfa, int sayfaBoyutu = 50)
        {
            int offset = Math.Max(0, sayfa - 1) * sayfaBoyutu;
            string sql = @"
WITH SayimListe AS (
    SELECT s.nStokID,
           s.sKodu,
           s.sAciklama,
           ISNULL(stk.StokMiktari, 0) AS StokMiktari
    FROM tbStok s
    OUTER APPLY (
        SELECT SUM(ISNULL(lGirisMiktar1, 0)) - SUM(ISNULL(lCikisMiktar1, 0)) AS StokMiktari
        FROM tbStokFisiDetayi d
        WHERE d.nStokID = s.nStokID
          AND (@depo IS NULL OR @depo = '' OR d.sDepo = @depo)
    ) stk
    WHERE (@arama IS NULL OR @arama = '' OR s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%')
)
SELECT *
FROM SayimListe
ORDER BY nStokID
OFFSET @offset ROWS FETCH NEXT @sayfaBoyutu ROWS ONLY;";

            return SorguCalistirDataTable(
                sql,
                Parametre("@arama", string.IsNullOrWhiteSpace(arama) ? null : arama),
                Parametre("@depo", string.IsNullOrWhiteSpace(depo) ? null : depo),
                Parametre("@offset", offset),
                Parametre("@sayfaBoyutu", sayfaBoyutu)
            );
        }

        public static (int? girisFisId, int? cikisFisId) SayimDuzeltmeFisYaz(string depo, string aciklama, List<(int stokId, decimal farkMiktar, string satirAciklama)> farklar)
        {
            if (farklar == null || farklar.Count == 0)
            {
                return (null, null);
            }

            List<(int stokId, decimal miktar, decimal? birimFiyat, string satirAciklama)> girisKalemleri = farklar
                .Where(f => f.farkMiktar > 0)
                .Select(f => (f.stokId, f.farkMiktar, (decimal?)null, f.satirAciklama))
                .ToList();

            List<(int stokId, decimal miktar, decimal? birimFiyat, string satirAciklama)> cikisKalemleri = farklar
                .Where(f => f.farkMiktar < 0)
                .Select(f => (f.stokId, Math.Abs(f.farkMiktar), (decimal?)null, f.satirAciklama))
                .ToList();

            int? girisFisId = null;
            int? cikisFisId = null;

            if (girisKalemleri.Count > 0)
            {
                girisFisId = StokFisiOlustur(1, "SY", depo, null, aciklama, girisKalemleri);
            }

            if (cikisKalemleri.Count > 0)
            {
                cikisFisId = StokFisiOlustur(2, "SY", depo, null, aciklama, cikisKalemleri);
            }

            return (girisFisId, cikisFisId);
        }

        public static DataTable Sinif1leriGetir()
        {
            return SorguCalistirDataTable("SELECT DISTINCT sSinifKodu1 FROM tbStok WHERE sSinifKodu1 IS NOT NULL AND sSinifKodu1 <> '' ORDER BY sSinifKodu1");
        }

        public static DataTable Sinif2leriGetir(string sinif1)
        {
            return SorguCalistirDataTable(
                "SELECT DISTINCT sSinifKodu2 FROM tbStok WHERE sSinifKodu1 = @sinif1 AND sSinifKodu2 IS NOT NULL AND sSinifKodu2 <> '' ORDER BY sSinifKodu2",
                Parametre("@sinif1", sinif1)
            );
        }

        public static DataSet UrunDetayGetir(int stokId)
        {
            string sql = @"
SELECT * FROM tbStok WHERE nStokID = @stokId;
SELECT sBarkod FROM tbStokBarkodu WHERE nStokID = @stokId ORDER BY sBarkod;
SELECT sFiyatTipi, lFiyat FROM tbStokFiyati WHERE nStokID = @stokId;
SELECT SUM(ISNULL(lGirisMiktar1,0)) - SUM(ISNULL(lCikisMiktar1,0)) AS StokMiktari
FROM tbStokFisiDetayi WHERE nStokID = @stokId;";

            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@stokId", stokId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    ds.Tables[0].TableName = "Urun";
                    ds.Tables[1].TableName = "Barkodlar";
                    ds.Tables[2].TableName = "Fiyatlar";
                    ds.Tables[3].TableName = "Stok";
                    return ds;
                }
            }
        }

        public static bool BarkodVarMi(string barkod, int? stokId)
        {
            string sql = "SELECT COUNT(1) FROM tbStokBarkodu WHERE sBarkod = @barkod AND (@stokId IS NULL OR nStokID <> @stokId)";
            object sonuc = SorguCalistirScalar(sql, Parametre("@barkod", barkod), Parametre("@stokId", stokId));
            return Convert.ToInt32(sonuc) > 0;
        }

        public static bool StokKoduVarMi(string kodu, int? stokId)
        {
            string sql = "SELECT COUNT(1) FROM tbStok WHERE sKodu = @kodu AND (@stokId IS NULL OR nStokID <> @stokId)";
            object sonuc = SorguCalistirScalar(sql, Parametre("@kodu", kodu), Parametre("@stokId", stokId));
            return Convert.ToInt32(sonuc) > 0;
        }

        public static void UrunGuncelle(int stokId, string kodu, string aciklama, string kisaAdi, string birim, string sinif1, string sinif2, List<string> barkodlar, Dictionary<string, decimal> fiyatlar)
        {
            TransactionCalistir((conn, tran) =>
            {
                const string stokSql = @"
UPDATE tbStok
SET sKodu = @kodu,
    sAciklama = @aciklama,
    sKisaAdi = @kisaAdi,
    sBirimCinsi1 = @birim,
    sSinifKodu1 = @sinif1,
    sSinifKodu2 = @sinif2
WHERE nStokID = @stokId";
                using (SqlCommand cmd = new SqlCommand(stokSql, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@kodu", kodu);
                    cmd.Parameters.AddWithValue("@aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@kisaAdi", (object)kisaAdi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@birim", birim);
                    cmd.Parameters.AddWithValue("@sinif1", (object)sinif1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sinif2", (object)sinif2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand("DELETE FROM tbStokBarkodu WHERE nStokID = @stokId", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    cmd.ExecuteNonQuery();
                }

                if (barkodlar != null)
                {
                    foreach (string barkod in barkodlar.Where(b => !string.IsNullOrWhiteSpace(b)))
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO tbStokBarkodu (nStokID, sBarkod) VALUES (@stokId, @barkod)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@stokId", stokId);
                            cmd.Parameters.AddWithValue("@barkod", barkod);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (fiyatlar != null)
                {
                    foreach (var fiyat in fiyatlar)
                    {
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM tbStokFiyati WHERE nStokID = @stokId AND sFiyatTipi = @tip", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@stokId", stokId);
                            cmd.Parameters.AddWithValue("@tip", fiyat.Key);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand("INSERT INTO tbStokFiyati (nStokID, sFiyatTipi, lFiyat) VALUES (@stokId, @tip, @fiyat)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@stokId", stokId);
                            cmd.Parameters.AddWithValue("@tip", fiyat.Key);
                            cmd.Parameters.AddWithValue("@fiyat", fiyat.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            });
        }

        public static void UrunEkle(string kodu, string aciklama, string kisaAdi, string birim, string sinif1, string sinif2, List<string> barkodlar, Dictionary<string, decimal> fiyatlar)
        {
            TransactionCalistir((conn, tran) =>
            {
                string sql = @"
INSERT INTO tbStok (sKodu, sAciklama, sKisaAdi, sBirimCinsi1, sSinifKodu1, sSinifKodu2)
VALUES (@kodu, @aciklama, @kisaAdi, @birim, @sinif1, @sinif2);
SELECT CAST(SCOPE_IDENTITY() AS int);";
                int stokId;
                using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@kodu", kodu);
                    cmd.Parameters.AddWithValue("@aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@kisaAdi", (object)kisaAdi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@birim", birim);
                    cmd.Parameters.AddWithValue("@sinif1", (object)sinif1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sinif2", (object)sinif2 ?? DBNull.Value);
                    stokId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (barkodlar != null)
                {
                    foreach (string barkod in barkodlar.Where(b => !string.IsNullOrWhiteSpace(b)))
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO tbStokBarkodu (nStokID, sBarkod) VALUES (@stokId, @barkod)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@stokId", stokId);
                            cmd.Parameters.AddWithValue("@barkod", barkod);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (fiyatlar != null)
                {
                    foreach (var fiyat in fiyatlar)
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO tbStokFiyati (nStokID, sFiyatTipi, lFiyat) VALUES (@stokId, @tip, @fiyat)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@stokId", stokId);
                            cmd.Parameters.AddWithValue("@tip", fiyat.Key);
                            cmd.Parameters.AddWithValue("@fiyat", fiyat.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            });
        }

        public static DataTable BarkodlariGetir(int stokId)
        {
            return SorguCalistirDataTable(
                "SELECT sBarkod FROM tbStokBarkodu WHERE nStokID = @stokId ORDER BY sBarkod",
                Parametre("@stokId", stokId)
            );
        }

        public static void BarkodEkle(int stokId, string barkod)
        {
            SorguCalistirNonQuery(
                "INSERT INTO tbStokBarkodu (nStokID, sBarkod) VALUES (@stokId, @barkod)",
                Parametre("@stokId", stokId),
                Parametre("@barkod", barkod)
            );
        }

        public static void BarkodSil(int stokId, string barkod)
        {
            SorguCalistirNonQuery(
                "DELETE FROM tbStokBarkodu WHERE nStokID = @stokId AND sBarkod = @barkod",
                Parametre("@stokId", stokId),
                Parametre("@barkod", barkod)
            );
        }

        public static int UrunSayisiGetir(string arama, string barkod, string sinif1, string sinif2, string stokDurum, decimal? minFiyat, decimal? maxFiyat)
        {
            StringBuilder sql = new StringBuilder(@"
SELECT COUNT(1)
FROM tbStok s
OUTER APPLY (SELECT TOP 1 sBarkod FROM tbStokBarkodu b WHERE b.nStokID = s.nStokID ORDER BY b.sBarkod) b
OUTER APPLY (SELECT TOP 1 lFiyat FROM tbStokFiyati f WHERE f.nStokID = s.nStokID AND f.sFiyatTipi = @fiyatTipi) f
OUTER APPLY (
    SELECT SUM(ISNULL(lGirisMiktar1,0)) - SUM(ISNULL(lCikisMiktar1,0)) AS StokMiktari
    FROM tbStokFisiDetayi d WHERE d.nStokID = s.nStokID
) stk
WHERE 1=1");
            List<SqlParameter> parametreler = new List<SqlParameter>
            {
                Parametre("@fiyatTipi", Ayarlar.VarsayilanFiyatTipi)
            };

            if (!string.IsNullOrWhiteSpace(arama))
            {
                sql.Append(" AND (s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%')");
                parametreler.Add(Parametre("@arama", arama));
            }

            if (!string.IsNullOrWhiteSpace(barkod))
            {
                sql.Append(" AND b.sBarkod = @barkod");
                parametreler.Add(Parametre("@barkod", barkod));
            }

            if (!string.IsNullOrWhiteSpace(sinif1) && sinif1 != "Hepsi")
            {
                sql.Append(" AND s.sSinifKodu1 = @sinif1");
                parametreler.Add(Parametre("@sinif1", sinif1));
            }

            if (!string.IsNullOrWhiteSpace(sinif2) && sinif2 != "Hepsi")
            {
                sql.Append(" AND s.sSinifKodu2 = @sinif2");
                parametreler.Add(Parametre("@sinif2", sinif2));
            }

            if (minFiyat.HasValue)
            {
                sql.Append(" AND f.lFiyat >= @minFiyat");
                parametreler.Add(Parametre("@minFiyat", minFiyat));
            }

            if (maxFiyat.HasValue && maxFiyat.Value > 0)
            {
                sql.Append(" AND f.lFiyat <= @maxFiyat");
                parametreler.Add(Parametre("@maxFiyat", maxFiyat));
            }

            if (!string.IsNullOrWhiteSpace(stokDurum))
            {
                string durum = stokDurum.ToLowerInvariant();
                if (durum == "var")
                {
                    sql.Append(" AND stk.StokMiktari > 0");
                }
                else if (durum == "yok")
                {
                    sql.Append(" AND stk.StokMiktari <= 0");
                }
            }

            object sonuc = SorguCalistirScalar(sql.ToString(), parametreler.ToArray());
            return sonuc == null ? 0 : Convert.ToInt32(sonuc);
        }

        public static DataTable UrunleriGetir(string arama, string barkod, string sinif1, string sinif2, string stokDurum, decimal? minFiyat, decimal? maxFiyat, int sayfa, int sayfaBoyutu = 50)
        {
            int offset = Math.Max(0, sayfa - 1) * sayfaBoyutu;
            StringBuilder sql = new StringBuilder(@"
SELECT s.nStokID,
       s.sKodu,
       s.sAciklama,
       b.sBarkod AS Barkod,
       s.sBirimCinsi1 AS sBirimCinsi,
       f.lFiyat AS Fiyat,
       stk.StokMiktari,
       s.sSinifKodu1 AS Kategori1,
       s.sSinifKodu2 AS Kategori2
FROM tbStok s
OUTER APPLY (SELECT TOP 1 sBarkod FROM tbStokBarkodu b WHERE b.nStokID = s.nStokID ORDER BY b.sBarkod) b
OUTER APPLY (SELECT TOP 1 lFiyat FROM tbStokFiyati f WHERE f.nStokID = s.nStokID AND f.sFiyatTipi = @fiyatTipi) f
OUTER APPLY (
    SELECT SUM(ISNULL(lGirisMiktar1,0)) - SUM(ISNULL(lCikisMiktar1,0)) AS StokMiktari
    FROM tbStokFisiDetayi d WHERE d.nStokID = s.nStokID
) stk
WHERE 1=1");
            List<SqlParameter> parametreler = new List<SqlParameter>
            {
                Parametre("@fiyatTipi", Ayarlar.VarsayilanFiyatTipi)
            };

            if (!string.IsNullOrWhiteSpace(arama))
            {
                sql.Append(" AND (s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%')");
                parametreler.Add(Parametre("@arama", arama));
            }

            if (!string.IsNullOrWhiteSpace(barkod))
            {
                sql.Append(" AND b.sBarkod = @barkod");
                parametreler.Add(Parametre("@barkod", barkod));
            }

            if (!string.IsNullOrWhiteSpace(sinif1) && sinif1 != "Hepsi")
            {
                sql.Append(" AND s.sSinifKodu1 = @sinif1");
                parametreler.Add(Parametre("@sinif1", sinif1));
            }

            if (!string.IsNullOrWhiteSpace(sinif2) && sinif2 != "Hepsi")
            {
                sql.Append(" AND s.sSinifKodu2 = @sinif2");
                parametreler.Add(Parametre("@sinif2", sinif2));
            }

            if (minFiyat.HasValue)
            {
                sql.Append(" AND f.lFiyat >= @minFiyat");
                parametreler.Add(Parametre("@minFiyat", minFiyat));
            }

            if (maxFiyat.HasValue && maxFiyat.Value > 0)
            {
                sql.Append(" AND f.lFiyat <= @maxFiyat");
                parametreler.Add(Parametre("@maxFiyat", maxFiyat));
            }

            if (!string.IsNullOrWhiteSpace(stokDurum))
            {
                string durum = stokDurum.ToLowerInvariant();
                if (durum == "var")
                {
                    sql.Append(" AND stk.StokMiktari > 0");
                }
                else if (durum == "yok")
                {
                    sql.Append(" AND stk.StokMiktari <= 0");
                }
            }

            sql.Append(" ORDER BY s.nStokID OFFSET @offset ROWS FETCH NEXT @sayfaBoyutu ROWS ONLY;");
            parametreler.Add(Parametre("@offset", offset));
            parametreler.Add(Parametre("@sayfaBoyutu", sayfaBoyutu));

            return SorguCalistirDataTable(sql.ToString(), parametreler.ToArray());
        }

        public static void UrunSil(int stokId, bool barkodlarDahil)
        {
            TransactionCalistir((conn, tran) =>
            {
                const string kontrolSql = @"
SELECT COUNT(1)
FROM (
    SELECT nStokID FROM tbAlisverisSiparis WHERE nStokID = @stokId
    UNION ALL
    SELECT nStokID FROM tbStokFisiDetayi WHERE nStokID = @stokId
) x";
                using (SqlCommand kontrolCmd = new SqlCommand(kontrolSql, conn, tran))
                {
                    kontrolCmd.Parameters.AddWithValue("@stokId", stokId);
                    int adet = Convert.ToInt32(kontrolCmd.ExecuteScalar());
                    if (adet > 0)
                    {
                        throw new InvalidOperationException("Satış veya stok hareketi bulunan ürün silinemez.");
                    }
                }

                if (barkodlarDahil)
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM tbStokBarkodu WHERE nStokID = @stokId", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@stokId", stokId);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlCommand cmd = new SqlCommand("DELETE FROM tbStok WHERE nStokID = @stokId", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    cmd.ExecuteNonQuery();
                }
            });
        }

       public static DataTable GunSatisOzeti()
       {
           string sql = @"
SELECT CAST(GETDATE() AS date) AS Tarih,
       SUM(ISNULL(lToplamTutar, 0)) AS ToplamTutar,
       SUM(ISNULL(lToplamKdv, 0)) AS ToplamKdv,
       COUNT(1) AS SatisAdedi
FROM tbAlisVeris
WHERE CAST(dteFisTarihi AS date) = CAST(GETDATE() AS date);";
           return SorguCalistirDataTable(sql);
       }

        public static DataTable UrunAra(string arama)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                HashSet<string> stokKolonlar = TabloKolonlariniGetir(conn, null, "tbStok");
                HashSet<string> fiyatKolonlar = TabloKolonlariniGetir(conn, null, "tbStokFiyati");

                string kdvKolon = IlkKolonuBul(stokKolonlar, "nKdvOrani", "lKdvOrani", "nKdv", "lKdv");
                string fiyatKolon = IlkKolonuBul(fiyatKolonlar, "lFiyat", "nFiyat");

                string kdvSecim = string.IsNullOrWhiteSpace(kdvKolon) ? "@varsayilanKdv" : $"s.{kdvKolon}";
                string fiyatApply = string.IsNullOrWhiteSpace(fiyatKolon)
                    ? "OUTER APPLY (SELECT CAST(0 AS decimal(18,2)) AS lFiyat) f"
                    : $"OUTER APPLY (SELECT TOP 1 f.{fiyatKolon} AS lFiyat FROM tbStokFiyati f WHERE f.nStokID = s.nStokID AND f.sFiyatTipi = @fiyatTipi) f";

                string sql = $@"
SELECT TOP 20 s.nStokID,
       s.sKodu,
       s.sAciklama,
       s.sBirimCinsi1 AS sBirimCinsi,
       b.sBarkod,
       f.lFiyat AS lFiyat,
       {kdvSecim} AS nKdvOrani
FROM tbStok s
OUTER APPLY (SELECT TOP 1 sBarkod FROM tbStokBarkodu b WHERE b.nStokID = s.nStokID ORDER BY b.sBarkod) b
{fiyatApply}
WHERE s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%'
ORDER BY s.sAciklama";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@arama", arama);
                    cmd.Parameters.AddWithValue("@fiyatTipi", Ayarlar.VarsayilanFiyatTipi);
                    cmd.Parameters.AddWithValue("@varsayilanKdv", Ayarlar.VarsayilanKdvOrani);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static DataTable BarkodIleUrunBul(string barkod)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                HashSet<string> stokKolonlar = TabloKolonlariniGetir(conn, null, "tbStok");
                HashSet<string> fiyatKolonlar = TabloKolonlariniGetir(conn, null, "tbStokFiyati");

                string kdvKolon = IlkKolonuBul(stokKolonlar, "nKdvOrani", "lKdvOrani", "nKdv", "lKdv");
                string fiyatKolon = IlkKolonuBul(fiyatKolonlar, "lFiyat", "nFiyat");

                string kdvSecim = string.IsNullOrWhiteSpace(kdvKolon) ? "@varsayilanKdv" : $"s.{kdvKolon}";
                string fiyatApply = string.IsNullOrWhiteSpace(fiyatKolon)
                    ? "OUTER APPLY (SELECT CAST(0 AS decimal(18,2)) AS lFiyat) f"
                    : $"OUTER APPLY (SELECT TOP 1 f.{fiyatKolon} AS lFiyat FROM tbStokFiyati f WHERE f.nStokID = s.nStokID AND f.sFiyatTipi = @fiyatTipi) f";

                string sql = $@"
SELECT TOP 1 s.nStokID,
       s.sKodu,
       s.sAciklama,
       s.sBirimCinsi1 AS sBirimCinsi,
       b.sBarkod,
       f.lFiyat AS lFiyat,
       {kdvSecim} AS nKdvOrani
FROM tbStok s
INNER JOIN tbStokBarkodu b ON b.nStokID = s.nStokID
{fiyatApply}
WHERE b.sBarkod = @barkod";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@barkod", barkod);
                    cmd.Parameters.AddWithValue("@fiyatTipi", Ayarlar.VarsayilanFiyatTipi);
                    cmd.Parameters.AddWithValue("@varsayilanKdv", Ayarlar.VarsayilanKdvOrani);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static int SatisKaydet(DataTable sepet, int? musteriId, string odemeTipi, decimal nakit, decimal kart, decimal dipYuzde)
        {
            if (sepet == null || sepet.Rows.Count == 0)
            {
                return 0;
            }

            return TransactionCalistir((conn, tran) =>
            {
                HashSet<string> alisVerisKolonlar = TabloKolonlariniGetir(conn, tran, "tbAlisVeris");
                HashSet<string> siparisKolonlar = TabloKolonlariniGetir(conn, tran, "tbAlisverisSiparis");
                HashSet<string> stokMasterKolonlar = TabloKolonlariniGetir(conn, tran, "tbStokFisiMaster");
                HashSet<string> stokDetayKolonlar = TabloKolonlariniGetir(conn, tran, "tbStokFisiDetayi");
                HashSet<string> odemeKolonlar = TabloKolonlariniGetir(conn, tran, "tbOdeme");
                HashSet<string> nakitKolonlar = TabloKolonlariniGetir(conn, tran, "tbNakitKasa");

                bool alisVerisIdentity = IdentityKolonVarMi(conn, tran, "tbAlisVeris", "nAlisverisID");
                int satisId = alisVerisIdentity ? 0 : YeniIdUret(conn, tran, "tbAlisVeris", "nAlisverisID");

                List<SatisSatir> satirlar = SatisSatirlariHazirla(sepet, dipYuzde);
                decimal netToplam = satirlar.Sum(s => s.NetTutar);
                decimal kdvToplam = satirlar.Sum(s => s.KdvTutar);
                decimal brutToplam = satirlar.Sum(s => s.BrutTutar);
                decimal iskontoToplam = satirlar.Sum(s => s.SatirIskonto) + satirlar.Sum(s => s.DipIskonto);

                Dictionary<string, object> master = new Dictionary<string, object>
                {
                    ["nAlisverisID"] = satisId,
                    ["dteFisTarihi"] = DateTime.Now,
                    ["lToplamTutar"] = netToplam,
                    ["lNetTutar"] = netToplam,
                    ["lToplamKdv"] = kdvToplam,
                    ["lMalBedeli"] = brutToplam,
                    ["lIndirimTutar"] = iskontoToplam,
                    ["sFisTipi"] = odemeTipi,
                    ["nMusteriID"] = musteriId,
                    ["sDepo"] = Ayarlar.DepoKodu,
                    ["sKasiyer"] = Ayarlar.KasiyerRumuzu
                };

                if (alisVerisIdentity && alisVerisKolonlar.Contains("nAlisverisID"))
                {
                    satisId = DinamikInsert(conn, tran, "tbAlisVeris", master, true);
                }
                else
                {
                    DinamikInsert(conn, tran, "tbAlisVeris", master, false);
                }

                bool siparisIdentity = IdentityKolonVarMi(conn, tran, "tbAlisverisSiparis", "nIslemID");

                foreach (SatisSatir satir in satirlar)
                {
                    Dictionary<string, object> detay = new Dictionary<string, object>
                    {
                        ["nAlisverisID"] = satisId,
                        ["nStokID"] = satir.StokId,
                        ["lMiktar"] = satir.Miktar,
                        ["lBirimFiyat"] = satir.BirimFiyat,
                        ["lBrutTutar"] = satir.BrutTutar,
                        ["lTutar"] = satir.NetTutar,
                        ["lNetTutar"] = satir.NetTutar,
                        ["lKdvOrani"] = satir.KdvOrani,
                        ["lKdvTutar"] = satir.KdvTutar,
                        ["sAciklama"] = satir.Aciklama
                    };

                    if (!siparisIdentity)
                    {
                        string siparisIdKolon = IlkKolonuBul(siparisKolonlar, "nIslemID", "nDetayID", "nSiparisID");
                        if (!string.IsNullOrWhiteSpace(siparisIdKolon))
                        {
                            detay[siparisIdKolon] = YeniIdUret(conn, tran, "tbAlisverisSiparis", siparisIdKolon);
                        }
                    }

                    DinamikInsert(conn, tran, "tbAlisverisSiparis", detay, siparisIdentity);
                }

                bool stokMasterIdentity = IdentityKolonVarMi(conn, tran, "tbStokFisiMaster", "nStokFisiID");
                int stokFisiId = stokMasterIdentity ? 0 : YeniIdUret(conn, tran, "tbStokFisiMaster", "nStokFisiID");

                Dictionary<string, object> stokMaster = new Dictionary<string, object>
                {
                    ["nStokFisiID"] = stokFisiId,
                    ["nGirisCikis"] = 2,
                    ["sFisTipi"] = "SAT",
                    ["dteFisTarihi"] = DateTime.Now,
                    ["sDepo"] = Ayarlar.DepoKodu,
                    ["lToplamMiktar"] = satirlar.Sum(s => s.Miktar),
                    ["lMalBedeli"] = brutToplam,
                    ["lNetTutar"] = netToplam,
                    ["lToplamTutar"] = netToplam,
                    ["sAciklama"] = "Satış fişi",
                    ["nAlisverisID"] = satisId
                };

                if (stokMasterIdentity && stokMasterKolonlar.Contains("nStokFisiID"))
                {
                    stokFisiId = DinamikInsert(conn, tran, "tbStokFisiMaster", stokMaster, true);
                }
                else
                {
                    DinamikInsert(conn, tran, "tbStokFisiMaster", stokMaster, false);
                }

                bool stokDetayIdentity = IdentityKolonVarMi(conn, tran, "tbStokFisiDetayi", "nIslemID");

                foreach (SatisSatir satir in satirlar)
                {
                    Dictionary<string, object> stokDetay = new Dictionary<string, object>
                    {
                        ["nStokFisiID"] = stokFisiId,
                        ["nStokID"] = satir.StokId,
                        ["nGirisCikis"] = 2,
                        ["sFisTipi"] = "SAT",
                        ["sDepo"] = Ayarlar.DepoKodu,
                        ["dteFisTarihi"] = DateTime.Now,
                        ["dteIslemTarihi"] = DateTime.Now,
                        ["lCikisMiktar1"] = satir.Miktar,
                        ["lCikisFiyat"] = satir.BirimFiyat,
                        ["lCikisTutar"] = satir.NetTutar,
                        ["lNetTutar"] = satir.NetTutar,
                        ["sAciklama"] = satir.Aciklama
                    };

                    if (!stokDetayIdentity)
                    {
                        string stokDetayIdKolon = IlkKolonuBul(stokDetayKolonlar, "nIslemID", "nDetayID");
                        if (!string.IsNullOrWhiteSpace(stokDetayIdKolon))
                        {
                            stokDetay[stokDetayIdKolon] = YeniIdUret(conn, tran, "tbStokFisiDetayi", stokDetayIdKolon);
                        }
                    }

                    DinamikInsert(conn, tran, "tbStokFisiDetayi", stokDetay, stokDetayIdentity);
                }

                KaydetOdeme(conn, tran, odemeKolonlar, satisId, odemeTipi, nakit, kart, netToplam);
                KaydetNakitKasa(conn, tran, nakitKolonlar, satisId, nakit);

                return satisId;
            });
        }

        public static DataTable FiyatlariGetir(int stokId)
        {
            return SorguCalistirDataTable(
                "SELECT sFiyatTipi, lFiyat FROM tbStokFiyati WHERE nStokID = @stokId",
                Parametre("@stokId", stokId)
            );
        }

        public static void FiyatGuncelle(int stokId, string fiyatTipi, decimal fiyat)
        {
            TransactionCalistir((conn, tran) =>
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM tbStokFiyati WHERE nStokID = @stokId AND sFiyatTipi = @tip", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    cmd.Parameters.AddWithValue("@tip", fiyatTipi);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbStokFiyati (nStokID, sFiyatTipi, lFiyat) VALUES (@stokId, @tip, @fiyat)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    cmd.Parameters.AddWithValue("@tip", fiyatTipi);
                    cmd.Parameters.AddWithValue("@fiyat", fiyat);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        private static string IlkKolonuBul(HashSet<string> kolonlar, params string[] adaylar)
        {
            foreach (string aday in adaylar)
            {
                if (kolonlar.Contains(aday))
                {
                    return aday;
                }
            }
            return null;
        }

        private static List<SatisSatir> SatisSatirlariHazirla(DataTable sepet, decimal dipYuzde)
        {
            List<SatisSatir> satirlar = new List<SatisSatir>();

            foreach (DataRow row in sepet.Rows)
            {
                int stokId = Convert.ToInt32(row["nStokID"]);
                decimal miktar = Convert.ToDecimal(row["lMiktar"]);
                decimal birimFiyat = Convert.ToDecimal(row["lBirimFiyat"]);
                decimal iskonto = Convert.ToDecimal(row["nIskontoYuzde"]);
                decimal kdvOrani = Convert.ToDecimal(row["nKdvOrani"]);
                string aciklama = row["sAciklama"].ToString();

                decimal brut = Yardimcilar.YuvarlaKurus(miktar * birimFiyat);
                decimal satirIskonto = Yardimcilar.YuvarlaKurus(brut * (iskonto / 100m));
                decimal net = Yardimcilar.YuvarlaKurus(brut - satirIskonto);

                satirlar.Add(new SatisSatir
                {
                    StokId = stokId,
                    Miktar = miktar,
                    BirimFiyat = birimFiyat,
                    BrutTutar = brut,
                    SatirIskonto = satirIskonto,
                    NetTutar = net,
                    KdvOrani = kdvOrani,
                    Aciklama = aciklama
                });
            }

            decimal satirNetToplam = satirlar.Sum(s => s.NetTutar);
            decimal dipIskontoTutar = dipYuzde > 0 ? Yardimcilar.YuvarlaKurus(satirNetToplam * (dipYuzde / 100m)) : 0m;
            decimal netToplam = Yardimcilar.YuvarlaKurus(satirNetToplam - dipIskontoTutar);

            foreach (SatisSatir satir in satirlar)
            {
                decimal pay = satirNetToplam > 0 ? satir.NetTutar / satirNetToplam : 0m;
                decimal dipPay = Yardimcilar.YuvarlaKurus(dipIskontoTutar * pay);
                satir.DipIskonto = dipPay;
                satir.NetTutar = Yardimcilar.YuvarlaKurus(satir.NetTutar - dipPay);
                satir.KdvTutar = Yardimcilar.KdvTutarHesapla(satir.NetTutar, satir.KdvOrani);
            }

            decimal netDetayToplam = satirlar.Sum(s => s.NetTutar);
            decimal fark = Yardimcilar.YuvarlaKurus(netToplam - netDetayToplam);
            if (Math.Abs(fark) >= 0.01m)
            {
                SatisSatir hedef = satirlar.OrderByDescending(s => s.NetTutar).FirstOrDefault();
                if (hedef != null)
                {
                    hedef.NetTutar = Yardimcilar.YuvarlaKurus(hedef.NetTutar + fark);
                    hedef.KdvTutar = Yardimcilar.KdvTutarHesapla(hedef.NetTutar, hedef.KdvOrani);
                }
            }

            return satirlar;
        }

        private static void KaydetOdeme(SqlConnection conn, SqlTransaction tran, HashSet<string> odemeKolonlar, int satisId, string odemeTipi, decimal nakit, decimal kart, decimal netToplam)
        {
            List<(string Tip, decimal Tutar)> odemeler = new List<(string Tip, decimal Tutar)>();
            if (nakit > 0)
            {
                odemeler.Add(("Nakit", nakit));
            }
            if (kart > 0)
            {
                odemeler.Add(("Kart", kart));
            }
            if (odemeler.Count == 0)
            {
                odemeler.Add((odemeTipi, netToplam));
            }

            bool odemeIdentity = IdentityKolonVarMi(conn, tran, "tbOdeme", "nOdemeID");
            string odemeIdKolon = IlkKolonuBul(odemeKolonlar, "nOdemeID", "nTahsilatID", "nIslemID");

            foreach (var odeme in odemeler)
            {
                Dictionary<string, object> odemeKaydi = new Dictionary<string, object>
                {
                    ["nAlisverisID"] = satisId,
                    ["dteOdemeTarihi"] = DateTime.Now,
                    ["lTutar"] = odeme.Tutar,
                    ["sOdemeTipi"] = odeme.Tip,
                    ["sAciklama"] = $"Satış Ödeme - {odeme.Tip}"
                };

                if (!odemeIdentity && !string.IsNullOrWhiteSpace(odemeIdKolon))
                {
                    odemeKaydi[odemeIdKolon] = YeniIdUret(conn, tran, "tbOdeme", odemeIdKolon);
                }

                DinamikInsert(conn, tran, "tbOdeme", odemeKaydi, odemeIdentity);
            }
        }

        private static void KaydetNakitKasa(SqlConnection conn, SqlTransaction tran, HashSet<string> nakitKolonlar, int satisId, decimal nakit)
        {
            if (nakit <= 0)
            {
                return;
            }

            bool nakitIdentity = IdentityKolonVarMi(conn, tran, "tbNakitKasa", "nKasaID");
            string nakitIdKolon = IlkKolonuBul(nakitKolonlar, "nKasaID", "nIslemID");

            Dictionary<string, object> nakitKaydi = new Dictionary<string, object>
            {
                ["nAlisverisID"] = satisId,
                ["dteIslemTarihi"] = DateTime.Now,
                ["lTutar"] = nakit,
                ["sIslemTipi"] = "Nakit",
                ["sAciklama"] = "Satış Nakit Tahsilat"
            };

            if (!nakitIdentity && !string.IsNullOrWhiteSpace(nakitIdKolon))
            {
                nakitKaydi[nakitIdKolon] = YeniIdUret(conn, tran, "tbNakitKasa", nakitIdKolon);
            }

            DinamikInsert(conn, tran, "tbNakitKasa", nakitKaydi, nakitIdentity);
        }

        private class SatisSatir
        {
            public int StokId { get; set; }
            public decimal Miktar { get; set; }
            public decimal BirimFiyat { get; set; }
            public decimal BrutTutar { get; set; }
            public decimal SatirIskonto { get; set; }
            public decimal DipIskonto { get; set; }
            public decimal NetTutar { get; set; }
            public decimal KdvOrani { get; set; }
            public decimal KdvTutar { get; set; }
            public string Aciklama { get; set; }
        }

        public static SqlParameter Parametre(string ad, object deger)
        {
            return new SqlParameter(ad, deger ?? DBNull.Value);
        }

        public static List<SqlParameter> ParametreListe(params SqlParameter[] parametreler)
        {
            List<SqlParameter> liste = new List<SqlParameter>();
            if (parametreler != null)
            {
                liste.AddRange(parametreler);
            }
            return liste;
        }
    }
}
