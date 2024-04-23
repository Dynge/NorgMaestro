from attrs import define
from neorg_lsp.methods import ResponseHandler
import logging

from neorg_lsp.rpc import BaseNotification, BaseResponse, BaseRequest

log = logging.getLogger("neorg_lsp")


@define
class ShutdownHandler(ResponseHandler):
    request: BaseRequest

    def handle_request(self) -> None:
        log.info("Shutting down...")
        BaseNotification.send_notification("WHYYYY")
        return BaseResponse(
            self.request.id,
        ).send_message()
