\ expect: 111
\ Return stack across nested calls
\ Each level preserves a value on return stack
: inner 10 ;
: middle 100 >r inner r> + ;
: outer 1 >r middle r> + ;
: main outer ;
