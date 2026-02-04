// expected: 262143
#include <stdio.h>
long treecount(long depth) {
    if (depth == 0) return 1;
    return treecount(depth - 1) + treecount(depth - 1) + 1;
}

int main() {
    for (int i = 0; i < 1000; i++) {
        treecount(17);
    }
    printf("%ld\n", treecount(17));
    return 0;
}
