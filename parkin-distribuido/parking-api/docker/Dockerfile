FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN sed -i "s/main/main contrib non-free/" /etc/apt/sources.list.d/debian.sources 2>/dev/null; apt-get update && apt-get install -y ttf-mscorefonts-installer fontconfig fonts-noto-cjk openssl && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /app/https && openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes -keyout /app/https/private.key -out /app/https/certificate.crt -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost" && openssl pkcs12 -export -out /app/https/aspnetapp.pfx -inkey /app/https/private.key -in /app/https/certificate.crt -password pass:CREDENTIAL_PLACEHOLDER
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=CREDENTIAL_PLACEHOLDER
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/https/aspnetapp.pfx
WORKDIR /app
EXPOSE 80
COPY publish/ .
ENTRYPOINT ["dotnet", "Parking.Api.dll"]
