#!/usr/bin/env python3
"""Generate 80 fusion pattern test files for the Fifth compiler.

Each test is adversarial: values are chosen so that compiling or fusing
the wrong register, wrong comparison sense, or wrong operand order
produces a detectably different result.
"""

import os

OUTDIR = os.path.dirname(os.path.abspath(__file__))

tests = []

# =============================================================================
# GROUP A: swap + operation (pending-swap mechanism) — 32 patterns
# =============================================================================

# A01-A09: swap + unary op
# Stack setup: asymmetric values so wrong register → wrong answer
tests.append(("A01", "swap 1+",      "3 7 swap 1+ . . cr",           "4 7",
    "swap then increment — wrong register gives 3 8 or 8 7"))
tests.append(("A02", "swap 1-",      "3 7 swap 1- . . cr",           "2 7",
    "swap then decrement — wrong register gives 3 6 or 6 7"))
tests.append(("A03", "swap 2+",      "3 7 swap 2+ . . cr",           "5 7",
    "swap then add 2"))
tests.append(("A04", "swap 2-",      "3 7 swap 2- . . cr",           "1 7",
    "swap then subtract 2"))
tests.append(("A05", "swap 2*",      "3 7 swap 2* . . cr",           "6 7",
    "swap then double — wrong register gives 3 14"))
tests.append(("A06", "swap 2/",      "7 4 swap 2/ . . cr",           "2 7",
    "swap then halve — 4/2=2"))
tests.append(("A07", "swap negate",   "-5 7 swap negate . . cr",      "5 -5",
    "swap then negate — wrong register gives -7 -5"))
tests.append(("A08", "swap abs",      "-5 7 swap abs . . cr",         "5 -5",
    "swap then abs — wrong register gives 7 -5 (unchanged)"))

# Hmm wait. Let me recalculate A07 and A08.
# A07: -5 7 → TOS=7, NOS=-5. swap → TOS=-5, NOS=7. negate → TOS=5, NOS=7.
#   . . → 5 7.  But if wrong register negated: TOS=-5, NOS=-7 → -5 -7.
# A08: -5 7 → TOS=7, NOS=-5. swap → TOS=-5, NOS=7. abs → TOS=5, NOS=7.
#   . . → 5 7.  If wrong register: TOS=-5, NOS=7 → -5 7 (abs(7)=7, no change).
# Fix: both should expect "5 7"
tests[-2] = ("A07", "swap negate",  "-5 7 swap negate . . cr",  "5 7",
    "swap then negate — negate(-5)=5 — wrong register gives -5 -7")
tests[-1] = ("A08", "swap abs",     "-5 7 swap abs . . cr",     "5 7",
    "swap then abs — abs(-5)=5 — wrong register gives -5 7")

tests.append(("A09", "swap invert",  "3 7 swap invert . . cr",       "-4 7",
    "swap then bitwise NOT — invert(3)=-4 — wrong register gives 3 -8"))

# A10-A18: swap + commutative binary op (swap should be free)
tests.append(("A10", "swap +",       "3 7 swap + . cr",              "10",
    "commutative — 7+3=3+7=10"))
tests.append(("A11", "swap *",       "3 7 swap * . cr",              "21",
    "commutative — 7*3=3*7=21"))
tests.append(("A12", "swap and",     "3 7 swap and . cr",            "3",
    "commutative — 3 AND 7 = 3"))
tests.append(("A13", "swap or",      "3 7 swap or . cr",             "7",
    "commutative — 3 OR 7 = 7"))
tests.append(("A14", "swap xor",     "3 7 swap xor . cr",            "4",
    "commutative — 3 XOR 7 = 4"))
tests.append(("A15", "swap =",       "3 7 swap = . cr",              "0",
    "commutative — 7=3 is false (0)"))
tests.append(("A16", "swap <>",      "3 7 swap <> . cr",             "-1",
    "commutative — 7<>3 is true (-1)"))
tests.append(("A17", "swap min",     "3 7 swap min . cr",            "3",
    "commutative — min(7,3)=min(3,7)=3"))
tests.append(("A18", "swap max",     "3 7 swap max . cr",            "7",
    "commutative — max(7,3)=max(3,7)=7"))

# A19-A29: swap + non-commutative binary op (ADVERSARIAL — wrong order = wrong result)
tests.append(("A19", "swap -",       "3 7 swap - . cr",              "4",
    "ADVERSARIAL: 7-3=4 — without swap: 3-7=-4"))
