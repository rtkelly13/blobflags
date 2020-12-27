FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

RUN apt-get update -y  \
  && curl -sL https://deb.nodesource.com/setup_current.x | bash \
  && apt-get install -y nodejs

WORKDIR /src

# Check copied files
# COPY . .
# RUN ls -l -R ui

COPY ./.config .
COPY ./.paket .
COPY global.json .
RUN dotnet tool restore

COPY paket.dependencies .
COPY paket.lock .
RUN dotnet paket restore

WORKDIR /src/ui

COPY ./ui/package-lock.json .
COPY ./ui/package.json .

RUN npm install

COPY ./ui .

WORKDIR /src

COPY build.fsx .
RUN dotnet fake build

FROM mcr.microsoft.com/dotnet/aspnet:3.1-alpine AS final

COPY --from=build ./src/ui/deploy .
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]