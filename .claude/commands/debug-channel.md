## THE ONLY GOAL THAT MATTERS

**SELF-HOSTING. NOTHING ELSE.**

```bash
./engine/fifth compiler/shannon/main.fs compiler/shannon/main.fs /tmp/s2
/tmp/s2 compiler/shannon/main.fs compiler/shannon/main.fs /tmp/s3
diff /tmp/s2 /tmp/s3  # MUST BE IDENTICAL
```

Until this works, the investigation is NOT closed. "Tests pass" means nothing. "Investigation closed" is FORBIDDEN until self-hosting succeeds.

If you read DEBUG-STATE.md and see "CLOSED" but self-hosting doesn't work: REOPEN IT IMMEDIATELY.

---

# /debug-channel - Persistent State Compiler Debugging

---

## STEP 1: LAUNCH PARALLEL AGENTS (THIS IS NOT OPTIONAL)

**BEFORE YOU DO ANYTHING ELSE - READ THIS:**

You MUST launch parallel agents as your FIRST action. Not after "understanding the problem." Not after "reading the code." NOW.

```
+===============================================================================+
| MANDATORY FIRST ACTION: PARALLEL LAUNCH                                       |
+===============================================================================+
|                                                                               |
| DO NOT:                                                                       |
|   - "Let me first understand the problem"                                     |
|   - "Let me read the relevant code"                                           |
|   - "Let me check the current state"                                          |
|   - Do ANYTHING sequentially                                                  |
|                                                                               |
| DO:                                                                           |
|   1. Read DEBUG-STATE.md (if exists)                                          |
|   2. Identify the top 3 hypotheses                                            |
|   3. IMMEDIATELY launch 3 parallel agents, one per hypothesis                 |
|   4. Each agent investigates independently and reports back                   |
|   5. YOU synthesize results and update state                                  |
|                                                                               |
| If you do not launch parallel agents as your first action,                    |
| you are doing it wrong and will fail like every agent before you.             |
+===============================================================================+
```

### The Parallel Launch Template (Use This NOW)

```
I am launching 3 parallel agents to test hypotheses independently:

AGENT 1 - H1: [hypothesis from DEBUG-STATE.md or formulate]
  Task: [specific investigation]
  Report: findings + probability update

AGENT 2 - H2: [second hypothesis]
  Task: [specific investigation]
  Report: findings + probability update

AGENT 3 - H3: [third hypothesis]
  Task: [specific investigation]
  Report: findings + probability update

I will synthesize their results when they return.
```

### Why This Is First

Every previous agent:
1. Read the state
2. Formed ONE hypothesis
3. Investigated sequentially
4. Said "found it!"
5. Was wrong
6. Repeated until context exhausted

The bug persists because sequential investigation is O(n) and context is finite.

Parallel investigation is O(1). You get 3x the information in the same context.

**IF YOU ARE NOT LAUNCHING PARALLEL AGENTS RIGHT NOW, STOP AND DO IT.**

---

## STEP 2: While Agents Run, Read Context

Only AFTER launching parallel agents, read:

1. `compiler/shannon/DEBUG-STATE.md` - Current state
2. HYPOTHESES section - What's been tried
3. CERTAIN FACTS - Do not re-verify
4. RULED OUT - Do not re-test

---

## THE PERSISTENCE PRINCIPLE

> Context compaction is lossy compression.
> A state file is lossless.
> The file IS the context.

