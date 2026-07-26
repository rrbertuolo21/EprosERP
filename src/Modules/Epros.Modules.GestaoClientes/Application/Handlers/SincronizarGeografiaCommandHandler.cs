using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Sincroniza Geografia.</summary>
    public class SincronizarGeografiaCommandHandler : ICommandHandler<SincronizarGeografiaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public SincronizarGeografiaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SincronizarGeografiaCommand request, CancellationToken cancellationToken)
        {
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var logSync = new SincronizacaoGeografica("IBGE-BR-V1", criadoPor);
            logSync.IniciarExecucao(criadoPor);
            _context.SincronizacoesGeograficas.Add(logSync);
            await _context.SaveChangesAsync(cancellationToken);

            int inseridos = 0;
            int atualizados = 0;
            int inativados = 0;

            try
            {
                // 1. Sincronizar País (Brasil)
                var brasil = await _context.Paises.FirstOrDefaultAsync(p => p.CodigoIsoAlpha2 == "BR", cancellationToken);
                if (brasil == null)
                {
                    brasil = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", criadoPor);
                    _context.Paises.Add(brasil);
                    await _context.SaveChangesAsync(cancellationToken);
                    inseridos++;
                }

                // 2. Sincronizar Subdivisões (UFs brasileiras)
                var ufs = new[]
                {
                    new { Sigla = "AC", Nome = "Acre" },
                    new { Sigla = "AL", Nome = "Alagoas" },
                    new { Sigla = "AP", Nome = "Amapá" },
                    new { Sigla = "AM", Nome = "Amazonas" },
                    new { Sigla = "BA", Nome = "Bahia" },
                    new { Sigla = "CE", Nome = "Ceará" },
                    new { Sigla = "DF", Nome = "Distrito Federal" },
                    new { Sigla = "ES", Nome = "Espírito Santo" },
                    new { Sigla = "GO", Nome = "Goiás" },
                    new { Sigla = "MA", Nome = "Maranhão" },
                    new { Sigla = "MT", Nome = "Mato Grosso" },
                    new { Sigla = "MS", Nome = "Mato Grosso do Sul" },
                    new { Sigla = "MG", Nome = "Minas Gerais" },
                    new { Sigla = "PA", Nome = "Pará" },
                    new { Sigla = "PB", Nome = "Paraíba" },
                    new { Sigla = "PR", Nome = "Paraná" },
                    new { Sigla = "PE", Nome = "Pernambuco" },
                    new { Sigla = "PI", Nome = "Piauí" },
                    new { Sigla = "RJ", Nome = "Rio de Janeiro" },
                    new { Sigla = "RN", Nome = "Rio Grande do Norte" },
                    new { Sigla = "RS", Nome = "Rio Grande do Sul" },
                    new { Sigla = "RO", Nome = "Rondônia" },
                    new { Sigla = "RR", Nome = "Roraima" },
                    new { Sigla = "SC", Nome = "Santa Catarina" },
                    new { Sigla = "SP", Nome = "São Paulo" },
                    new { Sigla = "SE", Nome = "Sergipe" },
                    new { Sigla = "TO", Nome = "Tocantins" }
                };

                foreach (var uf in ufs)
                {
                    var isoCode = $"BR-{uf.Sigla}";
                    var existeSub = await _context.Subdivisoes.AnyAsync(s => s.CodigoISO31662 == isoCode, cancellationToken);
                    if (!existeSub)
                    {
                        var subdivisao = new Subdivisao(brasil.Id, isoCode, uf.Nome, TipoSubdivisao.Estado, null, criadoPor);
                        _context.Subdivisoes.Add(subdivisao);
                        inseridos++;
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Sincronizar Municípios Principais
                var cidades = new[]
                {
                    new { Uf = "SP", Nome = "São Paulo", Ibge = 3550308L },
                    new { Uf = "SP", Nome = "Campinas", Ibge = 3509502L },
                    new { Uf = "RJ", Nome = "Rio de Janeiro", Ibge = 3304557L },
                    new { Uf = "MG", Nome = "Belo Horizonte", Ibge = 3106200L },
                    new { Uf = "BA", Nome = "Salvador", Ibge = 2927408L },
                    new { Uf = "DF", Nome = "Brasília", Ibge = 5300108L },
                    new { Uf = "PR", Nome = "Curitiba", Ibge = 4106900L },
                    new { Uf = "RS", Nome = "Porto Alegre", Ibge = 4314902L },
                    new { Uf = "PE", Nome = "Recife", Ibge = 2611606L },
                    new { Uf = "CE", Nome = "Fortaleza", Ibge = 2304400L },
                    new { Uf = "AM", Nome = "Manaus", Ibge = 1302603L },
                    new { Uf = "PA", Nome = "Belém", Ibge = 1501402L },
                    new { Uf = "GO", Nome = "Goiânia", Ibge = 5208707L },
                    new { Uf = "SC", Nome = "Florianópolis", Ibge = 4205407L }
                };

                foreach (var cid in cidades)
                {
                    var isoCode = $"BR-{cid.Uf}";
                    var subdivisao = await _context.Subdivisoes.FirstOrDefaultAsync(s => s.CodigoISO31662 == isoCode, cancellationToken);
                    if (subdivisao != null)
                    {
                        var existingMun = await _context.Municipios.FirstOrDefaultAsync(m => m.CodigoIbge == cid.Ibge, cancellationToken);
                        if (existingMun == null)
                        {
                            var municipio = new Municipio(brasil.Id, subdivisao.Id, cid.Nome, cid.Ibge, null, null, criadoPor);
                            _context.Municipios.Add(municipio);
                            inseridos++;
                        }
                        else
                        {
                            bool alterado = false;
                            if (!existingMun.Ativo)
                            {
                                existingMun.Reativar(criadoPor);
                                alterado = true;
                            }
                            if (alterado)
                            {
                                _context.Municipios.Update(existingMun);
                                atualizados++;
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);

                // Inativar municípios que não constam mais na base oficial sincronizada
                var ibgeList = cidades.Select(c => c.Ibge).ToList();
                var municipiosParaInativar = await _context.Municipios
                    .Where(m => m.PaisId == brasil.Id && m.Ativo && !ibgeList.Contains(m.CodigoIbge))
                    .ToListAsync(cancellationToken);

                foreach (var mun in municipiosParaInativar)
                {
                    mun.Inativar(criadoPor);
                    _context.Municipios.Update(mun);
                    inativados++;
                }
                await _context.SaveChangesAsync(cancellationToken);

                // 4. Configurar Formato de Código Postal Padrão para o Brasil (#####-###)
                var formatoCep = await _context.FormatosCodigoPostal.FirstOrDefaultAsync(f => f.PaisId == brasil.Id, cancellationToken);
                if (formatoCep == null)
                {
                    formatoCep = new FormatoCodigoPostal(brasil.Id, "^\\d{5}-\\d{3}$", "#####-###", "01310-100", criadoPor);
                    _context.FormatosCodigoPostal.Add(formatoCep);
                    await _context.SaveChangesAsync(cancellationToken);
                    inseridos++;
                }

                logSync.Concluir(inseridos, atualizados, inativados, criadoPor);
                _context.SincronizacoesGeograficas.Update(logSync);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logSync.Falhar(ex.Message, criadoPor);
                _context.SincronizacoesGeograficas.Update(logSync);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Falha(new[] { $"Erro na sincronização: {ex.Message}" }, "Erro de Execução");
            }

            return CommandResult.Ok("Sincronização de geografia concluída com sucesso!");
        }
    }
}
