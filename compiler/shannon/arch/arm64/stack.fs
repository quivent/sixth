\ stack.fs - Data stack operations + literal loading (Shannon Layer 1)
\ Depends on: asm.fs (code buffer + instruction encoders)
\
\ Register assignments:
\   X19 = TOS (top of stack, cached in register)
\   X22 = data stack pointer (grows downward)
\   X9  = scratch (NOS after pop, temp)
\   X10 = scratch (used by mod, etc.)
\   X28 = return stack pointer (future)
\
\ Stack convention:
\   push = STR X19, [X22, #-8]!   (pre-decrement store)
\   pop  = LDR Xn, [X22], #8      (post-increment load)

\ ============================================================
\ STACK PRIMITIVES
\ ============================================================

: push-tos ( -- )  \ Push TOS to memory stack
  19 22 -8 arm-str-pre emit32 ;

: pop-nos ( -- )   \ Pop memory stack into X9 (scratch)
  9 22 8 arm-ldr-post emit32 ;

: emit-drop ( -- )  \ Pop memory stack into TOS (discard old TOS)
  19 22 8 arm-ldr-post emit32 ;

\ ============================================================
\ LITERAL LOADING
\ ============================================================

\ Load 64-bit immediate into TOS (pushes old TOS first)
\ Uses MOVZ for bits[15:0], MOVK for each non-zero 16-bit chunk above.
: emit-lit ( n -- )
  push-tos
  dup $FFFF and 19 swap 0 arm-movz emit32
  dup 16 rshift $FFFF and ?dup if 19 swap 16 arm-movk emit32 then
  dup 32 rshift $FFFF and ?dup if 19 swap 32 arm-movk emit32 then
      48 rshift $FFFF and ?dup if 19 swap 48 arm-movk emit32 then ;

\ ============================================================
\ PROLOGUE / EPILOGUE
\ ============================================================

\ Entry: set up data stack pointer, return stack pointer, variable base, and here
\ Memory layout:
\   [X20 + 0] = here pointer (initialized to X20 + here-offset)
\   [X20 + 8 ...] = variable storage, then here-allocated data
: gen-prologue ( here-offset -- )
  22 31 2048 arm-sub-imm emit32     \ SUB X22, SP, #2048 (data stack: 2KB)
  28 31 3072 arm-sub-imm emit32     \ SUB X28, SP, #3072 (return stack: 1KB)
  \ X20 = X28 - 4096 (4KB for here/allot, using X28 as base since SP encoding is tricky)
  20 28 4095 arm-sub-imm emit32     \ SUB X20, X28, #4095
  20 20 1 arm-sub-imm emit32        \ SUB X20, X20, #1 (X20 = X28 - 4096)
  \ Initialize here pointer: [X20] = X20 + here-offset
  dup $1000 < if
    9 20 rot arm-add-imm emit32     \ ADD X9, X20, #offset (fits in imm12)
  else
    \ Large offset: load into X9 then add to X20
    dup $FFFF and 9 swap 0 arm-movz emit32
    16 rshift $FFFF and ?dup if 9 swap 16 arm-movk emit32 then
    9 20 9 arm-add-reg emit32       \ ADD X9, X20, X9
  then
  9 20 0 arm-str-off emit32 ;       \ STR X9, [X20] (store initial here)

\ Emit variable address: push X20+offset to TOS
: emit-var-addr ( offset -- )
  push-tos                          \ save current TOS
  dup 0= if
    drop
    19 20 arm-mov-reg emit32        \ MOV X19, X20 (offset 0)
  else
    dup $1000 < if
      19 20 rot arm-add-imm emit32  \ ADD X19, X20, #offset (fits in imm12)
    else
      \ Large offset: load into X9 then add
      dup $FFFF and 9 swap 0 arm-movz emit32   \ MOVZ X9, #lo
      16 rshift $FFFF and ?dup if 9 swap 16 arm-movk emit32 then  \ MOVK X9, #hi
      19 20 9 arm-add-reg emit32               \ ADD X19, X20, X9
    then
  then ;

\ Exit: TOS → exit code, terminate via SVC
: gen-epilogue ( -- )
  0 19 arm-mov-reg emit32            \ MOV X0, X19
  16 1 0 arm-movz emit32             \ MOVZ X16, #1
  $80 arm-svc emit32 ;               \ SVC #0x80

\ ============================================================
\ STACK MANIPULATION
\ ============================================================

: emit-dup ( -- )  \ ( x -- x x ) duplicate TOS
  push-tos ;

: emit-swap ( -- )  \ ( x y -- y x ) exchange TOS and NOS
  9 22 0 arm-ldr-off emit32          \ LDR X9, [X22] - load NOS into X9
  19 22 0 arm-str-off emit32         \ STR X19, [X22] - store TOS to NOS slot
  19 9 arm-mov-reg emit32 ;          \ MOV X19, X9 - X9 (old NOS) to TOS

: emit-over ( -- )  \ ( x y -- x y x ) copy NOS to TOS
  push-tos                           \ push old TOS
  19 22 1 arm-ldr-off emit32 ;       \ LDR X19, [X22, #8] - load NOS to TOS

: emit-rot ( -- )  \ ( x y z -- y z x ) rotate 3rd to TOS
  \ Stack: TOS=z, [X22]=y (NOS), [X22+8]=x (3rd)
  \ Want:  TOS=x, [X22]=z, [X22+8]=y
  9 22 0 arm-ldr-off emit32          \ X9 = y (NOS)
  10 22 1 arm-ldr-off emit32         \ X10 = x (3rd)
  19 22 0 arm-str-off emit32         \ [X22] = z (old TOS)
  9 22 1 arm-str-off emit32          \ [X22+8] = y
  19 10 arm-mov-reg emit32 ;         \ TOS = x

: emit-nip ( -- )  \ ( x y -- y ) discard NOS
  \ Just increment stack pointer, TOS unchanged
  22 22 8 arm-add-imm emit32 ;       \ ADD X22, X22, #8

: emit-tuck ( -- )  \ ( x y -- y x y ) copy TOS below NOS
  emit-swap emit-over ;

: emit-2dup ( -- )  \ ( x y -- x y x y ) duplicate top pair
  9 22 0 arm-ldr-off emit32          \ X9 = NOS (x)
  push-tos                           \ push y
  19 9 arm-mov-reg emit32            \ TOS = x
  push-tos                           \ push x
  9 22 1 arm-ldr-off emit32          \ X9 = y (now at [X22+8])
  19 9 arm-mov-reg emit32 ;          \ TOS = y

: emit-2drop ( -- )  \ ( x y -- ) discard top pair
  emit-drop emit-drop ;

: emit--rot ( -- )  \ ( x y z -- z x y ) reverse rotate
  \ Stack: TOS=z, [X22]=y, [X22+8]=x
  \ Want:  TOS=y, [X22]=x, [X22+8]=z
  9 22 0 arm-ldr-off emit32          \ X9 = y
  10 22 1 arm-ldr-off emit32         \ X10 = x
  19 22 1 arm-str-off emit32         \ [X22+8] = z (old TOS)
  10 22 0 arm-str-off emit32         \ [X22] = x
  19 9 arm-mov-reg emit32 ;          \ TOS = y

\ ============================================================
\ RETURN STACK OPERATIONS
\ ============================================================

: emit->r ( -- )  \ ( x -- ) ( R: -- x ) push TOS to return stack
  19 28 -8 arm-str-pre emit32        \ STR X19, [X28, #-8]!
  emit-drop ;                        \ pop data stack to TOS

: emit-r> ( -- )  \ ( -- x ) ( R: x -- ) pop return stack to TOS
  push-tos                           \ save current TOS
  19 28 8 arm-ldr-post emit32 ;      \ LDR X19, [X28], #8

: emit-r@ ( -- )  \ ( -- x ) ( R: x -- x ) copy top of return stack
  push-tos                           \ save current TOS
  19 28 0 arm-ldr-off emit32 ;       \ LDR X19, [X28] (no increment)

\ ============================================================
\ MEMORY OPERATIONS
\ ============================================================

: emit-@ ( -- )  \ ( addr -- value ) fetch 64-bit value
  19 19 0 arm-ldr-off emit32 ;       \ LDR X19, [X19]

: emit-! ( -- )  \ ( value addr -- ) store 64-bit value
  pop-nos                            \ X9 = value (was NOS)
  9 19 0 arm-str-off emit32          \ STR X9, [X19]
  emit-drop ;                        \ TOS = next item

: emit-c@ ( -- )  \ ( addr -- byte ) fetch byte (zero-extended)
  19 19 0 arm-ldrb-off emit32 ;      \ LDRB W19, [X19]

: emit-c! ( -- )  \ ( byte addr -- ) store byte
  pop-nos                            \ X9 = byte (was NOS)
  9 19 0 arm-strb-off emit32         \ STRB W9, [X19]
  emit-drop ;                        \ TOS = next item

: emit-+! ( -- )  \ ( n addr -- ) add n to memory cell
  pop-nos                            \ X9 = n (was NOS)
  10 19 0 arm-ldr-off emit32         \ LDR X10, [X19] (current value)
  10 10 9 arm-add-reg emit32         \ ADD X10, X10, X9
  10 19 0 arm-str-off emit32         \ STR X10, [X19]
  emit-drop ;                        \ TOS = next item

: emit-sp@ ( -- )  \ ( -- addr ) push data stack pointer
  push-tos                           \ save current TOS
  19 22 arm-mov-reg emit32 ;         \ MOV X19, X22

\ ============================================================
\ DICTIONARY WORDS (here, allot, ,, c,)
\ ============================================================
\ here pointer is stored at [X20], data starts at [X20+8]

: emit-here ( -- )  \ ( -- addr ) push current here pointer
  push-tos                           \ save current TOS
  19 20 0 arm-ldr-off emit32 ;       \ LDR X19, [X20] (here pointer)

: emit-allot ( -- )  \ ( n -- ) advance here by n bytes
  9 20 0 arm-ldr-off emit32          \ LDR X9, [X20] (load here)
  9 9 19 arm-add-reg emit32          \ ADD X9, X9, X19 (here += n)
  9 20 0 arm-str-off emit32          \ STR X9, [X20] (store new here)
  emit-drop ;                        \ drop n from stack

: emit-comma ( -- )  \ ( x -- ) store cell at here, advance by 8
  9 20 0 arm-ldr-off emit32          \ LDR X9, [X20] (load here)
  19 9 0 arm-str-off emit32          \ STR X19, [X9] (store value)
  9 9 8 arm-add-imm emit32           \ ADD X9, X9, #8 (here += 8)
  9 20 0 arm-str-off emit32          \ STR X9, [X20] (store new here)
  emit-drop ;                        \ drop value from stack

: emit-c-comma ( -- )  \ ( c -- ) store byte at here, advance by 1
  9 20 0 arm-ldr-off emit32          \ LDR X9, [X20] (load here)
  19 9 0 arm-strb-off emit32         \ STRB W19, [X9] (store byte)
  9 9 1 arm-add-imm emit32           \ ADD X9, X9, #1 (here += 1)
  9 20 0 arm-str-off emit32          \ STR X9, [X20] (store new here)
  emit-drop ;                        \ drop value from stack

\ ============================================================
\ I/O OPERATIONS (macOS ARM64 syscalls)
\ ============================================================

: emit-emit ( -- )  \ ( c -- ) output character
  \ Store byte to stack, write(1, &byte, 1)
  19 31 -16 arm-str-pre emit32       \ STR X19, [SP, #-16]! (push to real stack, 16-aligned)
  0 1 0 arm-movz emit32              \ MOV X0, #1 (stdout)
  1 31 0 arm-add-imm emit32          \ ADD X1, SP, #0 (address of byte - can't use MOV for SP)
  2 1 0 arm-movz emit32              \ MOV X2, #1 (count)
  16 4 0 arm-movz emit32             \ MOV X16, #4 (write syscall)
  $80 arm-svc emit32                 \ SVC #0x80
  31 31 16 arm-add-imm emit32        \ ADD SP, SP, #16 (restore stack)
  emit-drop ;                        \ pop character from data stack

: emit-cr ( -- )  \ output newline
  push-tos                           \ save TOS
  19 10 0 arm-movz emit32            \ MOV X19, #10 (newline)
  emit-emit ;                        \ emit it

: emit-type ( -- )  \ ( addr len -- ) output string
  \ write(1, addr, len) - len is TOS, addr is NOS
  0 1 0 arm-movz emit32              \ MOV X0, #1 (stdout)
  2 19 arm-mov-reg emit32            \ MOV X2, X19 (len = TOS)
  emit-drop                          \ pop len, TOS = addr
  1 19 arm-mov-reg emit32            \ MOV X1, X19 (addr = TOS)
  emit-drop                          \ pop addr
  16 4 0 arm-movz emit32             \ MOV X16, #4 (write syscall)
  $80 arm-svc emit32 ;               \ SVC #0x80

\ ============================================================
\ FILE OPERATIONS (macOS ARM64 syscalls)
\ ============================================================
\ Syscall numbers: open=5, close=6, read=3, write=4
\ Return: X0 = result (negative on error)

\ File path buffer offset and slurp buffer offset
\ Path buffer: X20 + 3072, 256 bytes
\ Slurp buffer: X20 + 3328, 256KB (for reading source files)
3072 constant PATH-BUF-OFFSET
3328 constant SLURP-BUF-OFFSET
262144 constant SLURP-BUF-SIZE

\ Helper: store 32-bit value at code-buf + offset (for patching instructions)
: patch32 ( u32 offset -- )
  code-buf + >r
  dup $FF and r@ c!
  8 rshift dup $FF and r@ 1+ c!
  8 rshift dup $FF and r@ 2 + c!
  8 rshift $FF and r> 3 + c! ;

: emit-open-file ( -- )  \ ( c-addr u fam -- fileid ior )
  \ Copy path to null-terminated buffer, then open()
  \ Save fam (flags) to X11
  11 19 arm-mov-reg emit32            \ MOV X11, X19
  emit-drop                           \ TOS = u
  \ Save len to X12
  12 19 arm-mov-reg emit32            \ MOV X12, X19
  emit-drop                           \ TOS = c-addr (source)

  \ X9 = dest start = X20 + PATH-BUF-OFFSET
  9 20 PATH-BUF-OFFSET arm-add-imm emit32

  \ X13 = copy dest pointer (starts at X9)
  13 9 arm-mov-reg emit32             \ MOV X13, X9

  \ CBZ X12, done (skip copy if len=0)
  code-pos @                          \ save CBZ position for patching
  12 0 arm-cbz emit32                 \ placeholder

  \ loop body start
  code-pos @                          \ save loop-body address

  \ Copy one byte: LDRB from source, STRB to dest, both post-increment
  10 19 1 arm-ldrb-post emit32        \ LDRB W10, [X19], #1
  10 13 1 arm-strb-post emit32        \ STRB W10, [X13], #1
  12 12 1 arm-sub-imm emit32          \ SUB X12, X12, #1

  \ CBNZ X12, loop (continue if len > 0)
  over code-pos @ - 4 /               \ offset = (loop - here) / 4 (negative)
  12 swap arm-cbnz emit32
  drop                                \ drop loop addr

  \ done: patch the CBZ to jump here
  code-pos @                          \ done address
  over - 4 /                          \ offset from CBZ to done
  12 swap arm-cbz                     \ regenerate CBZ with correct offset
  swap patch32                        \ patch at saved position (32-bit write)

  \ Store null terminator at end of copied string
  10 0 0 arm-movz emit32              \ MOV X10, #0
  10 13 0 arm-strb-off emit32         \ STRB W10, [X13]

  \ open(path, flags, mode) - syscall #5
  0 9 arm-mov-reg emit32              \ MOV X0, X9 (path buffer)
  1 11 arm-mov-reg emit32             \ MOV X1, X11 (flags)
  2 $1A4 0 arm-movz emit32            \ MOV X2, #0644 octal = 420
  16 5 0 arm-movz emit32              \ MOV X16, #5 (open)
  $80 arm-svc emit32                  \ SVC #0x80

  \ macOS ARM64: carry flag set on error, X0 = errno (positive)
  \ B.CS error (branch if carry set) - jump over success path
  code-pos @                          \ save B.CS position for patching
  2 0 arm-bcond emit32                \ placeholder B.CS (cond=2=CS)

  \ Success path: return (fd 0)
  push-tos                            \ make room for fileid
  0 22 0 arm-str-off emit32           \ [X22] = X0 (fd)
  19 0 0 arm-movz emit32              \ X19 = 0 (ior = success)
  code-pos @                          \ save B position for skip
  0 arm-b emit32                      \ placeholder B (skip error path)

  \ Error path: negate errno -> return (-errno ior)
  swap code-pos @ over - 4 /          \ compute B.CS offset
  2 swap arm-bcond swap patch32       \ patch B.CS

  push-tos                            \ make room for error result
  9 0 arm-mov-reg emit32              \ MOV X9, X0 (errno)
  19 31 9 arm-sub-reg emit32          \ SUB X19, XZR, X9 (negate: -errno)
  19 22 0 arm-str-off emit32          \ [X22] = -errno (fileid)
  19 0 0 arm-movz emit32              \ X19 = 0 (ior, conventionally 0)

  \ Patch the success path's unconditional branch to skip error
  code-pos @ over - 4 /               \ compute B offset
  arm-b swap patch32 ;                \ patch B

: emit-close-file ( -- )  \ ( fileid -- ior )
  \ close(fd) -> 0 on success, negative on error
  0 19 arm-mov-reg emit32            \ MOV X0, X19 (fd)
  16 6 0 arm-movz emit32             \ MOV X16, #6 (close syscall)
  $80 arm-svc emit32                 \ SVC #0x80
  19 0 arm-mov-reg emit32 ;          \ MOV X19, X0 (return syscall result)

: emit-write-file ( -- )  \ ( addr u fileid -- ior )
  \ write(fd, buf, count) -> bytes written or negative error
  0 19 arm-mov-reg emit32            \ MOV X0, X19 (fd)
  emit-drop                          \ TOS = u
  2 19 arm-mov-reg emit32            \ MOV X2, X19 (count)
  emit-drop                          \ TOS = addr
  1 19 arm-mov-reg emit32            \ MOV X1, X19 (buf)
  16 4 0 arm-movz emit32             \ MOV X16, #4 (write syscall)
  $80 arm-svc emit32                 \ SVC #0x80
  19 0 arm-mov-reg emit32 ;          \ MOV X19, X0 (return bytes written or error)

: emit-read-file ( -- )  \ ( addr u fileid -- u2 ior )
  \ read(fd, buf, count) -> bytes read or negative error
  0 19 arm-mov-reg emit32            \ MOV X0, X19 (fd)
  emit-drop                          \ TOS = u
  2 19 arm-mov-reg emit32            \ MOV X2, X19 (count)
  emit-drop                          \ TOS = addr
  1 19 arm-mov-reg emit32            \ MOV X1, X19 (buf)
  16 3 0 arm-movz emit32             \ MOV X16, #3 (read syscall)
  $80 arm-svc emit32                 \ SVC #0x80
  \ Return (u2 ior) where u2=bytes, ior=0 always (check u2<0 for error)
  push-tos                           \ make room for u2
  0 22 0 arm-str-off emit32          \ [X22] = X0 (u2)
  19 0 0 arm-movz emit32 ;           \ X19 = 0 (ior)

: emit-slurp-file ( -- )  \ ( c-addr u -- addr2 u2 ior )
  \ Read entire file into SLURP-BUF. Returns buffer addr, bytes read, ior.
  \ On error returns (0 0 -1)

  \ === Step 1: Copy path to PATH-BUF and null-terminate ===
  \ Save len to X12
  12 19 arm-mov-reg emit32            \ MOV X12, X19 (len)
  emit-drop                           \ TOS = c-addr (source)

  \ X9 = dest start = X20 + PATH-BUF-OFFSET
  9 20 PATH-BUF-OFFSET arm-add-imm emit32

  \ X13 = copy dest pointer (starts at X9)
  13 9 arm-mov-reg emit32             \ MOV X13, X9

  \ CBZ X12, copy-done (skip copy if len=0)
  code-pos @                          \ save CBZ position for patching
  12 0 arm-cbz emit32                 \ placeholder

  \ Copy loop
  code-pos @                          \ save loop-body address
  10 19 1 arm-ldrb-post emit32        \ LDRB W10, [X19], #1
  10 13 1 arm-strb-post emit32        \ STRB W10, [X13], #1
  12 12 1 arm-sub-imm emit32          \ SUB X12, X12, #1
  over code-pos @ - 4 /               \ offset = (loop - here) / 4
  12 swap arm-cbnz emit32
  drop                                \ drop loop addr

  \ copy-done: patch the CBZ
  code-pos @ over - 4 /
  12 swap arm-cbz swap patch32

  \ Store null terminator
  10 0 0 arm-movz emit32              \ MOV X10, #0
  10 13 0 arm-strb-off emit32         \ STRB W10, [X13]

  emit-drop                           \ pop c-addr from stack

  \ === Step 2: Open file (O_RDONLY=0) ===
  0 9 arm-mov-reg emit32              \ MOV X0, X9 (path buffer)
  1 0 0 arm-movz emit32               \ MOV X1, #0 (O_RDONLY)
  2 0 0 arm-movz emit32               \ MOV X2, #0 (mode, unused for read)
  16 5 0 arm-movz emit32              \ MOV X16, #5 (open)
  $80 arm-svc emit32                  \ SVC #0x80

  \ Check for error (carry set)
  code-pos @                          \ save B.CS position
  2 0 arm-bcond emit32                \ B.CS error (placeholder)

  \ === Step 3: Read into SLURP-BUF ===
  \ Save fd to X14
  14 0 arm-mov-reg emit32             \ MOV X14, X0 (fd)

  \ read(fd, SLURP-BUF, SLURP-BUF-SIZE)
  0 14 arm-mov-reg emit32             \ MOV X0, X14 (fd)
  1 20 SLURP-BUF-OFFSET arm-add-imm emit32  \ ADD X1, X20, #SLURP-BUF-OFFSET
  \ Load SLURP-BUF-SIZE (262144 = 0x40000) into X2
  2 0 0 arm-movz emit32               \ MOV X2, #0
  2 4 16 arm-movk emit32              \ MOVK X2, #4, LSL #16 (X2 = 0x40000)
  16 3 0 arm-movz emit32              \ MOV X16, #3 (read)
  $80 arm-svc emit32                  \ SVC #0x80

  \ Save bytes read to X15
  15 0 arm-mov-reg emit32             \ MOV X15, X0 (bytes read)

  \ === Step 4: Close file ===
  0 14 arm-mov-reg emit32             \ MOV X0, X14 (fd)
  16 6 0 arm-movz emit32              \ MOV X16, #6 (close)
  $80 arm-svc emit32                  \ SVC #0x80

  \ === Step 5: Return success (addr2 u2 0) ===
  push-tos                            \ make room
  9 20 SLURP-BUF-OFFSET arm-add-imm emit32  \ ADD X9, X20, #SLURP-BUF-OFFSET
  9 22 0 arm-str-off emit32           \ [X22] = addr2
  push-tos                            \ make room
  15 22 0 arm-str-off emit32          \ [X22] = u2 (bytes read)
  19 0 0 arm-movz emit32              \ X19 = 0 (ior = success)
  code-pos @                          \ save B position
  0 arm-b emit32                      \ B skip-error (placeholder)

  \ === Error path: return (0 0 -1) ===
  swap code-pos @ over - 4 /          \ compute B.CS offset
  2 swap arm-bcond swap patch32       \ patch B.CS

  push-tos                            \ make room
  19 0 0 arm-movz emit32              \ MOV X19, #0
  19 22 0 arm-str-off emit32          \ [X22] = 0 (addr2)
  push-tos                            \ make room
  19 22 0 arm-str-off emit32          \ [X22] = 0 (u2)
  19 0 0 arm-movz emit32              \ MOV X19, #0
  19 19 arm-mvn emit32                \ MVN X19, X19 (X19 = -1)

  \ Patch the success path's B to skip error
  code-pos @ over - 4 /
  arm-b swap patch32 ;
