// expected: 1000000000 (fits in 64-bit, no actual double-cell needed in C)
#include <stdio.h>
int main() {
    unsigned long long sum = 0;
    for (long i = 0; i < 1000000000; i++)
        sum += 1;
    printf("%llu\n", sum);
    return 0;
}
