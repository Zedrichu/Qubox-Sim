module Tests.UnitTestingGrammar
(** F#
 -*- coding: utf-8 -*-
UnitTestingGrammar

Description: Module implementing unit tests on the grammar 
                productions & parsing of QuLang

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 27/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> TEST
*)
open NUnit.Framework

open QuLangProcessor
open AST
open Tags

type testResult =
    | Pass | Fail

[<SetUp>]
let Setup () =
    System.Globalization.CultureInfo.CurrentCulture <- new System.Globalization.CultureInfo("en-US")
    ()

[<Test>]
let ``Basic Allocation + Hadamard works + CS, CH, RYY`` () =
    let ast,_ = Handler.parseQuLang "Qalloc Q[2]; Calloc C; H Q; CS q[0], q[1]; CH q[0], q[1]; RYY (Pi/2) q[0], q[1];"
    Assert.That(ast, Is.EqualTo (Some ( AllocQC (BitA ("Q", 2), BitS "C"),
                      Flow [UnaryGate (H, BitS "Q"); BinaryGate (CS, BitA ("q", 0), BitA ("q", 1));
                      BinaryGate (CH, BitA ("q", 0), BitA ("q", 1));
                      BinaryParamGate(RYY, BinaryOp (Pi, Div, Num 2), BitA ("q", 0), BitA ("q", 1))])))
      
[<Test>]   
let ``Allocation + Chain of gates`` () =
    let (ast, _) = Handler.parseQuLang "Qalloc q; Calloc c; H q; X q; Measure q -> c;"
    Assert.That(ast, Is.EqualTo (Some(AllocQC (BitS "q", BitS "c"),
                        Flow [UnaryGate(H, BitS "q"); UnaryGate(X, BitS "q"); Measure (BitS "q", BitS "c")])))
    
[<Test>]
let ``Empty Program treated as no Operation`` () =
    let (ast,_) = Handler.parseQuLang ""
    Assert.That(ast, Is.EqualTo (None) )

[<Test>]
let ``Complex Quantum Code (if, allocation, Z, Y, S, I)`` () =
    let (ast,_) = Handler.parseQuLang "Qalloc q[5]; Calloc c[2]; Z q[1]; Y q[2]; S a;
                                    If ( c |> Click ) ID x;"
    Assert.That(ast,
       Is.EqualTo ( Some (
        AllocQC (BitA ("q", 5), BitA ("c", 2)), Flow
         [UnaryGate(Z, BitA("q",1)); UnaryGate(Y, BitA("q",2)); UnaryGate(
             S, BitS("a")); Condition (Check (BitS "c", Click), UnaryGate(ID, BitS "x"))])))

[<Test>]
let ``Complex Quantum Code (mixed alloc, T, SX, arith, RZ, Pi )`` () =
    let (ast,_) = Handler.parseQuLang "Qalloc q[2]; Calloc c,r; T x; SX x; RZ (5+3*Pi/2) q[1];"
    Assert.That(ast, Is.EqualTo (Some (AllocQC (BitA ("q", 2), BitSeq (BitS "c", BitS "r")),
                                    Flow [UnaryGate (T, BitS "x"); UnaryGate (SX, BitS "x")
                                          ParamGate (RZ, BinaryOp (Num 5, Add, BinaryOp
                                          (BinaryOp (Num 3, Mul, Pi), Div, Num 2)), BitA ("q", 1))])))
    
[<Test>]
let ``Focus on booleans, or, not, >, SXDG, SWAP`` () =
    let ast,_ = Handler.parseQuLang "Qalloc x,a; Calloc c; If ( 5>6 or (not 4>=3) and (5.2 == 5)) SXDG a; SWAP x,a;"
    Assert.That(ast, Is.EqualTo (Some (AllocQC (BitSeq (BitS "x", BitS "a"), BitS "c"), Flow
                                   [Condition (LogicOp (RelationOp (Num 5, GT, Num 6), Or,
                                    LogicOp (Not (RelationOp (Num 4, GTE, Num 3)), And,
                                    RelationOp (Float 5.2, EQ, Num 5))), UnaryGate (SXDG, BitS "a"));
                                    BinaryGate (SWAP, BitS "x", BitS "a")])));
    
[<Test>]
let ``Focus on assigns, arithmetic, RY, RX`` () =
    let ast,_ = Handler.parseQuLang "Qalloc x; Calloc a; b:=-Pi/2-(+3); RY (4) z; RX (3-2) k; true && (5<3 || false || 5.0^2<=10) =| c;"
    Assert.That(ast, Is.EqualTo (Some(AllocQC (BitS "x", BitS "a"), Flow [Assign("b",BinaryOp
            (BinaryOp (UnaryOp (Minus, Pi), Div, Num 2), Sub, UnaryOp (Plus, Num 3)));
            ParamGate (RY, Num 4, BitS "z");
            ParamGate (RX, BinaryOp (Num 3, Sub, Num 2), BitS "k");
            AssignB ("c", LogicOp (B true, And, LogicOp
            (LogicOp (RelationOp (Num 5, LT, Num 3), Or, B false), Or,
            RelationOp (BinaryOp (Float 5.0, Pow, Num 2), LTE, Num 10))))])));  

