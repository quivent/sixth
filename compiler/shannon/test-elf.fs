\ test-elf.fs - Test harness for ELF module
\ Verifies ELF64 header generation

\ ============================================================
\ TEST SETUP - Fake code buffer for elf-header
\ Must be defined BEFORE elf.fs is loaded
\ ============================================================

create code-buf 64 allot
variable code-pos  0 code-pos !

\ ============================================================
\ INLINE ELF MODULE (from elf.fs)
\ ============================================================

create elf-buf 256 allot
variable elf-pos  0 elf-pos !

: e,  ( b -- )        elf-buf elf-pos @ + c!  1 elf-pos +! ;
: e2, ( word -- )     dup e, 8 rshift e, ;
: e4, ( dword -- )    dup e2, 16 rshift e2, ;
: e8, ( qword -- )    dup e4, 32 rshift e4, ;

: elf-header ( -- )
  0 elf-pos !
  $7f e, 69 e, 76 e, 70 e,
  2 e,
  1 e,
  1 e,
  0 e,
  0 e8,
  2 e2,
  $3e e2,
  1 e4,
  $4000B0 e8,
  64 e8,
  0 e8,
  0 e4,
  64 e2,
  56 e2,
  2 e2,
  0 e2,
  0 e2,
  0 e2,
  1 e4,
  5 e4,
  0 e8,
  $400000 e8,
  $400000 e8,
  176 code-pos @ +
  dup e8,
  e8,
  $1000 e8,
  1 e4,
  6 e4,
  0 e8,
  $800000 e8,
  $800000 e8,
  0 e8,
  $200000 e8,
  $1000 e8, ;

\ ============================================================
\ TEST HELPERS
\ ============================================================

variable test-pass  0 test-pass !
variable test-fail  0 test-fail !

: check ( actual expected name-addr name-len -- )
  2>r = if
    1 test-pass +!
  else
    1 test-fail +!
    2r@ type ."  FAILED" cr
  then 2r> 2drop ;

: check-byte ( offset expected name-addr name-len -- )
  2>r swap elf-buf + c@ = if
    1 test-pass +!
  else
    1 test-fail +!
    2r@ type ."  FAILED" cr
  then 2r> 2drop ;

: check-word ( offset expected name-addr name-len -- )
  2>r swap elf-buf + dup c@ swap 1+ c@ 8 lshift or = if
    1 test-pass +!
  else
    1 test-fail +!
    2r@ type ."  FAILED" cr
  then 2r> 2drop ;

: check-qword ( offset expected name-addr name-len -- )
  2>r swap elf-buf +
  dup c@
  over 1+ c@ 8 lshift or
  over 2 + c@ 16 lshift or
  over 3 + c@ 24 lshift or
  over 4 + c@ 32 lshift or
  over 5 + c@ 40 lshift or
  over 6 + c@ 48 lshift or
  swap 7 + c@ 56 lshift or
  = if
    1 test-pass +!
  else
    1 test-fail +!
    2r@ type ."  FAILED" cr
  then 2r> 2drop ;

\ ============================================================
\ RUN TESTS
\ ============================================================

elf-header

\ --- Magic bytes (0x7f 'E' 'L' 'F') ---
0 $7f s" magic[0]=0x7f" check-byte
1 $45 s" magic[1]='E'" check-byte
2 $4c s" magic[2]='L'" check-byte
3 $46 s" magic[3]='F'" check-byte

\ --- ELF identification ---
4 2 s" EI_CLASS=64-bit" check-byte
5 1 s" EI_DATA=little-endian" check-byte
6 1 s" EI_VERSION=1" check-byte

\ --- ELF header fields ---
24 $4000B0 s" e_entry=0x4000B0" check-qword
32 64 s" e_phoff=64" check-qword
52 64 s" e_ehsize=64" check-word
54 56 s" e_phentsize=56" check-word
56 2 s" e_phnum=2" check-word

\ --- Program header 1 (code segment) at offset 64 ---
64 1 s" ph1_type=PT_LOAD" check-byte
68 5 s" ph1_flags=RX" check-byte
80 $400000 s" ph1_vaddr=0x400000" check-qword

\ --- Program header 2 (data segment) at offset 120 ---
120 1 s" ph2_type=PT_LOAD" check-byte
124 6 s" ph2_flags=RW" check-byte
136 $800000 s" ph2_vaddr=0x800000" check-qword

\ --- Total header size ---
elf-pos @ 176 s" elf-pos=176" check

\ ============================================================
\ REPORT
\ ============================================================

cr
test-fail @ 0= if
  ." PASS: All " test-pass @ . ." tests passed" cr
else
  ." FAIL: " test-fail @ . ." failures, " test-pass @ . ." passed" cr
then

bye
