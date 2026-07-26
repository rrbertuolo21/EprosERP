-- Políticas RLS para lookup cross-tenant em endpoints públicos de autenticação.
DROP POLICY IF EXISTS auth_cross_tenant_select ON aplicativo.usuarios;
CREATE POLICY auth_cross_tenant_select ON aplicativo.usuarios
  FOR SELECT
  USING (current_setting('app.allow_cross_tenant_auth', true) = 'true');

DROP POLICY IF EXISTS auth_cross_tenant_select ON plataforma.empresas;
CREATE POLICY auth_cross_tenant_select ON plataforma.empresas
  FOR SELECT
  USING (current_setting('app.allow_cross_tenant_auth', true) = 'true');
