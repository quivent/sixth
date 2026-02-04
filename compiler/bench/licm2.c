// expected: 100000000
// Hoist computation out of loop
#include <stdio.h>

int main() {
    long a = 3, b = 7;
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        long x = a + b;  // loop invariant
        if (x == 10) sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
