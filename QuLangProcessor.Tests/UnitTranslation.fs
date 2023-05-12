module Tests.UnitTestingTranslation

open NUnit.Framework
open QuLangProcessor.Handler
open QuLangProcessor.Tags
open QuLangProcessor.AST

[<SetUp>]
let Setup () =
    System.Globalization.CultureInfo.CurrentCulture <- new System.Globalization.CultureInfo("en-US")
    ()

[<Test>]
let ``Check Q# translation of circuit`` () =
    let ast = AllocQC (BitA ("Q", 2), BitS "C"),
                      Flow [UnaryGate (H, BitS "Q"); BinaryGate (CS, BitA ("q", 0), BitA ("q", 1));
                      BinaryGate (CH, BitA ("q", 0), BitA ("q", 1));
                      BinaryParamGate(RYY, BinaryOp (Pi, Div, Num 2), BitA ("q", 0), BitA ("q", 1))]
    let str = translateCircuit ast
    Assert.IsNotEmpty str

[<Test>]
let ``Check Q# translation of circuit #2`` () =
    let ast = AllocQC (BitSeq (BitS "x", BitS "a"), BitA ("c", 1)), Flow
                                   [Condition (LogicOp (RelationOp (Num 5, GT, Num 6), Or,
                                    LogicOp (Not (RelationOp (Num 4, GTE, Num 3)), And,
                                    RelationOp (Float 5.2, EQ, Num 5))), UnaryGate (SXDG, BitS "a"));
                                    BinaryGate (SWAP, BitS "x", BitS "a")]
    let str = translateCircuit ast
    Assert.IsNotEmpty str
    
[<Test>]
let ``Check Q# translation of circuit #3`` () =
    let ast = AllocQC (BitA ("q", 5), BitA ("c", 2)), Flow
                [UnaryGate(Z, BitA("q",1))
                 UnaryGate(SX, BitA("q",2))
                 UnaryGate(S, BitS("a"))
                 Condition (Check (BitS "c", Click),
                            UnaryGate(ID, BitS "x"))
                 ParamGate(RZ, Pi, BitS "x")
                 ParamGate(RX, Pi, BitS "x")
                 ParamGate(RY, Pi, BitS "x");]
    let str = translateCircuit ast
    Assert.IsNotEmpty str
    
[<Test>]
let ``Check Q# translation of circuit #4`` () =
    let ast = AllocQC (BitS "q", BitSeq (BitS "c", BitS "r")), Flow
                [BinaryGate (CNOT, BitS "q", BitS "a");
                Toffoli (BitS "a", BitS "b", BitS "c");
                Assign("a", Num 5); AssignB("b", LogicOp(B true, Xor, B false) );
                UnaryGate (SDG, BitS "q"); UnaryGate (TDG, BitS "x");
                Condition (Check (BitS "c", NoClick), Reset(BitS "q"));
                Unitary(Num 0, BinaryOp (Pi, Div, Num 2), Num 0, BitS "q")]
    let str = translateCircuit ast
    Assert.IsNotEmpty str
    
[<Test>]
let ``Check Q# translation of circuit #5`` () =
    let ast = AllocQC (BitA ("q", 2), BitS "x"), Flow
                [ParamGate (P, UnaryOp(Plus, VarA "a"), BitA ("q", 1))
                 Reset ( BitA ("q", 2) )
                 BinaryParamGate(RXX, Num 5, BitS "x", BitS "y");
                 Condition (LogicOp (LogicOp (RelationOp (VarA "b", LT, BinaryOp (Num 3, Mod, Num 2)),
                       And, B true), Or, VarB "c"), BinaryParamGate(RZZ, UnaryOp (Minus, VarA "a"),
                                                                    BitS "x", BitS "y"));
                Measure (BitA ("q", 1), BitA ("c", 1));
                Measure (BitA ("q", 1), BitS "c")]
    let str = translateCircuit ast
    Assert.IsNotEmpty str

[<Test>]
let ``Check QuLang back-compilation of circuit - (double parsing procedure) #1`` () =
    let code = "Qalloc Q[2]; Calloc C; H Q; CS q[0], q[1]; Barrier x; CH q[0], q[1]; RYY (Pi/2) q[0], q[1];"
    let ast, _ = parseQuLang code
    let trans, err = Option.get ast |> backCompileCircuit |> parseQuLang
    Assert.AreEqual (Success, err)
    Assert.AreEqual (ast, trans)
    
    
[<Test>]
let ``Check QuLang back-compilation of circuit - (double parsing procedure) #2`` () =
    let code = "Qalloc x,a; Calloc c; y:=5+3; If ( 5>6 or (not 4>=3) and (5.2 == 5)) SXDG a; SWAP x,a;"
    let ast, _ = parseQuLang code
    let trans, err = Option.get ast |> backCompileCircuit |> parseQuLang
    Assert.AreEqual (Success, err)
    Assert.AreEqual (ast, trans)
    
[<Test>]
let ``Check QuLang back-compilation of circuit - (double parsing procedure) #3`` () =
    let code = "Qalloc q; Calloc c; CNOT q, a; PhaseDisk; (true xor false) =| bl; CCX a, b, c; SDG q; TDG x; If (c |> NoClick ) Reset q; U (0, Pi/2, 0) q;"
    let ast, _ = parseQuLang code
    let trans, err = Option.get ast |> backCompileCircuit |> parseQuLang
    Assert.AreEqual (Success, err)
    Assert.AreEqual (ast, trans)
    
[<Test>]
let ``Check QuLang back-compilation of circuit - (double parsing procedure) #4`` () =
    let code = "Qalloc q[2]; Calloc x; P(a) q[1]; If (b < 3%2 and true or ~c ) ID q[2]; Measure q[1] -> c;"
    let ast, _ = parseQuLang code
    let trans, err = Option.get ast |> backCompileCircuit |> parseQuLang
    Assert.AreEqual (Success, err)
    Assert.AreEqual (ast, trans)