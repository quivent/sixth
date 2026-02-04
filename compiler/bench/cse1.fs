\ expected: 1100000000
\ Common subexpression elimination - repeated computation

: compute ( a b -- result )
  over over + >r   \ a+b saved
  over over * >r   \ a*b saved
  2drop
  r> r> + ;        \ (a*b) + (a+b)

: main
  0 100000000 0 do
    2 3 compute +
  loop . cr ;
