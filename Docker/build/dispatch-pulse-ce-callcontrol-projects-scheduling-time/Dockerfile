
FROM debian:bullseye

# Add packages we need.
RUN apt-get update && apt-get install -y \
	tini \
	asterisk \
	openssh-server \
	&& rm -rf /var/lib/apt/lists/*


RUN rm -rv /etc/asterisk/*
COPY asterisk-config-files/* /etc/asterisk/

COPY entry.sh /entry.sh
RUN chmod +x /entry.sh
COPY script.sh /script.sh
RUN chmod +x /script.sh

ENTRYPOINT ["/usr/bin/tini", "--", "/entry.sh"]
CMD ["bash"]

