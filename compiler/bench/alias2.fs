\ expected: 102400000
\ Restrict-like pattern - read from one, write to another

create src 8192 allot
create dst 8192 allot

: init ( -- ) 1024 0 do i i 8 * src + ! loop ;

: copy-scaled ( -- )
  1024 0 do
    i 8 * src + @ 2 * i 8 * dst + !
  loop ;

: main
  init
  0 100000 0 do
    copy-scaled
    1024 +
  loop . cr ;
