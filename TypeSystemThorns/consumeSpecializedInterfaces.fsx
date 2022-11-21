(* 
    You can't consume specializations of generic interfaces as if they were a concrete interface

    https://github.com/fsharp/fslang-suggestions/issues/1036
*)
type Foo<'T> = abstract thing: 'T -> int

let fooInt (foo: #Foo<int>) = foo.thing 0
let fooString (foo: #Foo<string>) = foo.thing ":("

(*
    We can define implementations of specializations of a generic interface
*)
type Bar() =
    interface Foo<int> with
        member _.thing (x: int) = 0

    interface Foo<string> with
        member _.thing (x: string) = 1

(*
    But we can't consume different specializations from the same implementing type. This is currently by design. The reason given is that this makes type inference for 'a :> seq<'a> <-> 'b :> seq<'b> work in a more expected way.
*)
let main env =
    let a = fooInt env
    // let b = fooString env
        // 1. The type 'string' does not match the type 'int'
    ()

(*
    The workaround is to duplicate the interfaces / specialize them yourself. This also removes any relation between the two specializations that you got with a generic - any code which worked with the (unspecialized) generic interface will also need to be duplicated. A potential workaround for that is to nest the interfaces to be able to talk about them as a group.
*)
type FooString = abstract thing: string -> int
type FooInt = abstract thing: int -> int

type BarDupe() =
    interface FooInt with
        member _.thing (x: int) = 0

    interface FooString with
        member _.thing (x: string) = 1

let fooIntDupe (foo: #FooInt) = foo.thing 0
let fooStringDupe (foo: #FooString) = foo.thing ":("

let mainDupe env =
    let a = fooIntDupe env
    let b = fooStringDupe env
    ()
