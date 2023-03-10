module Tests

open NUnit.Framework

open QuantumLanguage
open AST

type testResult =
        | Pass
        | Fail

[<SetUp>]
let Setup () =
    System.Globalization.CultureInfo.CurrentCulture <- new System.Globalization.CultureInfo("en-US")
    ()

[<Test>]
let ``Basic Allocation + Hadamard works`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc Q; Calloc C; H Q;"
    Assert.That(ast, Is.EqualTo (AllocQC(BitS "Q", BitS "C"), H (BitS "Q") ) )
      
[<Test>]   
let ``Allocation + Chain of gates`` () =
    let (ast, _) = Handler.ParseQuLang "Qalloc q; Calloc c; H q; X q; Measure q -> c;"
    Assert.That(ast, Is.EqualTo (AllocQC (BitS "q", BitS "c"),
                        Order (H (BitS "q"), Order (X (BitS "q"), Measure (BitS "q", BitS "c")))))
[<Test>]
let ``Empty Program treated as no Operation`` () =
    let (ast,_) = Handler.ParseQuLang ""
    Assert.That(ast, Is.EqualTo (NOP, NOP) )

[<Test>]
let ``Complex Quantum Code (if, allocation, Z, Y, S, I)`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc q[5]; Calloc c[2]; Z q[1]; Y q[2]; S a;
                                    If ( c |> Click ) ID x;"
    Assert.That(ast,
       Is.EqualTo (
        AllocQC (BitA ("q", 5), BitA ("c", 2)),
         Order(Z (BitA("q",1)), Order (Y (BitA("q",2)), Order (
             S (BitS("a")), Condition (Check (BitS "c", Click), I (BitS "x")))))))

[<Test>]
let ``Complex Quantum Code (mixed alloc, T, SX, arith, RZ, Pi )`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc q[2]; Calloc c,r; T x; SX x; RZ (5+3*Pi/2) q[1];"
    Assert.That(ast, Is.EqualTo (AllocQC (BitA ("q", 2), BitSeq (BitS "c", BitS "r")),
                                    Order (T (BitS "x"), Order (SX (BitS "x"), RZ
                                    (PlusExpr (Num 5, DivExpr (TimesExpr (Num 3, Pi), Num 2)),
                                    BitA ("q", 1))))))
    
[<Test>]
let ``Focus on booleans, or, not, >, SXDG, SWAP`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc x,a; Calloc c; If ( 5>6 or (not 4>=3) and (5.2 == 5)) SXDG a; SWAP x,a;"
    Assert.That(ast, Is.EqualTo (AllocQC (BitSeq (BitS "x", BitS "a"), BitS "c"),
                                   Order (Condition (LogOr (Greater (Num 5, Num 6),
                                     LogAnd (Neg (GreaterEqual (Num 4, Num 3)), Equal (Float 5.2, Num 5))),
                                       SXDG (BitS "a")), SWAP (BitS "x", BitS "a"))))
    
[<Test>]
let ``Focus on assigns, arithmetic, RY, RX`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc x; Calloc a; b:=-Pi/2-(+3); RY (4) z; RX (3-2) k; true && (5<3 || false || 5.0^2<=10) =| c;"
    Assert.That(ast, Is.EqualTo (AllocQC (BitS "x", BitS "a"), Order
                                  (Assign ("b", MinusExpr (DivExpr (UMinusExpr Pi, Num 2), UPlusExpr (Num 3))),
                                    Order (RY (Num 4, BitS "z"), Order (RX (MinusExpr (Num 3, Num 2), BitS "k"),
                                    AssignB ("c", LogAnd (Bool true, LogOr
                                    (LogOr (Less (Num 5, Num 3), Bool false),
                                    LessEqual (PowExpr (Float 5.0, Num 2), Num 10)))))))) )    

[<Test>]
let ``Focus on U, CNOT, CCX, SDG, TDG, Reset, NoClick`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc q; Calloc c; CNOT q, a; CCX a, b, c; SDG q; TDG x; If (c |> NoClick ) Reset q; U (0, Pi/2, 0) q;" 
    Assert.That(ast, Is.EqualTo (AllocQC (BitS "q", BitS "c"),
                                  Order (CNOT (BitS "q", BitS "a"), Order (CCX (BitS "a", BitS "b", BitS "c"),
                                   Order (SDG (BitS "q"), Order (TDG (BitS "x"), Order
                                   (Condition (Check (BitS "c", NoClick), Reset (BitS "q")),
                                    U (Num 0, DivExpr (Pi, Num 2), Num 0, BitS "q"))))))) )

[<Test>]
let ``Focus on PhaseDisk, Barrier, P, RXX, RZZ`` () =
    let (ast,_) = Handler.ParseQuLang "Qalloc a; Calloc z; PhaseDisk ;  Barrier q[1]; P (+Pi/2) a; RXX(0) a, b; RZZ(Pi) q[0], q[1];"
    Assert.That(ast, Is.EqualTo (AllocQC (BitS "a", BitS "z"), Order (PhaseDisk,
                                 Order (Barrier (BitA ("q", 1)), Order
                                 (P (DivExpr(UPlusExpr Pi, Num 2), BitS "a"), Order
                                 (RXX (Num 0, BitS "a", BitS "b"),
                                 RZZ (Pi, BitA ("q", 0), BitA ("q", 1))))))) )

[<Test>]
let ``Focus on variable, NotEqual`` () =
    let (ast, error) = Handler.ParseQuLang "Qalloc q[2]; Calloc x; P(a) q[1]; If (b < 3%2 and true or ~c ) ID q[2]; Measure q[1] -> c;"
    Assert.That(ast, Is.EqualTo (AllocQC (BitA ("q", 2), BitS "x"), Order
                                (P (VarA "a", BitA ("q", 1)), Order
                                  (Condition (LogOr (LogAnd (Less
                                  (VarA "b", ModExpr (Num 3, Num 2)), Bool true),
                                    VarB "c"), I (BitA ("q", 2))),
                                    Measure (BitA ("q", 1), BitS "c")))))
    Assert.That(error, Is.EqualTo Success)

[<Test>]
let ``Empty program with skipped characters`` () =
    let (ast, error) = Handler.ParseQuLang "    \n \r    \t"
    Assert.That(ast, Is.EqualTo (NOP, NOP))
    Assert.That(error, Is.EqualTo Success)
    
[<Test>]
let ``Invalid Program creates AST of Error`` () =
    let ast,error = Handler.ParseQuLang "Some error program!"
    let testError =
        match error with
        | SyntaxError _ -> Pass
        | _ -> Fail
    Assert.That(testError, Is.EqualTo Pass)
    Assert.That(ast, Is.EqualTo (NOP, NOP) )