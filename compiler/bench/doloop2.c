// expected: 499999999500000000
// DO/LOOP with i arithmetic, 1B iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 1000000000; i++) {
        sum += i;
    }
    printf("%ld\n", sum);
    return 0;
}
