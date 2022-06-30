namespace SRTPConstraints

module Types =
    module Classes =
        type Foo() = 
            member _.Quack () = "Foo quacks"

        type Bar() =
            member _.Quack () = "Bar quacks"
            member _.Bark () = "Bar barks"

        type Baz() =
            member _.Bark () = "Baz barks"

    module Records =
        type Foo =
            { Quack : unit -> string
            }

        type Bar =
            { Quack : unit -> string
              Bark  : unit -> string
            }

        type Baz =
            { Bark : unit -> string
            }

