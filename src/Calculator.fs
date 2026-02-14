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
    | BackspacePressed          // Backspace (delete last character)

/// Calculator state
type Model = {
    Display: string
    PendingOp: (MathOp * float) option
    StartNew: bool  // true = next digit replaces display instead of appending
}

// ============================================================================
// Service Functions
// ============================================================================

/// Perform a math operation, handling divide-by-zero
let doMathOp (op: MathOp) (a: float) (b: float) : MathResult =
    match op with
    | Add -> Success (a + b)
    | Subtract -> Success (a - b)
    | Multiply -> Success (a * b)
    | Divide ->
        if b = 0.0 then DivideByZeroError
        else Success (a / b)

/// Parse display string to float
let parseDisplay (display: string) : float =
    match System.Double.TryParse(display) with
    | true, v -> v
    | false, _ -> 0.0

/// Format float result to display string
let formatResult (value: float) : string =
    string value
