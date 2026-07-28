# Token substitution matrix - Other

See `TokenSubstitutionMatrix.md` for what each column means.

### Db2

- generator: `Db2Generator`
- generator.Quoter: `Db2Quoter`
- processor.Quoter: `Db2Quoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `''` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `'True'` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'7/28/2026 1:45:06 PM'` | `'2026-07-28-13.45.06'` | `'2026-07-28-13.45.06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'7/28/2026 1:45:06 PM -04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `'1234.5'` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| int | `-1234` | `'-1234'` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `'-1234'` | `-1234` | `-1234` |
| short(uncased) | `-12` | `'-12'` | `-12` | `-12` |
| byte(uncased) | `7` | `'7'` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Db2ISeries

- generator: `Db2ISeriesGenerator`
- generator.Quoter: `Db2ISeriesQuoter`
- processor.Quoter: `Db2ISeriesQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `''` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `'True'` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'7/28/2026 1:45:06 PM'` | `'2026-07-28-13.45.06'` | `'2026-07-28-13.45.06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'7/28/2026 1:45:06 PM -04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `'1234.5'` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| int | `-1234` | `'-1234'` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `'-1234'` | `-1234` | `-1234` |
| short(uncased) | `-12` | `'-12'` | `-12` | `-12` |
| byte(uncased) | `7` | `'7'` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Firebird

- generator: `FirebirdGenerator`
- generator.Quoter: `FirebirdQuoter`
- processor.Quoter: `FirebirdQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `''` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `'True'` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'7/28/2026 1:45:06 PM'` | `'2026-07-28 13:45:06'` | `'2026-07-28 13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'7/28/2026 1:45:06 PM -04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `'1234.5'` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| int | `-1234` | `'-1234'` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `'-1234'` | `-1234` | `-1234` |
| short(uncased) | `-12` | `'-12'` | `-12` | `-12` |
| byte(uncased) | `7` | `'7'` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Hana8

- generator: `HanaGenerator`
- generator.Quoter: `HanaQuoter`
- processor.Quoter: `HanaQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `''` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `'True'` | `TRUE` | `TRUE` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'7/28/2026 1:45:06 PM'` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'7/28/2026 1:45:06 PM -04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `'1234.5'` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| int | `-1234` | `'-1234'` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `'-1234'` | `-1234` | `-1234` |
| short(uncased) | `-12` | `'-12'` | `-12` | `-12` |
| byte(uncased) | `7` | `'7'` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Hana10

- generator: `HanaGenerator`
- generator.Quoter: `HanaQuoter`
- processor.Quoter: `HanaQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `''` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `'True'` | `TRUE` | `TRUE` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'7/28/2026 1:45:06 PM'` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'7/28/2026 1:45:06 PM -04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `'1234.5'` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| int | `-1234` | `'-1234'` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `'-1234'` | `-1234` | `-1234` |
| short(uncased) | `-12` | `'-12'` | `-12` | `-12` |
| byte(uncased) | `7` | `'7'` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Redshift

- generator: `RedshiftGenerator`
- generator.Quoter: `RedshiftQuoter`
- processor.Quoter: `RedshiftQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `''` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `'True'` | `true` | `true` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'7/28/2026 1:45:06 PM'` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'7/28/2026 1:45:06 PM -04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `'1234.5'` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `'1234.5678'` | `1234.5678` | `1234.5678` |
| int | `-1234` | `'-1234'` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `'-1234'` | `-1234` | `-1234` |
| short(uncased) | `-12` | `'-12'` | `-12` | `-12` |
| byte(uncased) | `7` | `'7'` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `SYSDATE` | `SYSDATE` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

