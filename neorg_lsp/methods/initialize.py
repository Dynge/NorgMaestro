import time
from attrs import define
from neorg_lsp.methods import ResponseHandler
from neorg_lsp.rpc import BaseNotification
from neorg_lsp.rpc.initialize import (
    CompletionOptions,
    InitializeParams,
    InitializeRequest,
    InitializeResult,
    ServerCapabilities,
)
import logging

from neorg_lsp.state import State

log = logging.getLogger("neorg_lsp")


@define
class InitializeHandler(ResponseHandler):
    request: InitializeRequest
    state: State

    def handle_request(self) -> None:
        log.info("Initializing LSP...")
        self.state.init_state(self.request)
        BaseNotification.send_notification(f"Hello from Lsp. Timestamp: {time.time()}")
        return InitializeResult.success(
            self.request.id,
            InitializeParams(
                ServerCapabilities(CompletionOptions(False, ["å", "æ", "ø"]))
            ),
        ).send_message()
