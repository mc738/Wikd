namespace Wikd.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 29/07/2022 20:54:26 (utc) via Freql.Sqlite.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `categories`.
    type Category =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("displayName")>] DisplayName: string }
    
        static member Blank() =
            { Name = String.Empty
              DisplayName = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE categories (
	name TEXT NOT NULL, display_name TEXT NOT NULL,
	CONSTRAINT categories_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              display_name
        FROM categories
        """
    
        static member TableName() = "categories"
    
    /// A record representing a row in the table `category_pages`.
    type CategoryPage =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("category")>] Category: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("isDraft")>] IsDraft: bool }
    
        static member Blank() =
            { Id = String.Empty
              Category = String.Empty
              Version = 0
              RawBlob = BlobField.Empty()
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow
              IsDraft = true }
    
        static member CreateTableSql() = """
        CREATE TABLE "category_pages" (
	id TEXT NOT NULL,
	category TEXT NOT NULL,
	version INTEGER NOT NULL,
	raw_blob BLOB NOT NULL,
	hash TEXT NOT NULL,
	created_on TEXT NOT NULL,
	is_draft INTEGER NOT NULL,
	CONSTRAINT category_page_PK PRIMARY KEY (id),
	CONSTRAINT category_page_UN UNIQUE (category,version),
	CONSTRAINT category_page_FK FOREIGN KEY (category) REFERENCES categories(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              id,
              category,
              version,
              raw_blob,
              hash,
              created_on,
              is_draft
        FROM category_pages
        """
    
        static member TableName() = "category_pages"
    
    /// A record representing a row in the table `page_categories`.
    type PageCategory =
        { [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("category")>] Category: string }
    
        static member Blank() =
            { Page = String.Empty
              Category = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE page_categories (
	page TEXT NOT NULL,
	category TEXT NOT NULL,
	CONSTRAINT page_categories_PK PRIMARY KEY (page,category),
	CONSTRAINT page_categories_FK FOREIGN KEY (page) REFERENCES pages(name),
	CONSTRAINT page_categories_FK_1 FOREIGN KEY (category) REFERENCES categories(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              page,
              category
        FROM page_categories
        """
    
        static member TableName() = "page_categories"
    
    /// A record representing a row in the table `page_metadata`.
    type PageMetadataItem =
        { [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("value")>] Value: string }
    
        static member Blank() =
            { Page = String.Empty
              Name = String.Empty
              Value = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE page_metadata (
	page TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	CONSTRAINT page_metadata_PK PRIMARY KEY (page,name),
	CONSTRAINT page_metadata_FK FOREIGN KEY (page) REFERENCES pages(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              page,
              name,
              value
        FROM page_metadata
        """
    
        static member TableName() = "page_metadata"
    
    /// A record representing a row in the table `page_tags`.
    type PageTag =
        { [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { Page = String.Empty
              Tag = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE page_tags (
	page TEXT NOT NULL,
	tag TEXT NOT NULL,
	CONSTRAINT page_tags_PK PRIMARY KEY (page,tag),
	CONSTRAINT page_tags_FK FOREIGN KEY (page) REFERENCES pages(name),
	CONSTRAINT page_tags_FK_1 FOREIGN KEY (tag) REFERENCES tags(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              page,
              tag
        FROM page_tags
        """
    
        static member TableName() = "page_tags"
    
    /// A record representing a row in the table `page_versions`.
    type PageVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("isDraft")>] IsDraft: bool }
    
        static member Blank() =
            { Id = String.Empty
              Page = String.Empty
              Version = 0
              RawBlob = BlobField.Empty()
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow
              IsDraft = true }
    
        static member CreateTableSql() = """
        CREATE TABLE page_versions (
	id TEXT NOT NULL,
	page TEXT NOT NULL,
	version INTEGER NOT NULL,
	raw_blob BLOB NOT NULL,
	hash TEXT NOT NULL,
	created_on TEXT NOT NULL,
	is_draft INTEGER NOT NULL,
	CONSTRAINT page_versions_PK PRIMARY KEY (id),
	CONSTRAINT page_versions_UN UNIQUE (page,version),
	CONSTRAINT page_versions_FK FOREIGN KEY (page) REFERENCES pages(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              id,
              page,
              version,
              raw_blob,
              hash,
              created_on,
              is_draft
        FROM page_versions
        """
    
        static member TableName() = "page_versions"
    
    /// A record representing a row in the table `pages`.
    type Page =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("displayName")>] DisplayName: string
          [<JsonPropertyName("parent")>] Parent: string option
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("pageOrder")>] PageOrder: int
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Name = String.Empty
              DisplayName = String.Empty
              Parent = None
              CreatedOn = DateTime.UtcNow
              PageOrder = 0
              Active = true }
    
        static member CreateTableSql() = """
        CREATE TABLE pages (
	name TEXT NOT NULL,
	display_name TEXT NOT NULL,
	parent TEXT,
	created_on TEXT NOT NULL,
	"page_order" INTEGER NOT NULL,
	active INTEGER NOT NULL,
	CONSTRAINT pages_PK PRIMARY KEY (name),
	CONSTRAINT pages_FK FOREIGN KEY (parent) REFERENCES pages(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              display_name,
              parent,
              created_on,
              page_order,
              active
        FROM pages
        """
    
        static member TableName() = "pages"
    
    /// A record representing a row in the table `resources`.
    type Resource =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("ext")>] Ext: string
          [<JsonPropertyName("outputFolder")>] OutputFolder: string
          [<JsonPropertyName("rawBlob")>] RawBlob: string
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Name = String.Empty
              Ext = String.Empty
              OutputFolder = String.Empty
              RawBlob = String.Empty
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow }
    
        static member CreateTableSql() = """
        CREATE TABLE resources (
	name TEXT NOT NULL,
	ext TEXT NOT NULL,
	output_folder TEXT NOT NULL,
	raw_blob TEXT NOT NULL,
	hash TEXT NOT NULL,
	created_on TEXT NOT NULL,
	CONSTRAINT resources_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              ext,
              output_folder,
              raw_blob,
              hash,
              created_on
        FROM resources
        """
    
        static member TableName() = "resources"
    
    /// A record representing a row in the table `tags`.
    type Tag =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("displayName")>] DisplayName: string
          [<JsonPropertyName("color")>] Color: string option
          [<JsonPropertyName("icon")>] Icon: string option }
    
        static member Blank() =
            { Name = String.Empty
              DisplayName = String.Empty
              Color = None
              Icon = None }
    
        static member CreateTableSql() = """
        CREATE TABLE tags (
	name TEXT NOT NULL,
	display_name TEXT NOT NULL,
	color TEXT,
	icon TEXT,
	CONSTRAINT tags_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              display_name,
              color,
              icon
        FROM tags
        """
    
        static member TableName() = "tags"
    

