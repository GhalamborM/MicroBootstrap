## Migration Scripts

```bash

dotnet ef migrations add InitialCreate -c InternalMessageDbContext
dotnet ef database update -c InternalMessageDbContext
```
