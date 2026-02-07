\ expect: 0
\ Test 2swap with address-like values that could confuse pointer vs data handling
\ Uses values that look like valid ARM64 addresses
: main
  0                      \ null pointer
  4096                   \ page size - common address boundary
  140737488351232        \ 0x7FFFFFFFE000 - near stack in typical layout
  140737488355328        \ 0x7FFFFFFFF000 - another stack-like address
  2swap
  \ Stack: 140737488351232 140737488355328 0 4096
  \ TOS=4096, then 0, then 0x7FFFFFFFF000, then 0x7FFFFFFFE000
  4096 - abs             \ TOS should be 4096
  swap 0 - abs +         \ next should be 0
  swap 140737488355328 - abs +  \ next should be 0x7FFFFFFFF000
  swap 140737488351232 - abs +  \ next should be 0x7FFFFFFFE000
;
