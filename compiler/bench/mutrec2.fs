\ expected: 10000000
\ 2 mutually recursive functions, 10M total calls
\ f calls g, g calls f, alternating down to 0

: f2 ( n -- result ) recursive
  dup 0= if exit then
  1- g2 1+ ;

: g2 ( n -- result ) recursive
  dup 0= if exit then
  1- f2 1+ ;

: main
  0 100000 0 do
    100 f2 +
  loop . cr ;
