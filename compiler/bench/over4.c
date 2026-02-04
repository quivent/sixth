// expected: 200000000
// Over on depth 4, 200M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1, c = 2, d = 3;
    for (long i = 0; i < 200000000L; i++) {
        long over = a;  // over from depth 4
        a = over + 1;
    }
    (void)b;
    (void)c;
    (void)d;
    printf("%ld\n", a);
    return 0;
}
