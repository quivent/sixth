\ run.fs - Fifth native compiler test runner (strict output comparison)
\ Usage: fifth compiler/tests/run.fs

require lib/str.fs

: 0<> ( n -- flag ) 0= 0= ;

\ Counters
variable pass    0 pass !
variable cfail   0 cfail !
variable rfail   0 rfail !
variable wrong   0 wrong !
variable skip    0 skip !
variable total   0 total !
variable total-ms  0 total-ms !

\ Find newline position in string
: find-nl ( addr u -- pos )
  0 begin
    2dup > while
    2 pick over + c@ 10 = if nip nip exit then
    1+
  repeat
  nip nip ;

\ Extract test name from path like "compiler/tests/123-foo.fs"
variable last-slash
: path>name ( addr u -- name-addr name-u )
  0 last-slash !
  over swap
  0 do
    dup i + c@ [char] / = if i 1+ last-slash ! then
  loop
  drop last-slash @ +
  dup 64 + >r
  dup begin dup r@ < while dup c@ [char] . <> while 1+ repeat then
  r> drop
  over - ;

\ Stable copies for command building
create name-stash 64 allot
variable name-stash-len
create path-stash 256 allot
variable path-stash-len

: stash-name ( addr u -- ) dup name-stash-len ! name-stash swap move ;
: stash$ ( -- addr u ) name-stash name-stash-len @ ;
: stash-path ( addr u -- ) dup path-stash-len ! path-stash swap move ;
: path$ ( -- addr u ) path-stash path-stash-len @ ;

\ Expected output buffer
create expect-buf 512 allot
variable expect-len

\ Actual output buffer
create actual-buf 4096 allot
variable actual-len

\ Wrong test name log (for summary)
create wrong-log 8192 allot
variable wrong-log-len

: wrong-log+ ( addr u -- )
  wrong-log-len @ 8000 > if 2drop exit then
  wrong-log wrong-log-len @ + over >r swap move r>
  wrong-log-len +!
  10 wrong-log wrong-log-len @ + c!
  1 wrong-log-len +! ;

\ Strip trailing whitespace
: strip-ws ( addr u -- addr u' )
  begin dup 0> while
    2dup + 1- c@
    dup 32 = over 10 = or over 13 = or over 9 = or
    nip while
    1-
  repeat then ;

\ Extract expect line via shell
: get-expect ( -- flag )
  0 expect-len !
  str-reset
  s" head -1 " str+
  path$ str+
  s"  > /tmp/t-firstline" str+
  str$ system
  s" /tmp/t-firstline" slurp-file
  dup 10 < if 2drop 0 exit then
  over c@ [char] \ <> if 2drop 0 exit then
  over 1+ c@ 32 <> if 2drop 0 exit then
  over 2 + c@ [char] e <> if 2drop 0 exit then
  10 /string
  2dup find-nl
  dup 0= if
    drop dup expect-len ! expect-buf swap move -1 exit
  then
  >r drop r>
  dup 512 > if drop 512 then
  dup expect-len !
  expect-buf swap move -1 ;

\ Read actual output (already normalized by shell pipeline)
: get-actual ( -- )
  0 actual-len !
  s" /tmp/t-out" slurp-file
  dup 4096 > if 2drop exit then
  strip-ws
  dup actual-len !
  actual-buf swap move ;

\ Run one test file
: run-test ( path-addr path-u -- )
  1 total +!
  2dup path>name stash-name
  stash-path

  clock-ms >r

  get-expect

  \ Compile
  str-reset
  s" ./fifth compiler/tf.fs " str+
  path$ str+
  s"  /tmp/t-" str+
  stash$ str+
  s"  >/dev/null 2>&1" str+

  str$ system-rc
  0<> if
    drop
    clock-ms r> - total-ms +!
    1 cfail +!
    [char] C emit
    exit
  then

  if
    \ Run with output capture, normalize newlines to spaces
    str-reset
    s" timeout 2 /tmp/t-" str+
    stash$ str+
    s"  2>&1 | tr '\n' ' ' | tr -s ' ' > /tmp/t-out" str+

    str$ system-rc
    clock-ms r> - total-ms +!

    0<> if
      1 rfail +!
      [char] R emit
      exit
    then

    get-actual
    actual-buf actual-len @
    expect-buf expect-len @
    str= if
      1 pass +!
      [char] . emit
    else
      1 wrong +!
      [char] W emit
      stash$ wrong-log+
    then
  else
    \ No expect line - just check exit code
    str-reset
    s" timeout 2 /tmp/t-" str+
    stash$ str+
    s"  >/dev/null 2>&1" str+

    str$ system-rc
    clock-ms r> - total-ms +!

    0<> if
      1 rfail +!
      [char] R emit
    else
      1 skip +!
      [char] - emit
    then
  then ;

\ Stable buffer for file list
65536 constant LIST-MAX
create list-buf LIST-MAX allot
variable list-len

\ Count newlines in buffer
: count-lines ( addr u -- n )
  0 -rot
  begin dup 0> while
    over c@ 10 = if rot 1+ -rot then
    1 /string
  repeat 2drop ;

\ Main
: run-all ( -- )
  s" ls compiler/tests/[0-9]*.fs > /tmp/fifth-test-list.txt 2>/dev/null" system
  s" /tmp/fifth-test-list.txt" slurp-file
  dup LIST-MAX < 0= if
    2drop ." Test list too large for buffer" cr exit
  then
  dup 0= if 2drop ." No test files found" cr exit then
  dup list-len ! list-buf swap move
  list-buf list-len @

  2dup count-lines
  ." Running " . ." tests" cr

  begin
    dup 0> while
    2dup find-nl >r
    over r@
    dup 3 > if run-test else 2drop then
    r> 1+ /string
  repeat
  2drop

  cr cr
  ." TOTAL: " total @ .
  ." PASS: " pass @ .
  ." WRONG: " wrong @ .
  ." CFAIL: " cfail @ .
  ." RFAIL: " rfail @ .
  ." SKIP: " skip @ .
  cr ." Time: " total-ms @ . ." ms" cr

  wrong @ 0> if
    cr ." Wrong output:" cr
    wrong-log wrong-log-len @ type
  then ;

run-all bye
