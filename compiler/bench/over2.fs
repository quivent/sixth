\ expected: 500000000
\ Over on depth 2, 500M times
: main
  0 1                       ( a b )
  500000000 0 do
    over                    ( a b a )
    1 + nip swap            ( a' b )
  loop
  drop . cr
;
