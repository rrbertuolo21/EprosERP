-- Corrige histórico EF quando tabelas já existem mas a migration não foi registrada
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
  ('20260626203206_InitialCreate', '8.0.11'),
  ('20260627123503_InitialCreate', '8.0.11'),
  ('20260627131449_InitialCreate', '8.0.11'),
  ('20260627133457_InitialCreate', '8.0.11'),
  ('20260627134731_InitialCreate', '8.0.11'),
  ('20260627141716_InitialCreate', '8.0.11'),
  ('20260627144625_InitialCreate', '8.0.11'),
  ('20260627145639_InitialCreate', '8.0.11')
ON CONFLICT DO NOTHING;

SELECT "MigrationId" FROM "__EFMigrationsHistory" WHERE "MigrationId" LIKE '2026062%' OR "MigrationId" LIKE '202606271%' ORDER BY 1;
