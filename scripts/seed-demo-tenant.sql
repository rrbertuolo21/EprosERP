-- Re-seed demo user no tenant padrão (RLS da API usa tenant-padrao em login anônimo)
BEGIN;

SELECT set_config('app.current_tenant_id', 'tenant-padrao', true);

DELETE FROM aplicativo.configuracoes_empresas WHERE email = 'demo@epros.local';
DELETE FROM aplicativo.usuarios_empresas WHERE usuario_id = 'b2222222-2222-2222-2222-222222222222';
DELETE FROM aplicativo.usuarios WHERE email = 'demo@epros.local';
DELETE FROM plataforma.empresas WHERE cnpj = '12345678000195';
DELETE FROM plataforma.pessoa_grupos WHERE id = 'c3333333-3333-3333-3333-333333333333';

INSERT INTO plataforma.pessoa_grupos (
    id, descricao, sync_id, tenant_id, sync_version, criado_em, criado_por
) VALUES (
    'c3333333-3333-3333-3333-333333333333',
    'Grupo Geral - Empresa Demo Ltda',
    'c3333333-3333-3333-3333-333333333334',
    'tenant-padrao',
    1,
    NOW(),
    'seed-local'
);

INSERT INTO plataforma.empresas (
    id, razao_social, nome_fantasia, cnpj,
    regime_tributario, regime_apuracao,
    pessoa_grupo_id, produto_grupo_id, plano_contas_financeiro_id, tributario_grupo_id,
    logradouro, numero, complemento, bairro, cep, cidade, estado,
    ativo, eh_mei, eh_industria, date_format, tipo_configuracao_estoque,
    sync_id, tenant_id, sync_version, criado_em, criado_por
) VALUES (
    'a1111111-1111-1111-1111-111111111111',
    'Empresa Demo Ltda',
    'Empresa Demo Ltda',
    '12345678000195',
    1, 1,
    'c3333333-3333-3333-3333-333333333333',
    'a1111111-1111-1111-1111-111111111112',
    'a1111111-1111-1111-1111-111111111113',
    'a1111111-1111-1111-1111-111111111114',
    'Logradouro Padrão', 'S/N', 'Self Register', 'Bairro Padrão', '00000000', 'Cidade Padrão', 'SP',
    true, false, false, 'DD-MM-YYYY', '',
    'a1111111-1111-1111-1111-111111111115',
    'tenant-padrao',
    1,
    NOW(),
    'seed-local'
);

INSERT INTO aplicativo.usuarios (
    id, nome, email, password_hash,
    mfa_habilitado, status, tipo, forcar_troca_senha, access_failed_count, api_key_rate_limit,
    sync_id, tenant_id, sync_version, criado_em, criado_por
) VALUES (
    'b2222222-2222-2222-2222-222222222222',
    'Administrador Demo',
    'demo@epros.local',
    'pbkdf2.sha256.100000.p9IopO1Rq1cEJeUz850V6g==.yhgZUcYrQQUxWMbkPsV2EP3kE4NAcWepQDBKjKYki58=',
    false, 1, 1, false, 0, 60,
    'b2222222-2222-2222-2222-222222222223',
    'tenant-padrao',
    1,
    NOW(),
    'seed-local'
);

INSERT INTO aplicativo.usuarios_empresas (
    id, tenant_id, usuario_id, empresa_id, eh_admin,
    sync_id, sync_version, criado_em, criado_por
) VALUES (
    'd4444444-4444-4444-4444-444444444444',
    'tenant-padrao',
    'b2222222-2222-2222-2222-222222222222',
    'a1111111-1111-1111-1111-111111111111',
    true,
    'd4444444-4444-4444-4444-444444444445',
    1,
    NOW(),
    'seed-local'
);

INSERT INTO aplicativo.configuracoes_empresas (
    id, tenant_id, empresa_id, nome, email,
    endereco, time_zone_id, date_format, currency_id, vat_percentage, vat_type, currency_position,
    sync_id, sync_version, criado_em, criado_por
) VALUES (
    'e5555555-5555-5555-5555-555555555555',
    'tenant-padrao',
    'a1111111-1111-1111-1111-111111111111',
    'Empresa Demo Ltda',
    'demo@epros.local',
    'Logradouro Padrão, S/N',
    1, 'DD-MM-YYYY', 1, 0, 1, 1,
    'e5555555-5555-5555-5555-555555555556',
    1,
    NOW(),
    'seed-local'
);

COMMIT;

SELECT email, tenant_id FROM aplicativo.usuarios WHERE email = 'demo@epros.local';
