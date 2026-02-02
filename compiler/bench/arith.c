// expected: 4294967040
#include <stdio.h>
int main() {
    long sum = 0;
    for (long i = 0; i < 1000000000; i++)
        sum += (i * 3 + 7) & 0xFF;
    printf("%ld\n", sum);
    return 0;
}