/// Module generated on 29/07/2022 20:54:26 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `categories`.
    type NewCategory =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("displayName")>] DisplayName: string }
    
        static member Blank() =
            { Name = String.Empty
              DisplayName = String.Empty }
    
    
    /// A record representing a new row in the table `category_pages`.
    type NewCategoryPage =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("category")>] Category: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("isDraft")>] IsDraft: bool }
    
        static member Blank() =
            { Id = String.Empty
              Category = String.Empty
              Version = 0
              RawBlob = BlobField.Empty()
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow
              IsDraft = true }
    
    
    /// A record representing a new row in the table `page_categories`.
    type NewPageCategory =
        { [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("category")>] Category: string }
    
        static member Blank() =
            { Page = String.Empty
              Category = String.Empty }
    
    
    /// A record representing a new row in the table `page_metadata`.
    type NewPageMetadataItem =
        { [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("value")>] Value: string }
    
        static member Blank() =
            { Page = String.Empty
              Name = String.Empty
              Value = String.Empty }
    
    
    /// A record representing a new row in the table `page_tags`.
    type NewPageTag =
        { [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { Page = String.Empty
              Tag = String.Empty }
    
    
    /// A record representing a new row in the table `page_versions`.
    type NewPageVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("page")>] Page: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("isDraft")>] IsDraft: bool }
    
        static member Blank() =
            { Id = String.Empty
              Page = String.Empty
              Version = 0
              RawBlob = BlobField.Empty()
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow
              IsDraft = true }
    
    
    /// A record representing a new row in the table `pages`.
    type NewPage =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("displayName")>] DisplayName: string
          [<JsonPropertyName("parent")>] Parent: string option
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("pageOrder")>] PageOrder: int
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Name = String.Empty
              DisplayName = String.Empty
              Parent = None
              CreatedOn = DateTime.UtcNow
              PageOrder = 0
              Active = true }
    
    
    /// A record representing a new row in the table `resources`.
    type NewResource =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("ext")>] Ext: string
          [<JsonPropertyName("outputFolder")>] OutputFolder: string
          [<JsonPropertyName("rawBlob")>] RawBlob: string
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Name = String.Empty
              Ext = String.Empty
              OutputFolder = String.Empty
              RawBlob = String.Empty
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow }
    
    
    /// A record representing a new row in the table `tags`.
    type NewTag =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("displayName")>] DisplayName: string
          [<JsonPropertyName("color")>] Color: string option
          [<JsonPropertyName("icon")>] Icon: string option }
    
        static member Blank() =
            { Name = String.Empty
              DisplayName = String.Empty
              Color = None
              Icon = None }
    
    
