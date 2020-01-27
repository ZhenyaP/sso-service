FROM debian:stretch

RUN echo 'deb http://nginx.org/packages/debian/ stretch nginx' >> /etc/apt/sources.list
RUN apt-get update -y && apt-get install --no-install-recommends -y ca-certificates curl netbase dos2unix wget gnupg2 dirmngr openssl && \	
    rm -rf /var/lib/apt/lists/*
RUN bash -c "while true; do \
 apt-key adv --no-tty --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 573BFD6B3D8FBC641079A6ABABF5BD827BD9BF62; \
 if [ \$? -eq 0 ]; then \
    break; \
 fi; \
 sleep 10; \
done"
RUN apt-get update -y && apt-get install --no-install-recommends -y nginx-nr-agent && \	
    rm -rf /var/lib/apt/lists/*

COPY start.sh /opt/
COPY nginx-nr-agent.ini /etc/nginx-nr-agent/
RUN chmod +x /opt/start.sh && chmod +rw /etc/nginx-nr-agent/nginx-nr-agent.ini && dos2unix /opt/start.sh