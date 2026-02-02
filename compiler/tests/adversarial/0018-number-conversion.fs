\ Adversarial Test 0018: Number Conversion
\ Base handling. Parsing. Formatting.

: test-decimal ( -- )
  decimal base @ 10 = if ." PASS" else ." FAIL" then cr ;

: test-hex ( -- )
  hex base @ 16 = decimal if ." PASS" else ." FAIL" then cr ;

: test-binary ( -- )
  2 base ! base @ 2 = decimal if ." PASS" else ." FAIL" then cr ;

: test-hold ( -- )
  <# 65 hold 0 0 #> 1 = swap c@ 65 = and if ." PASS" else ." FAIL" then cr ;

: test-number-format ( -- )
  <# 123 0 #s #> s" 123" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-sign ( -- )
  <# -5 dup abs 0 #s rot sign #>
  2 = swap dup c@ [char] - = swap 1+ c@ [char] 5 = and and
  if ." PASS" else ." FAIL" then cr ;

: test-sharp ( -- )
  <# 42 0 # #> 1 = swap c@ [char] 2 = and if ." PASS" else ." FAIL" then cr ;

: test-sharp-s ( -- )
  <# 123 0 #s #> 3 = if ." PASS" else ." FAIL" then cr ;

." 0018-number-conversion: " cr
." decimal:   " test-decimal
." hex:       " test-hex
." binary:    " test-binary
." hold:      " test-hold
." #s:        " test-sharp-s

\ REMINDER: Number conversion is base-sensitive. Pictured numeric output builds right-to-left.
