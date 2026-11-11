# Sirve la SPA de Monetra con nginx y redirige /api hacia el contenedor de la API.
# Build context esperado: la carpeta web/.
FROM nginx:1.27-alpine

# Configuración del sitio (SPA + proxy /api).
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Archivos estáticos de la aplicación.
COPY index.html /usr/share/nginx/html/
COPY css/ /usr/share/nginx/html/css/
COPY js/ /usr/share/nginx/html/js/
COPY img/ /usr/share/nginx/html/img/

EXPOSE 80
