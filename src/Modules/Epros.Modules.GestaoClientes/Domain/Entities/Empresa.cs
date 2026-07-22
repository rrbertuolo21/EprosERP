using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Empresa : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (long) — exceção oficial de porte (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }
        public string RazaoSocial { get; private set; } = string.Empty;
        public string? NomeFantasia { get; private set; }
        public string Cnpj { get; private set; } = string.Empty;
        public string? Cpf { get; private set; }
        public string? InscricaoEstadual { get; private set; }
        public string? InscricaoMunicipal { get; private set; }
        public string? InscricaoSuframa { get; private set; }
        public string? Cnae { get; private set; }
        public RegimeTributario RegimeTributario { get; private set; }
        public RegimeApuracao RegimeApuracao { get; private set; }
        public bool EhMei { get; private set; }
        // Campos portados fielmente do legado (Empresa) ausentes na versão anterior.
        public bool EhIndustria { get; private set; }
        public Guid? ContadorId { get; private set; }
        public ETipoConfiguracaoEstoque TipoConfiguracaoEstoque { get; private set; }

        public Guid? PessoaGrupoId { get; private set; }
        public Guid? ProdutoGrupoId { get; private set; }
        public Guid? PlanoContasFinanceiroId { get; private set; }
        public Guid? TributarioGrupoId { get; private set; }
        public Guid? NcmTributacaoId { get; private set; }
        public Guid? CertificadoDigitalId { get; private set; }
        public Guid? EmpresaParametrosDfeId { get; private set; }

        public string? LinkWebApiAppVendas { get; private set; }
        public string? TokenMercadoPagoPix { get; private set; }
        public string? Logo { get; private set; }
        public string DateFormat { get; private set; } = "MM-DD-YYYY";
        public Guid? TimeZoneId { get; private set; }
        public Guid? CurrencyId { get; private set; }

        public FusoHorario? TimeZone { get; private set; }
        public Moeda? Currency { get; private set; }

        public Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco Endereco { get; private set; } = null!;
        public bool Ativo { get; private set; }

        // Coleções e agregados portados fielmente do legado (Empresa).
        public EmpresaParametrosDfe? EmpresaParametrosDfe { get; private set; }
        private readonly List<EmpresaContato> _contatos = new();
        public IReadOnlyCollection<EmpresaContato> Contatos => _contatos.AsReadOnly();
        private readonly List<IeSt> _ieSts = new();
        public IReadOnlyCollection<IeSt> IeSts => _ieSts.AsReadOnly();

        protected Empresa() { } // EF Core

        public Empresa(
            string razaoSocial,
            string? nomeFantasia,
            string cnpj,
            string? inscricaoEstadual,
            string? inscricaoMunicipal,
            string? inscricaoSuframa,
            string? cnae,
            RegimeTributario regimeTributario,
            RegimeApuracao regimeApuracao,
            Guid? pessoaGrupoId,
            Guid? produtoGrupoId,
            Guid? planoContasFinanceiroId,
            Guid? tributarioGrupoId,
            Guid? ncmTributacaoId,
            Guid? certificadoDigitalId,
            Guid? empresaParametrosDfeId,
            string? linkWebApiAppVendas,
            string? tokenMercadoPagoPix,
            string? logo,
            Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco endereco,
            string tenantId,
            string criadoPor,
            bool ehMei = false,
            string dateFormat = "MM-DD-YYYY",
            Guid? timeZoneId = null,
            Guid? currencyId = null,
            string? cpf = null,
            bool ehIndustria = false,
            Guid? contadorId = null,
            ETipoConfiguracaoEstoque tipoConfiguracaoEstoque = ETipoConfiguracaoEstoque.NaoBloquear)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Empresa>()
                .Requires()
                .IsNotNullOrEmpty(razaoSocial, nameof(RazaoSocial), "Razão Social é obrigatória")
                .IsNotNullOrEmpty(cnpj, nameof(Cnpj), "CNPJ é obrigatório")
                .IsNotNull(endereco, nameof(Endereco), "Endereço é obrigatório")
                .HasMaxLen(razaoSocial, 250, nameof(RazaoSocial), "Razão Social deve ter no máximo 250 caracteres")
                .HasMaxLen(nomeFantasia ?? string.Empty, 250, nameof(NomeFantasia), "Nome Fantasia deve ter no máximo 250 caracteres")
                .HasMaxLen(inscricaoEstadual ?? string.Empty, 20, nameof(InscricaoEstadual), "Inscrição Estadual deve ter no máximo 20 caracteres")
                .HasMaxLen(inscricaoMunicipal ?? string.Empty, 20, nameof(InscricaoMunicipal), "Inscrição Municipal deve ter no máximo 20 caracteres")
                .HasMaxLen(inscricaoSuframa ?? string.Empty, 20, nameof(InscricaoSuframa), "Inscrição Suframa deve ter no máximo 20 caracteres")
                .HasMaxLen(linkWebApiAppVendas ?? string.Empty, 500, nameof(LinkWebApiAppVendas), "Link Web API App Vendas deve ter no máximo 500 caracteres")
                .HasMaxLen(tokenMercadoPagoPix ?? string.Empty, 500, nameof(TokenMercadoPagoPix), "Token Mercado Pago Pix deve ter no máximo 500 caracteres")
                .HasMaxLen(logo ?? string.Empty, 500, nameof(Logo), "Logo deve ter no máximo 500 caracteres")
            );

            // Validar CNPJ
            var cnpjVo = new Cnpj(cnpj);
            if (!cnpjVo.IsValid)
            {
                AddNotifications(cnpjVo.Notifications);
            }

            if (endereco != null)
            {
                AddNotifications(endereco.Notifications);
            }

            RazaoSocial = razaoSocial;
            NomeFantasia = nomeFantasia;
            Cnpj = cnpjVo.Valor;
            InscricaoEstadual = inscricaoEstadual;
            InscricaoMunicipal = inscricaoMunicipal;
            InscricaoSuframa = inscricaoSuframa;
            Cnae = cnae;
            RegimeTributario = regimeTributario;
            RegimeApuracao = regimeApuracao;
            EhMei = ehMei;
            PessoaGrupoId = pessoaGrupoId;
            ProdutoGrupoId = produtoGrupoId;
            PlanoContasFinanceiroId = planoContasFinanceiroId;
            TributarioGrupoId = tributarioGrupoId;
            NcmTributacaoId = ncmTributacaoId;
            CertificadoDigitalId = certificadoDigitalId;
            EmpresaParametrosDfeId = empresaParametrosDfeId;
            LinkWebApiAppVendas = linkWebApiAppVendas;
            TokenMercadoPagoPix = tokenMercadoPagoPix;
            Logo = logo;
            Endereco = endereco!;
            DateFormat = string.IsNullOrEmpty(dateFormat) ? "MM-DD-YYYY" : dateFormat;
            TimeZoneId = timeZoneId;
            CurrencyId = currencyId;
            Cpf = cpf;
            EhIndustria = ehIndustria;
            ContadorId = contadorId;
            TipoConfiguracaoEstoque = tipoConfiguracaoEstoque;
            Ativo = true;
        }

        public void Atualizar(
            string razaoSocial,
            string? nomeFantasia,
            string? inscricaoEstadual,
            string? inscricaoMunicipal,
            string? inscricaoSuframa,
            string? cnae,
            RegimeTributario regimeTributario,
            RegimeApuracao regimeApuracao,
            Guid? pessoaGrupoId,
            Guid? produtoGrupoId,
            Guid? planoContasFinanceiroId,
            Guid? tributarioGrupoId,
            Guid? ncmTributacaoId,
            Guid? certificadoDigitalId,
            Guid? empresaParametrosDfeId,
            string? linkWebApiAppVendas,
            string? tokenMercadoPagoPix,
            string? logo,
            Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco endereco,
            string alteradoPor,
            bool ehMei = false,
            string dateFormat = "MM-DD-YYYY",
            Guid? timeZoneId = null,
            Guid? currencyId = null,
            string? cpf = null,
            bool ehIndustria = false,
            Guid? contadorId = null,
            ETipoConfiguracaoEstoque tipoConfiguracaoEstoque = ETipoConfiguracaoEstoque.NaoBloquear)
        {
            AddNotifications(new Contract<Empresa>()
                .Requires()
                .IsNotNullOrEmpty(razaoSocial, nameof(RazaoSocial), "Razão Social é obrigatória")
                .IsNotNull(endereco, nameof(Endereco), "Endereço é obrigatório")
                .HasMaxLen(razaoSocial, 250, nameof(RazaoSocial), "Razão Social deve ter no máximo 250 caracteres")
                .HasMaxLen(nomeFantasia ?? string.Empty, 250, nameof(NomeFantasia), "Nome Fantasia deve ter no máximo 250 caracteres")
                .HasMaxLen(inscricaoEstadual ?? string.Empty, 20, nameof(InscricaoEstadual), "Inscrição Estadual deve ter no máximo 20 caracteres")
                .HasMaxLen(inscricaoMunicipal ?? string.Empty, 20, nameof(InscricaoMunicipal), "Inscrição Municipal deve ter no máximo 20 caracteres")
                .HasMaxLen(inscricaoSuframa ?? string.Empty, 20, nameof(InscricaoSuframa), "Inscrição Suframa deve ter no máximo 20 caracteres")
                .HasMaxLen(linkWebApiAppVendas ?? string.Empty, 500, nameof(LinkWebApiAppVendas), "Link Web API App Vendas deve ter no máximo 500 caracteres")
                .HasMaxLen(tokenMercadoPagoPix ?? string.Empty, 500, nameof(TokenMercadoPagoPix), "Token Mercado Pago Pix deve ter no máximo 500 caracteres")
                .HasMaxLen(logo ?? string.Empty, 500, nameof(Logo), "Logo deve ter no máximo 500 caracteres")
            );

            if (endereco != null)
            {
                AddNotifications(endereco.Notifications);
            }

            if (IsValid)
            {
                RazaoSocial = razaoSocial;
                NomeFantasia = nomeFantasia;
                InscricaoEstadual = inscricaoEstadual;
                InscricaoMunicipal = inscricaoMunicipal;
                InscricaoSuframa = inscricaoSuframa;
                Cnae = cnae;
                RegimeTributario = regimeTributario;
                RegimeApuracao = regimeApuracao;
                EhMei = ehMei;
                PessoaGrupoId = pessoaGrupoId;
                ProdutoGrupoId = produtoGrupoId;
                PlanoContasFinanceiroId = planoContasFinanceiroId;
                TributarioGrupoId = tributarioGrupoId;
                NcmTributacaoId = ncmTributacaoId;
                CertificadoDigitalId = certificadoDigitalId;
                EmpresaParametrosDfeId = empresaParametrosDfeId;
                LinkWebApiAppVendas = linkWebApiAppVendas;
                TokenMercadoPagoPix = tokenMercadoPagoPix;
                Logo = logo;
                Endereco = endereco!;
                DateFormat = string.IsNullOrEmpty(dateFormat) ? "MM-DD-YYYY" : dateFormat;
                TimeZoneId = timeZoneId;
                CurrencyId = currencyId;
                Cpf = cpf;
                EhIndustria = ehIndustria;
                ContadorId = contadorId;
                TipoConfiguracaoEstoque = tipoConfiguracaoEstoque;
                MarcarAlterado(alteradoPor);
            }
        }

        public void AlterarLogo(string? logo, string alteradoPor)
        {
            Logo = logo;
            MarcarAlterado(alteradoPor);
        }

        // ---- Métodos de negócio portados fielmente do legado (Empresa) ----

        public void AdicionaContato(string nome, string? email, ETipoContatoTelefonico tipo, string? numero, string alteradoPor)
        {
            var contato = new EmpresaContato(Id, nome, email, tipo, numero, TenantId, alteradoPor);
            if (!contato.IsValid)
            {
                AddNotifications(contato.Notifications);
                return;
            }
            _contatos.Add(contato);
            MarcarAlterado(alteradoPor);
        }

        public void AlterarContato(Guid empresaContatoId, string nome, string? email, ETipoContatoTelefonico tipo, string? numero, string alteradoPor)
        {
            var localizado = _contatos.FirstOrDefault(x => x.Id == empresaContatoId);
            localizado?.Alterar(nome, email, tipo, numero, alteradoPor);

            if (localizado != null && !localizado.IsValid)
                AddNotifications(localizado.Notifications);
            else
                MarcarAlterado(alteradoPor);
        }

        public void DeletarContato(Guid empresaContatoId, string alteradoPor)
        {
            var localizado = _contatos.FirstOrDefault(x => x.Id == empresaContatoId);
            localizado?.Deletar(alteradoPor);
            MarcarAlterado(alteradoPor);
        }

        public void AdicionaIeSt(EEstado uf, string ie, string alteradoPor)
        {
            var iest = new IeSt(Id, uf, ie, TenantId, alteradoPor);
            if (!iest.IsValid)
            {
                AddNotifications(iest.Notifications);
                return;
            }
            _ieSts.Add(iest);
            MarcarAlterado(alteradoPor);
        }

        public void AlterarIeSt(Guid ieStId, EEstado uf, string ie, string alteradoPor)
        {
            var localizado = _ieSts.FirstOrDefault(x => x.Id == ieStId);
            localizado?.Alterar(uf, ie, alteradoPor);

            if (localizado != null && !localizado.IsValid)
                AddNotifications(localizado.Notifications);
            else
                MarcarAlterado(alteradoPor);
        }

        public void DeletarIeSt(Guid ieStId, string alteradoPor)
        {
            var localizado = _ieSts.FirstOrDefault(x => x.Id == ieStId);
            localizado?.Deletar(alteradoPor);
            MarcarAlterado(alteradoPor);
        }

        public void IncluirEmpresaParametrosDFe(bool destacarIcmsSt, ParametrosDfeNfe? nfe, ParametrosDfeNfceHomologacao? nfceHomologacao, ParametrosDfeNfceProducao? nfceProducao, ETipoAmbiente tipoAmbienteNfce, ETipoAmbiente tipoAmbienteNfe, string alteradoPor)
        {
            EmpresaParametrosDfe = new EmpresaParametrosDfe(Id, destacarIcmsSt, nfe, nfceHomologacao, nfceProducao, tipoAmbienteNfce, tipoAmbienteNfe, TenantId, alteradoPor);
            EmpresaParametrosDfeId = EmpresaParametrosDfe.Id;

            if (!EmpresaParametrosDfe.IsValid)
                AddNotifications(EmpresaParametrosDfe.Notifications);
            else
                MarcarAlterado(alteradoPor);
        }

        public void AlterarEmpresaParametrosDFe(bool destacarIcmsSt, ParametrosDfeNfe? nfe, ParametrosDfeNfceHomologacao? nfceHomologacao, ParametrosDfeNfceProducao? nfceProducao, ETipoAmbiente tipoAmbienteNfce, ETipoAmbiente tipoAmbienteNfe, string alteradoPor)
        {
            if (EmpresaParametrosDfe == null)
            {
                IncluirEmpresaParametrosDFe(destacarIcmsSt, nfe, nfceHomologacao, nfceProducao, tipoAmbienteNfce, tipoAmbienteNfe, alteradoPor);
                return;
            }

            EmpresaParametrosDfe.Alterar(destacarIcmsSt, nfe, nfceHomologacao, nfceProducao, tipoAmbienteNfce, tipoAmbienteNfe, alteradoPor);
            if (!EmpresaParametrosDfe.IsValid)
                AddNotifications(EmpresaParametrosDfe.Notifications);
            else
                MarcarAlterado(alteradoPor);
        }

        public void AtualizarNrNfce() => EmpresaParametrosDfe?.AtualizarNrNfce();
        public int ObterSerieNfce() => EmpresaParametrosDfe?.ObterSerieNfce() ?? 0;
        public long ObterNumeroNfce() => EmpresaParametrosDfe?.ObterNumeroNfce() ?? 0;
        public string ObterCscNfce() => EmpresaParametrosDfe?.ObterCscNfce() ?? string.Empty;
        public string ObterIdCscNfce() => EmpresaParametrosDfe?.ObterIdCscNfce() ?? string.Empty;
        public void AtualizarNrNfe() => EmpresaParametrosDfe?.AtualizarNrNfe();
        public int ObterSerieNfe() => EmpresaParametrosDfe?.ObterSerieNfe() ?? 0;
        public long ObterNumeroNfe() => EmpresaParametrosDfe?.ObterNumeroNfe() ?? 0;
    }
}
