// expected: 1000000000
// Simple DO/LOOP, 1B iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000000000; i++) {
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
