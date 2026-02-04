\ expected: 200000000
\ Over on depth 4, 200M times
: main
  0 1 2 3                   ( a b c d )
  200000000 0 do
    >r >r over r> r>        ( a b a c d ) over from depth 4
    drop nip swap >r swap r> ( a' b c d )
    >r >r >r 1 + r> r> r>
  loop
  drop drop drop . cr
;
