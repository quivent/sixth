// expected: 500
#include <stdio.h>
#define N 500
int adj[N][4];
int deg[N];
int disc[N], low[N];
char onstack[N];
int stack[N], sp;
int timer, scc_count;

void edge(int from, int to) {
    adj[from][deg[from]++] = to;
}

void init_graph() {
    for (int i = 0; i < N; i++) {
        deg[i] = 0;
        disc[i] = -1;
        onstack[i] = 0;
    }
    for (int i = 1; i < N; i++) {
        edge(i - 1, i);
    }
}

void tarjan_visit(int u) {
    disc[u] = low[u] = timer++;
    onstack[u] = 1;
    stack[sp++] = u;
    for (int i = 0; i < deg[u]; i++) {
        int v = adj[u][i];
        if (disc[v] < 0) {
            tarjan_visit(v);
            if (low[v] < low[u]) low[u] = low[v];
        } else if (onstack[v]) {
            if (disc[v] < low[u]) low[u] = disc[v];
        }
    }
    if (disc[u] == low[u]) {
        scc_count++;
        int w;
        do {
            w = stack[--sp];
            onstack[w] = 0;
        } while (w != u);
    }
}

void tarjan() {
    timer = 0;
    sp = 0;
    scc_count = 0;
    for (int i = 0; i < N; i++) {
        if (disc[i] < 0) tarjan_visit(i);
    }
}

int main() {
    init_graph();
    tarjan();
    printf("%d\n", scc_count);
    return 0;
}
