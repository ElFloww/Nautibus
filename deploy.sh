#!/usr/bin/env bash
# Nautibus — déploiement sur un conteneur LXC Proxmox (ou tout hôte Linux x86_64 avec Docker).
#
#   ./deploy.sh install   première installation : configuration, clés, construction, base, démarrage
#   ./deploy.sh update    reconstruit les images, met à jour le schéma de la base, redémarre
#   ./deploy.sh backup    sauvegarde complète de la base dans ./backups
#   ./deploy.sh status    état des services et santé de l'application
#   ./deploy.sh logs      journaux en direct (Ctrl+C pour quitter)
#   ./deploy.sh stop      arrête les services (les données sont conservées)
#
# Prérequis LXC (sur l'hôte Proxmox, conteneur arrêté) :
#   pct set <ID> --features nesting=1,keyctl=1     (Docker dans LXC)
#   au moins 4 Go de RAM (SQL Server en demande 2 à lui seul) et 20 Go de disque.
set -euo pipefail

cd "$(dirname "$(readlink -f "$0")")"
ENV_FILE=.env
CADDYFILE=deploy/Caddyfile
COMPOSE=(docker compose --env-file "$ENV_FILE")

info()  { printf '\033[1;34m▶ %s\033[0m\n' "$*"; }
ok()    { printf '\033[1;32m✔ %s\033[0m\n' "$*"; }
warn()  { printf '\033[1;33m⚠ %s\033[0m\n' "$*"; }
fail()  { printf '\033[1;31m✘ %s\033[0m\n' "$*" >&2; exit 1; }

# Mot de passe accepté par SQL Server (majuscule, minuscule, chiffre) et sans caractère gênant dans une chaîne de connexion.
password() { printf '%s%s' "$(tr -dc 'A-Za-z0-9' </dev/urandom | head -c 28)" "Aa9"; }

check_host() {
  [ "$(uname -m)" = "x86_64" ] || fail "SQL Server n'existe qu'en x86_64 (machine : $(uname -m))."
  local v_Mem
  v_Mem=$(awk '/MemTotal/ {print int($2/1024)}' /proc/meminfo)
  [ "$v_Mem" -ge 3500 ] || warn "Mémoire : ${v_Mem} Mo. Prévoir au moins 4 Go (SQL Server en exige 2)."

  if ! command -v docker >/dev/null 2>&1; then
    warn "Docker n'est pas installé."
    read -r -p "Installer Docker avec le script officiel (https://get.docker.com) ? [o/N] " v_Answer
    [[ "$v_Answer" =~ ^[oOyY]$ ]] || fail "Installez Docker et le plugin compose, puis relancez."
    command -v curl >/dev/null 2>&1 || { apt-get update && apt-get install -y curl; }
    curl -fsSL https://get.docker.com | sh
  fi
  docker compose version >/dev/null 2>&1 || fail "Le plugin « docker compose » est absent (paquet docker-compose-plugin)."
  if ! docker info >/dev/null 2>&1; then
    if grep -qa 'container=lxc' /proc/1/environ 2>/dev/null; then
      fail "Docker ne démarre pas dans ce LXC. Sur l'hôte Proxmox : pct set <ID> --features nesting=1,keyctl=1, puis redémarrez le conteneur."
    fi
    fail "Docker ne répond pas (droits ? service arrêté ?)."
  fi
}

# Valeur d'une clé du fichier .env.
env_get() { grep -E "^$1=" "$ENV_FILE" | tail -n1 | cut -d= -f2-; }

