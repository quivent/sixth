\ expect: 7
\ Pattern A31: swap drop
\ swap drop = nip — drops original TOS, keeps NOS
: main 3 7 swap drop . cr ;
