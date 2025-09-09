FROM node:18
WORKDIR /app
COPY package*.json ./
RUN npm install --development
COPY . .
EXPOSE 5000
CMD ["npm", "start"]
