// expected: 500000000
// WHILE with complex condition (i < limit AND i AND 1 = 0), 500M
#include <stdio.h>

int main(void) {
    long sum = 0;
    long i = 0;
    while (i < 1000000000 && (i & 1) == 0) {
        sum++;
        i += 2;
    }
    printf("%ld\n", sum);
    return 0;
}
