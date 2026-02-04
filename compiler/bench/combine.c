// expected: 184756
#include <stdio.h>
long comb_count = 0;

void combine(int n, int k, int start) {
    if (k == 0) { comb_count++; return; }
    if (n - start + 1 < k) return;
    for (int i = start + 1; i <= n; i++) {
        combine(n, k - 1, i);
    }
}

int main() {
    combine(20, 10, 0);
    printf("%ld\n", comb_count);
    return 0;
}
