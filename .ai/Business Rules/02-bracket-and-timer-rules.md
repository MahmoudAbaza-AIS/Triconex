# Bracket, timer, and latch rules

These rules are what the unit tests enforce. Spacing in the equation does not matter. Dots in addresses are underscores before the comparison.

## Brackets

`A(` … `)` builds an AND group. If the inside is only `O` contacts, the result is `AND(OR(...))` when that bracket is the whole rung, and the `OR(...)` is merged into a surrounding `AND` when more contacts follow.

`O ;` inside a bracket closes an AND term and ORs it with the next term. A bracket whose body is that kind of OR is the OR itself, not `AND(OR(...))`.

Two brackets where the first is an OR of AND groups and the second is an OR of contacts are written as:

`OR(AND(...), AND(...)), AND(OR(...))`

That is the form locked by `Pattern_12_1` in `BitLogicUnitTest`.

A trailing `O bit` after an AND chain becomes `OR(AND(...), bit)`, including `Q1_2=OR(M1_0,I3_3)` from `Logic-1.AWL`.

## Local bits

```
A   #STAT5
=   L 0.0
A   L 0.0
AN  #STAT7
=   #STAT8
A   L 0.0
BLD 102
=   #STAT7
```

Rows: `STAT8=AND(STAT5,!STAT7)` and `STAT7=STAT5`. The `L` assignment is not a row.

## Timers

The enable condition uses one gate for the whole list: any `O` makes it `OR`, otherwise any `XOR` makes it `XOR`, otherwise `AND`.

A later `A T n` / `= coil` is a second row. `NOT` before that coil negates the timer bit: `Q3_1=!T12`.

`S5T#2S` is stored as `T#2S`.

## German AWL

The same rung as `Logic-1.AWL`’s `OUT4` network, written with `U` and `UN`, produces `OUT4=OR(M1_0,AND(STAT5,!STAT6),AND(STAT6,M1_1))`.

## Pass and fail on the sheet

The sheet status is not a proof that the equation was simulated. Function blocks are marked passed when the block name is present. Functions are marked passed when the FC reference text is not the word `Unclear`.
