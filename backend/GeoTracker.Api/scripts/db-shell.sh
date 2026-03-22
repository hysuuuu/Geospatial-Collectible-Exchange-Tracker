#!/usr/bin/env bash
set -euo pipefail

# Starts a PostgreSQL Docker container (if needed) and opens a psql shell.
#
# Defaults can be overridden with flags or environment variables:
#   CONTAINER_NAME, POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB, POSTGRES_PORT, POSTGRES_IMAGE
#
# Examples:
#   ./scripts/db-shell.sh
#   ./scripts/db-shell.sh --container geotracker-postgres --user postgres --db geotrackerdb
#   POSTGRES_PASSWORD=mysecret ./scripts/db-shell.sh

CONTAINER_NAME="${CONTAINER_NAME:-geotracker-postgres}"
DB_USER="${POSTGRES_USER:-postgres}"
DB_PASSWORD="${POSTGRES_PASSWORD:-password}"
DB_NAME="${POSTGRES_DB:-geotrackerdb}"
DB_PORT="${POSTGRES_PORT:-5432}"
DB_IMAGE="${POSTGRES_IMAGE:-postgres:16}"
CREATE_IF_MISSING="true"

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]

Options:
  --container <name>   Docker container name (default: ${CONTAINER_NAME})
  --user <name>        PostgreSQL user (default: ${DB_USER})
  --password <value>   PostgreSQL password (default from POSTGRES_PASSWORD or 'postgres')
  --db <name>          Database name (default: ${DB_NAME})
  --port <port>        Host port to bind to 5432 (default: ${DB_PORT})
  --image <image>      PostgreSQL image (default: ${DB_IMAGE})
  --no-create          Fail if container does not exist
  -h, --help           Show this help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --container)
      CONTAINER_NAME="$2"
      shift 2
      ;;
    --user)
      DB_USER="$2"
      shift 2
      ;;
    --password)
      DB_PASSWORD="$2"
      shift 2
      ;;
    --db)
      DB_NAME="$2"
      shift 2
      ;;
    --port)
      DB_PORT="$2"
      shift 2
      ;;
    --image)
      DB_IMAGE="$2"
      shift 2
      ;;
    --no-create)
      CREATE_IF_MISSING="false"
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if ! command -v docker >/dev/null 2>&1; then
  echo "Error: docker command not found. Install Docker first." >&2
  exit 1
fi

if ! docker info >/dev/null 2>&1; then
  echo "Error: Docker daemon is not running. Start Docker and try again." >&2
  exit 1
fi

container_exists() {
  docker ps -a --format '{{.Names}}' | grep -Fxq "$CONTAINER_NAME"
}

container_running() {
  docker ps --format '{{.Names}}' | grep -Fxq "$CONTAINER_NAME"
}

if container_exists; then
  if ! container_running; then
    echo "Starting existing container: $CONTAINER_NAME"
    docker start "$CONTAINER_NAME" >/dev/null
  else
    echo "Container already running: $CONTAINER_NAME"
  fi
else
  if [[ "$CREATE_IF_MISSING" != "true" ]]; then
    echo "Error: container '$CONTAINER_NAME' does not exist and --no-create was set." >&2
    exit 1
  fi

  echo "Creating and starting container: $CONTAINER_NAME"
  docker run -d \
    --name "$CONTAINER_NAME" \
    -e POSTGRES_USER="$DB_USER" \
    -e POSTGRES_PASSWORD="$DB_PASSWORD" \
    -e POSTGRES_DB="$DB_NAME" \
    -p "$DB_PORT":5432 \
    "$DB_IMAGE" >/dev/null
fi

echo "Waiting for PostgreSQL to accept connections..."
for _ in {1..30}; do
  if docker exec "$CONTAINER_NAME" pg_isready -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

if ! docker exec "$CONTAINER_NAME" pg_isready -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; then
  echo "Error: PostgreSQL did not become ready in time." >&2
  exit 1
fi

echo "Opening psql in container '$CONTAINER_NAME' (db=$DB_NAME, user=$DB_USER)..."
exec docker exec -it "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME"
