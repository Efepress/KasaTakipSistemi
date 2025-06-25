# Projeyi derlemek için .NET 8 SDK imajýný kullan
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Proje dosyalarýný kopyala ve baðýmlýlýklarý yükle
COPY *.csproj ./
RUN dotnet restore

# Tüm proje dosyalarýný kopyala
COPY . ./

# Projeyi yayýnla (Release modunda)
RUN dotnet publish -c Release -o out

# Çalýþtýrma ortamý için ASP.NET 8 Runtime imajýný kullan
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Uygulamanýn hangi portu dinleyeceðini belirt
# Render, PORT adýnda bir ortam deðiþkeni saðlar, onu kullanalým.
# ENV ASPNETCORE_URLS=http://+:10000 satýrýný siliyoruz veya yorum yapýyoruz.

# Uygulamayý baþlat
ENTRYPOINT ["dotnet", "KasaTakipSistemi.dll"]