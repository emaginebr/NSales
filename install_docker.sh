docker build -t crosschainswap-api -f BTCSTXSwap.API\Dockerfile .
docker run -p 8080:80 crosschainswap-api


docker ps
docker network inspect docker-network
docker network create docker-network
#docker network connect docker-network postgres1
docker pull postgres
docker run --name postgres1 -e POSTGRES_PASSWORD=mysecretpassword --network docker-network -d postgres

docker stop monexup-api1
docker stop monexup-app1
docker stop monexup-api1
docker rm monexup-app1
cd Backend/MonexUp
docker build -t monexup-api -f MonexUp.API/Dockerfile .
docker run --name monexup-api1 -p 8080:443 -e ASPNETCORE_URLS="https://+" -e ASPNETCORE_HTTPS_PORTS=8080 --network docker-network monexup-api &
docker run --name monexup-api1 -e ASPNETCORE_URLS="https://+" -e ASPNETCORE_HTTPS_PORTS=443 --network docker-network monexup-api &
cd ../../Frontend/monexup-app
docker build -t monexup-app .
docker run --name monexup-app1 -p 80:80 -p 443:443 monexup-app &