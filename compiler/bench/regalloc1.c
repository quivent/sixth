// expected: 3150000000
// Register pressure - multiple live values
#include <stdio.h>

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += 7 + 14 + 7 + (i % 8);
    }
    printf("%ld\n", sum);
    return 0;
}
