
FROM debian:bullseye

# Add packages we need.
RUN apt-get update && apt-get install -y \
	tini \
	asterisk \
	openssh-server \
	&& rm -rf /var/lib/apt/lists/*


ENTRYPOINT ["/usr/bin/tini", "--", "/entry.sh"]
CMD ["bash"]

