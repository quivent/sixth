\ expect: 45 90
\ Test 821: do loop followed by stack ops
: main 0 10 0 do i + loop dup . 2* . cr ;