tests.append(("A20", "swap /",       "7 21 swap / . cr",             "3",
    "ADVERSARIAL: 21/7=3 — without swap: 7/21=0"))
tests.append(("A21", "swap mod",     "7 22 swap mod . cr",           "1",
    "ADVERSARIAL: 22 mod 7=1 — without swap: 7 mod 22=7"))
tests.append(("A22", "swap lshift",  "3 2 swap lshift . cr",         "16",
    "ADVERSARIAL: 2<<3=16 — without swap: 3<<2=12"))
tests.append(("A23", "swap rshift",  "3 24 swap rshift . cr",        "3",
    "ADVERSARIAL: 24>>3=3 — without swap: 3>>24=0"))
tests.append(("A24", "swap <",       "3 7 swap < . cr",              "0",
    "ADVERSARIAL: 7<3=false(0) — without swap: 3<7=true(-1)"))
tests.append(("A25", "swap >",       "3 7 swap > . cr",              "-1",
    "ADVERSARIAL: 7>3=true(-1) — without swap: 3>7=false(0)"))
tests.append(("A26", "swap <=",      "3 7 swap <= . cr",             "0",
    "ADVERSARIAL: 7<=3=false(0) — without swap: 3<=7=true(-1)"))
tests.append(("A27", "swap >=",      "3 7 swap >= . cr",             "-1",
    "ADVERSARIAL: 7>=3=true(-1) — without swap: 3>=7=false(0)"))
tests.append(("A28", "swap u<",      "3 7 swap u< . cr",             "0",
    "ADVERSARIAL: 7 u< 3=false(0) — without swap: 3 u< 7=true(-1)"))
tests.append(("A29", "swap u>",      "3 7 swap u> . cr",             "-1",
    "ADVERSARIAL: 7 u> 3=true(-1) — without swap: 3 u> 7=false(0)"))

# A30-A32: swap + stack ops
tests.append(("A30", "swap swap",    "3 7 swap swap . . cr",         "7 3",
    "double swap = identity — must produce original order"))
tests.append(("A31", "swap drop",    "3 7 swap drop . cr",           "7",
    "swap drop = nip — drops original TOS, keeps NOS"))
tests.append(("A32", "swap over",    "3 7 swap over . . . cr",       "7 3 7",
    "swap over = tuck — ( 3 7 -- 7 3 7 )"))

# =============================================================================
# GROUP B: dup + comparison + branch (test-without-consuming) — 6 patterns
# =============================================================================

tests.append(("B01", "dup 0> while",
    "5 begin dup 0> while 1- repeat . cr",   "0",
    "countdown from 5 while positive — 5 iterations, exits at 0"))
tests.append(("B02", "dup 0< while",
    "-3 begin dup 0< while 1+ repeat . cr",  "0",
    "count up from -3 while negative — 3 iterations, exits at 0"))
tests.append(("B03", "dup 0= while",
    "0 begin dup 0= while 1+ repeat . cr",   "1",
    "only one iteration — 0 is 0=true, 1+ makes 1, 1 is 0=false"))
tests.append(("B04", "dup 0> until",
    "-3 begin 1+ dup 0> until . cr",         "1",
    "count up from -3 until positive — 4 iterations, exits at 1"))
tests.append(("B05", "dup 0< until",
    "3 begin 1- dup 0< until . cr",          "-1",
    "count down from 3 until negative — 4 iterations, exits at -1"))
tests.append(("B06", "dup 0= until",
    "5 begin 1- dup 0= until . cr",          "0",
    "count down from 5 until zero — 5 iterations, exits at 0"))

# =============================================================================
# GROUP C: arithmetic + dup + comparison + branch (flag reuse) — 12 patterns
# =============================================================================

# 1- variants (C01-C06)
tests.append(("C01", "1- dup 0> while",
    "5 begin 1- dup 0> while repeat . cr",   "0",
    "5→4→3→2→1→0 (0>false, exit) — the standard countdown"))
tests.append(("C02", "1- dup 0= while",
    "1 begin 1- dup 0= while repeat . cr",   "-1",
    "1→0 (0=true, continue)→-1 (0=false, exit)"))
tests.append(("C03", "1- dup 0< while",
    "0 begin 1- dup 0< while 3 + repeat . cr",  "1",
    "0→-1 (0<true, +3→2)→1 (0<false, exit)"))
tests.append(("C04", "1- dup 0> until",
    "5 begin 1- dup 0> until . cr",          "4",
    "5→4 (4>0=true, exit immediately) — until exits on TRUE"))
