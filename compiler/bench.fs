\ bench.fs - Benchmark sixth.fs vs GCC at -O0/-O1/-O2/-O3
\ Ports run_benchmarks.py + forth2c.py to Forth
\ Usage: fifth compiler/bench.fs
\ Output: BENCHMARK01.md

require lib/str.fs

: 0<> ( n -- flag ) 0= 0= ;
: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;
: 0>= ( n -- flag ) 0< 0= ;

\ === Tokenizer for Forth-to-C translator ===

variable src-addr
variable src-rem

create tok-buf 128 allot
variable tok-len
: tok$ ( -- addr u ) tok-buf tok-len @ ;

: src-skip-ws ( -- )
  begin src-rem @ 0> while
    src-addr @ c@ 33 < while
    1 src-addr +! -1 src-rem +!
  repeat then ;

: src-skip-line ( -- )
  begin src-rem @ 0> while
    src-addr @ c@ 10 <> while
    1 src-addr +! -1 src-rem +!
  repeat then ;

: next-tok ( -- flag )
  begin
    src-skip-ws
    src-rem @ 0= if 0 tok-len ! false exit then
    src-addr @ c@ [char] \ = if
      src-skip-line
    else
      0 tok-len !
      begin src-rem @ 0> while src-addr @ c@ 32 > while
        src-addr @ c@ tok-buf tok-len @ + c!
        1 tok-len +!
        1 src-addr +! -1 src-rem +!
      repeat then
      tok-len @ 0> exit
    then
  again ;

: skip-paren ( -- )
  begin next-tok while tok$ s" )" str= if exit then repeat ;

\ === Word dictionary ===

64 constant MAX-DEFS
create def-names MAX-DEFS 33 * allot
variable def-count

: def-name@ ( n -- addr ) 33 * def-names + ;
: def-name$ ( n -- addr u ) def-name@ dup c@ swap 1+ swap ;

: add-def ( addr u -- )
  def-count @ MAX-DEFS >= if 2drop exit then
  dup 32 > if drop 32 then
  dup def-count @ def-name@ c!
  def-count @ def-name@ 1+ swap move
  1 def-count +! ;

: is-user-word? ( addr u -- flag )
  def-count @ 0 do
    2dup i def-name$ str= if 2drop true unloop exit then
  loop 2drop false ;

: skip-to-semi ( -- )
  begin next-tok while tok$ s" ;" str= if exit then repeat ;

variable src-save-a
variable src-save-r

: collect-words ( -- )
  0 def-count !
  src-addr @ src-save-a !
  src-rem @ src-save-r !
  begin next-tok while
    tok$ s" :" str= if
      next-tok if tok$ add-def then
      skip-to-semi
    then
  repeat
  src-save-a @ src-addr !
  src-save-r @ src-rem ! ;

\ === C output ===

variable c-fid

: c-emit ( c -- ) c-fid @ emit-file drop ;
: c-type ( addr u -- ) c-fid @ write-file drop ;
: c-nl ( -- ) 10 c-emit ;
: c-line ( addr u -- ) c-type c-nl ;

: c-emit-san ( c -- )
  dup [char] - = if drop [char] _ c-emit exit then
  dup [char] ? = if drop [char] _ c-emit [char] q c-emit exit then
  dup [char] ! = if drop [char] _ c-emit [char] x c-emit exit then
  dup [char] / = if drop [char] _ c-emit [char] d c-emit exit then
  c-emit ;

: c-type-san ( addr u -- )
  0 do dup i + c@ c-emit-san loop drop ;

34 constant DQ

\ === Number detection ===

: is-digit? ( c -- flag ) dup [char] 0 >= swap [char] 9 <= and ;

: is-forth-number? ( addr u -- flag )
  dup 0= if 2drop false exit then
  over c@ [char] $ = if
    1 /string dup 0= if 2drop false exit then
    drop c@ dup is-digit?
    over [char] a >= over [char] f <= and or
    swap dup [char] A >= swap [char] F <= and or exit
  then
  over c@ [char] - = if
    1 /string dup 0= if 2drop false exit then
    drop c@ is-digit? exit
  then
  drop c@ is-digit? ;

