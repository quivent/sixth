\ expect: 1
: sum-divisors ( n -- sum ) 0 over 2 / 1+ 1 do over i mod 0= if i + then loop nip ;
: perfect? ( n -- flag ) dup sum-divisors = if 1 else 0 then ;
: main 28 perfect? . cr ;
