# Docker Guide

This repository already contains a multi-stage Docker build for the ASP.NET API and the React frontend.

## What the container does

- Builds the React app with Node.
- Publishes the ASP.NET API with the .NET SDK.
- Copies the React production assets into `ModernPaySystem/wwwroot`.
- Runs the final API image on the ASP.NET runtime image.

## Build the image

Run this from the repository root:

```bash
docker build -f ModernPaySystem/DockerFile -t modernpaysystem:latest .
```

If you prefer a release tag:

```bash
docker build -f ModernPaySystem/DockerFile -t modernpaysystem:v1.0.0 .
```

## Run the image

The app listens on container port `8080`.

```bash
docker run --name modernpaysystem -p 8080:8080 modernpaysystem:latest
```

## Pass environment variables

The API reads configuration from app settings and environment variables. For Docker, override sensitive values at runtime instead of baking them into the image.

Example:

```bash
docker run --name modernpaysystem ^
  -p 8080:8080 ^
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=ModernPaySystemDb;Username=postgres;Password=0000" ^
  -e JwtSettings__SecretKey="replace-this-with-a-long-secret-key" ^
  -e Seeding__Enabled=false ^
  modernpaysystem:latest
```

## Common workflow

1. Build the image.
2. Start your database container or local database.
3. Run the API container with the database connection string injected.
4. Open `http://localhost:8080` in the browser.

## Notes

- The Dockerfile name is `DockerFile`, so use `-f ModernPaySystem/DockerFile` when building.
- The frontend output is already configured to land in the API `wwwroot` folder.
- If you change the frontend or API structure, rebuild the image so the static assets are refreshed.
