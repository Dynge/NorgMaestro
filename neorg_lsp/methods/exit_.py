

from attrs import define
from neorg_lsp.methods import ResponseHandler
import logging

from neorg_lsp.rpc import BaseNotification

log = logging.getLogger("neorg_lsp")


@define
class ExitHandler(ResponseHandler):

    def handle_request(self) -> None:
        log.info("Exiting")
        BaseNotification.send_notification("BYEEEEEE")
        exit()
