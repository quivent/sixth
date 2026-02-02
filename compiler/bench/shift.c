// expected: 1
#include <stdio.h>
int main() {
    long x = 1;
    for (long i = 0; i < 1000000000; i++) {
        long tmp = x << 1;
        tmp = tmp >> 1;
    }
    printf("%ld\n", x);
    return 0;
}
