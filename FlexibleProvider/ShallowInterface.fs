namespace FlexibleProvider.ShallowInterface

module Interfaces =
    type IFoo =
        abstract member FooFn : unit -> string

    type IBar =
        abstract member BarFn : unit -> int

module ProviderA =
    open Interfaces

    type Env() =   
        interface IFoo with
            member _.FooFn () = "Foo A"

module ProviderB =
    open Interfaces

    type Env() =   
        interface IFoo with
            member _.FooFn () = "Foo B"

        interface IBar with
            member _.BarFn () = 0

module Application =
    module Logic =
        open Interfaces

        let fooThing (env: #IFoo) =
            env.FooFn()

        let barThing (env: #IBar) =
            env.BarFn()

        let main env =
            let x = fooThing env
            let y = barThing env

            y

    // let runA () =
    //     Logic.main (ProviderA.Env())
    // 1. The type 'ProviderA.Env' is not compatible with the type 'Interfaces.IBar'

    let runB () =
        Logic.main (ProviderB.Env())

module GenericInterfacesLimitation =
    (* Limitation:

    Generic interfaces can be implemented per specialization, but cannot be consumed in f# until:
    https://github.com/fsharp/fslang-suggestions/issues/1036

    If the attribute approach is used, then this could still be a useful technique since we won't
    need to thread through type annotations. If explicit annotations are required by consumers then
    this will have very limited usefulness with flexible providers.  *)
    type IBaz<'T> =
        abstract member Fn : unit -> 'T

    type Baz() =
        interface IBaz<string> with
            member _.Fn () = ""
        interface IBaz<int> with
            member _.Fn () = 0

    let bazIntThing (env: #IBaz<int>) = env.Fn()
    let bazStrThing (env: #IBaz<string>) = env.Fn()

    let main<'T
                // when 'T :> IBaz<int>
                // and 'T :> IBaz<string>
                // 1. Type constraint mismatch. The type 
                       // ''T'    
                   // is not compatible with type
                       // 'IBaz<string>'    
            > env =
        let intX = bazIntThing env
        // let strX = bazStrThing env
            // 1. The type 'string' does not match the type 'int'
        ()
