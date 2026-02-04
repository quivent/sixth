\ compiler/shannon/test-scan.fs - Test harness for scan.fs scanner
\ Tests that the scanner correctly parses word definitions and stack comments.
\
\ Run with: ./engine/fifth compiler/shannon/test-scan.fs
\
\ This test sets up the required infrastructure, loads sample Forth source,
\ calls scan-all, and verifies that info-buf contains the expected entries.

\ ============================================================
\ INFRASTRUCTURE (normally from sixth.fs)
\ ============================================================

\ Define comparison operators not in base interpreter
: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;

\ s, stores a string in the dictionary, returning ( addr u )
\ The string is copied to HERE and HERE is advanced
: s, ( addr u -- addr u )
  here >r          \ save start address
  dup >r           \ save length
  0 ?do
    dup i + c@ c,
  loop
  drop
  r> r> swap       \ ( len addr )
  swap             \ ( addr len )
;

150000 constant INPUT-SIZE
create input-buf INPUT-SIZE allot
variable input-len  0 input-len !
variable input-pos  0 input-pos !

create token-buf 64 allot
variable token-len

\ ============================================================
\ STRING COMPARISON
\ ============================================================

: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  begin
    dup 0= if drop 2drop true exit then
    >r over c@ over c@ <> if 2drop r> drop false exit then
    1+ swap 1+ swap r> 1-
  again ;

\ ============================================================
\ TOKENIZER
\ ============================================================

: skip-ws ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@
    dup 32 <= if
      drop 1 input-pos +!
    else
      drop exit
    then
  again ;

: skip-line ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ 10 = if
      1 input-pos +! exit
    then
    1 input-pos +!
  again ;

: get-token ( -- addr u | 0 0 )
  skip-ws
  input-pos @ input-len @ >= if 0 0 exit then
  input-buf input-pos @ + c@ 92 = if  \ 92 = backslash
    skip-line
    recurse exit
  then
  input-buf input-pos @ + c@ 40 = if  \ 40 = open paren
    input-pos @ 1+ input-len @ < if
      input-buf input-pos @ 1+ + c@ 32 <= if
        1 input-pos +!
        begin
          input-pos @ input-len @ >= if 0 0 exit then
          input-buf input-pos @ + c@ 41 = if  \ 41 = close paren
            1 input-pos +! recurse exit
          then
          1 input-pos +!
        again
      then
    then
  then
  0 token-len !
  begin
    input-pos @ input-len @ >= if
      token-buf token-len @ exit
    then
    input-buf input-pos @ + c@
    dup 32 <= if
      drop token-buf token-len @ exit
    then
    token-buf token-len @ + c!
    1 token-len +!
    1 input-pos +!
    token-len @ 63 >= if token-buf token-len @ exit then
  again ;

\ ============================================================
\ LOAD SCANNER MODULE
\ ============================================================

\ Note: scan.fs defines INFO-MAX, info-buf, info-count, info-entry,
\ info-name=, info-find, scan-all, and related words.
\ It expects: get-token, str=, input-buf, input-pos, input-len

require compiler/shannon/scan.fs

\ ============================================================
\ TEST INFRASTRUCTURE
\ ============================================================

variable test-count  0 test-count !
variable fail-count  0 fail-count !

: test-name ( addr u -- )
  ." TEST: " type space ;

: test-ok ( -- )
  ." OK" cr
  1 test-count +! ;

: test-fail ( -- )
  ." FAIL" cr
  1 test-count +!
  1 fail-count +! ;

: assert= ( actual expected -- )
  = if test-ok else test-fail then ;

: assert-str= ( addr1 u1 addr2 u2 -- )
  str= if test-ok else test-fail then ;

\ ============================================================
\ HELPER: Load string into input buffer
\ ============================================================

: load-input ( addr u -- )
  dup input-len !
  0 input-pos !
  input-buf swap move ;

\ ============================================================
\ HELPER: Get name from info entry (first 24 bytes, null-terminated)
\ ============================================================

: entry-name ( entry -- addr u )
  \ Find null terminator in first 24 bytes
  dup 24 + over do
    i c@ 0= if
      drop dup i swap - unloop exit
    then
  loop
  24 ;

\ ============================================================
\ HELPER: Print info entry for debugging
\ ============================================================

: .entry ( entry -- )
  ." [" dup entry-name type ." ]"
  ."  nargs=" dup 24 + c@ .
  ."  rets=" dup 25 + c@ 1- .
  ."  io=" dup 26 + c@ .
  ."  body-pos=" 30 + @ . ;

