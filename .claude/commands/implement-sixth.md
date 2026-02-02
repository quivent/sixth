# /implement-sixth - Parallel Agent Implementation Protocol

Implement the next roadmap item using disciplined parallel exploration before modification.

---

## PROTOCOL

### Step 0: Identity Restoration

Invoke `/chuck-moore` first. You need Chuck's brutal minimalism to avoid over-engineering.

### Step 1: Load Context

Invoke `/remember-sixth` to load the compiler encoding and source. Confirm you have:
- Data structures and layouts
- State variables
- Compilation flow
- Register allocation scheme

### Step 2: Identify Target

Read `compiler/ROADMAP.md` and identify the next NOT STARTED item. Current priorities:

**Phase 3: State Machine**
- `[` - switch to interpret mode
- `]` - switch to compile mode
- `LITERAL` - compile literal

**Phase 5: Interpreter (remaining)**
- `ABORT` - clear and restart
- `QUIT` - main REPL loop
- `POSTPONE` - compile compilation
- `DOES>` - set runtime behavior

**Phase 6: File I/O**
- `OPEN-FILE`, `READ-FILE`, `READ-LINE`, `INCLUDE`

### Step 3: Parallel Exploration

**DO NOT START CODING.** Deploy 2-3 Task agents in parallel to explore:

**Agent 1: Architecture**
- Read relevant sections of compiler/sixth.fs
- Identify existing infrastructure that can be reused
- Find where new code should be inserted

**Agent 2: Dependencies**
- Trace what the target word depends on
- Check if dependencies are implemented
- Identify any blockers

**Agent 3: Patterns**
- Find similar implemented words
- Extract the pattern for this type of word
- Note any gotchas from existing implementations

Use the Task tool with `subagent_type: "general-purpose"` for each agent. Run them in parallel (single message, multiple tool calls).

### Step 4: Synthesize Findings

After agents report back:
1. Summarize what you learned
2. Identify the minimal implementation
3. Plan the exact edits (file, line numbers, code)
4. Get user approval if approach is non-obvious

### Step 5: Implement

Make surgical edits:
- Prefer editing existing code over adding new code
- Keep changes minimal - Chuck Moore style
- No speculative features

### Step 6: Verify

Deploy verification agent:
- Run the test suite: `./fifth compiler/tests/run.fs`
- Run relevant benchmarks if touching codegen
- Confirm no regressions

### Step 7: Commit

If tests pass, commit with descriptive message.

---

## WHY THIS WORKS

The naive approach (start coding immediately) fails because:
- You don't understand existing infrastructure
- You duplicate code that already exists
- You miss edge cases that similar words handle
- You waste cycles on wrong approaches

The parallel exploration approach works because:
- Agents find the relevant code quickly
- You see patterns before implementing
- Dependencies are identified upfront
- Implementation becomes mechanical

---

## ANTI-PATTERNS

**DON'T:**
- Start editing code before agents report
- Guess at how things work
- Add "just in case" code
- Implement multiple features at once

**DO:**
- Wait for agent findings
- Trace actual code paths
- Make minimal changes
- Test after each change

---

## EXAMPLE SESSION

User: `/implement-sixth`

You:
1. Invoke `/chuck-moore` - restore identity
2. Invoke `/remember-sixth` - load compiler
3. Read ROADMAP.md - identify next item (e.g., `[` word)
4. Deploy 3 agents in parallel exploring STATE, immediate words, compilation flow
5. Wait for results
6. Synthesize: "`[` just needs to set STATE to 0 and be marked IMMEDIATE"
7. Edit: Add gen-lbracket, mark immediate, ~5 lines
8. Test: Run test suite
9. Commit

Total: surgical precision, no flailing.
