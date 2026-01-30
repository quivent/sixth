\ lib.fs - Token matching words for ff.fs
\ Single character tokens
: is-:? ( a u -- ? ) 1 <> if drop false exit then c@ [char] : = ;
: is-;? ( a u -- ? ) 1 <> if drop false exit then c@ [char] ; = ;
: is-+? ( a u -- ? ) 1 <> if drop false exit then c@ [char] + = ;
: is--? ( a u -- ? ) 1 <> if drop false exit then c@ [char] - = ;
: is-*? ( a u -- ? ) 1 <> if drop false exit then c@ [char] * = ;
: is-/? ( a u -- ? ) 1 <> if drop false exit then c@ [char] / = ;
: is-.? ( a u -- ? ) 1 <> if drop false exit then c@ [char] . = ;
: is-=? ( a u -- ? ) 1 <> if drop false exit then c@ [char] = = ;
: is-<? ( a u -- ? ) 1 <> if drop false exit then c@ [char] < = ;
: is->? ( a u -- ? ) 1 <> if drop false exit then c@ [char] > = ;

\ Two character tokens
: is-cr? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] c <> if drop false exit then 1+ c@ [char] r = ;
: is-or? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] o <> if drop false exit then 1+ c@ [char] r = ;
: is-if? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] i <> if drop false exit then 1+ c@ [char] f = ;
: is-1+? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] 1 <> if drop false exit then 1+ c@ [char] + = ;
: is-1-? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] 1 <> if drop false exit then 1+ c@ [char] - = ;
: is-0=? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] 0 <> if drop false exit then 1+ c@ [char] = = ;
: is-0<? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] 0 <> if drop false exit then 1+ c@ [char] < = ;
: is-<>? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] < <> if drop false exit then 1+ c@ [char] > = ;
: is-<=? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] < <> if drop false exit then 1+ c@ [char] = = ;
: is->=? ( a u -- ? ) 2 <> if drop false exit then
  dup c@ [char] > <> if drop false exit then 1+ c@ [char] = = ;

\ Three character tokens
: is-dup? ( a u -- ? ) 3 <> if drop false exit then
  dup c@ [char] d <> if drop false exit then
  dup 1+ c@ [char] u <> if drop false exit then 2 + c@ [char] p = ;
: is-rot? ( a u -- ? ) 3 <> if drop false exit then
  dup c@ [char] r <> if drop false exit then
  dup 1+ c@ [char] o <> if drop false exit then 2 + c@ [char] t = ;
: is-nip? ( a u -- ? ) 3 <> if drop false exit then
  dup c@ [char] n <> if drop false exit then
  dup 1+ c@ [char] i <> if drop false exit then 2 + c@ [char] p = ;
: is-mod? ( a u -- ? ) 3 <> if drop false exit then
  dup c@ [char] m <> if drop false exit then
  dup 1+ c@ [char] o <> if drop false exit then 2 + c@ [char] d = ;
: is-and? ( a u -- ? ) 3 <> if drop false exit then
  dup c@ [char] a <> if drop false exit then
  dup 1+ c@ [char] n <> if drop false exit then 2 + c@ [char] d = ;
: is-xor? ( a u -- ? ) 3 <> if drop false exit then
  dup c@ [char] x <> if drop false exit then
  dup 1+ c@ [char] o <> if drop false exit then 2 + c@ [char] r = ;

\ Four character tokens
: is-over? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] o <> if drop false exit then
  dup 1+ c@ [char] v <> if drop false exit then
  dup 2 + c@ [char] e <> if drop false exit then 3 + c@ [char] r = ;
: is-swap? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] s <> if drop false exit then
  dup 1+ c@ [char] w <> if drop false exit then
  dup 2 + c@ [char] a <> if drop false exit then 3 + c@ [char] p = ;
: is-drop? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] d <> if drop false exit then
  dup 1+ c@ [char] r <> if drop false exit then
  dup 2 + c@ [char] o <> if drop false exit then 3 + c@ [char] p = ;
: is-tuck? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] t <> if drop false exit then
  dup 1+ c@ [char] u <> if drop false exit then
  dup 2 + c@ [char] c <> if drop false exit then 3 + c@ [char] k = ;
: is-2dup? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] 2 <> if drop false exit then
  dup 1+ c@ [char] d <> if drop false exit then
  dup 2 + c@ [char] u <> if drop false exit then 3 + c@ [char] p = ;
: is-else? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] e <> if drop false exit then
  dup 1+ c@ [char] l <> if drop false exit then
  dup 2 + c@ [char] s <> if drop false exit then 3 + c@ [char] e = ;
: is-then? ( a u -- ? ) 4 <> if drop false exit then
  dup c@ [char] t <> if drop false exit then
  dup 1+ c@ [char] h <> if drop false exit then
  dup 2 + c@ [char] e <> if drop false exit then 3 + c@ [char] n = ;

\ Five+ character tokens
: is-2drop? ( a u -- ? ) 5 <> if drop false exit then
  dup c@ [char] 2 <> if drop false exit then
  dup 1+ c@ [char] d <> if drop false exit then
  dup 2 + c@ [char] r <> if drop false exit then
  dup 3 + c@ [char] o <> if drop false exit then 4 + c@ [char] p = ;
: is-negate? ( a u -- ? ) 6 <> if drop false exit then
  dup c@ [char] n <> if drop false exit then
  dup 1+ c@ [char] e <> if drop false exit then
  dup 2 + c@ [char] g <> if drop false exit then
  dup 3 + c@ [char] a <> if drop false exit then
  dup 4 + c@ [char] t <> if drop false exit then 5 + c@ [char] e = ;
: is-invert? ( a u -- ? ) 6 <> if drop false exit then
  dup c@ [char] i <> if drop false exit then
  dup 1+ c@ [char] n <> if drop false exit then
  dup 2 + c@ [char] v <> if drop false exit then
  dup 3 + c@ [char] e <> if drop false exit then
  dup 4 + c@ [char] r <> if drop false exit then 5 + c@ [char] t = ;

\ Literals for pattern matching
: is-2? ( a u -- ? ) 1 <> if drop false exit then c@ [char] 2 = ;
