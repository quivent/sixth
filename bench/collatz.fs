\ collatz.fs - Collatz sequence: real branching + mixed arithmetic
\ For each n from 1 to 50000, compute Collatz steps to reach 1.
\ Sum all step counts. Heavy branching, division, multiplication.
\ gcc -O2 will strength-reduce 3*n+1 and optimize the branch.
\ sixth.fs emits literal code per word with no cross-word optimization.
\ This is where a real optimizing compiler pulls ahead.
: steps ( n -- count )
  0 swap
  begin dup 1 > while
    dup 1 and if
      3 * 1+
    else
      2 /
    then
    swap 1+ swap
  repeat drop ;
: main ( -- )
  0 50000 1 do
    i steps +
  loop . cr ;
