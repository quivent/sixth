// expected: 4200000000
// Loop invariant code motion - hoist constant from loop
#include <stdio.h>

int main() {
    long base = 42;
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += base;
    }
    printf("%ld\n", sum);
    return 0;
}
