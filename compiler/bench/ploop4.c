// expected: 100000000
// +LOOP variable step (1 + i/100000000), 100M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000000000; ) {
        sum++;
        i += 1 + i / 100000000;
    }
    printf("%ld\n", sum);
    return 0;
}
