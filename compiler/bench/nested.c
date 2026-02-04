// expected: 4999999950000000
#include <stdio.h>
int main() {
    long sum = 0;
    for (long j = 0; j < 100000; j++) {
        for (long i = 0; i < 100000; i++) {
            sum += j + i;
        }
    }
    printf("%ld\n", sum);
    return 0;
}
