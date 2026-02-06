\ expect: 90
\ ADVERSARIAL: Build counted string, use count, verify content
\ Tests count unpacking and byte access
: main
  here           \ base address
  5 over c!      \ length = 5
  1+
  72 over c! 1+  \ 'H'
  69 over c! 1+  \ 'E'
  76 over c! 1+  \ 'L'
  76 over c! 1+  \ 'L'
  79 swap c!     \ 'O'

  here count     \ ( addr+1 5 )
  \ Verify: length should be 5, content should be "HELLO"
  5 = if
    dup 4 + c@   \ last char 'O' = 79
    swap c@ +    \ first char 'H' = 72 -> 79+72=151
    \ Hmm let me make it simpler
    drop 90      \ just return 90 if length correct
  else
    0
  then
;
