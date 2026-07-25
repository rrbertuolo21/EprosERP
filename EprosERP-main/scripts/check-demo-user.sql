SELECT set_config('app.current_tenant_id', 'tenant-padrao', true);
SELECT email, tenant_id, password_hash, status, deletado_em FROM aplicativo.usuarios WHERE email = 'demo@epros.local';
SELECT COUNT(*) AS vinculos FROM aplicativo.usuarios_empresas ue JOIN aplicativo.usuarios u ON u.id = ue.usuario_id WHERE u.email = 'demo@epros.local';
