// expected: 200000000
// Pick from depth 5, 200M times
#include <stdio.h>

int main(void) {
    long stack[6];
    stack[0] = 0;  // acc
    stack[1] = 1;
    stack[2] = 2;
    stack[3] = 3;
    stack[4] = 4;
    stack[5] = 5;
    for (long i = 0; i < 200000000L; i++) {
        long picked = stack[0];  // pick 5
        stack[0] = picked + 1;
    }
    printf("%ld\n", stack[0]);
    return 0;
}