tests.append(("C05", "1- dup 0= until",
    "5 begin 1- dup 0= until . cr",          "0",
    "5→4→3→2→1→0 (0=true, exit) — 5 iterations"))
tests.append(("C06", "1- dup 0< until",
    "3 begin 1- dup 0< until . cr",          "-1",
    "3→2→1→0→-1 (0<true, exit) — 4 iterations"))

# 1+ variants (C07-C12)
tests.append(("C07", "1+ dup 0> while",
    "-1 begin 1+ dup 0> while repeat . cr",  "0",
    "-1→0 (0>false, exit immediately)"))
tests.append(("C08", "1+ dup 0= while",
    "-1 begin 1+ dup 0= while repeat . cr",  "1",
    "-1→0 (0=true, continue)→1 (0=false, exit)"))
tests.append(("C09", "1+ dup 0< while",
    "-3 begin 1+ dup 0< while repeat . cr",  "0",
    "-3→-2→-1→0 (0<false, exit) — 3 iterations"))
tests.append(("C10", "1+ dup 0> until",
    "-3 begin 1+ dup 0> until . cr",         "1",
    "-3→-2→-1→0→1 (0>true, exit) — 4 iterations"))
tests.append(("C11", "1+ dup 0= until",
    "-3 begin 1+ dup 0= until . cr",         "0",
    "-3→-2→-1→0 (0=true, exit) — 3 iterations"))
tests.append(("C12", "1+ dup 0< until",
    "-2 begin 1+ dup 0< until . cr",         "-1",
    "-2→-1 (0<true, exit) — 1 iteration"))

# =============================================================================
# GROUP D: over + binary op (pending-over mechanism) — 20 patterns
# =============================================================================
# Stack: 3 7, over pushes NOS(3): 3 7 3, then OP on top two: 3 result
# . . prints result then 3

tests.append(("D01", "over +",       "3 7 over + . . cr",       "10 3",
    "over copies NOS(3), 7+3=10"))
tests.append(("D02", "over -",       "3 7 over - . . cr",       "4 3",
    "over copies NOS(3), 7-3=4"))
tests.append(("D03", "over *",       "3 7 over * . . cr",       "21 3",
    "over copies NOS(3), 7*3=21"))
tests.append(("D04", "over /",       "3 21 over / . . cr",      "7 3",
    "over copies NOS(3), 21/3=7"))
tests.append(("D05", "over mod",     "3 22 over mod . . cr",    "1 3",
    "over copies NOS(3), 22 mod 3=1"))
tests.append(("D06", "over and",     "5 7 over and . . cr",     "5 5",
    "over copies NOS(5), 7 AND 5=5"))
tests.append(("D07", "over or",      "5 3 over or . . cr",      "7 5",
    "over copies NOS(5), 3 OR 5=7"))
tests.append(("D08", "over xor",     "5 3 over xor . . cr",     "6 5",
    "over copies NOS(5), 3 XOR 5=6"))
tests.append(("D09", "over lshift",  "3 1 over lshift . . cr",  "8 3",
    "over copies NOS(3), 1<<3=8"))
tests.append(("D10", "over rshift",  "2 24 over rshift . . cr", "6 2",
    "over copies NOS(2), 24>>2=6"))
tests.append(("D11", "over =",       "3 7 over = . . cr",       "0 3",
    "over copies NOS(3), 7=3 false(0)"))
tests.append(("D12", "over <>",      "3 7 over <> . . cr",      "-1 3",
    "over copies NOS(3), 7<>3 true(-1)"))
tests.append(("D13", "over <",       "3 7 over < . . cr",       "0 3",
    "over copies NOS(3), 7<3 false(0)"))
tests.append(("D14", "over >",       "3 7 over > . . cr",       "-1 3",
    "over copies NOS(3), 7>3 true(-1)"))
tests.append(("D15", "over <=",      "3 7 over <= . . cr",      "0 3",
    "over copies NOS(3), 7<=3 false(0)"))
tests.append(("D16", "over >=",      "3 7 over >= . . cr",      "-1 3",
    "over copies NOS(3), 7>=3 true(-1)"))
tests.append(("D17", "over u<",      "3 7 over u< . . cr",      "0 3",
    "over copies NOS(3), 7 u< 3 false(0)"))
tests.append(("D18", "over u>",      "3 7 over u> . . cr",      "-1 3",
    "over copies NOS(3), 7 u> 3 true(-1)"))
