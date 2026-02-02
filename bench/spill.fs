\ spill.fs - Force register spills with deep stack arithmetic
\ sixth.fs has 3 registers (rax, rbx, rcx). Items 4+ go to memory.
\ gcc -O2 has 15 GPRs. It keeps everything in registers.
\ Use >r/r> to shuttle values and rot/swap/over at depth.
\ Each iteration: 4 variables, shuffled through return stack.
\ C equivalent: a+=d; d=c; c=b; b=a; (4 register rotate+accumulate)
variable va  variable vb  variable vc  variable vd
: main ( -- )
  1 va !  2 vb !  3 vc !  4 vd !
  100000000 0 do
    va @ vd @ + va !
    vb @ vc @ + vb !
    va @ vb @ + vc !
    vc @ vd @ + vd !
  loop
  vd @ . cr ;
