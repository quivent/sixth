\ expected: 1286500000
\ GCD of 10M pairs
\ Checksum: sum of all GCDs

: gcd ( a b -- g )
  begin dup while swap over mod repeat drop ;

: bench-gcdloop ( -- sum )
  0
  10000000 0 do
    i 1000 mod 1+
    i 500 mod 1+
    gcd +
  loop ;

: main bench-gcdloop . cr ;
