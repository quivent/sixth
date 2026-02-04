// expected: 1023332
#include <stdio.h>
#define N 100000

int parent[N];
int depth[N];

void init_tree(void) {
    parent[0] = 0;
    depth[0] = 0;
    for (int i = 1; i < N; i++) {
        parent[i] = (i - 1) / 2;
        depth[i] = depth[parent[i]] + 1;
    }
}

int lca(int a, int b) {
    while (depth[a] > depth[b]) a = parent[a];
    while (depth[b] > depth[a]) b = parent[b];
    while (a != b) {
        a = parent[a];
        b = parent[b];
    }
    return a;
}

unsigned int lcg(unsigned int n) {
    return (n * 1103515245u + 12345u) & 0x7fffffffu;
}

int main(void) {
    init_tree();
    long sum = 0;
    unsigned int seed = 12345;
    for (int i = 0; i < N; i++) {
        seed = lcg(seed);
        int a = (int)(seed % N);
        seed = lcg(seed);
        int b = (int)(seed % N);
        sum += lca(a, b);
    }
    printf("%ld\n", sum);
    return 0;
}
