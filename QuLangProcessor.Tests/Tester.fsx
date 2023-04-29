#r "FsLexYacc.Runtime.dll"

open FSharp.Text.Lexing
open System
#load "../QuLangProcessor/Tags.fs"
open QuLangProcessor.Tags
#load "../QuLangProcessor/AST.fs"
open QuLangProcessor.AST
#load "../QuLangProcessor/Parser.fs"
open QuLangProcessor.Parser
#load "../QuLangProcessor/Lexer.fs"
open QuLangProcessor.Lexer
#load "../QuLangProcessor/Compiler.fs"
open QuLangProcessor.Compiler
#load "../QuLangProcessor/BackCompiler.fs"
open QuLangProcessor.BackCompiler
#load "../QuLangProcessor/Translator.fs"
open QuLangProcessor.Translator
#load "../QuLangProcessor/Handler.fs"
open QuLangProcessor.Handler

mainMenu()