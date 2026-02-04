// expected: 500000000
// Swap pairs, 500M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1;
    long tmp;
    for (long i = 0; i < 500000000L; i++) {
        // swap
        tmp = a; a = b; b = tmp;
        // swap back
        tmp = a; a = b; b = tmp;
        a = a + 1;
    }
    (void)b;
    printf("%ld\n", a);
    return 0;
}
