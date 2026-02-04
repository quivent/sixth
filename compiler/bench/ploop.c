// expected: 166666668
#include <stdio.h>
int main() {
    long sum = 0;
    for (long i = 0; i < 1000000000; i += 3) {
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
