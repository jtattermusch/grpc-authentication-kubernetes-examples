FROM mcr.microsoft.com/dotnet/core/sdk:2.1
WORKDIR /app

ADD . /app

RUN dotnet publish -o out -c Release -f netcoreapp2.1 GreeterServer.csproj

ENTRYPOINT ["dotnet", "exec", "/app/out/GreeterServer.dll"]
