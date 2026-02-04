// expected: 499
#include <stdio.h>
#define N 500
#define INF 1000000

int adj[N][4];
int weight[N][4];
int deg[N];
int key[N];
char inMST[N];

void edge(int from, int to, int w) {
    adj[from][deg[from]] = to;
    weight[from][deg[from]] = w;
    deg[from]++;
}

void init_graph() {
    for (int i = 0; i < N; i++) deg[i] = 0;
    for (int i = 1; i < N; i++) {
        edge(i, i - 1, 1);
        edge(i - 1, i, 1);
    }
}

int find_min() {
    int minNode = -1, minKey = INF;
    for (int i = 0; i < N; i++) {
        if (!inMST[i] && key[i] < minKey) {
            minKey = key[i];
            minNode = i;
        }
    }
    return minNode;
}

int prim() {
    for (int i = 0; i < N; i++) {
        key[i] = INF;
        inMST[i] = 0;
    }
    key[0] = 0;
    int mst_weight = 0;
    for (int iter = 0; iter < N; iter++) {
        int u = find_min();
        if (u < 0) break;
        mst_weight += key[u];
        inMST[u] = 1;
        for (int i = 0; i < deg[u]; i++) {
            int v = adj[u][i];
            int w = weight[u][i];
            if (!inMST[v] && w < key[v]) {
                key[v] = w;
            }
        }
    }
    return mst_weight;
}

int main() {
    init_graph();
    printf("%d\n", prim());
    return 0;
}
