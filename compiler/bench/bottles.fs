\ expected: 4851
\ 99 bottles benchmark - sum bottle numbers, run 10000 times

: bottles-sum ( -- sum )
  0
  99 1 do
    i +
  loop ;

: main ( -- )
  0
  10000 0 do
    drop bottles-sum
  loop
  . cr ;
