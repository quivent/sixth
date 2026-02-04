\ expected: 1
\ Even/odd check - iterative

: main
  10000000 2 mod 0=
  if 1 else 0 then . cr ;