: emit-c-number ( addr u -- )
  s"   s[++sp] = " c-type
  over c@ [char] $ = if
    s" 0x" c-type 1 /string
  then
  c-type s" LL;" c-line ;

\ === Do-loop depth ===

variable do-depth

\ Current word name for recursion
create cur-word 64 allot
variable cur-word-len
: cur-word$ ( -- addr u ) cur-word cur-word-len @ ;

\ === Token-to-C emitter ===

: emit-c-tok ( -- )
  tok$ s" (" str= if skip-paren exit then

  \ Arithmetic
  tok$ s" +" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a+b; }" c-line exit then
  tok$ s" -" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a-b; }" c-line exit then
  tok$ s" *" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a*b; }" c-line exit then
  tok$ s" /" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a/b; }" c-line exit then
  tok$ s" mod" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a%b; }" c-line exit then
  tok$ s" /mod" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a%b; s[++sp]=a/b; }" c-line exit then
  tok$ s" and" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a&b; }" c-line exit then
  tok$ s" or" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a|b; }" c-line exit then
  tok$ s" xor" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=a^b; }" c-line exit then
  tok$ s" invert" str= if s"   s[sp] = ~s[sp];" c-line exit then
  tok$ s" negate" str= if s"   s[sp] = -s[sp];" c-line exit then
  tok$ s" abs" str= if s"   s[sp] = s[sp]<0 ? -s[sp] : s[sp];" c-line exit then
  tok$ s" 1+" str= if s"   s[sp]++;" c-line exit then
  tok$ s" 1-" str= if s"   s[sp]--;" c-line exit then
  tok$ s" 2+" str= if s"   s[sp]+=2;" c-line exit then
  tok$ s" 2-" str= if s"   s[sp]-=2;" c-line exit then
  tok$ s" 2*" str= if s"   s[sp]*=2;" c-line exit then
  tok$ s" 2/" str= if s"   s[sp]/=2;" c-line exit then

  \ Comparison
  tok$ s" <" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=(a<b)?-1:0; }" c-line exit then
  tok$ s" >" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=(a>b)?-1:0; }" c-line exit then
  tok$ s" =" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=(a==b)?-1:0; }" c-line exit then
  tok$ s" <>" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=(a!=b)?-1:0; }" c-line exit then
  tok$ s" 0<" str= if s"   s[sp] = (s[sp]<0)?-1:0;" c-line exit then
  tok$ s" 0>" str= if s"   s[sp] = (s[sp]>0)?-1:0;" c-line exit then
  tok$ s" 0=" str= if s"   s[sp] = (s[sp]==0)?-1:0;" c-line exit then
  tok$ s" max" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=(a>b)?a:b; }" c-line exit then
  tok$ s" min" str= if s"   { int64_t b=s[sp--], a=s[sp]; s[sp]=(a<b)?a:b; }" c-line exit then

  \ Stack ops
  tok$ s" dup" str= if s"   s[sp+1]=s[sp]; sp++;" c-line exit then
  tok$ s" drop" str= if s"   sp--;" c-line exit then
  tok$ s" swap" str= if s"   { int64_t t=s[sp]; s[sp]=s[sp-1]; s[sp-1]=t; }" c-line exit then
  tok$ s" over" str= if s"   s[sp+1]=s[sp-1]; sp++;" c-line exit then
  tok$ s" rot" str= if s"   { int64_t c=s[sp],b=s[sp-1],a=s[sp-2]; s[sp-2]=b; s[sp-1]=c; s[sp]=a; }" c-line exit then
  tok$ s" -rot" str= if s"   { int64_t c=s[sp],b=s[sp-1],a=s[sp-2]; s[sp-2]=c; s[sp-1]=a; s[sp]=b; }" c-line exit then
  tok$ s" nip" str= if s"   s[sp-1]=s[sp]; sp--;" c-line exit then
  tok$ s" tuck" str= if s"   { int64_t t=s[sp]; s[sp]=s[sp-1]; s[sp-1]=t; s[++sp]=t; }" c-line exit then
  tok$ s" 2dup" str= if s"   s[sp+1]=s[sp-1]; s[sp+2]=s[sp]; sp+=2;" c-line exit then
  tok$ s" 2drop" str= if s"   sp-=2;" c-line exit then
  tok$ s" depth" str= if s"   s[sp+1]=sp+1; sp++;" c-line exit then

  \ I/O
  tok$ s" ." str= if
    s"   printf(" c-type DQ c-emit s" %ld " c-type DQ c-emit s" , s[sp--]);" c-line exit then
  tok$ s" cr" str= if
    s"   printf(" c-type DQ c-emit s" \n" c-type DQ c-emit s" );" c-line exit then
  tok$ s" emit" str= if s"   putchar((int)s[sp--]);" c-line exit then

  \ Exit
  tok$ s" exit" str= if s"   return sp;" c-line exit then

  \ Control flow (state machine - no lookahead needed)
  tok$ s" if" str= if s"   { int64_t cond=s[sp--]; if(cond) {" c-line exit then
  tok$ s" else" str= if s"   } else {" c-line exit then
  tok$ s" then" str= if s"   }}" c-line exit then
  tok$ s" begin" str= if s"   for(;;) {" c-line exit then
  tok$ s" until" str= if s"   if(s[sp--]) break; }" c-line exit then
  tok$ s" while" str= if s"   if(!s[sp--]) break;" c-line exit then
  tok$ s" repeat" str= if s"   }" c-line exit then
  tok$ s" again" str= if s"   }" c-line exit then

  \ Do/loop
  tok$ s" do" str= if
    s"   { int64_t _i" c-type do-depth @ [char] 0 + c-emit
    s" =s[sp--], _lim" c-type do-depth @ [char] 0 + c-emit
    s" =s[sp--]; for(;;) {" c-line
    1 do-depth +! exit
  then
  tok$ s" loop" str= if
    -1 do-depth +!
    s"   if(++_i" c-type do-depth @ [char] 0 + c-emit
    s"  >= _lim" c-type do-depth @ [char] 0 + c-emit
    s" ) break; }}" c-line exit
  then
  tok$ s" +loop" str= if
    -1 do-depth +!
    s"   _i" c-type do-depth @ [char] 0 + c-emit
    s"  += s[sp--]; if(_i" c-type do-depth @ [char] 0 + c-emit
    s"  >= _lim" c-type do-depth @ [char] 0 + c-emit
    s" ) break; }}" c-line exit
  then
  tok$ s" i" str= if do-depth @ 0> if
    s"   s[++sp] = _i" c-type do-depth @ 1- [char] 0 + c-emit s" ;" c-line exit
  then then
  tok$ s" j" str= if do-depth @ 1 > if
    s"   s[++sp] = _i" c-type do-depth @ 2 - [char] 0 + c-emit s" ;" c-line exit
  then then

  \ Recursion
  tok$ cur-word$ str= if
    s"   sp = call_" c-type cur-word$ c-type-san s" (s, sp);" c-line exit then
  tok$ s" recurse" str= if
    s"   sp = call_" c-type cur-word$ c-type-san s" (s, sp);" c-line exit then

  \ User-defined word call
  tok$ is-user-word? if
    s"   sp = call_" c-type tok$ c-type-san s" (s, sp);" c-line exit then

  \ Number
  tok$ is-forth-number? if tok$ emit-c-number exit then

  \ Unknown
  s"   /* UNKNOWN: " c-type tok$ c-type s"  */" c-line ;

