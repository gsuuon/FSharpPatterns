## Flexible Provider
Dependency management using flexible types and a provider instance. Useful for library-layer code. Less useful if we're only concerned with a single application. Also contains example usage with a reader computation expression.

Tags: provider, interface, reader, flexible type, environment

### Why?
 - Declarative
    - Separate what to do and how to do it
 - Portable
    - Different providers for different platforms
 - Code reuse
    - Share common components across applications 

### Limits
Need to explicitly thread through provider instance without a reader

### Comparison with direct calls
__Advantages__  
Can define middle-layer abstraction, e.g. layout or glue logic, while allowing flexibility to swap out low-level details.

__Disadvantages__  
Additional abstraction code to understand and maintain


### Inspiration
 - https://bartoszsypytkowski.com/dealing-with-complex-dependency-injection-in-f/
 - https://fsharpforfunandprofit.com/posts/dependencies-3/
