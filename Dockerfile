#FROM node:18
FROM nginx:alpine 

WORKDIR /var/www

COPY monexup.com.chained.crt /etc/nginx/conf.d
COPY monexup.com.key /etc/nginx/conf.d
COPY build /var/www/monexup.com

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
