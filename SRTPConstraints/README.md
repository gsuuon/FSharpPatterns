## SRTP Constraints
Examples of using SRTP with a couple different constraint techniques.

Tags: SRTP, constraint, dependency, contract, structural, subtype

### Why?
 - Structural subtyping
 - Interface alternative (generally only for F# consumers)
 - Dependency management

### Limits
All generic calling functions need to be inlined

### Comparison with interfaces
__Advantages__  
Avoids nominal type-resolution limitations

__Disadvantages__  
Interop with C# will be harder
