// expected: 1000000000
// UNTIL with 0= condition, 1B iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 1000000000;
    do {
        sum++;
        i--;
    } while (i != 0);
    printf("%ld\n", sum);
    return 0;
}
