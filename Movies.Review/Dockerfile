FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "Movies.Review/Movies.Review.csproj"
RUN dotnet publish "Movies.Review/Movies.Review.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8082
ENV ASPNETCORE_URLS=http://+:8082
ENTRYPOINT ["dotnet", "Movies.Review.dll"]