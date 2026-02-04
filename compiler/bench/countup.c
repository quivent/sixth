// expected: 1000000000
// 1+ dup limit < while, 1B iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 0;
    while (1) {
        i++;
        sum++;
        if (!(i < 1000000000)) break;
    }
    printf("%ld\n", sum);
    return 0;
}
