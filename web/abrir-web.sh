#!/usr/bin/env bash
# Abre el sitio web de Monetra de un solo clic.
#
# El sitio funciona SIN backend: en la pantalla de inicio pulsa
# "Entrar en modo demostración" para explorar la app con datos de ejemplo.
# (Para usar datos reales, levanta aparte la API en backend/src/Monetra.Api.)
#
# Uso:  ./abrir-web.sh            (desde la carpeta web/)
#       bash web/abrir-web.sh     (desde la raíz del repositorio)

set -e

PORT=8080
# Carpeta donde está este script (la carpeta web/), sin importar desde dónde se ejecute.
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
URL="http://localhost:${PORT}/"

# Elige Python 3 (python3 en Fedora/Ubuntu, python en otros).
if command -v python3 >/dev/null 2>&1; then
    PY=python3
elif command -v python >/dev/null 2>&1; then
    PY=python
else
    echo "No se encontró Python. Instálalo con:  sudo dnf install python3"
    exit 1
fi

echo "Monetra: sirviendo el sitio en ${URL}"
echo "En la pantalla de inicio pulsa \"Entrar en modo demostración\"."
echo "Para detenerlo: Ctrl + C"
echo

# Arranca el servidor estático en segundo plano y lo detiene al cerrar el script.
"$PY" -m http.server "$PORT" --directory "$DIR" >/dev/null 2>&1 &
SERVER_PID=$!
trap 'kill "$SERVER_PID" 2>/dev/null' EXIT

# Espera a que el servidor responda antes de abrir el navegador.
for _ in $(seq 1 20); do
    if curl -s -o /dev/null "$URL" 2>/dev/null; then break; fi
    sleep 0.3
done

# Abre el navegador con el comando disponible según el sistema.
if command -v xdg-open >/dev/null 2>&1; then xdg-open "$URL" >/dev/null 2>&1 || true
elif command -v open >/dev/null 2>&1; then open "$URL" || true
else echo "Abre manualmente esta dirección en tu navegador: ${URL}"
fi

wait "$SERVER_PID"
