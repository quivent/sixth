// expected: 500000000
// Pick from depth 2, 500M times
#include <stdio.h>

int main(void) {
    long stack[3];
    stack[0] = 0;  // acc
    stack[1] = 1;  // d1
    stack[2] = 2;  // d2
    for (long i = 0; i < 500000000L; i++) {
        long picked = stack[0];  // pick 2
        stack[0] = picked + 1;
    }
    printf("%ld\n", stack[0]);
    return 0;
}
