import json
import logging
import re
import time
from sys import stdin
from sys import stdout
from typing import Any

import cattrs

from neorg_lsp.methods import LspMethods
from neorg_lsp.methods.initialize import handle_init
from neorg_lsp.rpc import BaseNotification
from neorg_lsp.rpc import BaseRequest
from neorg_lsp.rpc import BaseResponse
from neorg_lsp.rpc import ClientMessage
from neorg_lsp.rpc.requests import InitializeRequest

logging.basicConfig(filename="wow.log", level=logging.DEBUG)
log = logging.getLogger("neorg-lsp")


DecodedMessage = tuple[str, str]


CONTENT_LENGTH = len("Content-Length: ")


def encode_message(data: dict) -> bytes:
    encoded_message = b"Content-Length: "
    content = json.dumps(cattrs.unstructure(data)).encode("utf-8")
    return encoded_message + str(len(content)).encode("utf-8") + b"\r\n\r\n" + content


def decode_message(message: bytes, to_type):
    base_message = cattrs.structure(json.loads(message.decode("utf-8")), to_type)
    return base_message


def send_message(response: Any) -> None:
    byte_res = encode_message(cattrs.unstructure(response))
    stdout.buffer.write(byte_res)
    stdout.buffer.flush()


def send_notification(message: str, level: int = 1) -> None:
    send_message(
        BaseNotification("window/showMessage", {"type": level, "message": message})
    )


# TODO: Implement State
# TODO: Implement textOpen and textEdit to populate references and categories

if __name__ == "__main__":
    timeout = 1
    buffer = b""
    while stdin.readable():
        buffer = buffer + stdin.buffer.read(1)

        # TODO: Smarter validation of input and error handling for proper reset
        # I need some sort of scanner to find Content-Length and discard everything before it.
        match_header = re.match(
            r"Content-Length: (\d+)\r\n\r\n", buffer.decode("utf-8")
        )
        if not match_header:
            continue
        buffer = b""
        content_length = int(match_header.group(1))
        content = stdin.buffer.read(content_length)
        base = decode_message(content, ClientMessage)
        log.info(f"{base.method=}")
        if base.method == LspMethods.INITIALIZE.value:
            log.info("Initializing LSP...")
            base = decode_message(content, BaseRequest)
            req = cattrs.structure(base.params, InitializeRequest)
            response = handle_init(base, req)
            send_message(response)
            send_notification(f"Hello from Lsp. Timestamp: {time.time()}")

        elif base.method == LspMethods.SHUTDOWN.value:
            log.info("Shutting down...")
            send_notification("WHYYYY")
            base = decode_message(content, BaseRequest)
            send_message(
                BaseResponse(
                    base.id,
                    None,
                    None,
                )
            )
        elif base.method == LspMethods.EXIT.value:
            log.info("Exiting")
            send_notification("BYEEEEEE")
            exit()
