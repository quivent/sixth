\ expect: 0
\ Test: Word name lengths - from 1 char to 15 chars
\ Each word returns its name length

: a ( -- n ) 1 ;
: ab ( -- n ) 2 ;
: abc ( -- n ) 3 ;
: abcd ( -- n ) 4 ;
: abcde ( -- n ) 5 ;
: abcdef ( -- n ) 6 ;
: abcdefg ( -- n ) 7 ;
: abcdefgh ( -- n ) 8 ;
: abcdefghi ( -- n ) 9 ;
: abcdefghij ( -- n ) 10 ;
: abcdefghijk ( -- n ) 11 ;
: abcdefghijkl ( -- n ) 12 ;
: abcdefghijklm ( -- n ) 13 ;
: abcdefghijklmn ( -- n ) 14 ;
: abcdefghijklmno ( -- n ) 15 ;

: main
  a ab + abc + abcd + abcde +
  abcdef + abcdefg + abcdefgh + abcdefghi +
  abcdefghij + abcdefghijk + abcdefghijkl +
  abcdefghijklm + abcdefghijklmn + abcdefghijklmno +
  120 - ;
