# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file csproj từ thư mục gốc vào để restore
COPY *.csproj ./
RUN dotnet restore

# Copy tất cả file từ thư mục gốc vào (vì code bạn nằm ở gốc rồi)
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CMS.dll"]