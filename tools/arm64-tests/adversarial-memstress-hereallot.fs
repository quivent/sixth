\ expect: 80
\ ADVERSARIAL: Stress the here pointer with rapid allot/fill/read
\ Tests here pointer integrity across multiple allot calls
: main
  here 8 allot    \ alloc 8, here advances
  here 8 allot    \ alloc 8 more
  here 8 allot    \ alloc 8 more
  here 8 allot    \ alloc 8 more
  here 8 allot    \ alloc 8 more - total 40 bytes

  here 40 - 40 80 fill   \ fill all 40 bytes with 'P' (80)

  here 20 - c@    \ read middle of allocation (should be 80)
;
