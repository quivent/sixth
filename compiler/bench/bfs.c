// expected: 1000
#include <stdio.h>
#define N 1000
int adj[N][10];
int deg[N];
char visited[N];
int queue[N];

void init_graph() {
    for (int i = 0; i < N; i++) deg[i] = 0;
    for (int i = 1; i < N; i++) {
        adj[i][deg[i]++] = i - 1;
        adj[i-1][deg[i-1]++] = i;
        if (i % 7 == 0) {
            adj[i][deg[i]++] = i / 7;
            adj[i/7][deg[i/7]++] = i;
        }
    }
}

int bfs(int start) {
    for (int i = 0; i < N; i++) visited[i] = 0;
    int qhead = 0, qtail = 0;
    visited[start] = 1;
    queue[qtail++] = start;
    int count = 1;
    while (qhead < qtail) {
        int node = queue[qhead++];
        for (int i = 0; i < deg[node]; i++) {
            int neighbor = adj[node][i];
            if (!visited[neighbor]) {
                visited[neighbor] = 1;
                queue[qtail++] = neighbor;
                count++;
            }
        }
    }
    return count;
}

int main() {
    init_graph();
    printf("%d\n", bfs(0));
    return 0;
}
