\ expect: 99
\ STRESS: CREATE buffer passed through multiple call levels
\ Tests: Buffer address surviving deep call chain

create storage 8 allot

: store-inner ( addr n -- )
  swap ! ;

: store-mid ( addr n -- )
  store-inner ;

: store-outer ( addr n -- )
  store-mid ;

: load-inner ( addr -- n )
  @ ;

: load-mid ( addr -- n )
  load-inner ;

: load-outer ( addr -- n )
  load-mid ;

: main
  storage 99 store-outer
  storage load-outer
;
