// expected: 262143
#include <stdio.h>
long treebuild(long depth) {
    if (depth == 0) return 1;
    return treebuild(depth - 1) + treebuild(depth - 1) + 1;
}

int main() {
    printf("%ld\n", treebuild(17));
    return 0;
}
