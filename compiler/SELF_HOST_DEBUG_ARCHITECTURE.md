# Self-Hosting Debug Architecture

## Problem Statement

The sixth compiler crashes during self-hosting (sixth.fs compiling sixth.fs). One day of ad-hoc debugging has failed to find the root cause. The crash manifests as:

```
Invalid xt=<garbage> at ip=480 (dict_count=709)
CRASH sig=11
```

**Current state after today's work:**
- Tests: 1660/1660 PASS
- CODE-SIZE: 256KB → 512KB (needed for self-hosting code volume)
- RT-DICT-MAX: 256 → 512 (needed for self-hosting word count)
- dict-add: bounds check added
- Self-hosting: still crashes

## Why Ad-Hoc Debugging Failed

1. **Agents lack persistent state** - each subagent starts fresh, re-discovers context
2. **No shared scratchpad** - findings aren't accumulated between agents
3. **No narrowing protocol** - binary search gets repeated without converging
4. **Agents violate constraints** - they "fix" things that break 177 tests
5. **No verification gate** - changes land without test validation
6. **Wrong mental model** - agents guess (code overflow, dict overflow) instead of measuring

## Architecture: Convergent Debugging Pipeline

### Principle: Measure, Don't Guess

Every hypothesis must produce a MEASUREMENT before any code change. The pipeline enforces this.

### Phase 0: Ground Truth (Sequential, One Agent)

**Purpose:** Establish immutable facts before any investigation.

**Agent:** `general-purpose` with explicit constraints

**Output file:** `/tmp/sixth-debug/ground-truth.md`

**Tasks:**
1. Record exact commit hash
2. Run `./compiler/tests/test` → record pass count
3. Run self-hosting → record exact error output
4. Run `wc -l compiler/sixth.fs` → record line count
5. Count definitions: `:`, `variable`, `create`, `constant`, `2constant`
6. Record all buffer sizes: CODE-SIZE, DICT-SIZE, INPUT-SIZE, INFO-MAX, RT-DICT-MAX, INIT-MAX
7. Record DATA-BASE layout with byte offsets

**Gate:** This file exists and is complete before ANY other phase starts.

### Phase 1: Binary Search with Invariants (Parallel, 10 Agents)

**Purpose:** Find the EXACT line that triggers the crash, with proof.

**Key insight that was missing:** Previous binary searches tested `head -N` which changes what words are DEFINED but also what words are CALLED. The crash may not be at the line that's added but at the line that USES a corrupted structure.

**Protocol per agent:**

Each agent gets a RANGE (e.g., lines 2900-2950) and tests:
```bash
head -$LINE compiler/sixth.fs > /tmp/test.fs
echo ": main ;" >> /tmp/test.fs
timeout 10 ./engine/fifth compiler/sixth.fs /tmp/test.fs /tmp/out 2>&1
```

**Critical rule:** Agents MUST NOT modify compiler/sixth.fs. They create temp files only.

**Agent assignments (10 parallel):**
| Agent | Range | Step |
|-------|-------|------|
| 1 | 100-500 | 50 |
| 2 | 500-1000 | 50 |
| 3 | 1000-1500 | 50 |
| 4 | 1500-2000 | 50 |
| 5 | 2000-2200 | 20 |
| 6 | 2200-2400 | 20 |
| 7 | 2400-2600 | 20 |
| 8 | 2600-2800 | 20 |
| 9 | 2800-3000 | 20 |
| 10 | 3000-3490 | 50 |

**Output:** Each agent writes to `/tmp/sixth-debug/range-N.txt`:
```
LINE OK/CRASH [error message first 80 chars]
```

**Gate:** All 10 complete. Orchestrator identifies the TRANSITION line (last OK → first CRASH).

### Phase 2: Precise Bisection (Sequential, 1 Agent)

**Purpose:** Narrow from Phase 1's range to the EXACT line.

**Input:** Transition range from Phase 1 (e.g., lines 2975-2985)

**Method:** Test every single line in the range.

**Output:** `/tmp/sixth-debug/exact-line.txt` with the single line number.

**Gate:** Exact line identified and verified twice.

### Phase 3: Root Cause Analysis (Parallel, 5 Specialized Agents)

**Purpose:** Five independent analyses of WHY that line crashes.

Each agent reads ground-truth.md and exact-line.txt, then investigates ONE hypothesis:

#### Agent A: Memory Layout Analysis
- Calculate exact byte offset of every `create` buffer
- Check if any buffer at the crash point overlaps another
- Map DATA-BASE layout end-to-end
- Tool: arithmetic only, no code changes

