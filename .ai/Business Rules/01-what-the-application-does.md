# What the application does

A user selects an AWL or STL text export and clicks **Execute**. The application turns each network it understands into an equation and puts those equations on Excel sheets grouped by function block, function, or standalone file.

The equations are a review form of the rung, not a Triconex project and not a PLC download.

## Containers

A STEP 7 export is read in this order:

1. Every `FUNCTION_BLOCK` … `END_FUNCTION_BLOCK` region.
2. Every `FUNCTION` … `END_FUNCTION` region.
3. If neither exists, the networks in the file itself.

Each network is numbered from 1 inside its parent.

## Equation shape

Addresses lose the `#` symbol. Dots become underscores, so `M 1.0` is `M1_0` and `DB103.DBX 0.2` is `DB103_DBX0_2`.

| STL / AWL | Equation |
| --- | --- |
| `A` / `U` | `AND` |
| `AN` / `UN` | `AND` with `!` on that operand |
| `O` | `OR` |
| `ON` | `OR` with `!` on that operand |
| `X` | `Exclusive` |
| `XN` | `Exclusive` with `!` on that operand |
| `A(` / `U(` , `O(` , `X(` , `)` | Nested gate |
| `O ;` or `A ;` with an empty operand | Closes the current AND term and continues an OR (or AND) chain |
| `S` before `R` | `RS(set, reset)` |
| `R` before `S` | `SR(set, reset)` |
| `SP` / `SI` | `TP(enable, preset)` |
| `SD` / `SE` | `TON(enable, preset)` |
| `SF` / `SA` | `TOF(enable, preset)` |
| `L S5T#2S` | Preset `T#2S` (`S5` is removed) |
| `NOT` before `=` | The right-hand side is negated |
| `BLD` and `NOP` | Dropped |
| `=` to `L` then several coils | The local bit is replaced by the expression that was stored in it, and the `L` coil is not a row |

A single contact stays a direct assignment: `Q160_0=!I6_4`. Several contacts of the same gate become one call: `AND(...)` or `OR(...)`.

## Set/reset together with a coil

When parentheses contain `S` or `R` and the network also assigns another coil, the latch is one row and the coil is the next row. The contacts that only feed set and reset stay on the latch row. The coil uses the latch bit as an input.

Example from `Logic-1.AWL`:

`M14_0=SR(M13_6,M13_7)`

`M14_2=OR(AND(M14_0,!I2_5,!I2_6),I2_5)`

## What is still raw text

Word logic (`L` / `T` of bytes and double words), `UC` / `CALL`, `OPN`, `LAR2`, and data-block initial values are not boolean equations. Those lines are kept as instructions when they appear in a network. If the network matches no parser, the row is the original tokens and the rest of the file still exports.

`FC2034.awl`, `FC2035.awl`, and `FC2038.awl` are word-logic fragments without `NETWORK` or `FUNCTION` headers. They load without throwing. `Siemens-Logic-2.AWL` is a data block. `Siemens-Logic-3.AWL` is another export that must load without throwing.

## Cleanup that is intentionally off

`ComplexEquationParser.CleanLogic` still has four commented replacements: empty `(),`, inserting a comma before a gate, removing a comma before a gate, and collapsing repeated commas. They are not part of the running cleanup. The active cleanup removes spaces around parentheses, inserts `,(` when an identifier is glued to `OR` or `AND`, strips remaining spaces, balances missing parentheses, and simplifies a one-argument `AND` or `OR` unless that gate sits directly after `)` (so `AND(...)OR(M1_0)` keeps the `OR`).

An equation that is already one complete gate, or a comma-separated list of complete gates, is not passed through the older “outermost gate” regrouping. That regrouping remains for the flat networks whose raw text is still `AND(...)OR(...)` without a wrapping call.
