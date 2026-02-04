\ bench.fs - sixth.fs vs GCC -O0 -O1 -O2 -O3 benchmark
\ Usage: ./engine/fifth compiler/native/bench.fs [N]   N=1..5 (default 5)

\ Missing words
: >= ( a b -- flag ) < 0= ;

\ ============================================================
\ STRING BUFFER
\ ============================================================
create cmd 1024 allot
variable cmd#
: c0   0 cmd# ! ;
: c+ ( addr u -- ) cmd cmd# @ + swap dup cmd# +! move ;
: c1 ( ch -- )     cmd cmd# @ + c!  1 cmd# +! ;
: c$ ( -- addr u ) cmd cmd# @ ;

\ Benchmark name (persistent)
create bn 16 allot
variable bn#
: bn! ( addr u -- ) dup bn# ! bn swap move ;
: bn$ ( -- addr u ) bn bn# @ ;

\ ============================================================
\ FILE WRITE HELPERS
\ ============================================================
variable wfid
: >f ( addr u -- ) wfid @ write-file throw ;
: >n  10 wfid @ emit-file throw ;
: >open ( addr u -- ) w/o create-file throw wfid ! ;
: >shut wfid @ close-file throw ;

\ ============================================================
\ TIMING
\ ============================================================
variable t0
: tick  clock-ms t0 ! ;
: tock  clock-ms t0 @ - ;

\ Print milliseconds as " X.XXs" — always 6 chars
: digit ( n -- ) [char] 0 + emit ;
: .sec ( sec -- ) dup 10 < if space digit else dup 10 / digit 10 mod digit then ;
: .frac ( ms -- ) 1000 mod dup 100 / digit 10 / 10 mod digit ;
: .t ( ms -- )
  dup 0< if drop ."  FAIL " exit then
  dup 100000 >= if drop ." 99.99s" exit then
  dup 1000 / .sec [char] . emit .frac [char] s emit ;

\ ============================================================
\ CREATE BENCHMARK SOURCES IN /tmp/fb/
\ ============================================================

: mk-fib-fs   s" /tmp/fb/fib.fs" >open
  s" : fib ( n -- r ) dup 2 < if else dup 1- recurse swap 2 - recurse + then ;" >f >n
  s" : main ( -- ) 45 fib . cr ;" >f >n >shut ;

: mk-fib-c   s" /tmp/fb/fib.c" >open
  s" #include <stdio.h>" >f >n
  s" long fib(long n){if(n<2)return n;return fib(n-1)+fib(n-2);}" >f >n
  s\" int main(){printf(\"%ld\\n\",fib(45));return 0;}" >f >n >shut ;

: mk-sum-fs   s" /tmp/fb/sum.fs" >open
  s" : main ( -- )" >f >n
  s"   0 1000000000" >f >n
  s"   begin dup while" >f >n
  s"     swap over + swap 1-" >f >n
  s"   repeat drop . cr ;" >f >n >shut ;

: mk-sum-c   s" /tmp/fb/sum.c" >open
  s" #include <stdio.h>" >f >n
  s" #include <stdint.h>" >f >n
  s" int main(){" >f >n
  s"   int64_t s=0;" >f >n
  s"   for(int64_t i=1;i<=1000000000LL;i++)s+=i;" >f >n
  s\"   printf(\"%ld\\n\",s);return 0;}" >f >n >shut ;

: mk-loop-fs   s" /tmp/fb/loop.fs" >open
  s" : main ( -- )" >f >n
  s"   1000000000 begin 1- dup while repeat drop 0 . cr ;" >f >n >shut ;

: mk-loop-c   s" /tmp/fb/loop.c" >open
  s" #include <stdio.h>" >f >n
  s" #include <stdint.h>" >f >n
  s" int main(){" >f >n
  s"   for(volatile int64_t i=1000000000LL;i>0;i--);" >f >n
  s\"   printf(\"0\\n\");return 0;}" >f >n >shut ;

: mk-doloop-fs   s" /tmp/fb/doloop.fs" >open
  s" : main ( -- )" >f >n
  s"   1000000000 0 do loop 0 . cr ;" >f >n >shut ;

: mk-doloop-c   s" /tmp/fb/doloop.c" >open
  s" #include <stdio.h>" >f >n
  s" #include <stdint.h>" >f >n
  s" int main(){" >f >n
  s"   for(volatile int64_t i=0;i<1000000000LL;i++);" >f >n
  s\"   printf(\"0\\n\");return 0;}" >f >n >shut ;

