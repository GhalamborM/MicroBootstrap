## Migration Scripts

```bash

dotnet ef migrations add InitialInternalMessagesMigration -o \Migrations\Data\Migrations\ -c InternalMessageDbContext
dotnet ef database update -c InternalMessageDbContext
```
