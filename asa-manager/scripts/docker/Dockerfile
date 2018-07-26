FROM microsoft/dotnet:2.0.3-runtime-jessie

MAINTAINER Devis Lucato (https://github.com/dluc)

LABEL Tags="Azure,IoT,Solutions,ASA,Azure Stream Analytics,.NET"

COPY . /app/
WORKDIR /app

RUN \
    # Ensures the entry point is executable
    chmod ugo+x /app/run.sh && \
    # Clean up destination folder
    rm -f /app/Dockerfile /app/.dockerignore

ENTRYPOINT ["/bin/bash", "/app/run.sh"]
