\ expected: 300000000
\ Over on depth 3, 300M times
: main
  0 1 2                     ( a b c )
  300000000 0 do
    >r over r>              ( a b a c ) over from depth 3
    drop nip swap           ( a' b c )
    >r >r 1 + r> r>
  loop
  2drop . cr
;
