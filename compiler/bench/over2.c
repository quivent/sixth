// expected: 500000000
// Over on depth 2, 500M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1;
    for (long i = 0; i < 500000000L; i++) {
        long over = a;  // over
        a = over + 1;
    }
    (void)b;
    printf("%ld\n", a);
    return 0;
}
