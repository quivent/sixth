# Fusion Patterns — Complete Enumeration

## Count History

| Attempt | Count | Verdict |
|---------|-------|---------|
| First   | 15    | **Wrong.** Missed binary ops entirely. Treated 20+ patterns as 3. |
| Second  | 41    | **Wrong.** Missed `over+binary` (20), `dup+binary` (5), annihilation (5). |
| Third   | 80    | Correct. Full cross-product enumeration. |

A fusible pattern is a sequence of Forth words that can be compiled to fewer
or cheaper x86 instructions than compiling each word independently.

---

## Group A: swap + operation (32 patterns)

**Mechanism:** pending-swap

| # | Pattern | Test code | Expected | Adversarial property |
|---|---------|-----------|----------|---------------------|
| A01 | `swap 1+` | `3 7 swap 1+ . . cr` | `4 7` | swap then increment — wrong register gives 3 8 or 8 7 |
| A02 | `swap 1-` | `3 7 swap 1- . . cr` | `2 7` | swap then decrement — wrong register gives 3 6 or 6 7 |
| A03 | `swap 2+` | `3 7 swap 2+ . . cr` | `5 7` | swap then add 2 |
| A04 | `swap 2-` | `3 7 swap 2- . . cr` | `1 7` | swap then subtract 2 |
| A05 | `swap 2*` | `3 7 swap 2* . . cr` | `6 7` | swap then double — wrong register gives 3 14 |
| A06 | `swap 2/` | `4 7 swap 2/ . . cr` | `2 7` | swap then halve — 4/2=2 — wrong register gives 7 2 |
| A07 | `swap negate` | `-5 7 swap negate . . cr` | `5 7` | swap then negate — negate(-5)=5 — wrong register gives -5 -7 |
| A08 | `swap abs` | `-5 7 swap abs . . cr` | `5 7` | swap then abs — abs(-5)=5 — wrong register gives -5 7 |
| A09 | `swap invert` | `3 7 swap invert . . cr` | `-4 7` | swap then bitwise NOT — invert(3)=-4 — wrong register gives 3 -8 |
| A10 | `swap +` | `3 7 swap + . cr` | `10` | commutative — 7+3=3+7=10 |
| A11 | `swap *` | `3 7 swap * . cr` | `21` | commutative — 7*3=3*7=21 |
| A12 | `swap and` | `3 7 swap and . cr` | `3` | commutative — 3 AND 7 = 3 |
| A13 | `swap or` | `3 7 swap or . cr` | `7` | commutative — 3 OR 7 = 7 |
| A14 | `swap xor` | `3 7 swap xor . cr` | `4` | commutative — 3 XOR 7 = 4 |
| A15 | `swap =` | `3 7 swap = . cr` | `0` | commutative — 7=3 is false (0) |
| A16 | `swap <>` | `3 7 swap <> . cr` | `-1` | commutative — 7<>3 is true (-1) |
| A17 | `swap min` | `3 7 swap min . cr` | `3` | commutative — min(7,3)=min(3,7)=3 |
| A18 | `swap max` | `3 7 swap max . cr` | `7` | commutative — max(7,3)=max(3,7)=7 |
| A19 | `swap -` | `3 7 swap - . cr` | `4` | ADVERSARIAL: 7-3=4 — without swap: 3-7=-4 |
| A20 | `swap /` | `7 21 swap / . cr` | `3` | ADVERSARIAL: 21/7=3 — without swap: 7/21=0 |
| A21 | `swap mod` | `7 22 swap mod . cr` | `1` | ADVERSARIAL: 22 mod 7=1 — without swap: 7 mod 22=7 |
| A22 | `swap lshift` | `3 2 swap lshift . cr` | `16` | ADVERSARIAL: 2<<3=16 — without swap: 3<<2=12 |
| A23 | `swap rshift` | `3 24 swap rshift . cr` | `3` | ADVERSARIAL: 24>>3=3 — without swap: 3>>24=0 |
| A24 | `swap <` | `3 7 swap < . cr` | `0` | ADVERSARIAL: 7<3=false(0) — without swap: 3<7=true(-1) |
| A25 | `swap >` | `3 7 swap > . cr` | `-1` | ADVERSARIAL: 7>3=true(-1) — without swap: 3>7=false(0) |
| A26 | `swap <=` | `3 7 swap <= . cr` | `0` | ADVERSARIAL: 7<=3=false(0) — without swap: 3<=7=true(-1) |
| A27 | `swap >=` | `3 7 swap >= . cr` | `-1` | ADVERSARIAL: 7>=3=true(-1) — without swap: 3>=7=false(0) |
| A28 | `swap u<` | `3 7 swap u< . cr` | `0` | ADVERSARIAL: 7 u< 3=false(0) — without swap: 3 u< 7=true(-1) |
| A29 | `swap u>` | `3 7 swap u> . cr` | `-1` | ADVERSARIAL: 7 u> 3=true(-1) — without swap: 3 u> 7=false(0) |
| A30 | `swap swap` | `3 7 swap swap . . cr` | `7 3` | double swap = identity — must produce original order |
| A31 | `swap drop` | `3 7 swap drop . cr` | `7` | swap drop = nip — drops original TOS, keeps NOS |
| A32 | `swap over` | `3 7 swap over . . . cr` | `7 3 7` | swap over = tuck — ( 3 7 -- 7 3 7 ) |

