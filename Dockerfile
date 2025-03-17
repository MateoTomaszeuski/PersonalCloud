FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src/.
COPY . .

RUN mkdir -p /app/wwwroot/media /app/wwwroot/albums

RUN dotnet restore
RUN dotnet publish ./PersonalCloud/PersonalCloud.csproj -c Release -o /app/publish
WORKDIR /app/publish

ENTRYPOINT ["dotnet", "PersonalCloud.dll"]