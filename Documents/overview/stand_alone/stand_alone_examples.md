<meta name="wikd:title" content="Examples">
<meta name="wikd:order" content="0">

# Stand alone examples

`Wikd` can be run as a stand alone application. 
`Wikd.App` is a command line tool for building and render wikis,
that can easily be integrated in to build pipelines.

## Commands

The following commands are supported in standalone move

### Import

Import wiki files to a `wikd` store and render pages.

```shell
$ Wikd.exe import -p "[root path]" -s "[store path]" -o "[output path]" -f
```

#### Arguments

* `-p` or `--path` - The root page of the wiki files.
* `-s` or `--store` - The path to the `wikd` store.
* `-o` or `--output` - The output path.
* `-f` or `--force` - Force the creation of a new store. The as running `clean` first.

#### Notes

This will check for update to pages. 
If a page has not has not been updated the import will be skipped but the page will still be rendered.

### Build

Build a wiki from the latest page versions in a store.

```shell
Wikd.exe build -p "[root path]" -s "[store path]" -o "[output path]"
```

#### Arguments

* `-p` or `--path` - The root page of the wiki files.
* `-s` or `--store` - The path to the `wikd` store.
* `-o` or `--output` - The output path.
* `-m` or `--mode` - The rendering mode. Supports `static` and `dynamic`.

### Clean

Clean a wiki store. This will create a new empty store.

```shell
Wikd.exe clean -s "[store path]"
```

#### Arguments

* `-s` or `--store` - the path to the `wikd` store.

#### Notes

This will **delete** any existing data in the store. 
If this might be an issue make a backup of the store first.