## Group B: dup + comparison + branch (6 patterns)

**Mechanism:** dup-branch fusion

| # | Pattern | Test code | Expected | Adversarial property |
|---|---------|-----------|----------|---------------------|
| B01 | `dup 0> while` | `5 begin dup 0> while 1- repeat . cr` | `0` | countdown from 5 while positive — 5 iterations, exits at 0 |
| B02 | `dup 0< while` | `-3 begin dup 0< while 1+ repeat . cr` | `0` | count up from -3 while negative — 3 iterations, exits at 0 |
| B03 | `dup 0= while` | `0 begin dup 0= while 1+ repeat . cr` | `1` | only one iteration — 0 is 0=true, 1+ makes 1, 1 is 0=false |
| B04 | `dup 0> until` | `-3 begin 1+ dup 0> until . cr` | `1` | count up from -3 until positive — 4 iterations, exits at 1 |
| B05 | `dup 0< until` | `3 begin 1- dup 0< until . cr` | `-1` | count down from 3 until negative — 4 iterations, exits at -1 |
| B06 | `dup 0= until` | `5 begin 1- dup 0= until . cr` | `0` | count down from 5 until zero — 5 iterations, exits at 0 |

## Group C: arithmetic + dup + comparison + branch (12 patterns)

**Mechanism:** flag-reuse fusion

| # | Pattern | Test code | Expected | Adversarial property |
|---|---------|-----------|----------|---------------------|
| C01 | `1- dup 0> while` | `5 begin 1- dup 0> while repeat . cr` | `0` | 5→4→3→2→1→0 (0>false, exit) — the standard countdown |
| C02 | `1- dup 0= while` | `1 begin 1- dup 0= while repeat . cr` | `-1` | 1→0 (0=true, continue)→-1 (0=false, exit) |
| C03 | `1- dup 0< while` | `0 begin 1- dup 0< while 3 + repeat . cr` | `1` | 0→-1 (0<true, +3→2)→1 (0<false, exit) |
| C04 | `1- dup 0> until` | `5 begin 1- dup 0> until . cr` | `4` | 5→4 (4>0=true, exit immediately) — until exits on TRUE |
| C05 | `1- dup 0= until` | `5 begin 1- dup 0= until . cr` | `0` | 5→4→3→2→1→0 (0=true, exit) — 5 iterations |
| C06 | `1- dup 0< until` | `3 begin 1- dup 0< until . cr` | `-1` | 3→2→1→0→-1 (0<true, exit) — 4 iterations |
| C07 | `1+ dup 0> while` | `-1 begin 1+ dup 0> while repeat . cr` | `0` | -1→0 (0>false, exit immediately) |
| C08 | `1+ dup 0= while` | `-1 begin 1+ dup 0= while repeat . cr` | `1` | -1→0 (0=true, continue)→1 (0=false, exit) |
| C09 | `1+ dup 0< while` | `-3 begin 1+ dup 0< while repeat . cr` | `0` | -3→-2→-1→0 (0<false, exit) — 3 iterations |
| C10 | `1+ dup 0> until` | `-3 begin 1+ dup 0> until . cr` | `1` | -3→-2→-1→0→1 (0>true, exit) — 4 iterations |
| C11 | `1+ dup 0= until` | `-3 begin 1+ dup 0= until . cr` | `0` | -3→-2→-1→0 (0=true, exit) — 3 iterations |
| C12 | `1+ dup 0< until` | `-2 begin 1+ dup 0< until . cr` | `-1` | -2→-1 (0<true, exit) — 1 iteration |

