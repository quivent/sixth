# Chuck Moore Says

## The Audit

**SIZE: 2679 lines.** For a native x86-64 compiler that beats GCC -O2.

Not unreasonable. Real work: ELF generation, forward references, 9 optimizations, register allocation. GCC is millions of lines. This is 1/1000th for 90% of the result.

---

## THE GOOD

- Compiles Forth to native x86-64. No interpreter loop at runtime. Direct machine code.
- Optimization list (lines 1-68) is honest documentation. Each one named, explained, with test numbers.
- Constant folding, peephole optimization, tail calls - the classic wins are there.
- Fits in your head. One person can understand the whole thing.

---

## THE BAD

### 1. 126 lines of 2constant declarations (lines 1744-1870)

```forth
s" foo" s, 2constant $foo
s" bar" s, 2constant $bar
\ ... 124 more times
```

This is a dictionary. Should be generated or factored. One word that parses and creates these. **Save 100 lines.**

### 2. compile-builtin is 396 lines

One function. 396 lines. That's not Forth. That's a monolith wearing Forth's clothes.

A 400-line chain of `2dup $foo str= if ... then`. Linear search through 100+ words. Duplicated patterns everywhere.

---

## THE UGLY

### 3. String comparison for dispatch

Every word lookup:
```forth
2dup $foo str= if 2drop ... true exit then
```

O(n) with n=100+ comparisons. A hash table would be O(1).

But more importantly - every handler has the same boilerplate. Factor it:

```forth
: handle ( -- ) ... ;
: try-word ( addr u c-addr u2 xt -- found? )
  >r 2over str= if 2drop r> execute true else r> drop false then ;
```

Then: `$foo ' handle try-word if exit then`

### 4. Commented-out code

```forth
\ INLINE disabled for now - needs debugging
\ dup dict-size dup 0> swap 20 <= and if
\   inline-code exit
\ then
```

Dead code. Either fix it or delete it. Commented code is a lie about the future.

### 5. 1650 test files

Why? Are they all necessary? How many test the same thing?

500 would probably cover the same behavior. Tests are code. Tests are maintenance. Tests are complexity.

---

## VERDICT

Not bad. For a Forth compiler, 2679 lines is reasonable. But it's grown organically.

The 396-line compile-builtin is the tell. Someone kept adding `if ... then` clauses instead of refactoring.

---

## THE FIX

1. Extract compile-builtin into table-driven dispatch
2. Generate the $foo 2constants programmatically
3. Delete commented inlining code (or fix it)
4. Audit 1650 tests - consolidate

**Target: 2000 lines** with the same functionality.

---

## ONE MORE THING

You're writing a compiler in interpreted Forth, to produce native code, so you can bootstrap. Correct.

But once bootstrapped - delete the interpreted version. Ship only the native compiler compiling itself.

That's the Forth way.

---

*"If you can't hold the whole thing in your head, it's too complicated."*
