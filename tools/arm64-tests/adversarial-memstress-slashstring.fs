\ expect: 69
\ ADVERSARIAL: Chain /string operations and verify with c@
\ Tests /string pointer arithmetic
: main
  s" ABCDEFGH"    \ ( addr 8 )
  2 /string       \ ( addr+2 6 ) - now points to "CDEFGH"
  2 /string       \ ( addr+4 4 ) - now points to "EFGH"
  drop c@         \ get first char of result = 'E' = 69
;
