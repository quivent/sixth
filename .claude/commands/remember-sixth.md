# /remember-sixth - Load Sixth Compiler Context

Load the compiler encoding and source into context for modification work.

---

## INSTRUCTIONS

1. Read `compiler/ENCODING.md` - the one-page map of the compiler
2. Read `compiler/sixth.fs` - the full source (read in chunks if needed)

After reading, confirm you have loaded:
- Data structures and their layouts
- State variables and valid values
- Compilation flow (5 steps)
- Key words and their stack effects
- Register allocation scheme
- The lazy evaluation trick

---

## USAGE

Call this command before:
- Modifying the compiler
- Adding new builtins
- Debugging codegen issues
- Understanding optimization passes

---

## CONTEXT RESTORATION

After loading, state what you now know:

```
SIXTH COMPILER LOADED
- code-buf: 4096 bytes at code-pos
- stack-depth: 0=empty, 1=rax, 2=+rbx, 3=+rcx, 4+=r15 memory
- ct-stack: compile-time constants for folding
- Optimizations: fold, fuse, swap-absorb, dup+cmp fusion
- Ready to modify compiler
```

If ENCODING.md is stale or missing, run `/reencode-sixth-compiler` first.
