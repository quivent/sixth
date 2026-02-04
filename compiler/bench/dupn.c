// expected: 300000000
// Dup at various depths, 300M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1, c = 2, d = 3;
    for (long i = 0; i < 300000000L; i++) {
        long tmp = d;  // dup top
        (void)tmp;
        tmp = c;       // dup second
        (void)tmp;
        a = a + 1;
    }
    (void)b;
    (void)c;
    (void)d;
    printf("%ld\n", a);
    return 0;
}
