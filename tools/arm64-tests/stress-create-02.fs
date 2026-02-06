\ expect: 88
\ STRESS: Large buffer (8192 bytes) with access at edges
\ Tests: Allocating large buffer, accessing first, middle, and last bytes

create bigbuf 8192 allot

: main
  88 bigbuf !                   \ first cell
  77 bigbuf 4096 + !            \ middle cell
  66 bigbuf 8184 + !            \ near-end cell
  55 bigbuf 8191 + c!           \ very last byte
  bigbuf @                      \ should be 88
;
