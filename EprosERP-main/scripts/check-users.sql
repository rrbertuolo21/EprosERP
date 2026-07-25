SELECT email, tenant_id, tipo FROM aplicativo.usuarios WHERE deletado_em IS NULL ORDER BY criado_em;
