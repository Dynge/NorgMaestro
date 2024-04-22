# Docs: https://github.com/tree-sitter/py-tree-sitter

# TS Query syntax: https://tree-sitter.github.io/tree-sitter/using-parsers#query-syntax
import itertools
from tree_sitter import Language, Parser
from dataclasses import dataclass

from pathlib import Path

NEORG_LANGUAGE = Language("neorg_lsp/resources/parsers/norg.so", "norg")
NEORG_META_LANGUAGE = Language("neorg_lsp/resources/parsers/norg_meta.so", "norg_meta")

norg_parser = Parser()
norg_parser.set_language(NEORG_LANGUAGE)

norg_meta_parser = Parser()
norg_meta_parser.set_language(NEORG_META_LANGUAGE)


@dataclass
class Metadata:
    title: str
    category: list[str]


def get_metadata_text(file_bytes: bytes) -> bytes:
    tree = norg_parser.parse(file_bytes)

    metadata_query = NEORG_LANGUAGE.query("""
    (ranged_verbatim_tag
      name: (tag_name) @tag.name (#eq? @tag.name "document.meta")
      content: (ranged_verbatim_tag_content) @metadata.content
        )
    """)
    metadata_match = metadata_query.matches(tree.root_node)
    if not metadata_match:
        return b""

    return (
        next(
            map(lambda match: match[1].get("metadata.content").text, metadata_match),
            b"",
        )
        or b""
    )


def get_title(metadata_bytes: bytes) -> str:
    metadata_tree = norg_meta_parser.parse(metadata_bytes)

    title_query = NEORG_META_LANGUAGE.query("""
    (pair
      (key) @meta.key (#eq? @meta.key "title")
      (string) @title.string
    )
    """)
    titles = title_query.matches(metadata_tree.root_node)

    if not titles:
        return ""

    return (
        next(
            map(
                lambda match: match[1].get("title.string").text.decode("utf-8"),
                filter(lambda match: len(match[1]) > 0, titles),
            )
        )
        or ""
    )


def get_categories(metadata_bytes: bytes) -> list[str]:
    metadata_tree = norg_meta_parser.parse(metadata_bytes)

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
    all_mets = []
    set_cats = set()
    for path in norg_dir.iterdir():
        if path.suffix != ".norg":
            continue
        with path.open("rb") as f:
            first_lines = list(itertools.islice(f.readlines(), 50))
            metadata_text = get_metadata_text(b"".join(first_lines))
            cats = get_categories(metadata_text)
            tit = get_title(metadata_text)
            set_cats.update(cats)
            all_mets.append(Metadata(tit, cats))

    print(list(map(lambda mdata: mdata.title, all_mets)))
    print(set_cats)


if __name__ == "__main__":
    main()
