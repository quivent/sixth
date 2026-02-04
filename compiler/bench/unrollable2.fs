\ expected: 823739198976
\ Polynomial evaluation unroll pattern - Horner's method

: poly ( x -- result )
  dup dup * >r     \ x; r: x^2
  dup r@ * >r      \ x; r: x^2 x^3
  r> 3 * swap      \ 3*x^3 x
  r> 2 * +         \ 3*x^3+2*x^2 x
  + 1+ ;           \ 3*x^3+2*x^2+x+1

: main
  0 1024 0 do i poly + loop . cr ;
