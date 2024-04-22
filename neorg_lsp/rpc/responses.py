import attrs

from neorg_lsp.rpc.requests import InitializeRequest


@attrs.define
class ServerCapabilities:
    capabilities: dict


@attrs.define
class InitializeResult:
    id: int
    jsonrpc: str = "2.0"
    result: ServerCapabilities | None = None
    error: dict | None = None

    @staticmethod
    def success(id: int, result: ServerCapabilities):
        req = InitializeResult(id)
        req.result = result
        return req
