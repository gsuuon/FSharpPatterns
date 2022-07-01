namespace FlexibleProvider.Reader


module Reader =
    type Reader<'env, 'a> = Reader of ('env -> 'a)

    let env = Reader id
    let run env (Reader f) = f env

    type ReaderBuilder() =
        member inline _.Return x = Reader (fun _ -> x)
        member inline _.Bind ((Reader g), f) =
            Reader (fun env ->
                let (Reader f') = g env |> f
                f' env
            )
        member inline _.Zero () = Reader (fun _ -> ())

    let reader = ReaderBuilder()

module Application =
    open Reader
    open FlexibleProvider.ShallowInterface
    open FlexibleProvider.ShallowInterface.Interfaces

    module InlinedBody =
        let fooAndBar() = reader {
            let! (foo: #IFoo) = env
            let fooX = foo.FooFn()

            let! (bar: #IBar) = env
            let barX = bar.BarFn()

            return fooX.Length + barX
        }

        // let _ = run (ProviderA.Env()) (fooAndBar())
            // 1. The type 'ProviderA.Env' is not compatible with the type 'IBar'
        let _ = run (ProviderB.Env()) (fooAndBar())

    module Composed =
        let fooThing = Reader (fun (env: #IFoo) -> env.FooFn())
        let barThing = Reader (fun (env: #IBar) -> env.BarFn())

        let fooNBar() = reader {
            let! fooX = fooThing
            let! barX = barThing
            return fooX.Length + barX
        }

        // let _ = run (ProviderA.Env()) (fooNBar())
            // 1. The type 'ProviderA.Env' is not compatible with the type 'IBar'
        let _ = run (ProviderB.Env()) (fooNBar())
