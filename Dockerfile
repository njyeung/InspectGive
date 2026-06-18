FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN apt-get update && apt-get install -y \
    curl \
    jq \
    zip \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

COPY . .

RUN chmod +x ./build.sh

ENTRYPOINT ["/bin/bash", "-c"]
CMD ["./build.sh && dotnet publish -c Release -o /out/InspectGive && cd /out && zip -r InspectGive.zip InspectGive"]