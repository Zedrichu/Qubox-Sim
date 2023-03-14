module public QuantumLanguage.VisitorPattern

type IVisitor<'T, 'S> =
    abstract member Visit : 'T -> 'S
and IVisitable<'T> =
    abstract member Accept : IVisitor<'T, 'S> -> 'S