## Group D: over + binary op (20 patterns)

**Mechanism:** pending-over

| # | Pattern | Test code | Expected | Adversarial property |
|---|---------|-----------|----------|---------------------|
| D01 | `over +` | `3 7 over + . . cr` | `10 3` | over copies NOS(3), 7+3=10 |
| D02 | `over -` | `3 7 over - . . cr` | `4 3` | over copies NOS(3), 7-3=4 |
| D03 | `over *` | `3 7 over * . . cr` | `21 3` | over copies NOS(3), 7*3=21 |
| D04 | `over /` | `3 21 over / . . cr` | `7 3` | over copies NOS(3), 21/3=7 |
| D05 | `over mod` | `3 22 over mod . . cr` | `1 3` | over copies NOS(3), 22 mod 3=1 |
| D06 | `over and` | `5 7 over and . . cr` | `5 5` | over copies NOS(5), 7 AND 5=5 |
| D07 | `over or` | `5 3 over or . . cr` | `7 5` | over copies NOS(5), 3 OR 5=7 |
| D08 | `over xor` | `5 3 over xor . . cr` | `6 5` | over copies NOS(5), 3 XOR 5=6 |
| D09 | `over lshift` | `3 1 over lshift . . cr` | `8 3` | over copies NOS(3), 1<<3=8 |
| D10 | `over rshift` | `2 24 over rshift . . cr` | `6 2` | over copies NOS(2), 24>>2=6 |
| D11 | `over =` | `3 7 over = . . cr` | `0 3` | over copies NOS(3), 7=3 false(0) |
| D12 | `over <>` | `3 7 over <> . . cr` | `-1 3` | over copies NOS(3), 7<>3 true(-1) |
| D13 | `over <` | `3 7 over < . . cr` | `0 3` | over copies NOS(3), 7<3 false(0) |
| D14 | `over >` | `3 7 over > . . cr` | `-1 3` | over copies NOS(3), 7>3 true(-1) |
| D15 | `over <=` | `3 7 over <= . . cr` | `0 3` | over copies NOS(3), 7<=3 false(0) |
| D16 | `over >=` | `3 7 over >= . . cr` | `-1 3` | over copies NOS(3), 7>=3 true(-1) |
| D17 | `over u<` | `3 7 over u< . . cr` | `0 3` | over copies NOS(3), 7 u< 3 false(0) |
| D18 | `over u>` | `3 7 over u> . . cr` | `-1 3` | over copies NOS(3), 7 u> 3 true(-1) |
| D19 | `over min` | `3 7 over min . . cr` | `3 3` | over copies NOS(3), min(7,3)=3 |
| D20 | `over max` | `3 7 over max . . cr` | `7 3` | over copies NOS(3), max(7,3)=7 |

## Group E: dup + binary op (5 patterns)

**Mechanism:** self-application fusion

| # | Pattern | Test code | Expected | Adversarial property |
|---|---------|-----------|----------|---------------------|
| E01 | `dup +` | `7 dup + . cr` | `14` | dup + = double = 2* — 7+7=14 |
| E02 | `dup *` | `7 dup * . cr` | `49` | dup * = square — 7*7=49 |
| E03 | `dup xor` | `7 dup xor . cr` | `0` | dup xor = zero — any XOR itself = 0 |
| E04 | `dup and` | `7 dup and . cr` | `7` | dup and = identity — any AND itself = itself |
| E05 | `dup or` | `7 dup or . cr` | `7` | dup or = identity — any OR itself = itself |

## Group F: annihilation (5 patterns)

**Mechanism:** cancel detection

| # | Pattern | Test code | Expected | Adversarial property |
|---|---------|-----------|----------|---------------------|
| F01 | `dup drop` | `42 dup drop . cr` | `42` | dup drop = identity — value must survive unchanged |
| F02 | `>r r>` | `42 >r r> . cr` | `42` | >r r> = identity — round-trip through return stack |
| F03 | `2>r 2r>` | `3 7 2>r 2r> . . cr` | `7 3` | 2>r 2r> = identity — round-trip double-cell through return stack |
| F04 | `rot -rot` | `3 5 7 rot -rot . . . cr` | `7 5 3` | rot -rot = identity — triple rotation and back |
| F05 | `-rot rot` | `3 5 7 -rot rot . . . cr` | `7 5 3` | -rot rot = identity — reverse triple rotation and back |

