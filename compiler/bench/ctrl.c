// expected: 500000000
#include <stdio.h>
int main() {
    long count = 0;
    for (long i = 0; i < 1000000000; i++)
        if (i & 1) count++;
    printf("%ld\n", count);
    return 0;
}
