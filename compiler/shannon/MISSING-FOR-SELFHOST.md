# Shannon: Complete Feature Audit

Audit date: 2026-02-04

## Legend
- ✓ = Has handler wired in try-*
- ✗ = In dispatch table, NO handler
- - = Not in dispatch table

---

## Stack Operations (F_STACKOP)

| Word | Handler |
|------|---------|
| dup | ✓ |
| drop | ✓ |
| swap | ✓ |
| over | ✓ |
| rot | ✓ |
| nip | ✓ |
| tuck | ✓ |
| 2dup | ✓ |
| 2drop | ✓ |
| 2swap | ✗ |
| 2over | ✗ |
| -rot | ✗ |
| ?dup | ✗ |
| depth | ✗ |
| pick | ✗ |
| dup2 | ✗ |

---

## Binary Arithmetic - Commutative (F_FOLD2C)

| Word | Handler |
|------|---------|
| + | ✓ |
| * | ✓ |
| and | ✓ |
| or | ✓ |
| xor | ✓ |
| min | ✓ |
| max | ✓ |
| = | ✓ |
| <> | ✗ |
| d+ | ✗ |

---

## Binary Arithmetic - Non-commutative (F_FOLD2N)

| Word | Handler |
|------|---------|
| - | ✓ |
| / | ✗ |
| mod | ✓ |
| /mod | ✓ |
| */mod | ✗ |
| */ | ✗ |
| lshift | ✓ |
| rshift | ✓ |
| < | ✓ |
| > | ✓ |
| <= | ✗ |
| >= | ✗ |
| u< | ✗ |
| within | ✗ |
| um* | ✗ |
| m* | ✗ |
| um/mod | ✗ |
| sm/rem | ✗ |
| fm/mod | ✗ |
| d- | ✗ |

---

## Unary Arithmetic (F_FOLD1)

| Word | Handler |
|------|---------|
| negate | ✓ |
| invert | ✓ |
| abs | ✓ |
| 1+ | ✓ |
| 1- | ✓ |
| 2* | ✓ |
| 2/ | ✓ |
| 2+ | ✗ |
| 2- | ✗ |
| cells | ✓ |
| cell+ | ✗ |
| chars | ✗ |
| char+ | ✗ |
| 0= | ✓ |
| 0< | ✓ |
| 0> | ✓ |
| 0<> | ✗ |
| s>d | ✗ |
| >body | ✗ |
| count | ✗ |
| nos+ | ✗ |
| tuck+ | ✗ |

---

## Constants (F_FOLD1)

| Word | Handler |
|------|---------|
| bl | ✗ |
| true | ✗ |
| false | ✗ |
| r/o | ✗ |
| w/o | ✗ |
| r/w | ✗ |

---

## Memory (F_MEMORY)

| Word | Handler |
|------|---------|
| @ | ✓ |
| ! | ✓ |
| c@ | ✓ |
| c! | ✓ |
| +! | ✓ |
| move | ✗ |
| fill | ✗ |
| base | ✗ |
| >in | ✗ |
| here | ✗ |
| , | ✗ |
| c, | ✗ |
| s, | ✗ |

---

## I/O (F_IO)

| Word | Handler |
|------|---------|
| . | ✓ |
| cr | ✓ |
| emit | ✓ |
| type | ✓ |
| space | ✓ |
| spaces | ✗ |
| u. | ✗ |
| key | ✓ |
| <# | ✗ |
| hold | ✗ |
| sign | ✗ |
| # | ✗ |
| #s | ✗ |
| #> | ✗ |
| decimal | ✗ |
| source | ✗ |
| parse | ✗ |
| word | ✗ |
| accept | ✗ |
| refill | ✗ |
| find | ✗ |
| ' | ✗ (compile-tick exists but NOT WIRED) |
| interpret | ✗ |
| evaluate | ✗ |
| open-file | ✗ |
| create-file | ✗ |
| close-file | ✗ |
| read-file | ✗ |
| write-file | ✗ |
| slurp-file | ✗ |
| include | ✗ |
| argc | ✗ |
| argv | ✗ |

---

## Return Stack (F_RSTACK)

| Word | Handler |
|------|---------|
| >r | ✓ |
| r> | ✓ |
| r@ | ✓ |
| 2>r | ✓ |
| 2r> | ✓ |
| 2r@ | ✓ |

---

## Control Flow (F_CONTROL)

| Word | Handler |
|------|---------|
| if | ✓ |
| then | ✓ |
| else | ✓ |
| begin | ✓ |
| while | ✓ |
| repeat | ✓ |
| until | ✓ |
| again | ✓ |
| do | ✓ |
| ?do | ✗ |
| loop | ✓ |
| +loop | ✗ |
| i | ✓ |
| j | ✓ |
| leave | ✗ |
| unloop | ✓ |
| exit | ✓ |
| recurse | ✓ |
| recursive | ✗ |
| execute | ✗ |
| [ | ✗ |
| ] | ✗ |
| literal | ✗ |
| postpone | ✗ |
| does> | ✗ |
| quit | ✗ |
| abort | ✗ |
| throw | ✗ |
| [char] | ✓ |

---

## Fused Compare+Branch (F_CONTROL)

| Word | Handler |
|------|---------|
| <if | ✓ |
| >if | ✓ |
| =if | ✓ |
| 0<if | ✓ |
| 0=if | ✓ |
| 0=until | ✗ |
| nzloop | ✗ |
| 1-nzloop | ✗ |

---

## Strings

| Word | Handler |
|------|---------|
| s" | ✓ |
| ." | ✓ |
| [char] | ✓ |
| char | ✓ |
| ['] | ✗ |

---

## Definitions (handled in main.fs compile-all)

| Word | Handler |
|------|---------|
| : | ✓ |
| ; | ✓ |
| variable | ✓ |
| constant | ✓ |
| create | ✓ |
| allot | ✓ |
| immediate | ✓ (just added) |

---

## Summary

**Total dispatch entries:** ~115
**Handlers implemented:** ~50
**Missing handlers:** ~65

## ROOT CAUSE

**sixth.fs: 189 handlers. Shannon: 91 handlers.**

But the interesting question isn't "how many handlers?" It's "how many does it need?"

## THE ACTUAL DELTA - 12 Words Blocking Self-Hosting

| Word | Uses in Shannon | sixth.fs has handler | Shannon has handler |
|------|-----------------|---------------------|-------------------|
| include | 65x | YES | NO |
| move | 18x | YES | NO |
| throw | 17x | YES | NO |
| bye | 12x | ? | NO |
| fill | 9x | YES | NO |
| argv | 7x | YES | NO |
| argc | 6x | YES | NO |
| write-file | 4x | YES | NO |
| create-file | 3x | YES | NO |
| close-file | 3x | YES | NO |
| w/o | 3x | YES | NO |
| slurp-file | 2x | YES | NO |

**That's 12 words blocking self-hosting. Not 98. Twelve.**

The handlers exist in sixth.fs. They need to be ported to Shannon modules.

### Not even in dispatch table:
- `/string` (used 4x in main.fs parse-number)
- `bye` (used 12x in main.fs)

### Critical for self-hosting (used in Shannon source):
- include (15x)
- ?do (6x)
- move (5x)
- fill (2x)
- slurp-file (1x)
- create-file, write-file, close-file (elf.fs)
- w/o (elf.fs)
- throw (elf.fs)

### Defined elsewhere but not wired:
- ' (compile-tick in strings.fs - orphaned)
- >= <= (defined in main.fs as Forth words, not native)
