\ expect: 0
\ Test: Memory + control flow + arithmetic stress
\ Deep nesting with memory ops at every level

variable buf
variable stor0
variable stor1
variable stor2
variable stor3
variable stor4
variable stor5
variable stor6
variable stor7

: level3 ( n -- n' )
  dup 0 = if stor0 @ over stor0 @ + else
  dup 1 = if stor1 @ over stor1 @ + else
  dup 2 = if stor2 @ over stor2 @ + else
  dup 3 = if stor3 @ over stor3 @ + else
  dup 4 = if stor4 @ over stor4 @ + else
  dup 5 = if stor5 @ over stor5 @ + else
  dup 6 = if stor6 @ over stor6 @ + else
  stor7 @ over stor7 @ +
  then then then then then then then ;

: level2 ( n -- n' )
  dup 8 < if
    dup dup 2 * swap
    dup 0 = if drop stor0 ! else
    dup 1 = if drop stor1 ! else
    dup 2 = if drop stor2 ! else
    dup 3 = if drop stor3 ! else
    dup 4 = if drop stor4 ! else
    dup 5 = if drop stor5 ! else
    dup 6 = if drop stor6 ! else
    drop stor7 !
    then then then then then then then
    level3
  else
    drop 99
  then ;

: level1 ( -- sum )
  0
  8 0 do
    i level2 +
  loop ;

: main
  level1
  \ Complex stack manipulation with nested conditionals
  \ Verified via interpreter: result is 35
  35 = if 0 else 1 then ;
