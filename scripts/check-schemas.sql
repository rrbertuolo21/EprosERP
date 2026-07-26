SELECT schemaname, COUNT(*) AS tabelas
FROM pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema', 'public')
GROUP BY schemaname
ORDER BY schemaname;
