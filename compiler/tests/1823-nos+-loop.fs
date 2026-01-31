\ expect: 5
\ nos+ in a loop: increment NOS 5 times
\ Start: ( 0 5 ). Each iteration: nos+ increments NOS by 1, 1-nzloop decrements TOS
\ After 5 iterations NOS goes 0->1->2->3->4->5, TOS goes 5->4->3->2->1->0
: main 0 5 begin nos+ 1-nzloop drop . cr ;
