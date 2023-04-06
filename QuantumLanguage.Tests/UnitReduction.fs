module Tests.Optimization

open Microsoft.FSharp.Core
open NUnit.Framework

open QuantumLanguage
open Handler
open AST

[<SetUp>]
let Setup () =
    System.Globalization.CultureInfo.CurrentCulture <- System.Globalization.CultureInfo("en-US")
    ()

[<Test>]
let ``Test non-optimizable AST`` () =
    let code = "Qalloc q[2]; Calloc c; H q[0]; H q[1]; CNOT q[0], q[1]; Measure q[0] -> c[0];"
    let ast, err = parseQuLang code
    Assert.AreEqual (Success, err)
    let mem, err = ast |> Option.get |> analyzeSemantics
    Assert.AreEqual (Success, err)
    let _, schema = ast.Value
    let optimal, _, _ = optimizeAST schema mem
    Assert.AreEqual (optimal, schema)
    
[<Test>]
let ``Test undefined arithmetic variable in AST`` () =
    let code = "Qalloc q[2]; Calloc c; H q[0]; x:=5+5+a-2; CNOT q[0], q[1]; Measure q[0] -> c[0];"
    let ast, err = parseQuLang code
    Assert.AreEqual (Success, err)
    let mem, err = ast |> Option.get |> analyzeSemantics
    Assert.AreEqual (Success, err)
    let _, schema = ast.Value
    let _, _, err = optimizeAST schema mem
    match err with
    | Success -> Assert.Fail()
    | EvaluationError _ -> Assert.Pass()
    
[<Test>]
let ``Test undefined conditions/boolean variables in AST`` () =
    let code = "Qalloc q[2]; Calloc c; H q[0]; true and (5 > 3) =| y; If (~y or not (c |> NoClick) ) CNOT q[0], q[1]; Measure q[0] -> c[0];"
    let ast, err = parseQuLang code
    Assert.AreEqual (Success, err)
    let mem, err = ast |> Option.get |> analyzeSemantics
    Assert.AreEqual (Success, err)
    let _, schema = ast.Value
    let _, _, err = optimizeAST schema mem
    Assert.AreEqual (Success, err)
    
[<Test>]
let ``Test successful conditions/arithmetic variables in AST`` () =
    let code = "Qalloc q[2]; Calloc c; H q[0]; a:=3; true =| x; If (a<5 or not (c |> NoClick) ) CNOT q[0], q[1]; Measure q[0] -> c[0];"
    let ast, err = parseQuLang code
    Assert.AreEqual (Success, err)
    let mem, err = ast |> Option.get |> analyzeSemantics
    Assert.AreEqual (Success, err)
    let _, schema = ast.Value
    let _, _, err = optimizeAST schema mem
    Assert.AreEqual (Success, err)
    
[<Test>]
let ``Test arithmetic reductions solely #1`` () =
    let code = "5+3-2+4/2*(3+2)+0-0*3"
    let ast, err = parseArith code
    Assert.AreEqual (Success, err)
    let optimal, _, err = optimizeArithmetic (Option.get ast) Memory.empty
    Assert.AreEqual (Success, err)
    Assert.AreEqual (Num 16, optimal)
    
[<Test>]
let ``Test arithmetic reductions solely #2`` () =
    let code = "5.0- 2.3+2/2*4/2*(3+2)+ 0.0-0*3+5%2+6^0+Pi*1"
    let ast, err = parseArith code
    Assert.AreEqual (Success, err)
    let optimal, _, err = optimizeArithmetic (Option.get ast) Memory.empty
    Assert.AreEqual (Success, err)
    Assert.AreEqual (BinaryOp(Pi, Add, Float 14.7), optimal)

[<Test>]
let ``Test arithmetic reductions solely #3`` () =
    let code = "-5.0- 2.3+(-2.0/2)*4/2*(3+ 1.0*2)+(Pi/3)/(Pi/3)- 0.0-(+1) + 0.0-0/1.0*3+5%2+6^0+Pi* 1.0"
    let ast, err = parseArith code
    Assert.AreEqual (Success, err)
    let optimal, _, err = optimizeArithmetic (Option.get ast) Memory.empty
    Assert.AreEqual (Success, err)
    
    Assert.AreEqual (BinaryOp(Pi, Add, Float -15.3), optimal)    
    
[<Test>]
let ``Test undefined arithmetic variable in reduction`` () =
    let code = "5+3-2+4/2*(3+ 2.0)+ 0.0/1 -a*0"
    let ast, err = parseArith code
    Assert.AreEqual (Success, err)
    let _, _, err = optimizeArithmetic (Option.get ast) Memory.empty
    match err with
    | Success -> Assert.Fail()
    | EvaluationError _ -> Assert.Pass()

    
[<Test>]
let ``Test boolean reductions solely #1`` () =
    let code = "true and (false or 3 > 1)"
    let ast, err = parseBool code
    Assert.AreEqual (Success, err)
    let optimal, _, err = optimizeLogic (Option.get ast) Memory.empty
    Assert.AreEqual (Success, err)
    Assert.AreEqual (B true, optimal)
    
[<Test>]
let ``Test undefined boolean variable solely`` () =
    let code = "true and (false or 3 > 1) xor ~z"
    let ast, err = parseBool code
    Assert.AreEqual (Success, err)
    let _, _, err = optimizeLogic (Option.get ast) Memory.empty
    match err with
    | Success -> Assert.Fail()
    | EvaluationError _ -> Assert.Pass()
    
[<Test>]
let ``Test boolean reductions solely #2`` () =
    let code = "not (not true) and ((false or 3 <= 1.0) xor (1.0 > 2.0 and 9.0 == 9))"
    let ast, err = parseBool code
    Assert.AreEqual (Success, err)
    let optimal, _, _ = optimizeLogic (Option.get ast) Memory.empty
    Assert.AreEqual (B false, optimal)

[<Test>]
let ``Test boolean reductions solely #3`` () =
    let code = "not (true or 5.0 < 3.0 and 5 != 4.0 ^ 2) and ((false or 3.0 != 1*0.0) xor not (1.0 >= 0 and 9.0 == 9))"
    let ast, err = parseBool code
    Assert.AreEqual (Success, err)
    let optimal, _, _ = optimizeLogic (Option.get ast) Memory.empty
    Assert.AreEqual (B false, optimal)