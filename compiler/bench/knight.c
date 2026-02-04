// expected: 336000
// Knight's tour - count valid knight moves
#include <stdio.h>
#include <string.h>

#define SIZE 8

char board[SIZE * SIZE];

int b(int x, int y) { return board[y * SIZE + x]; }
void bs(int val, int x, int y) { board[y * SIZE + x] = val; }

int in_bounds(int x, int y) {
    return x >= 0 && x < SIZE && y >= 0 && y < SIZE;
}

int dx[] = {2, 1, -1, -2, -2, -1, 1, 2};
int dy[] = {1, 2, 2, 1, -1, -2, -2, -1};

int count_valid_moves(int x, int y) {
    int count = 0;
    for (int i = 0; i < 8; i++) {
        int nx = x + dx[i];
        int ny = y + dy[i];
        if (in_bounds(nx, ny) && !b(nx, ny)) {
            count++;
        }
    }
    return count;
}

void init_board(void) {
    memset(board, 0, SIZE * SIZE);
}

int main(void) {
    long sum = 0;
    for (int iter = 0; iter < 1000; iter++) {
        init_board();
        for (int y = 0; y < SIZE; y++) {
            for (int x = 0; x < SIZE; x++) {
                sum += count_valid_moves(x, y);
            }
        }
    }
    printf("%ld\n", sum);
    return 0;
}
