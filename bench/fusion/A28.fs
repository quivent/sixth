\ expect: SKIP
\ Pattern A28: swap u< (UNIMPLEMENTED — u< not in compiler)
\ ADVERSARIAL: 7 u< 3=false(0) — without swap: 3 u< 7=true(-1)
\ SKIP: u< is not implemented in sixth.fs
: main 3 7 swap u< . cr ;
