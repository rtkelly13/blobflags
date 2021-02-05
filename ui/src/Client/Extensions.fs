module Extensions

open Elmish

let mapUpdate (modelMap: 'a -> 'c) (msgMap: 'b -> 'd) (item: 'a * Cmd<'b>) =
    let (model, cmd) = item
    (modelMap model, Cmd.map msgMap cmd )