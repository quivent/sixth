// expected: 500000000
// Pick from depth 0 (dup), 500M times
#include <stdio.h>

int main(void) {
    long acc = 0;
    for (long i = 0; i < 500000000L; i++) {
        long picked = acc;  // pick 0 = dup
        acc = picked + 1;
    }
    printf("%ld\n", acc);
    return 0;
}
