#### Migration Scripts

```bash

dotnet ef migrations add InitialOutboxMigration -o \Migrations\Outbox -c OutboxDataContext
dotnet ef database update -c OutboxDataContext
```
