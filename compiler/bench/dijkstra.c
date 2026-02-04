// expected: 67063
#include <stdio.h>
#define N 500
#define INF 2147483647
long dist[N];
char done[N];
int adj[N][10];
int weight[N][10];
int deg[N];

void edge(int from, int to, int w) {
    adj[from][deg[from]] = to;
    weight[from][deg[from]] = w;
    deg[from]++;
}

void init_graph() {
    for (int i = 0; i < N; i++) deg[i] = 0;
    for (int i = 1; i < N; i++) {
        edge(i, i - 1, (i % 17) + 1);
        edge(i - 1, i, (i % 17) + 1);
    }
    for (int i = 50; i < N; i++) {
        edge(i, i - 50, (i % 23) + 1);
        edge(i - 50, i, (i % 23) + 1);
    }
}

int find_min() {
    int minNode = -1;
    long minDist = INF;
    for (int i = 0; i < N; i++) {
        if (!done[i] && dist[i] < minDist) {
            minDist = dist[i];
            minNode = i;
        }
    }
    return minNode;
}

void dijkstra(int start) {
    for (int i = 0; i < N; i++) {
        dist[i] = INF;
        done[i] = 0;
    }
    dist[start] = 0;
    for (int iter = 0; iter < N; iter++) {
        int u = find_min();
        if (u < 0) break;
        done[u] = 1;
        for (int i = 0; i < deg[u]; i++) {
            int v = adj[u][i];
            long alt = dist[u] + weight[u][i];
            if (alt < dist[v]) {
                dist[v] = alt;
            }
        }
    }
}

int main() {
    init_graph();
    dijkstra(0);
    long sum = 0;
    for (int i = 0; i < N; i++) {
        if (dist[i] < INF) sum += dist[i];
    }
    printf("%ld\n", sum);
    return 0;
}
