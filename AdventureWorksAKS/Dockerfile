FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["AdventureWorksAKS/AdventureWorksAKS.csproj", "AdventureWorksAKS/"]
RUN dotnet restore "AdventureWorksAKS/AdventureWorksAKS.csproj"
COPY . .
WORKDIR "/src/AdventureWorksAKS"
RUN dotnet build "AdventureWorksAKS.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AdventureWorksAKS.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AdventureWorksAKS.dll"]