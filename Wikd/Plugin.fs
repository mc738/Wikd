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
              Scripts = wc.Scripts }

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
