// expected: 1000000000
// BEGIN/UNTIL, 1B iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 0;
    do {
        sum++;
        i++;
    } while (i < 1000000000);
    printf("%ld\n", sum);
    return 0;
}
