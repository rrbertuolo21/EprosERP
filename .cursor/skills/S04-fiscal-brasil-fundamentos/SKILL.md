---
name: S04-fiscal-brasil-fundamentos
description: >-
  Fundamentos fiscais brasileiros para o Epros ERP: NF-e (modelo 55), NFC-e (modelo 65), CFOP, NCM, CST/CSOSN, Substituição Tributária, certificado digital A1/A3 por CNPJ, contingência offline e guarda legal de XMLs por 5 anos. Use em QUALQUER tarefa que toque documento fiscal, tributação, emissão de nota, ou specs/testes/código de features fiscais — inclui dados de homologação e checklist pré-emissão.
---

# fiscal-brasil-fundamentos

> **S04 · Camada 0 — Fundação** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **NF-e, NFC-e, nota fiscal, CFOP, NCM, CST, substituição tributária, certificado digital, SEFAZ, contingência, XML fiscal, emissão**.

## O que esta skill cobre

O conhecimento tributário-base do ERP: NF-e vs NFC-e, ciclo de vida do documento fiscal, CFOP, NCM, CST/CSOSN, Substituição Tributária, certificado digital A1/A3, contingência offline e prazos legais de guarda.

Evitar que agentes sugiram bobagem fiscal (CFOP de venda em compra, certificado compartilhado entre tenants) e dar base comum para specs, código e testes de qualquer feature que toque documento fiscal — a maior fonte de bugs P0 num ERP brasileiro.

## Instruções para o agente

1. Antes de sugerir CFOP, alíquota ou campo fiscal, pergunte pelo estado (UF) do tenant — regras variam por estado.
2. Nunca sugira compartilhar certificado digital entre tenants: certificado é por CNPJ (regra inviolável).
3. Toda feature fiscal deve prever o caminho de contingência offline e a guarda do XML por 5 anos no MinIO.
4. Use os dados de homologação (CNPJs e NCMs de teste) desta skill em exemplos e testes — nunca dados reais de cliente.
5. Em dúvida tributária profunda (interpretação de legislação), recomende validação com contador — a skill orienta, não substitui consultoria.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `exemplos/xml-nfe-anotado.xml` — XML de homologação comentado campo a campo
- ⬜ `exemplos/tabela-cfop-epros.md` — CFOPs por operação do sistema
- ⬜ `checklists/pre-emissao.md` — o que validar antes de emitir (tenant configurado, NCM, certificado, série)
- ⬜ `exemplos/dados-homologacao.md` — CNPJs, NCMs e certificados de teste

## Como completar esta skill (do v1-semente à versão completa)

1. Compile o conteúdo a partir da documentação da lib Hercules.NET e dos manuais SEFAZ (foco no que o Epros usa).
2. Extraia do módulo Fiscal/DFe existente os fluxos já implementados e documente-os.
3. Monte a tabela de CFOPs cruzando as operações reais dos 20 clientes legados.
4. Valide o conteúdo com o contador/consultor fiscal da empresa antes de publicar a v1.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
