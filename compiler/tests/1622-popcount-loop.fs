\ expect: 4
\ 105 = 01101001 binary, popcount = 4
variable count
: main
  0 count !
  105
  begin dup 0> while
    dup 1 and count @ + count !
    1 rshift
  repeat drop
  count @ . cr ;
