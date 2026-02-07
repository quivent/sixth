\ expect: 140737488355328 140737488351232 4096 0
\ Test 2swap with address-like values that could confuse pointer vs data handling
\ Uses values that look like valid ARM64 addresses
: main
  0                      \ null pointer
  4096                   \ page size - common address boundary
  140737488351232        \ 0x7FFFFFFFE000 - near stack in typical layout
  140737488355328        \ 0x7FFFFFFFF000 - another stack-like address
  2swap
  \ Stack: 0x7FFFFFFFE000 0x7FFFFFFFF000 0 4096
  . space  \ 0x7FFFFFFFF000
  . space  \ 0x7FFFFFFFE000
  . space  \ 4096
  .        \ 0
;
