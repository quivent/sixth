\ expected: 500000000
\ Swap pairs, 500M times
: main
  0 1                       ( a b )
  500000000 0 do
    swap                    ( b a )
    swap                    ( a b ) back to original
    swap 1 + swap           ( a' b )
  loop
  drop . cr
;
