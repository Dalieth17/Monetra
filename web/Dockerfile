# Compila la SPA de React con Vite y la sirve con nginx, que además redirige
# /api hacia el contenedor de la API. Build context esperado: la carpeta web/.

# --- Etapa 1: build de producción con Node ---
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci || npm install
COPY . .
RUN npm run build

# --- Etapa 2: servir los estáticos con nginx ---
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
