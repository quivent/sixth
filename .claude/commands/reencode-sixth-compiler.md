# /reencode-sixth-compiler - Regenerate Compiler Encoding

Produce a one-page map of compiler/sixth.fs that fits in your head.

---

## INSTRUCTIONS

Read `compiler/sixth.fs` completely. Then write `compiler/ENCODING.md` with:

### DATA STRUCTURES (5 max)
For each: name, purpose, memory layout, size in bytes/cells

### STATE VARIABLES (10 max)
For each: name, what it tracks, valid values

### COMPILATION FLOW
5 steps from Forth source to ELF binary. No more than 5.

### KEY WORDS (20 max)
The 20 most important words. For each:
- Name
- Stack effect ( before -- after )
- One-line purpose
- What it calls or modifies

### CODE GENERATION
- Where does machine code get emitted?
- What is the byte encoding pattern for instructions?
- How does stack caching work? (3 sentences max)

### REGISTER ALLOCATION
- Which registers hold what?
- When do values spill to memory?

### THE TRICK
What is the one key insight that makes this compiler work? One paragraph maximum.

---

## CONSTRAINTS

- **ONE PAGE MAXIMUM**
- If it does not fit on one page, you do not understand it yet
- No prose. Tables and bullets only.
- Maximum signal, minimum bits.

---

## OUTPUT

Write to: `compiler/ENCODING.md`

Overwrite the existing file. The encoding must reflect the current state of the compiler.

---

## WHY THIS EXISTS

When you cannot hold 3000 lines in your head, you need a map. This is the map. Read it before modifying the compiler. Regenerate it after significant changes.

If the encoding is wrong, the compiler will surprise you. If the encoding is right, you know where everything is.
