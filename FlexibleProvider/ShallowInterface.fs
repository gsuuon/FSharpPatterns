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
