\ expect: ABCDABCD
\ ADVERSARIAL: Exact boundary move (copy to adjacent location)
\ Tests move where dst = src + length (no overlap, just touching)
\ This should work correctly with no data corruption
: main
  s" ABCD" drop here 4 move    \ copy "ABCD" to here
  here here 4 + 4 move         \ copy here to here+4 (adjacent)
  here 8 type                  \ should print "ABCDABCD"
;
