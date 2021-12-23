# Publishing to Docker Hub

1. cd to *.sln directory
2. docker build -t signalr-server:tagname -f .\SignalRServer\Dockerfile .
2. docker tag signalr-server:tagname xclayl/signalr-server:tagname
2. docker push xclayl/signalr-server:tagname
5. Update Docker Hub page if needed