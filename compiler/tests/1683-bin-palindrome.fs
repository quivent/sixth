\ expect: 1 0
\ Check if number is palindrome in binary
\ 9 = 1001 -> palindrome. 10 = 1010 -> not palindrome
variable pn variable pbits
: highbit ( n -- pos )
  0 swap begin dup 1 > while 1 rshift swap 1+ swap repeat drop ;
: bit@ ( n pos -- bit ) rshift 1 and ;
: binpal? ( n -- 0|1 )
  dup pn ! dup highbit pbits !
  pbits @ 1+ 2/ 0 do
    pn @ i bit@
    pn @ pbits @ i - bit@
    <> if 0 exit then
  loop 1 ;
: main
  9 binpal? .
  10 binpal? .
  cr ;
