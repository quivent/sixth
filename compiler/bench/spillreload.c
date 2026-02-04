// expected: 200000000
// Force spill then reload, 200M times
#include <stdio.h>

int main(void) {
    // 8 stack items, some will spill to memory
    long stack[8];
    stack[0] = 0;  // a
    stack[1] = 1;  // b
    stack[2] = 2;  // c
    stack[3] = 3;  // d
    stack[4] = 4;  // e
    stack[5] = 5;  // f
    stack[6] = 6;  // g
    stack[7] = 7;  // h

    for (long i = 0; i < 200000000L; i++) {
        // Access spilled value (simulate 7 pick)
        long picked = stack[0];
        stack[0] = picked + 1;
        // Restore other values (simulating the reload after drops)
        stack[1] = 1;
        stack[2] = 2;
        stack[3] = 3;
        stack[4] = 4;
        stack[5] = 5;
        stack[6] = 6;
        stack[7] = 7;
    }
    printf("%ld\n", stack[0]);
    return 0;
}
