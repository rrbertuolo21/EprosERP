using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public enum EEstadoPessoa
    {
        Rascunho = 1,
        EmValidacao = 2,
        Ativo = 3,
        Inativo = 4,
        Bloqueado = 5
    }

    public class Pessoa : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (long) — exceção oficial de porte (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }
        public ETipoPessoa TipoPessoa { get; private set; }
        public ETipoIndicadorIe TipoIndicadorIe { get; private set; }
        public Guid? PessoaGrupoId { get; private set; }
        public long? InscricaoSuframa { get; private set; }
        public string? TitularContaBancaria { get; private set; }
        public string? AgenciaContaBancaria { get; private set; }
        public string? NumeroContaBancaria { get; private set; }
        public ETipoPix? TipoPix { get; private set; }
        public string? ChavePix { get; private set; }
        public string? Observacoes { get; private set; }

        public bool EhCliente { get; private set; }
        public bool EhFornecedor { get; private set; }
        public bool EhTransportadora { get; private set; }
        public bool EhMotorista { get; private set; }
        public bool EhPrestadorServico { get; private set; }
        public bool EhFuncionario { get; private set; }
        public bool EhProdutorRural { get; private set; }
        public bool EhInativo { get; private set; }
        public EEstadoPessoa Status { get; private set; }

        // Subtypes (1:1)
        public virtual PessoaFisica? PessoaFisica { get; private set; }
        public virtual PessoaJuridica? PessoaJuridica { get; private set; }
        public virtual PessoaEstrangeiro? PessoaEstrangeiro { get; private set; }

        // Roles (1:1)
        public virtual PessoaCliente? PessoaCliente { get; private set; }
        public virtual PessoaFuncionario? PessoaFuncionario { get; private set; }
        public virtual PessoaMotorista? PessoaMotorista { get; private set; }
        public virtual PessoaTransportadora? PessoaTransportadora { get; private set; }
        public virtual PessoaPrestadorServico? PessoaPrestadorServico { get; private set; }

        // Collections (1:N)
        public virtual ICollection<Endereco> Enderecos { get; private set; } = new List<Endereco>();
        public virtual ICollection<PessoaContato> Contatos { get; private set; } = new List<PessoaContato>();
        public virtual ICollection<PessoaVeiculo> Veiculos { get; private set; } = new List<PessoaVeiculo>();

        protected Pessoa() { } // EF Core

        public Pessoa(
            ETipoPessoa tipoPessoa,
            ETipoIndicadorIe tipoIndicadorIe,
            Guid? pessoaGrupoId,
            long? inscricaoSuframa,
            string? titularContaBancaria,
            string? agenciaContaBancaria,
            string? numeroContaBancaria,
            ETipoPix? tipoPix,
            string? chavePix,
            string? observacoes,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoPessoa = tipoPessoa;
            TipoIndicadorIe = tipoIndicadorIe;
            PessoaGrupoId = pessoaGrupoId;
            InscricaoSuframa = inscricaoSuframa;
            TitularContaBancaria = titularContaBancaria;
            AgenciaContaBancaria = agenciaContaBancaria;
            NumeroContaBancaria = numeroContaBancaria;
            TipoPix = tipoPix;
            ChavePix = chavePix;
            Observacoes = observacoes;
            EhInativo = false;
            Status = EEstadoPessoa.Ativo; // default active
        }

        public void VincularFisica(PessoaFisica? fisica)
        {
            PessoaFisica = fisica;
        }

        public void VincularJuridica(PessoaJuridica? juridica)
        {
            PessoaJuridica = juridica;
        }

        public void VincularEstrangeiro(PessoaEstrangeiro? estrangeiro)
        {
            PessoaEstrangeiro = estrangeiro;
        }

        public void VincularCliente(PessoaCliente? cliente)
        {
            PessoaCliente = cliente;
            EhCliente = cliente != null;
        }

        public void VincularFuncionario(PessoaFuncionario? funcionario)
        {
            PessoaFuncionario = funcionario;
            EhFuncionario = funcionario != null;
        }

        public void VincularMotorista(PessoaMotorista? motorista)
        {
            PessoaMotorista = motorista;
            EhMotorista = motorista != null;
        }

        public void VincularTransportadora(PessoaTransportadora? transportadora)
        {
            PessoaTransportadora = transportadora;
            EhTransportadora = transportadora != null;
        }

        public void VincularPrestadorServico(PessoaPrestadorServico? prestador)
        {
            PessoaPrestadorServico = prestador;
            EhPrestadorServico = prestador != null;
        }

        public void DefinirFornecedor(bool ehFornecedor)
        {
            EhFornecedor = ehFornecedor;
        }

        public void DefinirProdutorRural(bool ehProdutorRural)
        {
            EhProdutorRural = ehProdutorRural;
        }

        public void AdicionarEndereco(Endereco endereco)
        {
            if (endereco == null) return;
            // 1.01 (RN endereço principal único): ao adicionar um endereço marcado como Principal,
            // desmarca os demais principais do mesmo tipo desta pessoa (1 principal por tipo de endereço).
            if (endereco.Principal)
            {
                foreach (var existente in Enderecos)
                {
                    if (existente.Principal && existente.TipoEndereco == endereco.TipoEndereco)
                    {
                        existente.DefinirPrincipal(false, endereco.CriadoPor ?? CriadoPor ?? "system");
                    }
                }
            }
            Enderecos.Add(endereco);
        }

        public void LimparEnderecos()
        {
            Enderecos.Clear();
        }

        public void AdicionarContato(PessoaContato contato)
        {
            if (contato == null) return;
            Contatos.Add(contato);
        }

        public void LimparContatos()
        {
            Contatos.Clear();
        }

        public void AdicionarVeiculo(PessoaVeiculo veiculo)
        {
            if (veiculo == null) return;
            Veiculos.Add(veiculo);
        }

        public void LimparVeiculos()
        {
            Veiculos.Clear();
        }

        public void Submeter(string usuario)
        {
            Status = EEstadoPessoa.EmValidacao;
            MarcarAlterado(usuario);
        }

        public void AtivarDireto(string usuario)
        {
            Status = EEstadoPessoa.Ativo;
            EhInativo = false;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            Status = EEstadoPessoa.Ativo;
            EhInativo = false;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            Status = EEstadoPessoa.Rascunho;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            Status = EEstadoPessoa.Inativo;
            EhInativo = true;
            MarcarAlterado(usuario);
        }

        public void Bloquear(string usuario)
        {
            Status = EEstadoPessoa.Bloqueado;
            MarcarAlterado(usuario);
        }

        public void Reativar(string usuario)
        {
            Status = EEstadoPessoa.Ativo;
            EhInativo = false;
            MarcarAlterado(usuario);
        }

        public void AtualizarDadosBase(
            ETipoPessoa tipoPessoa,
            ETipoIndicadorIe tipoIndicadorIe,
            Guid? pessoaGrupoId,
            long? inscricaoSuframa,
            string? titularContaBancaria,
            string? agenciaContaBancaria,
            string? numeroContaBancaria,
            ETipoPix? tipoPix,
            string? chavePix,
            string? observacoes,
            string usuario)
        {
            TipoPessoa = tipoPessoa;
            TipoIndicadorIe = tipoIndicadorIe;
            PessoaGrupoId = pessoaGrupoId;
            InscricaoSuframa = inscricaoSuframa;
            TitularContaBancaria = titularContaBancaria;
            AgenciaContaBancaria = agenciaContaBancaria;
            NumeroContaBancaria = numeroContaBancaria;
            TipoPix = tipoPix;
            ChavePix = chavePix;
            Observacoes = observacoes;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();

            AddNotifications(new Contract<Pessoa>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(ETipoPessoa), TipoPessoa), nameof(TipoPessoa), "TipoPessoa não consta na lista [Origem: Pessoa]")
                .IsTrue(Enum.IsDefined(typeof(ETipoIndicadorIe), TipoIndicadorIe), nameof(TipoIndicadorIe), "TipoIndicadorIe não consta na lista [Origem: Pessoa]")
                .HasMaxLen(TitularContaBancaria ?? string.Empty, 150, nameof(TitularContaBancaria), "O campo TitularContaBancaria deve ter no máximo 150 caracteres [Origem: Pessoa]")
                .HasMaxLen(AgenciaContaBancaria ?? string.Empty, 20, nameof(AgenciaContaBancaria), "O campo AgenciaContaBancaria deve ter no máximo 20 caracteres [Origem: Pessoa]")
                .HasMaxLen(NumeroContaBancaria ?? string.Empty, 20, nameof(NumeroContaBancaria), "O campo NumeroContaBancaria deve ter no máximo 20 caracteres [Origem: Pessoa]")
                .HasMaxLen(ChavePix ?? string.Empty, 32, nameof(ChavePix), "O campo ChavePix deve ter no máximo 32 caracteres [Origem: Pessoa]")
                .HasMaxLen(Observacoes ?? string.Empty, 300, nameof(Observacoes), "O campo Observacoes deve ter no máximo 300 caracteres [Origem: Pessoa]")
            );

            if (PessoaGrupoId.HasValue && PessoaGrupoId.Value == Guid.Empty)
            {
                AddNotification(nameof(PessoaGrupoId), "O campo PessoaGrupoId deve ser maior que Zero [Origem: Pessoa]");
            }

            if (InscricaoSuframa.HasValue && InscricaoSuframa.Value > 999999999)
            {
                AddNotification(nameof(InscricaoSuframa), "O campo InscricaoSuframa deve ser menor ou igual a 999999999");
            }

            // RN-PEM-089: Pelo menos um papel
            if (!EhCliente && !EhFornecedor && !EhTransportadora && !EhMotorista && !EhPrestadorServico && !EhFuncionario && !EhProdutorRural)
            {
                AddNotification(nameof(EhCliente), "Pelo menos uma opção deve estar marcada (Cliente, Funcionário, Motorista, Prestador de Serviço, Transportadora ou Fornecedor).");
            }

            // RN-PEM-083: Motorista x Transportadora
            if (EhMotorista && EhTransportadora)
            {
                AddNotification(nameof(EhMotorista), "A pessoa não pode estar marcada como Motorista e Transportadora! Escolha somente um.");
            }

            // RN-PEM-084: Motorista PJ
            if (EhMotorista && TipoPessoa == ETipoPessoa.PessoaJuridica)
            {
                AddNotification(nameof(EhMotorista), "A pessoa Motorista não pode ser Pessoa Jurídica!");
            }

            // Validar presença de subtipo 1:1
            if (TipoPessoa == ETipoPessoa.PessoaFisica)
            {
                if (PessoaFisica == null)
                    AddNotification(nameof(PessoaFisica), "PessoaFisica não pode ser Null");
                else
                    AddNotifications(PessoaFisica.Notifications);
            }
            else if (TipoPessoa == ETipoPessoa.PessoaJuridica)
            {
                if (PessoaJuridica == null)
                    AddNotification(nameof(PessoaJuridica), "PessoaJuridica não pode ser Null");
                else
                    AddNotifications(PessoaJuridica.Notifications);
            }
            else if (TipoPessoa == ETipoPessoa.PessoaEstrangeira)
            {
                if (PessoaEstrangeiro == null)
                    AddNotification(nameof(PessoaEstrangeiro), "PessoaEstrangeiro não pode ser Null");
                else
                    AddNotifications(PessoaEstrangeiro.Notifications);
            }

            // Validar presença de papel 1:1
            if (EhCliente)
            {
                if (PessoaCliente == null)
                    AddNotification(nameof(PessoaCliente), "PessoaCliente não pode ser Null");
                else
                    AddNotifications(PessoaCliente.Notifications);
            }
            if (EhFuncionario)
            {
                if (PessoaFuncionario == null)
                    AddNotification(nameof(PessoaFuncionario), "PessoaFuncionario não pode ser Null");
                else
                    AddNotifications(PessoaFuncionario.Notifications);
            }
            if (EhMotorista)
            {
                if (PessoaMotorista == null)
                    AddNotification(nameof(PessoaMotorista), "PessoaMotorista não pode ser Null");
                else
                    AddNotifications(PessoaMotorista.Notifications);
            }
            if (EhPrestadorServico)
            {
                if (PessoaPrestadorServico == null)
                    AddNotification(nameof(PessoaPrestadorServico), "PessoaPrestadorServico não pode ser Null");
                else
                    AddNotifications(PessoaPrestadorServico.Notifications);
            }
            if (EhTransportadora)
            {
                if (PessoaTransportadora == null)
                    AddNotification(nameof(PessoaTransportadora), "PessoaTransportadora não pode ser Null");
                else
                    AddNotifications(PessoaTransportadora.Notifications);
            }

            // RN-PEM-010: Principal Address check (max one Principal address)
            var principalAddressesCount = Enderecos.Count(e => e.TipoEndereco == ETipoEndereco.Principal);
            if (principalAddressesCount > 1)
            {
                AddNotification(nameof(Enderecos), "Já existe um endereço Principal cadastrado para essa pessoa");
            }

            // RN-PEM-077: CEP só pode ser Nulo para estrangeiro
            if (TipoPessoa != ETipoPessoa.PessoaEstrangeira)
            {
                foreach (var end in Enderecos)
                {
                    if (string.IsNullOrWhiteSpace(end.Cep))
                    {
                        AddNotification(nameof(Enderecos), "CEP só pode ser Nulo para estrangeiro");
                        break;
                    }
                }
            }

            // RN-PEM-090/091: Contatos principal check
            if (Contatos.Any())
            {
                var principalContatosCount = Contatos.Count(c => c.EhPrincipal);
                if (principalContatosCount == 0)
                {
                    AddNotification(nameof(Contatos), "Deve haver um contato como Principal");
                }
                else if (principalContatosCount > 1)
                {
                    AddNotification(nameof(Contatos), "Deve haver apenas um contato como Principal");
                }
            }

            // Propagate child notifications:
            foreach (var end in Enderecos)
                AddNotifications(end.Notifications);
            foreach (var cont in Contatos)
                AddNotifications(cont.Notifications);
            foreach (var veic in Veiculos)
                AddNotifications(veic.Notifications);
        }
    }
}
