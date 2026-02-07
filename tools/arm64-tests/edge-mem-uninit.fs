\ expect: 0
\ EDGE: Documents that CREATE buffers contain stack garbage, NOT zeros
\ Tests: This test is expected to FAIL if memory were zero-initialized
\ The ARM64 runtime uses stack-based memory for variables/buffers
\ Stack memory is NOT zeroed by the OS - this is intentional/known behavior
\ IMPORTANT: Always initialize CREATE buffers before reading!

create virgin 8 allot

: main
  \ Read without ever writing - this will be garbage
  \ If this returns 0, something changed in the runtime
  virgin @
  0= if
    1       \ Unexpected: memory was zero
  else
    0       \ Expected: memory was garbage (non-zero)
  then
;
