namespace Wikd

open System.IO
open System.Text.RegularExpressions

module Tools =

    type WikdDirectory =
        { Path: string
          Name: string
          Files: WikdFile list
          IndexFile: WikdFile option
          Children: WikdDirectory list }

    and WikdFile = { Path: string; Name: string }

    let tryGet<'T> (predict: 'T -> bool) (values: 'T list) =
        values
        |> List.tryFind predict
        |> Option.map (fun v -> Some v, values |> List.filter (predict >> not))
        |> Option.defaultWith (fun _ -> None, values)

    let getIndex (files: WikdFile list) =
        tryGet (fun f -> f.Name = "index") files

    let rec scanDirectory (scanChildren: bool) (path: string) =

        let files =
            Directory.EnumerateFiles(path)
            |> List.ofSeq
            |> List.filter (fun f -> Regex.IsMatch(f, "^*.md$"))
            |> List.map (fun f ->
                ({ Path = f
                   Name = Path.GetFileNameWithoutExtension f }: WikdFile))

        let (indexFile, otherFiles) = getIndex files

        let children =
            match scanChildren with
            | true ->
                Directory.EnumerateDirectories(path)
                |> List.ofSeq
                |> List.map (scanDirectory true)
            | false -> []

        ({ Path = path
           Name = Path.GetFileNameWithoutExtension path
           Files = otherFiles
           IndexFile = indexFile
           Children = children }: WikdDirectory)

    