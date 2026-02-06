\ expect: 142
\ STRESS: CREATE with interleaved cell and byte access
\ Tests: Write cells, read bytes; write bytes, read cells - little endian awareness

create mixed 16 allot

: main
  \ Store cell value 100 at offset 0 (bytes 0-7)
  100 mixed !
  \ Store individual bytes at offset 8
  10 mixed 8 + c!
  20 mixed 9 + c!
  12 mixed 10 + c!
  0 mixed 11 + c!
  0 mixed 12 + c!
  0 mixed 13 + c!
  0 mixed 14 + c!
  0 mixed 15 + c!
  \ Read back cell at 0
  mixed @                    \ 100
  \ Read back bytes
  mixed 8 + c@ +             \ +10 = 110
  mixed 9 + c@ +             \ +20 = 130
  mixed 10 + c@ +            \ +12 = 142
;
