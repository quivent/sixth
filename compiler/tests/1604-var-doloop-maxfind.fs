\ expect: 99
create arr 40 allot
variable best
: main
  23 arr 0 cells + !
  99 arr 1 cells + !
  7 arr 2 cells + !
  45 arr 3 cells + !
  12 arr 4 cells + !
  arr @ best !
  5 1 do
    arr i cells + @ dup best @ > if best ! else drop then
  loop
  best @ . cr ;
