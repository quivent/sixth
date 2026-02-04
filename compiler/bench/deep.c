// expected: 4500000000
#include <stdio.h>
int main() {
    long a = 0, b = 1, c = 2, d = 3, e = 4;
    for (long i = 0; i < 500000000; i++) {
        // 5 pick 4 pick 3 pick 2 pick over + + + + +
        long sum = a + b + c + d + e;
        e = sum + 5;
    }
    printf("%ld\n", a + b + c + d + e);
    return 0;
}
