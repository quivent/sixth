\ expected: 166666668
\ +loop stress - non-unit stride

: main
  0 1000000000 0 do
    1+ i 3 +loop . cr ;
