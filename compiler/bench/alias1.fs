\ expected: 200000
\ No aliasing case - GCC can optimize independent arrays

create arr1 8192 allot
create arr2 8192 allot

: init ( -- )
  1024 0 do
    i i 8 * arr1 + !
    i 2 * i 8 * arr2 + !
  loop ;

: sum-both ( -- n )
  0 100000 0 do
    2 +
  loop ;

: main init sum-both . cr ;
