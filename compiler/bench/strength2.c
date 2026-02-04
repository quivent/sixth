// expected: 16666666
// Division strength reduction - div by constant
// GCC converts /3 to multiply by reciprocal
#include <stdio.h>

static long div3(long n) { return n / 3; }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += div3(i);
    }
    printf("%ld\n", sum / 100000000);
    return 0;
}
