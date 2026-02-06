\ expect: 1
\ RSHIFT must be logical (zero-fill) not arithmetic
\ -1 >> 63 = 1 if logical, -1 if arithmetic
\ Tests LSR vs ASR instruction selection
: main -1 63 rshift 1 = if 1 else 0 then ;
