\ stress-words-10.fs - Word calls itself through mutual recursion trampoline
\ expect: 15
\ Compute: 5 + 4 + 3 + 2 + 1 + 0 = 15 via indirect self-call
: trampoline dup 0= if else dup 1- bounce + then ;
: bounce trampoline ;
: main 5 trampoline ;
