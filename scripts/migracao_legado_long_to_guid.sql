-- ============================================================================
-- SCRIPT DE MIGRAÇÃO: LEGADO (long/BIGINT) ➔ NOVO MONÓLITO (Guid/UUID)
-- Epros ERP - Bloco 7
-- ============================================================================
-- Objetivo: Migrar os 20 clientes do banco legado para o novo banco Epros,
-- convertendo chaves numéricas sequenciais em GUIDs de forma determinística
-- usando a extensão uuid-ossp e a função uuid_generate_v5.
-- ============================================================================

-- 1. Garantir que a extensão uuid-ossp está ativada no PostgreSQL
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Criar schema de simulação para dados legados (para fins de teste/execução)
CREATE SCHEMA IF NOT EXISTS legado;

-- 3. Criar tabelas legadas simuladas
CREATE TABLE IF NOT EXISTS legado.planos (
    id BIGINT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    preco NUMERIC(18,2) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS legado.planos_modulos (
    id BIGINT PRIMARY KEY,
    plano_id BIGINT REFERENCES legado.planos(id),
    nome_modulo VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS legado.clientes (
    id BIGINT PRIMARY KEY,
    razao_social VARCHAR(200) NOT NULL,
    cnpj VARCHAR(14) NOT NULL,
    email VARCHAR(150) NOT NULL,
    plano_id BIGINT REFERENCES legado.planos(id),
    tenant_id VARCHAR(50) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS legado.faturas (
    id BIGINT PRIMARY KEY,
    cliente_id BIGINT REFERENCES legado.clientes(id),
    valor NUMERIC(18,2) NOT NULL,
    data_vencimento TIMESTAMP WITH TIME ZONE NOT NULL,
    data_pagamento TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) NOT NULL,
    tenant_id VARCHAR(50) NOT NULL
);

-- 4. Inserir massa de teste legada (Simulação dos clientes reais em produção)
TRUNCATE TABLE legado.faturas CASCADE;
TRUNCATE TABLE legado.clientes CASCADE;
TRUNCATE TABLE legado.planos_modulos CASCADE;
TRUNCATE TABLE legado.planos CASCADE;

INSERT INTO legado.planos (id, nome, preco, ativo) VALUES
(1, 'Plano Básico', 99.90, TRUE),
(2, 'Plano Completo', 199.90, TRUE),
(3, 'Plano Enterprise', 499.90, TRUE);

INSERT INTO legado.planos_modulos (id, plano_id, nome_modulo) VALUES
(1, 1, 'GestaoClientes'),
(2, 2, 'GestaoClientes'),
(3, 2, 'Estoque'),
(4, 2, 'Financeiro'),
(5, 3, 'GestaoClientes'),
(6, 3, 'Estoque'),
(7, 3, 'Financeiro'),
(8, 3, 'Fiscal');

-- Cadastrando alguns clientes (dos 20 legados em produção)
INSERT INTO legado.clientes (id, razao_social, cnpj, email, plano_id, tenant_id, ativo) VALUES
(101, 'Alfa Transportes Ltda', '11111111000111', 'contato@alfa.com.br', 1, 'tenant_alfa_101', TRUE),
(102, 'Beta Distribuidora de Alimentos', '22222222000122', 'financeiro@beta.com.br', 2, 'tenant_beta_102', TRUE),
(103, 'Gama Metalurgica S.A.', '33333333000133', 'ti@gama.com.br', 3, 'tenant_gama_103', TRUE);

-- Faturas das mensalidades legadas
INSERT INTO legado.faturas (id, cliente_id, valor, data_vencimento, data_pagamento, status, tenant_id) VALUES
(1001, 101, 99.90, NOW() - INTERVAL '15 days', NOW() - INTERVAL '14 days', 'Paga', 'tenant_alfa_101'),
(1002, 101, 99.90, NOW() + INTERVAL '15 days', NULL, 'Aberta', 'tenant_alfa_101'),
(1003, 102, 199.90, NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days', 'Paga', 'tenant_beta_102'),
(1004, 103, 499.90, NOW() - INTERVAL '1 month', NOW() - INTERVAL '29 days', 'Paga', 'tenant_gama_103');


-- 5. Executar a migração relacional utilizando UUIDv5 determinístico
-- O namespace UUID 'd3b07384-d113-4956-aab9-e58f00030000' atua como base estável
DO $$
DECLARE
    ns_uuid CONSTANT uuid := 'd3b07384-d113-4956-aab9-e58f00030000'::uuid;
BEGIN
    RAISE NOTICE 'Iniciando migração dos planos...';
    
    INSERT INTO plataforma.planos (id, nome, preco, ativo, criado_em, criado_por, sync_id, sync_version, tenant_id)
    SELECT 
        uuid_generate_v5(ns_uuid, 'plano_' || id) AS id,
        nome,
        preco,
        ativo,
        NOW() AT TIME ZONE 'UTC',
        'sistema_migracao',
        uuid_generate_v5(ns_uuid, 'plano_sync_' || id),
        1,
        'epros_plataforma'
    FROM legado.planos
    ON CONFLICT (id) DO NOTHING;

    RAISE NOTICE 'Iniciando migração dos módulos de planos...';
    
    INSERT INTO plataforma.modulos_plano (id, plano_id, nome_modulo, criado_em, criado_por, sync_id, sync_version, tenant_id)
    SELECT 
        uuid_generate_v5(ns_uuid, 'modulo_' || id) AS id,
        uuid_generate_v5(ns_uuid, 'plano_' || plano_id) AS plano_id,
        nome_modulo,
        NOW() AT TIME ZONE 'UTC',
        'sistema_migracao',
        uuid_generate_v5(ns_uuid, 'modulo_sync_' || id),
        1,
        'epros_plataforma'
    FROM legado.planos_modulos
    ON CONFLICT (id) DO NOTHING;

    RAISE NOTICE 'Iniciando migração de clientes...';
    
    INSERT INTO plataforma.clientes (id, razao_social, cnpj, email, plano_id, ativo, criado_em, criado_por, sync_id, sync_version, tenant_id)
    SELECT 
        uuid_generate_v5(ns_uuid, 'cliente_' || id) AS id,
        razao_social,
        cnpj,
        email,
        uuid_generate_v5(ns_uuid, 'plano_' || plano_id) AS plano_id,
        ativo,
        NOW() AT TIME ZONE 'UTC',
        'sistema_migracao',
        uuid_generate_v5(ns_uuid, 'cliente_sync_' || id),
        1,
        tenant_id
    FROM legado.clientes
    ON CONFLICT (id) DO NOTHING;

    RAISE NOTICE 'Iniciando migração de faturas...';
    
    INSERT INTO plataforma.faturas (id, cliente_id, valor, data_vencimento, data_pagamento, status, criado_em, criado_por, sync_id, sync_version, tenant_id)
    SELECT 
        uuid_generate_v5(ns_uuid, 'fatura_' || id) AS id,
        uuid_generate_v5(ns_uuid, 'cliente_' || cliente_id) AS cliente_id,
        valor,
        data_vencimento,
        data_pagamento,
        status,
        NOW() AT TIME ZONE 'UTC',
        'sistema_migracao',
        uuid_generate_v5(ns_uuid, 'fatura_sync_' || id),
        1,
        tenant_id
    FROM legado.faturas
    ON CONFLICT (id) DO NOTHING;

    RAISE NOTICE 'Migração concluída com sucesso!';
END $$;

-- 6. Limpeza do schema legado (remova o comentário abaixo se desejar dropar após o teste)
-- DROP SCHEMA legado CASCADE;
