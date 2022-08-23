namespace Wikd


module DataStore =


    open Freql.Core.Common.Types
    open Freql.Sqlite
    open Microsoft.Data.Sqlite
    open System
    open System.IO
    open System.Security.Cryptography
    open Wikd.Persistence

    module Internal =

        let initialize (ctx: SqliteContext) =
            [ Records.Category.CreateTableSql()
              Records.Page.CreateTableSql()
              Records.Resource.CreateTableSql()
              Records.Tag.CreateTableSql()
              Records.CategoryPage.CreateTableSql()
              Records.PageCategory.CreateTableSql()
              Records.PageTag.CreateTableSql()
              Records.PageVersion.CreateTableSql()
              Records.PageMetadataItem.CreateTableSql() ]
            |> List.iter (fun t -> ctx.ExecuteSqlNonQuery t |> ignore)

        let addPage (ctx: SqliteContext) (name: string) (displayName: string) (parent: string option) (order: int) (directory: string option) =

            ({ Name = name
               DisplayName = displayName
               Parent = parent
               CreatedOn = DateTime.UtcNow
               PageOrder = order
               Active = true
               Directory = directory }: Parameters.NewPage)
            |> Operations.insertPage ctx

        let addPageVersion
            (ctx: SqliteContext)
            (id: string)
            (page: string)
            (version: int)
            (stream: Stream)
            (isDraft: bool)
            =
            let hash =
                hashStream (SHA256.Create()) stream

            let bf = BlobField.FromStream stream

            ({ Id = id
               Page = page
               Version = version
               RawBlob = bf
               Hash = hash
               CreatedOn = DateTime.UtcNow
               IsDraft = isDraft }: Parameters.NewPageVersion)
            |> Operations.insertPageVersion ctx

        let getPage (ctx: SqliteContext) (name: string) =
            Operations.selectPageRecord ctx [ "WHERE name = @0" ] [ name ]

        let getAllActivePages (ctx: SqliteContext) =
            Operations.selectPageRecords ctx [ "WHERE active = 1" ] []

        let getPagesForParent (ctx: SqliteContext) (parent: string) =
            Operations.selectPageRecords
                ctx
                [ "WHERE parent = @0 AND active = 1"
                  "ORDER BY page_order" ]
                [ parent ]

        let getMaxOrderPageForParent (ctx: SqliteContext) (parent: string) =
            Operations.selectPageRecord
                ctx
                [ "WHERE parent = @0 AND active = 1"
                  "ORDER BY page_order DESC LIMIT 1" ]
                [ parent ]


        let getTopLevelPages (ctx: SqliteContext) =
            Operations.selectPageRecords
                ctx
                [ "WHERE parent IS NULL and active = 1"
                  "ORDER BY page_order" ]
                []

        let getMaxOrderTopLevelPage (ctx: SqliteContext) =
            Operations.selectPageRecord
                ctx
                [ "WHERE parent IS NULL and active = 1"
                  "ORDER BY page_order DESC LIMIT 1" ]
                []

        let getPageLatestVersion (ctx: SqliteContext) (page: string) =
            Operations.selectPageVersionRecord
                ctx
                [ "WHERE page = @0"
                  "ORDER BY version DESC LIMIT 1" ]
                [ page ]

    type WikdStore(ctx: SqliteContext) =

        static member Create(path) =
            match File.Exists path with
            | true -> SqliteContext.Open path |> WikdStore
            | false ->
                let ctx = SqliteContext.Create(path)
                Internal.initialize ctx
                WikdStore(ctx)

        member _.AddPage(name, displayName, parent, directory) =

            let order =
                match parent with
                | Some p -> Internal.getMaxOrderPageForParent ctx p
                | None -> Internal.getMaxOrderTopLevelPage ctx
                |> Option.map (fun p -> p.PageOrder + 1)
                |> Option.defaultValue 0

            Internal.addPage ctx name displayName parent order directory

        member _.AddPageVersion(id, page, stream, isDraft) =

            let version =
                Internal.getPageLatestVersion ctx page
                |> Option.map (fun p -> p.Version + 1)
                |> Option.defaultValue 1

            Internal.addPageVersion ctx id page version stream isDraft

        member _.GetAllPages() = Internal.getAllActivePages ctx

        member _.GetLatestPageVersion(page) = Internal.getPageLatestVersion ctx page

        member _.GetPagesForParent(parent) = Internal.getPagesForParent ctx parent

        member _.GetTopLevelPages() = Internal.getTopLevelPages ctx

        member _.GetPage(page) = Internal.getPage ctx page
        
        