import enum
import logging
from typing import Protocol

from attrs import define

log = logging.getLogger("neorg_lsp")


class LspMethod(enum.Enum):
    INITIALIZE: str = "initialize"
    SHUTDOWN: str = "shutdown"
    EXIT: str = "exit"
    COMPLETION: str = "textDocument/completion"


class ResponseHandler(Protocol):
    def handle_request(self) -> None: ...


@define
class NoopHandler(ResponseHandler):

    def handle_request(self) -> None:
        log.info("Did nothing")
        return


