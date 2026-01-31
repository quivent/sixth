\ expect: 100 200 300
create arr 24 allot
: main
  100 arr !
  200 arr 8 + !
  300 arr 16 + !
  arr @ . arr 8 + @ . arr 16 + @ . cr ;
