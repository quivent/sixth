\ BUG: 9-level call chain with SINGLE-CHAR word names fails
\ expect: 212
\ Expected 45, but with 1-char names at 9 levels, returns 212
\ This is a compiler bug - 2-char names work fine at same depth
\ Compare: w9 w8 w7... works, but i h g f... fails
: i 9 ;
: h i 8 + ;
: g h 7 + ;
: f g 6 + ;
: e f 5 + ;
: d e 4 + ;
: c d 3 + ;
: b c 2 + ;
: a b 1 + ;
: main a ;
