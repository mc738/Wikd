namespace Wikd

open System.Text.Json
open FsToolbox.Core
open Wikd.Renderer

module Plugin =

    type WikdConfiguration =
        { StorePath: string
          TemplatePath: string
          Mode: RenderMode
          Icons: IconType option
          Styles: string list
          Scripts: string list }

        static member TryDeserialize(json: JsonElement) =
            match Json.tryGetStringProperty "storePath" json, Json.tryGetStringProperty "templatePath" json with
            | Some sp, Some tp ->
                { StorePath = sp
                  TemplatePath = tp
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
            | None, _ -> Error "Missing `storePath` property"
            | _, None -> Error "Missing `templatePath` property"

        member wc.GetRenderSettings() =
            { Mode = wc.Mode
              Icons = wc.Icons
              Styles = wc.Styles
              Scripts = wc.Scripts }
