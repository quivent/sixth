\ stress-words-03.fs - Word modifies variable then calls another word
\ expect: 100
variable acc
: bump acc @ 1+ acc ! ;
: bump10 bump bump bump bump bump bump bump bump bump bump ;
: main 0 acc ! bump10 bump10 bump10 bump10 bump10 bump10 bump10 bump10 bump10 bump10 acc @ ;
