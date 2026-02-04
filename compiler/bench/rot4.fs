\ expected: 200000000
\ Rot equivalent on 4 items, 200M times
: main
  0 1 2 3                   ( a b c d )
  200000000 0 do
    >r rot r>               ( b c a d ) rotate bottom 3, keep d
    >r rot r>               ( c a b d )
    >r rot r>               ( a b c d ) back to original
    >r >r >r 1 + r> r> r>   ( a' b c d )
  loop
  drop drop drop . cr
;
