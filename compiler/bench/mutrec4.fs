\ expected: 2000000
\ 4 mutually recursive functions, 2M total calls
\ f->g->h->i->f chain

: f4 ( n -- result ) recursive
  dup 0= if exit then
  1- g4 1+ ;

: g4 ( n -- result ) recursive
  dup 0= if exit then
  1- h4 1+ ;

: h4 ( n -- result ) recursive
  dup 0= if exit then
  1- i4 1+ ;

: i4 ( n -- result ) recursive
  dup 0= if exit then
  1- f4 1+ ;

: main
  0 20000 0 do
    100 f4 +
  loop . cr ;
