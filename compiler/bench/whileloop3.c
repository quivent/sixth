// expected: 1000000000
// Nested WHILE 2 deep, 100K x 10K
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 0;
    while (i < 100000) {
        long j = 0;
        while (j < 10000) {
            sum++;
            j++;
        }
        i++;
    }
    printf("%ld\n", sum);
    return 0;
}
