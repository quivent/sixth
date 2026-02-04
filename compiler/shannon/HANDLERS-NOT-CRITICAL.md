# Handlers in sixth.fs Not Needed for Shannon Self-Hosting

These handlers exist in sixth.fs but are NOT required for Shannon to compile itself.

---

## Internal Helpers (10)

These are internal codegen helpers, not Forth words.

| Handler | Description | Purpose |
|---------|-------------|---------|
| `3arg-promote` | Promote 3 stack items to registers | Internal stack management for 3-arg ops |
| `cmp-setup` | Set up comparison operation | Helper for comparison codegen |
| `epilogue` | Generate function epilogue | Exit syscall after main returns |
| `prologue` | Generate function prologue | Stack setup, argc/argv capture |
| `interpret-body` | Runtime interpreter loop body | For INTERPRET/EVALUATE - not needed in compiled code |
| `open-file-core` | Core file open syscall | Shared by open-file, create-file |
| `tail-recurse` | Tail call optimization | Jump instead of call for tail recursion |
| `until-fused` | Fused until with comparison | Optimization variant |
| `while-fused` | Fused while with comparison | Optimization variant |
| `rsub` | Reverse subtract | NOS - TOS instead of TOS - NOS |

---

## Standard Forth Words Not Used by Shannon (45)

These are standard Forth words that sixth.fs can compile, but Shannon's own source doesn't use them.

### Arithmetic Extensions

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `*/` | ( n1 n2 n3 -- n4 ) | n1*n2/n3 scaled | Shannon doesn't use scaled arithmetic |
| `*/mod` | ( n1 n2 n3 -- rem quot ) | n1*n2/n3 with remainder | Not used |
| `2+` | ( n -- n+2 ) | Add 2 | Shannon uses `1+ 1+` or `2 +` |
| `2-` | ( n -- n-2 ) | Subtract 2 | Not used |
| `d+` | ( d1 d2 -- d3 ) | Double-cell add | No double-cell arithmetic in Shannon |
| `d-` | ( d1 d2 -- d3 ) | Double-cell subtract | No double-cell arithmetic |
| `m*` | ( n1 n2 -- d ) | Signed multiply to double | Not used |
| `um*` | ( u1 u2 -- ud ) | Unsigned multiply to double | Not used |
| `um/mod` | ( ud u -- rem quot ) | Unsigned double divide | Not used |
| `sm/rem` | ( d n -- rem quot ) | Symmetric divide | Not used |
| `fm/mod` | ( d n -- rem quot ) | Floored divide | Not used |
| `s>d` | ( n -- d ) | Sign-extend to double | Not used |

### Comparison Extensions

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `u` | ( u1 u2 -- flag ) | Unsigned less than | Shannon uses signed comparisons |
| `within` | ( n lo hi -- flag ) | Range check | Not used |

### Stack Extensions

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `dup2` | ( x y -- x y x y ) | Duplicate pair (alias for 2dup) | Shannon uses `2dup` |
| `nos+` | ( n -- ) | Add to NOS | Internal optimization helper |
| `tuck+` | ( n1 n2 -- n1+n2 n2 ) | Add and tuck | Optimization pattern |
| `cell+` | ( addr -- addr+8 ) | Add cell size | Shannon uses `8 +` |
| `cells` | ( n -- n*8 ) | Multiply by cell size | Shannon HAS this - miscategorized |
| `count` | ( c-addr -- addr u ) | Get counted string | Shannon doesn't use counted strings |
| `>body` | ( xt -- addr ) | Get data field from XT | Not used |

### I/O Extensions

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `spaces` | ( n -- ) | Emit n spaces | Shannon uses `space` in loops |
| `accept` | ( addr n -- u ) | Read line from input | Not used for file compilation |
| `source` | ( -- addr u ) | Current input source | Not used |
| `parse` | ( char -- addr u ) | Parse delimited string | Shannon has own tokenizer |
| `word` | ( char -- c-addr ) | Parse to counted string | Not used |
| `find` | ( c-addr -- xt flag ) | Dictionary lookup | Shannon has own dict-find |
| `refill` | ( -- flag ) | Refill input buffer | Not used |
| `>in` | ( -- addr ) | Parse position variable | Shannon tracks own input-pos |
| `base` | ( -- addr ) | Number base variable | Shannon hardcodes decimal/$hex |
| `hold` | ( char -- ) | Add to pictured output | Shannon doesn't use PNO |
| `sign` | ( n -- ) | Add sign to pictured output | Not used |

### Control Flow Extensions

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `abort` | ( -- ) | Abort execution | Shannon uses `throw` |
| `quit` | ( -- ) | Clear stacks, restart | Not used in compiled code |
| `evaluate` | ( addr u -- ) | Interpret string | Not needed - pure compiler |
| `interpret` | ( -- ) | Main interpreter loop | Not needed - pure compiler |
| `postpone` | ( "name" -- ) | Compile compilation semantics | Not used in Shannon source |

### File I/O Extensions

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `open-file` | ( addr u mode -- fid ior ) | Open existing file | Shannon uses slurp-file |
| `read-file` | ( addr u fid -- u2 ior ) | Read from file | Shannon uses slurp-file |
| `r/o` | ( -- mode ) | Read-only mode constant | Not used |
| `r/w` | ( -- mode ) | Read-write mode constant | Not used |

### Optimized Loop Variants

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `0=until` | ( -- ) | Fused `0= until` | Optimization - not in Shannon source |
| `nzloop` | ( -- ) | Non-zero loop | Optimization variant |
| `1-nzloop` | ( -- ) | Decrement and non-zero loop | Optimization variant |

### Miscellaneous

| Word | Stack Effect | Description | Why Not Needed |
|------|--------------|-------------|----------------|
| `div` | ( n1 n2 -- quot ) | Division (just quotient) | Shannon uses `/mod nip` pattern |
| `call` | Internal | Emit call instruction | Shannon has own call emission |
| `ret` | Internal | Emit ret instruction | Shannon has own ret emission |

---

## Summary

- **10 internal helpers**: Not Forth words, just codegen infrastructure
- **45 standard words**: Valid Forth but Shannon's source doesn't use them

None of these block self-hosting. They can be added later for ANS Forth compliance.
