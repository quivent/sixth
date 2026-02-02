\ Adversarial Test 0017: Parsing Words
\ Words that consume input. Tricky to implement.

: test-parse-name ( -- )
  s" hello" parse-name s" hello" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-parse-char ( -- )
  s" hello|world" [char] | parse
  s" hello" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-word ( -- )
  bl word count s" test" compare 0= if ." PASS" else ." FAIL" then cr ;

test

: test-source ( -- )
  source nip 0> if ." PASS" else ." FAIL" then cr ;

: test-to-in ( -- )
  >in @ 0>= if ." PASS" else ." FAIL" then cr ;

." 0017-parsing: " cr
." source:    " test-source
." >in:       " test-to-in

\ REMINDER: Parsing words read from the input stream. Stateful and subtle.
