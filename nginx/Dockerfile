FROM alpine:3.8

RUN apk add --no-cache nginx-mod-http-lua

# Delete default config
RUN rm -r /etc/nginx/conf.d && rm /etc/nginx/nginx.conf

# Create folder for PID file
RUN mkdir -p /run/nginx

COPY ./nginx.conf /etc/nginx/nginx.conf
COPY ./run_nginx.sh /tmp/run_nginx.sh
RUN chmod +x /tmp/run_nginx.sh
RUN echo "ipv6" >> /etc/modules
RUN echo "http://dl-2.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories; \
    echo "http://dl-3.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories; \
    echo "http://dl-4.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories; \
    echo "http://dl-5.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories; \
	echo "http://dl-2.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories; \
    echo "http://dl-3.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories; \
    echo "http://dl-4.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories; \
    echo "http://dl-5.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories
RUN apk --update add bash python py-pip groff less mailcap vim && \
pip install awscli --upgrade --user && \
apk --no-cache add dos2unix curl wget iputils
RUN apk upgrade
RUN ln -sf /dev/stdout /etc/nginx/nginx.access.log && ln -sf /dev/stderr /etc/nginx/nginx.error.log
RUN dos2unix /tmp/run_nginx.sh