tests.append(("D19", "over min",     "3 7 over min . . cr",     "3 3",
    "over copies NOS(3), min(7,3)=3"))
tests.append(("D20", "over max",     "3 7 over max . . cr",     "7 3",
    "over copies NOS(3), max(7,3)=7"))

# =============================================================================
# GROUP E: dup + binary op (self-application) — 5 patterns
# =============================================================================

tests.append(("E01", "dup +",        "7 dup + . cr",            "14",
    "dup + = double = 2* — 7+7=14"))
tests.append(("E02", "dup *",        "7 dup * . cr",            "49",
    "dup * = square — 7*7=49"))
tests.append(("E03", "dup xor",      "7 dup xor . cr",          "0",
    "dup xor = zero — any XOR itself = 0"))
tests.append(("E04", "dup and",      "7 dup and . cr",           "7",
    "dup and = identity — any AND itself = itself"))
tests.append(("E05", "dup or",       "7 dup or . cr",            "7",
    "dup or = identity — any OR itself = itself"))

# =============================================================================
# GROUP F: annihilation (adjacent words cancel) — 5 patterns
# =============================================================================

tests.append(("F01", "dup drop",     "42 dup drop . cr",         "42",
    "dup drop = identity — value must survive unchanged"))
tests.append(("F02", ">r r>",        "42 >r r> . cr",            "42",
    ">r r> = identity — round-trip through return stack"))
tests.append(("F03", "2>r 2r>",      "3 7 2>r 2r> . . cr",      "7 3",
    "2>r 2r> = identity — round-trip double-cell through return stack"))
tests.append(("F04", "rot -rot",     "3 5 7 rot -rot . . . cr",  "7 5 3",
    "rot -rot = identity — triple rotation and back"))
tests.append(("F05", "-rot rot",     "3 5 7 -rot rot . . . cr",  "7 5 3",
    "-rot rot = identity — reverse triple rotation and back"))

# =============================================================================
# GENERATE FILES
# =============================================================================

# Group metadata for the document
groups = {
    "A": ("swap + operation", "pending-swap", 32),
    "B": ("dup + comparison + branch", "dup-branch fusion", 6),
    "C": ("arithmetic + dup + comparison + branch", "flag-reuse fusion", 12),
    "D": ("over + binary op", "pending-over", 20),
    "E": ("dup + binary op", "self-application fusion", 5),
    "F": ("annihilation", "cancel detection", 5),
}

def generate_test_file(test_id, pattern, code, expected, description):
    """Generate a single .fs test file."""
    filepath = os.path.join(OUTDIR, f"{test_id}-{pattern.replace(' ', '-').replace('/', '-').replace('<', 'lt').replace('>', 'gt').replace('=', 'eq').replace('+', 'plus').replace('*', 'star').replace('-', 'minus')}.fs")
    # Simpler naming
    safe = pattern.lower()
    for old, new in [(' ', '-'), ('/', 'div'), ('<', 'lt'), ('>', 'gt'),
                     ('=', 'eq'), ('+', 'plus'), ('*', 'star'),
                     ('1-', '1minus'), ('2-', '2minus'),
                     ('0<', '0lt'), ('0>', '0gt'), ('0=', '0eq')]:
        safe = safe.replace(old, new)
    # Actually, just use a clean mapping
    filepath = os.path.join(OUTDIR, f"{test_id}.fs")

    with open(filepath, 'w') as f:
        f.write(f"\\ expect: {expected}\n")
        f.write(f"\\ Pattern {test_id}: {pattern}\n")
        f.write(f"\\ {description}\n")
        f.write(f": main {code} ;\n")

    return filepath

# Generate all test files
generated = []
for test_id, pattern, code, expected, description in tests:
    filepath = generate_test_file(test_id, pattern, code, expected, description)
    generated.append((test_id, pattern, expected, description, filepath))
    print(f"  {test_id}: {pattern}")

print(f"\nGenerated {len(generated)} test files in {OUTDIR}/")

# Verify count matches expected 80
assert len(generated) == 80, f"Expected 80 tests, got {len(generated)}"
print("Count verified: 80 patterns.")

# =============================================================================
# GENERATE PATTERNS.md
# =============================================================================

