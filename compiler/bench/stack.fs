\ expected: 1500000000
\ Stack manipulation stress - rot, -rot, 2swap, nip, tuck

: main
  0 1 2 500000000 0 do
    rot + swap -     \ a b c -> b c a -> b c+a -> c+a b -> c+a-b b
    over +           \ restore balance
  loop
  + + . cr ;
