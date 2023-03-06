#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["sales-microservice/src/AMR.SalesMicroservice/AMR.SalesMicroservice.csproj", "sales-microservice/src/AMR.SalesMicroservice/"]
COPY ["shared/src/AMR.Shared/AMR.Shared.csproj", "shared/src/AMR.Shared/"]
COPY ["sales-microservice/src/AMR.SalesMicroservice.Business/AMR.SalesMicroservice.Business.csproj", "sales-microservice/src/AMR.SalesMicroservice.Business/"]
COPY ["sales-microservice/src/AMR.SalesMicroservice.Infrastructure/AMR.SalesMicroservice.Infrastructure.csproj", "sales-microservice/src/AMR.SalesMicroservice.Infrastructure/"]
RUN dotnet restore "sales-microservice/src/AMR.SalesMicroservice/AMR.SalesMicroservice.csproj"
COPY . .
WORKDIR "/src/sales-microservice/src/AMR.SalesMicroservice"
RUN dotnet build "AMR.SalesMicroservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AMR.SalesMicroservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AMR.SalesMicroservice.dll"]