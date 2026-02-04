// expected: 1000000000
// Triple nested using I J, 1K x 1K x 1K
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long k = 0; k < 1000; k++) {
        for (long j = 0; j < 1000; j++) {
            for (long i = 0; i < 1000; i++) {
                volatile long tmp = j + i;
                (void)tmp;
                sum++;
            }
        }
    }
    printf("%ld\n", sum);
    return 0;
}
