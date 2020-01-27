FROM hashicorp/terraform:full

WORKDIR /terraform
COPY . .

RUN chmod +x deploy_manual.sh && \
    echo "ipv6" >> /etc/modules
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
RUN dos2unix deploy_manual.sh && export PATH=$GOPATH:~/.local/bin:$PATH && rm backend.tf