: .info-buf ( -- )
  ." INFO-BUF: " info-count @ . ." entries" cr
  info-count @ 0 ?do
    ."   " i . ." : " i info-entry .entry cr
  loop ;

\ ============================================================
\ TEST SOURCE - stored as a buffer
\ ============================================================
\ Test cases:
\   : test1 ( a b -- c ) + ;           nargs=2, rets=1
\   : test2 ( n -- ) drop ;            nargs=1, rets=0
\   : test3 ( -- n ) 42 ;              nargs=0, rets=1
\   : test4 ( a b c -- d e ) rot + ;   nargs=3, rets=2

create test-src-buf 256 allot
variable test-src-len

: init-test-source ( -- )
  \ Build test source manually character by character
  \ ": test1 ( a b -- c ) + ;" followed by newline
  \ ": test2 ( n -- ) drop ;" followed by newline
  \ ": test3 ( -- n ) 42 ;" followed by newline
  \ ": test4 ( a b c -- d e ) rot + ;" followed by newline
  s" : test1 ( a b -- c ) + ;" test-src-buf swap dup >r move r>
  10 test-src-buf over + c! 1+   \ newline
  s" : test2 ( n -- ) drop ;" test-src-buf 2 pick + swap dup >r move r> +
  10 test-src-buf over + c! 1+   \ newline
  s" : test3 ( -- n ) 42 ;" test-src-buf 2 pick + swap dup >r move r> +
  10 test-src-buf over + c! 1+   \ newline
  s" : test4 ( a b c -- d e ) rot + ;" test-src-buf 2 pick + swap dup >r move r> +
  10 test-src-buf over + c! 1+   \ newline
  test-src-len !
;

\ ============================================================
\ TEST CASES
\ ============================================================

: run-tests ( -- )
  \ Initialize test source
  init-test-source

  \ Load test source into input buffer
  test-src-buf test-src-len @ load-input

  \ Scan the source
  scan-all

  \ Debug: show what we found
  ." === Scanner Results ===" cr
  .info-buf
  cr

  \ ----------------------------------------
  \ Test: info-count should be 4
  \ ----------------------------------------
  s" info-count=4" test-name
  info-count @ 4 assert=

  \ ----------------------------------------
  \ Test: entry 0 should be "test1"
  \ ----------------------------------------
  s" entry0-name=test1" test-name
  0 info-entry entry-name s" test1" assert-str=

  s" entry0-nargs=2" test-name
  0 info-entry 24 + c@ 2 assert=

  s" entry0-rets=1" test-name
  0 info-entry 25 + c@ 1- 1 assert=

  \ ----------------------------------------
  \ Test: entry 1 should be "test2"
  \ ----------------------------------------
  s" entry1-name=test2" test-name
  1 info-entry entry-name s" test2" assert-str=

  s" entry1-nargs=1" test-name
  1 info-entry 24 + c@ 1 assert=

  s" entry1-rets=0" test-name
  1 info-entry 25 + c@ 1- 0 assert=

  \ ----------------------------------------
  \ Test: entry 2 should be "test3"
  \ ----------------------------------------
  s" entry2-name=test3" test-name
  2 info-entry entry-name s" test3" assert-str=

  s" entry2-nargs=0" test-name
  2 info-entry 24 + c@ 0 assert=

  s" entry2-rets=1" test-name
  2 info-entry 25 + c@ 1- 1 assert=

  \ ----------------------------------------
  \ Test: entry 3 should be "test4"
  \ ----------------------------------------
  s" entry3-name=test4" test-name
  3 info-entry entry-name s" test4" assert-str=

  s" entry3-nargs=3" test-name
  3 info-entry 24 + c@ 3 assert=

  s" entry3-rets=2" test-name
  3 info-entry 25 + c@ 1- 2 assert=

  \ ----------------------------------------
  \ Test: body-pos points to correct location
  \ ----------------------------------------
  \ For test1, body should start after ": test1 ( a b -- c ) "
  \ which is around position 21
  s" entry0-body-pos>0" test-name
  0 info-entry 30 + @ 0 > if test-ok else test-fail then

  \ ----------------------------------------
  \ Summary
  \ ----------------------------------------
  cr ." === Summary ===" cr
  ." Tests: " test-count @ .
  ." Pass: " test-count @ fail-count @ - .
  ." Fail: " fail-count @ . cr
  fail-count @ 0= if
    ." All tests passed!" cr
  else
    ." SOME TESTS FAILED!" cr
  then
;

\ ============================================================
\ MAIN
\ ============================================================

run-tests
bye
