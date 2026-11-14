# Token substitution matrix - SqlServer

See `TokenSubstitutionMatrix.md` for what each column means.

### SqlServer(unversioned)

- generator: `SqlServer2016Generator`
- generator.Quoter: `SqlServer2008Quoter`
- processor.Quoter: `SqlServer2008Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### SqlServer2000

- generator: `SqlServer2000Generator`
- generator.Quoter: `SqlServer2000Quoter`
- processor.Quoter: `SqlServer2000Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### SqlServer2005

- generator: `SqlServer2005Generator`
- generator.Quoter: `SqlServer2005Quoter`
- processor.Quoter: `SqlServer2005Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### SqlServer2008

- generator: `SqlServer2008Generator`
- generator.Quoter: `SqlServer2008Quoter`
- processor.Quoter: `SqlServer2008Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### SqlServer2012

- generator: `SqlServer2012Generator`
- generator.Quoter: `SqlServer2008Quoter`
- processor.Quoter: `SqlServer2008Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### SqlServer2014

- generator: `SqlServer2014Generator`
- generator.Quoter: `SqlServer2008Quoter`
- processor.Quoter: `SqlServer2008Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### SqlServer2016

- generator: `SqlServer2016Generator`
- generator.Quoter: `SqlServer2008Quoter`
- processor.Quoter: `SqlServer2008Quoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `N'O''Brien'` | `N'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111-2222-3333-4444-555555555555'` | `'11111111-2222-3333-4444-555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `'2026-07-28T13:45:06'` | `'2026-07-28T13:45:06'` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `GETDATE()` | `GETDATE()` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'01:02:03'` | `'01:02:03'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

