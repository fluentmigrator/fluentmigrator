# Token substitution matrix - MySql

See `TokenSubstitutionMatrix.md` for what each column means.

### MySql4

- generator: `MySql4Generator`
- generator.Quoter: `MySqlQuoter`
- processor.Quoter: `MySqlQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `<throws NotSupportedException>` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### MySql5

- generator: `MySql5Generator`
- generator.Quoter: `MySqlQuoter`
- processor.Quoter: `MySqlQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `<throws NotSupportedException>` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### MySql8

- generator: `MySql8Generator`
- generator.Quoter: `MySqlQuoter`
- processor.Quoter: `MySqlQuoter`

| value | `$(x)` | `$[x]` no quoter | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `<throws NotSupportedException>` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

