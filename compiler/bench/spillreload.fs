\ expected: 200000000
\ Force spill then reload, 200M times
: main
  0 1 2 3 4 5 6 7           ( a b c d e f g h ) 8 items forces spills
  200000000 0 do
    \ Access deeply spilled value
    7 pick                  ( a b c d e f g h a )
    1 +
    \ Store back
    >r drop drop drop drop drop drop drop r>
    1 2 3 4 5 6 7           ( a' b c d e f g h )
  loop
  drop drop drop drop drop drop drop . cr
;
