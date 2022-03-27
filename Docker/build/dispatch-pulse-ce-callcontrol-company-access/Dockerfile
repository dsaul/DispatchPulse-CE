
FROM debian:bullseye

# Add packages we need.
RUN apt-get update && apt-get install -y \
	asterisk \
	&& rm -rf /var/lib/apt/lists/*


ENTRYPOINT ["/sbin/tini", "--", "/entry.sh"]
CMD ["bash"]

