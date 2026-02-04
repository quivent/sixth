// expected: 332833500
#include <stdio.h>
#define N 1000
int adj[N][5];
int deg[N];
int indegree[N];
int result[N];
int queue[N];

void edge(int from, int to) {
    adj[from][deg[from]++] = to;
    indegree[to]++;
}

void init_graph() {
    for (int i = 0; i < N; i++) {
        deg[i] = 0;
        indegree[i] = 0;
    }
    for (int i = 1; i < N; i++) {
        edge(i - 1, i);
    }
}

void toposort() {
    int qhead = 0, qtail = 0;
    for (int i = 0; i < N; i++) {
        if (indegree[i] == 0) {
            queue[qtail++] = i;
        }
    }
    int ridx = 0;
    while (qhead < qtail) {
        int node = queue[qhead++];
        result[ridx++] = node;
        for (int i = 0; i < deg[node]; i++) {
            int neighbor = adj[node][i];
            indegree[neighbor]--;
            if (indegree[neighbor] == 0) {
                queue[qtail++] = neighbor;
            }
        }
    }
}

int main() {
    init_graph();
    toposort();
    long sum = 0;
    for (int i = 0; i < N; i++) {
        sum += (long)result[i] * i;
    }
    printf("%ld\n", sum);
    return 0;
}
