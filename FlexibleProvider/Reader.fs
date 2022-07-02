namespace FlexibleProvider.Reader
#nowarn "0064"

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

            let barIsAlsoFoo = bar.FooFn()
                // NOTE:
                // Since the type of bar includes all previous constraints, we can
                // use bar to access any previously accessed interface :/

            return fooX.Length + barX
        }

        // let _ = run (ProviderA.Env()) (fooAndBar())
            // 1. The type 'ProviderA.Env' is not compatible with the type 'IBar'
        let _ = run (ProviderB.Env()) (fooAndBar())

        module InlinedProvider =    
            // Read as "over"
            let inline (^/) (Reader f) env = f env

            type IFooBar =
                inherit IFoo
                inherit IBar

            let fooBar =
                reader {
                    let! (foo: #IFoo) = env
                    let strX = foo.FooFn()

                    let! intX =
                        reader {
                            let! (bar: #IBar) = env
                            return bar.BarFn()
                        }

                    return strX.Length + intX
                } ^/ {
                    // (Warning) This construct causes code to be less generic than indicated by its type annotations.
                    // The type variable implied by the use of a '#', '_' or other type annotation 
                    // has been constrained to be type 'IFooBar'.
                    new IFooBar with
                        member _.FooFn () = "foo"
                        member _.BarFn () = 0
                }

    module Composed =
        open Reader

        let fooThing = Reader (fun (env: #IFoo) -> env.FooFn())

        let barThing() = reader { 
            // If we use the reader builder, then the variable needs to be a function to be generic
            let! (bar: #IBar) = env
            return bar.BarFn()
        }

        let fooNBar() = reader {
            let! fooX = fooThing
            let! barX = barThing()
            return fooX.Length + barX
        }

        // let _ = run (ProviderA.Env()) (fooNBar())
            // 1. The type 'ProviderA.Env' is not compatible with the type 'IBar'
        let _ = run (ProviderB.Env()) (fooNBar())

        // If barThing were a value `let barThing =` and not a function, this usage would break
        // since it would've been specialized by the first usage
        let _ = run { new IBar with member _.BarFn () = 0 } (barThing())
