module Navigation

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Elmish


type Model = { IsBurgerOpen: bool }
type Msg = | FlipNavbar

let private navbarEnd =
    Navbar.End.div [] [
        Navbar.Item.div [] [
            Field.div [ Field.IsGrouped ] [
                Control.p [] [
                    Button.a [ Button.Props [ Href "https://github.com/rtkelly13/blobflags" ] ] [
                        Icon.icon [] [
                            Fa.i [ Fa.Brand.Github ] []
                        ]
                        span [] [ str "Source" ]
                    ]
                ]
            ]
        ]
    ]

let private navbarStart dispatch =
    Navbar.Start.div [] [
        Navbar.Item.a [] [ str "Home" ]
        Navbar.Item.div [ Navbar.Item.HasDropdown
                          Navbar.Item.IsHoverable ] [
            Navbar.Link.div [] [ str "Options" ]
            Navbar.Dropdown.div [] [
                Navbar.Item.a [ Navbar.Item.Props [ OnClick(fun _ -> dispatch FlipNavbar) ] ] [
                    str "Reset demo"
                ]
            ]
        ]
    ]

let private navbarView isBurgerOpen dispatch =
    div [ ClassName "navbar-bg" ] [
        Container.container [] [
            Navbar.navbar [ Navbar.CustomClass "is-primary" ] [
                Navbar.Brand.div [] [
                    Navbar.Item.a [ Navbar.Item.Props [ Href "#" ] ] [
                        Image.image [ Image.Is32x32 ] [
                            img [ Src "logo/blobflags.svg" ]
                        ]
                        Heading.p [ Heading.Is4 ] [
                            str "blobflags"
                        ]
                    ]
                    // Icon display only on mobile
                    Navbar.Item.a [ Navbar.Item.Props [ Href "https://github.com/rtkelly13/blobflags" ]
                                    Navbar.Item.CustomClass "is-hidden-desktop" ] [
                        Icon.icon [] [
                            Fa.i [ Fa.Brand.Github; Fa.Size Fa.FaLarge ] []
                        ]
                    ]
                    // Make sure to have the navbar burger as the last child of the brand
                    Navbar.burger [ Navbar.Burger.CustomClass((if isBurgerOpen then "is-active" else "")) ] [
                        span [] []
                        span [] []
                        span [] []
                    ]
                ]
                Navbar.menu [ Navbar.Menu.IsActive isBurgerOpen ] [
                    navbarStart dispatch
                    navbarEnd
                ]
            ]
        ]
    ]

let navBrand (model: Model) (dispatch: Msg -> unit) =
    div [ ClassName "navbar-bg" ] [
        Container.container [] [
            navbarView model.IsBurgerOpen dispatch
        ]
    ]


let init (): Model = { IsBurgerOpen = false }

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | FlipNavbar ->
        ({ model with
               IsBurgerOpen = not model.IsBurgerOpen },
         Cmd.none)

let view (model: Model) (dispatch: Msg -> unit) = navBrand model dispatch
