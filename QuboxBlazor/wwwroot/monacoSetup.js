/* Vanilla JavaScript
 -*- coding: utf-8 -*-
monacoSetup

Description: This file is used to setup the Monaco editor. The tokens used in the QuLang format are registered to a 
new language within the Monaco configuration. Using the token providers a color theme for the language is defined along
with code completion feature for the language.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 29/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/


require.config({ paths: { 'vs': '_content/BlazorMonaco/lib/monaco-editor/min/vs' }});

let gate = ["H","X","Z","ID","Y","S","T","SDG","TDG","SX","SXDG","P","RZ","RY","RX","U"];
let gate2 = ["CNOT", "CCX", "SWAP", "RZZ", "RXX", "not", "or", "and"];
let keyword = ["Qalloc", "Calloc", "Measure", "Reset", "Barrier", "If", "PhaseDisk", "->", "~", "=|", ":=", "|>"];
let operator = ["-", "!=", "*", "/", "^", "&&", "+","^","%", "<", "<=", "==", ">", ">=", "||"];
let resulter = ["Click", "NoClick", "Pi","true","false"];


require(['vs/editor/editor.main'], function() {
    monaco.languages.register({ id: 'qulang' });

    monaco.languages.setLanguageConfiguration('qulang', {
        brackets: [ ["(", ")"], ["[", "]"],],
        autoClosingPairs: [
            { open: "[", close: "]" },
            { open: "(", close: ")" },
        ],
        surroundingPairs: [
            { open: "[", close: "]" },
            { open: "(", close: ")" },
        ],
        folding: {
            markers: {
                start: new RegExp("^\\s*#pragma\\s+region\\b"),
                end: new RegExp("^\\s*#pragma\\s+endregion\\b"),
            },
        },
        wordPattern: /[a-zA-Z_][a-zA-Z0-9_]*/,
    });

    monaco.languages.setMonarchTokensProvider('qulang', {
        brackets: [
            { token: "delimiter.parenthesis", open: "(", close: ")" },
            { token: "delimiter.square", open: "[", close: "]" },
        ],
        gates: gate,
        gates2: gate2,
        keywords: keyword,
        operators: operator,
        results: resulter,
        tokenizer: {
            root: [
                [ /[a-zA-Z_][a-zA-Z0-9_]*/, {
                    cases: {
                        "@gates": "gate",
                        "@gates2": "gate2",
                        "@keywords": "keyword",
                        "@operators": "operator",
                        "@results": "result",
                        "@default": "variable",}}],
                [/[-+*\/^%:=&<>|]/, "operator"],
                [/[{}()\[\]]/, "@brackets"],
                [/[0-9]+/, "number"],
                [/[;,]/, "delimiter"],
            ]},
    });

    monaco.languages.registerCompletionItemProvider('qulang', {
        provideCompletionItems: (model, position) => {
            const suggestions = [
                ...keyword.map(k => {
                    return {
                        label: k,
                        kind: monaco.languages.CompletionItemKind.Keyword,
                        insertText: k,
                    };}),
                ...gate.map(g => {
                    return {
                        label: g,
                        kind: monaco.languages.CompletionItemKind.Keyword,
                        insertText: g,
                    };}),
                ...gate2.map(g => {
                    return {
                        label: g,
                        kind: monaco.languages.CompletionItemKind.Keyword,
                        insertText: g,
                    };}),
                ...resulter.map(g => {
                    return {
                        label: g,
                        kind: monaco.languages.CompletionItemKind.Keyword,
                        insertText: g,
                    };}),
            ];
            return { suggestions: suggestions };
        }
    });

    monaco.editor.defineTheme('qubox', {
        base: "vs-dark",
        inherit: true,
        rules: [
            {foreground: "#f65b1a", token: "number",},
            {foreground: "#0080ff", token: "result",},
            {foreground: "#fffadc", token: "variable",},
            {foreground: "#75c3a8", token: "delimiter",},
            {foreground: "#ddcb07", token: "delimiter.parenthesis",},
            {foreground: "#11f898", token: "delimiter.square",},
            {foreground: "#31da0f", token: "gate",},
            {foreground: "#ffec45", token: "gate2",},
            {foreground: "#f7005b", token: "keyword",},
            {foreground: "#be17fd", token: "operator",},
            {foreground: "#400080", background: "#ffff00", fontStyle: "bold", token: "invalid.illegal",},
        ],
        colors: {
            "editor.foreground": "#fffadc",
            "editor.background": "#070e14",
            "editor.selectionBackground": "#200000",
            "editor.lineHighlightBackground": "#141a20",
            "editor.lineHighlightBorder": "#ffffff",
            "editorLineNumber.foreground": "#75c3a8",
            "editorLineNumber.activeForeground": "#ffec45",
            "editorCursor.foreground": "#7b00ff",
            "editorWhitespace.foreground": "#ffffff",
        },
    });
});