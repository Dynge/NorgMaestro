from typing import Any

import attrs

from neorg_lsp.rpc import BaseRequest, BaseResponse


@attrs.define
class InitializeRequestParams:
    capabilities: dict[Any, Any] = dict()
    processId: int | None = None
    rootPath: str | None = None


@attrs.define
class InitializeRequest(BaseRequest):
    params: InitializeRequestParams = InitializeRequestParams()

@attrs.define
class CompletionOptions:
    resolveProvider: bool
    triggerCharacters: list[str]

@attrs.define
class ServerCapabilities:
    completionProvider: CompletionOptions


@attrs.define
class InitializeParams:
    capabilities: ServerCapabilities


@attrs.define
class InitializeResult(BaseResponse):
    result: InitializeParams | None = None

    @staticmethod
    def success(id: int, result: InitializeParams):
        req = InitializeResult(id)
        req.result = result
        return req
