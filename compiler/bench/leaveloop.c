// expected: 100000000
// DO/LOOP with LEAVE, 100M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 200000000; i++) {
        sum++;
        if (i > 99999999) break;
    }
    printf("%ld\n", sum);
    return 0;
}
