module Tests

open NUnit.Framework
open System
open QuantumLanguage
open AST

[<SetUp>]
let Setup () =
    ()

[<Test>]
let ``Basic Allocation + Hadamard works`` () =
    let ast = Handler.ParseQuLang "Qalloc Q; Calloc C; H Q;"
    Assert.That(ast, Is.EqualTo (AllocQC(BitS "Q", BitS "C"), H (BitS "Q") ) )
    
[<Test>]
let ``Empty Program treated as no Operation`` () =
    let ast = Handler.ParseQuLang ""
    Assert.That(ast, Is.EqualTo (NOP, NOP) )

[<Test>]
let ``Invalid Program creates AST of Error`` () =
    let ast = Handler.ParseQuLang "Some error program!"
    Assert.That(ast, Is.EqualTo (Error "Parse Error", Error "Parse Error") )