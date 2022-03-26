FROM node:bullseye
RUN npm install -g s3rver@3.7.1

RUN mkdir -p /data
VOLUME ["/data"]
EXPOSE 5000
CMD [ "s3rver", "--hostname", "0.0.0.0", "--port", "5000", "--directory", "/data" ]