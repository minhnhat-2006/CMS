# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy file csproj và restore trước để tận dụng cache của Docker
COPY ["CMS.csproj", "."]
RUN dotnet restore "./CMS.csproj"
# Copy toàn bộ code và build
COPY . .
RUN dotnet publish "CMS.csproj" -c Release -o /app/publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Đảm bảo tên file .dll khớp với tên Project của bạn
ENTRYPOINT ["dotnet", "CMS.dll"]