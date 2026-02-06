\ expect: TEST
\ ADVERSARIAL: Move to/from here pointer area
\ Tests that move works correctly with the heap area managed by here
: main
  s" TEST" drop here 4 move    \ copy "TEST" to here
  here 4 type                  \ print TEST
;
