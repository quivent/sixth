// expected: 19900
#include <stdio.h>
#define N 200
#define E 400
#define INF 1000000

int edges[E][3];  // from, to, weight
long dist[N];
int nedges;

void add_edge(int from, int to, int w) {
    edges[nedges][0] = from;
    edges[nedges][1] = to;
    edges[nedges][2] = w;
    nedges++;
}

void init_graph() {
    nedges = 0;
    for (int i = 1; i < N; i++) {
        add_edge(i - 1, i, 1);
    }
    for (int i = 50; i < N; i++) {
        add_edge(i, i - 50, 2);
    }
}

void bellman(int start) {
    for (int i = 0; i < N; i++) dist[i] = INF;
    dist[start] = 0;
    for (int iter = 1; iter < N; iter++) {
        for (int e = 0; e < nedges; e++) {
            int u = edges[e][0];
            int v = edges[e][1];
            int w = edges[e][2];
            if (dist[u] < INF && dist[u] + w < dist[v]) {
                dist[v] = dist[u] + w;
            }
        }
    }
}

int main() {
    init_graph();
    bellman(0);
    long sum = 0;
    for (int i = 0; i < N; i++) {
        if (dist[i] < INF) sum += dist[i];
    }
    printf("%ld\n", sum);
    return 0;
}
