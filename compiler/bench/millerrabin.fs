\ expected: 9608
\ Miller-Rabin primality test - count primes up to 100000

: modexp ( base exp mod -- result )
  >r 1 swap
  begin dup while
    dup 1 and if
      rot dup >r rot * r> swap r@ mod rot rot
    then
    1 rshift swap dup * r@ mod swap rot
  repeat
  drop nip r> drop ;

: miller-test ( n d r -- pass? )
  >r                        \ n d  R: r
  2 swap 3 pick modexp      \ n x=2^d mod n  R: r
  dup 1 = if r> 2drop drop 1 exit then
  dup 2 pick 1- = if r> 2drop drop 1 exit then
  \ Square r-1 times
  r> 1 do                   \ n x
    dup * 2 pick mod        \ n x'
    dup 2 pick 1- = if 2drop 1 unloop exit then
  loop
  2drop 0 ;

: is-prime-mr ( n -- flag )
  dup 2 < if drop 0 exit then
  dup 2 = if drop 1 exit then
  dup 1 and 0= if drop 0 exit then
  \ Write n-1 = 2^r * d
  dup 1-
  0                         \ n n-1 r=0
  begin over 1 and 0= while
    swap 1 rshift swap 1+
  repeat                    \ n d r
  miller-test ;

: main
  0
  100000 2 do
    i is-prime-mr if 1+ then
  loop
  . cr ;
