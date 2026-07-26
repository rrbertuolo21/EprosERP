using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Histórico do XML importado (NF-e de entrada/saída). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Importacoes.ImportacaoXml. A FK legada long EmpresaId vira Guid? EmpresaId
    /// (cross-module por Guid, sem navegação de EF para Empresa).
    /// </summary>
    public class ImportacaoXml : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public string Xml { get; private set; } = string.Empty;
        public ETipoXml TipoDeXml { get; private set; }
        public string NfeId { get; private set; } = string.Empty;
        public EStatusProcessamento StatusImportacaoXml { get; private set; }
        public string? MensagemErroImportacaoXml { get; private set; }
        public EStatusProcessamento StatusCadastro { get; private set; }
        public string? MensagemErroCadastro { get; private set; }
        public EStatusProcessamento StatusSalvarPdf { get; private set; }
        public string? MensagemErroSalvarPdf { get; private set; }
        public int CodigoSefaz { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;

        protected ImportacaoXml() { } // EF Core

        public ImportacaoXml(Guid? empresaId, string xml, ETipoXml tipoDeXml, string nfeId, string? mensagemErro, int codigoSefaz, string tipoEvento, string tenantId, string criadoPor, EStatusProcessamento statusImportacaoXml = EStatusProcessamento.Finalizado, EStatusProcessamento statusCadastro = EStatusProcessamento.NaoProcessado, EStatusProcessamento statusSalvarPdf = EStatusProcessamento.NaoProcessado)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Xml = xml;
            TipoDeXml = tipoDeXml;
            NfeId = nfeId;
            StatusImportacaoXml = statusImportacaoXml;
            StatusCadastro = statusCadastro;
            StatusSalvarPdf = statusSalvarPdf;
            MensagemErroImportacaoXml = mensagemErro;
            CodigoSefaz = codigoSefaz;
            TipoEvento = tipoEvento;
        }

        public void AdicionarMensagemErroImportacaoXml(string mensagemErro)
        {
            StatusImportacaoXml = EStatusProcessamento.Erro;
            MensagemErroImportacaoXml = mensagemErro;
        }

        public void IniciarCadastro()
        {
            StatusCadastro = EStatusProcessamento.Processando;
        }

        public void FinalizarCadastro()
        {
            StatusCadastro = EStatusProcessamento.Finalizado;
        }

        public void ReiniciarCadastro()
        {
            StatusCadastro = EStatusProcessamento.NaoProcessado;
        }

        public void AdicionarMensagemErroCadastro(string mensagemErro)
        {
            StatusCadastro = EStatusProcessamento.Erro;
            MensagemErroCadastro = mensagemErro;
        }

        public void IniciarSalvarPdf()
        {
            StatusSalvarPdf = EStatusProcessamento.Processando;
        }

        public void FinalizarSalvarPdf(string? mensagemErro)
        {
            StatusSalvarPdf = EStatusProcessamento.Finalizado;
            MensagemErroSalvarPdf = mensagemErro;
        }

        public void AdicionarMensagemErroSalvarPdf(string mensagemErro)
        {
            StatusSalvarPdf = EStatusProcessamento.Erro;
            MensagemErroSalvarPdf = mensagemErro;
        }

        public void ReiniciarSalvarPdf()
        {
            StatusSalvarPdf = EStatusProcessamento.NaoProcessado;
        }

        public void CancelarXml(int codigo, string tipoEvento)
        {
            CodigoSefaz = codigo;
            TipoEvento = tipoEvento;
        }
    }
}
