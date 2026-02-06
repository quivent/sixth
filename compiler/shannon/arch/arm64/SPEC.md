# ARM64 Port Specification

## Register Assignments
1. **X19** = TOS (top of stack, cached in register)
2. **X22** = data stack pointer (grows downward, points to first empty slot)
3. **X9** = scratch register (NOS after pop, temp calculations)
4. **X10** = scratch register (used by mod, rot, etc.)
5. **X28** = return stack pointer (reserved, not yet used)
6. **X0** = syscall argument 1 / return value
7. **X16** = syscall number (Darwin convention)

## Stack Convention
8. **Push**: `STR X19, [X22, #-8]!` (pre-decrement store) - decrements X22 by 8, then stores X19
9. **Pop to scratch**: `LDR X9, [X22], #8` (post-increment load) - loads to X9, then increments X22 by 8
10. **Pop to TOS**: `LDR X19, [X22], #8` - replaces TOS from memory stack
11. **Stack grows down**: Lower addresses = deeper stack

## Code Buffer (asm.fs)
12. **code-buf**: 262144 bytes allocated for generated code
13. **code-pos**: Current write position in code-buf
14. **>code**: Writes single byte to code-buf, increments code-pos
15. **emit32**: Writes 32-bit little-endian instruction (ARM64 fixed-width)

## Immediate Move Instructions (asm.fs)
16. **arm-movz**: `MOVZ Xd, #imm16, LSL #shift` - move wide with zero
17. **arm-movk**: `MOVK Xd, #imm16, LSL #shift` - move wide with keep

## Register-Register Operations (asm.fs)
18. **arm-add-reg**: `ADD Xd, Xn, Xm` - register addition
19. **arm-sub-reg**: `SUB Xd, Xn, Xm` - register subtraction
20. **arm-and-reg**: `AND Xd, Xn, Xm` - bitwise AND
21. **arm-orr-reg**: `ORR Xd, Xn, Xm` - bitwise OR
22. **arm-eor-reg**: `EOR Xd, Xn, Xm` - bitwise XOR
23. **arm-mul**: `MUL Xd, Xn, Xm` - multiply (encoded as MADD with XZR)
24. **arm-sdiv**: `SDIV Xd, Xn, Xm` - signed divide
25. **arm-msub**: `MSUB Xd, Xn, Xm, Xa` - multiply-subtract (Rd = Ra - Rn*Rm)

## Shift Operations (asm.fs)
26. **arm-lslv**: `LSLV Xd, Xn, Xm` - logical shift left variable
27. **arm-lsrv**: `LSRV Xd, Xn, Xm` - logical shift right variable
28. **arm-asrv**: `ASRV Xd, Xn, Xm` - arithmetic shift right variable
29. **arm-asr-imm**: `ASR Xd, Xn, #imm6` - arithmetic shift right immediate

## Immediate Arithmetic (asm.fs)
30. **arm-add-imm**: `ADD Xd, Xn, #imm12` - add immediate
31. **arm-sub-imm**: `SUB Xd, Xn, #imm12` - subtract immediate

## Move/Bitwise Helpers (asm.fs)
32. **arm-mov-reg**: `MOV Xd, Xm` - encoded as `ORR Xd, XZR, Xm`
33. **arm-mvn**: `MVN Xd, Xm` - bitwise NOT, encoded as `ORN Xd, XZR, Xm`
34. **arm-neg**: `NEG Xd, Xm` - negate, encoded as `SUB Xd, XZR, Xm`

## Load/Store (asm.fs)
35. **arm-str-pre**: `STR Xt, [Xn, #imm9]!` - store with pre-index
36. **arm-ldr-post**: `LDR Xt, [Xn], #imm9` - load with post-index
37. **arm-ldr-off**: `LDR Xt, [Xn, #imm12*8]` - load unsigned offset (scaled)
38. **arm-str-off**: `STR Xt, [Xn, #imm12*8]` - store unsigned offset (scaled)

## Compare Instructions (asm.fs)
39. **arm-cmp-reg**: `CMP Xn, Xm` - encoded as `SUBS XZR, Xn, Xm`
40. **arm-cmp-imm**: `CMP Xn, #imm12` - encoded as `SUBS XZR, Xn, #imm12`
41. **arm-cset**: `CSET Xd, cond` - set register to 0 or 1 based on condition
42. **arm-tst-reg**: `TST Xn, Xm` - encoded as `ANDS XZR, Xn, Xm`

## Branch Instructions (asm.fs)
43. **arm-bcond**: `B.cond offset` - conditional branch, 19-bit offset
44. **arm-b**: `B offset` - unconditional branch, 26-bit offset
45. **arm-bl**: `BL offset` - branch with link, 26-bit offset
46. **arm-svc**: `SVC #imm16` - supervisor call

## Stack Primitives (stack.fs)
47. **push-tos**: Push TOS to memory stack (`STR X19, [X22, #-8]!`)
48. **pop-nos**: Pop memory to X9 scratch (`LDR X9, [X22], #8`)
49. **emit-drop**: Pop memory to TOS (`LDR X19, [X22], #8`)
50. **emit-lit**: Push old TOS, load 64-bit immediate using MOVZ + MOVK sequence

