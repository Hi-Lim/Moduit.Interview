ARG PROJECT="Moduit.Interview.Engine.Docker.Linux"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS base
ARG PROJECT
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1-alpine AS build
WORKDIR /src
ARG NUGET_URL=https://api.nuget.org/v3/index.json
ARG QSI_URL=http://devops.quadrant-si.id/nexus/repository/nuget-hosted/
ARG PROJECT
COPY ["${PROJECT}/${PROJECT}.csproj", "${PROJECT}/"]
RUN dotnet restore --source $NUGET_URL --source $QSI_URL "${PROJECT}/${PROJECT}.csproj"
COPY . .
WORKDIR "/src/${PROJECT}"
RUN dotnet build ${PROJECT}.csproj -c Release -o /app --source $NUGET_URL --source $QSI_URL

FROM build AS publish
RUN dotnet publish ${PROJECT}.csproj -c Release -o /app --source $NUGET_URL --source $QSI_URL

FROM base AS final
ENV PROJECTNAME "${PROJECT}.dll"
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT dotnet $PROJECTNAME

