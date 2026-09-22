# SupportToolsServerDbPart

EF Core persistence of [SupportToolsServer](https://github.com/merabza/SupportToolsServer): `SupportToolsServerDbContext` (implements `ISupportToolsServerDbContext` from `SupportToolsServer.Application`) and the per-entity configurations.

| Project | Purpose |
|---|---|
| `SupportToolsServerDbPart.Db` | `SupportToolsServerDbContext` and `IEntityTypeConfiguration<T>` classes (`Configurations` folder) |

EF Core migrations live in the [SupportToolsServerDbTools](https://github.com/merabza/SupportToolsServerDbTools) repository (`SupportToolsServerDbTools.DbMigration` project).

## Repository layout — sibling repos are required

Projects reference sibling clones by relative path (`..\..\SupportToolsServer\...`, `..\..\SystemTools\...`), so the repositories must be cloned next to each other:

```
<root>\
├── SupportToolsServerDbPart\    this repository (SupportToolsServerDbPart.slnx lives here)
├── SupportToolsServer\          domain entities and application abstractions (merabza/SupportToolsServer)
├── SupportToolsServerShared\    API contracts (merabza/SupportToolsServerShared)
└── SystemTools\                 shared libraries (merabza/SystemTools)
```

## Build

```powershell
dotnet build SupportToolsServerDbPart.slnx
```

## License

[MIT](LICENSE)
