﻿namespace Wikd

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.RegularExpressions
open FDOM.Core.Common
open FDOM.Rendering
open Fluff.Core
open FsToolbox.Core
open Wikd.DataStore
open Wikd.Persistence


module Renderer =

    [<RequireQualifiedAccess>]
    type RenderMode =
        | Static
        | Dynamic

        static member TryDeserialize(str: string) =
            match str.ToLower() with
            | "static" -> Ok RenderMode.Static
            | "dynamic" -> Ok RenderMode.Dynamic
            | _ -> Error $"Unknown render mode `{str}`"

        static member Deserialize(str: string) =
            match RenderMode.TryDeserialize str with
            | Ok rm -> Some rm
            | Error _ -> None

        member rm.Serialize() =
            match rm with
            | Static -> "static"
            | Dynamic -> "dynamic"

    type IconType =
        | FontAwesome of KitUrl: string

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "type" json with
            | Some "font-awesome" ->
                match Json.tryGetStringProperty "kitUrl" json with
                | Some kitUrl -> FontAwesome kitUrl |> Ok
                | None -> Error "Missing `kitUrl` property"
            | Some t -> Error $"Unknown icon type `{t}`"
            | None -> Error "Missing `type` property"

        static member FromJson(json: JsonElement) =
            match IconType.TryFromJson json with
            | Ok it -> Some it
            | Error e -> None

    type RendererSettings =
        { Mode: RenderMode
          Icons: IconType option
          Styles: string list
          Scripts: string list
          IndexHeaderLevel: IndexHeaderLevel }

    and IndexHeaderLevel =
        { IndexH1: bool
          IndexH2: bool
          IndexH3: bool
          IndexH4: bool
          IndexH5: bool
          IndexH6: bool }

        static member Default() =
            { IndexH1 = false
              IndexH2 = false
              IndexH3 = false
              IndexH4 = false
              IndexH5 = false
              IndexH6 = false }

    type PageItem =
        { Name: string
          DisplayName: string
          Icon: string
          Order: int
          Children: PageItem list }

    type WikdParameters =
        { RootPath: string
          NavBarHtml: string option
          Template: Mustache.Template
          RendererSettings: RendererSettings }


    let toLines (str: string) =
        str.Split(Environment.NewLine) |> List.ofArray

    let rec getPages (store: WikdStore, page: Records.Page) =
        let children =
            store.GetPagesForParent page.Name
            |> List.filter (fun p -> p.Active)
            |> List.map (fun cp -> getPages (store, cp))
            |> List.sortBy (fun pd -> pd.Order)

        { Name = page.Name
          DisplayName = page.DisplayName
          Icon = page.Icon |> Option.defaultValue "fas fa-file-alt"
          Order = page.PageOrder
          Children = children }

    let rec createStaticIndex (pages: PageItem list) =
        //let indexItems =

        pages
        |> List.map (fun p ->
            let children = createStaticIndex (p.Children |> List.sortBy (fun p -> p.Order)) |> String.concat ""

            // Test
            //$"""<details class="index-item static"><summary><a href="./{p.Name}.html">{p.DisplayName}</a></summary><div>{children}</div></details>"""


            $"""<div class="index-item static"><a href="./{p.Name}.html">{p.DisplayName}</a>{children}</div>""")

    let rec createDynamicIndex (level: int) (pages: PageItem list) =
        //let indexItems =

        pages
        |> List.map (fun p ->
            let children = createDynamicIndex (level + 1) (p.Children) |> String.concat ""

            let id = System.Guid.NewGuid().ToString("n").Substring(0, 6)

            match p.Children.IsEmpty with
            | true ->
                $"""<div id="{id}" class="index-item dynamic page"><div class="item-title" data-indent="{level}"><p style="margin-left: 20px" onclick="fetchPage('./{p.Name}.html', '{id}')"><span style="margin-left: 5px; margin-right: 5px"><i class="{p.Icon}"></i></span>{p.DisplayName}</p></div></div>"""
            | false ->
                $"""<div id="{id}" class="index-item dynamic"><div class="item-title" data-indent="{level}" style="display: flex;"><button class="expand-btn" onclick="expand('{id}')"><span><i id="{id}-icon" class="fas fa-chevron-right"></i></span></button><p onclick="fetchPage('./{p.Name}.html', '{id}')"><span style=" margin-left: 5px; margin-right: 5px"><i class="{p.Icon}"></i></span>{p.DisplayName}</p></div><div class="children">{children}</div></div>""")

    let rewriteLinks (content: DOM.InlineContent) =
        match content with
        | DOM.InlineContent.Link link ->
            let capture = Regex.Match(link.Url, "[^/]+(?=\.md$)")

            let url =
                match capture.Success with
                | true -> $"{capture.Value}.html"
                | false -> link.Url

            { link with Url = url } |> DOM.InlineContent.Link
        | DOM.InlineContent.Span span -> span |> DOM.InlineContent.Span
        | DOM.InlineContent.Text text -> text |> DOM.InlineContent.Text

    let blockRewriter
        (indexHeaderLevel: IndexHeaderLevel)
        (contentRewriter: DOM.InlineContent -> DOM.InlineContent)
        (block: DOM.BlockContent)
        =
        match block with
        | DOM.BlockContent.Header h ->
            let index =
                match h.Level with
                | DOM.HeaderLevel.H1 -> indexHeaderLevel.IndexH1
                | DOM.HeaderLevel.H2 -> indexHeaderLevel.IndexH2
                | DOM.HeaderLevel.H3 -> indexHeaderLevel.IndexH3
                | DOM.HeaderLevel.H4 -> indexHeaderLevel.IndexH4
                | DOM.HeaderLevel.H5 -> indexHeaderLevel.IndexH5
                | DOM.HeaderLevel.H6 -> indexHeaderLevel.IndexH6

            { h with
                Indexed = index
                Content = h.Content |> List.map contentRewriter }
            |> DOM.BlockContent.Header
        | DOM.BlockContent.Paragraph p ->
            { p with
                Content = p.Content |> List.map contentRewriter }
            |> DOM.BlockContent.Paragraph
        | DOM.BlockContent.Code c ->
            { c with
                Content = c.Content |> List.map contentRewriter }
            |> DOM.BlockContent.Code
        | DOM.BlockContent.List l ->
            { l with
                Items =
                    l.Items
                    |> List.map (fun li ->
                        { li with
                            Content = li.Content |> List.map contentRewriter }) }
            |> DOM.BlockContent.List
        | DOM.BlockContent.Image _ -> block

    let rec renderStaticPages
        (store: WikdStore)
        (template: Mustache.Token list)
        (data: Mustache.Data)
        (rootPath: string)
        (indexHeaderLevel: IndexHeaderLevel)
        (page: PageItem)
        =
        //let outputDir = Path.Combine(rootPath, path)

        if Directory.Exists rootPath |> not then
            Directory.CreateDirectory rootPath |> ignore

        store.GetLatestPageVersion page.Name
        |> Option.iter (fun pv ->
            pv.RawBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> toLines
            |> FDOM.Core.Parsing.Parser.ParseLines
            |> fun p -> p.CreateBlockContent()
            |> List.map (blockRewriter indexHeaderLevel rewriteLinks)
            |> Html.renderBlocksWithTemplate template data
            |> fun r -> File.WriteAllText(Path.Combine(rootPath, $"{page.Name}.html"), r))

        page.Children
        |> List.iter (renderStaticPages store template data rootPath indexHeaderLevel)

    type PageData =
        { Version: int
          CreatedOn: DateTime
          Hash: string }

    let rec renderDynamicPages (store: WikdStore) (rootPath: string) (page: PageItem) =

        // TODO - index header level.

        store.GetLatestPageVersion page.Name
        |> Option.iter (fun pv ->
            pv.RawBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> toLines
            |> FDOM.Core.Parsing.Parser.ParseLines
            |> fun p -> p.CreateBlockContent()
            |> List.map (blockRewriter (IndexHeaderLevel.Default()) rewriteLinks)
            |> Html.renderArticle
            |> fun r -> File.WriteAllText(Path.Combine(rootPath, $"{page.Name}.html"), r))

        // example:
        // <article>...</article>
        // ###METADATA###
        // { "createdOn": "", "version": 0 }

        page.Children |> List.iter (renderDynamicPages store rootPath)

    let run (store: WikdStore) (parameters: WikdParameters) =

        let pages = store.GetTopLevelPages() |> List.map (fun tlp -> getPages (store, tlp))

        // "../css/wikd.css"
        // "../js/wikd.js"

        let styles =
            parameters.RendererSettings.Styles
            |> List.map (fun s -> [ "url", Mustache.Value.Scalar s ] |> Map.ofList |> Mustache.Value.Object)
            |> Mustache.Array

        let sharedData =
            [ "styles",
              parameters.RendererSettings.Styles
              |> List.map (fun s -> [ "url", Mustache.Value.Scalar s ] |> Map.ofList |> Mustache.Value.Object)
              |> Mustache.Array
              "scripts",
              parameters.RendererSettings.Scripts
              |> List.map (fun s -> [ "url", Mustache.Value.Scalar s ] |> Map.ofList |> Mustache.Value.Object)
              |> Mustache.Array
              match parameters.RendererSettings.Icons with
              | Some icons ->
                  match icons with
                  | FontAwesome kitUrl ->
                      "icon_script", [ "url", Mustache.Value.Scalar kitUrl ] |> Map.ofList |> Mustache.Value.Object
              | None -> ()
              match parameters.NavBarHtml with
              | Some navBar -> "nav_html", Mustache.Scalar navBar
              | None -> () ]

        match parameters.RendererSettings.Mode with
        | RenderMode.Static ->

            let data =
                sharedData
                @ [ "index", Mustache.Value.Scalar <| (createStaticIndex pages |> String.concat "") ]
                |> Map.ofList
                |> fun v -> ({ Values = v; Partials = Map.empty }: Mustache.Data)

            pages
            |> List.iter (
                renderStaticPages
                    store
                    parameters.Template
                    data
                    parameters.RootPath
                    parameters.RendererSettings.IndexHeaderLevel
            )
        | RenderMode.Dynamic ->
            // Create the wiki page - index.html
            let data =
                sharedData
                @ [ "index", Mustache.Value.Scalar <| (createDynamicIndex 1 pages |> String.concat "") ]
                |> Map.ofList
                |> fun v -> ({ Values = v; Partials = Map.empty }: Mustache.Data)

            // Create index.

            Mustache.replace data true parameters.Template
            |> fun r -> File.WriteAllText(Path.Combine(parameters.RootPath, $"index.html"), r)

            pages |> List.iter (renderDynamicPages store parameters.RootPath)
