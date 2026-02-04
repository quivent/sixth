// expected: 50000000
// Mutually calling small functions - GCC inlines both
#include <stdio.h>

static int even_(long n) { return n % 2 == 0; }
static int odd_(long n) { return !even_(n); }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        if (even_(i)) sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
