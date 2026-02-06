// Count to 1 billion benchmark - tight loop
#include <stdio.h>

int main() {
    long sum = 0;
    for (long i = 0; i < 1000000000L; i++) {
        sum += i & 1;
    }
    printf("%ld\n", sum);
    return 0;
}
