// expected: 100000000
// Nested loop using J, 10K x 10K
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long j = 0; j < 10000; j++) {
        for (long i = 0; i < 10000; i++) {
            volatile long tmp = j + i; // prevent optimization
            (void)tmp;
            sum++;
        }
    }
    printf("%ld\n", sum);
    return 0;
}
