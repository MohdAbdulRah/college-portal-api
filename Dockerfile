# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution & project files first (better layer caching)
COPY CollegePortal.sln ./
COPY CollegePortal.API/CollegePortal.API.csproj CollegePortal.API/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY CollegePortal.API/ CollegePortal.API/

# Publish a Release build to /app/publish
RUN dotnet publish CollegePortal.API/CollegePortal.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Render injects PORT env var; ASP.NET Core reads ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CollegePortal.API.dll"]
