module Calculator

/// Math operations the calculator supports
type MathOp =
    | Add
    | Subtract
    | Multiply
    | Divide

/// Result of a math operation (handles divide-by-zero)
type MathResult =
    | Success of float
    | DivideByZeroError

/// Calculator messages (user actions)
type Msg =
    | DigitPressed of int       // 0-9
    | DecimalPressed            // .
    | OperatorPressed of MathOp // +, -, ×, ÷
    | EqualsPressed             // =
    | ClearPressed              // C/AC

/// Calculator state
type Model = {
    Display: string
    PendingOp: (MathOp * float) option
}
