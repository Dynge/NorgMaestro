from neorg_lsp.rpc import BaseRequest
from neorg_lsp.rpc.requests import InitializeRequest
from neorg_lsp.rpc.responses import InitializeResult
from neorg_lsp.rpc.responses import ServerCapabilities


def handle_init(base: BaseRequest, req: InitializeRequest) -> InitializeResult:
    return InitializeResult.success(
        base.id,
        ServerCapabilities(
            {
                "diagnosticsProvider": {
                    "identifier": "NeorgPyLsp",
                    "interFileDependencies": False,
                    "workspaceDiagnostics": True,
                }
            }
        ),
    )
