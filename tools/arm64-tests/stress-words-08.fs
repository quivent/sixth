\ stress-words-08.fs - Many small words vs few large words (50 tiny words)
\ expect: 50
: t01 1 ; : t02 1 ; : t03 1 ; : t04 1 ; : t05 1 ;
: t06 1 ; : t07 1 ; : t08 1 ; : t09 1 ; : t10 1 ;
: t11 1 ; : t12 1 ; : t13 1 ; : t14 1 ; : t15 1 ;
: t16 1 ; : t17 1 ; : t18 1 ; : t19 1 ; : t20 1 ;
: t21 1 ; : t22 1 ; : t23 1 ; : t24 1 ; : t25 1 ;
: t26 1 ; : t27 1 ; : t28 1 ; : t29 1 ; : t30 1 ;
: t31 1 ; : t32 1 ; : t33 1 ; : t34 1 ; : t35 1 ;
: t36 1 ; : t37 1 ; : t38 1 ; : t39 1 ; : t40 1 ;
: t41 1 ; : t42 1 ; : t43 1 ; : t44 1 ; : t45 1 ;
: t46 1 ; : t47 1 ; : t48 1 ; : t49 1 ; : t50 1 ;
: sum10a t01 t02 + t03 + t04 + t05 + t06 + t07 + t08 + t09 + t10 + ;
: sum10b t11 t12 + t13 + t14 + t15 + t16 + t17 + t18 + t19 + t20 + ;
: sum10c t21 t22 + t23 + t24 + t25 + t26 + t27 + t28 + t29 + t30 + ;
: sum10d t31 t32 + t33 + t34 + t35 + t36 + t37 + t38 + t39 + t40 + ;
: sum10e t41 t42 + t43 + t44 + t45 + t46 + t47 + t48 + t49 + t50 + ;
: main sum10a sum10b + sum10c + sum10d + sum10e + ;