: mk-arith-fs   s" /tmp/fb/arith.fs" >open
  s" : main ( -- )" >f >n
  s"   1 100000000" >f >n
  s"   begin dup while" >f >n
  s"     swap 3 * 7 + $FFFFFF and swap 1-" >f >n
  s"   repeat drop . cr ;" >f >n >shut ;

: mk-arith-c   s" /tmp/fb/arith.c" >open
  s" #include <stdio.h>" >f >n
  s" #include <stdint.h>" >f >n
  s" int main(){" >f >n
  s"   int64_t x=1;" >f >n
  s"   for(int64_t i=100000000;i>0;i--)x=(x*3+7)&0xFFFFFF;" >f >n
  s\"   printf(\"%ld\\n\",x);return 0;}" >f >n >shut ;

: mk-all
  s" mkdir -p /tmp/fb" system
  mk-fib-fs mk-fib-c
  mk-sum-fs mk-sum-c
  mk-loop-fs mk-loop-c
  mk-doloop-fs mk-doloop-c
  mk-arith-fs mk-arith-c ;

\ ============================================================
\ COMMAND BUILDERS
\ ============================================================
variable cur-opt   \ ascii '0'..'3'

: tf-cc ( -- addr u )
  c0 s" ./engine/fifth compiler/sixth.fs /tmp/fb/" c+ bn$ c+ s" .fs /tmp/fb/" c+ bn$ c+ s" _tf 2>/dev/null" c+ c$ ;

: tf-rc ( -- addr u )
  c0 s" /tmp/fb/" c+ bn$ c+ s" _tf >/tmp/fb/" c+ bn$ c+ s" _tf.out 2>/dev/null" c+ c$ ;

: gcc-cc ( -- addr u )
  c0 s" gcc -O" c+ cur-opt @ c1 s"  -o /tmp/fb/" c+ bn$ c+ s" _g" c+ cur-opt @ c1
  s"  /tmp/fb/" c+ bn$ c+ s" .c 2>/dev/null" c+ c$ ;

: gcc-rc ( -- addr u )
  c0 s" /tmp/fb/" c+ bn$ c+ s" _g" c+ cur-opt @ c1
  s"  >/tmp/fb/" c+ bn$ c+ s" _g" c+ cur-opt @ c1 s" .out 2>/dev/null" c+ c$ ;

\ ============================================================
\ RUN ONE BENCHMARK
\ ============================================================
create bt 10 cells allot   \ 5 compilers x (compile, run)

: run-bench ( -- )
  \ tf
  tick tf-cc system tock  bt !
  tick tf-rc system tock  bt cell+ !
  \ gcc O0..O3
  4 0 do
    [char] 0 i + cur-opt !
    tick gcc-cc system tock  bt i dup + 2 + cells + !
    tick gcc-rc system tock  bt i dup + 3 + cells + !
  loop ;

\ ============================================================
\ OUTPUT
\ ============================================================

: .sep   ."  |" ;
: .pair ( i -- )  dup + cells bt + dup @ .t  cell+ @ .t ;

: .header
  cr
  ." Sixth sixth.fs vs GCC     (times in seconds)" cr
  cr
  ."               " .sep ."  sixth.fs      " .sep ."  gcc -O0    " .sep ."  gcc -O1    " .sep ."  gcc -O2    " .sep ."  gcc -O3    " cr
  ."  benchmark    " .sep ."  comp   run " .sep ."  comp   run " .sep ."  comp   run " .sep ."  comp   run " .sep ."  comp   run " cr
  ." ──────────────┼─────────────┼─────────────┼─────────────┼─────────────┼─────────────" cr ;

: .row ( desc-addr desc-u -- )
  dup 14 < if 2dup type 14 swap - spaces else 14 type then
  5 0 do .sep space i .pair loop cr ;

\ ============================================================
\ BENCHMARK TABLE
\ ============================================================

: get-n ( -- n )
  argc 3 < if 5 exit then
  2 argv drop c@ [char] 0 - 5 min 1 max ;

: main
  mk-all
  .header
  get-n
  dup 1 >= if s" fib"    bn! run-bench s" fib(45) rec  " .row then
  dup 2 >= if s" sum"    bn! run-bench s" sum(1B)      " .row then
  dup 3 >= if s" loop"   bn! run-bench s" loop(1B)     " .row then
  dup 4 >= if s" doloop" bn! run-bench s" do-loop(1B)  " .row then
  dup 5 >= if s" arith"  bn! run-bench s" arith(100M)  " .row then
  drop cr
  s" rm -rf /tmp/fb" system
  bye ;

main
