open System
open System.IO
open Fluff.Core
open ToolBox.AppEnvironment
open ToolBox.Core.Mapping
open Wikd
open Wikd.DataStore
open Wikd.Renderer
open Wikd.Tools

type AppArgs =
    | Import of ImportOptions
    | Build of BuildOptions
    | Clean of CleanOptions

and ImportOptions =
    { [<ArgValue("-s", "--store")>]
      StorePath: string
      [<ArgValue("-p", "--path")>]
      RootPath: string
      [<ArgValue("-o", "--output")>]
      OutputPath: string
      [<ArgValue("-f", "--force")>]
      Force: bool }

and BuildOptions =
    { [<ArgValue("-s", "--store")>]
      StorePath: string
      [<ArgValue("-o", "--output")>]
      OutputPath: string }

and CleanOptions =
    { [<ArgValue("-s", "--store")>]
      StorePath: string }


//let files = scanDirectory true "C:\\Users\\44748\\Projects\\Wikd\\Documents"

//let md1, lines1 = FDOM.Core.Parsing.Parser.ExtractMetadata (File.ReadAllLines "C:\\Users\\44748\\Projects\\Wikd\\Documents\\overview\\index.md" |> List.ofArray)
//let md2, lines2 = FDOM.Core.Parsing.Parser.ExtractMetadata (File.ReadAllLines "C:\\Users\\44748\\Projects\\Wikd\\Documents\\overview\\stand_alone\\stand_alone_examples.md" |> List.ofArray) //FDOM.Core.Parsing.Parser().ParseLinesAndMetadata

let options =
    ArgParser.tryGetOptions<AppArgs> (Environment.GetCommandLineArgs() |> List.ofArray)


let store =
    WikdStore.Create "C:\\ProjectData\\wikd\\wikd.db"


let settings =
    {
        Mode = RenderMode.Dynamic
        Styles =
            [ "../css/wikd.css"
              "../css/prism.css" ]
        Scripts =
            [ "../js/wikd.js"; "../js/prism.js" ]
        Icons = Some <| FontAwesome ""
    }

import store printResult "wikd" "C:\\Users\\44748\\Projects\\Wikd\\Documents"

File.ReadAllText "C:\\Users\\44748\\Projects\\Wikd\\Resources\\wikd_template.mustache"
|> Mustache.parse
|> Renderer.run store "C:\\ProjectData\\wikd\\wiki-dynamic" settings

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
