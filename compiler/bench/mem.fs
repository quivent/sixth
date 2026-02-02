\ expected: 0
\ Pure memory stress - no arithmetic except index

4096 constant SIZE
create buf SIZE cells allot

: init SIZE 0 do i buf i cells + ! loop ;

: main
  init
  0 1000000000 0 do buf i $FFF and cells + @ + loop . cr ;
