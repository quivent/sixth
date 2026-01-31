\ expect: 10 20 30
\ cell+ for array traversal: store 3 values, read them back using cell+
create arr 24 allot
: main
  10 arr !
  20 arr cell+ !
  30 arr cell+ cell+ !
  arr @ .
  arr cell+ @ .
  arr cell+ cell+ @ .
  cr ;
