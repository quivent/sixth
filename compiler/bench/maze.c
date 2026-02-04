// expected: 1000
// Maze solver using flood fill
#include <stdio.h>
#include <string.h>

#define SIZE 16

char maze[SIZE * SIZE];
char visited[SIZE * SIZE];

int m(int x, int y) { return maze[y * SIZE + x]; }
void ms(int val, int x, int y) { maze[y * SIZE + x] = val; }
int v(int x, int y) { return visited[y * SIZE + x]; }
void vs(int val, int x, int y) { visited[y * SIZE + x] = val; }

int in_bounds(int x, int y) {
    return x >= 0 && x < SIZE && y >= 0 && y < SIZE;
}

void init_maze(void) {
    memset(maze, 0, SIZE * SIZE);
    memset(visited, 0, SIZE * SIZE);
    for (int i = 0; i < SIZE; i++) ms(1, i, 0);
    for (int i = 0; i < SIZE; i++) ms(1, i, SIZE - 1);
    for (int i = 0; i < SIZE; i++) ms(1, 0, i);
    for (int i = 0; i < SIZE; i++) ms(1, SIZE - 1, i);
}

int solve(int x, int y, int gx, int gy) {
    if (x == gx && y == gy) return 1;
    if (!in_bounds(x, y)) return 0;
    if (!m(x, y)) return 0;
    if (v(x, y)) return 0;
    vs(1, x, y);
    if (solve(x + 1, y, gx, gy)) return 1;
    if (solve(x - 1, y, gx, gy)) return 1;
    if (solve(x, y + 1, gx, gy)) return 1;
    if (solve(x, y - 1, gx, gy)) return 1;
    return 0;
}

int run_maze(void) {
    memset(visited, 0, SIZE * SIZE);
    return solve(0, 0, SIZE - 1, SIZE - 1);
}

int main(void) {
    int count = 0;
    for (int i = 0; i < 1000; i++) {
        init_maze();
        if (run_maze()) count++;
    }
    printf("%d\n", count);
    return 0;
}
