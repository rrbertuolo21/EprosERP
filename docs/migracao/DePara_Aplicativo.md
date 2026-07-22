# De -> Para: Migração módulo Aplicativo (Permissoes + Usuarios)

Auditoria de fidelidade do porte legado -> `Epros.Modules.Aplicativo`.

Data: 2026-07-01

## Fontes legadas auditadas

- `src/Epros.ERP.Domain/Entities/Permissoes` (Menu, MenuItemNivel1, MenuItemNivel2, PerfilUsuario, PerfilUsuarioAcesso)
- `src/Epros.ERP.Domain/Entities/Usuarios` (Usuario, UsuarioEmpresa)

## Convenção de campos herdados (considerados COBERTOS)

Legado `Entity` / `EntityNoTenat` -> novo `EntidadeSaaSBase`:

| Campo legado | Destino novo (EntidadeSaaSBase) |
|---|---|
| Id (long) | Id (Guid) |
| TenantId (só em `Entity`) | TenantId |
| DataCadastro | CriadoEm |
| DataAlteracao | AlteradoEm |
| Deletado | DeletadoEm |
| (novo) | SyncId / SyncVersion / CriadoPor / AlteradoPor (infra SaaS) |

Observação: FKs `long` do legado foram convertidas para `Guid` no novo modelo (mudança de tipo esperada na migração SaaS).

## Tabela De -> Para (campos próprios das entidades)

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Menu | Descricao | Menu.Descricao |
| Menu | Icon | Menu.Icon |
| Menu | To | Menu.To |
| Menu | Ordem | Menu.Ordem |
| Menu | Itens (ICollection<MenuItemNivel1>) | Menu.Itens |
| MenuItemNivel1 | MenuId (long) | MenuItemNivel1.MenuId (Guid) |
| MenuItemNivel1 | Descricao | MenuItemNivel1.Descricao |
| MenuItemNivel1 | Icon | MenuItemNivel1.Icon |
| MenuItemNivel1 | To | MenuItemNivel1.To |
| MenuItemNivel1 | Ordem | MenuItemNivel1.Ordem |
| MenuItemNivel1 | Itens (ICollection<MenuItemNivel2>) | MenuItemNivel1.Itens |
| MenuItemNivel1 | Menu (nav EF) | MenuItemNivel1.Menu |
| MenuItemNivel2 | MenuItemNivel1Id (long) | MenuItemNivel2.MenuItemNivel1Id (Guid) |
| MenuItemNivel2 | Descricao | MenuItemNivel2.Descricao |
| MenuItemNivel2 | Icon | MenuItemNivel2.Icon |
| MenuItemNivel2 | To | MenuItemNivel2.To |
| MenuItemNivel2 | Ordem | MenuItemNivel2.Ordem |
| MenuItemNivel2 | MenuItemNivel1 (nav EF) | MenuItemNivel2.MenuItemNivel1 |
| PerfilUsuario | Descricao | PerfilUsuario.Descricao |
| PerfilUsuario | Acessos (ICollection<PerfilUsuarioAcesso>) | PerfilUsuario.Acessos |
| PerfilUsuario | Usuarios (List<Usuario>) | AUSENTE (nav inversa não portada; ver notas) |
| PerfilUsuarioAcesso | PerfilUsuarioId (long) | PerfilUsuarioAcesso.PerfilUsuarioId (Guid) |
| PerfilUsuarioAcesso | MenuId (long) | PerfilUsuarioAcesso.MenuId (Guid) |
| PerfilUsuarioAcesso | MenuItemNivel1Id (long) | PerfilUsuarioAcesso.MenuItemNivel1Id (Guid) |
| PerfilUsuarioAcesso | MenuItemNivel2Id (long?) | PerfilUsuarioAcesso.MenuItemNivel2Id (Guid?) |
| PerfilUsuarioAcesso | Ver | PerfilUsuarioAcesso.Ver |
| PerfilUsuarioAcesso | Editar | PerfilUsuarioAcesso.Editar |
| PerfilUsuarioAcesso | Excluir | PerfilUsuarioAcesso.Excluir |
| PerfilUsuarioAcesso | Menu / MenuItemNivel1 / MenuItemNivel2 / PerfilUsuario (nav EF) | PerfilUsuarioAcesso.Menu / .MenuItemNivel1 / .MenuItemNivel2 / .PerfilUsuario |
| Usuario | SequenciaTenantId (long) | AUSENTE (não portado em nenhuma entidade do módulo) |
| Usuario | Login | Usuario.Login |
| Usuario | Senha | Usuario.PasswordHash (renomeado; agora hash) |
| Usuario | Email | Usuario.Email |
| Usuario | Ativo (bool) | Usuario.Status (enum UsuarioStatus; Ativo == Status.Active) |
| Usuario | UsuarioEmpresa (ICollection) | Usuario.* (via UsuarioEmpresa.UsuarioId; nav coleção não exposta) |
| UsuarioEmpresa | EmpresaId (long) | UsuarioEmpresa.EmpresaId (Guid) |
| UsuarioEmpresa | UsuarioId (long) | UsuarioEmpresa.UsuarioId (Guid) |
| UsuarioEmpresa | PerfilUsuarioId (long?) | UsuarioEmpresa.PerfilUsuarioId (Guid?) |
| UsuarioEmpresa | IsAdmin (bool) | UsuarioEmpresa.EhAdmin (renomeado) |
| UsuarioEmpresa | PerfilUsuario (nav EF) | AUSENTE (navegação não exposta; só a FK PerfilUsuarioId) |
| UsuarioEmpresa | Empresa (nav EF) | AUSENTE (cross-module; só a FK EmpresaId) |
| UsuarioEmpresa | Usuario (nav EF) | AUSENTE (navegação não exposta; só a FK UsuarioId) |

