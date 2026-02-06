\ expect: 0
\ Test: Chained +! operations on same cell - stress atomic updates
\ Increment a cell 1000 times, verify final value

variable counter

: stress-add ( -- )
  1000 0 do 1 counter +! loop ;

: stress-sub ( -- )
  500 0 do -1 counter +! loop ;

: main
  0 counter !
  stress-add
  counter @ 1000 <> if 1 exit then
  stress-sub
  counter @ 500 <> if 2 exit then
  \ Now do interleaved +! with different values
  0 counter !
  100 0 do
    7 counter +!
    -3 counter +!
  loop
  \ Each iteration adds 4, so 100*4=400
  counter @ 400 = if 0 else 3 then ;
