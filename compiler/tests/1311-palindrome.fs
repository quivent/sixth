\ expect: 1 0
: reverse ( n -- rev )
  0 swap
  begin dup 0> while
    swap 10 * over 10 mod + swap
    10 /
  repeat drop ;
: palindrome? ( n -- flag ) dup reverse = if 1 else 0 then ;
: main 12321 palindrome? . 12345 palindrome? . cr ;
