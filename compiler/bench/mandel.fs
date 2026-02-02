\ expected: (mandelbrot ASCII art)

: mchar ( n -- c )
  dup 80 > if drop 32 else
  dup 60 > if drop 46 else
  dup 40 > if drop 58 else
  dup 20 > if drop 43 else
  drop 35 then then then then ;

: sq ( n -- n*n ) dup * ;
: esc? ( zr zi -- zr zi flag ) over sq over sq + 262144 > ;

: pixel ( cr ci -- n )
  2dup 100
  begin
    dup 0> while
    esc? if drop nip nip exit then
    1-
    >r 2over
    over sq over sq - 5 pick +
    -rot * 2 * 4 pick +
    r>
  repeat
  nip nip nip nip ;

: row ( y -- )
  78 0 do
    i 12 * 640 - over 22 * 256 -
    pixel mchar emit
  loop drop ;

: main 24 0 do i row cr loop ;
