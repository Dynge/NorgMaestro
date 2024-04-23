import itertools
import logging
from pathlib import Path

from attrs import define

from neorg_lsp.neorg_ts import get_categories, get_metadata_text, get_title
from neorg_lsp.rpc.initialize import InitializeRequest


log = logging.getLogger("neorg_lsp")


@define
class Position:
    line: int
    character: int


@define
class DocumentCategories:
    begin: Position
    end: Position
    labels: list[str]


@define
class CategoryDocuments:
    uris: list[str]


@define
class TextDocumentState:
    title: str
    categories: DocumentCategories


UriToDocumentState = dict[str, TextDocumentState]
LabelToDocumentUris = dict[str, CategoryDocuments]


@define
class State:
    documents: UriToDocumentState = dict()
    labels: LabelToDocumentUris = dict()
    rootPath: Path = Path(".")

    def init_state(self, init_req: InitializeRequest) -> None:
        self.rootPath = Path(init_req.params.rootPath or ".")
        for path in self.rootPath.iterdir():
            if path.suffix != ".norg":
                continue
            with path.open("rb") as f:
                first_lines = list(itertools.islice(f.readlines(), 50))
                metadata_text = get_metadata_text(b"".join(first_lines))
                cats = get_categories(metadata_text)
                log.debug(f"{cats=}")
                title = get_title(metadata_text)
                self.documents[path.as_uri()] = TextDocumentState(
                    title=title,
                    categories=DocumentCategories(
                        Position(1, 1), Position(1, 1), cats
                    ),
                )
                for label in cats:
                    uris_to_label = self.labels.get(label, CategoryDocuments([]))
                    if path.as_uri() not in uris_to_label.uris:
                        uris_to_label.uris.append(path.as_uri())
                    self.labels[label] = uris_to_label
