// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)
//
// This program only needs to handle arguments that satisfy
// R0 >= 0, R1 >= 0, and R0*R1 < 32768.

// Put your code here.

// r0 = RAM[0]
// r1 = RAM[1]
// i = 0
// res = 0
// 
// (times = smaller number among r0 and r1)
//
// if r0 - r1 < 0:
//      n = r0
//      times = r1
//      goto LOOP
//
//
// LOOP:
// if i == n: (n - i == 0)
//      goto STOP
// res += r0
//
// STOP:
// RAM[2] = res

//@R2
//M = 0
//@i
//M = 0
//@res
//M = 0
//
//@R0
//D = M
//@R1
//D = D - M
//@R0_GREATER_R1
//D; JLT
//
//@R0
//D = M
//@n
//M = D
//
//@R1
//D = M
//@times
//M = D
//@LOOP
//0; JMP 
//
//(R0_GREATER_R1)
//    @R1
//    D = M
//    @n
//    M = D
//
//    @R0
//    D = M
//    @times
//    M = D
//
//(LOOP)
//    @times
//    D = M
//    @i
//    D = D - M
//    @STOP
//    D; JEQ
//
//    @n
//    D = M
//    @res
//    D = D + M  // PROBLEM???
//    @res
//    M = D
//
//    @i
//    M = M + 1
//    @LOOP
//    0; JMP
//
//(STOP)
//    @res
//    D = M
//    @R2
//    M = D
//
//(END)
//    @END
//    0; JMP
//



// VERSION 2
//@R0
//D = M
//@n
//M = D
//@R1
//D = M
//@m
//M = D
//@R2
//M = 0
////@i
////M = 0
//@res
//M = 0
//
//@mask
//M = 1
//
//(LOOP)
//    //@n
//    //D = M
//    //@i
//    //D = M - D
//    //@STOP
//    //D; JEQ
//
//    @mask
//    D = M
//    @STOP  //Stop loop if all bits in mask are 0
//    D; JEQ
//    
//    @n
//    D = D & M
//    // if D > 0, then res += m
//    @POSITIVE
//    D; JNE
//    
//
//// mask shift
//// m double
//(ITERATE)
//    @m
//    D = M
//    M = M + D
//    @mask
//    D = M
//    M = M + D
//    @LOOP
//    0; JMP
//
//(POSITIVE)
//    @m
//    D = M
//    @res
//    M = M + D
//    @ITERATE
//    0; JMP
//
//(STOP)
//    @res
//    D = M
//    @R2
//    M = D
//
//(END)
//    @END
//    0; JMP


// VERSION 3
@R2
M = 0
@mask
M = 1

(LOOP)
    @mask
    D = M
    @R0
    D = M - D   // Stop, if mask is greater than R0
    @END  //Stop loop if all bits in mask are 0
    D; JLT
    
    @mask
    D = M
    @R0
    D = D & M
    // if D > 0, then res += m
    @POSITIVE
    D; JNE
    

// mask shift
// m double
(ITERATE)
    @R1
    D = M
    M = M + D
    @mask
    D = M
    M = M + D
    @LOOP
    0; JMP

(POSITIVE)
    @R1
    D = M
    @R2
    M = M + D
    @ITERATE
    0; JMP

(END)
    @END
    0; JMP