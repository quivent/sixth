\ expect: 1 4 9 16 25
\ Producer fills buffer with 1-5, consumer squares and prints
create ring 64 allot
variable wp
variable rp
: produce ( val -- ) wp @ cells ring + ! wp @ 1+ wp ! ;
: consume ( -- val ) rp @ cells ring + @ rp @ 1+ rp ! ;
: main
  0 wp !  0 rp !
  \ produce 1..5
  5 0 do i 1+ produce loop
  \ consume and square
  5 0 do
    consume dup * .
  loop cr ;