#### Agent B: C Interpreter State
- Add TEMPORARY debug to engine/prims.c (will be reverted)
- Print dict_count, stack depth, and memory stats every 10000 ops
- Run self-hosting with debug, capture output
- Analyze: what's the state RIGHT BEFORE the crash?
- Tool: `valgrind --track-origins=yes` on self-hosting

#### Agent C: Forth-Level Trace
- Insert `.s` and state dumps in a COPY of sixth.fs
- Trace stack depth and key variables through the crash region
- Answer: what's on the stack when we crash?
- Tool: temp copy only, never modify original

#### Agent D: Control Flow Audit
- Read the exact crash line and 50 lines around it
- Map every `if/else/then`, `begin/while/repeat`, `do/loop`
- Check for mismatched control flow
- Check for return stack imbalance (`>r` without `r>`)
- Tool: reading only

#### Agent E: Differential Test
- Create a MINIMAL sixth.fs (just the word that crashes + its dependencies)
- Strip everything else
- Does the minimal version crash?
- If not, add words back one at a time until it does
- The LAST word added is the corruption source

**Output:** Each writes `/tmp/sixth-debug/hypothesis-{A-E}.md`

**Gate:** All 5 complete. Orchestrator reads all 5 and identifies convergent findings.

### Phase 4: Fix Proposal (Sequential, 1 Agent with Expert Review)

**Purpose:** Propose a fix based on converged findings.

**Input:** All hypothesis files + ground truth

**Protocol:**
1. Write the proposed fix as a diff
2. Apply it to a temp copy
3. Run ALL 1660 tests on the temp copy
4. Run self-hosting on the temp copy
5. Only if BOTH pass: propose the fix

**Expert review:** Invoke `/chuck-moore` or `/linus-torvalds` to review the fix for:
- Does it address root cause or just mask symptoms?
- Does it add complexity?
- Is there a simpler fix?

### Phase 5: Verification (Parallel, 3 Agents)

**Purpose:** Verify the fix is correct and complete.

| Agent | Task |
|-------|------|
| 1 | Run full test suite 3 times, check for flaky results |
| 2 | Run self-hosting, then use the OUTPUT binary to compile a simple program |
| 3 | Run self-hosting under valgrind, check for any memory errors |

**Gate:** All 3 pass. Fix is committed.

## Available Minds and When to Use Them

### For Root Cause Analysis
| Mind | Invocation | Use When |
|------|-----------|----------|
| **Linus Torvalds** | `/linus-torvalds` | Code review, "this is wrong because..." |
| **Chuck Moore** | `/chuck-moore` | Forth-specific insight, stack discipline |
| **Richard Feynman** | `/feynman` | "What's ACTUALLY happening?" First principles |
| **Claude Shannon** | `/shannon` | Information theory angle - where is state being lost? |

### For Architecture Review
| Mind | Invocation | Use When |
|------|-----------|----------|
| **Barbara Liskov** | `/liskov` | Abstraction boundaries, invariant violations |
| **Margaret Hamilton** | `/hamilton` | Systems reliability, error propagation |
| **Leslie Lamport** | `/lamport` | Temporal reasoning, ordering bugs |

### For Fix Validation
| Mind | Invocation | Use When |
|------|-----------|----------|
| **Linus Reviews** | `/linus-review` | Final code review before commit |
| **Linus Calls BS** | `/linus-calls-bullshit` | Sanity check on agent findings |

## Orchestration Rules

1. **No agent modifies compiler/sixth.fs** except Phase 4 (on a temp copy)
2. **Every agent writes to /tmp/sixth-debug/** - findings accumulate
3. **Gates are enforced** - Phase N+1 doesn't start until Phase N's gate passes
4. **Convergence required** - if Phase 3 agents disagree, run Phase 3 again with different approaches
5. **Test suite is the oracle** - 1660/1660 PASS is the invariant, never broken
6. **Revert on failure** - if any change causes test failures, revert immediately, no exceptions

## How to Execute

```
Step 1: mkdir -p /tmp/sixth-debug
Step 2: Run Phase 0 (1 agent)
Step 3: Run Phase 1 (10 parallel agents)
Step 4: Orchestrator reads Phase 1 output, identifies range
Step 5: Run Phase 2 (1 agent with the range)
Step 6: Run Phase 3 (5 parallel agents with exact line)
Step 7: Orchestrator reads Phase 3, identifies convergent root cause
Step 8: Run Phase 4 (1 agent proposes fix, expert reviews)
Step 9: Run Phase 5 (3 parallel agents verify)
Step 10: Commit if all gates pass
```

Total agents: 20 across 5 phases. Phases 1, 3, 5 are parallel.
Sequential dependencies enforced by gates.
