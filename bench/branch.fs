\ branch.fs - Alternating branches (10M iterations)
: main ( -- )
  0 10000000 0 do
    i 1 and if 1+ else 1- then
  loop . cr ;
