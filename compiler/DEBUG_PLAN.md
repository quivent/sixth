# Self-Hosting Debug Plan

## Core Problem
The compiler grew to 541 definitions but buffer assumptions were designed for ~256.
The fix is reduction + proper symbolic layout, not just expanding buffers.

---

## Phase 1: Ground Truth
**Goal**: Know exactly what works and what doesn't

### Step 1.1: Find Working Baseline [SEQUENTIAL]
- Git bisect to find last commit where ALL tests pass
- Record: commit hash, test counts, date
- Status: NOT STARTED

### Step 1.2: Document Current State [PARALLEL with 1.1]
- Count definitions by type (colon, variable, constant, create)
- List all buffer size constants
- Status: NOT STARTED

### Step 1.3: Map Data Segment [PARALLEL with 1.1, 1.2]
- Create byte-by-byte map from DATA-BASE to data-here
- Identify every field, its size, its purpose
- Find any gaps or overlaps
- Status: NOT STARTED

**Phase 1 Output**: baseline.md with commit, counts, layout diagram

---

## Phase 2: Symbolic Layout
**Goal**: Single source of truth for all offsets

### Step 2.1: Identify All Magic Numbers [SEQUENTIAL]
- Grep for DATA-BASE + <number>
- List all hardcoded offsets
- Status: NOT STARTED

### Step 2.2: Create Computed Constants [DEPENDS ON 2.1]
- Each offset = previous + previous_size
- Chain: rt-dict-buf-end = rt-dict-buf + RT-DICT-MAX * RT-DICT-ENTRY-SIZE
- rt-state = rt-dict-buf-end
- etc.
- Status: NOT STARTED

### Step 2.3: Verify Symbolic Layout [DEPENDS ON 2.2]
- Change RT-DICT-MAX from 256 to 128, run tests
- Change back to 256, run tests
- Both should pass if layout is truly symbolic
- Status: NOT STARTED

**Phase 2 Output**: sixth.fs with no hardcoded offsets

---

## Phase 3: Reduce Definitions
**Goal**: Under 300 definitions

### Step 3.1: Audit Constants [PARALLEL - 3 agents]
- Agent A: String constants ($+, $-, etc.) - can these be a lookup table?
- Agent B: Numeric constants - which are used only once?
- Agent C: Which constants duplicate builtin values?
- Status: NOT STARTED

### Step 3.2: Audit Colon Definitions [PARALLEL - 2 agents]
- Agent A: Which words are never called? (dead code)
- Agent B: Which words are called once and could inline at source level?
- Status: NOT STARTED

### Step 3.3: Implement Reductions [DEPENDS ON 3.1, 3.2]
- Create string table for builtin names
- Remove dead code
- Inline trivial words
- Status: NOT STARTED

### Step 3.4: Verify Reduction [DEPENDS ON 3.3]
- Count definitions again
- Run full test suite
- Status: NOT STARTED

**Phase 3 Output**: Smaller sixth.fs, <300 definitions, all tests pass

---

## Phase 4: Incremental Self-Hosting
**Goal**: Self-hosting works

### Step 4.1: Chunk Compilation [SEQUENTIAL]
- Compile lines 1-1000, check for crash
- Compile lines 1-2000, check for crash
- Continue until crash found
- Status: NOT STARTED

### Step 4.2: Binary Search Crash [DEPENDS ON 4.1]
- Narrow crash to specific line range
- Identify the exact construct causing crash
- Status: NOT STARTED

### Step 4.3: Fix and Test [DEPENDS ON 4.2]
- Fix ONE issue
- Run test suite
- Run chunk compilation again
- Repeat until self-hosting works
- Status: NOT STARTED

**Phase 4 Output**: Self-hosting compiler

---

## Phase 5: Hardening
**Goal**: Prevent future regressions

### Step 5.1: Add Debug Assertions [PARALLEL - 2 agents]
- Agent A: Buffer overflow checks (dict-count < DICT-SIZE, etc.)
- Agent B: Stack depth sanity checks
- Status: NOT STARTED

### Step 5.2: Create Regression Tests [PARALLEL with 5.1]
- Test for each bug fixed
- Test for buffer boundary conditions
- Status: NOT STARTED

### Step 5.3: Document Constraints [DEPENDS ON 5.1, 5.2]
- Maximum definitions supported
- Buffer size relationships
- Known limitations
- Status: NOT STARTED

**Phase 5 Output**: Hardened compiler with regression tests

---

## Parallelization Summary

```
Phase 1:  [1.1]------>
          [1.2]------>  All three can run in parallel
          [1.3]------>

Phase 2:  [2.1]-->[2.2]-->[2.3]  Sequential chain

Phase 3:  [3.1a]--\
          [3.1b]--->[3.3]-->[3.4]
          [3.1c]--/
          [3.2a]--/
          [3.2b]-/

Phase 4:  [4.1]-->[4.2]-->[4.3]  Sequential chain (can't parallelize debugging)

Phase 5:  [5.1a]--\
          [5.1b]--->[5.3]
          [5.2]---/
```

---

## Current Status

| Phase | Status | Blockers |
|-------|--------|----------|
| 1. Ground Truth | NOT STARTED | None |
| 2. Symbolic Layout | NOT STARTED | Phase 1 |
| 3. Reduce Definitions | NOT STARTED | Phase 2 |
| 4. Self-Hosting | NOT STARTED | Phase 3 |
| 5. Hardening | NOT STARTED | Phase 4 |

---

## Next Action
Start Phase 1 with 3 parallel agents:
- Agent 1: Git bisect for working baseline
- Agent 2: Count definitions by type
- Agent 3: Map data segment layout
