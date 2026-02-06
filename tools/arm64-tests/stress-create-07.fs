\ expect: 77
\ STRESS: CREATE buffer fill with boundary conditions
\ Tests: Fill exactly buffer size, verify no overrun

create fillbuf 16 allot
create sentinel 8 allot

: my-fill ( addr u c -- )
  -rot 0 do
    2dup i + c!
  loop 2drop ;

: main
  \ Put sentinel value
  12345 sentinel !
  \ Fill buffer exactly
  fillbuf 16 77 my-fill
  \ Check sentinel wasn't corrupted
  sentinel @ 12345 = if
    fillbuf c@            \ 77
  else
    0                     \ fail - sentinel corrupted
  then
;
