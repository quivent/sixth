\ expect: ABCABC
\ Outer loop 2 iters, inner loop 3 iters, emit A+inner_index
variable rounds
: main
  2 rounds !
  rounds @ 0 do
    3 0 do 65 i + emit loop
  loop cr ;