configure() {
  if [ -f "$ENV_FILE" ]; then
    ok "Configuration existante conservée ($ENV_FILE)."
    return
  fi
  info "Configuration (laisser vide pour la valeur proposée)"
  local v_Ip v_Site v_Email v_SmtpHost v_SmtpUser v_SmtpPassword v_SmtpFrom v_SmtpPort
  v_Ip=$(hostname -I 2>/dev/null | awk '{print $1}')
  read -r -p "Nom de domaine public (ex. nautibus.mondomaine.fr), ou vide pour l'adresse IP ${v_Ip} : " v_Site
  read -r -p "E-mail de contact (certificat HTTPS et notifications push) : " v_Email
  [ -n "$v_Email" ] || fail "L'e-mail de contact est obligatoire (exigé par Let's Encrypt et les services push)."
  echo "Envoi d'e-mails (confirmation de compte, mot de passe oublié) — vide pour désactiver :"
  read -r -p "  Serveur SMTP : " v_SmtpHost
  if [ -n "$v_SmtpHost" ]; then
    read -r -p "  Port [587] : " v_SmtpPort
    read -r -p "  Utilisateur : " v_SmtpUser
    read -r -s -p "  Mot de passe : " v_SmtpPassword; echo
    read -r -p "  Adresse d'expédition : " v_SmtpFrom
  fi

  local v_PublicUrl="https://${v_Site:-$v_Ip}"
  umask 077
  cat >"$ENV_FILE" <<EOF
# Généré par deploy.sh le $(date -Iseconds). Contient des secrets : ne pas partager ni versionner.
SITE_ADDRESS=${v_Site}
SITE_IP=${v_Ip}
PUBLIC_URL=${v_PublicUrl}
ADMIN_EMAIL=${v_Email}

DB_NAME=Nautibus
DB_APP_USER=nautibus_app
DB_APP_PASSWORD=$(password)
MSSQL_SA_PASSWORD=$(password)

SMTP_HOST=${v_SmtpHost}
SMTP_PORT=${v_SmtpPort:-587}
SMTP_SSL=true
SMTP_USER=${v_SmtpUser:-}
SMTP_PASSWORD=${v_SmtpPassword:-}
SMTP_FROM=${v_SmtpFrom:-}

PUSH_SUBJECT=mailto:${v_Email}
PUSH_PUBLIC_KEY=
PUSH_PRIVATE_KEY=
EOF
  ok "Configuration écrite dans $ENV_FILE (droits 600)."
}

write_caddyfile() {
  local v_Site v_Email
  v_Site=$(env_get SITE_ADDRESS)
  v_Email=$(env_get ADMIN_EMAIL)
  {
    # Options globales : e-mail du compte Let's Encrypt (facultatif).
    [ -z "$v_Email" ] || printf '{\n\temail %s\n}\n\n' "$v_Email"
    # Le bloc réutilisable doit précéder les sites qui l'importent.
    printf '%s\n' '(nautibus) {' \
      '	encode zstd gzip' \
      '	header {' \
      '		Strict-Transport-Security "max-age=31536000"' \
      '		X-Content-Type-Options nosniff' \
      '		Referrer-Policy strict-origin-when-cross-origin' \
      '	}' \
      '	@api path /api/* /hubs/* /health' \
      '	reverse_proxy @api api:8080' \
      '	reverse_proxy web:80' \
      '}' ''
    if [ -n "$v_Site" ]; then
      # Domaine public : certificat Let's Encrypt automatique (ports 80 et 443 joignables depuis Internet).
      printf '%s {\n\timport nautibus\n}\n' "$v_Site"
    else
      # Réseau local : autorité interne de Caddy (avertissement du navigateur tant qu'elle n'est pas approuvée).
      printf 'https://%s, https://localhost {\n\ttls internal\n\timport nautibus\n}\n' "$(env_get SITE_IP)"
    fi
  } >"$CADDYFILE"
  ok "Proxy HTTPS configuré ($CADDYFILE)."
}

generate_push_keys() {
  [ -z "$(env_get PUSH_PUBLIC_KEY)" ] || return 0
  info "Génération des clés de notification push (VAPID)"
  local v_Keys v_Public v_Private
  v_Keys=$("${COMPOSE[@]}" run --rm --no-deps -T api generate-vapid-keys)
  v_Public=$(printf '%s\n' "$v_Keys" | sed -n 's/^Push:PublicKey *= *//p' | tr -d '\r')
  v_Private=$(printf '%s\n' "$v_Keys" | sed -n 's/^Push:PrivateKey *= *//p' | tr -d '\r')
  [ -n "$v_Public" ] && [ -n "$v_Private" ] || fail "Génération des clés VAPID impossible."
  sed -i "s|^PUSH_PUBLIC_KEY=.*|PUSH_PUBLIC_KEY=${v_Public}|; s|^PUSH_PRIVATE_KEY=.*|PUSH_PRIVATE_KEY=${v_Private}|" "$ENV_FILE"
  ok "Clés VAPID enregistrées (ne plus les changer : les abonnements existants deviendraient invalides)."
}

