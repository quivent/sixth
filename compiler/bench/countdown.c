// expected: 1000000000
// 1- dup 0> while, 1B iterations - tests loop elimination
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 1000000000;
    while (1) {
        i--;
        sum++;
        if (!(i > 0)) break;
    }
    printf("%ld\n", sum);
    return 0;
}
