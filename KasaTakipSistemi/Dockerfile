# Projeyi derlemek i�in .NET 8 SDK imaj�n� kullan
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Proje dosyalar�n� kopyala ve ba��ml�l�klar� y�kle
COPY *.csproj ./
RUN dotnet restore

# T�m proje dosyalar�n� kopyala
COPY . ./

# Projeyi yay�nla (Release modunda)
RUN dotnet publish -c Release -o out

# �al��t�rma ortam� i�in ASP.NET 8 Runtime imaj�n� kullan
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Uygulaman�n hangi portu dinleyece�ini belirt
# Render, PORT ad�nda bir ortam de�i�keni sa�lar, onu kullanal�m.
# ENV ASPNETCORE_URLS=http://+:10000 sat�r�n� siliyoruz veya yorum yap�yoruz.

# Uygulamay� ba�lat
ENTRYPOINT ["dotnet", "KasaTakipSistemi.dll"]