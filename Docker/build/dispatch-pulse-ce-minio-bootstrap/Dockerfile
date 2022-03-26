# STAGE 1
FROM debian:bullseye


ADD https://dl.min.io/client/mc/release/linux-amd64/mc /usr/local/bin/mc
ADD minio-bootstrap.sh /usr/local/bin/minio-bootstrap.sh
RUN chmod +x /usr/local/bin/mc /usr/local/bin/minio-bootstrap.sh

#ENTRYPOINT ["dotnet", "API.dll"]

CMD ["/usr/local/bin/minio-bootstrap.sh"]


