\ === Translation driver ===

: emit-word-body ( -- )
  begin next-tok while
    tok$ s" ;" str= if
      cur-word$ s" main" str= if
        s"   return 0;" c-line
      else
        s"   return sp;" c-line
      then
      s" }" c-line s" " c-line exit
    then
    emit-c-tok
  repeat ;

: emit-c-words ( -- )
  begin next-tok while
    tok$ s" :" str= if
      next-tok drop
      tok-len @ cur-word-len !
      tok-buf cur-word cur-word-len @ move
      0 do-depth !
      cur-word$ s" main" str= if
        s" int main(void) {" c-line
        s"   int sp = -1;" c-line
      else
        s" static int call_" c-type cur-word$ c-type-san
        s" (int64_t *s, int sp) {" c-line
      then
      emit-word-body
    then
  repeat ;

: forth-to-c ( c-path-addr c-path-u -- flag )
  w/o create-file 0<> if drop false exit then
  c-fid !
  collect-words
  s" #include <stdio.h>" c-line
  s" #include <stdint.h>" c-line
  s" int64_t s[1024];" c-line
  s" " c-line
  def-count @ 0 do
    i def-name$ s" main" str= 0= if
      s" static int call_" c-type i def-name$ c-type-san
      s" (int64_t *s, int sp);" c-line
    then
  loop
  s" " c-line
  emit-c-words
  c-fid @ close-file drop
  true ;

