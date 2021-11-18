FROM mcr.microsoft.com/dotnet/aspnet:6.0

RUN apt-get update && \
	apt-get install -y \
	ffmpeg \
	&& rm -rf /var/lib/apt/lists/*