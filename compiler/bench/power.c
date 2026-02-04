// expected: 1073741824
#include <stdio.h>
long power(long base, long exp) {
    if (exp == 0) return 1;
    if (exp & 1) {
        return base * power(base, exp - 1);
    } else {
        long half = power(base, exp / 2);
        return half * half;
    }
}

int main() {
    for (long i = 0; i < 10000000; i++) {
        power(2, 30);
    }
    printf("%ld\n", power(2, 30));
    return 0;
}
