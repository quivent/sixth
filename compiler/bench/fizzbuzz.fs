\ expected: 266332
\ FizzBuzz benchmark - sum non-fizzbuzz numbers 1-1000, run 1000 times

: fizzbuzz-sum ( limit -- sum )
  0 swap 1+ 1 do
    i 15 mod 0= if
    else i 3 mod 0= if
    else i 5 mod 0= if
    else i + then then then
  loop ;

: main ( -- )
  0
  1000 0 do
    drop 1000 fizzbuzz-sum
  loop
  . cr ;
