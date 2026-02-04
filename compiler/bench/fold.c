// expected: 1000000000
#include <stdio.h>
int main() {
    long sum = 0;
    for (long i = 0; i < 1000000000; i++) {
        // This folds at compile time in C too
        int x = (3 + 4) * 2 - 5 + 1;  // = 10
        if (x == 10) sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
