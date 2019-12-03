using System;
using System.Linq;
using System.Collections.Generic;

public static class Lowerer
{
    /// <summary>
    /// Lowers a ValueNode
    /// </summary>
    public static string ToText(this ValueNode node, bool isAccurate = false) {
        if (node is ComplexStringNode strNode) {
            var sections = (from section in strNode.CodeSections
                            select '{' + section.ToText(isAccurate) + '}').ToArray();

            return "$\"" + String.Format(strNode.Representation, sections) + '"';
        }

        if (node is StringNode) return '"' + (node as StringNode).Value + '"';

        if (node is FunctionCallNode funcCallNode) {
            var args = (from arg in funcCallNode.CallingParameters
                              select arg.ToText(isAccurate)).ToArray();

            var funcName = funcCallNode.FunctionName.ToText(isAccurate);

            return $"{funcName}({String.Join(", ", args)})";
        }

        if (!(node is OperationNode op)) return node.Representation;

        if (isAccurate) return ToAccurateText(op);

        var opType = op.OperationType;

        if (opType.StartsWith("prefix")) {

            var operand = op.Operands[0].ToText(isAccurate);

            if (opType.EndsWith("Not")) return $"!{operand}";

            if (opType.EndsWith("Neg")) return $"-{operand}";

            if (opType.EndsWith("Pos")) return $"+{operand}";

            if (opType.EndsWith("Incr")) return $"++{operand}";

            if (opType.EndsWith("Decr")) return $"--{operand}";
        }

        if (opType.StartsWith("postfix")) {
            var operand = op.Operands[0].ToText(isAccurate);

            if (opType.EndsWith("Incr")) return $"{operand}++";

            if (opType.EndsWith("Decr")) return $"{operand}--";
        }

        if (opType.StartsWith("binary")) {
            var op1 = op.Operands[0].ToText(isAccurate);
            var op2 = op.Operands[1].ToText(isAccurate);

            if (opType.EndsWith("Assign")) return $"{op1} = {op2}";

            if (opType.EndsWith("Add")) return $"{op1} + {op2}";

            if (opType.EndsWith("Sub")) return $"{op1} - {op2}";

            if (opType.EndsWith("Mul")) return $"{op1} * {op2}";

            if (opType.EndsWith("Div")) return $"{op1} / {op2}";

            if (opType.EndsWith("Pow")) return $"{op1} ^ {op2}";

            if (opType.EndsWith("Eq"))  return $"{op1} == {op2}";

            if (opType.EndsWith("NotEq")) return $"{op1} != {op2}";

            if (opType.EndsWith("Or"))  return $"{op1} || {op2}";

            if (opType.EndsWith("And")) return $"{op1} && {op2}";

            if (opType.EndsWith("Greater")) return $"{op1} > {op2}";

            if (opType.EndsWith("GreaterOrEq")) return $"{op1} >= {op2}";

            if (opType.EndsWith("Less")) return $"{op1} < {op2}";

            if (opType.EndsWith("LessOrEq")) return $"{op1} <= {op2}";
        }

        throw new Exception($"Unknown operation {opType}");
    }

    public static string ToText(this StatementNode node, bool isAccurate = false) {
        if (node is ValueNode) return ToText(node as ValueNode, isAccurate);

        if (node is FunctionDeclarationNode funcDefNode) {
            var funcName = funcDefNode.Name.Representation;

            var parameters = (from param in funcDefNode.Parameters
                              select param.Representation).ToArray();

            var body = (from statement in funcDefNode.Value.Content
                        select statement.ToText(isAccurate));

            return $"def {funcName}({String.Join(", ", parameters)}) {{\n\t"
                  + String.Join("\n\t", body)
                  + "\n}";
        }

        if (node is DeclarationNode declarationNode) {

            var varName = declarationNode.Name.Representation;

            var value = declarationNode.Value.ToText(isAccurate);

            return $"var {varName} = ({value})";
        }

        if (node is ReturnNode returnNode) {
            if (!returnNode.IsReturningValue) return $"return";

            return $"return ({returnNode.Value.ToText(isAccurate)})";
        }

        throw new Exception($"Unknown StatementNode type {node.GetType()} ({node.Representation})");
    }

    private static string ToAccurateText(OperationNode op) {
        var opType = op.OperationType;

        if (opType.StartsWith("prefix")) {
            var operand = op.Operands[0].ToText(true);

            if (opType.EndsWith("Not")) return $"!({operand})";

            if (opType.EndsWith("Neg")) return $"-({operand})";

            if (opType.EndsWith("Pos")) return $"+({operand})";

            if (opType.EndsWith("Incr")) return $"++({operand})";

            if (opType.EndsWith("Decr")) return $"--({operand})";
        }

        if (opType.StartsWith("postfix")) {
            var operand = op.Operands[0].ToText(true);

            if (opType.EndsWith("Incr")) return $"({operand})++";

            if (opType.EndsWith("Decr")) return $"({operand})--";
        }

        if (opType.StartsWith("binary")) {
            var op1 = op.Operands[0].ToText(true);
            var op2 = op.Operands[1].ToText(true);

            if (opType.EndsWith("Assign")) return $"({op1}) = ({op2})";

            if (opType.EndsWith("Add")) return $"(({op1}) + ({op2}))";

            if (opType.EndsWith("Sub")) return $"(({op1}) - ({op2}))";

            if (opType.EndsWith("Mul")) return $"(({op1}) * ({op2}))";

            if (opType.EndsWith("Div")) return $"(({op1}) / ({op2}))";

            if (opType.EndsWith("Pow")) return $"(({op1}) ^ ({op2}))";

            if (opType.EndsWith("Eq"))  return $"(({op1}) == ({op2}))";

            if (opType.EndsWith("NotEq")) return $"(({op1}) != ({op2}))";

            if (opType.EndsWith("Or"))  return $"(({op1}) || ({op2}))";

            if (opType.EndsWith("And")) return $"(({op1}) && ({op2}))";

            if (opType.EndsWith("Greater")) return $"(({op1}) > ({op2}))";

            if (opType.EndsWith("GreaterOrEq")) return $"(({op1}) >= ({op2}))";

            if (opType.EndsWith("Less")) return $"(({op1}) < ({op2}))";

            if (opType.EndsWith("LessOrEq")) return $"(({op1}) <= ({op2}))";
        }

        if (opType == "arrayAccess") return $"({op.Operands[0].ToText(true)})[{op.Operands[1].ToText(true)}]";

        throw new Exception($"Unknown operation {opType}");
    }
}