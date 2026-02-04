\ expected: 300000000
\ 3-way rotate via swaps, 300M times
: main
  0 1 2                     ( a b c )
  300000000 0 do
    swap >r swap r>         ( c a b ) rot via swaps
    swap >r swap r>         ( b c a )
    swap >r swap r>         ( a b c ) back to original
    >r >r 1 + r> r>         ( a' b c )
  loop
  2drop . cr
;
