module QuantumLanguage.VisitorPattern

type IVisitor<'T> =
    abstract member Visit : 'T -> unit
and IVisitable<'T> =
    abstract member Accept : IVisitor<'T> -> unit
