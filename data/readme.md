docker run -d \
  --name ftube_db \
  -e POSTGRES_USER=root \
  -e POSTGRES_PASSWORD=password \
  -p 5656:5432 \
  -v "/home/nd/Documents/My Repos/FrontTube/db:/var/lib/postgresql/data:Z" \
  postgres:16