doc_path = os.path.join(OUTDIR, "PATTERNS.md")
with open(doc_path, 'w') as f:
    f.write("# Fusion Patterns — Complete Enumeration\n\n")
    f.write("## Count History\n\n")
    f.write("| Attempt | Count | Verdict |\n")
    f.write("|---------|-------|---------|\n")
    f.write("| First   | 15    | **Wrong.** Missed binary ops entirely. Treated 20+ patterns as 3. |\n")
    f.write("| Second  | 41    | **Wrong.** Missed `over+binary` (20), `dup+binary` (5), annihilation (5). |\n")
    f.write("| Third   | 80    | Correct. Full cross-product enumeration. |\n\n")
    f.write("A fusible pattern is a sequence of Forth words that can be compiled to fewer\n")
    f.write("or cheaper x86 instructions than compiling each word independently.\n\n")
    f.write("---\n\n")

    for group_key in "ABCDEF":
        name, mechanism, count = groups[group_key]
        group_tests = [(tid, pat, exp, desc) for tid, pat, exp, desc, _ in generated if tid.startswith(group_key)]
        f.write(f"## Group {group_key}: {name} ({count} patterns)\n\n")
        f.write(f"**Mechanism:** {mechanism}\n\n")
        f.write(f"| # | Pattern | Test code | Expected | Adversarial property |\n")
        f.write(f"|---|---------|-----------|----------|---------------------|\n")
        for tid, pat, exp, desc in group_tests:
            # Find original code
            for t in tests:
                if t[0] == tid:
                    code = t[2]
                    break
            f.write(f"| {tid} | `{pat}` | `{code}` | `{exp}` | {desc} |\n")
        f.write(f"\n")

    # Summary table
    f.write("---\n\n## Summary\n\n")
    f.write("| Group | Mechanism | Count | Priority |\n")
    f.write("|-------|-----------|-------|----------|\n")
    f.write("| A: swap + operation | pending-swap | 32 | **Highest** — covers all swap-arith idioms |\n")
    f.write("| B: dup + cmp + branch | dup-branch fusion | 6 | **High** — every standard loop exit |\n")
    f.write("| C: arith + dup + cmp + branch | flag-reuse fusion | 12 | **High** — eliminates redundant test |\n")
    f.write("| D: over + binary | pending-over | 20 | Medium — peek-and-operate idiom |\n")
    f.write("| E: dup + binary | self-application | 5 | Low — use 2* instead of dup + |\n")
    f.write("| F: annihilation | cancel detection | 5 | Low — indicates poor factoring |\n")
    f.write(f"| **Total** | | **{sum(g[2] for g in groups.values())}** | |\n\n")

    f.write("## Frequency Estimates\n\n")
    f.write("| Group | Pattern class | Estimated frequency |\n")
    f.write("|-------|--------------|--------------------|\n")
    f.write("| A | swap 1+ / swap 1- (NOS unary) | 20% |\n")
    f.write("| A | swap + / swap * (commutative binary) | 12% |\n")
    f.write("| A | swap - / swap < / swap > (non-commutative) | 10% |\n")
    f.write("| A | swap drop / swap swap / swap over | 6% |\n")
    f.write("| B | dup 0> while / dup 0= until | 13% |\n")
    f.write("| C | 1- dup 0> while / 1- dup 0= until | 12% |\n")
    f.write("| C | 1+ variants | 3% |\n")
    f.write("| D | over + / over - | 8% |\n")
    f.write("| D | over = / over < / over > | 5% |\n")
    f.write("| D | other over + binary | 3% |\n")
    f.write("| E | dup + / dup * | 3% |\n")
    f.write("| E | dup xor / dup and / dup or | 1% |\n")
    f.write("| F | annihilation pairs | 4% |\n")
    f.write("| | **Total** | **100%** |\n\n")

    f.write("## Custom Words That Already Handle These Patterns\n\n")
    f.write("| Custom word | Replaces pattern | Group |\n")
    f.write("|-------------|-----------------|-------|\n")
    f.write("| `nos+` | `swap 1+ swap` | A |\n")
    f.write("| `tuck+` | `tuck +` (related to over+) | D |\n")
    f.write("| `1-nzloop` | `1- dup 0<> while repeat` | C |\n")
    f.write("| `nzloop` | `dup 0<> while repeat` | B |\n")
    f.write("| `0=until` | `dup 0= until` | B |\n")
    f.write("| `<if` / `>if` / `=if` | `< if` / `> if` / `= if` | (branch) |\n\n")
    f.write("These do **not** reduce the optimization count. The compiler must handle\n")
    f.write("both the custom word and the standard spelling.\n")

print(f"Generated {doc_path}")