sqlcmd() {
  "${COMPOSE[@]}" exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -C -b -S localhost -U sa -P "$(env_get MSSQL_SA_PASSWORD)" "$@"
}

deploy_database() {
  # Le schéma (db-init) et le compte applicatif (db-user) sont gérés par docker compose au démarrage.
  info "Base : démarrage de SQL Server, mise à jour du schéma, compte applicatif"
  "${COMPOSE[@]}" up -d --wait sqlserver redis
  "${COMPOSE[@]}" run --rm db-init
  "${COMPOSE[@]}" run --rm --no-deps db-user
  ok "Base prête."
}

wait_healthy() {
  info "Attente du démarrage de l'application"
  for _ in $(seq 1 60); do
    if "${COMPOSE[@]}" exec -T caddy wget -q -O /dev/null "http://api:8080/api/v1/vehicles/status" 2>/dev/null; then
      ok "Nautibus répond : $(env_get PUBLIC_URL)"
      echo "  Le premier import des horaires prend une à deux minutes ; le temps réel suit."
      return 0
    fi
    sleep 5
  done
  warn "L'application ne répond pas encore. Voir : ./deploy.sh logs"
}

backup() {
  mkdir -p backups
  local v_Db v_File
  v_Db=$(env_get DB_NAME)
  v_File="${v_Db}-$(date +%Y%m%d-%H%M%S).bak"
  info "Sauvegarde de la base ${v_Db}"
  "${COMPOSE[@]}" up -d --wait sqlserver
  "${COMPOSE[@]}" exec -T sqlserver mkdir -p /var/opt/mssql/backup
  sqlcmd -Q "BACKUP DATABASE [${v_Db}] TO DISK = N'/var/opt/mssql/backup/${v_File}' WITH COMPRESSION, CHECKSUM, INIT" >/dev/null 2>&1 \
    || sqlcmd -Q "BACKUP DATABASE [${v_Db}] TO DISK = N'/var/opt/mssql/backup/${v_File}' WITH CHECKSUM, INIT" >/dev/null
  "${COMPOSE[@]}" cp "sqlserver:/var/opt/mssql/backup/${v_File}" "backups/${v_File}"
  "${COMPOSE[@]}" exec -T sqlserver rm -f "/var/opt/mssql/backup/${v_File}"
  ok "Sauvegarde : backups/${v_File} (copier aussi .env, indispensable pour restaurer)."
}

case "${1:-}" in
  install)
    check_host
    configure
    write_caddyfile
    info "Construction des images (plusieurs minutes la première fois)"
    "${COMPOSE[@]}" build
    generate_push_keys
    deploy_database
    "${COMPOSE[@]}" up -d
    wait_healthy
    ;;
  update)
    check_host
    [ -f "$ENV_FILE" ] || fail "Pas de configuration : lancer d'abord ./deploy.sh install"
    write_caddyfile
    "${COMPOSE[@]}" build --pull
    backup
    deploy_database
    "${COMPOSE[@]}" up -d --remove-orphans
    wait_healthy
    ;;
  backup) backup ;;
  status)
    "${COMPOSE[@]}" ps
    "${COMPOSE[@]}" exec -T caddy wget -q -O - http://api:8080/health 2>/dev/null | head -c 2000; echo
    ;;
  logs) "${COMPOSE[@]}" logs -f --tail=100 ;;
  stop) "${COMPOSE[@]}" down ;;
  *)
    sed -n '2,13p' "$0" | sed 's/^# \{0,1\}//'
    exit 1
    ;;
esac
