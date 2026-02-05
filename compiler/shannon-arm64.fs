\ compiler/shannon-arm64.fs - Clean-room ARM64/Mach-O native compiler
\
\ Top-level include file. Loads all modules in dependency order.
\ Tests live in tools/arm64-tests/ — run with: tools/arm64-tests/test
\
\ Architecture:
\   asm.fs    — ARM64 instruction encoding + code buffer
\   stack.fs  — Data stack ops, literal loading, prologue/epilogue
\   prims.fs  — Arithmetic + bitwise primitives
\   macho.fs  — Mach-O binary format, build, save
\
\ Reference: tools/macho-test.c (Phase 0, all tests passing)
\ Host: engine/fifth (C interpreter on macOS ARM64)
\ Output: Mach-O dynamically-linked executable via LC_MAIN

include compiler/shannon/arch/arm64/asm.fs
include compiler/shannon/arch/arm64/stack.fs
include compiler/shannon/arch/arm64/prims.fs
include compiler/shannon/arch/arm64/control.fs
include compiler/shannon/arch/arm64/macho.fs
include compiler/shannon/arch/arm64/compile.fs
