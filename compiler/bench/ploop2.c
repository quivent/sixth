// expected: 333333334
// +LOOP step 3, 333M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000000000; i += 3) {
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
