\ expect: 10 20 30 40 50
\ cell+ in a do/loop for array traversal
create arr 40 allot
: main
  10 arr !
  20 arr 8 + !
  30 arr 16 + !
  40 arr 24 + !
  50 arr 32 + !
  arr 5 0 do dup @ . cell+ loop drop cr ;
