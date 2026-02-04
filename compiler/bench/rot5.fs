\ expected: 200000000
\ Rot equivalent on 5 items, 200M times
: main
  0 1 2 3 4                 ( a b c d e )
  200000000 0 do
    >r >r rot r> r>         ( b c a d e ) rotate bottom 3
    >r >r rot r> r>         ( c a b d e )
    >r >r rot r> r>         ( a b c d e ) back to original
    >r >r >r >r 1 + r> r> r> r> ( a' b c d e )
  loop
  drop drop drop drop . cr
;
