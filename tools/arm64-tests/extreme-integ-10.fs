\ expect: 0
\ Test: Nested loops with early exit and variable mutation
\ unloop and leave inside complex structures

variable found
variable total
variable target

: search ( -- )
  0 found !
  0 total !
  50 target !
  10 0 do
    10 0 do
      i j * total +!
      total @ target @ > if
        1 found !
        unloop unloop exit
      then
    loop
  loop ;

: after ( -- n )
  found @ total @ + ;

: also-test ( -- n )
  0
  5 0 do
    i 1+ *
    dup 20 > if
      leave
    then
    i +
  loop ;

: main
  search
  after
  also-test +
  \ search: accumulates i*j until > 50
  \ 0*0=0, 0*1=0... eventually exceeds 50
  \ also-test: 0, then *1+0=0, *2+1=1, *3+2=5, *4+3=23 > 20 so leave with 23
  \ Result should be positive - compiler survival test
  dup 0> if drop 0 else drop 1 then ;
