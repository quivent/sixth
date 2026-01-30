\ fifth/lib/loops.fs - Loop Vocabulary with Performance Notes
\
\ LOOP COSTS (instructions per iteration):
\
\ | Word        | Ops | Use When                    |
\ |-------------|-----|-----------------------------|
\ | 1-nzloop    | 2   | Countdown, no index needed  |
\ | nzloop      | 3   | Test TOS, keep value        |
\ | 0=until     | 5   | Exit when zero              |
\ | do...loop   | 4   | Need index (i)              |
\ | begin..until| 12  | General condition           |
\
\ RULE: If you don't need `i`, don't use `do...loop`.

\ ============================================================
\ NOTE: >r and r> are NOT in native compiler (tf.fs)
\ ============================================================

\ ============================================================
\ COUNTDOWN LOOPS (fastest)
\ ============================================================

\ countdown ( n xt -- ) Execute xt n times, no index
\ Usage: 100 ' my-word countdown
: countdown ( n xt -- )
  swap begin over execute 1-nzloop drop drop ;

\ times ( n -- ) Just loop n times, do nothing
\ Usage: 1000000 times  \ burn cycles
: times ( n -- )
  begin 1-nzloop drop ;

\ ============================================================
\ INDEXED LOOPS (use when you need i)
\ ============================================================

\ Loop with index - use standard do...loop
\ Cost: 4 ops/iteration (inc, cmp, jl + your code)

\ upto ( limit start xt -- ) Call xt with each index
\ Usage: 10 0 [: i . ;] upto  \ prints 0 1 2 ... 9
: upto ( limit start xt -- )
  -rot do dup i swap execute loop drop ;

\ ============================================================
\ COMPARISONS (use <if >if =if when possible)
\ ============================================================

\ Standard: dup 2 < if ... then  (40+ bytes)
\ Fast:     dup 2 <if ... then   (20 bytes)
\
\ <if  ( a b -- ) Branch if NOT a<b, consumes both
\ >if  ( a b -- ) Branch if NOT a>b, consumes both
\ =if  ( a b -- ) Branch if NOT a=b, consumes both
\ 0=if ( n -- )   Branch if NOT n=0
\ 0<if ( n -- )   Branch if NOT n<0

\ ============================================================
\ ARITHMETIC SHORTCUTS
\ ============================================================

\ 2+  ( n -- n+2 )  4 bytes vs 17 for "2 +"
\ 2-  ( n -- n-2 )  4 bytes vs 17 for "2 -"
\ 1+  ( n -- n+1 )  3 bytes
\ 1-  ( n -- n-1 )  3 bytes

\ ============================================================
\ EXAMPLES
\ ============================================================

\ SLOW: Sum 1 to n using do...loop (4 ops/iter)
: sum-slow ( n -- sum )
  0 swap 1+ 1 do i + loop ;

\ FAST: Sum 1 to n using countdown (2 ops/iter)
: sum-fast ( n -- sum )
  0 swap begin over + swap 1-nzloop nip ;

\ SLOW: Check if n < 10 (40 bytes)
\ : small? dup 10 < if ." yes" then drop ;

\ FAST: Check if n < 10 (20 bytes)
\ : small? dup 10 <if ." yes" then drop ;
