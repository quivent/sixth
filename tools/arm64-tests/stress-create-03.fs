\ expect: 65
\ STRESS: CREATE followed by immediate use with zero-size allot
\ Tests: What happens with create ... 0 allot? Then use?

create zero 0 allot
create real 8 allot

: main
  65 real !
  \ zero buffer has no space - but what does 'zero' point to?
  \ Don't write to it - just verify 'real' still works
  real @
;
