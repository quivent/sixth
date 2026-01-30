\ test_logic_25.fs - chained comparisons
: main : between ( n lo hi -- flag ) over <= swap rot >= and ;5 3 7 between . cr ;
