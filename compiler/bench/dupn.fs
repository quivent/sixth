\ expected: 300000000
\ Dup at various depths, 300M times
: main
  0 1 2 3                   ( a b c d )
  300000000 0 do
    dup drop                ( a b c d ) dup top
    over nip                ( a b c d ) dup second via over
    >r dup r> swap drop     ( a b c d ) more stack work
    >r >r >r 1 + r> r> r>   ( a' b c d )
  loop
  drop drop drop . cr
;
