// expected: 99999
#include <stdio.h>
#define N 100000

int left_child[N];
int right_child[N];
int key[N];
int node_count = 0;

void insert(int val) {
    if (node_count == 0) {
        key[0] = val;
        left_child[0] = -1;
        right_child[0] = -1;
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
                node_count++;
                return;
            }
            curr = right_child[curr];
        }
    }
}

int find_depth(int val) {
    int curr = 0;
    int depth = 0;
    while (curr >= 0) {
        if (key[curr] == val) return depth;
        if (val < key[curr]) {
            curr = left_child[curr];
        } else {
            curr = right_child[curr];
        }
        depth++;
    }
    return depth;
}

int lcg(int n) {
    return (n * 1103515245 + 12345) & 2147483647;
}

void build_tree() {
    node_count = 0;
    insert(50000);
    int seed = 12345;
    for (int i = 1; i < N; i++) {
        seed = lcg(seed);
        insert(seed % N);
    }
}

long search_all() {
    long sum = 0;
    int seed = 12345;
    for (int i = 1; i < N; i++) {
        seed = lcg(seed);
        sum += find_depth(seed % N);
    }
    return sum;
}

int main() {
    build_tree();
    printf("%ld\n", search_all());
    return 0;
}
