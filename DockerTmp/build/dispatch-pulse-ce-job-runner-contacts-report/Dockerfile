# STAGE 1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Add packages we need.
RUN apt-get update && apt-get install -y \
	openssh-client \
	bash \
	ca-certificates \
	git \
	&& update-ca-certificates \
	&& rm -rf /var/lib/apt/lists/*
	
RUN mkdir -p ~/.ssh && chmod 700 ~/.ssh
RUN ssh-keyscan github.com >> ~/.ssh/known_hosts

WORKDIR /source
RUN git clone --depth 1 https://github.com/dsaul/DispatchPulse-CE -b master /source

WORKDIR /source
RUN ls -la

WORKDIR /source/C#/JobRunnerContactsReport
RUN dotnet restore

RUN dotnet publish --output /app --configuration Debug

# Cleanup Build
RUN apt-get remove --purge -y git openssh-client git && \
	apt-get autoremove -y


# STAGE 2
FROM mcr.microsoft.com/dotnet/runtime:6.0

WORKDIR /app
COPY --from=build /app .
#ENTRYPOINT ["dotnet", "API.dll"]

CMD ["./JobRunnerContactsReport"]


















