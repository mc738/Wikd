namespace Wikd

open System
open System.IO
open System.Text
open FDOM.Rendering
open Fluff.Core
open Wikd.DataStore
open Wikd.Persistence


module Renderer =

    type PageItem =
        { Name: string
          DisplayName: string
          Children: PageItem list }

    let toLines (str: string) =
        str.Split(Environment.NewLine) |> List.ofArray

    let rec getPages (store: WikdStore, page: Records.Page) =
        let children =
            store.GetPagesForParent page.Name
            |> List.map (fun cp -> getPages (store, cp))

        { Name = page.Name
          DisplayName = page.DisplayName
          Children = children }


    let rec createIndex (pages: PageItem list) =
        //let indexItems =
        
        
        
        pages
        |> List.map (fun p ->
            let children = createIndex (p.Children) |> String.concat ""
            $"""<div class="index-item"><a href="./{p.Name}.html">{p.DisplayName}</a>{children}</div>""")
        //|> String.concat ""
        ///|> List.concat ""
            
        (*    [ "name", Mustache.Value.Scalar p.Name
              "url", Mustache.Value.Scalar $"./{p.Name}.html"
              match p.Children.IsEmpty |> not with
              | true -> "children", createIndex (p.Children)
              | false -> () ]
            |> Map.ofList
            |> Mustache.Value.Object)
        |> Mustache.Array*)




    let rec renderPages
        (store: WikdStore)
        (template: Mustache.Token list)
        (data: Mustache.Data)
        (rootPath: string)
        (page: PageItem)
        =
        //let outputDir = Path.Combine(rootPath, path)

        //if Directory.Exists outputDir |> not then Directory.CreateDirectory outputDir |> ignore

        store.GetLatestPageVersion page.Name
        |> Option.iter (fun pv ->
            pv.RawBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> toLines
            |> FDOM.Core.Parsing.Parser.ParseLines
            |> fun p -> p.CreateBlockContent()
            |> Html.renderBlocksWithTemplate template data
            |> fun r -> File.WriteAllText(Path.Combine(rootPath, $"{page.Name}.html"), r))

        page.Children
        |> List.iter (renderPages store template data rootPath)


    let run (store: WikdStore) (rootPath: string) (template: Mustache.Token list) =

        let pages =
            store.GetTopLevelPages()
            |> List.map (fun tlp -> getPages (store, tlp))

        [ "index", Mustache.Value.Scalar <| (createIndex pages |> String.concat "")
          "css_url", Mustache.Value.Scalar "../css/wikd.css"
          "script_url", Mustache.Value.Scalar "../js/wikd.js" ]
        |> Map.ofList
        |> fun v -> ({ Values = v; Partials = Map.empty }: Mustache.Data)
        |> fun d ->
            pages |> List.iter (renderPages store template d rootPath)