\ === Benchmark runner ===

\ Counters
variable yes-count    0 yes-count !
variable no-count     0 no-count !
variable nt-count     0 nt-count !
variable cfail-count  0 cfail-count !
variable both-count   0 both-count !
variable tf-fail-count 0 tf-fail-count !
variable test-total   0 test-total !

\ Timing accumulators (nanoseconds)
variable tf-comp-sum    0 tf-comp-sum !
variable tf-run-sum     0 tf-run-sum !
variable g2-comp-sum    0 g2-comp-sum !
variable g2-run-sum     0 g2-run-sum !
variable tf-comp-n      0 tf-comp-n !
variable g2-comp-n      0 g2-comp-n !
variable tf-run-n       0 tf-run-n !
variable g2-run-n       0 g2-run-n !

\ Per-test timing (nanoseconds, -1 = fail)
variable t-tf-comp
variable t-tf-run
variable t-g0-comp   variable t-g1-comp   variable t-g2-comp   variable t-g3-comp
variable t-g0-run    variable t-g1-run    variable t-g2-run    variable t-g3-run

\ Output buffers
create tf-out-buf 4096 allot   variable tf-out-len
create gcc-out-buf 4096 allot  variable gcc-out-len
variable trans-ok

\ Markdown output file
variable md-fid
: md-emit ( c -- ) md-fid @ emit-file drop ;
: md-type ( addr u -- ) md-fid @ write-file drop ;
: md-nl ( -- ) 10 md-emit ;
: md-line ( addr u -- ) md-type md-nl ;

\ Number formatting
create digit-buf 20 allot
: md-num ( n -- )
  dup 0< if [char] - md-emit negate then
  dup 0= if drop [char] 0 md-emit exit then
  0 >r
  begin dup 0> while
    dup 10 mod [char] 0 + digit-buf r@ + c!
    r> 1+ >r 10 /
  repeat drop
  r> begin dup 0> while 1- digit-buf over + c@ md-emit repeat drop ;

: md-ms ( ns -- )
  dup 0< if s" FAIL" md-type drop exit then
  dup 1000000 / md-num
  [char] . md-emit
  1000 / 1000 mod
  dup 100 / [char] 0 + md-emit
  dup 10 / 10 mod [char] 0 + md-emit
  10 mod [char] 0 + md-emit ;

: md-ratio ( tf-ns gcc-ns -- )
  dup 0< over 0= or if 2drop s" n/a" md-type exit then
  swap dup 0< if 2drop s" n/a" md-type exit then
  swap 100 * swap /
  dup 100 / md-num [char] . md-emit 100 mod
  dup 10 / [char] 0 + md-emit 10 mod [char] 0 + md-emit
  [char] x md-emit ;

