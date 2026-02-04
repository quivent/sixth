// expected: 99999
#include <stdio.h>
#define N 100000

int parent[N];
int rnk[N];

int find(int x) {
    while (parent[x] != x) x = parent[x];
    return x;
}

void unite(int x, int y) {
    int rx = find(x), ry = find(y);
    if (rx == ry) return;
    if (rnk[rx] < rnk[ry]) {
        parent[rx] = ry;
    } else if (rnk[rx] > rnk[ry]) {
        parent[ry] = rx;
    } else {
        parent[ry] = rx;
        rnk[rx]++;
    }
}

void init_uf() {
    for (int i = 0; i < N; i++) {
        parent[i] = i;
        rnk[i] = 0;
    }
}

int lcg(int n) {
    return (n * 1103515245 + 12345) & 2147483647;
}

void do_unions() {
    int seed = 12345;
    for (int i = 0; i < N; i++) {
        seed = lcg(seed);
        int a = seed % N;
        seed = lcg(seed);
        int b = seed % N;
        unite(a, b);
    }
}

int count_roots() {
    int count = 0;
    for (int i = 0; i < N; i++) {
        if (parent[i] == i) count++;
    }
    return count;
}

int main() {
    init_uf();
    do_unions();
    printf("%d\n", count_roots());
    return 0;
}
