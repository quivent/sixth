\ expected: 198
\ Palindrome benchmark - count palindromic numbers 1-9999, run 200 times

create numbuf 12 allot

: num>str ( n -- addr len )
  numbuf 11 + dup >r
  begin
    swap 10 /mod swap [char] 0 + rot 1- tuck c!
    swap dup 0=
  until drop
  r> over - ;

: palindrome? ( addr len -- flag )
  2dup + 1- swap
  begin 2dup < while
    over c@ over c@ <> if 2drop 2drop 0 exit then
    swap 1+ swap 1-
  repeat
  2drop 2drop -1 ;

: num-palindrome? ( n -- flag )
  num>str palindrome? ;

: count-palindromes ( limit -- count )
  0 swap 1 do
    i num-palindrome? if 1+ then
  loop ;

: main ( -- )
  0
  200 0 do
    drop 10000 count-palindromes
  loop
  . cr ;
