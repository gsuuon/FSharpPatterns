namespace Workarounds.GenericInheritanceInference

(*
Generic inheritance sometimes requires annotations, forcing us to pin the type of an argument in an intermediate class.
We can pass a create function rather than a value to get inference to flow through the constructor.
*)

module GenericInheritance =
    open System.Collections.Generic

    type Foo<'K, 'V>(cache: IDictionary<'K, 'V>) = class end
    type Bar<'K, 'V>(cache: IDictionary<'K, 'V>) = inherit Foo<'K, 'V>(cache)
    type Baz<'K, 'V>(cache) = inherit Foo<'K, 'V>(cache)
        // The inferred type of cache in Baz is correct
        // val cache : IDictionary<'K,'V>
        // we still need the annotation or Baz(strIntDict) fails to type check

    let strIntDict = new Dictionary<string, int>()

    let x = Foo(strIntDict)
    let y = Bar(strIntDict)
    // let z = Baz(strIntDict)
        // 1. The type ''K' does not match the type 'string'.

module GenericInheritanceFn =
    open System.Collections.Generic

    type Foo<'K, 'V>(mkCache: unit -> IDictionary<'K, 'V>) = class end
    type Bar<'K, 'V>(mkCache: unit -> IDictionary<'K, 'V>) = inherit Foo<'K, 'V>(mkCache)
    type Baz<'K, 'V>(mkCache) = inherit Foo<'K, 'V>(mkCache)

    let mkStrIntDict () = new Dictionary<string, int>() :> IDictionary<_,_>

    let x = Foo(mkStrIntDict)
    let y = Bar(mkStrIntDict)
    let z = Baz(mkStrIntDict)
