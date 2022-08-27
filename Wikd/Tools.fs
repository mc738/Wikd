namespace Wikd

open System
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open Wikd.DataStore

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

    [<RequireQualifiedAccess>]
    type ImportResult =
        | PageAdded of Name : string
        | VersionAdded of Name : string
        | Skipped of Message : string
        | Failure of Message : string

    let importFile (store: WikdStore) (parent: string option) (directory: string option) (file: WikdFile) =
        try
            let metadata, lines =
                File.ReadAllLines file.Path
                |> List.ofArray
                |> FDOM.Core.Parsing.Parser.ExtractMetadata

            let name =
                metadata.TryFind "wikd:name"
                |> Option.defaultValue file.Name

            let title =
                metadata.TryFind "wikd:title"
                |> Option.defaultValue file.Name

            let isDraft =
                metadata.TryFind "wikd:draft"
                |> Option.bind (fun v ->
                    match Boolean.TryParse v with
                    | true, b -> Some b
                    | false, _ -> None)
                |> Option.defaultValue false

            let icon =
                metadata.TryFind "wikd:icon"
            
            use ms =
                new MemoryStream(
                    lines
                    |> String.concat Environment.NewLine
                    |> Encoding.UTF8.GetBytes
                )

            match store.GetLatestPageVersion name with
            | Some pv ->
                // compare hashes.
                let hash = hashStream (SHA256.Create()) ms

                match String.Equals(hash, pv.Hash, StringComparison.OrdinalIgnoreCase), pv.IsDraft, isDraft with
                | true, true, true
                | true, false, false
                | true, true, false -> ImportResult.Skipped $"Page `{name}` not updated."
                | true, false, true
                | false, _, _ ->
                    ms.Position <- 0L
                    store.AddPageVersion(Guid.NewGuid().ToString("n"), name, ms, isDraft)
                    ImportResult.VersionAdded name
            | None ->
                // Page doesn't exist.
                store.AddPage(name, title, parent, directory, icon)
                store.AddPageVersion(Guid.NewGuid().ToString("n"), name, ms, isDraft)
                ImportResult.PageAdded name

        with
        | exn -> ImportResult.Failure exn.Message

    let printResult (result: ImportResult) =
        match result with
        | ImportResult.PageAdded name ->
            printfn $"Page `{name}` added"
        | ImportResult.VersionAdded name ->
            printfn $"Page `{name}` version added"
        | ImportResult.Skipped message ->
            printfn $"Page skipped. Reason: {message}"
        | ImportResult.Failure message ->
            printfn $"Page import failed. Error: {message}"

    let import (store: WikdStore) (resultHandler: ImportResult -> unit) (name: string) (path: string) =
        let data = scanDirectory true path

        let rec importPages
            (directoryPaths: string list)
            (parent: string option)
            (directory: WikdDirectory)
            =

            let dir =
                match directoryPaths.IsEmpty with
                | true -> None
                | false -> directoryPaths |> String.concat "/" |> Some

            match directory.IndexFile with
            | Some index ->
                importFile store parent dir { index with Name = directory.Name }
                |> resultHandler
            | None -> ()
            
            directory.Files
            |> List.map (importFile store (Some directory.Name) dir)
            |> List.iter resultHandler
            
            directory.Children
            |> List.iter (importPages (directoryPaths @ [ directory.Name ]) (Some directory.Name))
            
        importPages [] None { data with Name = name }