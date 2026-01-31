\ run.fs - Fifth native compiler test runner
\ Usage: fifth compiler/tests/run.fs

require lib/str.fs

: 0<> ( n -- flag ) 0= 0= ;

\ Counters
variable pass    0 pass !
variable cfail   0 cfail !
variable rfail   0 rfail !
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
\ Uses variable because do/loop reserves the return stack
variable last-slash
: path>name ( addr u -- name-addr name-u )
  0 last-slash !
  over swap
  0 do
    dup i + c@ [char] / = if i 1+ last-slash ! then
  loop
  drop last-slash @ +
  \ Find dot, with bounds safety: scan at most 64 chars
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

\ Run one test file
: run-test ( path-addr path-u -- )
  1 total +!
  2dup path>name stash-name
  stash-path

  clock-ms >r

  \ Build + run compile command
  str-reset
  s" ./fifth compiler/tf.fs " str+
  path$ str+
  s"  /tmp/t-" str+
  stash$ str+
  s"  >/dev/null 2>&1" str+

  str$ system-rc
  0<> if
    clock-ms r> - total-ms +!
    1 cfail +!
    [char] C emit
    exit
  then

  \ Build + run execution command
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
    1 pass +!
    [char] . emit
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

  \ Process each line
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
  ." CFAIL: " cfail @ .
  ." RFAIL: " rfail @ .
  cr ." Time: " total-ms @ . ." ms" cr ;

run-all bye
