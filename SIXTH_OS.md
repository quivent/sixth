# Sixth OS: The Path to Bare Metal

## The Problem

Linux kernel: 30 million lines of C.

Sixth runs on Linux. That is 30 million lines of dependency we do not control.

## The Solution

colorForth: ~2KB. ~500 lines of Forth. Boots from BIOS. No OS. Forth *is* the OS.

Chuck Moore solved this in 2001. We just haven't listened yet.

---

## Current State: Sixth on Linux

Sixth assumes Linux for:

| Feature | Linux syscall | Lines in Sixth |
|---------|---------------|----------------|
| Write to stdout | sys_write (1) | ~15 |
| Exit program | sys_exit (60) | ~5 |
| Read file | sys_read (0) | ~20 |
| Open file | sys_open (2) | ~15 |
| Memory | static allocation | ~10 |

**Total syscall layer: ~65 lines.**

Plus ~80 lines for ELF header generation.

---

## Target State: Sixth on colorForth

colorForth provides:
- Direct keyboard via `key`
- Direct screen via `emit` (VGA)
- Block storage via `block` `update` `flush`
- No files. No ELF. No loader.

### What Changes in Sixth

| Change | Lines | Notes |
|--------|-------|-------|
| Replace sys_write with VGA emit | ~15 | Direct to video memory |
| Replace sys_read with block read | ~20 | 1KB blocks, not files |
| Remove ELF header generation | -80 | Just raw machine code |
| Add block-based source loading | ~30 | Blocks instead of files |
| Boot integration | ~20 | colorForth calls Sixth |

**Net change: ~65 lines replaced, ~50 lines added, ~80 lines deleted.**

Sixth gets *smaller* on colorForth. No ELF overhead. No syscall wrappers.

### What Changes in colorForth

Nothing.

It already boots. It already has I/O. Sixth is just another Forth program that generates machine code and jumps to it.

---

## The Merge

```
colorForth (2KB) + Sixth compiler (~2800 lines) = complete system
```

Boot from BIOS. Type Forth. Compile to native x86. Run.

No Linux. No C. No ELF. No dependencies.

**~3000 lines total from power-on to running optimized native code.**

---

## Comparison

| Layer | Linux Stack | Sixth OS |
|-------|-------------|----------|
| Application | varies | Forth |
| Compiler | GCC (15M lines) | Sixth (2800 lines) |
| OS | Linux (30M lines) | colorForth (2KB) |
| Bootloader | GRUB | colorForth |
| **Total** | **45M+ lines** | **~3000 lines** |

Ratio: 15,000:1

---

## The Hardware Path

Chuck Moore went further. The GA144 chip runs Forth as its instruction set.

| Layer | Linux | colorForth | GA144 |
|-------|-------|------------|-------|
| Application | C/Python | Forth | Forth |
| OS | Linux (30M) | colorForth (2KB) | none |
| Instruction set | x86 (4000+) | x86 | Forth (32) |

The logical end state is not Sixth on bare metal.

The logical end state is Sixth *as* the metal.

---

## Phases

| Phase | Description | Status |
|-------|-------------|--------|
| Current | Sixth on Linux | WORKING |
| Phase 10 | Sixth on colorForth | NOT STARTED |
| Phase 11 | Sixth on GA144 | FUTURE |

---

## References

- [colorForth](https://colorforth.github.io/cf.htm) - Chuck Moore's bare metal Forth
- [GreenArrays GA144](https://www.greenarraychips.com/) - Forth in silicon
- [Chuck Moore's creations](https://github.com/AshleyF/Color/blob/master/Docs/chuck_moores_creations.md)

---

## The Point

3000 lines of Forth from power-on to optimized native code.

That's the answer Chuck Moore already gave us.
