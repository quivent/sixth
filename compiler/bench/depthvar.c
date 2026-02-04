// expected: 500000000
// Varying depth 1-8, 500M ops
#include <stdio.h>

int main(void) {
    long acc = 0;
    long stack[8];
    for (long i = 0; i < 500000000L; i++) {
        // Push 7 values (depth goes to 8)
        stack[1] = 1;
        stack[2] = 2;
        stack[3] = 3;
        stack[4] = 4;
        stack[5] = 5;
        stack[6] = 6;
        stack[7] = 7;
        // Pop them (depth back to 1)
        // Increment acc
        acc = acc + 1;
    }
    printf("%ld\n", acc);
    return 0;
}
