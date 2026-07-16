# Stage 1: Build Frontend (React/Vite)
FROM node:22-alpine AS web-build
WORKDIR /web

COPY package*.json ./
RUN npm install
COPY . .
RUN NODE_OPTIONS="--max-old-space-size=4096" npm run build

# Stage 2: Build Backend (.NET API)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src

COPY backend/Directory.Build.props backend/
COPY backend/src/ backend/src/

RUN dotnet restore backend/src/Template.Api/Template.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/Template.Api/Template.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=api-build /app/publish .
COPY --from=web-build /web/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Template.Api.dll"]
