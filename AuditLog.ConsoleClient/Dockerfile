FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
ARG source
WORKDIR /app
COPY ${source:-./obj/docker/publish} .
ENTRYPOINT ["dotnet", "AuditLog.ConsoleClient.dll"]