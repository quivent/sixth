// expected: 500000000
// Stack depth 5, 500M ops
#include <stdio.h>

int main(void) {
    long stack[5];
    stack[0] = 0;  // acc
    stack[1] = 1;
    stack[2] = 2;
    stack[3] = 3;
    stack[4] = 4;
    for (long i = 0; i < 500000000L; i++) {
        stack[0] = stack[0] + 1;
    }
    printf("%ld\n", stack[0]);
    return 0;
}
