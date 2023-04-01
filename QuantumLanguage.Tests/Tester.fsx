#r "FsLexYacc.Runtime.dll"

open FSharp.Text.Lexing
open System
#load "../QuantumLanguage/VisitorPattern.fs"
open QuantumLanguage.VisitorPattern
#load "../QuantumLanguage/AST.fs"
open QuantumLanguage.AST
#load "../QuantumLanguage/Parser.fs"
open QuantumLanguage.Parser
#load "../QuantumLanguage/Lexer.fs"
open QuantumLanguage.Lexer
#load "../QuantumLanguage/Interpreter.fs"
open QuantumLanguage.Interpreter
#load "../QuantumLanguage/Compiler.fs"
open QuantumLanguage.Compiler
#load "../QuantumLanguage/Translator.fs"
open QuantumLanguage.Translator
#load "../QuantumLanguage/Handler.fs"
open QuantumLanguage.Handler

mainMenu()