## Stack Manipulation (stack.fs)
51. **emit-dup**: `( x -- x x )` - calls push-tos
52. **emit-swap**: `( x y -- y x )` - load NOS to X9, store TOS to NOS slot, MOV X9 to TOS
53. **emit-over**: `( x y -- x y x )` - push-tos, then load [X22+8] to TOS
54. **emit-rot**: `( x y z -- y z x )` - uses X9/X10 scratch, 5 instructions
55. **emit-nip**: `( x y -- y )` - just `ADD X22, X22, #8` (discard NOS)
56. **emit-tuck**: `( x y -- y x y )` - emit-swap emit-over
57. **emit-2dup**: `( x y -- x y x y )` - load NOS, push TOS, MOV, push, load, MOV
58. **emit-2drop**: `( x y -- )` - emit-drop emit-drop
59. **emit--rot**: `( x y z -- z x y )` - reverse rotation using X9/X10

## Arithmetic Primitives (prims.fs)
60. **emit-add**: `( x y -- x+y )` - pop-nos, ADD X19, X9, X19
61. **emit-sub**: `( x y -- x-y )` - pop-nos, SUB X19, X9, X19 (NOS - TOS)
62. **emit-mul**: `( x y -- x*y )` - pop-nos, MUL X19, X9, X19
63. **emit-div**: `( x y -- x/y )` - pop-nos, SDIV X19, X9, X19
64. **emit-mod**: `( x y -- x mod y )` - SDIV to X10, MSUB for remainder
65. **emit-negate**: `( x -- -x )` - SUB X19, XZR, X19
66. **emit-1+**: `( x -- x+1 )` - ADD X19, X19, #1
67. **emit-1-**: `( x -- x-1 )` - SUB X19, X19, #1

## Bitwise Primitives (prims.fs)
68. **emit-and**: `( x y -- x&y )` - pop-nos, AND X19, X9, X19
69. **emit-or**: `( x y -- x|y )` - pop-nos, ORR X19, X9, X19
70. **emit-xor**: `( x y -- x^y )` - pop-nos, EOR X19, X9, X19
71. **emit-invert**: `( x -- ~x )` - MVN X19, X19
72. **emit-lshift**: `( x n -- x<<n )` - pop-nos, LSLV X19, X9, X19
73. **emit-rshift**: `( x n -- x>>n )` - pop-nos, LSRV X19, X9, X19

## Comparison Primitives (prims.fs)
74. **emit-=**: `( x y -- flag )` - CMP, CSET EQ, NEG (true=-1)
75. **emit-<>**: `( x y -- flag )` - CMP, CSET NE, NEG
76. **emit-<**: `( x y -- flag )` - CMP, CSET LT, NEG (signed)
77. **emit->**: `( x y -- flag )` - CMP, CSET GT, NEG (signed)
78. **emit-<=**: `( x y -- flag )` - CMP, CSET LE, NEG
79. **emit->=**: `( x y -- flag )` - CMP, CSET GE, NEG
80. **emit-u<**: `( x y -- flag )` - CMP, CSET LO, NEG (unsigned)
81. **emit-u>**: `( x y -- flag )` - CMP, CSET HI, NEG (unsigned)
82. **emit-0=**: `( x -- flag )` - CMP X19, #0, CSET EQ, NEG
83. **emit-0<>**: `( x -- flag )` - CMP X19, #0, CSET NE, NEG
84. **emit-0<**: `( x -- flag )` - ASR X19, X19, #63 (sign extension)
85. **emit-0>**: `( x -- flag )` - CMP X19, #0, CSET GT, NEG

## Prologue/Epilogue (stack.fs)
86. **gen-prologue**: `SUB X22, SP, #2048` - set up data stack 2KB below SP
87. **gen-epilogue**: `MOV X0, X19; MOVZ X16, #1; SVC #0x80` - exit with TOS

## Mach-O Generation (macho.fs)
88. **build-macho**: Constructs complete Mach-O header in macho-buf
89. **save-binary**: Writes macho-buf + code-buf to file, sets executable
90. **CODE-OFFSET**: 624 bytes (header size before code)
91. **11 load commands**: SEGMENT_64, sections, UNIX_THREAD, etc.
92. **16KB page alignment**: Darwin/ARM64 requirement

## Compiler (compile.fs)
93. **compile-token**: Dispatches word/number to appropriate emit-* function
94. **parse-number**: Converts string to number, returns flag
95. **compile-string**: Compiles source string to code buffer
96. **str=**: String equality comparison for word lookup

## Control Flow (control.fs) - CREATED BUT NOT YET TESTED
97. **cf-stack**: 256-cell control flow stack for forward references
98. **gen-if**: CBZ X19 + emit-drop, returns patch address
99. **gen-then**: Patches forward reference from if/else
100. **gen-else**: B unconditional + patch if's CBZ

## Test Results
- **39 tests passing** (phase1: 1, phase2: 34, phase3: 4)
- All arithmetic, bitwise, stack, and comparison primitives verified
- Phase 3 tests compile from source strings (not just direct emit calls)
