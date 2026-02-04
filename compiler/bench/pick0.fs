\ expected: 500000000
\ Pick from depth 0 (dup), 500M times
: main
  0                         ( acc )
  500000000 0 do
    0 pick                  ( acc acc ) same as dup
    1 + nip                 ( acc' )
  loop
  . cr
;
