namespace Wikd

open System.Text.Json
open FsToolbox.Core
open Wikd.Renderer

module Plugin =

    type WikdConfiguration =
        { StorePath: string
          ContentRootPath: string
          TemplatePath: string
          OutputPath: string
          NavBar: NavBarConfiguration option
          Mode: RenderMode
          Icons: IconType option
          IndexHeaderLevel1: bool
          IndexHeaderLevel2: bool
          IndexHeaderLevel3: bool
          IndexHeaderLevel4: bool
          IndexHeaderLevel5: bool
          IndexHeaderLevel6: bool
          Styles: string list
          Scripts: string list }

        static member TryDeserialize(json: JsonElement) =
            match
                Json.tryGetStringProperty "storePath" json,
                Json.tryGetStringProperty "contentRootPath" json,
                Json.tryGetStringProperty "templatePath" json,
                Json.tryGetStringProperty "outputPath" json
            with
            | Some sp, Some cr, Some tp, Some op ->
                { StorePath = sp
                  ContentRootPath = cr
                  TemplatePath = tp
                  OutputPath = op
                  NavBar = Json.tryGetProperty "navBar" json |> Option.bind NavBarConfiguration.Deserialize
                  Mode =
                    Json.tryGetStringProperty "mode" json
                    |> Option.bind RenderMode.Deserialize
                    |> Option.defaultValue RenderMode.Static
                  Icons = Json.tryGetProperty "icons" json |> Option.bind IconType.FromJson
                  IndexHeaderLevel1 = Json.tryGetBoolProperty "indexHeaderLevel1" json |> Option.defaultValue false
                  IndexHeaderLevel2 = Json.tryGetBoolProperty "indexHeaderLevel2" json |> Option.defaultValue false
                  IndexHeaderLevel3 = Json.tryGetBoolProperty "indexHeaderLevel3" json |> Option.defaultValue false
                  IndexHeaderLevel4 = Json.tryGetBoolProperty "indexHeaderLevel4" json |> Option.defaultValue false
                  IndexHeaderLevel5 = Json.tryGetBoolProperty "indexHeaderLevel5" json |> Option.defaultValue false
                  IndexHeaderLevel6 = Json.tryGetBoolProperty "indexHeaderLevel6" json |> Option.defaultValue false
                  Styles =
                    Json.tryGetProperty "styles" json
                    |> Option.bind Json.tryGetStringArray
                    |> Option.defaultValue []
                  Scripts =
                    Json.tryGetProperty "scripts" json
                    |> Option.bind Json.tryGetStringArray
                    |> Option.defaultValue [] }
                |> Ok
            | None, _, _, _ -> Error "Missing `storePath` property"
            | _, None, _, _ -> Error "Missing `contentRootPath` property"
            | _, _, None, _ -> Error "Missing `templatePath` property"
            | _, _, _, None -> Error "Missing `outputPath` property"

        member wc.GetRenderSettings() =
            { Mode = wc.Mode
              Icons = wc.Icons
              Styles = wc.Styles
              Scripts = wc.Scripts
              IndexHeaderLevel =
                { IndexH1 = wc.IndexHeaderLevel1
                  IndexH2 = wc.IndexHeaderLevel2
                  IndexH3 = wc.IndexHeaderLevel3
                  IndexH4 = wc.IndexHeaderLevel4
                  IndexH5 = wc.IndexHeaderLevel5
                  IndexH6 = wc.IndexHeaderLevel6 } }

    and NavBarConfiguration =
        { TemplateName: string
          DataPath: string }

        static member TryDeserialize(json: JsonElement) =
            match Json.tryGetStringProperty "templateName" json, Json.tryGetStringProperty "dataPath" json with
            | Some tn, Some dp -> Ok { TemplateName = tn; DataPath = dp }
            | None, _ -> Error "Missing `templateName` property"
            | _, None -> Error "Missing `dataPath` property"

        static member Deserialize(json: JsonElement) =
            match NavBarConfiguration.TryDeserialize json with
            | Ok nbc -> Some nbc
            | Error _ -> None
