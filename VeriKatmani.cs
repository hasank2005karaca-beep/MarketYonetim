using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
