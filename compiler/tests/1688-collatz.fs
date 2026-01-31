\ expect: 8
\ Collatz sequence length for 6: 6->3->10->5->16->8->4->2->1 = 8 steps
: collatz ( n -- steps )
  0 swap
  begin dup 1 > while
    dup 1 and if
      3 * 1+
    else
      2/
    then
    swap 1+ swap
  repeat drop ;
: main 6 collatz . cr ;
