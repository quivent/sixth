\ expect: SKIP
\ Pattern A29: swap u> (UNIMPLEMENTED — u> not in compiler)
\ ADVERSARIAL: 7 u> 3=true(-1) — without swap: 3 u> 7=false(0)
\ SKIP: u> is not implemented in sixth.fs
: main 3 7 swap u> . cr ;
