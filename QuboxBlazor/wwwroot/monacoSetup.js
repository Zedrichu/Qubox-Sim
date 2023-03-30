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
                [/[-+*\/^%=&<>|]/, "operator"],
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
            {foreground: "#00ffbd", token: "number",},
            {foreground: "#00ff70", token: "result",},
            {foreground: "#fffadc", token: "variable",},
            {foreground: "#fff300", token: "delimiter",},
            {foreground: "#0066ff", token: "delimiter.parenthesis",},
            {foreground: "#0066ff", token: "delimiter.square",},
            {foreground: "#49fc03", token: "gate",},
            {foreground: "#ff00b7", token: "gate2",},
            {foreground: "#0077ff", token: "keyword",},
            {foreground: "#ff00ff", token: "operator",},
            {foreground: "#400080", background: "#ffff00", fontStyle: "bold", token: "invalid.illegal",},
        ],
        colors: {
            "editor.foreground": "#fffadc",
            "editor.background": "#051215",
            "editor.selectionBackground": "#80000080",
            "editor.lineHighlightBackground": "#330000",
            "editor.lineHighlightBorder": "#ffffff",
            "editorLineNumber.foreground": "#3a3a3a",
            "editorLineNumber.activeForeground": "#ffe5a7",
            "editorCursor.foreground": "#7b00ff",
            "editorWhitespace.foreground": "#ffffff",
        },
    });
});