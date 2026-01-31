\ expect: 3
\ Count occurrences of 'l' (108) in "Hello World"
create buf 12 allot
: main
  72 buf c!             \ H
  101 buf 1+ c!         \ e
  108 buf 2+ c!         \ l
  108 buf 3 + c!        \ l
  111 buf 4 + c!        \ o
  32 buf 5 + c!         \ space
  87 buf 6 + c!         \ W
  111 buf 7 + c!        \ o
  114 buf 8 + c!        \ r
  108 buf 9 + c!        \ l
  100 buf 10 + c!       \ d
  0
  11 0 do
    buf i + c@ 108 = if 1+ then
  loop
  . cr ;
