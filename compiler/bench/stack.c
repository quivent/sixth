// expected: 1500000000
#include <stdio.h>
int main() {
    long a = 0, b = 1, c = 2, t;
    for (long i = 0; i < 500000000; i++) {
        // rot: a b c -> b c a
        t = a; a = b; b = c; c = t;
        // + swap -: b c a -> b c+a -> c+a b -> c+a-b b
        c = c + a;
        t = b; b = c; c = t;
        a = c - b;
        // over +: restore balance
        a = a + b;
    }
    printf("%ld\n", a + b + c);
    return 0;
}
