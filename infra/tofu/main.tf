terraform {
  required_providers {
    docker = {
      source  = "registry.terraform.io/kreuzwerker/docker"
      version = "~> 3.0.1"
    }
  }
}

provider "docker" {}

# Rede integrada
resource "docker_network" "epros_network" {
  name   = "epros-network"
  driver = "bridge"
}

# Volumes persistentes
resource "docker_volume" "postgres_data" {
  name = "postgres_data"
}

resource "docker_volume" "minio_data" {
  name = "minio_data"
}

resource "docker_volume" "valkey_data" {
  name = "valkey_data"
}

# Contêiner PostgreSQL 16
resource "docker_container" "postgres" {
  name  = "epros-postgres"
  image = "postgres:16-alpine"
  
  env = [
    "POSTGRES_DB=epros",
    "POSTGRES_USER=epros",
    "POSTGRES_PASSWORD=epros_dev_password"
  ]

  ports {
    internal = 5432
    external = 5432
  }

  volumes {
    volume_name    = docker_volume.postgres_data.name
    container_path = "/var/lib/postgresql/data"
  }

  networks_advanced {
    name = docker_network.epros_network.name
  }
}

# Contêiner Keycloak 24
resource "docker_container" "keycloak" {
  name  = "epros-keycloak"
  image = "quay.io/keycloak/keycloak:24.0.0"
  
  command = ["start-dev", "--import-realm"]

  env = [
    "KEYCLOAK_ADMIN=admin",
    "KEYCLOAK_ADMIN_PASSWORD=admin"
  ]

  ports {
    internal = 8080
    external = 8080
  }

  # Monta o diretório de importação de Realms local de forma absoluta
  volumes {
    host_path      = "${abspath("${path.module}/../keycloak")}"
    container_path = "/opt/keycloak/data/import"
  }

  networks_advanced {
    name = docker_network.epros_network.name
  }
}

# Contêiner HashiCorp Vault 1.16
resource "docker_container" "vault" {
  name  = "epros-vault"
  image = "hashicorp/vault:1.16.0"

  env = [
    "VAULT_DEV_ROOT_TOKEN_ID=epros-dev-token",
    "VAULT_DEV_LISTEN_ADDRESS=0.0.0.0:8200"
  ]

  ports {
    internal = 8200
    external = 8200
  }

  capabilities {
    add = ["IPC_LOCK"]
  }

  networks_advanced {
    name = docker_network.epros_network.name
  }
}

# Contêiner MinIO
resource "docker_container" "minio" {
  name  = "epros-minio"
  image = "minio/minio:latest"

  command = ["server", "/data", "--console-address", ":9001"]

  env = [
    "MINIO_ROOT_USER=epros_minio",
    "MINIO_ROOT_PASSWORD=epros_minio_password"
  ]

  ports {
    internal = 9000
    external = 9000
  }

  ports {
    internal = 9001
    external = 9001
  }

  volumes {
    volume_name    = docker_volume.minio_data.name
    container_path = "/data"
  }

  networks_advanced {
    name = docker_network.epros_network.name
  }
}

# Contêiner Valkey 7
resource "docker_container" "valkey" {
  name  = "epros-valkey"
  image = "valkey/valkey:7-alpine"

  ports {
    internal = 6379
    external = 6379
  }

  volumes {
    volume_name    = docker_volume.valkey_data.name
    container_path = "/data"
  }

  networks_advanced {
    name = docker_network.epros_network.name
  }
}
