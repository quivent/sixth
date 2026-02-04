// expected: 500000000
// Stack depth 4 - first spill level, 500M ops
#include <stdio.h>

int main(void) {
    long stack[4];
    stack[0] = 0;  // acc
    stack[1] = 1;  // d1
    stack[2] = 2;  // d2
    stack[3] = 3;  // d3
    for (long i = 0; i < 500000000L; i++) {
        stack[0] = stack[0] + 1;
    }
    printf("%ld\n", stack[0]);
    return 0;
}
