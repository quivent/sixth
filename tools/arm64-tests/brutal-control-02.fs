\ expect: 0
\ Test: BEGIN/WHILE/REPEAT with nested IF and stack effects

: count-bits ( n -- count )
  0 swap                    \ count n
  begin
    dup 0 >
  while
    dup 1 and if
      swap 1+ swap          \ increment count
    then
    1 rshift                \ n = n >> 1
  repeat
  drop                      \ drop the 0
;

: main
  0 count-bits 0 <> if 1 then
  1 count-bits 1 <> if 2 then
  7 count-bits 3 <> if 3 then
  255 count-bits 8 <> if 4 then
  256 count-bits 1 <> if 5 then
  0
;
