# CLAUDE.md - Sixth Project Context

## PRIMARY PURPOSE

**Build a compiler that compiles Forth-like code to native x86-64 FASTER than GCC -O2 compiles C, with runtime performance at parity or better.**

This is the only goal that matters. Everything else serves this:
- Inlining builtins → runtime speed
- Constant folding → runtime speed
- No interpretation overhead → compile speed + runtime speed
- Self-hosting → delete C dependency, prove the compiler works

Do NOT get distracted by:
- "Forth purity" or traditional Forth architecture
- Making INTERPRET work for arbitrary runtime code
- Chuck Moore's 133 words or threaded code models

The C interpreter is scaffolding for bootstrap. Once sixth compiles sixth, C is deleted.

## What Sixth Is

Sixth is a native x86-64 compiler for a Forth-like language. It compiles source directly to ELF binaries with no runtime interpreter. Built on Fifth, its predecessor.

**Name origin**: Forth -> Fifth -> Sixth. Each generation advancing Forth for modern development.

## Project Structure

```
~/sixth/
├── engine/               C interpreter (the runtime)
│   ├── sixth.c           Main entry point
│   ├── vm.c              Virtual machine core
│   ├── prims.c           Primitive words
│   └── io.c              I/O and file operations
├── compiler/             Native compiler (sixth.fs)
├── examples/             Example applications
└── sixth                 CLI wrapper script

~/.sixth/                 Package system (SIXTH_HOME)
├── lib/                  Core libraries
│   ├── str.fs            String buffers, parsing
│   ├── html.fs           HTML generation
│   ├── sql.fs            SQLite interface
│   ├── template.fs       Template system
│   ├── ui.fs             UI components
│   ├── pkg.fs            Package system
│   └── core.fs           Loads all libraries
└── packages/             Installed packages
    └── claude-tools/     Example package
```

## Core Principles

1. **No dynamic allocation** - Use static buffers (`str-reset` / `str+` / `str$`), never `allocate`/`free`
2. **Shell-out pattern** - No C bindings. SQLite via `sqlite3` CLI, file open via `open` command
3. **HTML escaping by default** - `text` escapes, `raw` bypasses. Never use `raw` for user data
4. **Stack comments everywhere** - Every word needs `( before -- after )` documentation
5. **Composable words** - Small words that combine. No monolithic definitions

## Critical Forth Knowledge

### Things That Will Break You

- **Word spacing**: `</div>nl` is ONE undefined word. `</div> nl` is TWO words. Forth tokenizes on whitespace only.
- **`s"` has no escapes**: Use `s\"` for embedded quotes (`s\" ...\"...\"..."`). Standard `s"` treats backslash as literal.
- **`s+` crashes**: Dynamic string concatenation causes memory errors. Always use buffer pattern.
- **Stack errors = cryptic crashes**: "Invalid memory address" usually means a stack imbalance. Add `.s` calls to debug.
- **SQL single quotes**: Shell quoting uses single quotes around the SQL. SQL string literals inside conflict. Avoid `WHERE col='value'`; use numeric comparisons, ORDER BY, or parameter workarounds.

### Buffer System

Two independent buffers to avoid conflicts:

