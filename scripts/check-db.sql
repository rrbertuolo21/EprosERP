SELECT COUNT(*) AS migrations FROM "__EFMigrationsHistory";
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1;
SELECT schemaname, tablename FROM pg_tables WHERE schemaname = 'qualidade';
SELECT is_completed FROM aplicativo.installation_state;
