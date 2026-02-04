\ expected: 500000000
\ Rot on 3 items, 500M times
: main
  0 1 2                     ( a b c )
  500000000 0 do
    rot                     ( b c a )
    rot                     ( c a b )
    rot                     ( a b c ) back to original
    >r >r 1 + r> r>         ( a' b c )
  loop
  2drop . cr
;
