from attrs import define
from neorg_lsp.methods import ResponseHandler
import logging

from neorg_lsp.rpc.completion import CompletionItemResult, CompletionRequest
from neorg_lsp.state import State

log = logging.getLogger("neorg_lsp")


@define
class CompletionHandler(ResponseHandler):
    request: CompletionRequest
    state: State

    def handle_request(self) -> None:
        log.info("Getting Completions...")
        return CompletionItemResult.get_completions(
            self.request, self.state
        ).send_message()