\ Path helpers
variable last-slash
create name-stash 64 allot   variable name-stash-len
create path-stash 256 allot  variable path-stash-len

: stash-name ( addr u -- ) dup name-stash-len ! name-stash swap move ;
: stash$ ( -- addr u ) name-stash name-stash-len @ ;
: stash-path ( addr u -- ) dup path-stash-len ! path-stash swap move ;
: path$ ( -- addr u ) path-stash path-stash-len @ ;

: path>name ( addr u -- name-addr name-u )
  0 last-slash !
  over swap
  0 do dup i + c@ [char] / = if i 1+ last-slash ! then loop
  drop last-slash @ +
  dup 64 + >r
  dup begin dup r@ < while dup c@ [char] . <> while 1+ repeat then
  r> drop over - ;

\ Parse nanoseconds from /tmp/fb/timing
: parse-time ( -- ns )
  s" /tmp/fb/timing" slurp-file
  0 >r
  begin dup 0> while
    over c@ dup is-digit? if
      [char] 0 - r> 10 * + >r
    else drop then
    1 /string
  repeat 2drop r> ;

\ Timed system call using date +%s%N
\ Build command in str-buf FIRST, then call this
: ns-time ( -- ns exit-code )
  str2-reset
  s" t0=$(date +%s%N); " str2+
  str$ str2+
  s" ; rc=$?; t1=$(date +%s%N); echo $((t1-t0))>/tmp/fb/timing; exit $rc" str2+
  str2$ system-rc
  >r parse-time r> ;

