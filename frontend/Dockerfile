FROM node:18-alpine AS dev
WORKDIR /app

COPY ./frontend/package*.json ./
RUN npm install

COPY ./frontend .

ENV PORT=${VITE_FRONTEND_PORT}

EXPOSE ${PORT}

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "${PORT}"]

