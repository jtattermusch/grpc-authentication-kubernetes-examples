FROM mcr.microsoft.com/dotnet/core/sdk:2.1
WORKDIR /app

COPY . /app

RUN dotnet publish -o out -c Release -f netcoreapp2.1 GreeterClient.csproj

ENTRYPOINT ["dotnet", "exec", "/app/out/GreeterClient.dll"]
