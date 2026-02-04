// expected: 200000000
// Rot equivalent on 5 items, 200M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1, c = 2, d = 3, e = 4;
    long tmp;
    for (long i = 0; i < 200000000L; i++) {
        // rot bottom 3: a b c -> b c a
        tmp = a; a = b; b = c; c = tmp;
        // rot bottom 3: b c a -> c a b
        tmp = a; a = b; b = c; c = tmp;
        // rot bottom 3: c a b -> a b c
        tmp = a; a = b; b = c; c = tmp;
        a = a + 1;
    }
    (void)d;
    (void)e;
    printf("%ld\n", a);
    return 0;
}
