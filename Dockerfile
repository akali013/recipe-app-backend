# syntax=docker/dockerfile:1
# Source: https://docs.docker.com/guides/dotnet/containerize/

FROM --platform=$BUILDPLATFORM dhi.io/dotnet:9-sdk AS build
ARG TARGETARCH
COPY . /src
WORKDIR /src
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

# Use python image to run RecipeGetter.py to initialize recipe data in RecipeData.json
FROM python:3.13.13-alpine3.23 AS recipe-getter
WORKDIR /recipeData
COPY RecipeGetter.py .
RUN pip install requests pandas
RUN python RecipeGetter.py

FROM dhi.io/aspnetcore:9 AS final
WORKDIR /app
COPY --from=build /app .
COPY --from=recipe-getter /recipeData /app
ENTRYPOINT ["dotnet", "recipe-app-backend.dll"]