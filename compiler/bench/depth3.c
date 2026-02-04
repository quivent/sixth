// expected: 1000000000
// Stack depth 3 throughout, 1B ops
#include <stdio.h>

int main(void) {
    long acc = 0;
    long d1 = 0;
    long d2 = 0;
    for (long i = 0; i < 1000000000L; i++) {
        acc = acc + 1;
    }
    (void)d1;
    (void)d2;
    printf("%ld\n", acc);
    return 0;
}
