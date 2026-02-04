\ expect: done
\ category: regression
\ reason: stress string literal handling - self-hosting crash triggered by many s" literals

: s00 s" string constant number zero " drop drop ;
: s01 s" string constant number one " drop drop ;
: s02 s" string constant number two " drop drop ;
: s03 s" string constant number three " drop drop ;
: s04 s" string constant number four " drop drop ;
: s05 s" string constant number five " drop drop ;
: s06 s" string constant number six " drop drop ;
: s07 s" string constant number seven " drop drop ;
: s08 s" string constant number eight " drop drop ;
: s09 s" string constant number nine " drop drop ;
: s10 s" string constant number ten " drop drop ;
: s11 s" string constant number eleven " drop drop ;
: s12 s" string constant number twelve " drop drop ;
: s13 s" string constant number thirteen " drop drop ;
: s14 s" string constant number fourteen " drop drop ;
: s15 s" string constant number fifteen " drop drop ;
: s16 s" string constant number sixteen " drop drop ;
: s17 s" string constant number seventeen " drop drop ;
: s18 s" string constant number eighteen " drop drop ;
: s19 s" string constant number nineteen " drop drop ;
: s20 s" string constant number twenty " drop drop ;
: s21 s" string constant number twenty-one " drop drop ;
: s22 s" string constant number twenty-two " drop drop ;
: s23 s" string constant number twenty-three " drop drop ;
: s24 s" string constant number twenty-four " drop drop ;
: s25 s" string constant number twenty-five " drop drop ;
: s26 s" string constant number twenty-six " drop drop ;
: s27 s" string constant number twenty-seven " drop drop ;
: s28 s" string constant number twenty-eight " drop drop ;
: s29 s" string constant number twenty-nine " drop drop ;
: s30 s" string constant number thirty " drop drop ;
: s31 s" string constant number thirty-one " drop drop ;
: s32 s" string constant number thirty-two " drop drop ;
: s33 s" string constant number thirty-three " drop drop ;
: s34 s" string constant number thirty-four " drop drop ;
: s35 s" string constant number thirty-five " drop drop ;
: s36 s" string constant number thirty-six " drop drop ;
: s37 s" string constant number thirty-seven " drop drop ;
: s38 s" string constant number thirty-eight " drop drop ;
: s39 s" string constant number thirty-nine " drop drop ;
: s40 s" string constant number forty " drop drop ;
: s41 s" string constant number forty-one " drop drop ;
: s42 s" string constant number forty-two " drop drop ;
: s43 s" string constant number forty-three " drop drop ;
: s44 s" string constant number forty-four " drop drop ;
: s45 s" string constant number forty-five " drop drop ;
: s46 s" string constant number forty-six " drop drop ;
: s47 s" string constant number forty-seven " drop drop ;
: s48 s" string constant number forty-eight " drop drop ;
: s49 s" string constant number forty-nine " drop drop ;
: s50 s" string constant number fifty " drop drop ;
: s51 s" string constant number fifty-one " drop drop ;
: s52 s" string constant number fifty-two " drop drop ;
: s53 s" string constant number fifty-three " drop drop ;
: s54 s" string constant number fifty-four " drop drop ;
: s55 s" string constant number fifty-five " drop drop ;
: s56 s" string constant number fifty-six " drop drop ;
: s57 s" string constant number fifty-seven " drop drop ;
: s58 s" string constant number fifty-eight " drop drop ;
: s59 s" string constant number fifty-nine " drop drop ;
: s60 s" string constant number sixty " drop drop ;
: s61 s" string constant number sixty-one " drop drop ;
: s62 s" string constant number sixty-two " drop drop ;
: s63 s" string constant number sixty-three " drop drop ;
: s64 s" string constant number sixty-four " drop drop ;
: s65 s" string constant number sixty-five " drop drop ;
: s66 s" string constant number sixty-six " drop drop ;
: s67 s" string constant number sixty-seven " drop drop ;
: s68 s" string constant number sixty-eight " drop drop ;
: s69 s" string constant number sixty-nine " drop drop ;
: s70 s" string constant number seventy " drop drop ;
: s71 s" string constant number seventy-one " drop drop ;
: s72 s" string constant number seventy-two " drop drop ;
: s73 s" string constant number seventy-three " drop drop ;
: s74 s" string constant number seventy-four " drop drop ;
: s75 s" string constant number seventy-five " drop drop ;
: s76 s" string constant number seventy-six " drop drop ;
: s77 s" string constant number seventy-seven " drop drop ;
: s78 s" string constant number seventy-eight " drop drop ;
: s79 s" string constant number seventy-nine " drop drop ;
: s80 s" string constant number eighty " drop drop ;
: s81 s" string constant number eighty-one " drop drop ;
: s82 s" string constant number eighty-two " drop drop ;
: s83 s" string constant number eighty-three " drop drop ;
: s84 s" string constant number eighty-four " drop drop ;
: s85 s" string constant number eighty-five " drop drop ;
: s86 s" string constant number eighty-six " drop drop ;
: s87 s" string constant number eighty-seven " drop drop ;
: s88 s" string constant number eighty-eight " drop drop ;
: s89 s" string constant number eighty-nine " drop drop ;
: s90 s" string constant number ninety " drop drop ;
: s91 s" string constant number ninety-one " drop drop ;
: s92 s" string constant number ninety-two " drop drop ;
: s93 s" string constant number ninety-three " drop drop ;
: s94 s" string constant number ninety-four " drop drop ;
: s95 s" string constant number ninety-five " drop drop ;
: s96 s" string constant number ninety-six " drop drop ;
: s97 s" string constant number ninety-seven " drop drop ;
: s98 s" string constant number ninety-eight " drop drop ;
: s99 s" string constant number ninety-nine " drop drop ;

: main
  s00 s01 s02 s03 s04 s05 s06 s07 s08 s09
  s10 s11 s12 s13 s14 s15 s16 s17 s18 s19
  s20 s21 s22 s23 s24 s25 s26 s27 s28 s29
  s30 s31 s32 s33 s34 s35 s36 s37 s38 s39
  s40 s41 s42 s43 s44 s45 s46 s47 s48 s49
  s50 s51 s52 s53 s54 s55 s56 s57 s58 s59
  s60 s61 s62 s63 s64 s65 s66 s67 s68 s69
  s70 s71 s72 s73 s74 s75 s76 s77 s78 s79
  s80 s81 s82 s83 s84 s85 s86 s87 s88 s89
  s90 s91 s92 s93 s94 s95 s96 s97 s98 s99
  s" done" type ;
