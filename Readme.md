SharpBoomerang
==============

Parser and unparser (pretty printer) from the same grammar. Somewhat akin to the [boomerang package for Haskell][0], or the [Boomerang language][1].

[0]: https://hackage.haskell.org/package/boomerang
[1]: http://www.seas.upenn.edu/~harmony/

Features
--------

- Written in F# 4.0
- In F#, you can define the grammar in a functional style with combinators!

Considerations
--------------

- Still pretty rough, especially from C#
- Not many tests yet
- Not yet optimized for performance
- Not yet a complete parsing framework

Requirements
------------

### Visual Studio

You'll need Visual Studio 2015 -- earlier versions will not work. Just load `SharpBoomerang.sln` and go!

### Xamarin Studio

If you are using Mono, you'll need version 4.3.1 or newer (currently available in [alpha][2]). Just load `SharpBoomerang.sln` in Xamarin Studio and go!

[2]: http://www.mono-project.com/download/alpha/

Docs
----

Documentation can be found in the `docs` directory, or [online][3]. The HTML docs can be built by running `build.fsx`.

[3]: http://chkn.github.io/SharpBoomerang/Introduction.html

### TL;DR

For those who are impatient, here are some samples that are more fully discussed in the docs.

    [hide]
    #load @"SharpBoomerang.fsx"
    open SharpBoomerang
    open SharpBoomerang.Combinators

#### Hello World

    let bhello =
        blit %"Hello" >>       // boomerang the literal string, "Hello"
        ~~(blit %",") %true >> // optionally parse a comma (",") -- "%true" means it will always be printed
        !+(bws %' ') >>        // parse one or more whitespace characters -- it will print one space
        blit %"World!"         // boomerang the literal string, "World!"

#### Parse a Name

    type Title =
        | Mr
        | Ms
        | Dr

    type Name = {
        Title : Title option;
        First : string;
        Last  : string;
    }

    let bspace = !+(bws %' ')
    let btitle = ~~~(bdu<Title> .>> ~~(blit %".") %true .>> bspace)
    let bname = btitle .>>. bstr .>> bspace .>>. bstr .>>% ((fun (t, f, l) -> { Title = t; First = f; Last = l }), (fun n -> (n.Title, n.First, n.Last)))

#### Parse JSON

See the full document [here][4].

[4]: http://chkn.github.io/SharpBoomerang/JSON.html


Tests
-----

The NUnit tests are under the `tests` directory. The tests should be runnable from the Unit Tests pad in Xamarin Studio or the Test Explorer in Visual Studio.


Contributions, Feedback, etc.
-----------------------------

All welcome! Feel free to file PRs or Issues. Also consider poking me on Twitter @chknofthescene, as I sometimes miss GitHub notifications.