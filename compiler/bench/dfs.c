// expected: 1000
#include <stdio.h>
#define N 1000
int adj[N][10];
int deg[N];
char visited[N];
int stack[N];

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

int dfs(int start) {
    for (int i = 0; i < N; i++) visited[i] = 0;
    int sp = 0;
    stack[sp++] = start;
    int count = 0;
    while (sp > 0) {
        int node = stack[--sp];
        if (!visited[node]) {
            visited[node] = 1;
            count++;
            for (int i = 0; i < deg[node]; i++) {
                int neighbor = adj[node][i];
                if (!visited[neighbor]) {
                    stack[sp++] = neighbor;
                }
            }
        }
    }
    return count;
}

int main() {
    init_graph();
    printf("%d\n", dfs(0));
    return 0;
}
