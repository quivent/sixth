\ expect: olleH
\ Print string reversed char by char
create buf 8 allot
: main
  72 buf c!           \ H
  101 buf 1+ c!       \ e
  108 buf 2+ c!       \ l
  108 buf 3 + c!      \ l
  111 buf 4 + c!      \ o
  5 0 do
    4 i - buf + c@ emit
  loop cr ;
