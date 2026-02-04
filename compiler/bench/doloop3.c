// expected: 1000000000
// Nested DO/LOOP 2 deep, 100K x 10K
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 100000; i++) {
        for (long j = 0; j < 10000; j++) {
            sum++;
        }
    }
    printf("%ld\n", sum);
    return 0;
}
