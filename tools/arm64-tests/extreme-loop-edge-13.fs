\ expect: 42
\ Edge case: UNLOOP inside conditional (verify rstack cleanup)
\ Tests that UNLOOP correctly removes loop params mid-iteration
: helper ( -- n )
  10 0 do
    i 3 = if
      42
      unloop
      exit
    then
  loop
  99
;
: main helper ;
\ When i=3, we unloop (remove 2 cells from rstack) and exit with 42
\ Without proper unloop, exit would return to wrong address
