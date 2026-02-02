\ Adversarial Test 0011: String Operations
\ Counted strings. Slicing. Comparison.

: test-count ( -- )
  s" hello" 5 = swap c@ [char] h = and if ." PASS" else ." FAIL" then cr ;

: test-char ( -- )
  [char] A 65 = if ." PASS" else ." FAIL" then cr ;

: test-compare-equal ( -- )
  s" abc" s" abc" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-compare-less ( -- )
  s" abc" s" abd" compare 0< if ." PASS" else ." FAIL" then cr ;

: test-compare-greater ( -- )
  s" abd" s" abc" compare 0> if ." PASS" else ." FAIL" then cr ;

: test-compare-length ( -- )
  s" ab" s" abc" compare 0< if ." PASS" else ." FAIL" then cr ;

: test-slash-string ( -- )
  s" hello" 2 /string 3 = swap c@ [char] l = and if ." PASS" else ." FAIL" then cr ;

: test-blank ( -- )
  here 5 2dup bl fill + 1- c@ 32 = if ." PASS" else ." FAIL" then cr ;

: test-move ( -- )
  s" abc" drop here 3 move here c@ [char] a = if ." PASS" else ." FAIL" then cr ;

: test-cmove ( -- )
  s" abc" drop here 3 cmove here 2 + c@ [char] c = if ." PASS" else ." FAIL" then cr ;

." 0011-string-ops: " cr
." s\" len:    " test-count
." [char]:    " test-char
." cmp =:     " test-compare-equal
." cmp <:     " test-compare-less
." cmp >:     " test-compare-greater
." cmp len:   " test-compare-length
." /string:   " test-slash-string
." blank:     " test-blank
." move:      " test-move
." cmove:     " test-cmove

\ REMINDER: String ops are address arithmetic. Off-by-one errors everywhere.
