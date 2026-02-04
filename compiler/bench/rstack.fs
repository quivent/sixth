\ expected: 499999999500000000
\ Return stack stress - >r r> r@ in hot loop

: main
  0 1000000000 0 do
    i >r r@ + r> drop
  loop . cr ;
