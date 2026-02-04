// expected: 11111
// N-puzzle (8-puzzle) - count inversions to check solvability
#include <stdio.h>

#define SIZE 9

int puzzle[SIZE];

int count_inversions(void) {
    int count = 0;
    for (int i = 0; i < SIZE; i++) {
        for (int j = i + 1; j < SIZE; j++) {
            if (puzzle[i] > 0 && puzzle[j] > 0 && puzzle[i] > puzzle[j]) {
                count++;
            }
        }
    }
    return count;
}

int is_solvable(void) {
    return (count_inversions() & 1) == 0;
}

void init_puzzle(int seed) {
    for (int i = 0; i < SIZE; i++) puzzle[i] = i + 1;
    puzzle[SIZE - 1] = 0;

    int pos1 = seed % SIZE;
    int pos2 = (pos1 + 1) % SIZE;
    int tmp = puzzle[pos1];
    puzzle[pos1] = puzzle[pos2];
    puzzle[pos2] = tmp;
}

int main(void) {
    int count = 0;
    for (int i = 0; i < 100000; i++) {
        init_puzzle(i);
        if (is_solvable()) count++;
    }
    printf("%d\n", count);
    return 0;
}
