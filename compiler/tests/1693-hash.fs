\ expect: 107
\ Simple hash function on "Hello" (72,101,108,108,111)
\ hash = (hash*31 + c) mod 997
create buf 5 allot
variable hval
: main
  72 buf c!  101 buf 1+ c!  108 buf 2+ c!  108 buf 3 + c!  111 buf 4 + c!
  0 hval !
  5 0 do
    hval @ 31 * buf i + c@ + 997 mod hval !
  loop
  hval @ . cr ;
