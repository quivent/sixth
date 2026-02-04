\ expected: 243590549
\ Modular exponentiation - compute base^exp mod m

: modexp ( base exp mod -- result )
  >r 1 swap              \ base result exp  R: mod
  begin dup while
    dup 1 and if
      rot dup >r         \ result exp base  R: mod base
      rot * r> swap      \ exp base result*base  R: mod
      r@ mod             \ exp base result'
      rot rot            \ result' exp base
    then
    1 rshift swap        \ result exp/2 base
    dup * r@ mod         \ result exp/2 base^2
    swap rot             \ base result exp
  repeat
  drop nip r> drop ;

: main
  0
  1000 1 do
    1000 1 do
      i j 1000000007 modexp xor
    loop
  loop
  . cr ;
