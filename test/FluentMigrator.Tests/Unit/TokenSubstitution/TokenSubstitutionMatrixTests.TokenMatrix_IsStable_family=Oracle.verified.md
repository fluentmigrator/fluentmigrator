# Token substitution matrix - Oracle

See `TokenSubstitutionMatrix.md` for what each column means.

### Oracle

- generator: `OracleGenerator`
- generator.Quoter: `OracleQuoter`
- processor.Quoter: `OracleQuoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111222233334444555555555555'` | `'11111111222233334444555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'0 1:2:3.0'` | `'0 1:2:3.0'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### OracleManaged

- generator: `OracleManagedGenerator`
- generator.Quoter: `OracleQuoter`
- processor.Quoter: `OracleQuoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111222233334444555555555555'` | `'11111111222233334444555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'0 1:2:3.0'` | `'0 1:2:3.0'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### DotConnectOracle

- generator: `OracleGenerator`
- generator.Quoter: `OracleQuoter`
- processor.Quoter: `OracleQuoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111222233334444555555555555'` | `'11111111222233334444555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'0 1:2:3.0'` | `'0 1:2:3.0'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Oracle12C

- generator: `Oracle12CGenerator`
- generator.Quoter: `OracleQuoter`
- processor.Quoter: `OracleQuoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111222233334444555555555555'` | `'11111111222233334444555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'0 1:2:3.0'` | `'0 1:2:3.0'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### Oracle12CManaged

- generator: `Oracle12CManagedGenerator`
- generator.Quoter: `OracleQuoter`
- processor.Quoter: `OracleQuoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111222233334444555555555555'` | `'11111111222233334444555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'0 1:2:3.0'` | `'0 1:2:3.0'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

### DotConnectOracle12C

- generator: `Oracle12CGenerator`
- generator.Quoter: `OracleQuoter`
- processor.Quoter: `OracleQuoter`

| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |
|---|---|---|---|
| null | *(empty)* | `NULL` | `NULL` |
| DBNull | *(empty)* | `NULL` | `NULL` |
| string | `O'Brien` | `'O''Brien'` | `'O''Brien'` |
| NonUnicodeString | `ansi` | `'ansi'` | `'ansi'` |
| char | `x` | `'x'` | `'x'` |
| bool | `True` | `1` | `1` |
| Guid | `11111111-2222-3333-4444-555555555555` | `'11111111222233334444555555555555'` | `'11111111222233334444555555555555'` |
| DateTime | `07/28/2026 13:45:06` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` |
| DateTimeOffset | `07/28/2026 13:45:06 -04:00` | `'2026-07-28T13:45:06-04:00'` | `'2026-07-28T13:45:06-04:00'` |
| double | `1234.5678` | `1234.5678` | `1234.5678` |
| float | `1234.5` | `1234.5` | `1234.5` |
| decimal | `1234.5678` | `1234.5678` | `1234.5678` |
| int | `-1234` | `-1234` | `-1234` |
| long(uncased) | `-1234` | `-1234` | `-1234` |
| short(uncased) | `-12` | `-12` | `-12` |
| byte(uncased) | `7` | `7` | `7` |
| SystemMethods | `CurrentDateTime` | `LOCALTIMESTAMP` | `LOCALTIMESTAMP` |
| Enum | `Tuesday` | `'Tuesday'` | `'Tuesday'` |
| byte[] | `System.Byte[]` | `0xdead` | `0xdead` |
| TimeSpan | `01:02:03` | `'0 1:2:3.0'` | `'0 1:2:3.0'` |
| RawSql | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` | `CURRENT_TIMESTAMP` |
| RawSql+backslash | `c REGEXP '\d+'` | `c REGEXP '\d+'` | `c REGEXP '\d+'` |
| RawSql+quote | `name = 'O''Brien'` | `name = 'O''Brien'` | `name = 'O''Brien'` |

