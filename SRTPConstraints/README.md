# SRTP Constraints
Examples of using SRTP with a couple different constraint techniques.

Tags: SRTP, constraint, dependency, contract, structural, subtype

## Why?
 - Structural subtyping
 - Interface alternative (generally only for F# consumers)
 - Dependency management

## Limits
All generic calling functions need to be inlined

## Advantages
Avoids nominal type-resolution limitations

## Disadvantages
Interop with C# will be harder
