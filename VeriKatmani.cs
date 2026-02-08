using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MarketYonetim
{
    public static class VeriKatmani
    {
        private static SqlConnection BaglantiOlustur()
        {
            return new SqlConnection(Ayarlar.ConnectionString);
        }

        private static bool TabloVarMi(SqlConnection conn, SqlTransaction tran, string tablo)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM sys.tables WHERE name = @tablo", conn, tran))
            {
                cmd.Parameters.AddWithValue("@tablo", tablo);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool KolonIdentityMi(SqlConnection conn, SqlTransaction tran, string tablo, string kolon)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT c.is_identity
                FROM sys.columns c
                INNER JOIN sys.tables t ON c.object_id = t.object_id
                WHERE t.name = @tablo AND c.name = @kolon", conn, tran))
            {
                cmd.Parameters.AddWithValue("@tablo", tablo);
                cmd.Parameters.AddWithValue("@kolon", kolon);
                object sonuc = cmd.ExecuteScalar();
                return sonuc != null && sonuc != DBNull.Value && Convert.ToBoolean(sonuc);
            }
        }

        private static DataTable DataTableGetir(string sql, List<SqlParameter> parametreler)
        {
            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                if (parametreler != null)
                {
                    cmd.Parameters.AddRange(parametreler.ToArray());
                }
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        private static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tran, string sql, List<SqlParameter> parametreler)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                if (parametreler != null)
                {
                    cmd.Parameters.AddRange(parametreler.ToArray());
                }
                return cmd.ExecuteNonQuery();
            }
        }

        private static object ExecuteScalar(SqlConnection conn, SqlTransaction tran, string sql, List<SqlParameter> parametreler)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                if (parametreler != null)
                {
                    cmd.Parameters.AddRange(parametreler.ToArray());
                }
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable Sinif1leriGetir()
        {
            const string sql = @"
                SELECT DISTINCT sSinifKodu1
                FROM tbStokSinifi
                WHERE sSinifKodu1 IS NOT NULL AND sSinifKodu1 <> ''
                ORDER BY sSinifKodu1";
            return DataTableGetir(sql, null);
        }

        public static DataTable Sinif2leriGetir(string sinif1)
        {
            const string sql = @"
                SELECT DISTINCT sSinifKodu2
                FROM tbStokSinifi
                WHERE sSinifKodu1 = @sinif1 AND sSinifKodu2 IS NOT NULL AND sSinifKodu2 <> ''
                ORDER BY sSinifKodu2";
            return DataTableGetir(sql, new List<SqlParameter> { new SqlParameter("@sinif1", sinif1) });
        }

        public static bool StokKoduVarMi(string kodu, int? stokId)
        {
            const string sql = "SELECT TOP 1 nStokID FROM tbStok WHERE sKodu = @kodu AND (@stokId IS NULL OR nStokID <> @stokId)";
            List<SqlParameter> parametreler = new List<SqlParameter>
            {
                new SqlParameter("@kodu", kodu),
                new SqlParameter("@stokId", (object)stokId ?? DBNull.Value)
            };
            DataTable dt = DataTableGetir(sql, parametreler);
            return dt.Rows.Count > 0;
        }

        public static bool BarkodVarMi(string barkod, int? stokId)
        {
            const string sql = "SELECT TOP 1 nStokID FROM tbStokBarkodu WHERE sBarkod = @barkod AND (@stokId IS NULL OR nStokID <> @stokId)";
            List<SqlParameter> parametreler = new List<SqlParameter>
            {
                new SqlParameter("@barkod", barkod),
                new SqlParameter("@stokId", (object)stokId ?? DBNull.Value)
            };
            DataTable dt = DataTableGetir(sql, parametreler);
            return dt.Rows.Count > 0;
        }

        public static int UrunSayisiGetir(
            string arama,
            string barkod,
            string sinif1,
            string sinif2,
            string stokDurum,
            decimal? minFiyat,
            decimal? maxFiyat)
        {
            List<SqlParameter> parametreler = new List<SqlParameter>();
            string sql = @"
                SELECT COUNT(1)
                FROM tbStok s
                LEFT JOIN tbStokSinifi si ON s.nStokID = si.nStokID
                WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(arama) && arama.Length >= 2)
            {
                sql += " AND (s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%')";
                parametreler.Add(new SqlParameter("@arama", arama.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(barkod))
            {
                sql += " AND EXISTS (SELECT 1 FROM tbStokBarkodu b WHERE b.nStokID = s.nStokID AND b.sBarkod = @barkod)";
                parametreler.Add(new SqlParameter("@barkod", barkod.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(sinif1) && sinif1 != "Hepsi")
            {
                sql += " AND si.sSinifKodu1 = @sinif1";
                parametreler.Add(new SqlParameter("@sinif1", sinif1));
            }

            if (!string.IsNullOrWhiteSpace(sinif2) && sinif2 != "Hepsi")
            {
                sql += " AND si.sSinifKodu2 = @sinif2";
                parametreler.Add(new SqlParameter("@sinif2", sinif2));
            }

            if (minFiyat.HasValue || maxFiyat.HasValue)
            {
                sql += @"
                    AND EXISTS (
                        SELECT 1
                        FROM tbStokFiyati f
                        WHERE f.nStokID = s.nStokID
                          AND f.sFiyatTipi = @fiyatTipi
                          AND (@minFiyat IS NULL OR f.lFiyat >= @minFiyat)
                          AND (@maxFiyat IS NULL OR f.lFiyat <= @maxFiyat)
                    )";
                parametreler.Add(new SqlParameter("@fiyatTipi", Ayarlar.VarsayilanFiyatTipi));
                parametreler.Add(new SqlParameter("@minFiyat", (object)minFiyat ?? DBNull.Value));
                parametreler.Add(new SqlParameter("@maxFiyat", (object)maxFiyat ?? DBNull.Value));
            }

            if (!string.IsNullOrWhiteSpace(stokDurum) && stokDurum != "hepsi")
            {
                sql += @"
                    AND EXISTS (
                        SELECT 1
                        FROM (
                            SELECT SUM(ISNULL(fd.lMiktar, 0)) AS StokMiktari
                            FROM tbStokFisiDetayi fd
                            WHERE fd.nStokID = s.nStokID
                        ) st
                        WHERE (@stokDurum = 'var' AND st.StokMiktari > 0)
                           OR (@stokDurum = 'yok' AND st.StokMiktari <= 0)
                           OR (@stokDurum = 'az' AND st.StokMiktari > 0 AND st.StokMiktari < @kritik)
                    )";
                parametreler.Add(new SqlParameter("@stokDurum", stokDurum));
                parametreler.Add(new SqlParameter("@kritik", Ayarlar.KritikStokEsigi));
            }

            using (SqlConnection conn = BaglantiOlustur())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parametreler.Count > 0)
                {
                    cmd.Parameters.AddRange(parametreler.ToArray());
                }
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static DataTable UrunleriGetir(
            string arama,
            string barkod,
            string sinif1,
            string sinif2,
            string stokDurum,
            decimal? minFiyat,
            decimal? maxFiyat,
            int sayfa,
            int sayfaBoyutu = 50)
        {
            List<SqlParameter> parametreler = new List<SqlParameter>();
            string sql = @"
                SELECT
                    s.nStokID,
                    s.sKodu,
                    s.sAciklama,
                    s.sBirimCinsi1 AS sBirimCinsi,
                    ISNULL(b.sBarkod, '') AS Barkod,
                    ISNULL(f.lFiyat, 0) AS Fiyat,
                    ISNULL(st.StokMiktari, 0) AS StokMiktari,
                    ISNULL(si.sSinifKodu1, '') AS Kategori1,
                    ISNULL(si.sSinifKodu2, '') AS Kategori2
                FROM tbStok s
                LEFT JOIN tbStokSinifi si ON s.nStokID = si.nStokID
                OUTER APPLY (
                    SELECT TOP 1 sb.sBarkod
                    FROM tbStokBarkodu sb
                    WHERE sb.nStokID = s.nStokID
                    ORDER BY sb.sBarkod
                ) b
                OUTER APPLY (
                    SELECT TOP 1 sf.lFiyat
                    FROM tbStokFiyati sf
                    WHERE sf.nStokID = s.nStokID AND sf.sFiyatTipi = @fiyatTipi
                ) f
                OUTER APPLY (
                    SELECT SUM(ISNULL(fd.lMiktar, 0)) AS StokMiktari
                    FROM tbStokFisiDetayi fd
                    WHERE fd.nStokID = s.nStokID
                ) st
                WHERE 1=1";

            parametreler.Add(new SqlParameter("@fiyatTipi", Ayarlar.VarsayilanFiyatTipi));

            if (!string.IsNullOrWhiteSpace(arama) && arama.Length >= 2)
            {
                sql += " AND (s.sAciklama LIKE @arama + '%' OR s.sKodu LIKE @arama + '%')";
                parametreler.Add(new SqlParameter("@arama", arama.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(barkod))
            {
                sql += " AND EXISTS (SELECT 1 FROM tbStokBarkodu sb WHERE sb.nStokID = s.nStokID AND sb.sBarkod = @barkod)";
                parametreler.Add(new SqlParameter("@barkod", barkod.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(sinif1) && sinif1 != "Hepsi")
            {
                sql += " AND si.sSinifKodu1 = @sinif1";
                parametreler.Add(new SqlParameter("@sinif1", sinif1));
            }

            if (!string.IsNullOrWhiteSpace(sinif2) && sinif2 != "Hepsi")
            {
                sql += " AND si.sSinifKodu2 = @sinif2";
                parametreler.Add(new SqlParameter("@sinif2", sinif2));
            }

            if (minFiyat.HasValue)
            {
                sql += " AND EXISTS (SELECT 1 FROM tbStokFiyati sf WHERE sf.nStokID = s.nStokID AND sf.sFiyatTipi = @fiyatTipi AND sf.lFiyat >= @minFiyat)";
                parametreler.Add(new SqlParameter("@minFiyat", minFiyat.Value));
            }

            if (maxFiyat.HasValue)
            {
                sql += " AND EXISTS (SELECT 1 FROM tbStokFiyati sf WHERE sf.nStokID = s.nStokID AND sf.sFiyatTipi = @fiyatTipi AND sf.lFiyat <= @maxFiyat)";
                parametreler.Add(new SqlParameter("@maxFiyat", maxFiyat.Value));
            }

            if (!string.IsNullOrWhiteSpace(stokDurum) && stokDurum != "hepsi")
            {
                sql += @"
                    AND (
                        (@stokDurum = 'var' AND ISNULL(st.StokMiktari, 0) > 0)
                        OR (@stokDurum = 'yok' AND ISNULL(st.StokMiktari, 0) <= 0)
                        OR (@stokDurum = 'az' AND ISNULL(st.StokMiktari, 0) > 0 AND ISNULL(st.StokMiktari, 0) < @kritik)
                    )";
                parametreler.Add(new SqlParameter("@stokDurum", stokDurum));
                parametreler.Add(new SqlParameter("@kritik", Ayarlar.KritikStokEsigi));
            }

            sql += " ORDER BY s.nStokID OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
            parametreler.Add(new SqlParameter("@offset", (sayfa - 1) * sayfaBoyutu));
            parametreler.Add(new SqlParameter("@fetch", sayfaBoyutu));

            return DataTableGetir(sql, parametreler);
        }

        public static DataSet UrunDetayGetir(int stokId)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 1 s.nStokID, s.sKodu, s.sAciklama, s.sKisaAdi, s.sBirimCinsi1,
                                 ISNULL(si.sSinifKodu1, '') AS sSinifKodu1,
                                 ISNULL(si.sSinifKodu2, '') AS sSinifKodu2
                    FROM tbStok s
                    LEFT JOIN tbStokSinifi si ON s.nStokID = si.nStokID
                    WHERE s.nStokID = @stokId", conn))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds, "Urun");
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT sBarkod
                    FROM tbStokBarkodu
                    WHERE nStokID = @stokId
                    ORDER BY sBarkod", conn))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds, "Barkodlar");
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT sFiyatTipi, lFiyat
                    FROM tbStokFiyati
                    WHERE nStokID = @stokId
                    ORDER BY sFiyatTipi", conn))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds, "Fiyatlar");
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT SUM(ISNULL(fd.lMiktar, 0)) AS StokMiktari
                    FROM tbStokFisiDetayi fd
                    WHERE fd.nStokID = @stokId", conn))
                {
                    cmd.Parameters.AddWithValue("@stokId", stokId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds, "Stok");
                    }
                }
            }

            return ds;
        }

        public static int UrunEkle(
            string kodu,
            string aciklama,
            string kisaAdi,
            string birimCinsi,
            string sinif1,
            string sinif2,
            List<string> barkodlar,
            Dictionary<string, decimal> fiyatlarByTip)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        bool identity = KolonIdentityMi(conn, tran, "tbStok", "nStokID");
                        int stokId;
                        if (identity)
                        {
                            string insertSql = @"
                                INSERT INTO tbStok (sKodu, sAciklama, sKisaAdi, sBirimCinsi1)
                                VALUES (@kodu, @aciklama, @kisaAdi, @birimCinsi);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                            object sonuc = ExecuteScalar(conn, tran, insertSql, new List<SqlParameter>
                            {
                                new SqlParameter("@kodu", kodu),
                                new SqlParameter("@aciklama", aciklama),
                                new SqlParameter("@kisaAdi", (object)kisaAdi ?? DBNull.Value),
                                new SqlParameter("@birimCinsi", birimCinsi)
                            });
                            stokId = Convert.ToInt32(sonuc);
                        }
                        else
                        {
                            stokId = YeniIdUret(conn, tran, "tbStok", "nStokID");
                            string insertSql = @"
                                INSERT INTO tbStok (nStokID, sKodu, sAciklama, sKisaAdi, sBirimCinsi1)
                                VALUES (@stokId, @kodu, @aciklama, @kisaAdi, @birimCinsi);";
                            ExecuteNonQuery(conn, tran, insertSql, new List<SqlParameter>
                            {
                                new SqlParameter("@stokId", stokId),
                                new SqlParameter("@kodu", kodu),
                                new SqlParameter("@aciklama", aciklama),
                                new SqlParameter("@kisaAdi", (object)kisaAdi ?? DBNull.Value),
                                new SqlParameter("@birimCinsi", birimCinsi)
                            });
                        }

                        if (TabloVarMi(conn, tran, "tbStokSinifi"))
                        {
                            ExecuteNonQuery(conn, tran, @"
                                INSERT INTO tbStokSinifi (nStokID, sSinifKodu1, sSinifKodu2)
                                VALUES (@stokId, @sinif1, @sinif2)", new List<SqlParameter>
                            {
                                new SqlParameter("@stokId", stokId),
                                new SqlParameter("@sinif1", (object)sinif1 ?? DBNull.Value),
                                new SqlParameter("@sinif2", (object)sinif2 ?? DBNull.Value)
                            });
                        }

                        if (barkodlar != null)
                        {
                            foreach (string barkod in barkodlar)
                            {
                                ExecuteNonQuery(conn, tran, @"
                                    INSERT INTO tbStokBarkodu (nStokID, sBarkod)
                                    VALUES (@stokId, @barkod)", new List<SqlParameter>
                                {
                                    new SqlParameter("@stokId", stokId),
                                    new SqlParameter("@barkod", barkod)
                                });
                            }
                        }

                        if (fiyatlarByTip != null)
                        {
                            foreach (KeyValuePair<string, decimal> fiyat in fiyatlarByTip)
                            {
                                ExecuteNonQuery(conn, tran, @"
                                    INSERT INTO tbStokFiyati (nStokID, sFiyatTipi, lFiyat)
                                    VALUES (@stokId, @fiyatTipi, @fiyat)", new List<SqlParameter>
                                {
                                    new SqlParameter("@stokId", stokId),
                                    new SqlParameter("@fiyatTipi", fiyat.Key),
                                    new SqlParameter("@fiyat", fiyat.Value)
                                });
                            }
                        }

                        tran.Commit();
                        return stokId;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void UrunGuncelle(
            int stokId,
            string kodu,
            string aciklama,
            string kisaAdi,
            string birimCinsi,
            string sinif1,
            string sinif2,
            List<string> barkodlar,
            Dictionary<string, decimal> fiyatlarByTip)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(conn, tran, @"
                            UPDATE tbStok
                            SET sKodu = @kodu,
                                sAciklama = @aciklama,
                                sKisaAdi = @kisaAdi,
                                sBirimCinsi1 = @birimCinsi
                            WHERE nStokID = @stokId", new List<SqlParameter>
                        {
                            new SqlParameter("@kodu", kodu),
                            new SqlParameter("@aciklama", aciklama),
                            new SqlParameter("@kisaAdi", (object)kisaAdi ?? DBNull.Value),
                            new SqlParameter("@birimCinsi", birimCinsi),
                            new SqlParameter("@stokId", stokId)
                        });

                        if (TabloVarMi(conn, tran, "tbStokSinifi"))
                        {
                            object sinifVar = ExecuteScalar(conn, tran,
                                "SELECT COUNT(1) FROM tbStokSinifi WHERE nStokID = @stokId",
                                new List<SqlParameter> { new SqlParameter("@stokId", stokId) });

                            if (Convert.ToInt32(sinifVar) > 0)
                            {
                                ExecuteNonQuery(conn, tran, @"
                                    UPDATE tbStokSinifi
                                    SET sSinifKodu1 = @sinif1, sSinifKodu2 = @sinif2
                                    WHERE nStokID = @stokId", new List<SqlParameter>
                                {
                                    new SqlParameter("@sinif1", (object)sinif1 ?? DBNull.Value),
                                    new SqlParameter("@sinif2", (object)sinif2 ?? DBNull.Value),
                                    new SqlParameter("@stokId", stokId)
                                });
                            }
                            else
                            {
                                ExecuteNonQuery(conn, tran, @"
                                    INSERT INTO tbStokSinifi (nStokID, sSinifKodu1, sSinifKodu2)
                                    VALUES (@stokId, @sinif1, @sinif2)", new List<SqlParameter>
                                {
                                    new SqlParameter("@stokId", stokId),
                                    new SqlParameter("@sinif1", (object)sinif1 ?? DBNull.Value),
                                    new SqlParameter("@sinif2", (object)sinif2 ?? DBNull.Value)
                                });
                            }
                        }

                        ExecuteNonQuery(conn, tran, "DELETE FROM tbStokBarkodu WHERE nStokID = @stokId",
                            new List<SqlParameter> { new SqlParameter("@stokId", stokId) });

                        if (barkodlar != null)
                        {
                            foreach (string barkod in barkodlar)
                            {
                                ExecuteNonQuery(conn, tran, @"
                                    INSERT INTO tbStokBarkodu (nStokID, sBarkod)
                                    VALUES (@stokId, @barkod)", new List<SqlParameter>
                                {
                                    new SqlParameter("@stokId", stokId),
                                    new SqlParameter("@barkod", barkod)
                                });
                            }
                        }

                        if (fiyatlarByTip != null)
                        {
                            foreach (KeyValuePair<string, decimal> fiyat in fiyatlarByTip)
                            {
                                object fiyatVar = ExecuteScalar(conn, tran,
                                    "SELECT COUNT(1) FROM tbStokFiyati WHERE nStokID = @stokId AND sFiyatTipi = @fiyatTipi",
                                    new List<SqlParameter>
                                    {
                                        new SqlParameter("@stokId", stokId),
                                        new SqlParameter("@fiyatTipi", fiyat.Key)
                                    });

                                if (Convert.ToInt32(fiyatVar) > 0)
                                {
                                    ExecuteNonQuery(conn, tran, @"
                                        UPDATE tbStokFiyati
                                        SET lFiyat = @fiyat
                                        WHERE nStokID = @stokId AND sFiyatTipi = @fiyatTipi", new List<SqlParameter>
                                    {
                                        new SqlParameter("@stokId", stokId),
                                        new SqlParameter("@fiyatTipi", fiyat.Key),
                                        new SqlParameter("@fiyat", fiyat.Value)
                                    });
                                }
                                else
                                {
                                    ExecuteNonQuery(conn, tran, @"
                                        INSERT INTO tbStokFiyati (nStokID, sFiyatTipi, lFiyat)
                                        VALUES (@stokId, @fiyatTipi, @fiyat)", new List<SqlParameter>
                                    {
                                        new SqlParameter("@stokId", stokId),
                                        new SqlParameter("@fiyatTipi", fiyat.Key),
                                        new SqlParameter("@fiyat", fiyat.Value)
                                    });
                                }
                            }
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void UrunSil(int stokId, bool satisGecmisiVarsaEngelle = true)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        object satisVar = ExecuteScalar(conn, tran, @"
                            SELECT TOP 1 1
                            FROM tbStokFisiDetayi
                            WHERE nStokID = @stokId
                            UNION ALL
                            SELECT TOP 1 1
                            FROM tbAlisVeris
                            WHERE nStokID = @stokId", new List<SqlParameter> { new SqlParameter("@stokId", stokId) });

                        if (satisVar != null && satisGecmisiVarsaEngelle)
                        {
                            throw new InvalidOperationException("Bu ürüne ait satış/alış geçmişi bulunduğu için silme engellendi.");
                        }

                        ExecuteNonQuery(conn, tran, "DELETE FROM tbStokBarkodu WHERE nStokID = @stokId",
                            new List<SqlParameter> { new SqlParameter("@stokId", stokId) });
                        ExecuteNonQuery(conn, tran, "DELETE FROM tbStokFiyati WHERE nStokID = @stokId",
                            new List<SqlParameter> { new SqlParameter("@stokId", stokId) });

                        if (TabloVarMi(conn, tran, "tbStokSinifi"))
                        {
                            ExecuteNonQuery(conn, tran, "DELETE FROM tbStokSinifi WHERE nStokID = @stokId",
                                new List<SqlParameter> { new SqlParameter("@stokId", stokId) });
                        }

                        ExecuteNonQuery(conn, tran, "DELETE FROM tbStok WHERE nStokID = @stokId",
                            new List<SqlParameter> { new SqlParameter("@stokId", stokId) });

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static DataTable BarkodlariGetir(int stokId)
        {
            const string sql = "SELECT sBarkod FROM tbStokBarkodu WHERE nStokID = @stokId ORDER BY sBarkod";
            return DataTableGetir(sql, new List<SqlParameter> { new SqlParameter("@stokId", stokId) });
        }

        public static void BarkodEkle(int stokId, string barkod)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(conn, tran, @"
                            INSERT INTO tbStokBarkodu (nStokID, sBarkod)
                            VALUES (@stokId, @barkod)", new List<SqlParameter>
                        {
                            new SqlParameter("@stokId", stokId),
                            new SqlParameter("@barkod", barkod)
                        });
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void BarkodSil(int stokId, string barkod)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(conn, tran, "DELETE FROM tbStokBarkodu WHERE nStokID = @stokId AND sBarkod = @barkod",
                            new List<SqlParameter>
                            {
                                new SqlParameter("@stokId", stokId),
                                new SqlParameter("@barkod", barkod)
                            });
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static DataTable FiyatlariGetir(int stokId)
        {
            const string sql = "SELECT sFiyatTipi, lFiyat FROM tbStokFiyati WHERE nStokID = @stokId ORDER BY sFiyatTipi";
            return DataTableGetir(sql, new List<SqlParameter> { new SqlParameter("@stokId", stokId) });
        }

        public static void FiyatGuncelle(int stokId, string fiyatTipi, decimal yeniFiyat)
        {
            using (SqlConnection conn = BaglantiOlustur())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        object fiyatVar = ExecuteScalar(conn, tran,
                            "SELECT COUNT(1) FROM tbStokFiyati WHERE nStokID = @stokId AND sFiyatTipi = @fiyatTipi",
                            new List<SqlParameter>
                            {
                                new SqlParameter("@stokId", stokId),
                                new SqlParameter("@fiyatTipi", fiyatTipi)
                            });

                        if (Convert.ToInt32(fiyatVar) > 0)
                        {
                            ExecuteNonQuery(conn, tran, @"
                                UPDATE tbStokFiyati
                                SET lFiyat = @fiyat
                                WHERE nStokID = @stokId AND sFiyatTipi = @fiyatTipi", new List<SqlParameter>
                            {
                                new SqlParameter("@stokId", stokId),
                                new SqlParameter("@fiyatTipi", fiyatTipi),
                                new SqlParameter("@fiyat", yeniFiyat)
                            });
                        }
                        else
                        {
                            ExecuteNonQuery(conn, tran, @"
                                INSERT INTO tbStokFiyati (nStokID, sFiyatTipi, lFiyat)
                                VALUES (@stokId, @fiyatTipi, @fiyat)", new List<SqlParameter>
                            {
                                new SqlParameter("@stokId", stokId),
                                new SqlParameter("@fiyatTipi", fiyatTipi),
                                new SqlParameter("@fiyat", yeniFiyat)
                            });
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static int YeniIdUret(SqlConnection conn, SqlTransaction tran, string tablo, string kolon)
        {
            object sonuc = ExecuteScalar(conn, tran, $"SELECT ISNULL(MAX({kolon}), 0) + 1 FROM {tablo}", null);
            return Convert.ToInt32(sonuc);
        }
    }
}
