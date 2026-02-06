\ expect: HEL
\ ADVERSARIAL: Same source and destination
\ Tests move with src == dst (no-op effectively, but should not crash)
\ This is a valid edge case that should preserve the data
: main
  s" HELLO" drop here 5 move   \ copy HELLO to here
  here here 3 move             \ move here to itself
  here 3 type                  \ should still be "HEL"
;
