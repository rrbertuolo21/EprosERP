#!/usr/bin/env python3
"""Gera .cursor/rules/*.mdc a partir das skills e do Context Agent da Fábrica Epros."""
import os, re, glob, pathlib

ROOT = "/Users/rafaelrbertuolo/Desktop/FabricaSoftware"
OUT = os.path.join(ROOT, "projetos", "epros", "cursor-install", "rules")
pathlib.Path(OUT).mkdir(parents=True, exist_ok=True)

def split_frontmatter(text):
    """Retorna (dict_frontmatter_bruto_linhas, corpo) de um arquivo com '---' no topo."""
    if text.startswith("---"):
        parts = text.split("---", 2)
        if len(parts) >= 3:
            return parts[1], parts[2].lstrip("\n")
    return "", text

def get_field(fm, key):
    """Extrai um campo do frontmatter YAML simples, incluindo bloco folded '>' multi-linha."""
    lines = fm.splitlines()
    for i, ln in enumerate(lines):
        m = re.match(r'^' + re.escape(key) + r':\s*(.*)$', ln)
        if m:
            val = m.group(1).strip()
            if val in (">", "|", ">-", "|-"):  # bloco multi-linha
                collected = []
                for nxt in lines[i+1:]:
                    if re.match(r'^\S', nxt):  # próxima chave top-level
                        break
                    collected.append(nxt.strip())
                return " ".join(x for x in collected if x)
            return val.strip().strip('"')
    return ""

def flatten(s):
    return re.sub(r'\s+', ' ', s).strip()

# ---- Skills S01..S30 ----
count = 0
for skill_md in sorted(glob.glob(os.path.join(ROOT, "projetos", "epros", "skills", "S*", "SKILL.md"))):
    folder = os.path.basename(os.path.dirname(skill_md))   # ex: S03-epros-multi-tenancy
    text = open(skill_md, encoding="utf-8").read()
    fm, body = split_frontmatter(text)
    name = get_field(fm, "name") or folder
    desc = flatten(get_field(fm, "description")) or f"Skill {folder} do Epros"
    # description como aspas duplas seguras
    desc_safe = desc.replace('"', "'")
    mdc = (
        "---\n"
        f'description: "{desc_safe}"\n'
        "globs:\n"
        "alwaysApply: false\n"
        "---\n\n"
        f"{body.rstrip()}\n"
    )
    out_path = os.path.join(OUT, f"{folder}.mdc")
    open(out_path, "w", encoding="utf-8").write(mdc)
    count += 1

# ---- Context Agent (sempre ativo) ----
ctx_path = os.path.join(ROOT, "agentes", "00-context-agent.md")
ctx = open(ctx_path, encoding="utf-8").read()
ctx_mdc = (
    "---\n"
    'description: "Contexto e regras invioláveis do Epros — sempre ativo. Identidade do projeto, stack fechada, EntidadeSaaSBase, CQRS, multi-tenancy e as regras SEMPRE/NUNCA."\n'
    "alwaysApply: true\n"
    "---\n\n"
    f"{ctx.rstrip()}\n"
)
open(os.path.join(OUT, "00-context.mdc"), "w", encoding="utf-8").write(ctx_mdc)

print(f"Gerados: {count} skills + 00-context.mdc  ->  {OUT}")
for f in sorted(os.listdir(OUT)):
    print("  ", f)
