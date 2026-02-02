# CLAUDE.md - Sixth Project Context

## What Sixth Is

Sixth is a practical Forth ecosystem with its own interpreter, compiler, and standard libraries. Built on Fifth, its predecessor. No external dependencies beyond standard Unix tools (sqlite3 for database features).

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

## NO BASH FOR TESTING

**This is Forth. Use Forth.**

NEVER use bash heredocs, pipes, or shell scripting to test Forth code. That's Unix brain damage.

### Native Compiler Tests

Test files live in `compiler/tests/`. Pattern:
```forth
\ expect: <expected output>
\ Description
: main <test code> ;
```

Add a test:
1. Create `compiler/tests/NNNN-name.fs` with the pattern above
2. Run: `./sixth compiler/tests/run.fs`

Example:
```forth
\ expect: 5
\ Test addition
: main 2 3 + . cr ;
```

Manual single test:
```bash
./sixth compiler/sixth.fs compiler/tests/1000-refill-eof.fs /tmp/t && /tmp/t
```

### Interpreter Tests

```bash
./fifth test.fs
./fifth -e "1 2 + . cr"
```

No heredocs. No pipes. No bash string manipulation. Write a `.fs` file or use `-e`.

## Test Policy

**Hayes tests (`compiler/tests/hayes/`) cover standard Forth.** Do not duplicate.

Hayes tests thousands of edge cases for: `+`, `-`, `*`, `/`, `DO`, `LOOP`, `IF`, `ELSE`, `RECURSE`, `>R`, `R>`, and every other ANS Forth word. If it's standard Forth behavior, Hayes already tests it.

**Write tests ONLY for:**

1. **Compiler optimizations** - Constant folding, instruction fusing, peephole patterns
2. **Codegen bugs** - Wrong assembly output, register allocation issues, not Forth semantics
3. **Words unique to Sixth** - Anything not in ANS Forth
4. **Regression tests** - Specific bug fixes with a ticket/commit reference

**Test file locations:**

| Range | Purpose |
|-------|---------|
| `01-99` | Primitive codegen verification |
| `100-999` | Combined operations, control flow codegen |
| `1000-1049` | Compiler optimizations (folding, fusing, fwd-refs) |
| `1000+` (named) | Integration tests (algorithms exercising whole compiler) |

**Before writing a test, ask:** Does Hayes already cover this? If yes, do not write the test.

## TEST ENFORCEMENT - MANDATORY

**YOU MUST USE `new-test.sh` TO CREATE TESTS. NO EXCEPTIONS.**

```bash
./compiler/tests/new-test.sh <number> <name> <category> <reason>
```

Example:
```bash
./compiler/tests/new-test.sh 1050 fold-lshift fold "Verify left-shift folding eliminates runtime op"
```

**Direct file creation is FORBIDDEN.** The script:
1. Requires a valid category (codegen/optimization/fold/fuse/fwd-ref/regression/sixth-word/integration)
2. Requires a reason explaining WHY this test exists
3. Rejects reasons that smell like standard Forth testing
4. Creates the file with required headers

**Tests without proper headers will be deleted.**

Every test file MUST have:
```forth
\ expect: <output>
\ category: <valid-category>
\ reason: <why this test exists, what compiler behavior it verifies>
```

**If you cannot articulate why Hayes doesn't cover this, you should not write the test.**
