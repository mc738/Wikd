namespace Wikd

open System
open System.IO
open System.Text
open FDOM.Core.Common
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
            let children =
                createIndex (p.Children) |> String.concat ""

            $"""<div class="index-item"><a href="./{p.Name}.html">{p.DisplayName}</a>{children}</div>""")
    //|> String.concat ""
    ///|> List.concat ""
    ///|> List.concat ""
    ///|> List.concat ""
    ///|> List.concat ""
    ///|> List.concat ""
    ///|> List.concat ""
    ///|> List.concat ""
    ///|> List.concat ""

    (*    [ "name", Mustache.Value.Scalar p.Name
              "url", Mustache.Value.Scalar $"./{p.Name}.html"
              match p.Children.IsEmpty |> not with
              | true -> "children", createIndex (p.Children)
              | false -> () ]
            |> Map.ofList
            |> Mustache.Value.Object)
        |> Mustache.Array*)


    let rewriteLinks (content: DOM.InlineContent) =
        match content with
        | DOM.InlineContent.Link link ->
            { link with Url = link.Url.Replace(".md", ".html") }
            |> DOM.InlineContent.Link
        | DOM.InlineContent.Span span -> span |> DOM.InlineContent.Span
        | DOM.InlineContent.Text text -> text |> DOM.InlineContent.Text

    let blockRewriter (contentRewriter: DOM.InlineContent -> DOM.InlineContent) (block: DOM.BlockContent) =
        match block with
        | DOM.BlockContent.Header h ->
            { h with Content = h.Content |> List.map contentRewriter }
            |> DOM.BlockContent.Header
        | DOM.BlockContent.Paragraph p ->
            { p with Content = p.Content |> List.map contentRewriter }
            |> DOM.BlockContent.Paragraph
        | DOM.BlockContent.Code c ->
            { c with Content = c.Content |> List.map contentRewriter }
            |> DOM.BlockContent.Code
        | DOM.BlockContent.List l ->
            { l with
                Items =
                    l.Items
                    |> List.map (fun li -> { li with Content = li.Content |> List.map contentRewriter }) }
            |> DOM.BlockContent.List
        | DOM.BlockContent.Image _ -> block


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
            |> List.map (blockRewriter rewriteLinks)
            |> Html.renderBlocksWithTemplate template data
            |> fun r -> File.WriteAllText(Path.Combine(rootPath, $"{page.Name}.html"), r))

        page.Children
        |> List.iter (renderPages store template data rootPath)

    let run (store: WikdStore) (rootPath: string) (template: Mustache.Token list) =

        let pages =
            store.GetTopLevelPages()
            |> List.map (fun tlp -> getPages (store, tlp))

        [ "index",
          Mustache.Value.Scalar
          <| (createIndex pages |> String.concat "")
          "css_url", Mustache.Value.Scalar "../css/wikd.css"
          "script_url", Mustache.Value.Scalar "../js/wikd.js" ]
        |> Map.ofList
        |> fun v -> ({ Values = v; Partials = Map.empty }: Mustache.Data)
        |> fun d ->
            pages
            |> List.iter (renderPages store template d rootPath)