/// Module generated on 29/07/2022 20:54:26 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.Category` from the table `categories`.
    /// Internally this calls `context.SelectSingleAnon<Records.Category>` and uses Records.Category.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectCategoryRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectCategoryRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Category.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Category>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Category>` and uses Records.Category.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectCategoryRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectCategoryRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Category.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Category>(sql, parameters)
    
    let insertCategory (context: SqliteContext) (parameters: Parameters.NewCategory) =
        context.Insert("categories", parameters)
    
    /// Select a `Records.CategoryPage` from the table `category_pages`.
    /// Internally this calls `context.SelectSingleAnon<Records.CategoryPage>` and uses Records.CategoryPage.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectCategoryPageRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectCategoryPageRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.CategoryPage.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.CategoryPage>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.CategoryPage>` and uses Records.CategoryPage.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectCategoryPageRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectCategoryPageRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.CategoryPage.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.CategoryPage>(sql, parameters)
    
    let insertCategoryPage (context: SqliteContext) (parameters: Parameters.NewCategoryPage) =
        context.Insert("category_pages", parameters)
    
    /// Select a `Records.PageCategory` from the table `page_categories`.
    /// Internally this calls `context.SelectSingleAnon<Records.PageCategory>` and uses Records.PageCategory.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageCategoryRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageCategoryRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageCategory.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.PageCategory>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.PageCategory>` and uses Records.PageCategory.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageCategoryRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageCategoryRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageCategory.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.PageCategory>(sql, parameters)
    
    let insertPageCategory (context: SqliteContext) (parameters: Parameters.NewPageCategory) =
        context.Insert("page_categories", parameters)
    
    /// Select a `Records.PageMetadataItem` from the table `page_metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.PageMetadataItem>` and uses Records.PageMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.PageMetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.PageMetadataItem>` and uses Records.PageMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.PageMetadataItem>(sql, parameters)
    
    let insertPageMetadataItem (context: SqliteContext) (parameters: Parameters.NewPageMetadataItem) =
        context.Insert("page_metadata", parameters)
    
    /// Select a `Records.PageTag` from the table `page_tags`.
    /// Internally this calls `context.SelectSingleAnon<Records.PageTag>` and uses Records.PageTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageTagRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageTagRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageTag.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.PageTag>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.PageTag>` and uses Records.PageTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageTagRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageTagRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageTag.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.PageTag>(sql, parameters)
    
    let insertPageTag (context: SqliteContext) (parameters: Parameters.NewPageTag) =
        context.Insert("page_tags", parameters)
    
    /// Select a `Records.PageVersion` from the table `page_versions`.
    /// Internally this calls `context.SelectSingleAnon<Records.PageVersion>` and uses Records.PageVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageVersionRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageVersionRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageVersion.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.PageVersion>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.PageVersion>` and uses Records.PageVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageVersionRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageVersionRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.PageVersion.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.PageVersion>(sql, parameters)
    
    let insertPageVersion (context: SqliteContext) (parameters: Parameters.NewPageVersion) =
        context.Insert("page_versions", parameters)
    
    /// Select a `Records.Page` from the table `pages`.
    /// Internally this calls `context.SelectSingleAnon<Records.Page>` and uses Records.Page.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Page.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Page>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Page>` and uses Records.Page.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectPageRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectPageRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Page.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Page>(sql, parameters)
    
    let insertPage (context: SqliteContext) (parameters: Parameters.NewPage) =
        context.Insert("pages", parameters)
    
    /// Select a `Records.Resource` from the table `resources`.
    /// Internally this calls `context.SelectSingleAnon<Records.Resource>` and uses Records.Resource.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectResourceRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectResourceRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Resource.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Resource>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Resource>` and uses Records.Resource.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectResourceRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectResourceRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Resource.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Resource>(sql, parameters)
    
    let insertResource (context: SqliteContext) (parameters: Parameters.NewResource) =
        context.Insert("resources", parameters)
    
    /// Select a `Records.Tag` from the table `tags`.
    /// Internally this calls `context.SelectSingleAnon<Records.Tag>` and uses Records.Tag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTagRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTagRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Tag.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Tag>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Tag>` and uses Records.Tag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTagRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTagRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Tag.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Tag>(sql, parameters)
    
    let insertTag (context: SqliteContext) (parameters: Parameters.NewTag) =
        context.Insert("tags", parameters)
    