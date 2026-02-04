// expected: 500000000
// Rot on 3 items, 500M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1, c = 2;
    long tmp;
    for (long i = 0; i < 500000000L; i++) {
        // rot: a b c -> b c a
        tmp = a; a = b; b = c; c = tmp;
        // rot: b c a -> c a b
        tmp = a; a = b; b = c; c = tmp;
        // rot: c a b -> a b c
        tmp = a; a = b; b = c; c = tmp;
        a = a + 1;
    }
    printf("%ld\n", a);
    return 0;
}
