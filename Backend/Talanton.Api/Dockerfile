FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore Talanton.Api.csproj
RUN dotnet publish Talanton.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

# By default the host watches appsettings*.json for changes, which allocates an inotify
# instance per file. Container hosts cap inotify instances well below what that assumes
# (Render's limit is 128), so startup throws before any application code runs. Config is
# supplied by environment variables in every deployed environment, so reloading the JSON
# files at runtime buys nothing.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

EXPOSE 10000

ENTRYPOINT ["dotnet", "Talanton.Api.dll"]