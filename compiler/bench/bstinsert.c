// expected: 4999950000
#include <stdio.h>
#define N 100000

int left_child[N];
int right_child[N];
int key[N];
int root = 0;
int node_count = 0;
long depth_sum = 0;

void insert(int val) {
    if (node_count == 0) {
        key[0] = val;
        left_child[0] = -1;
        right_child[0] = -1;
        node_count = 1;
        root = 0;
        depth_sum = 0;
        return;
    }
    int curr = root;
    int depth = 0;
    while (1) {
        depth++;
        if (val < key[curr]) {
            if (left_child[curr] < 0) {
                left_child[curr] = node_count;
                key[node_count] = val;
                left_child[node_count] = -1;
                right_child[node_count] = -1;
                node_count++;
                depth_sum += depth;
                return;
            }
            curr = left_child[curr];
        } else {
            if (right_child[curr] < 0) {
                right_child[curr] = node_count;
                key[node_count] = val;
                left_child[node_count] = -1;
                right_child[node_count] = -1;
                node_count++;
                depth_sum += depth;
                return;
            }
            curr = right_child[curr];
        }
    }
}

int lcg(int n) {
    return (n * 1103515245 + 12345) & 2147483647;
}

void build_tree() {
    node_count = 0;
    depth_sum = 0;
    insert(50000);
    int seed = 12345;
    for (int i = 1; i < N; i++) {
        seed = lcg(seed);
        insert(seed % N);
    }
}

int main() {
    build_tree();
    printf("%ld\n", depth_sum);
    return 0;
}
