\ expected: 5000000
\ 3 mutually recursive functions, 5M total calls
\ f->g->h->f chain

: f3 ( n -- result ) recursive
  dup 0= if exit then
  1- g3 1+ ;

: g3 ( n -- result ) recursive
  dup 0= if exit then
  1- h3 1+ ;

: h3 ( n -- result ) recursive
  dup 0= if exit then
  1- f3 1+ ;

: main
  0 50000 0 do
    100 f3 +
  loop . cr ;
