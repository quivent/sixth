\ Test 964: chained if-else simulating 3-way branch
: classify dup 0 < if drop 65 emit else dup 0 = if drop 66 emit else drop 67 emit then then ;
: main -1 classify 0 classify 1 classify cr ;
