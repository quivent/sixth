\ expected: 500000000
\ Pick from depth 1 (over), 500M times
: main
  0 1                       ( acc dummy )
  500000000 0 do
    1 pick                  ( acc dummy acc ) same as over
    1 + nip swap            ( acc' dummy )
  loop
  drop . cr
;
