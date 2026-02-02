\ validate.fs - Enforce test policy: no standard Forth tests
\ Usage: sixth compiler/tests/validate.fs [test-file]
\ Exit 1 if test violates policy, 0 if ok

require lib/str.fs

\ Required categories (tests MUST have one)
: valid-category? ( addr u -- flag )
  2dup s" codegen" str= if 2drop -1 exit then
  2dup s" optimization" str= if 2drop -1 exit then
  2dup s" fold" str= if 2drop -1 exit then
  2dup s" fuse" str= if 2drop -1 exit then
  2dup s" fwd-ref" str= if 2drop -1 exit then
  2dup s" regression" str= if 2drop -1 exit then
  2dup s" sixth-word" str= if 2drop -1 exit then
  2dup s" integration" str= if 2drop -1 exit then
  2drop 0 ;

\ Forbidden patterns - these suggest testing standard Forth
create forbidden-patterns
  here s" test addition" s, s,
  here s" test subtraction" s, s,
  here s" test multiplication" s, s,
  here s" test division" s, s,
  here s" test loop" s, s,
  here s" test if" s, s,
  here s" test do" s, s,
  here s" test begin" s, s,
  here s" test while" s, s,
  here s" test until" s, s,
  here s" test case" s, s,
  here s" test recurse" s, s,
  here s" test stack" s, s,
  0 ,

variable test-file-contents
variable test-file-len
variable has-category
variable has-reason
variable line-num

create line-buf 256 allot
variable line-len

: lowercase ( c -- c' ) dup [char] A [char] Z 1+ within if 32 + then ;

: str-contains-ci? ( haystack-addr haystack-u needle-addr needle-u -- flag )
  \ Case-insensitive substring search
  2swap 2dup + >r
  begin
    2dup r@ < while
    over c@ lowercase 3 pick c@ lowercase = if
      \ potential match - check rest
      2over nip >r  \ needle len on rstack
      2dup drop r>
      2over 2>r
      0 begin
        dup 5 pick < while
        over 4 pick + c@ lowercase
        3 pick 3 pick + c@ lowercase
        <> if drop -1 swap then
        1+
      repeat
      2r> 2drop
      swap 0< 0= if
        2drop 2drop r> drop -1 exit
      then
      drop
    then
    1 /string
  repeat
  2drop 2drop r> drop 0 ;

: check-forbidden ( addr u -- flag )
  \ Returns true if contains forbidden pattern
  \ TODO: implement pattern matching
  2drop 0 ;

: validate-test ( path-addr path-u -- exit-code )
  2dup slurp-file
  dup 0= if 2drop ." Cannot read file: " type cr 1 exit then

  0 has-category !
  0 has-reason !
  1 line-num !

  \ Check first 20 lines for required headers
  20 0 do
    2dup 0= if leave then
    \ find newline
    2dup 10 scan
    dup 0= if
      2drop 2dup  \ last line
    else
      2swap 2 pick -
    then
    ( remaining file | line )

    \ Check for category header: "\ category: <cat>"
    2dup s" \ category:" str-contains-ci? if
      \ Extract category after colon
      2dup [char] : scan 1 /string  \ skip colon
      begin dup 0> while over c@ 32 = while 1 /string repeat then  \ skip spaces
      dup 0> if
        2dup 32 scan drop over -  \ get word
        valid-category? has-category !
      then
    then

    \ Check for reason header: "\ reason:" or "\ bug:" or "\ regression:"
    2dup s" \ reason:" str-contains-ci? if -1 has-reason ! then
    2dup s" \ bug:" str-contains-ci? if -1 has-reason ! then
    2dup s" \ regression:" str-contains-ci? if -1 has-reason ! then
    2dup s" \ optimization:" str-contains-ci? if -1 has-reason ! then

    2drop
    \ Advance past newline
    dup 0> if 1 /string then
    1 line-num +!
  loop
  2drop

  has-category @ 0= if
    ." POLICY VIOLATION: Missing '\ category:' header" cr
    ." Valid categories: codegen optimization fold fuse fwd-ref regression sixth-word integration" cr
    ." " cr
    ." Hayes tests cover standard Forth. Your test must specify what compiler-specific" cr
    ." behavior it tests. Add a header like:" cr
    ." " cr
    ."   \ category: optimization" cr
    ."   \ reason: Verify constant folding eliminates runtime addition" cr
    ." " cr
    2drop 1 exit
  then

  has-reason @ 0= if
    ." POLICY VIOLATION: Missing reason header" cr
    ." Add '\ reason:', '\ bug:', or '\ regression:' explaining WHY this test exists." cr
    ." " cr
    ." If Hayes tests already cover this behavior, do not write the test." cr
    2drop 1 exit
  then

  2drop 0 ;

: main
  argc 2 < if
    ." Usage: sixth compiler/tests/validate.fs <test-file>" cr
    ." Validates that a test follows the test policy." cr
    1 bye
  then

  1 argv validate-test bye ;

main
