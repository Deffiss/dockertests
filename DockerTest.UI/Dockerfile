FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DockerTest.UI/DockerTest.UI.csproj", "DockerTest.UI/"]
RUN dotnet restore "DockerTest.UI/DockerTest.UI.csproj"
COPY . .
WORKDIR "/src/DockerTest.UI"
RUN dotnet build "DockerTest.UI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DockerTest.UI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DockerTest.UI.dll"]