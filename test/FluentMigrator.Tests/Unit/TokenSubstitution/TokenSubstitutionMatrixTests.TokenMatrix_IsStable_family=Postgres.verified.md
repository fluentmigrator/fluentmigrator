# Token substitution matrix - Postgres

See `TokenSubstitutionMatrix.md` for what each column means.

### Postgres(ForceQuote=True)

- generator: `Postgres15_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres92(ForceQuote=True)

- generator: `Postgres92Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres10_0(ForceQuote=True)

- generator: `Postgres10_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres11_0(ForceQuote=True)

- generator: `Postgres11_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres15_0(ForceQuote=True)

- generator: `Postgres15_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres(ForceQuote=False)

- generator: `Postgres15_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres92(ForceQuote=False)

- generator: `Postgres92Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres10_0(ForceQuote=False)

- generator: `Postgres10_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres11_0(ForceQuote=False)

- generator: `Postgres11_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Postgres15_0(ForceQuote=False)

- generator: `Postgres15_0Generator`
- generator.Quoter: `PostgresQuoter`
- processor.Quoter: `PostgresQuoter`

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
| SystemMethods | `CurrentDateTime` | `'CurrentDateTime'` | `now()` | `now()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `'System.Byte[]'` | `E'\\xDEAD'` | `E'\\xDEAD'` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `'FluentMigrator.RawSql'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `'FluentMigrator.RawSql'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