---

## Summary

| Group | Mechanism | Count | Priority |
|-------|-----------|-------|----------|
| A: swap + operation | pending-swap | 32 | **Highest** — covers all swap-arith idioms |
| B: dup + cmp + branch | dup-branch fusion | 6 | **High** — every standard loop exit |
| C: arith + dup + cmp + branch | flag-reuse fusion | 12 | **High** — eliminates redundant test |
| D: over + binary | pending-over | 20 | Medium — peek-and-operate idiom |
| E: dup + binary | self-application | 5 | Low — use 2* instead of dup + |
| F: annihilation | cancel detection | 5 | Low — indicates poor factoring |
| **Total** | | **80** | |

## Frequency Estimates

| Group | Pattern class | Estimated frequency |
|-------|--------------|--------------------|
| A | swap 1+ / swap 1- (NOS unary) | 20% |
| A | swap + / swap * (commutative binary) | 12% |
| A | swap - / swap < / swap > (non-commutative) | 10% |
| A | swap drop / swap swap / swap over | 6% |
| B | dup 0> while / dup 0= until | 13% |
| C | 1- dup 0> while / 1- dup 0= until | 12% |
| C | 1+ variants | 3% |
| D | over + / over - | 8% |
| D | over = / over < / over > | 5% |
| D | other over + binary | 3% |
| E | dup + / dup * | 3% |
| E | dup xor / dup and / dup or | 1% |
| F | annihilation pairs | 4% |
| | **Total** | **100%** |

## Custom Words That Already Handle These Patterns

| Custom word | Replaces pattern | Group |
|-------------|-----------------|-------|
| `nos+` | `swap 1+ swap` | A |
| `tuck+` | `tuck +` (related to over+) | D |
| `1-nzloop` | `1- dup 0<> while repeat` | C |
| `nzloop` | `dup 0<> while repeat` | B |
| `0=until` | `dup 0= until` | B |
| `<if` / `>if` / `=if` | `< if` / `> if` / `= if` | (branch) |

These do **not** reduce the optimization count. The compiler must handle
both the custom word and the standard spelling.

---

## Test Results (baseline, no fusion optimizations)

### Per-Test Results

