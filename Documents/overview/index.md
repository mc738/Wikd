<meta name="wikd:title" content="Overview">
<meta name="wikd:order" content="0">

# Wikd overview

`Wikd` is a tool for build wiki's. Pages can be created as `markdown` files and imported into the `wikd` store.

## Stand alone

`wikd` can be run as a standalone application via the command line to create and manage pages.

## StaticCMS plugin

`wikd` can be used as a plugin for [StaticCMS](https://github.com/mc738/StaticCMS).
This way wikis can be built and managed as part of a larger site.

TODO - add example script.

## Metadata

`wikd` pages support metadata to help define them. This is included at the top of the raw `md` file.

Metadata takes the form of `html` meta tags:

```html
<meta name="wikd:title" content="My page">
```

Supported tags include:

* `wikd:title` - The page title.
* `wikd:order` - The page order.
* `wikd:icon` - The page icon.

## Rendering modes

`wikd` currently supports 2 rendering modes:

* Static - each page is rendered as a standalone `.html` file. No `JavaScript` is needed.
* Dynamic - each page is rendered as a fragment, a single `index.html` file is generated from the template and `JavaScript` is used to fetch the pages dynamically.

Dynamic mode is still a *work in progress*, the output might be different to static pages. 
However, the main content should render the same.

Also dynamic is likely to be expanded to include `API` calls.

## Resources

TODO - add

## Themes

TODO - add