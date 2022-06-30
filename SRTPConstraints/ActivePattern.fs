namespace SRTPConstraints.ActivePattern
// From comment: https://github.com/fsharp/fslang-suggestions/issues/1089#issuecomment-948704124

module ClassBased =
    module Constraints =
        let inline (|IsQuacker|) (x: ^a when 'a : (member Quack : unit -> string)) = x
            // When used in an inline function, can bubble up constraint
            // Still need to use SRTP call syntax (^a : ...)
        let inline (|Quack|) x = (^a : (member Quack : unit -> string) x)

        let inline (|Barker|)(x: ^a when 'a : (member Bark : unit -> string)) =
            fun () -> (^a : (member Bark : unit -> string) (x))

    module Sayers =
        open Constraints

        let inline barkButCanAlsoQuack x =
            let (IsQuacker _) = x // x has Quack, need to use SRTP  to call
            let (Barker bark) = x  // x has Bark

            bark()

        let inline quack (IsQuacker x) =
                // Still need to use SRTP to call Quack
                // unless this lands:
                // https://github.com/fsharp/fslang-suggestions/issues/641
                // though it's closed now :(
            (^a : (member Quack : unit -> string) x)

    module Say =
        open Sayers
        open SRTPConstraints.Types.Classes

        ignore <| quack (Foo())
        ignore <| quack (Bar())
        // ignore <| quack (Baz())
            // 1. The type 'Baz' does not support the operator 'Quack'

        // ignore <| barkButCanAlsoQuack (Foo())
            // 1. The type 'Foo' does not support the operator 'Bark'
        ignore <| barkButCanAlsoQuack (Bar())
        // ignore <| barkButCanAlsoQuack (Baz())
            // 1. The type 'Baz' does not support the operator 'Quack'


module RecordBased =
    // https://github.com/fsharp/fslang-suggestions/issues/1153
    // Allow record fields to implement signature members
    // 
    // When this lands the record will be uniform with the class definition

    module Constraints =
        let inline (|Quacker|) x = (^a : (member Quack : (unit -> string)) (x))
            // Gets the quack field
        let inline (|Barker|) x = (^a : (member Bark : (unit -> string)) (x))

    module Sayers =
        open Constraints 

        let inline quack (Quacker quack) = quack()
        let inline bark (Barker bark) = bark()
        let inline barkAndQuack x =
            let (Barker bark) = x
            let (Quacker quack) = x
            bark() + quack()

    module Say =
        open SRTPConstraints.Types.Records
        open Sayers
        
        let aFoo = {
            Quack = fun () -> "Foo quacks"
        }

        let aBar = {
            Quack = fun () -> "Bar quacks"
            Bark = fun () -> "Bar barks"
        }

        let aBaz = {
            Bark = fun () -> "Baz barks"
        }

        ignore <| quack aFoo
        ignore <| quack aBar
        // ignore <| quack aBaz
            // 1. The type 'Baz' does not support the operator 'get_Quack'

        // ignore <| barkAndQuack aFoo
            // 1. The type 'Foo' does not support the operator 'get_Bark'
        ignore <| barkAndQuack aBar
        // ignore <| barkAndQuack aBaz
            // 1. The type 'Baz' does not support the operator 'get_Quack'

