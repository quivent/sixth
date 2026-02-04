// expected: 17
#include <stdio.h>
long max(long a, long b) { return a > b ? a : b; }

long treedepth(long depth) {
    if (depth == 0) return 0;
    return max(treedepth(depth - 1), treedepth(depth - 1)) + 1;
}

int main() {
    for (int i = 0; i < 1000; i++) {
        treedepth(17);
    }
    printf("%ld\n", treedepth(17));
    return 0;
}
