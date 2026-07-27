FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN apt-get update -y \
  && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
  && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY . .

RUN dotnet tool restore \
  && dotnet restore blobflags.slnx

RUN cd src/BlobFlags.Client \
  && npm ci \
  && npm run build

RUN dotnet publish src/BlobFlags.Server -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:8085
EXPOSE 8085
ENTRYPOINT ["dotnet", "BlobFlags.Server.dll"]
