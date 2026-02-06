\ expect: 1
\ count: verify address is incremented by 1
: main
  here         \ save initial address
  dup 5 swap c! \ store length=5
  dup 1+ swap   \ ( here+1 here )
  count         \ ( here+1 addr len )
  drop          \ ( here+1 addr )
  =             \ addr should equal here+1
  if 1 else 0 then
;
