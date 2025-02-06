# ---------------------
# 1) Build stage
# ------alpine ---------------
    FROM node:18-alpine AS build
    WORKDIR /app
    
    # Copy package.json, package-lock.json (if any)
    COPY package*.json ./
    RUN npm install
    
    # Copy the rest of the files
    COPY . .
    
    # Build the Angular project
    RUN npm run build --prod
    
    # ---------------------
    # 2) Production stage
    # ---------------------
    FROM nginx:alpine
    # Copy the compiled dist folder from build stage
    COPY --from=build /app/dist/my-school /usr/share/nginx/html
    
    EXPOSE 80
    CMD ["nginx", "-g", "daemon off;"]
    