# Publishing to Docker Hub

1. cd to *.sln directory
2. docker build -t signalr-server:tagname -f .\SignalRServer\Dockerfile .
2. docker tag signalr-server:tagname xclayl/signalr-server:tagname
2. docker push xclayl/signalr-server:tagname
5. Tag latest?
5. Update Docker Hub page if needed

# Run image

1. docker run --rm -it -e Allow_Anonymous=TRUE -p 127.0.0.1:8080:80 xclayl/signalr-server:tagname 