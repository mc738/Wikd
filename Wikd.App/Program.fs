open System.IO
open Fluff.Core
open Wikd
open Wikd.DataStore
open Wikd.Renderer
open Wikd.Tools

//let files = scanDirectory true "C:\\Users\\44748\\Projects\\Wikd\\Documents"

//let md1, lines1 = FDOM.Core.Parsing.Parser.ExtractMetadata (File.ReadAllLines "C:\\Users\\44748\\Projects\\Wikd\\Documents\\overview\\index.md" |> List.ofArray)
//let md2, lines2 = FDOM.Core.Parsing.Parser.ExtractMetadata (File.ReadAllLines "C:\\Users\\44748\\Projects\\Wikd\\Documents\\overview\\stand_alone\\stand_alone_examples.md" |> List.ofArray) //FDOM.Core.Parsing.Parser().ParseLinesAndMetadata

let store =
    WikdStore.Create "C:\\ProjectData\\wikd\\wikd.db"

let styles =
    [ "../css/wikd.css"
      "../css/prism.css" ]

let scripts =
    [ "../js/wikd.js"; "../js/prism.js" ]

import store printResult "wikd" "C:\\Users\\44748\\Projects\\Wikd\\Documents"

File.ReadAllText "C:\\Users\\44748\\Projects\\Wikd\\Resources\\wikd_template.mustache"
|> Mustache.parse
|> Renderer.run store "C:\\ProjectData\\wikd\\wiki-dynamic" styles scripts RenderMode.Dynamic

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
