SELECT schemaname, tablename FROM pg_tables WHERE tablename LIKE '%fiscal%' OR schemaname LIKE '%fiscal%' OR schemaname = 'dms' ORDER BY 1,2;
SELECT tablename FROM pg_tables WHERE schemaname = 'plataforma' ORDER BY 1 LIMIT 15;
