// expected: 500000000
// +LOOP step 2, 500M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000000000; i += 2) {
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
