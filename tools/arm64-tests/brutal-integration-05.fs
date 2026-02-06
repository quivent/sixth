\ expect: 0
\ Brutal Integration Test 05: Simplified Stack Machine
\ Tests: dispatch, memory, state, nested conditionals

variable prog-base
variable stack-base
variable sp-var
variable pc-var

: push-st ( n -- ) sp-var @ cells stack-base @ + ! sp-var @ 1+ sp-var ! ;
: pop-st ( -- n ) sp-var @ 1- dup sp-var ! cells stack-base @ + @ ;

: prog@ ( n -- opcode ) cells prog-base @ + @ ;
: prog! ( opcode n -- ) cells prog-base @ + ! ;

: init-vm ( -- )
  here prog-base !  16 cells allot
  here stack-base ! 16 cells allot
  0 pc-var !  0 sp-var ! ;

: step ( -- done? )
  pc-var @ prog@
  dup 0 = if drop 1 exit then
  dup 1 = if drop pc-var @ 1+ prog@ push-st pc-var @ 2 + pc-var ! 0 exit then
  dup 2 = if drop pop-st pop-st + push-st pc-var @ 1+ pc-var ! 0 exit then
  dup 3 = if drop pop-st pop-st swap - push-st pc-var @ 1+ pc-var ! 0 exit then
  dup 4 = if drop pop-st pop-st * push-st pc-var @ 1+ pc-var ! 0 exit then
  drop pc-var @ 1+ pc-var ! 0 ;

: run-vm ( -- )
  0 pc-var !  0 sp-var !
  begin step until ;

: main
  init-vm
  \ Program: push 3, push 4, add, push 2, mul = (3+4)*2 = 14
  1 0 prog!  3 1 prog!
  1 2 prog!  4 3 prog!
  2 4 prog!
  1 5 prog!  2 6 prog!
  4 7 prog!
  0 8 prog!
  run-vm
  pop-st 14 <> if 1 exit then
  0 ;
