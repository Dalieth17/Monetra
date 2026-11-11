#!/usr/bin/env bash
# Compila la SPA de React y la abre en el navegador de un solo clic.
#
# El sitio funciona SIN backend: en la pantalla de inicio pulsa
# "Entrar en modo demostración" para explorar la app con datos de ejemplo.
# (Para usar datos reales, levanta aparte la API o usa docker-compose.)
#
# Uso:  ./abrir-web.sh        (desde la carpeta web/)
#       bash web/abrir-web.sh (desde la raíz del repositorio)

set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$DIR"

if ! command -v npm >/dev/null 2>&1; then
  echo "No se encontró Node/npm. Instálalo con:  sudo dnf install nodejs"
  exit 1
fi

if [ ! -d node_modules ]; then
  echo "Instalando dependencias (solo la primera vez)…"
  npm install
fi

echo "Monetra: arrancando el servidor de desarrollo…"
echo "En la pantalla de inicio pulsa \"Entrar en modo demostración\"."
echo "Para detenerlo: Ctrl + C"
echo

# El servidor de Vite abre el navegador y recarga en caliente.
exec npm run dev -- --open
