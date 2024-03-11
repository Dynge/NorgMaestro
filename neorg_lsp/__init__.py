# Docs: https://github.com/tree-sitter/py-tree-sitter

# TS Query syntax: https://tree-sitter.github.io/tree-sitter/using-parsers#query-syntax
import itertools
from typing import Optional
from tree_sitter import Language, Parser

from pathlib import Path

NEORG_LANGUAGE = Language("neorg_lsp/resources/parsers/norg.so", "norg")
NEORG_META_LANGUAGE = Language("neorg_lsp/resources/parsers/norg_meta.so", "norg_meta")

norg_parser = Parser()
norg_parser.set_language(NEORG_LANGUAGE)

norg_meta_parser = Parser()
norg_meta_parser.set_language(NEORG_META_LANGUAGE)


def get_metadata_text(file_bytes: bytes) -> Optional[bytes]:
    tree = norg_parser.parse(file_bytes)

    metadata_query = NEORG_LANGUAGE.query("""
    (ranged_verbatim_tag
      name: (tag_name) @tag.name (#eq? @tag.name "document.meta")
      content: (ranged_verbatim_tag_content) @metadata.content
        )
    """)
    metadata_match = metadata_query.matches(tree.root_node)
    if not metadata_match:
        return None

    return next(
        map(lambda match: match[1].get("metadata.content").text, metadata_match), b""
    )


def get_title(file_bytes: bytes) -> Optional[str]:
    metadata_tree = norg_meta_parser.parse(get_metadata_text(file_bytes) or b"")

    title_query = NEORG_META_LANGUAGE.query("""
    (pair
      (key) @meta.key (#eq? @meta.key "title")
      (string) @title.string
    )
    """)
    titles = title_query.matches(metadata_tree.root_node)

    if not titles:
        return None

    return next(
        map(
            lambda match: match[1].get("title.string").text.decode("utf-8"),
            filter(lambda match: len(match[1]) > 0, titles),
        )
    )


def get_categories(file_bytes: bytes) -> list[str]:
    metadata_tree = norg_meta_parser.parse(get_metadata_text(file_bytes) or b"")

    category_query = NEORG_META_LANGUAGE.query("""
    (pair
      (key) @meta.key (#eq? @meta.key "categories")
      [(array (string) @category.string)
      (string) @category.string]
    )
    """)
    categories = category_query.matches(metadata_tree.root_node)
    if not categories:
        return list()

    return list(
        map(
            lambda match: match[1].get("category.string").text.decode("utf-8"),
            filter(lambda match: len(match[1]) > 0, categories),
        )
    )


def main() -> None:
    norg_dir = Path(Path.home(), "notes/")
    all_cats: list[str] = []
    all_tits: list[str] = []
    for path in norg_dir.iterdir():
        if path.suffix != ".norg":
            continue
        with path.open("rb") as f:
            first_lines = list(itertools.islice(f.readlines(), 50))
            all_cats.extend(get_categories(b"".join(first_lines)))
            all_tits.append(get_title(b"".join(first_lines)) or "")
    print(list(set(all_cats)))
    print(list(set(all_tits)))


if __name__ == "__main__":
    main()
