FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY out/ .
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080
ENTRYPOINT ["dotnet", "barberShop.dll"]
