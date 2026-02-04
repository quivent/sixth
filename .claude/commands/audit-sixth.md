# /audit-sixth - Compiler Bloat Analysis

Find the waste in compiler/sixth.fs. Be brutal.

---

## INSTRUCTIONS

Read `compiler/sixth.fs` completely. Then write `compiler/AUDIT.md` with:

### 1. SIZE ANALYSIS

| Section | Lines | % of Total |
|---------|-------|------------|
| Header/comments | ? | ? |
| Data structures | ? | ? |
| Byte emission | ? | ? |
| Stack operations | ? | ? |
| Code generation | ? | ? |
| Builtin dispatch | ? | ? |
| Main compilation | ? | ? |
| **Total** | ? | 100% |

Which sections are disproportionately large?

### 2. REDUNDANCY

List code that appears more than once:

| Pattern | Occurrences | Lines Wasted |
|---------|-------------|--------------|
| ? | ? | ? |

Include: duplicate words, repeated sequences, copy-paste code.

### 3. DEAD CODE

Words defined but never called:

| Word | Line | Can Delete? |
|------|------|-------------|
| ? | ? | ? |

Variables declared but never used:

| Variable | Line | Can Delete? |
|----------|------|-------------|
| ? | ? | ? |

### 4. LONG WORDS

Words longer than 10 lines (should be factored):

| Word | Lines | Why So Long? |
|------|-------|--------------|
| ? | ? | ? |

### 5. PATCHES

Code that looks like it was added later to fix something:

| Location | Evidence | What It Fixed |
|----------|----------|---------------|
| ? | ? | ? |

Signs: inconsistent style, workarounds, special cases that break patterns.

### 6. UNNECESSARY COMPLEXITY

Abstractions that cost more than they save:

| Abstraction | Cost | Benefit | Verdict |
|-------------|------|---------|---------|
| ? | ? | ? | Keep/Delete |

### 7. VERDICT

Answer these questions:

1. **Current size**: ??? lines
2. **Minimum achievable** (keeping all features): ??? lines
3. **What must stay** (performance critical): list specific words
4. **Biggest wins**: top 3 changes that save the most lines
5. **Risk assessment**: what changes are safe vs dangerous

---

## OUTPUT

Write to: `compiler/AUDIT.md`

Overwrite existing file with fresh analysis.

---

## THE STANDARD

Every line is a liability. If you cannot justify a line's existence, it should not exist.

The question is not "does this code work?" The question is "does this code earn its place?"
