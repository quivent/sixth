\ expect: 1 4 9 16 25
create sq 40 allot
: main
  5 0 do i 1+ dup * sq i cells + ! loop
  5 0 do sq i cells + @ . loop cr ;
