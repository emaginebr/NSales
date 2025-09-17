docker build -t crosschainswap-api -f BTCSTXSwap.API\Dockerfile .
docker run -p 8080:80 crosschainswap-api