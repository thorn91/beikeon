# Backend

## Running

### Dev

- `docker-compose.yml` will run everything similar to production
- running (mostly) outside of the container require starting the `dev-db` service in `docker-compose.yml` before running
  the local server via launchProfiles or the command line

## Dev DB

- Hot reloading is wack inside of a docker container, so we default to using a separate dockerized postgres
  for development if the `local db` run configuration is used.
- This is optional if hot reload is not desired

## Tips and Tricks
