// expected: 100000000
// Nested DO/LOOP 4 deep, 100 x 100 x 100 x 100
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 100; i++) {
        for (long j = 0; j < 100; j++) {
            for (long k = 0; k < 100; k++) {
                for (long l = 0; l < 100; l++) {
                    sum++;
                }
            }
        }
    }
    printf("%ld\n", sum);
    return 0;
}
