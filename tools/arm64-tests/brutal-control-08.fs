\ expect: 0
\ Test: UNLOOP with nested DO/LOOP and early exit

: find-divisor ( n -- divisor | 0 )
  \ Find first divisor > 1 and < n, or 0 if prime
  dup 2 < if drop 0 exit then
  dup 2 = if drop 0 exit then
  dup                       \ n n
  2 do                      \ check 2 to n-1
    dup i mod 0= if
      drop i
      unloop exit           \ found divisor, clean up and exit
    then
  loop
  drop 0                    \ no divisor found (prime)
;

: main
  2 find-divisor 0 <> if 1 then    \ 2 is prime
  3 find-divisor 0 <> if 2 then    \ 3 is prime
  4 find-divisor 2 <> if 3 then    \ 4 = 2*2
  15 find-divisor 3 <> if 4 then   \ 15 = 3*5
  17 find-divisor 0 <> if 5 then   \ 17 is prime
  100 find-divisor 2 <> if 6 then  \ 100 = 2*50
  0
;
