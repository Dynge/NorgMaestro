# Docs: https://github.com/tree-sitter/py-tree-sitter

# TS Query syntax: https://tree-sitter.github.io/tree-sitter/using-parsers#query-syntax
from tree_sitter import Language, Parser

NEORG_BUILD = "neorg_lsp/resources/parsers/neorg.so"
Language.build_library(
    # Store the library in the `build` directory
    NEORG_BUILD,
    # Include one or more languages
    ["neorg_lsp/resources/parsers/norg.so", "neorg_lsp/resources/parsers/norg_meta.so"],
)
NEORG_LANGUAGE = Language(NEORG_BUILD, "python")
parser = Parser()
parser.set_language(NEORG_LANGUAGE)

tree = parser.parse(
    bytes(
        """
@document.meta
title: index
description:
authors: michael
categories: [
    index
]
created: 2023-06-18
updated: 2024-03-10T16:33:39+0100
version: 1.1.1
@end

* Welcome to my Neorg Index

""",
        "utf8",
    )
)

title_query = NEORG_LANGUAGE.query(
    """
(pair
  (key) @meta_key (#eq? @meta_key "title")
  (string) @title.string )
"""
)
category_query = NEORG_LANGUAGE.query(
    """
(pair
  (key) @meta_key (#eq? @meta_key "categories")
  [((array (string) @category.string) )
  (string) @category.string] )
"""
)
