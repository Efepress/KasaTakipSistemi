# Projeyi derlemek için .NET 8 SDK imajýný kullan
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# --- DEÐÝÞÝKLÝK BURADA BAÞLIYOR ---
# Önce sadece .sln ve .csproj dosyalarýný kopyala
# Bu, katmanlý önbellekleme (layer caching) için en iyi yöntemdir
COPY *.sln .
COPY KasaTakipSistemi/*.csproj ./KasaTakipSistemi/

# Baðýmlýlýklarý restore et
RUN dotnet restore

# Þimdi projenin geri kalan tüm dosyalarýný kopyala
COPY . .

# --- DEÐÝÞÝKLÝK BURADA BÝTÝYOR ---

# Projeyi yayýnla, çýktýlarý /app/out klasörüne koy
# Yayýnlarken proje dosyasýnýn tam yolunu belirtiyoruz
RUN dotnet publish KasaTakipSistemi/KasaTakipSistemi.csproj -c Release -o out

# Çalýþtýrma ortamý için ASP.NET 8 Runtime imajýný kullan
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Uygulamayý baþlat
ENTRYPOINT ["dotnet", "KasaTakipSistemi.dll"]