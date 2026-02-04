// expected: 3628800
#include <stdio.h>
int perm[10];
int used[10];
long perm_count = 0;

void permute(int depth) {
    if (depth == 10) { perm_count++; return; }
    for (int i = 0; i < 10; i++) {
        if (!used[i]) {
            perm[depth] = i;
            used[i] = 1;
            permute(depth + 1);
            used[i] = 0;
        }
    }
}

int main() {
    for (int i = 0; i < 10; i++) used[i] = 0;
    permute(0);
    printf("%ld\n", perm_count);
    return 0;
}
