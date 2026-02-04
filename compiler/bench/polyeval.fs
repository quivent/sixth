\ expected: -511200
\ Evaluate degree-100 polynomial, 100K times (using Horner's method)
\ Checksum: sum of (result mod 1000) for each evaluation

create coeffs 101 cells allot

: init-coeffs 101 0 do i 1+ coeffs i cells + ! loop ;

: poly-eval ( x -- result )
  0 101 0 do
    over * coeffs 100 i - cells + @ +
  loop swap drop ;

: bench-polyeval ( -- sum )
  init-coeffs
  0
  100000 0 do
    i 1000 mod poly-eval
    1000 mod +
  loop ;

: main bench-polyeval . cr ;
