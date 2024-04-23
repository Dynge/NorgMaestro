import json
from sys import stdout
from typing import Any

from attrs import define
from cattrs import unstructure, structure

import logging

log = logging.getLogger("neorg_lsp")


class Message:
    jsonrpc: str = "2.0"

    @classmethod
    def decode(cls, body: bytes):
        return structure(json.loads(body.decode("utf-8")), cls)

    def encode(self):
        encoded_message = b"Content-Length: "
        content = json.dumps(unstructure(self)).encode("utf-8")
        return (
            encoded_message + str(len(content)).encode("utf-8") + b"\r\n\r\n" + content
        )

    def send_message(self) -> None:
        log.debug(self)
        byte_res = self.encode()
        _ = stdout.buffer.write(byte_res)
        stdout.buffer.flush()

@define
class IncomingMessage(Message):
    method: str


@define
class BaseRequest(Message):
    id: int
    method: str
    params: Any | None = None


@define
class NotificationParams:
    message: str
    level: int = 1


@define
class BaseNotification(Message):
    method: str
    params: NotificationParams | None = None

    @staticmethod
    def send_notification(message: str, level: int = 1) -> None:
        BaseNotification(
            method="window/showMessage", params=NotificationParams(message, level)
        ).send_message()


@define
class BaseResponse(Message):
    id: int
    result: Any | None = None
    error: Any | None = None
