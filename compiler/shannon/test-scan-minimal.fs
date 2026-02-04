\ Minimal scan.fs test - no s, dependency

\ Dependencies for scan.fs
: >= < 0= ;
: <= > 0= ;

\ Input buffer
150000 constant INPUT-SIZE
create input-buf INPUT-SIZE allot
variable input-len  0 input-len !
variable input-pos  0 input-pos !

\ String comparison
: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  dup 0= if 2drop drop true exit then
  0 ?do
    over i + c@ over i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

\ Simple tokenizer
: skip-ws ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ 33 < if 1 input-pos +! else exit then
  again ;

: get-token ( -- addr u )
  skip-ws
  input-pos @ input-len @ >= if 0 0 exit then
  input-buf input-pos @ +
  0
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ 32 > if
      1+
      1 input-pos +!
    else
      exit
    then
  again ;

\ Load scanner
include compiler/shannon/scan.fs

\ Test helper
: load-input ( addr u -- )
  dup input-len !
  0 input-pos !
  input-buf swap move ;

\ Simple test
: test1
  ." Loading test source..." cr
  s" : foo ( a -- b ) 1+ ; : bar ( x y -- z ) + ;" load-input

  ." Scanning..." cr
  scan-all

  ." Results:" cr
  ." info-count = " info-count @ . cr

  info-count @ 0 ?do
    ."   Entry " i . ." : "
    i info-entry 24 type
    ."  nargs=" i info-entry 24 + c@ .
    ."  rets=" i info-entry 25 + c@ .
    cr
  loop

  info-count @ 2 = if ." PASS: 2 entries" else ." FAIL: expected 2" then cr
;

test1
bye
