\ branch.fs - Alternating branches (100M iterations)
: main ( -- )
  0 100000000 0 do
    i 1 and if 1+ else 1- then
  loop . cr ;
