namespace TypeLevels

module InheritNongenericInterface =
    (*
    Inherit non-generic interface from generic version of interface to separate generic and non-generic aspects,
    but allow consuming code to cast to non-generic if the underlying type is not needed

    Inspiration:
        https://github.com/fsprojects/Avalonia.FuncUI/blob/2a3e8def49e5804b1c731ad869135ba65c423f72/src/Avalonia.FuncUI/Types.fs
    *)
    type IFoo = 
        abstract member Count : int
    type IFoo<'T> =
        inherit IFoo
        abstract member Get : unit -> 'T
        abstract member Reverse : unit -> IFoo<'T>
        
    type Foo<'T> =
        { collection : List<'T>
        }
        interface IFoo<'T> with
            member this.Count = List.length this.collection
            member this.Reverse () = { collection = this.collection |> List.rev } 
            member this.Get () = this.collection.Head

    module Foo =
        let head<'T> (foo: IFoo<'T>) = foo.Get()
        let reverse (foo: IFoo<'T>) = foo.Reverse()
        let count (foo: IFoo) = foo.Count

    let main () = 
        let foo = { collection = ["hi"] }

        let x = Foo.head foo // can access underlying type
        let y = Foo.count foo

        let foo' = Foo.reverse foo

        let x' = Foo.head foo'
        let y' = Foo.count foo'

        ()

module ExistentialRank2 =
    (* Existential type by hiding a universal member
    
    Inspiration:
        https://eiriktsarpalis.github.io/typeshape/#/23
        https://www.gresearch.co.uk/blog/article/squeezing-more-out-of-the-f-type-system-introducing-crates/
    *)

    module CantSayThis =
        // When 'T is some type of list, but we don't care what the type is
        // type Foo<'T when List<'T>> = interface end
            // 1. Unexpected identifier in type name
            // 2. Unexpected keyword 'end' in definition. Expected incomplete structured construct at or before this point or other token.

        // type Foo<List<'T>> = interface end
            // 1. Unexpected identifier in type name. Expected infix operator, quote symbol or other token.
            // 2. Unexpected keyword 'end' in definition. Expected incomplete structured construct at or before this point or other token.

        // type Foo<#List<'T>> = interface end
            // 1. Unexpected symbol # in type name. Expected infix operator, quote symbol or other token.
            // 2. Unexpected keyword 'end' in definition. Expected incomplete structured construct at or before this point or other token.

        // type Foo<'T, 'a when 'T :> List<'a>> = interface end
            // 1. Invalid constraint: the type used for the constraint is sealed, which means the constraint could only be satisfied by at most one solution
            // 2. This construct causes code to be less generic than indicated by the type annotations. The type variable 'T has been constrained to be type 'List<'a>'.
            // 3. This type parameter has been used in a way that constrains it to always be 'List<'a>'

        (*
        Of course, the oo way to do this is to:

        type MyListTypeWrapper<'T> =
            // .. list 't things
        *)
        ()

    module Value =
        // exists 'a . F<'a> = forall 'ret . (forall 'a . F<'a> -> 'ret) -> 'ret
        type Existential =
            abstract member Apply : Universal<'R> -> 'R
        and Universal<'R> =
            abstract member Eval<'T> : 'T -> 'R

    module List =
        // exists 'a . 'a list = forall 'ret . (forall 'a . 'a list -> 'ret) -> 'ret
        type ExistsList =
            abstract member Apply : ForallList<'R> -> 'R
        and ForallList<'R> =
            abstract member Eval<'T> : List<'T> -> 'R
                                     // ^^^^^^^^ F<'a> = 'a list

        module ListOps =
            let make (xs : List<'t>) : ExistsList =
                { new ExistsList with
                    member _.Apply forall = forall.Eval xs
                }

            let length (aList: ExistsList) : int =
                { new ForallList<int> with
                    member _.Eval xs = List.length xs
                } |> aList.Apply

            let reverse (aList: ExistsList) : ExistsList =
                { new ForallList<ExistsList> with
                    member _.Eval xs = List.rev xs |> make
                } |> aList.Apply

            let head<'T> (aList: ExistsList) : 'T =
                { new ForallList<'T> with
                    member _.Eval xs = List.head xs :> obj :?> 'T
                        // Runtime error if we try to get underlying value as the wrong type
                } |> aList.Apply
                

            module App =
                let intXs = make [0;1]
                let strXs = make ["hi";"there"]

                let intXsLen = length intXs
                let strXsLen = length strXs

                let reversedIntXs = reverse intXs
                    // val reversedIntXs : ExistsList
                let reversedStrXs = reverse intXs

                // let intX = head intXs
                    // 1. Value restriction. The value 'intX' has been inferred to have generic type
                           // val intX: '_a    
                       // Either define 'intX' as a simple data term, make it a function with explicit arguments or, if you do not intend for it to be generic, add a type annotation.
                let intX : int = head intXs // runtime cast

module AdhocWitness =
    (*
    Adhoc polymorphism through explicit witness passing

    Inspiration: https://stackoverflow.com/a/7224269/16289574
    *)
    type Foo = Foo
        // SDU with single marker constructor acts as explicit witness of ^!
        with
            static member (^!) (Foo, x: int) = x + 1
            static member (^!) (Foo, x: string) = "hi" + x

    let cantDoThis () =
        let inline both f (x, y) = f x, f y
        // let _ = both id (0, 'a')
            // This expression was expected to have type 'int'     but here has type 'char'    
        ()

    module App =
        let inline hatBoth f (x, y) = f ^! x, f ^! y
        let y = hatBoth Foo (0, "hi") // Pass witness of HatBang
        let y' = hatBoth Foo ("there", 1)
        // let y = hatBoth Foo (0, 'a')
            // 1. No overloads match for method 'op_HatBang'.

        let inline both f (x, y) = f x, f y // still can't do this though
        // let x _ = both hatBoth (0, "hey")
            // 1. This expression was expected to have type
                   // 'int'    
               // but here has type
                   // 'string'    

        module SecondWitness =
            type Bar = Bar with
                static member (^!) (Bar, x: int) = x + 2
                static member (^!) (Bar, x: bool) = not x

            let x = hatBoth Bar (0, false)
            // let x' = hatBoth Bar (0, "hi")
                // 1. No overloads match for method 'op_HatBang'.
