# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Tự động tìm và copy các file project để restore
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ code và build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Đảm bảo lệnh này chạy đúng file dll của bạn
ENTRYPOINT ["dotnet", "CMS.dll"]