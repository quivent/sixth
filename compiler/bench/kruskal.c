// expected: 4458
#include <stdio.h>
#define NODES 500
#define EDGES 500

int parent[NODES];
int rnk[NODES];
int edges[EDGES][3];  // from, to, weight

int find(int x) {
    while (parent[x] != x) x = parent[x];
    return x;
}

int unite(int x, int y) {
    int rx = find(x), ry = find(y);
    if (rx == ry) return 0;
    if (rnk[rx] < rnk[ry]) {
        parent[rx] = ry;
    } else if (rnk[rx] > rnk[ry]) {
        parent[ry] = rx;
    } else {
        parent[ry] = rx;
        rnk[rx]++;
    }
    return 1;
}

void init_uf() {
    for (int i = 0; i < NODES; i++) {
        parent[i] = i;
        rnk[i] = 0;
    }
}

void init_edges() {
    for (int i = 0; i < EDGES; i++) {
        edges[i][0] = i % (NODES - 1);
        edges[i][1] = i % (NODES - 1) + 1;
        edges[i][2] = (i % 17) + 1;
    }
}

void sort_edges() {
    for (int i = 1; i < EDGES; i++) {
        for (int j = 0; j < i; j++) {
            if (edges[j][2] > edges[j+1][2]) {
                int t0 = edges[j][0], t1 = edges[j][1], t2 = edges[j][2];
                edges[j][0] = edges[j+1][0];
                edges[j][1] = edges[j+1][1];
                edges[j][2] = edges[j+1][2];
                edges[j+1][0] = t0;
                edges[j+1][1] = t1;
                edges[j+1][2] = t2;
            }
        }
    }
}

int kruskal() {
    init_uf();
    int count = 0, weight = 0;
    for (int i = 0; i < EDGES && count < NODES - 1; i++) {
        if (unite(edges[i][0], edges[i][1])) {
            count++;
            weight += edges[i][2];
        }
    }
    return weight;
}

int main() {
    init_edges();
    sort_edges();
    printf("%d\n", kruskal());
    return 0;
}