Traditional debugging fails because:
- Context compaction discards "unimportant" details (which were important)
- Session continuity requires the LLM to remember (it can't)
- Fresh agents start from zero (wasting all prior work)

**The fix:** A single canonical state file that:
- Contains everything needed to resume
- Is updated after EVERY action
- Survives /clear
- Is the FIRST thing any agent reads

---

## COLD START PROTOCOL

```
+===============================================================================+
| COLD START DETECTED                                                           |
+===============================================================================+
| Checking for persistent state...                                              |
|                                                                               |
| [ ] compiler/shannon/DEBUG-STATE.md exists?                                   |
|     -> YES: Load state, resume from NEXT ACTION                               |
|     -> NO:  Initialize new state file from template below                     |
|                                                                               |
| [ ] Git history available?                                                    |
|     -> Scan for [debug] commits to reconstruct if state file missing          |
|                                                                               |
| Agent has full context in: [X] seconds                                        |
+===============================================================================+
```

```bash
# Check for existing state
cat compiler/shannon/DEBUG-STATE.md 2>/dev/null || echo "NO STATE FILE - INITIALIZE"
```

If state file exists: **Read it entirely, resume from NEXT ACTION**
If no state file: Create from STATE FILE TEMPLATE below

---

## PARALLELISM FIRST (Default posture)

> Sequential debugging is O(n). Parallel debugging is O(1).
> If experiments are independent, RUN THEM SIMULTANEOUSLY.

**PARALLEL IS THE DEFAULT. SEQUENTIAL IS THE EXCEPTION.**

Before ANY sequential experiment, you MUST justify why it's not parallel:

```
+===============================================================================+
| PARALLELISM GATE (Answer before EVERY experiment)                             |
+===============================================================================+
| Do H1, H2, H3 tests require the SAME information first?                       |
|                                                                               |
|   NO  -> MANDATORY: Launch parallel agents, one per hypothesis                |
|   YES -> State the shared dependency: "All need [X] result first"             |
+-------------------------------------------------------------------------------+
| VALID reasons for sequential (must cite one):                                 |
|   [ ] Experiment B literally needs output value from Experiment A             |
|   [ ] Code change required (can't parallelize writes to same file)            |
|   [ ] Budget < 3 bits remaining (consolidate to preserve budget)              |
|   [ ] Single hypothesis remaining (nothing to parallelize)                    |
+-------------------------------------------------------------------------------+
| If NONE of these apply: YOU MUST PARALLELIZE                                  |
+===============================================================================+
```

### Parallel Agent Launch Template

When launching parallel agents, use this structure:

```
PARALLEL LAUNCH: [N] agents testing [N] independent hypotheses

Agent 1 - Test H1: [hypothesis description]
  Task: [specific experiment]
  Predict: If H1 true -> [outcome]. If H1 false -> [outcome].
  Report: Result + probability update. DO NOT make changes.

Agent 2 - Test H2: [hypothesis description]
  Task: [specific experiment]
  Predict: If H2 true -> [outcome]. If H2 false -> [outcome].
  Report: Result + probability update. DO NOT make changes.

Agent 3 - Test H3: [hypothesis description]
  Task: [specific experiment]
  Predict: If H3 true -> [outcome]. If H3 false -> [outcome].
  Report: Result + probability update. DO NOT make changes.
```

### Parallel Synthesis (After results return)

```
+===============================================================================+
| PARALLEL SYNTHESIS                                                            |
+===============================================================================+
| Agent 1: [result summary] -> H1: [%] -> [%]                                   |
| Agent 2: [result summary] -> H2: [%] -> [%]                                   |
| Agent 3: [result summary] -> H3: [%] -> [%]                                   |
+-------------------------------------------------------------------------------+
| COMBINED UPDATE: [what we now know that we didn't before]                     |
| CONVERGENCE: [highest H] at [%] confidence                                    |
| NEXT: [single best action given ALL results]                                  |
+===============================================================================+
```

---

## NO VICTORY LAPS (The pattern that has failed repeatedly)

> "I found it!" has been said many times. The bug remains.
> Confidence without verification is noise.

**THE FAILURE PATTERN:**

1. Agent reads code, forms mental model
2. Agent spots something that "looks wrong"
3. Agent announces "I found the bug!"
4. Agent makes a fix
5. Bug persists
6. Agent is confused, starts over
7. Repeat until context exhausted

**THE REQUIRED PATTERN:**

1. Agent forms hypothesis with explicit probability
2. Agent designs experiment that DISCRIMINATES (not just confirms)
3. Agent PREDICTS both outcomes before running
4. Agent runs experiment
5. Agent updates probability based on ACTUAL result
6. Only when confidence > 80% AND disassembly confirms: attempt fix
7. After fix: RUN THE FAILING TEST. Did it pass? Binary proof only.

### Victory Lap Gate

Before saying "I found it" or "This is the fix", answer:

```
+===============================================================================+
| VICTORY LAP GATE - All must be YES to claim solution                          |
+===============================================================================+
| [ ] Confidence in this hypothesis > 80%?                                      |
| [ ] At least 3 experiments support this conclusion?                           |
| [ ] Disassembly of failing case examined?                                     |
| [ ] Can explain EXACTLY which bytes are wrong and why?                        |
| [ ] Alternative hypotheses ruled out by evidence (not assumption)?            |
+-------------------------------------------------------------------------------+
| If ANY is NO: You have not found the bug. You have a hypothesis.              |
| State it as: "H[N] is now at [X]% confidence" - NOT "I found it"              |
+===============================================================================+
```

### After Attempting Fix

```
+===============================================================================+
| FIX VERIFICATION - Required after every fix attempt                           |
+===============================================================================+
| 1. Run the EXACT failing command from SIGNAL:                                 |
|    [paste command here]                                                       |
|                                                                               |
| 2. Result:                                                                    |
|    [ ] PASS - Binary runs, produces expected output                           |
|    [ ] FAIL - Same error                                                      |
|    [ ] DIFFERENT FAIL - New error (describe)                                  |
|                                                                               |
| 3. If PASS, also verify:                                                      |
|    [ ] Working cases still work (no regression)                               |
|    [ ] Binary size is reasonable (not 246 bytes)                              |
|    [ ] Disassembly shows expected code                                        |
|                                                                               |
| 4. Only after ALL checks pass:                                                |
|    Commit with: [debug] [bug-sig]: FIXED - [root cause]                       |
+===============================================================================+
```

---

## BANNED PHRASES (Immediate red flag)

The following phrases indicate premature certainty. If you catch yourself typing them, STOP.

```
+===============================================================================+
| BANNED PHRASES - If you type these, you are probably wrong                    |
+===============================================================================+
| PHRASE                          | WHAT TO SAY INSTEAD                         |
|---------------------------------|---------------------------------------------|
| "ROOT CAUSE FOUND!"             | "H[N] is now at [X]% confidence"            |
| "I found it!"                   | "Evidence suggests [X], confidence [Y]%"    |
| "This is definitely..."         | "This appears to be... [evidence]"          |
| "The bug is..."                 | "Hypothesis: the bug may be..."             |
| "Fixed!"                        | "Fix attempted. Running verification..."    |
| "But wait..."                   | [STOP. You were wrong. Update hypotheses.]  |
| "Actually..."                   | [STOP. You were wrong. Update hypotheses.]  |
| "Oh, I see now..."              | [STOP. Previous understanding was wrong.]   |
| "That's not it, the real..."    | [STOP. Log the failed hypothesis. Move on.] |
+-------------------------------------------------------------------------------+
| "But wait" after "Found it" = You didn't find it. You guessed.                |
| This pattern has repeated 10+ times on this bug. Stop doing it.               |
+===============================================================================+
```

---

## MANDATORY SELF-REFLECTION (Every 3 experiments)

Before continuing, answer honestly:

```
+===============================================================================+
| SELF-REFLECTION CHECKPOINT                                                    |
+===============================================================================+
| 1. Have I said "found it" or similar in this session?                         |
|    [ ] NO  - Good, continue                                                   |
|    [ ] YES - Did the bug actually get fixed? If no, I was wrong.              |
|                                                                               |
| 2. Have I had to backtrack ("but wait", "actually", "oh I see")?              |
|    [ ] NO  - Good, continue                                                   |
|    [ ] YES - My mental model is wrong. Need /refresh, not more guessing.      |
|                                                                               |
| 3. Am I reading code hoping to "understand" without a specific question?      |
|    [ ] NO  - Good, continue                                                   |
|    [ ] YES - STOP. Formulate a YES/NO question first.                         |
|                                                                               |
| 4. Am I about to try something "just to see what happens"?                    |
|    [ ] NO  - Good, continue                                                   |
|    [ ] YES - STOP. Predict BOTH outcomes first. No fishing expeditions.       |
|                                                                               |
| 5. Has entropy decreased since last checkpoint?                               |
|    [ ] YES - Good, continue                                                   |
|    [ ] NO  - STOP. /refresh required. Current approach is not working.        |
|                                                                               |
| 6. Am I repeating something that was already tried?                           |
|    [ ] NO  - Good, continue                                                   |
|    [ ] YES - STOP. Check EXPERIMENT LOG. It failed before. Why retry?         |
+===============================================================================+
```

---

## META-ANALYSIS (Why debugging this bug keeps failing)

### The Pattern

```
Session N:
  1. Agent reads state, forms hypothesis
  2. Agent investigates, gains confidence
  3. Agent announces "ROOT CAUSE FOUND!" or equivalent
  4. Agent attempts fix
  5. Fix doesn't work
  6. Agent says "But wait..." or "Actually..."
  7. Agent forms NEW hypothesis
  8. Repeat steps 2-7 until context exhausted
  9. Context compacts, next agent starts fresh

Result: No progress. Bug persists. User frustrated.
```

### Why This Happens

1. **Confirmation bias**: Agent looks for evidence that supports hypothesis, ignores contradicting evidence

2. **Narrative over verification**: Agent builds a story ("the bug is X because Y") instead of testing mechanically

3. **Premature certainty**: 60% confidence feels like 95% confidence. Agent announces before verified.

4. **Sunk cost**: Agent invested in hypothesis, reluctant to abandon even when evidence contradicts

5. **Context pressure**: Agent feels pressure to "make progress" before context runs out, rushes to conclusions

6. **No ground truth comparison**: Agent reasons about code without comparing actual vs expected bytes

### What Breaks The Pattern

1. **Parallel testing**: Test multiple hypotheses simultaneously. Don't invest in one until discrimination complete.

2. **Binary comparison**: Stop reasoning. Disassemble working case. Disassemble failing case. Diff the bytes.

3. **Mechanical verification**: The test either passes or fails. No interpretation. No "it's closer."

4. **Probability tracking**: 60% is not "found it." Track actual confidence. Update on evidence.

5. **Backtrack logging**: Every "but wait" goes in the log. If you backtrack 3 times, your approach is wrong.

6. **Forced refresh**: Entropy stalls → mandatory /refresh. No continuing with broken mental model.

---

## BACKTRACK COUNTER

Track how many times you've had to backtrack in this session:

```
+===============================================================================+
| BACKTRACK LOG                                                                 |
+===============================================================================+
| # | Previous Claim              | Why Wrong                | New Direction    |
|---|-----------------------------|--------------------------| -----------------|
| 1 | [what you thought]          | [evidence that broke it] | [what now]       |
| 2 | [what you thought]          | [evidence that broke it] | [what now]       |
| 3 | [what you thought]          | [evidence that broke it] | [what now]       |
+-------------------------------------------------------------------------------+
| BACKTRACK COUNT: [N]                                                          |
|                                                                               |
| If N >= 3: STOP EVERYTHING.                                                   |
|   Your mental model of this system is fundamentally wrong.                    |
|   Do not form another hypothesis.                                             |
|   Run /refresh with full architectural audit.                                 |
|   Consider: what assumption are ALL your hypotheses sharing?                  |
|   That shared assumption is probably the actual bug.                          |
+===============================================================================+
```

---

## CONSTANT REFRESH TRIGGERS

Refresh is not optional. These conditions FORCE a /refresh:

| Condition | Action |
|-----------|--------|
| Said "found it" but bug persists | /refresh immediately |
| Said "but wait" or "actually" | /refresh immediately |
| Backtrack count >= 2 | /refresh immediately |
| 3 experiments with no entropy decrease | /refresh immediately |
| Changed hypothesis without new evidence | /refresh immediately |
| About to re-test something from RULED OUT | /refresh immediately |
| "I don't understand why..." | /refresh immediately |
| Context below 20% | /refresh + checkpoint |

**If in doubt, /refresh. It costs 1 bit. Being wrong costs 20.**

---

## THE HARD TRUTH

This bug has resisted:
- Multiple Claude instances
- Linus persona (systems thinking)
- Chuck Moore persona (Forth expertise)
- Shannon persona (information theory)
- Ferrucci persona (systematic methodology)

Each thought they found it. None did.

**What this means:**
- The bug is subtle (obvious things have been tried)
- Mental models are wrong somewhere (not just missing a detail)
- The fix will be small but in an unexpected place
- Finding it requires ACTUAL DISCRIMINATION, not clever reasoning

**What will work:**
- Parallel hypothesis testing (cover more ground)
- Binary comparison (working vs failing disassembly)
- Mechanical verification (not "looks right")
- Humility (your first instinct is probably wrong too)

---

## LIFECYCLE

```
+-----------------------------------------------------------------------------+
|                         /debug-channel LIFECYCLE                            |
+-----------------------------------------------------------------------------+
|                                                                             |
|  +---------+     +------------------+     +-----------------+               |
|  | /clear  | --> | New agent starts | --> | Read DEBUG-STATE|               |
|  +---------+     +------------------+     +--------+--------+               |
|                                                    |                        |
|                                           +--------v--------+               |
|                                           | Resume from     |               |
|                                           | NEXT ACTION     |               |
|                                           +--------+--------+               |
|                                                    |                        |
|       +--------------------------------------------+------------------+     |
|       |                    EXPERIMENT LOOP                            |     |
|       |                                            v                  |     |
|       |  +----------+   +----------+   +----------------------+       |     |
|       |  | Plan     |-->| Execute  |-->| Update DEBUG-STATE   |--+    |     |
|       |  | (predict)|   | (test)   |   | (log, metrics, next) |  |    |     |
|       |  +----------+   +----------+   +----------------------+  |    |     |
|       |       ^                                                  |    |     |
|       |       +--------------------------------------------------+    |     |
|       |                                                               |     |
|       +---------------------------------------------------------------+     |
|                                                    |                        |
|                          +-----------------+-------+-------+                |
|                          v                 v               v                |
|                   +------------+    +------------+   +----------+           |
|                   | Confidence |    | Budget     |   | /clear   |           |
|                   | > 80%: FIX |    | exhausted: |   | State    |           |
|                   +------------+    | ESCALATE   |   | persists |           |
|                                     +------------+   +----------+           |
+-----------------------------------------------------------------------------+
```

---

## DECISION TREE

```
                           +---------------------+
                           |   Check Progress    |
                           +----------+----------+
                                      |
                      +---------------+---------------+
                      v               v               v
                +---------+    +-----------+   +-----------+
                |Entropy v|    |Entropy =  |   |Entropy ^  |
                |Progress |    |  Stalled  |   |REGRESSION |
                +----+----+    +-----+-----+   +-----+-----+
                     |               |               |
           +---------+---------+     |         +-----+-----+
           v                   v     v         v           v
      +---------+        +-------------+  +--------+  +--------+
      |Conf>80% |        |Conf 50-80%  |  |STOP    |  |REVERT  |
      |   FIX   |        | CONTINUE    |  |Refresh |  |to last |
      +---------+        +-------------+  |Recalib |  |good    |
           |                   |          +--------+  +--------+
           v                   v               |           |
      +---------+        +-------------+       v           v
      |Attempt  |        |Parallelize? |  +--------+  +--------+
      |fix, test|        |if H indep   |  |New hyp |  |Commit  |
      |commit   |        +-------------+  |needed  |  |first!  |
      +---------+                         +--------+  +--------+

LEGEND:
  Entropy v = decreasing (good)
  Entropy = = stalled (3 experiments, +/-0.1)
  Entropy ^ = increasing (regression)
  Conf = confidence in top hypothesis
```

---

## ACTION COSTS

| Action                          | Cost    | Parallel | Notes                      |
|---------------------------------|---------|----------|----------------------------|
| Read file (first time)          | 1 bit   | 0.5 each | Parallel reads encouraged  |
| Read file (re-read)             | 3 bits  | N/A      | Must justify why           |
| Run test                        | 2 bits  | 1 each   | Parallel tests encouraged  |
| Add debug output                | 2 bits  | 1 each   | Different locations OK     |
| Make code change                | 4 bits  | N/A      | Sequential only            |
| Disassemble binary              | 1 bit   | N/A      | Cheap, do often            |
| Launch parallel agent           | 1 bit   | N/A      | Flat cost per agent        |
| Git commit                      | 0 bits  | N/A      | **FREE - always commit**   |
| Git revert without commit       | **inf** | N/A      | **FORBIDDEN**              |
| "Explore" / "investigate"       | **inf** | N/A      | **FORBIDDEN** - not action |

**Budget: 20 bits per bug (hard limit)**

---

## DASHBOARD (Display EVERY message)

```
+===============================================================================+
| BUG: [signature]                               SESSION: [N] experiments       |
| LAYER: L[N] [file]                             GOAL: Self-hosting at [X]%     |
+===============================================================================+
| HYPOTHESES           NOW   PREV   D    LAYER  EVIDENCE                        |
| H1: [desc]           [%] < [%]  [+-N]   L[N]   [what moved it]                |
| H2: [desc]           [%] < [%]  [+-N]   L[N]   [what moved it]                |
| H3: [desc]           [%] < [%]  [+-N]   L[N]   [what moved it]                |
| Hw: Unknown          [%] < [%]  [+-N]   L[?]   [why this high]                |
+-------------------------------------------------------------------------------+
| PROGRESS                                                                      |
| Entropy: [X] bits (target <0.5)     [=======-------] [v/=/^]                  |
| Confidence: [X]% (target >80%)      [========------] [v/=/^]                  |
| Budget: [X]/20 bits remaining       [==========----]                          |
| Efficiency: [X] bits reduced/spent  [=========-----] (target >0.5)            |
+-------------------------------------------------------------------------------+
| PROJECTION                                                                    |
| At current efficiency: [N] more experiments to 80% confidence                 |
| Budget status: [sufficient / tight / insufficient]                            |
| Recommendation: [CONTINUE / PARALLELIZE / RECALIBRATE / FIX / ESCALATE]       |
+-------------------------------------------------------------------------------+
| TREND: [vvvxv] | STATUS: [GREEN/YELLOW/RED] | REFRESH: [N] experiments ago    |
+===============================================================================+
| BLOCKER: [current obstacle]                                                   |
| MISSING: [information gap]                                                    |
| NEXT: [highest-value action]                                                  |
+===============================================================================+
```

**Status Colors:**
- **GREEN**: Entropy decreasing, confidence > 60%, efficiency > 0.5
- **YELLOW**: Stalled (3 exp same entropy) OR confidence 40-60%
- **RED**: Regression (entropy increased) OR confidence < 40% OR budget < 5

---

## COMPILER WISDOM (Laws)

1. **Layer Law**: Bugs manifest N layers above cause. Trace downward.
2. **Pass Law**: Pass 1 poisons Pass 2. Verify each pass independently.
3. **Offset Law**: Every "+N" in address arithmetic is a bug waiting.
4. **Width Law**: Machine word size vs abstraction size. Machine wins silently.
5. **Bootstrap Law**: Self-hosting is the only complete test.
6. **Codegen Law**: Disassemble. Compare expected vs actual byte-by-byte.
7. **Simplicity Law**: Bug in "complex cases" affects all cases.

**Anti-Patterns That Will Fail:**
- Printf debugging without hypotheses (drown in output)
- Reading code "to understand it" (understand assumptions, not code)
- Fixing symptoms (bug resurfaces in different costume)
- Trusting simple cases (they hide edge cases)
- Debugging codegen without disassembly (debugging imagination)

---

## SHANNON ARCHITECTURE

```
PASS 1 (scan-all)          PASS 2 (compile-all)
      |                           |
      v                           v
  info-buf                   code-buf + dict-buf
  (metadata)                 (machine code)

LAYERS:
  L0: main.fs      - orchestration, compile-token dispatch
  L1: asm.fs       - x86-64 instruction encoding
  L2: stack.fs     - register-mapped stack (RAX/RBX/RCX/R15)
  L3: prims.fs     - primitive codegen (emit-*)
  L4: opt-*.fs     - optimizations (folding, fusing)
  L5: control.fs   - control flow (if/then/else, begin/while)
  L6: defs.fs      - variable, constant, create ($800 flag)
  L7: compile.fs   - high-level compilation orchestration
  L8: elf.fs       - ELF binary generation

CRITICAL WIDTH ISSUES:
  dict-buf[24]: code-addr (4 bytes) - @ reads 8! MUST mask $FFFFFFFF
  dict-buf[28]: flags (4 bytes)     - @ reads 8! MUST mask $FFFFFFFF
  info-buf fields: same issue - verify width before @

GOAL: Self-hosting
  ./engine/fifth shannon/main.fs shannon/main.fs /tmp/s2
  /tmp/s2 shannon/main.fs shannon/main.fs /tmp/s3
  diff /tmp/s2 /tmp/s3  # MUST BE IDENTICAL
```

---

## REFRESH TRIGGERS

Automatic refresh when:
- Entropy stalls (same +/-0.1 for 3 experiments)
- Layer transition (bug traced to different layer)
- Every 10 experiments
- After any fix attempt
- On session resume

### /refresh Command

Forces immediate architectural recalibration:
- Layer audit (which layers verified OK vs untested)
- Width audit (all @ on 4-byte fields masked?)
- Pass verification (Pass 1 output correct? Pass 2 input matches?)
- Disassembly (actual bytes vs expected)
- Hypothesis recalibration based on findings
- Self-hosting progress update

```
+===============================================================================+
| /refresh RESULTS                                                              |
+===============================================================================+
| LAYER AUDIT:                                                                  |
|   L0 main.fs     [OK] [BUG] [?]     L5 control.fs  [OK] [BUG] [?]            |
|   L1 asm.fs      [OK] [BUG] [?]     L6 defs.fs     [OK] [BUG] [?]            |
|   L2 stack.fs    [OK] [BUG] [?]     L7 compile.fs  [OK] [BUG] [?]            |
|   L3 prims.fs    [OK] [BUG] [?]     L8 elf.fs      [OK] [BUG] [?]            |
|   L4 opt-*.fs    [OK] [BUG] [?]                                               |
+-------------------------------------------------------------------------------+
| WIDTH AUDIT:                                                                  |
|   dict-addr @    [masked] [UNMASKED-BUG]                                      |
|   dict-flags @   [masked] [UNMASKED-BUG]                                      |
|   info-buf @     [masked] [UNMASKED-BUG]                                      |
+-------------------------------------------------------------------------------+
| CODEGEN AUDIT (for current failing case):                                     |
|   Source:   [the failing Forth code]                                          |
|   Expected: [what bytes should be generated]                                  |
|   Actual:   [disassembly of what was generated]                               |
|   Delta:    [specific differences]                                            |
+-------------------------------------------------------------------------------+
| SELF-HOST PROGRESS:                                                           |
|   [==========----------] 50% - blocks on: [specific missing feature]          |
+===============================================================================+
```

---

## EXPERIMENT PROTOCOL

1. **State hypothesis** with probability and layer
2. **Pay bit cost** from budget
3. **Predict** what experiment will show for each outcome
4. **Execute** single minimal test
5. **Update** DEBUG-STATE.md:
   - Append to experiment log (never delete)
   - Update hypothesis probabilities (Bayesian)
   - Update metrics (entropy, confidence, budget, efficiency)
   - Update NEXT ACTION
6. **Commit** if significant change

### Parallelism Check (Before every experiment)

```
+---------------------------------------------------------------+
| PARALLELISM CHECK                                             |
|                                                               |
| Can this experiment run simultaneously with others?           |
| [ ] YES -> Launch as parallel agent (0.5 bit each)            |
| [ ] NO  -> State dependency: "Needs result of [X] first"      |
|                                                               |
| Independent hypotheses that could test in parallel:           |
| * H1 test: [experiment A]                                     |
| * H2 test: [experiment B]                                     |
| * H3 test: [experiment C]                                     |
+---------------------------------------------------------------+
```

---

## GIT DISCIPLINE (Non-negotiable)

### THE IRON RULE

> **Never `git checkout`, `git reset`, or `git revert` without first committing current state.**

Even broken code is information. Losing it is losing bits.

### Before any destructive git operation:

```bash
# MANDATORY SEQUENCE
git add -A
git commit -m "WIP: [state description] - before reverting to investigate"
git log --oneline -1  # Record the hash
# NOW you may checkout/reset/revert
```

### Commit message format:

```
[debug] [bug-sig]: [what was tried] - [result]

Examples:
[debug] var-crash: mask dict-flags - still fails, main body missing
[debug] var-crash: WIP state before revert to investigate sixth.fs
[debug] var-crash: FIXED - ct-flush was not called after $800 path
```

---

## STATE FILE

**Location:** `compiler/shannon/DEBUG-STATE.md`

**Rule:** This file is the SINGLE SOURCE OF TRUTH.
- Read it FIRST, before any action
- Update it LAST, after every action
- Commit it with every significant change

The file survives /clear. It IS the debugging context.

---

## STATE FILE TEMPLATE

When no state file exists, create from this template:

```markdown
# SHANNON COMPILER DEBUG STATE
# Last updated: [timestamp]
# Session: 0 total experiments

## QUICK RESUME

To continue debugging:
1. Read this entire file
2. Run: /debug-channel --resume
3. Pick up at: [NEXT ACTION below]

## CURRENT BUG

SIGNAL: [exact command that fails]
WORKING: [nearest working variant]
DELTA: [precise difference between them]

## ARCHITECTURE SNAPSHOT

Bug is in: LAYER [N] - [file.fs]
Data flow: [source] -> [transform] -> [break point]

Relevant structures:
  [structure]: [description] - [any width/mask notes]

## HYPOTHESES

| ID | Description | Prob | Layer | Last Evidence |
|----|-------------|------|-------|---------------|
| H1 | [desc]      | [%]  | L[N]  | [evidence]    |
| H2 | [desc]      | [%]  | L[N]  | [evidence]    |
| Hw | Unknown     | [%]  | L[?]  | [why]         |

## CERTAIN FACTS (Do not re-verify)

1. [fact] - established by [experiment] on [date]

## RULED OUT (Do not re-test)

1. [hypothesis] - ruled out by [evidence]

## EXPERIMENT LOG (Append-only)

[NNN] [date] E:[entropy] C:[conf]% | [experiment] | [result] | [v/x/-]

## BLOCKERS

Current: [specific obstacle]
Type: [ ] Information [ ] Discrimination [ ] Tool [ ] Understanding

## GAPS

Missing: [specific information needed]
To fill: [experiment that would provide it]

## LAST DISASSEMBLY

Source: [failing code]
Binary: [path]
Key bytes:
  [offset]: [expected] vs [actual] - [significance]

## GIT TRAIL

[hash] [date] [message]

## METRICS

| Metric     | Value | Target | Status |
|------------|-------|--------|--------|
| Entropy    | [X]   | < 0.5  | [====] |
| Confidence | [X]%  | > 80%  | [====] |
| Budget     | [X]   | 20     | [====] |
| Efficiency | [X]   | > 0.5  | [====] |

## SELF-HOSTING PROGRESS

[====================] [X]%
Blocks on: [current missing feature]

## NEXT ACTION

**Do this first:**
[Specific, actionable next step]

**Then:**
[Following step]

## SESSION HISTORY

### Session 1 - [date]
- Started with: [initial state]
- Ended with: [final state]
- Key finding: [most important discovery]
```

---

## FORBIDDEN

1. **Debugging without reading DEBUG-STATE.md first** - You WILL repeat work
2. **Claiming codegen understanding without disassembly** - You're guessing
3. **Re-testing CERTAIN FACTS or RULED OUT items** - Wasted bits
4. **Reverting without committing current state** - Lost information
5. **"Reading code to understand it" without specific question** - Narrative debugging
6. **Printf debugging without hypothesis and prediction** - Noise generation
7. **Fixing symptoms instead of root cause** - Bug will return
8. **Sequential experiments when parallel possible** - Wasted time
9. **Losing the log** - Reconstruct from git before continuing
10. **Changing hypotheses without evidence** - Show the Bayesian update

---

## COMMANDS

### /debug-channel
Start or resume debugging. Auto-detects state file.

### /debug-channel --resume
Explicitly resume from DEBUG-STATE.md

### /debug-channel --status
Display current state without taking action

### /debug-channel --checkpoint
Force metrics update and state file write

### /refresh
Full architectural refresh (see REFRESH section)

---

## USAGE

```
/debug-channel              # Start or resume
/debug-channel --resume     # Explicit resume
/debug-channel --status     # Show state
/debug-channel --checkpoint # Force save
/refresh                    # Architectural refresh
```

**The file is the brain. Read first. Update last. Commit always.**
