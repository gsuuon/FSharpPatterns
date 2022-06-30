namespace SRTPConstraints.TypeAlias

// Record-based omitted, difference would be same as in ActivePattern

module Constraints = 
    type Quacker< ^a when 'a : (member Quack : unit -> string) > = 'a

module Sayer =
    open Constraints

    let inline quack (x: ^a Quacker) =
        (^a : (member Quack : unit -> string) x)

    let inline barkAndCanQuack (x: ^a Quacker) =
        (^a : (member Bark : unit -> string) x)

module Say =
    open SRTPConstraints.Types.Classes
    open Sayer

    ignore <| quack (Foo())
    ignore <| quack (Bar())
    // ignore <| quack (Baz())
        // 1. The type 'Baz' does not support the operator 'Quack'

    // ignore <| barkAndCanQuack (Foo())
        // 1. The type 'Constraints.Quacker<Foo>' does not support the operator 'Bark'
    ignore <| barkAndCanQuack (Bar())
    // ignore <| barkAndCanQuack (Baz())
        // 1. The type 'Baz' does not support the operator 'Quack'
