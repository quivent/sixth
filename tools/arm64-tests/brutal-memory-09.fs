\ expect: 0
\ Test: COUNT for counted strings
\ Verify address increment and length extraction

\ Build a counted string manually: length byte followed by chars
variable cs0 variable cs1
variable fail-code

: main
  0 fail-code !

  \ Store: 5 H e l l o (length=5, then "Hello")
  \ byte 0: 5 (length)
  \ byte 1: H (72)
  \ byte 2: e (101)
  \ byte 3: l (108)
  \ byte 4: l (108)
  \ byte 5: o (111)
  5 cs0 c!
  72 cs0 1 + c!
  101 cs0 2 + c!
  108 cs0 3 + c!
  108 cs0 4 + c!
  111 cs0 5 + c!

  \ COUNT should return addr+1 and the count byte
  cs0 count

  \ Stack should have: addr len
  5 <> if 1 fail-code ! then
  fail-code @ 0= if cs0 1 + <> if 2 fail-code ! then else drop then

  \ Verify string content
  fail-code @ 0= if cs0 1 + c@ 72 <> if 3 fail-code ! then then
  fail-code @ 0= if cs0 2 + c@ 101 <> if 4 fail-code ! then then
  fail-code @ 0= if cs0 5 + c@ 111 <> if 5 fail-code ! then then

  fail-code @ ;
