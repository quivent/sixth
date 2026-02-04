// expected: 99999
#include <stdio.h>
#define N 100000
#define D 10000

int left_child[N];
int right_child[N];
int key[N];
char deleted[N];
int node_count = 0;

void insert(int val) {
    if (node_count == 0) {
        key[0] = val;
        left_child[0] = -1;
        right_child[0] = -1;
        deleted[0] = 0;
        node_count = 1;
        return;
    }
    int curr = 0;
    while (1) {
        if (val < key[curr]) {
            if (left_child[curr] < 0) {
                left_child[curr] = node_count;
                key[node_count] = val;
                left_child[node_count] = -1;
                right_child[node_count] = -1;
                deleted[node_count] = 0;
                node_count++;
                return;
            }
            curr = left_child[curr];
        } else {
            if (right_child[curr] < 0) {
                right_child[curr] = node_count;
                key[node_count] = val;
                left_child[node_count] = -1;
                right_child[node_count] = -1;
                deleted[node_count] = 0;
                node_count++;
                return;
            }
            curr = right_child[curr];
        }
    }
}

void mark_delete(int val) {
    int curr = 0;
    while (curr >= 0) {
        if (key[curr] == val) {
            deleted[curr] = 1;
            return;
        }
        if (val < key[curr]) {
            curr = left_child[curr];
        } else {
            curr = right_child[curr];
        }
    }
}

int lcg(int n) {
    return (n * 1103515245 + 12345) & 2147483647;
}

void build_tree() {
    node_count = 0;
    for (int i = 0; i < N; i++) insert(i);
}

void delete_some() {
    int seed = 12345;
    for (int i = 0; i < D; i++) {
        seed = lcg(seed);
        mark_delete(seed % N);
    }
}

int count_remaining() {
    int count = 0;
    for (int i = 0; i < N; i++) {
        if (!deleted[i]) count++;
    }
    return count;
}

int main() {
    build_tree();
    delete_some();
    printf("%d\n", count_remaining());
    return 0;
}
