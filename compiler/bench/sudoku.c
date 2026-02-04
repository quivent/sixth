// expected: 100000
// Sudoku validator - check if a board is valid
#include <stdio.h>
#include <string.h>

char board[81];

int b(int row, int col) { return board[row * 9 + col]; }
void bs(int val, int row, int col) { board[row * 9 + col] = val; }

int check_row(int row) {
    int mask = 0;
    for (int i = 0; i < 9; i++) {
        int v = b(row, i);
        if (v > 0) {
            int bit = 1 << (v - 1);
            if (mask & bit) return 0;
            mask |= bit;
        }
    }
    return 1;
}

int check_col(int col) {
    int mask = 0;
    for (int i = 0; i < 9; i++) {
        int v = b(i, col);
        if (v > 0) {
            int bit = 1 << (v - 1);
            if (mask & bit) return 0;
            mask |= bit;
        }
    }
    return 1;
}

int check_box(int boxrow, int boxcol) {
    int mask = 0;
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            int v = b(boxrow + i, boxcol + j);
            if (v > 0) {
                int bit = 1 << (v - 1);
                if (mask & bit) return 0;
                mask |= bit;
            }
        }
    }
    return 1;
}

int valid_board(void) {
    for (int i = 0; i < 9; i++) if (!check_row(i)) return 0;
    for (int i = 0; i < 9; i++) if (!check_col(i)) return 0;
    for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
            if (!check_box(i * 3, j * 3)) return 0;
    return 1;
}

void init_board(void) {
    memset(board, 0, 81);
    bs(5,0,0); bs(3,0,1); bs(7,0,4);
    bs(6,1,0); bs(1,1,3); bs(9,1,4); bs(5,1,5);
    bs(9,2,1); bs(8,2,2); bs(6,2,7);
    bs(8,3,0); bs(6,3,4); bs(3,3,8);
    bs(4,4,0); bs(8,4,3); bs(3,4,5); bs(1,4,8);
    bs(7,5,0); bs(2,5,4); bs(6,5,8);
    bs(6,6,1); bs(2,6,6); bs(8,6,7);
    bs(4,7,3); bs(1,7,4); bs(9,7,5); bs(5,7,8);
    bs(8,8,4); bs(7,8,7); bs(9,8,8);
}

int main(void) {
    int count = 0;
    for (int i = 0; i < 100000; i++) {
        init_board();
        if (valid_board()) count++;
    }
    printf("%d\n", count);
    return 0;
}
