Due to .NET Framework not being shipped with the Roslyn compiler by default,
you are required to use 'old' syntax.
Some features that you cannot use:

- String interpolation ( `$"1+1: {1 + 1}"` )
- `out` variable definition in function call ( `TryGetValue("what", out var whatsValue)` )
- `is` variable definition in statement ( `if (x is int a) /* use a as int */ `
- Question mark for is-null check ( `nullValue?.ToString()` )