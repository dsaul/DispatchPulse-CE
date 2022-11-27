FROM mcr.microsoft.com/dotnet/runtime:6.0

RUN apt-get update && \
	apt-get install -y \
	ffmpeg \
	&& rm -rf /var/lib/apt/lists/*