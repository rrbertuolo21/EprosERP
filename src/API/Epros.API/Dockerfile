# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Epros.sln ./
COPY .config/dotnet-tools.json .config/
COPY src/API/Epros.API/Epros.API.csproj src/API/Epros.API/
COPY src/Infrastructure/Epros.Infrastructure/Epros.Infrastructure.csproj src/Infrastructure/Epros.Infrastructure/
COPY src/Shared/Epros.Shared/Epros.Shared.csproj src/Shared/Epros.Shared/
COPY src/External/Epros.ERP.Shared/Epros.ERP.Shared.csproj src/External/Epros.ERP.Shared/
COPY src/External/Epros.ERP.DfeCalculos/Epros.ERP.DfeCalculos.csproj src/External/Epros.ERP.DfeCalculos/
COPY src/Modules/Epros.Modules.GestaoClientes/Epros.Modules.GestaoClientes.csproj src/Modules/Epros.Modules.GestaoClientes/
COPY src/Modules/Epros.Modules.Estoque/Epros.Modules.Estoque.csproj src/Modules/Epros.Modules.Estoque/
COPY src/Modules/Epros.Modules.Fiscal/Epros.Modules.Fiscal.csproj src/Modules/Epros.Modules.Fiscal/
COPY src/Modules/Epros.Modules.Financeiro/Epros.Modules.Financeiro.csproj src/Modules/Epros.Modules.Financeiro/
COPY src/Modules/Epros.Modules.Vendas/Epros.Modules.Vendas.csproj src/Modules/Epros.Modules.Vendas/
COPY src/Modules/Epros.Modules.Aplicativo/Epros.Modules.Aplicativo.csproj src/Modules/Epros.Modules.Aplicativo/
COPY src/Modules/Epros.Modules.Qualidade/Epros.Modules.Qualidade.csproj src/Modules/Epros.Modules.Qualidade/
COPY src/Modules/Epros.Modules.Producao/Epros.Modules.Producao.csproj src/Modules/Epros.Modules.Producao/
COPY src/Modules/Epros.Modules.RH/Epros.Modules.RH.csproj src/Modules/Epros.Modules.RH/
COPY src/Modules/Epros.Modules.Projetos/Epros.Modules.Projetos.csproj src/Modules/Epros.Modules.Projetos/
COPY src/Modules/Epros.Modules.Manutencao/Epros.Modules.Manutencao.csproj src/Modules/Epros.Modules.Manutencao/
COPY src/Modules/Epros.Modules.GRC/Epros.Modules.GRC.csproj src/Modules/Epros.Modules.GRC/
COPY src/Modules/Epros.Modules.ESG/Epros.Modules.ESG.csproj src/Modules/Epros.Modules.ESG/
COPY src/Modules/Epros.Modules.DMS/Epros.Modules.DMS.csproj src/Modules/Epros.Modules.DMS/
COPY src/Modules/Epros.Modules.Imobiliaria/Epros.Modules.Imobiliaria.csproj src/Modules/Epros.Modules.Imobiliaria/

RUN dotnet restore src/API/Epros.API/Epros.API.csproj

COPY src/ src/
COPY scripts/migrate-all.sh scripts/
RUN chmod +x scripts/migrate-all.sh

RUN dotnet publish src/API/Epros.API/Epros.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS migrate
WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /src /src
COPY .config/dotnet-tools.json .config/
COPY scripts/migrate-all.sh /src/scripts/migrate-all.sh
COPY scripts/apply-auth-rls-policies.sql /src/scripts/apply-auth-rls-policies.sql
RUN sed -i 's/\r$//' /src/scripts/migrate-all.sh \
    && dotnet tool restore \
    && dotnet restore src/API/Epros.API/Epros.API.csproj \
    && chmod +x /src/scripts/migrate-all.sh

ENTRYPOINT ["/src/scripts/migrate-all.sh"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV EPROS_DFE_STORAGE=/data/dfe

RUN mkdir -p /data/dfe

COPY --from=build /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Epros.API.dll"]
