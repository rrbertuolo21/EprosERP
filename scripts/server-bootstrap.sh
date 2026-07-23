#!/usr/bin/env bash
# Bootstrap inicial de VPS Ubuntu para deploy do EprosERP.
# Execute como root: curl -fsSL ... | bash   OU   sudo bash scripts/server-bootstrap.sh
set -euo pipefail

DEPLOY_USER="${DEPLOY_USER:-deploy}"
REPO_URL="${REPO_URL:-https://github.com/SEU_ORG/EprosERP.git}"
INSTALL_DIR="${INSTALL_DIR:-/opt/epros}"

if [[ "${EUID}" -ne 0 ]]; then
  echo "Execute como root (sudo)."
  exit 1
fi

echo "==> Atualizando pacotes..."
export DEBIAN_FRONTEND=noninteractive
apt-get update -y
apt-get upgrade -y

echo "==> Instalando dependências..."
apt-get install -y ca-certificates curl gnupg ufw git

if ! command -v docker >/dev/null 2>&1; then
  echo "==> Instalando Docker Engine..."
  install -m 0755 -d /etc/apt/keyrings
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
  chmod a+r /etc/apt/keyrings/docker.asc
  echo \
    "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
    $(. /etc/os-release && echo "${VERSION_CODENAME}") stable" \
    > /etc/apt/sources.list.d/docker.list
  apt-get update -y
  apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
fi

echo "==> Configurando firewall (ufw)..."
ufw --force reset
ufw default deny incoming
ufw default allow outgoing
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

if ! id "${DEPLOY_USER}" >/dev/null 2>&1; then
  echo "==> Criando usuário ${DEPLOY_USER}..."
  useradd -m -s /bin/bash "${DEPLOY_USER}"
  usermod -aG docker "${DEPLOY_USER}"
fi

mkdir -p /backups
chown "${DEPLOY_USER}:${DEPLOY_USER}" /backups

if [[ ! -d "${INSTALL_DIR}/.git" ]]; then
  echo "==> Clonando repositório em ${INSTALL_DIR}..."
  git clone "${REPO_URL}" "${INSTALL_DIR}"
  chown -R "${DEPLOY_USER}:${DEPLOY_USER}" "${INSTALL_DIR}"
else
  echo "==> Repositório já existe em ${INSTALL_DIR}."
fi

echo ""
echo "==> Bootstrap concluído."
echo "Próximos passos (como ${DEPLOY_USER}):"
echo "  cd ${INSTALL_DIR}"
echo "  cp .env.production.example .env.production"
echo "  # Edite .env.production com domínios e senhas"
echo "  docker compose -f docker-compose.prod.yml build"
echo "  docker compose -f docker-compose.prod.yml up -d"