## Entidades ausentes

Nenhuma entidade legada está ausente. Todas as 7 entidades das fontes têm contraparte no módulo novo:
Menu, MenuItemNivel1, MenuItemNivel2, PerfilUsuario, PerfilUsuarioAcesso, Usuario, UsuarioEmpresa.

## Campos críticos faltando

1. **Usuario.SequenciaTenantId (long)** — AUSENTE. Único campo escalar de dado do legado sem destino
   em nenhuma entidade do módulo. No legado era um contador de sequência por tenant. Avaliar se a
   numeração sequencial por tenant ainda é requisito de negócio (ex.: código amigável do usuário) ou
   se foi intencionalmente descontinuada em favor do Id Guid.

## Campos não críticos / decisões de porte (não bloqueantes)

- **PerfilUsuario.Usuarios (List<Usuario>)** e navegações inversas de EF
  (`UsuarioEmpresa.PerfilUsuario/Empresa/Usuario`) não foram expostas como propriedades de navegação;
  os relacionamentos são preservados via FKs (`PerfilUsuarioId`, `EmpresaId`, `UsuarioId`). Perda apenas
  de conveniência de navegação ORM, sem perda de dado.
- **Usuario.Senha -> Usuario.PasswordHash**: renomeação semântica (armazenamento de hash). Coberto.
- **Usuario.Ativo -> Usuario.Status (enum)**: o booleano legado é representável pelo enum
  (Active vs demais). Coberto, com ganho de expressividade.
- **UsuarioEmpresa.IsAdmin -> EhAdmin**: renomeação. Coberto.

## Cobertura estimada

Campos de dado próprios (escalares/FK, excluindo navegações EF e herdados):
- Menu: 4/4
- MenuItemNivel1: 5/5
- MenuItemNivel2: 5/5
- PerfilUsuario: 1/1
- PerfilUsuarioAcesso: 7/7
- Usuario: 4/5 (falta SequenciaTenantId)
- UsuarioEmpresa: 4/4

Total campos de dado: 29/30 = **~97%**.
