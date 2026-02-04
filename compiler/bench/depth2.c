// expected: 1000000000
// Stack depth 2 throughout, 1B ops
#include <stdio.h>

int main(void) {
    long acc = 0;
    long dummy = 0;
    for (long i = 0; i < 1000000000L; i++) {
        acc = acc + 1;
    }
    (void)dummy;
    printf("%ld\n", acc);
    return 0;
}
