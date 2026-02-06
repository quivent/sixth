\ expect: 1
\ ADVERSARIAL: Alternating long/short paths
\ Tests that buffer doesn't have leftover data from long paths

variable buf
: main
  here buf ! 210 allot
  buf @
  47 over c!  \ /
  199 1 do 97 over i + c! loop
  200 0 open-file drop drop
  s" /tmp" 0 open-file drop
  0>= if 1 else 0 then
;
