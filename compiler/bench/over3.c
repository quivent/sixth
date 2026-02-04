// expected: 300000000
// Over on depth 3, 300M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1, c = 2;
    for (long i = 0; i < 300000000L; i++) {
        long over = a;  // over from depth 3
        a = over + 1;
    }
    (void)b;
    (void)c;
    printf("%ld\n", a);
    return 0;
}
