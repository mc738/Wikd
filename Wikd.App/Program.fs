open System.IO
open Wikd.Tools

let files = scanDirectory true "C:\\Users\\44748\\Projects\\Wikd\\Documents"

let md1, lines1 = FDOM.Core.Parsing.Parser.ExtractMetadata (File.ReadAllLines "C:\\Users\\44748\\Projects\\Wikd\\Documents\\overview\\index.md" |> List.ofArray)
let md2, lines2 = FDOM.Core.Parsing.Parser.ExtractMetadata (File.ReadAllLines "C:\\Users\\44748\\Projects\\Wikd\\Documents\\overview\\stand_alone\\stand_alone_examples.md" |> List.ofArray) //FDOM.Core.Parsing.Parser().ParseLinesAndMetadata

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"