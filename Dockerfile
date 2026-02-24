# 1. بنستخدم الـ SDK بتاع .NET 10 عشان نبني المشروع
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# نسخ ملفات الـ .csproj لكل المشاريع (زي ما هي في الريبو بتاعك)
COPY ["PMS.API/PMS.API.csproj", "PMS.API/"]
COPY ["PMS.Application/PMS.Application.csproj", "PMS.Application/"]
COPY ["PMS.Domain/PMS.Domain.csproj", "PMS.Domain/"]
COPY ["PMS.Infrastructure/PMS.Infrastructure.csproj", "PMS.Infrastructure/"]

# بنعمل Restore للـ Packages
RUN dotnet restore "PMS.API/PMS.API.csproj"

# بننسخ الكود كله
COPY . .

# بنعمل Publish للنسخة النهائية
WORKDIR "/src/PMS.API"
RUN dotnet publish "PMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. بنستخدم الـ Runtime بتاع .NET 10 عشان نشغل الـ API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# تشغيل المشروع
ENTRYPOINT ["dotnet", "PMS.API.dll"]