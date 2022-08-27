namespace Wikd

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open FDOM.Core.Common
open FDOM.Rendering
open Fluff.Core
open Wikd.DataStore
open Wikd.Persistence


module Renderer =

    [<RequireQualifiedAccess>]
    type RenderMode =
        | Static
        | Dynamic

    type IconType = FontAwesome of KitUrl: string

    type RendererSettings =
        { Mode: RenderMode
          Icons: IconType option
          Styles: string list
          Scripts: string list }


    type PageItem =
        { Name: string
          DisplayName: string
          Icon: string
          Children: PageItem list }

    let toLines (str: string) =
        str.Split(Environment.NewLine) |> List.ofArray

    let rec getPages (store: WikdStore, page: Records.Page) =
        let children =
            store.GetPagesForParent page.Name
            |> List.map (fun cp -> getPages (store, cp))

        { Name = page.Name
          DisplayName = page.DisplayName
          Icon = page.Icon |> Option.defaultValue "fas fa-file-alt"
          Children = children }


    let rec createStaticIndex (pages: PageItem list) =
        //let indexItems =

        pages
        |> List.map (fun p ->
            let children =
                createStaticIndex (p.Children) |> String.concat ""

            $"""<div class="index-item static"><a href="./{p.Name}.html">{p.DisplayName}</a>{children}</div>""")

    let rec createDynamicIndex (level: int) (pages: PageItem list) =
        //let indexItems =

        pages
        |> List.map (fun p ->
            let children =
                createDynamicIndex (level + 1) (p.Children)
                |> String.concat ""

            let id =
                System
                    .Guid
                    .NewGuid()
                    .ToString("n")
                    .Substring(0, 6)

            match p.Children.IsEmpty with
            | true ->
                $"""<div id="{id}" class="index-item dynamic page"><div class="item-title" data-indent="{level}"><p style="margin-left: 20px" onclick="fetchPage('./{p.Name}.html', '{id}')"><span style="margin-left: 5px; margin-right: 5px"><i class="{p.Icon}"></i></span>{p.DisplayName}</p></div></div>"""
            | false ->
                $"""<div id="{id}" class="index-item dynamic"><div class="item-title" data-indent="{level}" style="display: flex;"><button class="expand-btn" onclick="expand('{id}')"><span><i id="{id}-icon" class="fas fa-chevron-right"></i></span></button><p onclick="fetchPage('./{p.Name}.html', '{id}')"><span style=" margin-left: 5px; margin-right: 5px"><i class="{p.Icon}"></i></span>{p.DisplayName}</p></div><div class="children">{children}</div></div>""")

    let rewriteLinks (content: DOM.InlineContent) =
        match content with
        | DOM.InlineContent.Link link ->
            let capture =
                Regex.Match(link.Url, "[^/]+(?=\.md$)")

            let url =
                match capture.Success with
                | true -> $"{capture.Value}.html"
                | false -> link.Url

            { link with Url = url } |> DOM.InlineContent.Link
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


    let rec renderStaticPages
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
        |> List.iter (renderStaticPages store template data rootPath)

    type PageData =
        { Version: int
          CreatedOn: DateTime
          Hash: string }

    let rec renderDynamicPages (store: WikdStore) (rootPath: string) (page: PageItem) =


        store.GetLatestPageVersion page.Name
        |> Option.iter (fun pv ->
            pv.RawBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> toLines
            |> FDOM.Core.Parsing.Parser.ParseLines
            |> fun p -> p.CreateBlockContent()
            |> List.map (blockRewriter rewriteLinks)
            |> Html.renderArticle
            |> fun r -> File.WriteAllText(Path.Combine(rootPath, $"{page.Name}.html"), r))

        // example:
        // <article>...</article>
        // ###METADATA###
        // { "createdOn": "", "version": 0 }

        page.Children
        |> List.iter (renderDynamicPages store rootPath)

    let run (store: WikdStore) (rootPath: string) (settings: RendererSettings) (template: Mustache.Token list) =

        let pages =
            store.GetTopLevelPages()
            |> List.map (fun tlp -> getPages (store, tlp))

        // "../css/wikd.css"
        // "../js/wikd.js"

        let styles =
            settings.Styles
            |> List.map (fun s ->
                [ "url", Mustache.Value.Scalar s ]
                |> Map.ofList
                |> Mustache.Value.Object)
            |> Mustache.Array

        let sharedData =
            [ "styles",
              settings.Styles
              |> List.map (fun s ->
                  [ "url", Mustache.Value.Scalar s ]
                  |> Map.ofList
                  |> Mustache.Value.Object)
              |> Mustache.Array
              "scripts",
              settings.Scripts
              |> List.map (fun s ->
                  [ "url", Mustache.Value.Scalar s ]
                  |> Map.ofList
                  |> Mustache.Value.Object)
              |> Mustache.Array
              match settings.Icons with
              | Some icons ->
                  match icons with
                  | FontAwesome kitUrl ->
                      "faScript",
                      [ "url", Mustache.Value.Scalar kitUrl ]
                      |> Map.ofList
                      |> Mustache.Value.Object
              | None -> () ]


        match settings.Mode with
        | RenderMode.Static ->

            let data =
                sharedData
                @ [ "index",
                    Mustache.Value.Scalar
                    <| (createStaticIndex pages |> String.concat "") ]
                |> Map.ofList
                |> fun v -> ({ Values = v; Partials = Map.empty }: Mustache.Data)

            pages
            |> List.iter (renderStaticPages store template data rootPath)
        | RenderMode.Dynamic ->
            // Create the wiki page - index.html
            let data =
                sharedData @
                [ "index",
                  Mustache.Value.Scalar
                  <| (createDynamicIndex 1 pages |> String.concat "") ]
                |> Map.ofList
                |> fun v -> ({ Values = v; Partials = Map.empty }: Mustache.Data)

            // Create index.

            Mustache.replace data true template
            |> fun r -> File.WriteAllText(Path.Combine(rootPath, $"index.html"), r)

            pages
            |> List.iter (renderDynamicPages store rootPath)
