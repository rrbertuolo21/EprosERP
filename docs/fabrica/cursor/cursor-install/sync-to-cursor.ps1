#!/usr/bin/env pwsh
# Sincroniza o pacote gerado (cursor-install) + agentes -> .cursor/ na raiz do repo.
# Rode a partir de qualquer cwd; resolve a raiz pelo caminho deste script.
$ErrorActionPreference = 'Stop'

$installDir = $PSScriptRoot
$fabricaCursor = Split-Path $installDir -Parent
$fabrica = Split-Path $fabricaCursor -Parent
$docs = Split-Path $fabrica -Parent
$root = Split-Path $docs -Parent

$rulesSrc = Join-Path $installDir 'rules'
$agentesSrc = Join-Path $fabrica 'agentes'
$cursor = Join-Path $root '.cursor'
$rulesDst = Join-Path $cursor 'rules'
$skillsDst = Join-Path $cursor 'skills'
$commandsDst = Join-Path $cursor 'commands'

if (-not (Test-Path $rulesSrc)) {
  throw "Pacote ausente: $rulesSrc — rode gen_cursor_rules.py antes."
}

New-Item -ItemType Directory -Force -Path $rulesDst, $skillsDst, $commandsDst | Out-Null

Copy-Item (Join-Path $rulesSrc '*.mdc') $rulesDst -Force
Write-Host "Rules: $((Get-ChildItem $rulesDst -Filter '*.mdc').Count) -> $rulesDst"

foreach ($mdc in Get-ChildItem $rulesSrc -Filter 'S*.mdc') {
  $folder = $mdc.BaseName
  $text = Get-Content $mdc.FullName -Raw -Encoding UTF8
  if ($text -notmatch '(?s)^---\r?\n(.*?)\r?\n---\r?\n(.*)$') {
    Write-Warning "Sem frontmatter: $($mdc.Name)"
    continue
  }
  $fm = $Matches[1]
  $body = $Matches[2].TrimStart("`r", "`n")
  $desc = ''
  if ($fm -match 'description:\s*"(.*)"') { $desc = $Matches[1] }
  elseif ($fm -match 'description:\s*(.+)$') { $desc = $Matches[1].Trim() }

  $skillDir = Join-Path $skillsDst $folder
  New-Item -ItemType Directory -Force -Path $skillDir | Out-Null
  $skillMd = @"
---
name: $folder
description: >-
  $desc
---

$body
"@
  Set-Content -Path (Join-Path $skillDir 'SKILL.md') -Value $skillMd -Encoding utf8
}
Write-Host "Skills: $((Get-ChildItem $skillsDst -Directory).Count) -> $skillsDst"

$map = @{
  '05-planning-agent.md'       = 'planning.md'
  '06-architect-agent.md'      = 'architect.md'
  '07-dev-agent.md'            = 'dev.md'
  '08-qa-agent.md'             = 'qa.md'
  '09-ops-agent.md'            = 'ops.md'
  '10-security-agent.md'       = 'security.md'
  '12-code-review-agent.md'    = 'code-review.md'
  '16-engenharia-reversa.md'   = 'engenharia-reversa.md'
}
foreach ($kv in $map.GetEnumerator()) {
  $src = Join-Path $agentesSrc $kv.Key
  if (-not (Test-Path $src)) {
    Write-Warning "Agente ausente: $($kv.Key)"
    continue
  }
  Copy-Item $src (Join-Path $commandsDst $kv.Value) -Force
}

Write-Host "Commands: $((Get-ChildItem $commandsDst -Filter '*.md').Count) -> $commandsDst"
Write-Host "OK — .cursor sincronizado em $cursor"
