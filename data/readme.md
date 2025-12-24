docker run -d \
  --name front_tube_db \
  -e POSTGRES_USER=root \
  -e POSTGRES_PASSWORD=password \
  -p 5656:5432 \
  -v "/home/nd/Documents/My Repos/FrontTube/db:/var/lib/postgresql/data:Z" \
  postgres:16


PGPASSWORD=password psql -h localhost -p 5656 -U root -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'front_tube' AND pid <> pg_backend_pid();" && PGPASSWORD=password psql -h localhost -p 5656 -U root -d postgres -c "DROP DATABASE IF EXISTS front_tube;" && PGPASSWORD=password psql -h localhost -p 5656 -U root -d postgres -c "CREATE DATABASE front_tube;" && echo "DB recreated"