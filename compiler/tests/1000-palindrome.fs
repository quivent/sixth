\ expect: 1
: reverse-num 0 swap begin dup 0 > while swap 10 * over 10 mod + swap 10 / repeat drop ;
: palindrome? dup reverse-num = if 1 else 0 then ;
: main 12321 palindrome? . cr ;
