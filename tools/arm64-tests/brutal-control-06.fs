\ expect: 42
\ Test: EXIT from word called inside nested loops
\ Bug exposed: EXIT in called word doesn't properly propagate to caller

variable result

: check-val ( n -- n flag )
  dup 42 = if 1 else 0 then
;

: search-grid ( -- )
  100 0 do
    100 0 do
      i j +
      check-val if
        result !
        unloop unloop exit
      then
      drop
    loop
  loop
  0 result !
;

: main
  0 result !
  search-grid
  result @
;
