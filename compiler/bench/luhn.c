// expected: 200000
// Luhn checksum benchmark - validate credit card numbers, run 100000 times

#include <stdio.h>
#include <string.h>

char testnum[17];

void init_num(void) {
    strcpy(testnum, "4532015112830366");
}

int digit_at(int i) {
    return testnum[i] - '0';
}

int luhn_check(void) {
    int sum = 0;
    for (int i = 0; i < 16; i++) {
        int d = digit_at(15 - i);
        if (i % 2) {
            d *= 2;
            if (d > 9) d -= 9;
        }
        sum += d;
    }
    return sum % 10 == 0;
}

int main(void) {
    init_num();
    int result = 0;
    for (int i = 0; i < 100000; i++) {
        if (luhn_check()) {
            result += 2;
        } else {
            result++;
        }
    }
    printf("%d\n", result);
    return 0;
}
