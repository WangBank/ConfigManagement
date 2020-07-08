FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

FROM base AS final
WORKDIR /app
COPY . .
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
ENTRYPOINT ["dotnet", "ConfigManagement.dll"]