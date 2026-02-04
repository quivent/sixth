// expected: 1000000000
// Nested DO/LOOP 3 deep, 1K x 1K x 1K
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000; i++) {
        for (long j = 0; j < 1000; j++) {
            for (long k = 0; k < 1000; k++) {
                sum++;
            }
        }
    }
    printf("%ld\n", sum);
    return 0;
}
