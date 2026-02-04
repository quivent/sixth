\ regress.fs - Fast sixth.fs regression test
\ Compiles each test with sixth.fs, runs it, compares output to expected.
\ No GCC. No timing. Just correctness.
\ Usage: ./engine/fifth compiler/regress.fs
\ ~10 seconds for 1050 tests.

require lib/str.fs

: 0<> ( n -- flag ) 0= 0= ;

\ Counters
variable pass   0 pass !
variable fail   0 fail !
variable skip   0 skip !
variable total  0 total !

\ Buffers
create expect-buf 256 allot   variable expect-len
create actual-buf 4096 allot  variable actual-len
create path-buf 256 allot     variable path-len
create name-buf 64 allot      variable name-len

65536 constant LIST-MAX
create list-buf LIST-MAX allot
variable list-len

\ Strip trailing whitespace
: strip ( addr u -- addr u' )
  begin dup 0> while
    2dup + 1- c@ dup 32 = over 10 = or over 13 = or over 9 = or nip while
    1-
  repeat then ;

\ Parse "\ expect: XYZ" from first line of file
: parse-expect ( addr u -- ok? )
  0 expect-len !
  dup 10 < if 2drop false exit then
  over c@ [char] \ <> if 2drop false exit then
  \ Walk to find ':'
  0 begin 2dup > while
    2 pick over + c@ [char] : = if
      \ Found colon at offset 'over'. Skip past ": "
      1+ /string
      dup 0> if over c@ 32 = if 1 /string then then
      \ Copy chars to newline into expect-buf
      0 begin
        2dup > while
        2 pick over + c@ dup 10 = over 13 = or if
          drop expect-len ! 2drop true exit
        then
        expect-buf 2 pick + c!
        1+
      repeat
      dup expect-len ! 2drop
      expect-len @ 0> exit
    then
    1+
  repeat 2drop false ;

\ Stash path and extract name
variable last-slash
: stash-path ( addr u -- )
  dup path-len ! path-buf swap move ;
: path$ ( -- addr u ) path-buf path-len @ ;

: path>name ( addr u -- )
  0 last-slash !
  0 begin 2dup > while
    2 pick over + c@ [char] / = if dup 1+ last-slash ! then
    1+
  repeat drop
  drop last-slash @ + dup
  begin dup c@ [char] . <> over c@ 0<> and while 1+ repeat
  over - dup name-len ! name-buf swap move ;

\ Find newline
: find-nl ( addr u -- pos )
  0 begin 2dup > while
    2 pick over + c@ 10 = if nip nip exit then
    1+
  repeat nip nip ;

: run-test ( addr u -- )
  1 total +!
  2dup stash-path
  2dup path>name

  \ Read source, parse expected output
  slurp-file
  dup 0= if 2drop 1 skip +! exit then
  parse-expect 0= if 1 skip +! exit then

  \ Compile with sixth.fs
  str-reset
  s" ./engine/fifth compiler/sixth.fs " str+
  path$ str+
  s"  /tmp/_regress >/dev/null 2>&1" str+
  str$ system-rc
  0<> if 1 skip +! exit then

  \ Run
  str-reset
  s" timeout 2 /tmp/_regress > /tmp/_regress_out 2>&1" str+
  str$ system-rc
  0<> if 1 fail +! [char] F emit exit then

  \ Read actual output
  s" /tmp/_regress_out" slurp-file strip
  dup actual-len ! actual-buf swap move

  \ Compare
  actual-buf actual-len @
  expect-buf expect-len @
  str= if
    1 pass +! [char] . emit
  else
    1 fail +!
    [char] X emit
    name-buf name-len @ type
    [char] ( emit
    s" exp=" type expect-buf expect-len @ type
    s"  got=" type actual-buf actual-len @ type
    [char] ) emit
  then ;

: run-all ( -- )
  s" ls compiler/tests/[0-9]*.fs > /tmp/_regress_list.txt 2>/dev/null" system
  s" /tmp/_regress_list.txt" slurp-file
  dup LIST-MAX < 0= if 2drop ." Too many" cr exit then
  dup 0= if 2drop ." No tests" cr exit then
  dup list-len ! list-buf swap move
  list-buf list-len @
  begin dup 0> while
    2dup find-nl >r
    over r@
    dup 3 > if run-test else 2drop then
    r> 1+ /string
  repeat 2drop ;

variable t0
: main ( -- )
  clock-ms t0 !
  run-all
  cr
  ." Pass: " pass @ . ." / " total @ . cr
  ." Fail: " fail @ . cr
  ." Skip: " skip @ . cr
  ." Time: " clock-ms t0 @ - . ." ms" cr ;

main bye
