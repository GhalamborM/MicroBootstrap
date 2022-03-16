## Migration Scripts

```bash

dotnet ef migrations add InitialCreate -c OutboxDataContext
dotnet ef database update -c OutboxDataContext

```
