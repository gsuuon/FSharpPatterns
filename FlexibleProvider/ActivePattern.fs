namespace FlexibleProvider.ActivePattern

(*
Experiments with active patterns instead of a computation expression.

Active patterns have more syntax texture, easier to identify the modifier and the subject. May be easier to deal with type resolution than CE's.
*)

module Interfaces =
    type ILog =
        abstract member Log : string -> unit

module Pattern =
    open Interfaces

    type Reader<'env, 'a> = Reader of ('env -> 'a)
    let inline (|Result|) env (Reader f) = f env
    let inline (|Logged|) (env: #ILog) (Reader f) =
        let x = f env
        env.Log (x.ToString())
        x

module Application =    
    open Pattern
    open FlexibleProvider.ShallowInterface
    open FlexibleProvider.ShallowInterface.Interfaces

    let fooThing = Reader (fun (env: #IFoo) -> env.FooFn())
    let barThing = Reader (fun (env: #IBar) -> env.BarFn())

    let main env =
        let (|Result|) x = (|Result|) env x
        let (Result fooX) = fooThing
        let (Logged env barX) = barThing
        ()

    // let _ = main (ProviderA.Env())
        // 1. The type 'ProviderA.Env' is not compatible with the type 'IBar'
    // let _ = main (ProviderB.Env())
        // 1. The type 'ProviderB.Env' is not compatible with the type 'Interfaces.ILog'

    type LoggingProviderB() =
        inherit ProviderB.Env()
        interface Interfaces.ILog with
            member _.Log x = printfn "%s" x

    let _ = main (LoggingProviderB())

module CompositionComparison =
    open Pattern
    open Application

    let run env (Reader f) = f env
    let log env (Reader f) =
        let x = f env
        printfn "%A" x
        x

    let main env =
        let run x = run env x
        let fooX = run fooThing
        let barX = log env barThing
        ()

module RunWhenPassed =
    open Pattern
    open Application

    let main env =
        let runAndAdd1 (Result env x) = x + 1
        let bazX = runAndAdd1 barThing
        ()
