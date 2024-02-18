# Base Commands
# - create migration: dotnet ef migrations add <migration_name>
# - update database: dotnet ef database update
# - remove last migration: dotnet ef migrations remove
# - remove all migrations: dotnet ef migrations remove
# - remove all migrations and database: dotnet ef database drop

CURRENT_DIR=$(pwd)

cd "$(dirname "$0")" || exit
cd .. || exit

rm -rf ./Migrations 
dotnet ef migrations add AddSalt -o data/migrations
cd "$CURRENT_DIR" || exit