| Buffer | Words | Used By |
|--------|-------|---------|
| Primary (`str-buf`) | `str-reset` `str+` `str$` `str-char` | General string building, CSS classes, shell commands |
| Secondary (`str2-buf`) | `str2-reset` `str2+` `str2$` | `html-escape` (so escaping doesn't corrupt primary buffer) |

**Rule**: Never nest operations on the same buffer. If you need to build a string inside `html-escape`, use the primary buffer (html-escape uses secondary).

### Stack Discipline

```forth
\ WRONG - loses items
: bad  ( a b c -- ) swap drop ;  \ What happened to a?

\ RIGHT - document everything
: good ( addr u n -- addr u field-addr field-u )
  \ Extract nth field from pipe-delimited string
  ... ;
```

Common patterns:
- `2>r ... 2r>` - Save/restore string pair on return stack
- `2swap` - Exchange two string pairs: `( a1 u1 a2 u2 -- a2 u2 a1 u1 )`
- `2dup` - Copy top string pair: `( a u -- a u a u )`
- `2drop` - Discard string pair: `( a u -- )`
- `-rot` vs `swap` - Triple rotation vs pair swap. Getting these wrong causes null pointer crashes.

## Library Dependencies

```
str.fs          (standalone)
html.fs     --> str.fs
sql.fs      --> str.fs
template.fs --> html.fs --> str.fs
ui.fs       --> html.fs, template.fs
pkg.fs      --> str.fs
core.fs     --> str.fs, html.fs, sql.fs, pkg.fs
```

## Commands

```bash
# Run examples
./sixth examples/db-viewer.fs
./sixth examples/project-dashboard.fs

# One-liner
./sixth -e "2 3 + . cr"

# Interactive REPL
./sixth

# Load core libraries interactively
./sixth -e "require ~/.sixth/lib/pkg.fs use lib:core.fs"

# Package commands
./sixth pkg list
./sixth pkg path
```

## Package System

```forth
\ Bootstrap the package system
require ~/.sixth/lib/pkg.fs

\ Load libraries from ~/.sixth/lib/
use lib:core.fs
use lib:str.fs

\ Load packages from ~/.sixth/packages/
use pkg:claude-tools
```

## HTML Output Pattern

All examples follow this pattern:

```forth
s" /tmp/output.html" w/o create-file throw html>file

s" Page Title" html-head    \ Opens <!DOCTYPE>, <html>, <head>, <title>
  <style> ... </style>      \ CSS while head is still open
  ui-css                    \ Component styles
html-body                   \ Closes </head>, opens <body>

  \ ... page content ...

ui-js                       \ Tab switching JavaScript
html-end                    \ Closes </body></html>

html-fid @ close-file throw
```

**Key**: `html-head` leaves `<head>` open so you can inject `<style>` blocks. `html-body` closes it.

## SQL Query Pattern

```forth
s" path/to/db.db" s" SELECT col1, col2 FROM table" sql-exec
sql-open
begin sql-row? while
  dup 0> if
    2dup 0 sql-field type    \ first column
    2dup 1 sql-field type    \ second column
    2drop                    \ drop the row string
  else 2drop then
repeat 2drop
sql-close
```

Results are pipe-delimited. `sql-field` extracts by 0-based index.

## Conventions

- Core libraries go in `~/.sixth/lib/`
- Packages go in `~/.sixth/packages/NAME/`
- Every `.fs` file starts with a comment block: `\ sixth/path/file.fs - Description`
- Use `require` not `include` (prevents double-loading)
- CSS class names use kebab-case: `stat-card`, `grid-auto`, `bg-primary`
- Word names follow Forth convention: `<tag>`, `</tag>`, `tag.` (dot = convenience with content)

## What NOT To Do

- Don't use `allocate`/`free` for strings
- Don't try to `include` the same file twice (use `require`)
- Don't put single-quoted SQL literals in shell commands
- Don't assume `s"` strings persist after the word returns (they're transient)
- Don't redefine standard Forth words (`emit-file`, `type`, etc.)
- Don't create words with embedded whitespace (impossible in Forth)

## Testing

### Run Tests

```bash
./compiler/tests/test              # Run all tests (parallel)
./compiler/tests/test "1000*"      # Run tests matching pattern
VERBOSE=1 ./compiler/tests/test    # Show per-test timing
```

Output:
```
Running 44 tests...
............................................

TOTAL: 44  PASS: 44  WRONG: 0  CFAIL: 0  RFAIL: 0
Wall: 755ms  Compile: 469ms  Run: 45ms
```

- **Wall** - parallel elapsed time
- **Compile** - sum of individual compile times
- **Run** - sum of individual run times

### Test Structure

```
compiler/tests/
├── test              # Parallel test runner (canonical)
├── *.fs              # 46 compiler-specific tests
├── hayes/            # ANS Forth compliance (37 files) - DO NOT MODIFY
└── adversarial/      # Stress tests (34 files)
```

### Test Format

```forth
\ expect: <expected output>
: main <test code> ;
```

Empty `expect:` = test passes if it compiles and exits cleanly.

### Manual Single Test

```bash
./sixth compiler/sixth.fs compiler/tests/1000-fold-add.fs /tmp/t && /tmp/t
```

## Test Policy

**Hayes tests cover standard Forth. Do not duplicate.**

Write tests ONLY for:
1. **Constant folding** - `1000-fold-*.fs`
2. **Instruction fusing** - `1016-fuse-*.fs`
3. **Forward references** - `1030-fwd-ref-*.fs`
4. **Sixth runtime words** - `1000-refill-eof.fs`, `1003-find-*.fs`, etc.
5. **Strength reduction** - `2100-strength-*.fs`
6. **Primitive codegen sanity** - `01-lit.fs`, `07-dup.fs`, etc.

**If Hayes covers it, don't write a test.**