| ID | Pattern | Result | Compile (ms) | Run (ms) |
|----|---------|--------|-------------|---------|
| A01 | `swap 1+` | PASS | 8 | 1 |
| A02 | `swap 1-` | PASS | 7 | 1 |
| A03 | `swap 2+` | PASS | 7 | 1 |
| A04 | `swap 2-` | PASS | 9 | 1 |
| A05 | `swap 2*` | PASS | 8 | 1 |
| A06 | `swap 2/` | PASS | 7 | 1 |
| A07 | `swap negate` | PASS | 8 | 1 |
| A08 | `swap abs` | PASS | 7 | 1 |
| A09 | `swap invert` | PASS | 7 | 1 |
| A10 | `swap +` | PASS | 7 | 1 |
| A11 | `swap *` | PASS | 7 | 1 |
| A12 | `swap and` | PASS | 7 | 2 |
| A13 | `swap or` | PASS | 8 | 1 |
| A14 | `swap xor` | PASS | 7 | 1 |
| A15 | `swap =` | PASS | 8 | 1 |
| A16 | `swap <>` | PASS | 7 | 1 |
| A17 | `swap min` | PASS | 8 | 1 |
| A18 | `swap max` | PASS | 7 | 1 |
| A19 | `swap -` | PASS | 7 | 1 |
| A20 | `swap /` | PASS | 7 | 1 |
| A21 | `swap mod` | PASS | 7 | 1 |
| A22 | `swap lshift` | PASS | 7 | 1 |
| A23 | `swap rshift` | PASS | 8 | 1 |
| A24 | `swap <` | PASS | 7 | 1 |
| A25 | `swap >` | PASS | 8 | 1 |
| A26 | `swap <=` | PASS | 7 | 1 |
| A27 | `swap >=` | PASS | 7 | 1 |
| A28 | `swap u<` | **SKIP** | - | - |
| A29 | `swap u>` | **SKIP** | - | - |
| A30 | `swap swap` | PASS | 7 | 1 |
| A31 | `swap drop` | PASS | 7 | 1 |
| A32 | `swap over` | PASS | 7 | 1 |
| B01 | `dup 0> while` | PASS | 7 | 1 |
| B02 | `dup 0< while` | PASS | 8 | 1 |
| B03 | `dup 0= while` | PASS | 7 | 1 |
| B04 | `dup 0> until` | PASS | 8 | 1 |
| B05 | `dup 0< until` | PASS | 7 | 1 |
| B06 | `dup 0= until` | PASS | 7 | 1 |
| C01 | `1- dup 0> while` | PASS | 7 | 1 |
| C02 | `1- dup 0= while` | PASS | 7 | 1 |
| C03 | `1- dup 0< while` | PASS | 7 | 1 |
| C04 | `1- dup 0> until` | PASS | 8 | 1 |
| C05 | `1- dup 0= until` | PASS | 7 | 1 |
| C06 | `1- dup 0< until` | PASS | 7 | 1 |
| C07 | `1+ dup 0> while` | PASS | 7 | 1 |
| C08 | `1+ dup 0= while` | PASS | 8 | 1 |
| C09 | `1+ dup 0< while` | PASS | 7 | 1 |
| C10 | `1+ dup 0> until` | PASS | 8 | 1 |
| C11 | `1+ dup 0= until` | PASS | 7 | 1 |
| C12 | `1+ dup 0< until` | PASS | 7 | 1 |
| D01 | `over +` | PASS | 8 | 1 |
| D02 | `over -` | PASS | 7 | 1 |
| D03 | `over *` | PASS | 7 | 1 |
| D04 | `over /` | PASS | 8 | 1 |
| D05 | `over mod` | PASS | 7 | 1 |
| D06 | `over and` | PASS | 7 | 1 |
| D07 | `over or` | PASS | 7 | 1 |
| D08 | `over xor` | PASS | 7 | 1 |
| D09 | `over lshift` | PASS | 7 | 1 |
| D10 | `over rshift` | PASS | 8 | 2 |
| D11 | `over =` | PASS | 9 | 1 |
| D12 | `over <>` | PASS | 8 | 1 |
| D13 | `over <` | PASS | 7 | 1 |
| D14 | `over >` | PASS | 9 | 1 |
| D15 | `over <=` | PASS | 8 | 1 |
| D16 | `over >=` | PASS | 8 | 1 |
| D17 | `over u<` | **SKIP** | - | - |
| D18 | `over u>` | **SKIP** | - | - |
| D19 | `over min` | PASS | 9 | 1 |
| D20 | `over max` | PASS | 7 | 1 |
| E01 | `dup +` | PASS | 7 | 1 |
| E02 | `dup *` | PASS | 7 | 1 |
| E03 | `dup xor` | PASS | 7 | 1 |
| E04 | `dup and` | PASS | 8 | 1 |
| E05 | `dup or` | PASS | 8 | 1 |
| F01 | `dup drop` | PASS | 8 | 1 |
| F02 | `>r r>` | PASS | 8 | 1 |
| F03 | `2>r 2r>` | PASS | 7 | 1 |
| F04 | `rot -rot` | **SKIP** | - | - |
| F05 | `-rot rot` | **SKIP** | - | - |

### Totals

```
74 pass, 0 fail, 6 skip, 0 compile-fail — 80 total
Compile: 549ms total (7ms avg)
Run:      76ms total (1ms avg)
```

### Skipped (unimplemented words)

| Test | Missing word | Status |
|------|-------------|--------|
| A28, A29 | `u<`, `u>` | Not in compiler |
| D17, D18 | `u<`, `u>` | Not in compiler |
| F04, F05 | `-rot` | Not in compiler |

### Test Errors Found During Creation

| Test | Error | Fix |
|------|-------|-----|
| A06 | Wrong operand order in test (`7 4` should be `4 7`) | Fixed — my mistake, not compiler bug |

All 74 testable patterns produce correct output with the current compiler.
These tests serve as the **regression baseline** for fusion optimizations:
any optimization that changes codegen must still pass all 74.
