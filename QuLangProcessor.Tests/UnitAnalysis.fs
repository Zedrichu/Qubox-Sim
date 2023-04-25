module Tests.Analysis

open Microsoft.FSharp.Core
open NUnit.Framework
open QuLangProcessor.Handler
open QuLangProcessor.AST

[<Test>]
let ``Test for invalid bit registers`` () =
    let code = "Qalloc q; Calloc c, q; x:=a;"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    match err with
        | Success -> Assert.Fail()
        | SemanticError _ -> Assert.Pass()

[<Test>]
let ``Test for invalid bit operation`` () =
    let code = "Qalloc q; Calloc c; x:=a; H x[2]; P(50) c;"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    match err with
        | Success -> Assert.Fail()
        | SemanticError _ -> Assert.Pass()

[<Test>]
let ``Successful bit allocation and valid operators`` () =
    let code = "Qalloc q[2],r; Calloc c; ID q[0]; CNOT q[0], q[1]; RXX (Pi) q[0], r;"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    Assert.AreEqual(err, Success)
    
[<Test>]
let ``Test for invalid measurement #1`` () =
    let code = "Qalloc q[2],r; Calloc c; Measure c -> r;"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    match err with
        | Success -> Assert.Fail()
        | SemanticError _ -> Assert.Pass()
        
[<Test>]
let ``Test for invalid measurement #2`` () =
    let code = "Qalloc q[2],r; Calloc c; Measure r -> q[1];"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    match err with
        | Success -> Assert.Fail()
        | SemanticError _ -> Assert.Pass()
        
[<Test>]
let ``Overflow of register `` () =
    let code = "Qalloc q[2]; Calloc c; RX (Pi) q[2];"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    match err with
        | Success -> Assert.Fail()
        | SemanticError _ -> Assert.Pass()
    
[<Test>]
let ``Test successful Toffoli and Conditional`` () =
    let code = "Qalloc q[2], r; Calloc c; If (c |> Click) CCX q[0], q[1], r;"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    Assert.AreEqual(err, Success)
    
[<Test>]
let ``Unsuccessful conditional & parametric`` () =
    let code = "Qalloc q[2], r; Calloc c; Reset q[0]; PhaseDisk; If (not (true) and r |> Click) Z q;"
    let ast, err = parseQuLang code
    Assert.AreEqual(err, Success)
    let mem, err = ast |> Option.get |> analyzeSemantics
    match err with
        | Success -> Assert.Fail()
        | SemanticError _ -> Assert.Pass()