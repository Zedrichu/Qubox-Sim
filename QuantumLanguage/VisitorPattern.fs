module public QuantumLanguage.VisitorPattern

type IVisitor<'T, 'S> =
    abstract member Visit : 'T -> 'S

type IVisitable<'T> =
    abstract member Accept : IVisitor<'T, 'S> -> 'S
