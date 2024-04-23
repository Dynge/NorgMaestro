import attrs

from neorg_lsp.rpc import BaseRequest, BaseResponse
from neorg_lsp.state import State


@attrs.define
class CompletionPosition:
    line: int
    character: int


@attrs.define
class TextDocumentIdentifier:
    uri: str


@attrs.define
class TextDocumentPositionParams:
    textDocument: TextDocumentIdentifier
    position: CompletionPosition


@attrs.define
class CompletionParams(TextDocumentPositionParams):
    pass


@attrs.define
class CompletionRequest(BaseRequest):
    params: CompletionParams | None = None


@attrs.define
class MarkupContent:
    kind: str  # "plaintext" | "markdown"
    value: str | list[str]


@attrs.define
class CompletionItem:
    label: str
    documentation: str | MarkupContent | None = None


@attrs.define
class CompletionItemResult(BaseResponse):
    result: list[CompletionItem] | None = None

    @classmethod
    def get_completions(cls, req: CompletionRequest, lspState: State):
        def all_docs(doc_uris: list[str]) -> str:
            return "\n\t".join(
                [
                    f"[{lspState.documents[doc_uri].title}]({doc_uri})"
                    for doc_uri in doc_uris
                ]
            )

        return cls(
            req.id,
            result=[
                CompletionItem(
                    category,
                    MarkupContent(
                        "markdown",
                        f"""
# {category}

Found in the following files:

[
    {all_docs(category_docs.uris)}
]
""",
                    ),
                )
                for category, category_docs in lspState.labels.items()
            ],
        )
