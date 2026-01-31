\ expect: 5 4 3 2 1
\ Test 1434: print 5 values one by one — drain stack through register cascade
\ 1 2 3 4 5 → each . pops TOS, reloads from rbx→rax, rcx→rbx, mem→rcx
: main 1 2 3 4 5 . . . . . cr ;