[<Test>]
let ``Focus on U, CNOT, CCX, SDG, TDG, Reset, NoClick`` () =
    let ast,_ = Handler.parseQuLang "Qalloc q; Calloc c; CNOT q, a; CCX a, b, c; SDG q; TDG x; If (c |> NoClick ) Reset q; U (0, Pi/2, 0) q;" 
    Assert.That(ast, Is.EqualTo (Some( AllocQC (BitS "q", BitS "c"), Flow
                                [BinaryGate (CNOT, BitS "q", BitS "a");
                                Toffoli (BitS "a", BitS "b", BitS "c");
                                UnaryGate (SDG, BitS "q"); UnaryGate (TDG, BitS "x");
                                Condition (Check (BitS "c", NoClick), Reset(BitS "q"));
                                Unitary(Num 0, BinaryOp (Pi, Div, Num 2), Num 0, BitS "q")])));

[<Test>]
let ``Focus on PhaseDisk, Barrier, P, RXX, RZZ`` () =
    let (ast,_) = Handler.parseQuLang "Qalloc a; Calloc z; PhaseDisk ;  Barrier q[1]; P (+Pi/2) a; RXX(0) a, b; RZZ(Pi) q[0], q[1];"
    Assert.That(ast, Is.EqualTo (Some( AllocQC (BitS "a", BitS "z"), Flow [PhaseDisk;
                                Barrier (BitA ("q", 1)); ParamGate (P, BinaryOp (UnaryOp(Plus,Pi), Div, Num 2), BitS "a");
                                BinaryParamGate (RXX, Num 0, BitS "a", BitS "b");
                                BinaryParamGate (RZZ, Pi, BitA ("q", 0), BitA ("q", 1))])));

[<Test>]
let ``Focus on variable, NotEqual`` () =
    let (ast, error) = Handler.parseQuLang "Qalloc q[2]; Calloc x; P(a) q[1]; If (b < 3%2 and true or ~c ) ID q[2]; Measure q[1] -> c;"
    Assert.That(ast, Is.EqualTo (Some( AllocQC (BitA ("q", 2), BitS "x"), Flow
                                [ParamGate (P, VarA "a", BitA ("q", 1));
                                Condition (LogicOp (LogicOp (RelationOp (VarA "b", LT, BinaryOp (Num 3, Mod, Num 2)),
                                       And, B true), Or, VarB "c"), UnaryGate (ID, BitA ("q", 2)));
                                Measure (BitA ("q", 1), BitS "c")])));
    Assert.That(error, Is.EqualTo Success)

[<Test>]
let ``Empty program with skipped characters`` () =
    let (ast, error) = Handler.parseQuLang "    \n \r    \t"
    Assert.That(ast, Is.EqualTo None)
    Assert.That(error, Is.EqualTo Success)
    
[<Test>]
let ``Test boolean expression solely`` () =
    let (ast, error) = Handler.parseBool "true && (5<3 || false or 5.0 ^ 2 <=10) and (not (c |> Click) xor true)"
    Assert.That(error, Is.EqualTo Success)
    Assert.That(ast, Is.EqualTo (Some (LogicOp (LogicOp (B true, And, LogicOp
            (LogicOp (RelationOp (Num 5, LT, Num 3), Or, B false), Or,
            RelationOp (BinaryOp (Float 5.0, Pow, Num 2), LTE, Num 10))), And,
            LogicOp (Not (Check (BitS "c", Click)), Xor, B true)))));

[<Test>]
let ``Test error in boolean expression`` () =
    let ast, error = Handler.parseBool "true + 3.2"
    let test = match error with
                | SyntaxError _ -> true
    Assert.That (ast, Is.EqualTo None)
    Assert.That (test, Is.EqualTo true)

[<Test>]    
let ``Test arithmetic expression solely`` () =
    let (ast, error) = Handler.parseArith "5^3 % 3 + 7-3/3 + Pi/2*3 - 5.023"
    Assert.That(error, Is.EqualTo Success)
    Assert.That(ast, Is.EqualTo (Some (BinaryOp (BinaryOp (BinaryOp (BinaryOp
              (BinaryOp (BinaryOp (Num 5, Pow, Num 3), Mod, Num 3), Add, Num 7),
                Sub, BinaryOp (Num 3, Div, Num 3)), Add,
                BinaryOp (BinaryOp (Pi, Div, Num 2), Mul, Num 3)), Sub, Float 5.023))));

[<Test>]
let ``Test error in arithmetic expression`` () =
    let ast, error = Handler.parseArith "50 and 3.2"
    let test = match error with
                | SyntaxError _ -> true
    Assert.That (ast, Is.EqualTo None)
    Assert.That (test, Is.EqualTo true)
    
[<Test>]
let ``Invalid Program creates AST of Error`` () =
    let ast,error = Handler.parseQuLang "Some error program!"
    let testError =
        match error with
        | SyntaxError _ -> Pass
        | _ -> Fail
    Assert.That(testError, Is.EqualTo Pass)
    Assert.That(ast, Is.EqualTo None )
    