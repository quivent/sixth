// expected: 485
// Leap year benchmark - count leap years 1-2000, run 1000 times

#include <stdio.h>

int leap_year(int y) {
    if (y % 400 == 0) return 1;
    if (y % 100 == 0) return 0;
    return y % 4 == 0;
}

int count_leap(int limit) {
    int count = 0;
    for (int i = 1; i <= limit; i++) {
        if (leap_year(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 1000; i++) {
        result = count_leap(2000);
    }
    printf("%d\n", result);
    return 0;
}
