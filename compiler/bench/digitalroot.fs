\ expected: 499996
\ Digital root benchmark - sum digital roots 1-100000, run 100 times

: digit-sum ( n -- sum )
  0 swap
  begin dup while
    10 /mod swap rot + swap
  repeat drop ;

: digital-root ( n -- root )
  begin dup 9 > while digit-sum repeat ;

: sum-roots ( limit -- sum )
  0 swap 1+ 1 do
    i digital-root +
  loop ;

: main ( -- )
  0
  100 0 do
    drop 100000 sum-roots
  loop
  . cr ;
