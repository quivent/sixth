// expected: 1000000000
// BEGIN/WHILE/REPEAT, 1B iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 0;
    while (i < 1000000000) {
        sum++;
        i++;
    }
    printf("%ld\n", sum);
    return 0;
}
