namespace FlexibleProvider.NestedInterface
(*

Nesting interfaces prevents a noisy list of method names on the top-level environment.
It also allows us to use object expressions to implement the interfaces.

Limitation: we can't use the same member name for the inner interface with `member val` due to backing field

Note: generic interfaces can be implemented per specialization
*)

module Interfaces =
    open FlexibleProvider.ShallowInterface.Interfaces

    type IFooProvider =
        abstract member Foo : IFoo

    type IBarProvider =
        abstract member Bar : IBar

module Provider =
    open Interfaces
    open FlexibleProvider.ShallowInterface.Interfaces

    type Environment() =
        interface IFooProvider with
                // need to use `member val Foo` and not `member _.Foo` or else we get a new instance each access
            member val Foo =
                { new IFoo with
                    member _.FooFn () = "Foo thing"
                }

        interface IBarProvider with
            member val Bar =
                { new IBar with
                    member _.BarFn () = 0
                }

module Application =
    module Logic =
        open Interfaces

        let fooThing (env: #IFooProvider) =
            env.Foo.FooFn()

        let barThing (env: #IBarProvider) =
            env.Bar.BarFn()

        let main env =
            let x = fooThing env
            let y = barThing env

            y
    
    let _ = Logic.main (Provider.Environment())
