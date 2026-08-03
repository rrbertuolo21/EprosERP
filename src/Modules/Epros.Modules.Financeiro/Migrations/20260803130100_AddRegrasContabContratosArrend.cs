using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Financeiro.Migrations
{
    /// <summary>
    /// Cria as tabelas que estavam no modelo/snapshot mas cuja migration CreateTable nunca foi
    /// gerada (drift Classe A) — endpoints retornavam 500 por relacao inexistente e o smoke-from-zero
    /// nao subia. DDL extraido do proprio modelo (Database.GenerateCreateScript), portanto fiel as
    /// configuracoes de entidade. Sem xmin fisico (o modelo usa xmin de sistema do Postgres).
    /// </summary>
    public partial class AddRegrasContabContratosArrend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE financas.contratos_arrendamento (
    id uuid NOT NULL,
    descricao character varying(255) NOT NULL,
    pessoa_id uuid,
    data_inicio timestamp with time zone NOT NULL,
    valor_contraprestacao numeric(18,2) NOT NULL,
    quantidade_parcelas integer NOT NULL,
    taxa_incremental_periodo numeric(18,2) NOT NULL,
    pagamento_antecipado boolean NOT NULL,
    custos_diretos_iniciais numeric(18,2) NOT NULL,
    incentivos_recebidos numeric(18,2) NOT NULL,
    passivo_arrendamento_inicial numeric(18,2) NOT NULL,
    direito_de_uso_inicial numeric(18,2) NOT NULL,
    status integer NOT NULL,
    motivo_encerramento character varying(500),
    data_encerramento timestamp with time zone,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_contratos_arrendamento PRIMARY KEY (id)
);


CREATE TABLE financas.regras_contabilizacao (
    id uuid NOT NULL,
    tipo_evento character varying(80) NOT NULL,
    conta_debito_id uuid,
    conta_credito_id uuid,
    historico character varying(255),
    ativo boolean NOT NULL,
    sync_id uuid NOT NULL,
    tenant_id text NOT NULL,
    sync_version integer NOT NULL,
    criado_em timestamp with time zone NOT NULL,
    alterado_em timestamp with time zone,
    deletado_em timestamp with time zone,
    criado_por text,
    alterado_por text,
    CONSTRAINT p_k_regras_contabilizacao PRIMARY KEY (id)
);


CREATE UNIQUE INDEX ix__contrato_arrendamento_sync_id ON financas.contratos_arrendamento (sync_id);


CREATE INDEX ix__contrato_arrendamento_tenant_id ON financas.contratos_arrendamento (tenant_id);


CREATE INDEX ix_contrato_arrendamento_tenant_status ON financas.contratos_arrendamento (tenant_id, status);


CREATE UNIQUE INDEX ix__regra_contabilizacao_sync_id ON financas.regras_contabilizacao (sync_id);


CREATE INDEX ix__regra_contabilizacao_tenant_id ON financas.regras_contabilizacao (tenant_id);


CREATE INDEX ix_regra_contabilizacao_tenant_evento ON financas.regras_contabilizacao (tenant_id, tipo_evento);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS financas.regras_contabilizacao CASCADE;
                DROP TABLE IF EXISTS financas.contratos_arrendamento CASCADE;
");
        }
    }
}
