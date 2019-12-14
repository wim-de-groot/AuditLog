FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-./obj/docker/publish} .
ENTRYPOINT ["dotnet", "AuditLog.ConsoleClient.dll"]