\ Strip trailing whitespace
: strip-ws ( addr u -- addr u' )
  begin dup 0> while
    2dup + 1- c@ dup 32 = over 10 = or over 13 = or over 9 = or nip while
    1-
  repeat then ;

\ Capture + strip output from a file
: capture-to ( buf len-var path-addr path-u -- )
  slurp-file strip-ws            \ buf len-var data-addr data-u
  dup 4095 > if drop 4095 then   \ buf len-var data-addr data-u
  dup 3 pick !                   \ buf len-var data-addr data-u  (len stored)
  >r swap drop swap r>           \ data-addr buf data-u
  move ;

\ Find newline position in string
: find-nl ( addr u -- pos )
  0 begin
    2dup > while
    2 pick over + c@ 10 = if nip nip exit then
    1+
  repeat nip nip ;

\ Count newlines
: count-lines ( addr u -- n )
  0 -rot
  begin dup 0> while
    over c@ 10 = if rot 1+ -rot then
    1 /string
  repeat 2drop ;

\ === Per-test benchmark (split to avoid definition overflow) ===

: bench-init ( path-addr path-u -- )
  1 test-total +!
  2dup path>name stash-name
  stash-path
  -1 t-tf-comp ! -1 t-tf-run !
  -1 t-g0-comp ! -1 t-g1-comp ! -1 t-g2-comp ! -1 t-g3-comp !
  -1 t-g0-run ! -1 t-g1-run ! -1 t-g2-run ! -1 t-g3-run !
  0 tf-out-len ! 0 gcc-out-len ! 0 trans-ok ! ;

: bench-translate ( -- )
  path$ slurp-file
  dup 0= if 2drop 0 trans-ok ! exit then
  src-rem ! src-addr !
  s" /tmp/fb/test.c" forth-to-c trans-ok ! ;

: bench-tf-compile ( -- )
  str-reset
  s" ./sixth compiler/sixth.fs " str+ path$ str+ s"  /tmp/fb/tf-out >/dev/null 2>&1" str+
  ns-time
  0= if t-tf-comp ! else drop -1 t-tf-comp ! then ;

: bench-gcc-compile ( -- )
  trans-ok @ 0= if exit then
  str-reset s" gcc -O0 -o /tmp/fb/g0 /tmp/fb/test.c >/dev/null 2>&1" str+
  ns-time 0= if t-g0-comp ! else drop -1 t-g0-comp ! then
  str-reset s" gcc -O1 -o /tmp/fb/g1 /tmp/fb/test.c >/dev/null 2>&1" str+
  ns-time 0= if t-g1-comp ! else drop -1 t-g1-comp ! then
  str-reset s" gcc -O2 -o /tmp/fb/g2 /tmp/fb/test.c >/dev/null 2>&1" str+
  ns-time 0= if t-g2-comp ! else drop -1 t-g2-comp ! then
  str-reset s" gcc -O3 -o /tmp/fb/g3 /tmp/fb/test.c >/dev/null 2>&1" str+
  ns-time 0= if t-g3-comp ! else drop -1 t-g3-comp ! then ;

: bench-run ( -- )
  \ Run tf binary
  t-tf-comp @ 0>= if
    str-reset s" timeout 5 /tmp/fb/tf-out > /tmp/fb/tf-stdout 2>&1" str+
    ns-time
    0= if t-tf-run ! tf-out-buf tf-out-len s" /tmp/fb/tf-stdout" capture-to
    else drop -1 t-tf-run ! then
  then
  \ Run gcc-O0
  t-g0-comp @ 0>= if
    str-reset s" timeout 5 /tmp/fb/g0 > /tmp/fb/g0-stdout 2>&1" str+
    ns-time 0= if t-g0-run ! gcc-out-buf gcc-out-len s" /tmp/fb/g0-stdout" capture-to
    else drop -1 t-g0-run ! then
  then
  \ Run gcc-O1
  t-g1-comp @ 0>= if
    str-reset s" timeout 5 /tmp/fb/g1 > /tmp/fb/g1-stdout 2>&1" str+
    ns-time 0= if t-g1-run ! else drop -1 t-g1-run ! then
  then
  \ Run gcc-O2
  t-g2-comp @ 0>= if
    str-reset s" timeout 5 /tmp/fb/g2 > /tmp/fb/g2-stdout 2>&1" str+
    ns-time 0= if t-g2-run ! else drop -1 t-g2-run ! then
  then
  \ Run gcc-O3
  t-g3-comp @ 0>= if
    str-reset s" timeout 5 /tmp/fb/g3 > /tmp/fb/g3-stdout 2>&1" str+
    ns-time 0= if t-g3-run ! else drop -1 t-g3-run ! then
  then ;

: bench-judge ( -- addr u )
  tf-out-len @ 0> gcc-out-len @ 0> and if
    tf-out-buf tf-out-len @ gcc-out-buf gcc-out-len @ str= if
      1 yes-count +! s" YES"
    else 1 no-count +! s" NO" then
  else
    trans-ok @ 0= if 1 nt-count +! s" n/t"
    else
      tf-out-len @ 0= gcc-out-len @ 0= and if 1 both-count +! s" BOTH"
      else
        tf-out-len @ 0> if 1 cfail-count +! s" C-FAIL"
        else 1 tf-fail-count +! s" TF-FAIL" then
      then
    then
  then ;

: bench-accum ( -- )
  t-tf-comp @ 0>= if t-tf-comp @ tf-comp-sum +! 1 tf-comp-n +! then
  t-tf-run @ 0>= if t-tf-run @ tf-run-sum +! 1 tf-run-n +! then
  t-g2-comp @ 0>= if t-g2-comp @ g2-comp-sum +! 1 g2-comp-n +! then
  t-g2-run @ 0>= if t-g2-run @ g2-run-sum +! 1 g2-run-n +! then ;

: bench-row ( addr u -- )
  s" | " md-type test-total @ md-num
  s"  | " md-type stash$ md-type
  s"  | " md-type md-type
  s"  | " md-type t-tf-comp @ md-ms
  s"  | " md-type t-g0-comp @ md-ms
  s"  | " md-type t-g1-comp @ md-ms
  s"  | " md-type t-g2-comp @ md-ms
  s"  | " md-type t-g3-comp @ md-ms
  s"  | " md-type t-tf-run @ md-ms
  s"  | " md-type t-g0-run @ md-ms
  s"  | " md-type t-g1-run @ md-ms
  s"  | " md-type t-g2-run @ md-ms
  s"  | " md-type t-g3-run @ md-ms
  s"  | " md-type t-tf-run @ t-g2-run @ md-ratio
  s"  |" md-line ;

: bench-test ( path-addr path-u -- )
  bench-init
  bench-translate
  bench-tf-compile
  bench-gcc-compile
  bench-run
  bench-judge
  bench-accum
  bench-row
  [char] . emit ;

\ === File list buffer ===

65536 constant LIST-MAX
create list-buf LIST-MAX allot
variable list-len

\ === Main ===

: md-header ( -- )
  s" # Sixth Compiler Benchmark Results" md-line
  s" " md-line s" " md-line
  s" | # | Test | Correct | tf comp | gcc-O0 comp | gcc-O1 comp | gcc-O2 comp | gcc-O3 comp | tf run | gcc-O0 run | gcc-O1 run | gcc-O2 run | gcc-O3 run | tf/gcc-O2 |" md-line
  s" |---|------|---------|---------|-------------|-------------|-------------|-------------|--------|------------|------------|------------|------------|-----------|" md-line ;

: run-tests ( addr u -- )
  begin
    dup 0> while
    2dup find-nl >r
    over r@
    dup 3 > if bench-test else 2drop then
    r> 1+ /string
  repeat 2drop ;

: md-summary ( -- )
  s" " md-line
  s" ## Correctness Summary" md-line s" " md-line
  s" - **Correct (tf = gcc)**: " md-type yes-count @ md-num md-nl
  s" - **Wrong output**: " md-type no-count @ md-num md-nl
  s" - **sixth.fs no output**: " md-type tf-fail-count @ md-num md-nl
  s" - **C translation failed**: " md-type nt-count @ md-num md-nl
  s" - **GCC failed**: " md-type cfail-count @ md-num md-nl
  s" - **Both no output**: " md-type both-count @ md-num md-nl
  s" - **Total**: " md-type test-total @ md-num md-nl ;

: md-perf ( -- )
  s" " md-line
  s" ## Performance Summary" md-line s" " md-line
  tf-comp-n @ 0> if
    s" - sixth.fs avg compile: " md-type tf-comp-sum @ tf-comp-n @ / md-ms s" ms" md-line
  then
  g2-comp-n @ 0> if
    s" - gcc-O2 avg compile: " md-type g2-comp-sum @ g2-comp-n @ / md-ms s" ms" md-line
  then
  tf-run-n @ 0> if
    s" - sixth.fs avg runtime: " md-type tf-run-sum @ tf-run-n @ / md-ms s" ms" md-line
  then
  g2-run-n @ 0> if
    s" - gcc-O2 avg runtime: " md-type g2-run-sum @ g2-run-n @ / md-ms s" ms" md-line
  then ;

\ === Batch mode ===
\ bench.sh calls us with: ./sixth compiler/bench.fs <batch-file>
\ We read the batch file, run those tests, append rows to /tmp/fb/out.md
\ and write counters to /tmp/fb/counters.txt

: run-bench ( -- )
  s" rm -rf /tmp/fb && mkdir -p /tmp/fb" system
  s" BENCHMARK01.md" w/o create-file throw md-fid !
  md-header
  s" ls compiler/tests/[0-9]*.fs > /tmp/fb/testlist.txt 2>/dev/null" system
  s" /tmp/fb/testlist.txt" slurp-file
  dup LIST-MAX < 0= if 2drop ." Too large" cr exit then
  dup 0= if 2drop ." No files" cr exit then
  dup list-len ! list-buf swap move
  list-buf list-len @ 2dup count-lines
  ." Benchmarking " . ." tests" cr
  run-tests
  md-summary md-perf
  md-fid @ close-file drop
  cr ." Written to BENCHMARK01.md" cr
  ." Correct: " yes-count @ . ." / " test-total @ . cr
  ." Wrong: " no-count @ . cr ;

run-bench bye
