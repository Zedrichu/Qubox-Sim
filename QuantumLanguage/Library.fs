namespace QuantumLanguage

module Parser =

open System
open QuantumAST

let ParseQuCode code =
    let lexbuf = Lexing.LexBuffer<_>.FromString code
    let output = QuantumParser.start QuantumLexer.tokenize lexbuf
    output

Console.ReadKey(true) |> ignore