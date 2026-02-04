// expected: 142857143
// +LOOP step 7, 143M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000000000; i += 7) {
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
