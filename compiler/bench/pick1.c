// expected: 500000000
// Pick from depth 1 (over), 500M times
#include <stdio.h>

int main(void) {
    long stack[2];
    stack[0] = 0;  // acc
    stack[1] = 1;  // dummy
    for (long i = 0; i < 500000000L; i++) {
        long picked = stack[0];  // pick 1 = over
        stack[0] = picked + 1;
    }
    printf("%ld\n", stack[0]);
    return 0;
}
