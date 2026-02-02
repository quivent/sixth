\ arith-std.fs - Two-variable arithmetic WITHOUT nos+/1-nzloop
\ Same computation as arith.fs but standard Forth only.
\ C equivalent: while(n>0){a++;n--;} — gcc will use two registers, dec+jnz.
\ If sixth.fs is good, swap 1+ swap compiles to xchg+inc+xchg or similar.
\ If sixth.fs is bad, this will be 3-5x slower than arith.fs.
: main ( -- )
  0 1000000000
  begin
    swap 1+ swap
    1- dup 0>
  while repeat
  drop . cr ;
