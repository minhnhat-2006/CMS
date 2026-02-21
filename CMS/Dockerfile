# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# SỬA Ở ĐÂY: Trỏ đúng vào thư mục CMS/CMS để lấy file csproj
COPY CMS/CMS/*.csproj ./
RUN dotnet restore

# Copy toàn bộ code từ thư mục CMS/CMS vào
COPY CMS/CMS/ .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CMS.dll"]