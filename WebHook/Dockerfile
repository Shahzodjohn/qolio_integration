FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebHook/WebHook.csproj", "WebHook/"]
RUN dotnet restore "WebHook/WebHook.csproj"
COPY . .
WORKDIR "/src/WebHook"
RUN dotnet build "WebHook.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WebHook.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy the certificate to the final image
COPY --from=build /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
ENTRYPOINT ["dotnet", "WebHook.dll"]