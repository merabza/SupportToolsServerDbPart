# SupportToolsServerDbPart

EF Core persistence of [SupportToolsServer](https://github.com/merabza/SupportToolsServer): `SupportToolsServerDbContext` (implements `ISupportToolsServerDbContext` from `SupportToolsServerCore.Application.Abstractions`), the per-entity configurations, the unit of work and the database DI registration. Both the server and [SupportToolsServerDbTools](https://github.com/merabza/SupportToolsServerDbTools) use it.

| Project | Purpose |
|---|---|
| `SupportToolsServerDbPart.Db` | `SupportToolsServerDbContext`, `IEntityTypeConfiguration<T>` classes (`Configurations` folder), `SupportToolsServerUnitOfWork` and `AddSupportToolsServerDatabase` (`DependencyInjection` folder: registers the context, `ISupportToolsServerDbContext` and `IUnitOfWork`) |

EF Core migrations live in the [SupportToolsServerDbTools](https://github.com/merabza/SupportToolsServerDbTools) repository (`SupportToolsServerDbTools.DbMigration` project).

## Repository layout — sibling repos are required

Projects reference sibling clones by relative path (`..\..\SupportToolsServerCore\...`, `..\..\SystemTools\...`), so the repositories must be cloned next to each other:

```
<root>\
├── SupportToolsServerDbPart\    this repository (SupportToolsServerDbPart.slnx lives here)
├── SupportToolsServerCore\      domain entities and application abstractions (merabza/SupportToolsServerCore)
└── SystemTools\                 shared libraries (merabza/SystemTools)
```

## Build

```powershell
dotnet build SupportToolsServerDbPart.slnx
```

## License

[MIT](LICENSE)
