// expected: 4851
// 99 bottles benchmark - sum bottle numbers, run 10000 times

#include <stdio.h>

int bottles_sum(void) {
    int sum = 0;
    for (int i = 1; i < 99; i++) {
        sum += i;
    }
    return sum;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 10000; i++) {
        result = bottles_sum();
    }
    printf("%d\n", result);
    return 0;
}
