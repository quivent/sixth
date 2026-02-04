\ expected: 511500000
\ FFT on 1024 elements, 1000 times (integer approximation)
\ Uses fixed-point arithmetic scaled by 1000
\ Checksum: sum of first element of result * iterations

create real 1024 cells allot
create imag 1024 cells allot
create tmp-r 1024 cells allot
create tmp-i 1024 cells allot

1000 constant SCALE

: init-data ( iter -- )
  1024 0 do
    dup i + 1024 mod SCALE * real i cells + !
    0 imag i cells + !
  loop drop ;

: bit-reverse ( n bits -- rev )
  0 swap
  0 do
    swap 2* over 1 and or
    swap 2/
  loop drop ;

: fft-iter ( size -- )
  dup 2/ 0 do
    1024 over / 0 do
      j over * i +
      dup real swap cells + @
      over over + real swap cells + @
      2dup + 2/ tmp-r 3 pick cells + !
      - 2/ tmp-r 3 pick 3 pick + cells + !
      dup imag swap cells + @
      over over + imag swap cells + @
      2dup + 2/ tmp-i 4 pick cells + !
      - 2/ tmp-i 4 pick 4 pick + cells + !
      2drop drop
    loop drop
  loop
  1024 0 do
    tmp-r i cells + @ real i cells + !
    tmp-i i cells + @ imag i cells + !
  loop drop ;

: simple-fft ( -- )
  2 begin dup 1024 <= while
    dup fft-iter
    2*
  repeat drop ;

: bench-fft ( -- sum )
  0
  1000 0 do
    i init-data
    simple-fft
    real @ +
  loop ;

: main bench-fft . cr ;
