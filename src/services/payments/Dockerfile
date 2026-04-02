# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/payments/payments.csproj", "src/services/payments/"]
COPY ["src/shared/Messaging/Messaging.csproj", "src/shared/Messaging/"]
RUN dotnet restore "./src/services/payments/payments.csproj"
COPY . .
WORKDIR "/src/src/services/payments"
RUN dotnet build "./payments.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./payments.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage builds a self-contained migrations bundle for production deployments
FROM build AS bundle
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --self-contained -r linux-x64 -o /app/efbundle && chmod +x /app/efbundle

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=bundle /app/efbundle ./efbundle
ENTRYPOINT ["dotnet", "payments.dll"]