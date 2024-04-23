from collections.abc import Callable
from attrs import define

from neorg_lsp.methods import LspMethod, NoopHandler, ResponseHandler
from neorg_lsp.methods.completion import CompletionHandler
from neorg_lsp.methods.exit_ import ExitHandler
from neorg_lsp.methods.shutdown import ShutdownHandler
from neorg_lsp.methods.initialize import InitializeHandler
from neorg_lsp.rpc import BaseRequest
from neorg_lsp.rpc.completion import CompletionRequest
from neorg_lsp.rpc.initialize import InitializeRequest
from neorg_lsp.state import State
import logging

log = logging.getLogger("neorg_lsp")


@define
class MethodHandlerFactory:
    state: State

    def create_handler(self, method_value: str, content: bytes) -> ResponseHandler:
        try:
            method = LspMethod(method_value)
        except ValueError:
            log.warn(f"Not a known LSP method: {method_value}")
            return NoopHandler()

        handler: Callable[[], ResponseHandler] | None = {
            LspMethod.INITIALIZE: lambda: InitializeHandler(
                InitializeRequest.decode(content), self.state
            ),
            LspMethod.SHUTDOWN: lambda: ShutdownHandler(BaseRequest.decode(content)),
            LspMethod.EXIT: lambda: ExitHandler(),
            LspMethod.COMPLETION: lambda: CompletionHandler(
                CompletionRequest.decode(content), self.state
            ),
        }.get(method, None)
        if not handler:
            log.warn(f"No handler for {method=}")
            return NoopHandler()
        return handler()
