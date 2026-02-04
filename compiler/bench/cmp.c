// expected: 750000000
#include <stdio.h>
int main() {
    long sum = 0;
    for (long i = 0; i < 1000000000; i++) {
        if ((i & 3) == 0) sum++;
        if ((i & 4) > 0) sum++;
        if (i < 500000000) sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
