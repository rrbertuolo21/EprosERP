#!/usr/bin/env python3
"""
Seed TRANSACIONAL de validação (Classe D) — popula dado de negócio no tenant de teste
via a API real (respeita RLS/ABAC/validação), para que dropdowns venham preenchidos e as
telas [id] de edição fiquem testáveis. Idempotente: cria só até um alvo mínimo por recurso.

Uso:
  python3 scripts/seed_transacional.py            # tenant admin (tenant-teste-a)
Requer a stack local no ar (API em http://localhost:8080).
"""
import json
import sys
import urllib.request

API = "http://localhost:8080/api/v1"
EMAIL = "admin@teste-a.com.br"
SENHA = "Epros@Validacao#2026"


def req(method, path, token=None, body=None):
    url = f"{API}{path}"
    data = json.dumps(body).encode() if body is not None else None
    r = urllib.request.Request(url, data=data, method=method)
    r.add_header("Content-Type", "application/json")
    if token:
        r.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(r) as resp:
            return resp.status, json.loads(resp.read().decode() or "{}")
    except urllib.error.HTTPError as e:
        try:
            return e.code, json.loads(e.read().decode() or "{}")
        except Exception:
            return e.code, {}


def login():
    st, d = req("POST", "/public/auth/login", body={"email": EMAIL, "senha": SENHA})
    tok = (d.get("dados") or {}).get("token")
    if not tok:
        print(f"FALHA login ({st}): {d}")
        sys.exit(1)
    return tok


def total(token, path):
    st, d = req("GET", f"{path}?pagina=1&tamanhoPagina=1", token)
    dados = d.get("dados")
    if isinstance(dados, dict):
        return dados.get("total", dados.get("totalRegistros", 0)) or 0
    if isinstance(dados, list):
        return len(dados)
    return 0


def itens(token, path, n=50):
    st, d = req("GET", f"{path}?pagina=1&tamanhoPagina={n}", token)
    dados = d.get("dados")
    if isinstance(dados, dict):
        return dados.get("itens") or dados.get("Itens") or []
    if isinstance(dados, list):
        return dados
    return []


def criar(token, path, body, rotulo):
    st, d = req("POST", path, token, body)
    ok = d.get("sucesso") if isinstance(d, dict) else None
    msg = d.get("mensagem") if isinstance(d, dict) else d
    print(f"  [{st}] criar {rotulo}: sucesso={ok} msg={msg}")
    return d


def garantir(token, path, alvo, gerar_body, rotulo):
    """Cria itens até atingir `alvo` no recurso (idempotência por contagem)."""
    atual = total(token, path)
    print(f"{rotulo}: atual={atual} alvo={alvo}")
    for i in range(atual, alvo):
        criar(token, path, gerar_body(i), f"{rotulo}#{i+1}")


def main():
    token = login()
    print("== login OK ==\n")

    # ---- Cadastros base (apoio de dropdown) ----
    garantir(token, "/marcas-produtos", 3, lambda i: {"descricao": f"Marca {i+1}"}, "Marcas")
    garantir(token, "/categorias-produtos", 3, lambda i: {"descricao": f"Categoria {i+1}"}, "Categorias")
    garantir(token, "/unidades-de-medidas-comercial", 3,
             lambda i: {"unidadeMedida": ["UN", "KG", "CX"][i % 3], "descricao": ["Unidade", "Quilograma", "Caixa"][i % 3], "fator": 1},
             "Unidades")

    marcas = itens(token, "/marcas-produtos")
    cats = itens(token, "/categorias-produtos")
    uns = itens(token, "/unidades-de-medidas-comercial")
    mid = marcas[0]["id"] if marcas else None
    cid = cats[0]["id"] if cats else None
    uid = uns[0]["id"] if uns else None

    # ---- Produtos (base: POST /estoque/produtos; enums são INTEIROS) ----
    def produto_body(i):
        return {
            "categoriaId": cid, "marcaProdutoId": mid, "unidadeMedidaComercialId": uid,
            "codigo": f"PROD{i+1:04d}", "descricao": f"Produto de Teste {i+1}",
            "ean": "", "pesoLiquido": 1.0, "pesoBruto": 1.2,
            "valorVenda": 100.0 + i * 10, "valorVendaPrazo": 110.0 + i * 10, "valorCompra": 60.0 + i * 5,
            "tipoProduto": 0, "ativo": True, "imagem": "",
            "utilizaBalanca": False, "ncmId": None
        }
    garantir(token, "/estoque/produtos", 5, produto_body, "Produtos")

    # ---- Pessoas (parceiros) ----
    # Enums INTEIROS: ETipoPessoa PF=1/PJ=2; ETipoIndicadorIe ICMS=1/Isento=2/NaoContrib=3; ETipoContribuinte NaoInformado=0.
    pessoas = [
        {"tipoPessoa": 1, "tipoIndicadorIe": 3,
         "fisicaCpf": "11144477735", "fisicaNome": "Cliente", "fisicaSobrenome": "Teste PF",
         "ehCliente": True, "ehFornecedor": False, "ehTransportadora": False,
         "ehMotorista": False, "ehPrestadorServico": False, "ehFuncionario": False, "ehProdutorRural": False,
         "clienteEhConsumidorFinal": True, "clienteTipoContribuinte": 0},
        {"tipoPessoa": 2, "tipoIndicadorIe": 1,
         "juridicaCnpj": "11222333000181", "razaoSocial": "Fornecedor Teste LTDA", "nomeFantasia": "Fornecedor Teste",
         "ehCliente": False, "ehFornecedor": True, "ehTransportadora": False,
         "ehMotorista": False, "ehPrestadorServico": False, "ehFuncionario": False, "ehProdutorRural": False},
        {"tipoPessoa": 2, "tipoIndicadorIe": 1,
         "juridicaCnpj": "11444777000161", "razaoSocial": "Transportadora Teste LTDA", "nomeFantasia": "Transporta Teste",
         "ehCliente": False, "ehFornecedor": False, "ehTransportadora": True,
         "ehMotorista": False, "ehPrestadorServico": False, "ehFuncionario": False, "ehProdutorRural": False},
    ]
    atual = total(token, "/cadastros/pessoas")
    print(f"Pessoas: atual={atual} alvo={len(pessoas)}")
    for i in range(atual, len(pessoas)):
        criar(token, "/cadastros/pessoas", pessoas[i], f"Pessoa#{i+1}")

    print("\n== resumo final ==")
    for ep, rot in [("/marcas-produtos", "Marcas"), ("/categorias-produtos", "Categorias"),
                    ("/unidades-de-medidas-comercial", "Unidades"), ("/estoque-produtos", "Produtos"),
                    ("/cadastros/pessoas", "Pessoas")]:
        print(f"  {rot}: {total(token, ep)}")


if __name__ == "__main__":
    main()
